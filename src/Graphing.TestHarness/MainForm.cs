using Graphing.Controls.Models;
using Graphing.TestHarness.Libraries;
using Graphing.TestHarness.Scenarios;
using System;
using System.Windows.Forms;
using UnitRegistry;

namespace Graphing.TestHarness
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonScenarioA_Click(object sender, System.EventArgs e)
        {
            var graph = ScenarioDefinitions.BuildScenarioA();
            graphControl.SetGraphSource(graph);
        }

        private void buttonScenarioB_Click(object sender, System.EventArgs e)
        {
            var graph = ScenarioDefinitions.BuildScenarioB();
            graphControl.SetGraphSource(graph);
        }

        private void buttonTime_Click(object sender, System.EventArgs e)
        {
            var allTimeUnits = Units.Time.All;
            Random rnd = new Random(DateTime.Now.Millisecond);
            var timeIndex = rnd.Next(0, allTimeUnits.Length - 1);

            var formatter = NumericFormatterLibrary.GetTimeFormatter($"F{timeIndex + 1}");

            var newTimeUnit = allTimeUnits[timeIndex];
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("time"), newTimeUnit, formatter);
            graphControl.SetGraphSource(graph);
        }

        private void buttonElevation_Click(object sender, EventArgs e)
        {
            var allLengthUnits = Units.Length.All;
            Random rnd = new Random(DateTime.Now.Millisecond);
            var elevationIndex = rnd.Next(0, allLengthUnits.Length - 1);
            var newLengthUnit = allLengthUnits[elevationIndex];
            var formatter = NumericFormatterLibrary.GetElevationFormatter($"F{elevationIndex + 1}");
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("elevation"), newLengthUnit, formatter);
            graphControl.SetGraphSource(graph);
        }

        private void buttonPressure_Click(object sender, EventArgs e)
        {
            var allPressureUnits = Units.Pressure.All;
            Random rnd = new Random(DateTime.Now.Millisecond);
            var pressureIndex = rnd.Next(0, allPressureUnits.Length - 1);
            var newPressureUnit = allPressureUnits[pressureIndex];
            var formatter = NumericFormatterLibrary.GetPressureFormatter($"F{pressureIndex + 1}");
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("pressure"), newPressureUnit, formatter);
            graphControl.SetGraphSource(graph);
        }
    }
}
