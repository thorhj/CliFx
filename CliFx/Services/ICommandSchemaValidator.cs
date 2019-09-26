using System;
using System.Linq;
using System.Runtime.InteropServices;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Validates command schemas.
    /// </summary>
    public interface ICommandSchemaValidator
    {
        /// <summary>
        /// Validates the command schema and throws an exception if a validation fails.
        /// </summary>
        void ValidateCommandSchema(Type commandType, CommandSchema schema);
    }
}
