using Graphing.Controls.Models;
using Graphing.TestHarness;
using Graphing.TestHarness.AxisUnits;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Tests
{
    [TestFixture]
    public sealed class DateTimeCompositeFormatterTests
    {
        [Test]
        public void ExposesDatetimeFormatterIdentityAndValueType()
        {
            var formatter = new DateTimeCompositeFormatter();

            Assert.That(formatter.Id.Value, Is.EqualTo("datetime"));
            Assert.That(formatter.ValueType, Is.EqualTo(typeof(double)));
        }

        [Test]
        public void FormatsElapsedDurationAndAbsoluteDateTimeModes()
        {
            var formatter = new DateTimeCompositeFormatter(DateTimeFormats.ElapsedTimeLong, CultureInfo.InvariantCulture);

            Assert.That(formatter.Format(90061d), Is.EqualTo("1 day 01:01:01"));

            formatter.SetSelectedFormat(DateTimeFormats.LongTime);
            Assert.That(formatter.Format(3661d), Is.EqualTo("01:01:01"));

            formatter.SetSelectedFormat(DateTimeFormats.SortableDateTime);
            Assert.That(formatter.Format(3661d), Is.EqualTo("1970-01-01T01:01:01"));
        }

        [Test]
        public void TryParseReturnsSecondsForDurationAndDateTimeInputs()
        {
            var formatter = new DateTimeCompositeFormatter(DateTimeFormats.LongTime, CultureInfo.InvariantCulture);

            var durationSuccess = formatter.TryParse("02:03:04", CultureInfo.InvariantCulture, out var durationValue);

            Assert.That(durationSuccess, Is.True);
            Assert.That(durationValue, Is.TypeOf<double>());
            Assert.That((double)durationValue, Is.EqualTo(7384d).Within(0.0001d));

            formatter.SetSelectedFormat(DateTimeFormats.UniversalSortableDateTime);
            var dateSuccess = formatter.TryParse("1970-01-02 00:00:00Z", CultureInfo.InvariantCulture, out var dateValue);

            Assert.That(dateSuccess, Is.True);
            Assert.That(dateValue, Is.TypeOf<double>());
            Assert.That((double)dateValue, Is.EqualTo(86400d).Within(0.0001d));
        }
    }

    [TestFixture]
    public sealed class AxisUnitAndNumericFormatPresentationModelTests
    {
        [Test]
        public void NumericFormatterBehaviorRemainsUnchanged()
        {
            var formatter = new NumericFormatter(
                "time",
                UnitsRegistry.Default,
                "Time",
                NumericFormat.Fixed(2),
                CultureInfo.InvariantCulture);

            var model = new AxisUnitAndNumericFormatPresentationModel(new AxisId("x"), Units.Time.Second, formatter);

            Assert.That(model.IsDateTimeMode, Is.False);
            Assert.That(model.BuildPreviewText(), Is.EqualTo("1.00"));

            model.SetFormatKind(AxisNumericFormatKind.Scientific);
            Assert.That(model.TrySetDisplayPrecision("3"), Is.True);

            var formatterToApply = model.BuildFormatterToApply() as NumericFormatter;
            Assert.That(formatterToApply, Is.Not.Null);
            Assert.That(formatterToApply.FormatSpecifier, Is.EqualTo("E3"));
        }

        [Test]
        public void DatetimeModeDetectionUsesFormatterIdentity()
        {
            var model = new AxisUnitAndNumericFormatPresentationModel(
                new AxisId("x"),
                Units.Time.Second,
                new DatetimeIdOnlyFormatter());

            Assert.That(model.IsDateTimeMode, Is.True);
        }

        private sealed class DatetimeIdOnlyFormatter : IValueFormatter
        {
            public FormatterId Id { get; } = new FormatterId("datetime");

            public Type ValueType => typeof(double);

            public string Format(object value, IFormatProvider formatProvider = null)
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public sealed class AxisUnitAndNumericFormatDialogTests
    {
        [Test]
        public void NumericModeKeepsNumericControlsVisible()
        {
            var formatter = new NumericFormatter(
                "time",
                UnitsRegistry.Default,
                "Time",
                NumericFormat.Fixed(2),
                CultureInfo.InvariantCulture);

            var model = new AxisUnitAndNumericFormatPresentationModel(new AxisId("x"), Units.Time.Second, formatter);

            using (var dialog = new AxisUnitAndNumericFormatDialog("X Axis", model))
            {
                Assert.That(dialog.IsDateTimeModeForTesting, Is.False);
                Assert.That(dialog.ActiveFormatLabelTextForTesting, Is.EqualTo("Format:"));
                Assert.That(dialog.IsPrecisionVisibleForTesting, Is.True);
            }
        }

        [Test]
        public void DatetimeModeShowsDateTimeOptionsAndUpdatesPreviewOnSelectionChange()
        {
            var formatter = new DateTimeCompositeFormatter(DateTimeFormats.ShortDateAndShortTime, CultureInfo.InvariantCulture);
            var model = new AxisUnitAndNumericFormatPresentationModel(new AxisId("x"), Units.Time.Second, formatter);

            using (var dialog = new AxisUnitAndNumericFormatDialog("Time Axis", model))
            {
                Assert.That(dialog.IsDateTimeModeForTesting, Is.True);
                Assert.That(dialog.ActiveFormatLabelTextForTesting, Is.EqualTo("Date/Time Format:"));
                Assert.That(dialog.IsPrecisionVisibleForTesting, Is.False);

                var before = dialog.PreviewTextForTesting;

                dialog.FormatComboForTesting.SelectedValue = DateTimeFormats.ElapsedTimeLong;
                var elapsedPreview = dialog.PreviewTextForTesting;

                Assert.That(elapsedPreview, Does.Contain("day"));

                dialog.FormatComboForTesting.SelectedValue = DateTimeFormats.SortableDateTime;
                var sortablePreview = dialog.PreviewTextForTesting;

                Assert.That(sortablePreview, Is.EqualTo("1970-01-01T00:00:01"));
                Assert.That(sortablePreview, Is.Not.EqualTo(before));
            }
        }
    }
}
