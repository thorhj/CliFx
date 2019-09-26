﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandInputConverter"/>.
    /// </summary>
    public partial class CommandInputConverter : ICommandInputConverter
    {
        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Initializes an instance of <see cref="CommandInputConverter"/>.
        /// </summary>
        public CommandInputConverter(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider.GuardNotNull(nameof(formatProvider));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInputConverter"/>.
        /// </summary>
        public CommandInputConverter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Converts a single string value to specified target type.
        /// </summary>
        protected virtual object ConvertValue(string value, Type targetType)
        {
            targetType.GuardNotNull(nameof(targetType));

            try
            {
                // String or object
                if (targetType == typeof(string) || targetType == typeof(object))
                    return value;

                // Bool
                if (targetType == typeof(bool))
                    return value.IsNullOrWhiteSpace() || bool.Parse(value);

                // Char
                if (targetType == typeof(char))
                    return value.Single();

                // Sbyte
                if (targetType == typeof(sbyte))
                    return sbyte.Parse(value, _formatProvider);

                // Byte
                if (targetType == typeof(byte))
                    return byte.Parse(value, _formatProvider);

                // Short
                if (targetType == typeof(short))
                    return short.Parse(value, _formatProvider);

                // Ushort
                if (targetType == typeof(ushort))
                    return ushort.Parse(value, _formatProvider);

                // Int
                if (targetType == typeof(int))
                    return int.Parse(value, _formatProvider);

                // Uint
                if (targetType == typeof(uint))
                    return uint.Parse(value, _formatProvider);

                // Long
                if (targetType == typeof(long))
                    return long.Parse(value, _formatProvider);

                // Ulong
                if (targetType == typeof(ulong))
                    return ulong.Parse(value, _formatProvider);

                // Float
                if (targetType == typeof(float))
                    return float.Parse(value, _formatProvider);

                // Double
                if (targetType == typeof(double))
                    return double.Parse(value, _formatProvider);

                // Decimal
                if (targetType == typeof(decimal))
                    return decimal.Parse(value, _formatProvider);

                // DateTime
                if (targetType == typeof(DateTime))
                    return DateTime.Parse(value, _formatProvider);

                // DateTimeOffset
                if (targetType == typeof(DateTimeOffset))
                    return DateTimeOffset.Parse(value, _formatProvider);

                // TimeSpan
                if (targetType == typeof(TimeSpan))
                    return TimeSpan.Parse(value, _formatProvider);

                // Enum
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value, true);

                // Nullable
                var nullableUnderlyingType = targetType.GetNullableUnderlyingType();
                if (nullableUnderlyingType != null)
                    return !value.IsNullOrWhiteSpace() ? ConvertValue(value, nullableUnderlyingType) : null;

                // Has a constructor that accepts a single string
                var stringConstructor = GetStringConstructor(targetType);
                if (stringConstructor != null)
                    return stringConstructor.Invoke(new object[] {value});

                // Has a static parse method that accepts a single string and a format provider
                var parseMethodWithFormatProvider = GetStaticParseMethodWithFormatProvider(targetType);
                if (parseMethodWithFormatProvider != null)
                    return parseMethodWithFormatProvider.Invoke(null, new object[] {value, _formatProvider});

                // Has a static parse method that accepts a single string
                var parseMethod = GetStaticParseMethod(targetType);
                if (parseMethod != null)
                    return parseMethod.Invoke(null, new object[] {value});
            }
            catch (Exception ex)
            {
                // Wrap and rethrow exceptions that occur when trying to convert the value
                throw new CliFxException($"Can't convert value [{value}] to type [{targetType}].", ex);
            }

            // Throw if we can't find a way to convert the value
            throw new CliFxException($"Can't convert value [{value}] to type [{targetType}].");
        }

        /// <inheritdoc />
        public virtual object ConvertInputValues(IReadOnlyList<string> inputValues, Type targetType)
        {
            inputValues.GuardNotNull(nameof(inputValues));
            targetType.GuardNotNull(nameof(targetType));

            // Get the underlying type of IEnumerable<T> if it's implemented by the target type.
            // Ignore string type because it's IEnumerable<T> but we don't treat it as such.
            var enumerableUnderlyingType = targetType != typeof(string) ? targetType.GetEnumerableUnderlyingType() : null;

            // Convert to a non-enumerable type
            if (enumerableUnderlyingType == null)
            {
                // Throw if provided with more than 1 value
                if (inputValues.Count > 1)
                {
                    throw new CliFxException(
                        $"Can't convert a sequence of values [{inputValues.JoinToString(", ")}] " +
                        $"to non-enumerable type [{targetType}].");
                }

                // Retrieve a single value and convert
                var value = inputValues.SingleOrDefault();
                return ConvertValue(value, targetType);
            }
            // Convert to an enumerable type
            else
            {
                // Convert values to the underlying enumerable type and cast it to dynamic array
                var convertedValues = inputValues
                    .Select(v => ConvertValue(v, enumerableUnderlyingType))
                    .ToNonGenericArray(enumerableUnderlyingType);

                // Get the type of produced array
                var convertedValuesType = convertedValues.GetType();

                // Try to assign the array (works for T[], IReadOnlyList<T>, IEnumerable<T>, etc)
                if (targetType.IsAssignableFrom(convertedValuesType))
                    return convertedValues;

                // Try to inject the array into the constructor (works for HashSet<T>, List<T>, etc)
                var arrayConstructor = targetType.GetConstructor(new[] {convertedValuesType});
                if (arrayConstructor != null)
                    return arrayConstructor.Invoke(new object[] {convertedValues});

                // Throw if we can't find a way to convert the values
                throw new CliFxException(
                    $"Can't convert a sequence of values [{inputValues.JoinToString(", ")}] " +
                    $"to type [{targetType}].");
            }
        }
    }

    public partial class CommandInputConverter
    {
        private static ConstructorInfo GetStringConstructor(Type type) => type.GetConstructor(new[] {typeof(string)});

        private static MethodInfo GetStaticParseMethod(Type type) =>
            type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] {typeof(string)}, null);

        private static MethodInfo GetStaticParseMethodWithFormatProvider(Type type) =>
            type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] {typeof(string), typeof(IFormatProvider)}, null);
    }
}