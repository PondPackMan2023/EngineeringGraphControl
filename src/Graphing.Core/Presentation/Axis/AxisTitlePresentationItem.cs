using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Immutable axis-title presentation item rendered inside an axis-title band.
    /// </summary>
    public sealed class AxisTitlePresentationItem
    {
        public AxisTitlePresentationItem(
            string axisId,
            AxisSide side,
            AxisOrientation orientation,
            string title,
            GeometryPoint3D bottomLeft,
            GeometryPoint3D topRight,
            AxisBandRegionGeometry axisTitleRegion,
            AxisBandRegionGeometry axisTickLabelRegion)
        {
            AxisId = axisId;
            Side = side;
            Orientation = orientation;
            Title = title;
            BottomLeft = bottomLeft;
            TopRight = topRight;
            AxisTitleRegion = axisTitleRegion;
            AxisTickLabelRegion = axisTickLabelRegion;
        }

        public string AxisId { get; }

        public AxisSide Side { get; }

        public AxisOrientation Orientation { get; }

        public string Title { get; }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }

        public AxisBandRegionGeometry AxisTitleRegion { get; }

        public AxisBandRegionGeometry AxisTickLabelRegion { get; }
    }
}
