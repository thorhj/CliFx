using System.Reflection;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Schema of a defined command option.
    /// </summary>
    public class CommandArgumentSchema
    {
        /// <summary>
        /// Underlying property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Argument name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether an argument is required.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Argument description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The positional argument order (lower meaning earlier).
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandArgumentSchema"/>.
        /// </summary>
        public CommandArgumentSchema(PropertyInfo property, string name, bool isRequired, string description, int order)
        {
            Property = property; // can be null
            Name = name; // can be null
            IsRequired = isRequired;
            Description = description; // can be null
            Order = order;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (IsRequired)
                buffer.Append('*');

            if (!Name.IsNullOrWhiteSpace())
                buffer.Append(Name);

            return buffer.ToString();
        }
    }
}