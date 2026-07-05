using System;
using System.Collections.Generic;
using Graphing.Core.Pie.Models;
using Graphing.Core.Pie.Snapshot;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class PieGraphSnapshotBuilderTests
    {
        [Test]
        public void Build_PreservesGraphMetadataAndComputesTotal()
        {
            var formatter = new RecordingFormatter("test-fmt");
            var model = new PieGraphModel(
                "Mission Distribution",
                Units.Length.Meter,
                formatter,
                new IPieSliceModel[]
                {
                    new PieSliceModel("id-1", "Fuel", 60.0),
                    new PieSliceModel("id-2", "Mass", 40.0)
                });

            var snapshot = new PieGraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.Title, Is.EqualTo("Mission Distribution"));
            Assert.That(snapshot.Unit, Is.SameAs(Units.Length.Meter));
            Assert.That(snapshot.Formatter, Is.SameAs(formatter));
            Assert.That(snapshot.TotalValue, Is.EqualTo(100.0));
            Assert.That(snapshot.Slices.Count, Is.EqualTo(2));
            Assert.That(snapshot.Slices[0].Label, Is.EqualTo("Fuel"));
            Assert.That(snapshot.Slices[1].Value, Is.EqualTo(40.0));
        }

        [Test]
        public void Build_SingleSlice_ComputesFullPercentageAndSweep()
        {
            var formatter = new RecordingFormatter("single");
            var model = new PieGraphModel(
                "Single",
                Units.Length.Meter,
                formatter,
                new[] { new PieSliceModel("id-1", "Only", 10.0) });

            var slice = new PieGraphSnapshotBuilder().Build(model).Slices[0];

            Assert.That(slice.Percentage, Is.EqualTo(1.0).Within(1e-12));
            Assert.That(slice.StartAngle, Is.EqualTo(0.0).Within(1e-12));
            Assert.That(slice.SweepAngle, Is.EqualTo(360.0).Within(1e-12));
        }

        [Test]
        public void Build_TwoEqualSlices_ComputesExpectedPercentagesAndAngles()
        {
            var formatter = new RecordingFormatter("two");
            var model = new PieGraphModel(
                "Two",
                Units.Length.Meter,
                formatter,
                new IPieSliceModel[]
                {
                    new PieSliceModel("id-1", "A", 1.0),
                    new PieSliceModel("id-2", "B", 1.0)
                });

            var snapshot = new PieGraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.Slices[0].Percentage, Is.EqualTo(0.5).Within(1e-12));
            Assert.That(snapshot.Slices[0].StartAngle, Is.EqualTo(0.0).Within(1e-12));
            Assert.That(snapshot.Slices[0].SweepAngle, Is.EqualTo(180.0).Within(1e-12));
            Assert.That(snapshot.Slices[1].Percentage, Is.EqualTo(0.5).Within(1e-12));
            Assert.That(snapshot.Slices[1].StartAngle, Is.EqualTo(180.0).Within(1e-12));
            Assert.That(snapshot.Slices[1].SweepAngle, Is.EqualTo(180.0).Within(1e-12));
        }

        [Test]
        public void Build_MultipleUnevenSlices_ComputesDeterministicAccumulatedAngles()
        {
            var formatter = new RecordingFormatter("uneven");
            var model = new PieGraphModel(
                "Uneven",
                Units.Length.Meter,
                formatter,
                new IPieSliceModel[]
                {
                    new PieSliceModel("id-1", "A", 10.0),
                    new PieSliceModel("id-2", "B", 20.0),
                    new PieSliceModel("id-3", "C", 30.0)
                });

            var snapshot = new PieGraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.TotalValue, Is.EqualTo(60.0).Within(1e-12));
            Assert.That(snapshot.Slices[0].Percentage, Is.EqualTo(1.0 / 6.0).Within(1e-12));
            Assert.That(snapshot.Slices[0].StartAngle, Is.EqualTo(0.0).Within(1e-12));
            Assert.That(snapshot.Slices[0].SweepAngle, Is.EqualTo(60.0).Within(1e-12));
            Assert.That(snapshot.Slices[1].Percentage, Is.EqualTo(1.0 / 3.0).Within(1e-12));
            Assert.That(snapshot.Slices[1].StartAngle, Is.EqualTo(60.0).Within(1e-12));
            Assert.That(snapshot.Slices[1].SweepAngle, Is.EqualTo(120.0).Within(1e-12));
            Assert.That(snapshot.Slices[2].Percentage, Is.EqualTo(0.5).Within(1e-12));
            Assert.That(snapshot.Slices[2].StartAngle, Is.EqualTo(180.0).Within(1e-12));
            Assert.That(snapshot.Slices[2].SweepAngle, Is.EqualTo(180.0).Within(1e-12));
        }

        [Test]
        public void Build_EmptySliceCollection_ProducesZeroTotalAndNoSlices()
        {
            var formatter = new RecordingFormatter("empty");
            var model = new PieGraphModel("Empty", Units.Length.Meter, formatter, Array.Empty<IPieSliceModel>());

            var snapshot = new PieGraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.TotalValue, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices, Is.Empty);
        }

        [Test]
        public void Build_ZeroValuedSliceWithPositiveTotal_RemainsWithZeroPercentageAndSweep()
        {
            var formatter = new RecordingFormatter("zero-positive-total");
            var model = new PieGraphModel(
                "Zero",
                Units.Length.Meter,
                formatter,
                new IPieSliceModel[]
                {
                    new PieSliceModel("id-1", "Zero", 0.0),
                    new PieSliceModel("id-2", "Positive", 10.0)
                });

            var snapshot = new PieGraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.TotalValue, Is.EqualTo(10.0));
            Assert.That(snapshot.Slices[0].Percentage, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices[0].SweepAngle, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices[0].StartAngle, Is.EqualTo(0.0));
        }

        [Test]
        public void Build_AllZeroSlices_ProducesZeroTotalAndZeroPercentagesWithoutException()
        {
            var formatter = new RecordingFormatter("all-zero");
            var model = new PieGraphModel(
                "AllZero",
                Units.Length.Meter,
                formatter,
                new IPieSliceModel[]
                {
                    new PieSliceModel("id-1", "A", 0.0),
                    new PieSliceModel("id-2", "B", 0.0)
                });

            var snapshot = new PieGraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.TotalValue, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices.Count, Is.EqualTo(2));
            Assert.That(snapshot.Slices[0].Percentage, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices[1].Percentage, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices[0].SweepAngle, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices[1].SweepAngle, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices[0].StartAngle, Is.EqualTo(0.0));
            Assert.That(snapshot.Slices[1].StartAngle, Is.EqualTo(0.0));
        }

        [Test]
        public void Build_ThrowsForNegativeValue()
        {
            var formatter = new RecordingFormatter("invalid-negative");
            var model = new PieGraphModel(
                "Invalid",
                Units.Length.Meter,
                formatter,
                new[] { new PieSliceModel("id-1", "Neg", -1.0) });

            Assert.That(
                () => new PieGraphSnapshotBuilder().Build(model),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("negative value"));
        }

        [Test]
        public void Build_ThrowsForNaNValue()
        {
            var formatter = new RecordingFormatter("invalid-nan");
            var model = new PieGraphModel(
                "Invalid",
                Units.Length.Meter,
                formatter,
                new[] { new PieSliceModel("id-1", "NaN", double.NaN) });

            Assert.That(
                () => new PieGraphSnapshotBuilder().Build(model),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("NaN"));
        }

        [Test]
        public void Build_ThrowsForPositiveInfinityValue()
        {
            var formatter = new RecordingFormatter("invalid-pos-inf");
            var model = new PieGraphModel(
                "Invalid",
                Units.Length.Meter,
                formatter,
                new[] { new PieSliceModel("id-1", "Inf", double.PositiveInfinity) });

            Assert.That(
                () => new PieGraphSnapshotBuilder().Build(model),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("infinite value"));
        }

        [Test]
        public void Build_ThrowsForNegativeInfinityValue()
        {
            var formatter = new RecordingFormatter("invalid-neg-inf");
            var model = new PieGraphModel(
                "Invalid",
                Units.Length.Meter,
                formatter,
                new[] { new PieSliceModel("id-1", "NegInf", double.NegativeInfinity) });

            Assert.That(
                () => new PieGraphSnapshotBuilder().Build(model),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("infinite value"));
        }

        [Test]
        public void Build_ThrowsForNullModel()
        {
            Assert.That(
                () => new PieGraphSnapshotBuilder().Build(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Build_UsesFormatterToProduceFormattedValue()
        {
            var formatter = new RecordingFormatter("format-check");
            var model = new PieGraphModel(
                "Format",
                Units.Length.Meter,
                formatter,
                new[] { new PieSliceModel("id-1", "A", 12.5) });

            var snapshot = new PieGraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.Slices[0].FormattedValue, Is.EqualTo("formatted:12.5"));
            Assert.That(formatter.Calls.Count, Is.EqualTo(1));
            Assert.That(formatter.Calls[0].Value, Is.EqualTo(12.5));
            Assert.That(formatter.Calls[0].FormatProvider, Is.Null);
        }

        [Test]
        public void Build_WithNullFormatter_UsesDeterministicEmptyFormattedValue()
        {
            var model = new PieGraphModel(
                "NullFormatter",
                Units.Length.Meter,
                null,
                new[] { new PieSliceModel("id-1", "A", 1.0) });

            var snapshot = new PieGraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.Formatter, Is.Null);
            Assert.That(snapshot.Slices[0].FormattedValue, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SnapshotSlices_AreReadOnlyAndDefensivelyCopied()
        {
            var source = new List<IPieSliceModel>
            {
                new PieSliceModel("id-1", "A", 1.0)
            };

            var snapshot = new PieGraphSnapshotBuilder().Build(
                new PieGraphModel("Collection", Units.Length.Meter, new RecordingFormatter("collection"), source));

            source.Add(new PieSliceModel("id-2", "B", 2.0));

            Assert.That(snapshot.Slices.Count, Is.EqualTo(1));
            Assert.That(snapshot.Slices[0].Label, Is.EqualTo("A"));
            Assert.That(() => ((ICollection<PieSliceSnapshot>)snapshot.Slices).Add(
                new PieSliceSnapshot(new Graphing.Core.Pie.Presentation.PieSliceId("test"), "X", 0.0, string.Empty, 0.0, 0.0, 0.0)),
                Throws.TypeOf<NotSupportedException>());
        }

        private sealed class RecordingFormatter : IValueFormatter
        {
            private readonly FormatterId _id;

            public RecordingFormatter(string id)
            {
                _id = new FormatterId(id);
                Calls = new List<FormatCall>();
            }

            public FormatterId Id => _id;

            public Type ValueType => typeof(double);

            public List<FormatCall> Calls { get; }

            public string Format(object value, IFormatProvider formatProvider = null)
            {
                Calls.Add(new FormatCall(value, formatProvider));
                return $"formatted:{Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)}";
            }

            public readonly struct FormatCall
            {
                public FormatCall(object value, IFormatProvider formatProvider)
                {
                    Value = value;
                    FormatProvider = formatProvider;
                }

                public object Value { get; }

                public IFormatProvider FormatProvider { get; }
            }
        }
    }
}
