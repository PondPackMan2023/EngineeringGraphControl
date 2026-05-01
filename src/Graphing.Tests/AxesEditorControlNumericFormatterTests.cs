using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Graphing.Controls.Models;
using Graphing.Editors.Controls;
using Graphing.Editors.EditorModels;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Tests
{
    [TestFixture]
    public class AxesEditorControlNumericFormatterTests
    {
        [Test]
        [Apartment(ApartmentState.STA)]
        public void LoadControl_DisplaysRangeAndIncrement_UsingNumericFormatter()
        {
            var formatter = new NumericFormatter(
                "fmt-axis-display",
                UnitsRegistry.Default,
                "Y",
                "F4",
                CultureInfo.InvariantCulture);

            var axis = CreateAxisItem(formatter, minimum: 12.34567, maximum: 98.76543, increment: 0.125);
            var model = new AxesEditorModel();
            model.Axes.Add(axis);

            using (var control = new AxesEditorControl())
            {
                control.LoadControl(model);

                var minimumTextBox = GetPrivateField<TextBox>(control, "_minimumTextBox");
                var maximumTextBox = GetPrivateField<TextBox>(control, "_maximumTextBox");
                var incrementTextBox = GetPrivateField<TextBox>(control, "_incrementTextBox");

                Assert.That(minimumTextBox.Text, Is.EqualTo(formatter.Format(axis.Minimum)));
                Assert.That(maximumTextBox.Text, Is.EqualTo(formatter.Format(axis.Maximum)));
                Assert.That(incrementTextBox.Text, Is.EqualTo(formatter.Format(axis.Increment)));
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void EditedValues_RoundTripThroughFormatterInterpretation()
        {
            var formatter = new NumericFormatter(
                "fmt-axis-roundtrip",
                UnitsRegistry.Default,
                "Y",
                "R",
                CultureInfo.InvariantCulture);

            var axis = CreateAxisItem(formatter, minimum: 1.0, maximum: 2.0, increment: 0.5);
            var model = new AxesEditorModel();
            model.Axes.Add(axis);

            var editedMinimum = 12345.6789012345;
            var editedMaximum = -9876.54321098765;
            var editedIncrement = 0.0009765625;

            using (var control = new AxesEditorControl())
            {
                control.LoadControl(model);

                var minimumTextBox = GetPrivateField<TextBox>(control, "_minimumTextBox");
                var maximumTextBox = GetPrivateField<TextBox>(control, "_maximumTextBox");
                var incrementTextBox = GetPrivateField<TextBox>(control, "_incrementTextBox");

                minimumTextBox.Text = formatter.Format(editedMinimum);
                maximumTextBox.Text = formatter.Format(editedMaximum);
                incrementTextBox.Text = formatter.Format(editedIncrement);

                Assert.That(axis.Minimum, Is.EqualTo(editedMinimum));
                Assert.That(axis.Maximum, Is.EqualTo(editedMaximum));
                Assert.That(axis.Increment, Is.EqualTo(editedIncrement));
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Commit_NormalizesTextToFormatterOutput_WhenInterpretationSucceeds()
        {
            var formatter = new NumericFormatter(
                "fmt-axis-normalize",
                UnitsRegistry.Default,
                "Y",
                "F2",
                CultureInfo.InvariantCulture);

            var axis = CreateAxisItem(formatter, minimum: 1.0, maximum: 2.0, increment: 0.5);
            var model = new AxesEditorModel();
            model.Axes.Add(axis);

            using (var control = new AxesEditorControl())
            {
                control.LoadControl(model);

                var minimumTextBox = GetPrivateField<TextBox>(control, "_minimumTextBox");
                var maximumTextBox = GetPrivateField<TextBox>(control, "_maximumTextBox");
                var incrementTextBox = GetPrivateField<TextBox>(control, "_incrementTextBox");

                minimumTextBox.Text = "12.3400";
                maximumTextBox.Text = "98.7650";
                incrementTextBox.Text = "0.5000";

                InvokePrivateHandler(control, "minimumTextBox_Leave", minimumTextBox, EventArgs.Empty);
                InvokePrivateHandler(control, "maximumTextBox_Leave", maximumTextBox, EventArgs.Empty);
                InvokePrivateHandler(control, "incrementTextBox_Leave", incrementTextBox, EventArgs.Empty);

                Assert.That(axis.Minimum, Is.EqualTo(12.34));
                Assert.That(axis.Maximum, Is.EqualTo(98.765));
                Assert.That(axis.Increment, Is.EqualTo(0.5));

                Assert.That(minimumTextBox.Text, Is.EqualTo(formatter.Format(axis.Minimum)));
                Assert.That(maximumTextBox.Text, Is.EqualTo(formatter.Format(axis.Maximum)));
                Assert.That(incrementTextBox.Text, Is.EqualTo(formatter.Format(axis.Increment)));
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void InvalidInput_DoesNotOverwriteExistingValues()
        {
            var formatter = new NumericFormatter(
                "fmt-axis-invalid",
                UnitsRegistry.Default,
                "Y",
                "R",
                CultureInfo.InvariantCulture);

            var axis = CreateAxisItem(formatter, minimum: 10.25, maximum: 20.5, increment: 0.75);
            var model = new AxesEditorModel();
            model.Axes.Add(axis);

            using (var control = new AxesEditorControl())
            {
                control.LoadControl(model);

                var minimumTextBox = GetPrivateField<TextBox>(control, "_minimumTextBox");
                var maximumTextBox = GetPrivateField<TextBox>(control, "_maximumTextBox");
                var incrementTextBox = GetPrivateField<TextBox>(control, "_incrementTextBox");

                minimumTextBox.Text = "not-a-number";
                maximumTextBox.Text = "still-invalid";
                incrementTextBox.Text = "?";

                Assert.That(axis.Minimum, Is.EqualTo(10.25));
                Assert.That(axis.Maximum, Is.EqualTo(20.5));
                Assert.That(axis.Increment, Is.EqualTo(0.75));
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Commit_InvalidInput_RevertsTextToLastValidFormattedValue()
        {
            var formatter = new NumericFormatter(
                "fmt-axis-revert",
                UnitsRegistry.Default,
                "Y",
                "F3",
                CultureInfo.InvariantCulture);

            var axis = CreateAxisItem(formatter, minimum: 10.25, maximum: 20.5, increment: 0.75);
            var model = new AxesEditorModel();
            model.Axes.Add(axis);

            using (var control = new AxesEditorControl())
            {
                control.LoadControl(model);

                var minimumTextBox = GetPrivateField<TextBox>(control, "_minimumTextBox");
                var maximumTextBox = GetPrivateField<TextBox>(control, "_maximumTextBox");
                var incrementTextBox = GetPrivateField<TextBox>(control, "_incrementTextBox");

                minimumTextBox.Text = "bad-min";
                maximumTextBox.Text = "bad-max";
                incrementTextBox.Text = "bad-inc";

                InvokePrivateHandler(control, "minimumTextBox_Leave", minimumTextBox, EventArgs.Empty);
                InvokePrivateHandler(control, "maximumTextBox_Leave", maximumTextBox, EventArgs.Empty);
                InvokePrivateHandler(control, "incrementTextBox_Leave", incrementTextBox, EventArgs.Empty);

                Assert.That(axis.Minimum, Is.EqualTo(10.25));
                Assert.That(axis.Maximum, Is.EqualTo(20.5));
                Assert.That(axis.Increment, Is.EqualTo(0.75));

                Assert.That(minimumTextBox.Text, Is.EqualTo(formatter.Format(axis.Minimum)));
                Assert.That(maximumTextBox.Text, Is.EqualTo(formatter.Format(axis.Maximum)));
                Assert.That(incrementTextBox.Text, Is.EqualTo(formatter.Format(axis.Increment)));
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void EnteringNumericTextBox_SelectsAllText()
        {
            var formatter = new NumericFormatter(
                "fmt-axis-select-all",
                UnitsRegistry.Default,
                "Y",
                "F2",
                CultureInfo.InvariantCulture);

            var axis = CreateAxisItem(formatter, minimum: 10.25, maximum: 20.5, increment: 0.75);
            var model = new AxesEditorModel();
            model.Axes.Add(axis);

            using (var control = new AxesEditorControl())
            {
                control.LoadControl(model);

                var minimumTextBox = GetPrivateField<TextBox>(control, "_minimumTextBox");
                var maximumTextBox = GetPrivateField<TextBox>(control, "_maximumTextBox");
                var incrementTextBox = GetPrivateField<TextBox>(control, "_incrementTextBox");

                minimumTextBox.Select(0, 0);
                maximumTextBox.Select(0, 0);
                incrementTextBox.Select(0, 0);

                InvokePrivateHandler(control, "numericTextBox_Enter", minimumTextBox, EventArgs.Empty);
                InvokePrivateHandler(control, "numericTextBox_Enter", maximumTextBox, EventArgs.Empty);
                InvokePrivateHandler(control, "numericTextBox_Enter", incrementTextBox, EventArgs.Empty);

                Assert.That(minimumTextBox.SelectionStart, Is.EqualTo(0));
                Assert.That(minimumTextBox.SelectionLength, Is.EqualTo(minimumTextBox.TextLength));
                Assert.That(maximumTextBox.SelectionStart, Is.EqualTo(0));
                Assert.That(maximumTextBox.SelectionLength, Is.EqualTo(maximumTextBox.TextLength));
                Assert.That(incrementTextBox.SelectionStart, Is.EqualTo(0));
                Assert.That(incrementTextBox.SelectionLength, Is.EqualTo(incrementTextBox.TextLength));
            }
        }

        private static AxisItemEditorModel CreateAxisItem(NumericFormatter formatter, double minimum, double maximum, double increment)
        {
            return new AxisItemEditorModel(new AxisId("y-axis"), AxisSide.Left)
            {
                IsVisible = true,
                HasTitleOverride = false,
                Title = string.Empty,
                HasFixedRange = true,
                Minimum = minimum,
                Maximum = maximum,
                HasFixedIncrement = true,
                Increment = increment,
                DisplayUnit = Units.Length.Meter,
                NumericFormatter = formatter
            };
        }

        private static T GetPrivateField<T>(object instance, string fieldName)
            where T : class
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected field '{fieldName}' to exist.");

            var value = field.GetValue(instance) as T;
            Assert.That(value, Is.Not.Null, $"Expected field '{fieldName}' value to be assignable to {typeof(T).Name}.");
            return value;
        }

        private static void InvokePrivateHandler(object instance, string methodName, object sender, EventArgs eventArgs)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected method '{methodName}' to exist.");
            method.Invoke(instance, new object[] { sender, eventArgs });
        }
    }
}
