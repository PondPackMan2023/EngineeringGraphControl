using System.Drawing;
using System.IO;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Snapshot;
using Graphing.Controls.Utilities;
using NUnit.Framework;
using UnitRegistry;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Tests
{
    [TestFixture]
    public class MetafileGraphRendererTests
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
        public void MetafileRenderer_ImplementsIGraphRendererInterface()
        {
            IGraphRenderer renderer = new MetafileGraphRenderer();
            Assert.That(renderer, Is.InstanceOf<IGraphRenderer>());
        }

        [Test]
        public void MetafileRenderer_CreatesNonEmptyEmfPlusStream()
        {
            var renderer = new MetafileGraphRenderer();
            var presentation = BuildMinimalPresentation();

            using (var stream = new MemoryStream())
            {
                Assert.That(() => renderer.RenderToMetafile(400, 300, stream, presentation), Throws.Nothing);
                Assert.That(stream.Length, Is.GreaterThan(0));
            }
        }

        [Test]
        public void GraphExport_ExportsMetafileToStream()
        {
            var presentation = BuildMinimalPresentation();

            using (var stream = new MemoryStream())
            {
                Assert.That(() => GraphExport.ExportMetafile(new Size(400, 300), presentation, stream), Throws.Nothing);
                Assert.That(stream.Length, Is.GreaterThan(0));
            }
        }

        [Test]
        public void RendererSiblings_RenderSamePresentationIndependently()
        {
            var presentation = BuildMinimalPresentation();

            Assert.That(() =>
            {
                using (var bitmap = new Bitmap(400, 300))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    new WinFormsGraphRenderer().Render(graphics, new Rectangle(0, 0, 400, 300), presentation);
                }

                using (new BitmapGraphRenderer().RenderToBitmap(400, 300, presentation))
                {
                }

                using (var stream = new MemoryStream())
                {
                    new MetafileGraphRenderer().RenderToMetafile(400, 300, stream, presentation);
                    Assert.That(stream.Length, Is.GreaterThan(0));
                }
            }, Throws.Nothing);
        }
    }
}