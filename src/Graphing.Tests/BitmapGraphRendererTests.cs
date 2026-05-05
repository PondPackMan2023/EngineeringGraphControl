using System.Drawing;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Snapshot;
using NUnit.Framework;
using UnitRegistry;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Tests
{
    /// <summary>
    /// Tests for <see cref="BitmapGraphRenderer"/> that validate renderer invocation,
    /// bitmap dimensions, and DPI configuration. Pixel-perfect assertions are excluded
    /// per Phase RB-1 scope constraints.
    /// </summary>
    [TestFixture]
    public class BitmapGraphRendererTests
    {
        private static GraphPresentationModel BuildMinimalPresentation()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var series = new GraphSeriesModel(new SeriesId("s"), "s", SeriesType.Line, xField, yField, xAxis, yAxis);
            var model = new GraphModel(new IAxisModel[] { xAxis, yAxis }, new IGraphSeriesModel[] { series });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            return new GraphPresentationModel(snapshot);
        }

        [Test]
        public void BitmapRenderer_RendersWithoutException()
        {
            var renderer = new BitmapGraphRenderer();
            var presentation = BuildMinimalPresentation();

            using (var bmp = renderer.RenderToBitmap(400, 300, presentation))
            {
                Assert.That(bmp, Is.Not.Null);
            }
        }

        [Test]
        public void BitmapRenderer_ReturnsBitmapWithRequestedDimensions()
        {
            const int width = 800;
            const int height = 600;

            var renderer = new BitmapGraphRenderer();
            var presentation = BuildMinimalPresentation();

            using (var bmp = renderer.RenderToBitmap(width, height, presentation))
            {
                Assert.That(bmp.Width, Is.EqualTo(width));
                Assert.That(bmp.Height, Is.EqualTo(height));
            }
        }

        [Test]
        public void BitmapRenderer_BitmapHasRequestedDpi()
        {
            const float dpi = 144f;

            var renderer = new BitmapGraphRenderer(dpiX: dpi, dpiY: dpi);
            var presentation = BuildMinimalPresentation();

            using (var bmp = renderer.RenderToBitmap(400, 300, presentation))
            {
                Assert.That(bmp.HorizontalResolution, Is.EqualTo(dpi).Within(0.01f));
                Assert.That(bmp.VerticalResolution, Is.EqualTo(dpi).Within(0.01f));
            }
        }

        [Test]
        public void BitmapRenderer_ImplementsIGraphRendererInterface()
        {
            IGraphRenderer renderer = new BitmapGraphRenderer();
            Assert.That(renderer, Is.InstanceOf<IGraphRenderer>());
        }

        [Test]
        public void BitmapRenderer_AcceptsNullOptions_WithoutException()
        {
            var renderer = new BitmapGraphRenderer();
            var presentation = BuildMinimalPresentation();

            Assert.That(() =>
            {
                using (renderer.RenderToBitmap(400, 300, presentation, null)) { }
            }, Throws.Nothing);
        }
    }
}
