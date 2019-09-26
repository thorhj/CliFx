using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a property that defines a command option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandArgumentAttribute : Attribute
    {
        /// <summary>
        /// Argument name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether an argument is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Argument description, which is used in help text.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The positional argument order (lower meaning earlier).
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandArgumentAttribute"/>.
        /// </summary>
        public CommandArgumentAttribute(string name)
        {
            Name = name; // can be null
        }
    }
}