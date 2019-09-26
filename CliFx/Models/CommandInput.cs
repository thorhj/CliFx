using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Parsed command line input.
    /// </summary>
    public partial class CommandInput
    {
        /// <summary>
        /// Arguments not bound to directives or options.
        /// These arguments may be part of the command name or positional arguments.
        /// </summary>
        public IReadOnlyList<string> UnboundArguments { get; }

        /// <summary>
        /// Specified directives.
        /// </summary>
        public IReadOnlyList<string> Directives { get; }

        /// <summary>
        /// Specified options.
        /// </summary>
        public IReadOnlyList<CommandOptionInput> Options { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IEnumerable<string> unboundArguments, IReadOnlyList<string> directives, IReadOnlyList<CommandOptionInput> options)
        {
            UnboundArguments = new List<string>(unboundArguments ?? Enumerable.Empty<string>());
            Directives = directives.GuardNotNull(nameof(directives));
            Options = options.GuardNotNull(nameof(options));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IEnumerable<string> unboundArguments, IReadOnlyList<CommandOptionInput> options)
            : this(unboundArguments, EmptyDirectives, options)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IReadOnlyList<CommandOptionInput> options)
            : this(null, options)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IEnumerable<string> unboundArguments)
            : this(unboundArguments, EmptyOptions)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (UnboundArguments?.Count > 0)
                buffer.Append(string.Join(" ", UnboundArguments));

            foreach (var directive in Directives)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(directive);
            }

            foreach (var option in Options)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(option);
            }

            return buffer.ToString();
        }
    }

    public partial class CommandInput
    {
        private static readonly IReadOnlyList<string> EmptyDirectives = new string[0];
        private static readonly IReadOnlyList<CommandOptionInput> EmptyOptions = new CommandOptionInput[0];

        /// <summary>
        /// Empty input.
        /// </summary>
        public static CommandInput Empty { get; } = new CommandInput(EmptyOptions);
    }
}