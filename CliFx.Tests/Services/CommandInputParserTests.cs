﻿using System.Collections.Generic;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class CommandInputParserTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ParseCommandInput()
        {
            yield return new TestCaseData(new string[0], CommandInput.Empty);

            yield return new TestCaseData(
                new[] {"--option", "value"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "--option2", "value2"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("option1", "value1"),
                    new CommandOptionInput("option2", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"--option", "value1", "value2"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"--option", "value1", "--option", "value2"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-b", "value2"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", "value1"),
                    new CommandOptionInput("b", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "value2"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-a", "value2"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "-b", "value2"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("option1", "value1"),
                    new CommandOptionInput("b", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"--switch"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("switch")
                })
            );

            yield return new TestCaseData(
                new[] {"--switch1", "--switch2"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("switch1"),
                    new CommandOptionInput("switch2")
                })
            );

            yield return new TestCaseData(
                new[] {"-s"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("s")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "-b"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b")
                })
            );

            yield return new TestCaseData(
                new[] {"-ab"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b")
                })
            );

            yield return new TestCaseData(
                new[] {"-ab", "value"},
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"command"},
                new CommandInput(new [] { "command" })
            );

            yield return new TestCaseData(
                new[] {"command", "--option", "value"},
                new CommandInput(new [] { "command" }, new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"long", "command", "name"},
                new CommandInput(new [] { "long command name" })
            );

            yield return new TestCaseData(
                new[] {"long", "command", "name", "--option", "value"},
                new CommandInput(new [] { "long command name" }, new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"[debug]"},
                new CommandInput(null,
                    new[] {"debug"},
                    new CommandOptionInput[0])
            );

            yield return new TestCaseData(
                new[] {"[debug]", "[preview]"},
                new CommandInput(null,
                    new[] {"debug", "preview"},
                    new CommandOptionInput[0])
            );

            yield return new TestCaseData(
                new[] {"[debug]", "[preview]", "-o", "value"},
                new CommandInput(null,
                    new[] {"debug", "preview"},
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    })
            );

            yield return new TestCaseData(
                new[] {"command", "[debug]", "[preview]", "-o", "value"},
                new CommandInput(new [] { "command" },
                    new[] {"debug", "preview"},
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ParseCommandInput))]
        public void ParseCommandInput_Test(IReadOnlyList<string> commandLineArguments,
            CommandInput expectedCommandInput)
        {
            // Arrange
            var parser = new CommandInputParser();

            // Act
            var commandInput = parser.ParseCommandInput(commandLineArguments);

            // Assert
            commandInput.Should().BeEquivalentTo(expectedCommandInput);
        }
    }
}