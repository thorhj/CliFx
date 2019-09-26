using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandInitializer"/>.
    /// </summary>
    public class CommandInitializer : ICommandInitializer
    {
        private readonly ICommandInputConverter _commandInputConverter;

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer(ICommandInputConverter commandInputConverter)
        {
            _commandInputConverter = commandInputConverter.GuardNotNull(nameof(commandInputConverter));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer()
            : this(new CommandInputConverter())
        {
        }

        private IReadOnlyList<string> GetPositionalArguments(CommandSchema commandSchema, CommandInput commandInput)
        {
            var remainingCommandName = commandSchema.Name;
            var i = 0;
            for (; i < commandInput.UnboundArguments.Count; i++)
            {
                var argument = commandInput.UnboundArguments[i];
                if (remainingCommandName.StartsWith(argument))
                {
                    remainingCommandName = remainingCommandName.Substring(argument.Length).Trim();
                }
                else
                {
                    break;
                }
            }

            return commandInput.UnboundArguments.SkipWhile((_, index) => index < i).ToList();
        }

        private void SetCommandOptions(ICommand command, CommandSchema commandSchema, CommandInput commandInput)
        {
            // Keep track of unset required options to report an error at a later stage
            var unsetRequiredOptions = commandSchema.Options.Where(o => o.IsRequired).ToList();

            // Set command options
            foreach (var optionInput in commandInput.Options)
            {
                // Find matching option schema for this option input
                var optionSchema = commandSchema.Options.FindByAlias(optionInput.Alias);
                if (optionSchema == null)
                    continue;

                // Convert option to the type of the underlying property
                var convertedValue = _commandInputConverter.ConvertInputValues(optionInput.Values, optionSchema.Property.PropertyType);

                // Set value of the underlying property
                optionSchema.Property.SetValue(command, convertedValue);

                // Mark this required option as set
                if (optionSchema.IsRequired)
                    unsetRequiredOptions.Remove(optionSchema);
            }

            // Throw if any of the required options were not set
            if (unsetRequiredOptions.Any())
            {
                var unsetRequiredOptionNames = unsetRequiredOptions.Select(o => o.GetAliases().FirstOrDefault()).JoinToString(", ");
                throw new CliFxException($"One or more required options were not set: {unsetRequiredOptionNames}.");
            }
        }

        private void SetArgumentValues(ICommand command, CommandSchema commandSchema, CommandInput commandInput)
        {
            var positionalArguments = new Queue<string>(GetPositionalArguments(commandSchema, commandInput));
            var unsetRequiredArguments = new HashSet<CommandArgumentSchema>(commandSchema.Arguments.Where(a => a.IsRequired));

            // Loop over defined arguments in defined order
            foreach (var argumentSchema in commandSchema.Arguments.OrderBy(a => a.Order).ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
            {
                // If no more arguments were given
                if (positionalArguments.Count == 0)
                {
                    break;
                }

                IReadOnlyList<string> argumentValues;
                // Use the remaining arguments if the argument is a sequence type
                if (typeof(IEnumerable<>).IsAssignableFrom(argumentSchema.Property.PropertyType))
                {
                    argumentValues = positionalArguments.ToList();
                    positionalArguments.Clear();
                }
                // Otherwise, use just the next argument
                else
                {
                    argumentValues = new List<string>
                    {
                        positionalArguments.Dequeue()
                    };
                }

                // Convert the argument to the type of the underlying property
                var convertedValue = _commandInputConverter.ConvertInputValues(argumentValues, argumentSchema.Property.PropertyType);

                // Set the value of the underlying property
                argumentSchema.Property.SetValue(command, convertedValue);

                // Mark this required argument as set
                if (argumentSchema.IsRequired)
                    unsetRequiredArguments.Remove(argumentSchema);
            }

            // Throw if any of the required options were not set
            if (unsetRequiredArguments.Any())
            {
                var unsetRequiredArgumentNames = unsetRequiredArguments.Select(a => a.Name).JoinToString(", ");
                throw new CliFxException($"One or more required options were not set: {unsetRequiredArgumentNames}.");
            }
        }

        /// <inheritdoc />
        public void InitializeCommand(ICommand command, CommandSchema commandSchema, CommandInput commandInput)
        {
            command.GuardNotNull(nameof(command));
            commandSchema.GuardNotNull(nameof(commandSchema));
            commandInput.GuardNotNull(nameof(commandInput));

            SetCommandOptions(command, commandSchema, commandInput);
            SetArgumentValues(command, commandSchema, commandInput);
        }
    }
}