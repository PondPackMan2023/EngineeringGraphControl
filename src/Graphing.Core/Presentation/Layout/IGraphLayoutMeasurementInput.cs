using System.Collections.Generic;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Advisory measurement input supplied by a renderer.
    /// All values are returned in abstract normalized layout units.
    /// </summary>
    public interface IGraphLayoutMeasurementInput
    {
        double MeasureAxisTickThickness(AxisSide side, IReadOnlyList<AxisTickPresentation> ticks);

        double MeasureAxisTitleThickness(AxisSide side, string title);

        double MeasureAxisEndpointLabelExtent(AxisSide side, IReadOnlyList<AxisTickPresentation> ticks);

        LegendMeasurementAdvice MeasureLegend(
            LegendPlacement placement,
            IReadOnlyList<SeriesPresentationGeometry> series,
            double availablePrimarySpan);

        double MeasureTitleThickness(string text, bool isSubtitle);
    }
}
