using Graphing.Controls.Models;
using Graphing.TestHarness.Fields;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.TestHarness.Scenarios
{
    internal static class ScenarioDefinitions
    {
        internal static IGraphModel BuildScenarioA()
        {
            double[] times = new double[]
            {
                0, 3442, 3600, 7200, 10800,
                14400, 18000, 21600, 25200, 28800,
                32400, 36000, 39600, 43200, 46800,
                48600, 50400, 54000, 57600, 61200,
                61312, 64800, 68400, 72000, 75600,
                79200, 82800, 86400
            };
            double[] hgl = new double[]
            {
                178.916874, 178.325934, 178.323273, 178.263054, 178.205178,
                178.149535, 178.096069, 178.044946, 178.010493, 177.700000,
                177.284007, 176.879436, 176.489544, 176.113604, 175.750910,
                177.142583, 177.486841, 178.201346, 178.943626, 179.791984,
                178.325729, 178.248562, 178.154428, 178.064629, 178.014418,
                177.966439, 177.948468, 177.902741
            };

            var timeField = new GraphFieldDefinition("Time", "Time", Units.Time.Second, times);
            var hglField = new GraphFieldDefinition("HGL", "Hydraulic Grade Line", Units.Length.Meter, hgl);

            var xAxis = new AxisModel(new AxisId("time"), AxisOrientation.X, AxisSide.Bottom, Units.Time.Second, "sec",
                new NumericFormatter(new NumericFormatterId("time"), UnitsRegistry.Default));
            var yAxis = new AxisModel(new AxisId("elevation"), AxisOrientation.Y, AxisSide.Left, Units.Length.Meter, "m",
                new NumericFormatter(new NumericFormatterId("elevation"), UnitsRegistry.Default));

            var series = new GraphSeriesModel(126, "J-1", ChartType.Line, timeField, hglField, xAxis, yAxis);
            var graph = new GraphModel(new[] { xAxis, yAxis }, new[] { series });

            return graph;
        }

        internal static IGraphModel BuildScenarioB()
        {
            double[] times = new double[]
            {
                0, 3442, 3600, 7200, 10800,
                14400, 18000, 21600, 25200, 28800,
                32400, 36000, 39600, 43200, 46800,
                48600, 50400, 54000, 57600, 61200,
                61312, 64800, 68400, 72000, 75600,
                79200, 82800, 86400
            };
            double[] hgl = new double[]
            {
                178.916874, 178.325934, 178.323273, 178.263054, 178.205178,
                178.149535, 178.096069, 178.044946, 178.010493, 177.700000,
                177.284007, 176.879436, 176.489544, 176.113604, 175.750910,
                177.142583, 177.486841, 178.201346, 178.943626, 179.791984,
                178.325729, 178.248562, 178.154428, 178.064629, 178.014418,
                177.966439, 177.948468, 177.902741
            };
            double[] pressures = new double[]
            {
                45.304852, 44.466034, 44.462257, 44.376778, 44.294624,
                44.215641, 44.139748, 44.067181, 44.018276, 43.577541,
                42.987053, 42.412781, 41.859344, 41.325710, 40.810879,
                42.786308, 43.274971, 44.289185, 45.342827, 46.547039,
                44.465744, 44.356205, 44.222588, 44.095119, 44.023846,
                43.955742, 43.930233, 43.865326
            };

            var timeField = new GraphFieldDefinition("Time", "Time", Units.Time.Second, times);
            var hglField = new GraphFieldDefinition("HGL", "Hydraulic Grade Line", Units.Length.Meter, hgl);
            var pressureField = new GraphFieldDefinition("Pressure", "Pressure", Units.Pressure.Psi, pressures);

            var xAxis = new AxisModel(new AxisId("time"), AxisOrientation.X, AxisSide.Bottom, Units.Time.Second, "sec",
                new NumericFormatter(new NumericFormatterId("time"), UnitsRegistry.Default));
            var lengthYAxis = new AxisModel(new AxisId("elevation"), AxisOrientation.Y, AxisSide.Left, Units.Length.Meter, "m",
                new NumericFormatter(new NumericFormatterId("elevation"), UnitsRegistry.Default));
            var pressureYAxis = new AxisModel(new AxisId("pressure"), AxisOrientation.Y, AxisSide.Left, Units.Pressure.Psi, "psi",
                new NumericFormatter(new NumericFormatterId("pressure"), UnitsRegistry.Default));

            var hglSeries = new GraphSeriesModel(126, "J-1", ChartType.Line, timeField, hglField, xAxis, lengthYAxis);
            var psiSeries = new GraphSeriesModel(126, "J-1", ChartType.Line, timeField, pressureField, xAxis, pressureYAxis);

            var graph = new GraphModel(new[] { xAxis, lengthYAxis, pressureYAxis }, new[] { hglSeries, psiSeries });

            return graph;
        }
    }
}
