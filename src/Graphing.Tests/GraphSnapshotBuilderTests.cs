using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Snapshot;
using NUnit.Framework;
using UnitRegistry;

namespace Graphing.Tests
{
    [TestFixture]
    public class GraphSnapshotBuilderTests
    {
        [Test]
        public void Build_Throws_WhenAxisIdsAreDuplicated()
        {
            var unit = Units.Length.Meter;
            var duplicateAxisId = new AxisId("axis-duplicate");

            var xAxis = new AxisModel(duplicateAxisId, AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(duplicateAxisId, AxisOrientation.Y, AxisSide.Left, unit, "m", null);

            var model = new GraphModel(new IAxisModel[] { xAxis, yAxis }, new IGraphSeriesModel[0]);
            var builder = new GraphSnapshotBuilder();

            Assert.That(
                () => builder.Build(model),
                Throws.TypeOf<System.InvalidOperationException>()
                    .With.Message.Contains("duplicate AxisId")
                    .And.Message.Contains("axis-duplicate"));
        }
    }
}
