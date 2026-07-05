using System;
using System.Collections.Generic;
using Graphing.Core.Pie.Presentation;
using Graphing.Core.Pie.Snapshot;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class PieGraphPresentationBuilderTests
    {
        [Test]
        public void Build_CreatesPresentationAndPreservesSnapshotSliceMath()
        {
            var snapshot = CreateSnapshot(
                "Mission",
                new[]
                {
                    new PieSliceSnapshot(new PieSliceId("id-1"), "Fuel", 60.0, "60.0", 0.6, 0.0, 216.0),
                    new PieSliceSnapshot(new PieSliceId("id-2"), "Mass", 40.0, "40.0", 0.4, 216.0, 144.0)
                });

            var presentation = new PieGraphPresentationBuilder().Build(snapshot);

            Assert.That(presentation.Title, Is.EqualTo("Mission"));
            Assert.That(presentation.Slices.Count, Is.EqualTo(2));

            Assert.That(presentation.Slices[0].Value, Is.EqualTo(60.0));
            Assert.That(presentation.Slices[0].FormattedValue, Is.EqualTo("60.0"));
            Assert.That(presentation.Slices[0].Percentage, Is.EqualTo(0.6));
            Assert.That(presentation.Slices[0].StartAngle, Is.EqualTo(0.0));
            Assert.That(presentation.Slices[0].SweepAngle, Is.EqualTo(216.0));

            Assert.That(presentation.Slices[1].Value, Is.EqualTo(40.0));
            Assert.That(presentation.Slices[1].FormattedValue, Is.EqualTo("40.0"));
            Assert.That(presentation.Slices[1].Percentage, Is.EqualTo(0.4));
            Assert.That(presentation.Slices[1].StartAngle, Is.EqualTo(216.0));
            Assert.That(presentation.Slices[1].SweepAngle, Is.EqualTo(144.0));
        }

        [Test]
        public void Build_UsesDeterministicChartLevelCenterAndRadius_WithLegendVisibleByDefault()
        {
            var snapshot = CreateSnapshot("Default", new[] { new PieSliceSnapshot(new PieSliceId("id-1"), "A", 1.0, "1.0", 1.0, 0.0, 360.0) });

            var presentation = new PieGraphPresentationBuilder().Build(snapshot);

            Assert.That(presentation.Options.LegendVisible, Is.True);
            Assert.That(presentation.Center.X, Is.EqualTo(0.375).Within(1e-12));
            Assert.That(presentation.Center.Y, Is.EqualTo(0.5).Within(1e-12));
            Assert.That(presentation.Radius, Is.EqualTo(0.325).Within(1e-12));
        }

        [Test]
        public void Build_UsesLargerPieArea_WhenLegendHidden()
        {
            var snapshot = CreateSnapshot("HiddenLegend", new[] { new PieSliceSnapshot(new PieSliceId("id-1"), "A", 1.0, "1.0", 1.0, 0.0, 360.0) });

            var presentation = new PieGraphPresentationBuilder().Build(snapshot, new PieGraphPresentationOptions(false));

            Assert.That(presentation.Options.LegendVisible, Is.False);
            Assert.That(presentation.Center.X, Is.EqualTo(0.5).Within(1e-12));
            Assert.That(presentation.Center.Y, Is.EqualTo(0.5).Within(1e-12));
            Assert.That(presentation.Radius, Is.EqualTo(0.4).Within(1e-12));
            Assert.That(presentation.Legend, Is.Null);
        }

        [Test]
        public void PiePalette_Has16Entries_AndRepeatsAfter16ByIndex()
        {
            Assert.That(PiePalette.Default.Count, Is.EqualTo(16));
            Assert.That(PiePalette.GetColorForIndex(0), Is.EqualTo(PiePalette.GetColorForIndex(16)));
            Assert.That(PiePalette.GetColorForIndex(1), Is.EqualTo(PiePalette.GetColorForIndex(17)));
        }

        [Test]
        public void Build_AssignsColorsDeterministicallyBySliceOrder()
        {
            var snapshot = CreateSnapshot(
                "Colors",
                new[]
                {
                    new PieSliceSnapshot(new PieSliceId("id-1"), "A", 1.0, "1.0", 1.0 / 17.0, 0.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-2"), "B", 1.0, "1.0", 1.0 / 17.0, 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-3"), "C", 1.0, "1.0", 1.0 / 17.0, 2.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-4"), "D", 1.0, "1.0", 1.0 / 17.0, 3.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-5"), "E", 1.0, "1.0", 1.0 / 17.0, 4.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-6"), "F", 1.0, "1.0", 1.0 / 17.0, 5.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-7"), "G", 1.0, "1.0", 1.0 / 17.0, 6.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-8"), "H", 1.0, "1.0", 1.0 / 17.0, 7.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-9"), "I", 1.0, "1.0", 1.0 / 17.0, 8.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-10"), "J", 1.0, "1.0", 1.0 / 17.0, 9.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-11"), "K", 1.0, "1.0", 1.0 / 17.0, 10.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-12"), "L", 1.0, "1.0", 1.0 / 17.0, 11.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-13"), "M", 1.0, "1.0", 1.0 / 17.0, 12.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-14"), "N", 1.0, "1.0", 1.0 / 17.0, 13.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-15"), "O", 1.0, "1.0", 1.0 / 17.0, 14.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-16"), "P", 1.0, "1.0", 1.0 / 17.0, 15.0 * 360.0 / 17.0, 360.0 / 17.0),
                    new PieSliceSnapshot(new PieSliceId("id-17"), "Q", 1.0, "1.0", 1.0 / 17.0, 16.0 * 360.0 / 17.0, 360.0 / 17.0)
                });

            var first = new PieGraphPresentationBuilder().Build(snapshot);
            var second = new PieGraphPresentationBuilder().Build(snapshot);

            Assert.That(first.Slices[0].Color, Is.EqualTo(second.Slices[0].Color));
            Assert.That(first.Slices[16].Color, Is.EqualTo(second.Slices[16].Color));
            Assert.That(first.Slices[0].Color, Is.EqualTo(first.Slices[16].Color));
        }

        [Test]
        public void Build_CreatesRightSideLegendUsingSliceLabelsAndColors_WhenVisible()
        {
            var snapshot = CreateSnapshot(
                "Legend",
                new[]
                {
                    new PieSliceSnapshot(new PieSliceId("id-1"), "Fuel", 60.0, "60", 0.6, 0.0, 216.0),
                    new PieSliceSnapshot(new PieSliceId("id-2"), "Mass", 40.0, "40", 0.4, 216.0, 144.0)
                });

            var presentation = new PieGraphPresentationBuilder().Build(snapshot, new PieGraphPresentationOptions(true));

            Assert.That(presentation.Legend, Is.Not.Null);
            Assert.That(presentation.Legend.Placement, Is.EqualTo(PieLegendPlacement.Right));
            Assert.That(presentation.Legend.Bounds.Left, Is.EqualTo(0.72).Within(1e-12));
            Assert.That(presentation.Legend.Bounds.Right, Is.EqualTo(0.87).Within(1e-12));
            Assert.That(presentation.Legend.Entries.Count, Is.EqualTo(2));
            Assert.That(presentation.Legend.Entries[0].Label, Is.EqualTo("Fuel"));
            Assert.That(presentation.Legend.Entries[1].Label, Is.EqualTo("Mass"));
            Assert.That(presentation.Legend.Entries[0].Color, Is.EqualTo(presentation.Slices[0].Color));
            Assert.That(presentation.Legend.Entries[1].Color, Is.EqualTo(presentation.Slices[1].Color));
        }

        [Test]
        public void Build_EmptySnapshot_ProducesValidPresentationWithoutException()
        {
            var snapshot = CreateSnapshot("Empty", Array.Empty<PieSliceSnapshot>());

            var presentation = new PieGraphPresentationBuilder().Build(snapshot);

            Assert.That(presentation.Slices, Is.Empty);
            Assert.That(presentation.Legend, Is.Not.Null);
            Assert.That(presentation.Legend.Entries, Is.Empty);
        }

        [Test]
        public void Build_ZeroSweepSlices_AreRepresentedWithoutException()
        {
            var snapshot = CreateSnapshot(
                "ZeroSweep",
                new[]
                {
                    new PieSliceSnapshot(new PieSliceId("id-1"), "A", 0.0, "0", 0.0, 0.0, 0.0),
                    new PieSliceSnapshot(new PieSliceId("id-2"), "B", 10.0, "10", 1.0, 0.0, 360.0)
                });

            var presentation = new PieGraphPresentationBuilder().Build(snapshot);

            Assert.That(presentation.Slices.Count, Is.EqualTo(2));
            Assert.That(presentation.Slices[0].SweepAngle, Is.EqualTo(0.0));
            Assert.That(presentation.Slices[0].StartAngle, Is.EqualTo(0.0));
        }

        [Test]
        public void PresentationCollections_AreReadOnlyAndDefensive()
        {
            var inputSlices = new List<PieSliceSnapshot>
            {
                new PieSliceSnapshot(new PieSliceId("id-1"), "A", 1.0, "1", 1.0, 0.0, 360.0)
            };

            var snapshot = CreateSnapshot("Safety", inputSlices);
            var presentation = new PieGraphPresentationBuilder().Build(snapshot);

            inputSlices.Add(new PieSliceSnapshot(new PieSliceId("id-2"), "B", 2.0, "2", 0.0, 360.0, 0.0));

            Assert.That(presentation.Slices.Count, Is.EqualTo(1));
            Assert.That(() => ((ICollection<PieSlicePresentationGeometry>)presentation.Slices).Add(
                new PieSlicePresentationGeometry(new PieSliceId("id-x"), "X", 0.0, "0", 0.0, 0.0, 0.0, PieColor.Empty)),
                Throws.TypeOf<NotSupportedException>());
            Assert.That(() => ((ICollection<PieLegendEntryPresentationGeometry>)presentation.Legend.Entries).Add(
                new PieLegendEntryPresentationGeometry("X", PieColor.Empty, new PieBounds(0, 0, 0, 0))),
                Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void Build_ThrowsForNullSnapshot()
        {
            Assert.That(
                () => new PieGraphPresentationBuilder().Build(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        private static PieGraphSnapshot CreateSnapshot(string title, IReadOnlyList<PieSliceSnapshot> slices)
        {
            var total = 0d;
            for (var i = 0; i < slices.Count; i++)
            {
                total += slices[i].Value;
            }

            return new PieGraphSnapshot(title, Units.Length.Meter, new NullFormatter("snapshot-fmt"), total, slices);
        }

        private sealed class NullFormatter : IValueFormatter
        {
            private readonly FormatterId _id;

            public NullFormatter(string id)
            {
                _id = new FormatterId(id);
            }

            public FormatterId Id => _id;

            public Type ValueType => typeof(double);

            public string Format(object value, IFormatProvider formatProvider = null)
            {
                return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}
