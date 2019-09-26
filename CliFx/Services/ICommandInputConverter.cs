using System;
using System.Collections.Generic;

namespace CliFx.Services
{
    /// <summary>
    /// Converts input command options.
    /// </summary>
    public interface ICommandInputConverter
    {
        /// <summary>
        /// Converts an option to specified target type.
        /// </summary>
        object ConvertInputValues(IReadOnlyList<string> inputValues, Type targetType);
    }
}