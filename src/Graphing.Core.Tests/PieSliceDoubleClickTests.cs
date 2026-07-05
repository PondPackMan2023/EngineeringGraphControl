using System;
using System.Collections.Generic;
using System.Windows.Input;
using Graphing.Core.Pie.Models;
using Graphing.Core.Pie.Presentation;
using Graphing.Core.Pie.Snapshot;
using UnitRegistry;
using UnitRegistry.Formatting;
using NUnit.Framework;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class PieSliceInteractionContextTests
    {
        [Test]
        public void Constructor_InitializesAllProperties()
        {
            var sliceId = new PieSliceId("test-slice");
            var label = "Test Label";
            var value = 42.5;
            var formattedValue = "$42.50";
            var percentage = 25.0;

            var context = new PieSliceInteractionContext(sliceId, label, value, formattedValue, percentage);

            Assert.That(context.SliceId, Is.EqualTo(sliceId));
            Assert.That(context.Label, Is.EqualTo(label));
            Assert.That(context.Value, Is.EqualTo(value));
            Assert.That(context.FormattedValue, Is.EqualTo(formattedValue));
            Assert.That(context.Percentage, Is.EqualTo(percentage));
        }

        [Test]
        public void Context_IsImmutable()
        {
            var sliceId = new PieSliceId("test");
            var context = new PieSliceInteractionContext(sliceId, "Label", 100, "Formatted", 50);

            // All properties should be read-only
            Assert.That(context.SliceId, Is.Not.Null);
            Assert.That(context.Label, Is.Not.Null);
            Assert.That(context.FormattedValue, Is.Not.Null);
        }

        [Test]
        public void Context_PreservesPercentageWithDecimal()
        {
            var context = new PieSliceInteractionContext(
                new PieSliceId("test"),
                "Label",
                123.456,
                "123.46",
                33.333);

            Assert.That(context.Percentage, Is.EqualTo(33.333).Within(0.001));
        }

        [Test]
        public void Context_PreservesSliceId()
        {
            var sliceId = new PieSliceId("housing");
            var context = new PieSliceInteractionContext(sliceId, "Housing", 2500, "$2,500", 50.0);

            Assert.That(context.SliceId.Value, Is.EqualTo("housing"));
        }

        [Test]
        public void Context_PreservesAllData()
        {
            var sliceId = new PieSliceId("food");
            var label = "Food";
            var value = 1800.0;
            var formattedValue = "$1,800";
            var percentage = 36.0;

            var context = new PieSliceInteractionContext(sliceId, label, value, formattedValue, percentage);

            Assert.Multiple(() =>
            {
                Assert.That(context.SliceId.Value, Is.EqualTo("food"));
                Assert.That(context.Label, Is.EqualTo("Food"));
                Assert.That(context.Value, Is.EqualTo(1800.0));
                Assert.That(context.FormattedValue, Is.EqualTo("$1,800"));
                Assert.That(context.Percentage, Is.EqualTo(36.0));
            });
        }
    }

    [TestFixture]
    public class PieSliceDoubleClickCommandTests
    {
        private class TestCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public PieSliceInteractionContext ExecutedContext { get; private set; }
            public int ExecuteCount { get; private set; }

            public bool CanExecute(object parameter)
            {
                return parameter is PieSliceInteractionContext;
            }

            public void Execute(object parameter)
            {
                if (parameter is PieSliceInteractionContext context)
                {
                    ExecutedContext = context;
                    ExecuteCount++;
                }
            }
        }

        [Test]
        public void CommandExecution_ReceivesCorrectContext()
        {
            var command = new TestCommand();
            var sliceId = new PieSliceId("test-slice");
            var context = new PieSliceInteractionContext(sliceId, "Housing", 2500, "$2,500", 25.0);

            command.Execute(context);

            Assert.That(command.ExecuteCount, Is.EqualTo(1));
            Assert.That(command.ExecutedContext.SliceId.Value, Is.EqualTo("test-slice"));
            Assert.That(command.ExecutedContext.Label, Is.EqualTo("Housing"));
            Assert.That(command.ExecutedContext.Percentage, Is.EqualTo(25.0));
        }

        [Test]
        public void CommandExecution_WithInvalidParameter_DoesNotExecute()
        {
            var command = new TestCommand();

            command.Execute("not a context");

            Assert.That(command.ExecuteCount, Is.EqualTo(0));
            Assert.That(command.ExecutedContext, Is.Null);
        }

        [Test]
        public void CanExecute_ReturnsTrueForContextParameter()
        {
            var command = new TestCommand();
            var context = new PieSliceInteractionContext(
                new PieSliceId("test"),
                "Label",
                100,
                "100",
                50);

            Assert.That(command.CanExecute(context), Is.True);
        }

        [Test]
        public void CanExecute_ReturnsFalseForInvalidParameter()
        {
            var command = new TestCommand();

            Assert.That(command.CanExecute("not a context"), Is.False);
            Assert.That(command.CanExecute(null), Is.False);
        }

        [Test]
        public void CommandExecution_WithMultipleContexts()
        {
            var command = new TestCommand();
            var context1 = new PieSliceInteractionContext(
                new PieSliceId("slice1"),
                "First",
                100,
                "100",
                25);
            var context2 = new PieSliceInteractionContext(
                new PieSliceId("slice2"),
                "Second",
                200,
                "200",
                50);

            command.Execute(context1);
            Assert.That(command.ExecuteCount, Is.EqualTo(1));
            Assert.That(command.ExecutedContext.Label, Is.EqualTo("First"));

            command.Execute(context2);
            Assert.That(command.ExecuteCount, Is.EqualTo(2));
            Assert.That(command.ExecutedContext.Label, Is.EqualTo("Second"));
        }
    }
}

