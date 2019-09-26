using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandSchemaResolver"/>.
    /// </summary>
    public class CommandSchemaResolver : ICommandSchemaResolver
    {
        private readonly ICommandSchemaValidator _commandSchemaValidator;

        /// <summary>
        /// Initializes an instance of <see cref="CommandSchemaResolver"/>.
        /// </summary>
        public CommandSchemaResolver(ICommandSchemaValidator commandSchemaValidator)
        {
            _commandSchemaValidator = commandSchemaValidator;
        }

        private IReadOnlyList<CommandArgumentSchema> GetCommandArgumentSchemas(Type commandType)
        {
            var result = new List<CommandArgumentSchema>();

            foreach (var property in commandType.GetProperties())
            {
                var attribute = property.GetCustomAttribute<CommandArgumentAttribute>();

                // If an attribute is not set, then it's not an argument so we just skip it
                if (attribute is null)
                    continue;

                // Build argument schema
                var argumentSchema = new CommandArgumentSchema(property,
                    attribute.Name,
                    attribute.IsRequired,
                    attribute.Description,
                    attribute.Order);

                result.Add(argumentSchema);
            }

            return result;
        }

        private IReadOnlyList<CommandOptionSchema> GetCommandOptionSchemas(Type commandType)
        {
            var result = new List<CommandOptionSchema>();

            foreach (var property in commandType.GetProperties())
            {
                var attribute = property.GetCustomAttribute<CommandOptionAttribute>();

                // If an attribute is not set, then it's not an option so we just skip it
                if (attribute == null)
                    continue;

                // Build option schema
                var optionSchema = new CommandOptionSchema(property,
                    attribute.Name,
                    attribute.ShortName,
                    attribute.IsRequired,
                    attribute.Description);

                // Add schema to list
                result.Add(optionSchema);
            }

            return result;
        }

        /// <inheritdoc />
        public CommandSchema GetCommandSchemaFromUnboundArguments(IReadOnlyCollection<string> unboundArguments, IReadOnlyCollection<CommandSchema> availableCommandSchemas)
        {
            var unboundArgumentsString = string.Join(" ", unboundArguments);
            var mostSpecificSchema = availableCommandSchemas.OrderByDescending(schema => schema.Name)
                .FirstOrDefault(schema => unboundArgumentsString.StartsWith(schema.Name));
            return mostSpecificSchema;
        }

        /// <inheritdoc />
        public IReadOnlyList<CommandSchema> GetCommandSchemas(IReadOnlyList<Type> commandTypes)
        {
            commandTypes.GuardNotNull(nameof(commandTypes));

            // Make sure there's at least one command defined
            if (!commandTypes.Any())
            {
                throw new CliFxException("There are no commands defined.");
            }

            var result = new List<CommandSchema>();

            foreach (var commandType in commandTypes)
            {
                // Make sure command type implements ICommand.
                if (!commandType.Implements(typeof(ICommand)))
                {
                    throw new CliFxException($"Command type [{commandType}] must implement {typeof(ICommand)}.");
                }

                // Get attribute
                var attribute = commandType.GetCustomAttribute<CommandAttribute>();

                // Make sure attribute is set
                if (attribute == null)
                {
                    throw new CliFxException($"Command type [{commandType}] must be annotated with [{typeof(CommandAttribute)}].");
                }

                // Get option schemas
                var optionSchemas = GetCommandOptionSchemas(commandType);
                var argumentSchemas = GetCommandArgumentSchemas(commandType);

                // Build command schema
                var commandSchema = new CommandSchema(commandType,
                    attribute.Name,
                    attribute.Description,
                    optionSchemas,
                    argumentSchemas);

                // Make sure there are no other commands with the same name
                var existingCommandWithSameName = result
                    .FirstOrDefault(c => string.Equals(c.Name, commandSchema.Name, StringComparison.OrdinalIgnoreCase));

                if (existingCommandWithSameName != null)
                {
                    throw new CliFxException(
                        $"Command type [{existingCommandWithSameName.Type}] has the same name as another command type [{commandType}].");
                }

                _commandSchemaValidator.ValidateCommandSchema(commandType, commandSchema);

                // Add schema to list
                result.Add(commandSchema);
            }

            return result;
        }
    }
}