﻿using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command(Description = "Reads one option value from environment variables because target property is not a collection.")]
    public class EnvironmentVariableWithoutCollectionPropertyCommand : ICommand
    {
        [CommandOption("opt", EnvironmentVariableName = "ENV_MULTIPLE_VALUES")]
        public string? Option { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}
