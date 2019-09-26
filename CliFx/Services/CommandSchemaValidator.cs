using System;
using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Models;

namespace CliFx.Services
{
    /// <inheritdoc />
    public class CommandSchemaValidator : ICommandSchemaValidator
    { 
        private void ValidateUniqueOptionNames(Type commandType, CommandSchema schema)
        {
            var names = new HashSet<string>();
            foreach (var commandOptionSchema in schema.Options)
            {
                if (!names.Add(commandOptionSchema.Name))
                {
                    throw GetException(schema.Name);
                }
            }

            foreach (var commandArgumentSchema in schema.Arguments)
            {
                if (!names.Add(commandArgumentSchema.Name))
                {
                    throw GetException(commandArgumentSchema.Name);
                }
            }

            CliFxException GetException(string optionName)
            {
                return new CliFxException($"Command type [{commandType}] has options/arguments defined with the same name: {optionName}");
            }
        }

        private void ValidateUniqueOptionShortNames(Type commandType, CommandSchema schema)
        {
            var shortNames = new HashSet<char>();
            foreach (var commandOptionSchema in schema.Options)
            {
                if (commandOptionSchema.ShortName.HasValue && !shortNames.Add(commandOptionSchema.ShortName.Value))
                {
                    throw new CliFxException($"Command type [{commandType}] has options with the same short name: {commandOptionSchema.ShortName.Value}");
                }
            }
        }

        /// <inheritdoc />
        public void ValidateCommandSchema(Type commandType, CommandSchema schema)
        {
            ValidateUniqueOptionNames(commandType, schema);
            ValidateUniqueOptionShortNames(commandType, schema);
        }
    }
}