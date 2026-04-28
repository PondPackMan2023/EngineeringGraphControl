using System.Collections.Generic;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Axis-title band geometry allocated by the presentation model.
    /// </summary>
    public sealed class AxisTitleBandGeometry
    {
        public AxisTitleBandGeometry(
            AxisSide side,
            GeometryPoint3D bottomLeft,
            GeometryPoint3D topRight,
            AxisBandRegionGeometry axisTitleRegion,
            AxisBandRegionGeometry axisTickLabelRegion,
            IReadOnlyList<AxisTitlePresentationItem> items)
        {
            Side = side;
            BottomLeft = bottomLeft;
            TopRight = topRight;
            AxisTitleRegion = axisTitleRegion;
            AxisTickLabelRegion = axisTickLabelRegion;
            Items = items;
        }

        public AxisSide Side { get; }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }

        public AxisBandRegionGeometry AxisTitleRegion { get; }

        public AxisBandRegionGeometry AxisTickLabelRegion { get; }

        public IReadOnlyList<AxisTitlePresentationItem> Items { get; }
    }
}
