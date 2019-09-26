using System;
using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Resolves command schemas.
    /// </summary>
    public interface ICommandSchemaResolver
    {
        /// <summary>
        /// Resolves schemas of specified command types.
        /// </summary>
        IReadOnlyList<CommandSchema> GetCommandSchemas(IReadOnlyList<Type> commandTypes);

        /// <summary>
        /// Finds the most specific command schema that matches the arguments.
        ///
        /// The most specific command schema is the schema with the longest sequence of matching arguments.
        /// </summary>
        /// <param name="unboundArguments">The arguments that are not bound to options.</param>
        /// <param name="availableCommandSchemas">A collection of available command schemas.</param>
        /// <returns>The most specific command schema that matches the argument, or <c>null</c> if no match can be found.</returns>
        CommandSchema GetCommandSchemaFromUnboundArguments(IReadOnlyCollection<string> unboundArguments, IReadOnlyCollection<CommandSchema> availableCommandSchemas);
    }
}