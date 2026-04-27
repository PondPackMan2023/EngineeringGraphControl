using Graphing.Controls.Models;
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

            var newTimeUnit = allTimeUnits[timeIndex];
            var graph = graphControl.GraphModel.ChangeAxisUnit(new AxisId("time"), newTimeUnit);
            graphControl.SetGraphSource(graph);
        }
    }
}
