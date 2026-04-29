using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.TestHarness.Libraries;
using Graphing.TestHarness.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
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
            checkBoxPressureYAxis.Checked = true;
            checkBoxPressure.Checked = true;

            checkBoxPressureYAxis.Visible = false;
            checkBoxPressure.Visible = false;

            var graph = ScenarioDefinitions.BuildScenarioA();
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private void buttonScenarioB_Click(object sender, System.EventArgs e)
        {
            checkBoxPressureYAxis.Checked = true;
            checkBoxPressure.Checked = true;

            checkBoxPressureYAxis.Visible = true;
            checkBoxPressure.Visible = true;

            var graph = ScenarioDefinitions.BuildScenarioB();
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private void buttonScenarioC_Click(object sender, EventArgs e)
        {
            checkBoxPressureYAxis.Checked = true;
            checkBoxPressure.Checked = true;

            checkBoxPressureYAxis.Visible = false;
            checkBoxPressure.Visible = false;

            var graph = ScenarioDefinitions.BuildScenarioC();
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private void buttonScenarioD_Click(object sender, EventArgs e)
        {
            checkBoxPressureYAxis.Checked = true;
            checkBoxPressure.Checked = true;

            checkBoxPressureYAxis.Visible = false;
            checkBoxPressure.Visible = false;

            var graph = ScenarioDefinitions.BuildScenarioD();
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private void buttonTime_Click(object sender, System.EventArgs e)
        {
            var allTimeUnits = Units.Time.All;
            Random rnd = new Random(DateTime.Now.Millisecond);
            var timeIndex = rnd.Next(0, allTimeUnits.Length - 1);

            var formatter = NumericFormatterLibrary.GetTimeFormatter($"F{timeIndex + 1}");

            var newTimeUnit = allTimeUnits[timeIndex];
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("time"), newTimeUnit, formatter);
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private void buttonElevation_Click(object sender, EventArgs e)
        {
            var allLengthUnits = Units.Length.All;
            Random rnd = new Random(DateTime.Now.Millisecond);
            var elevationIndex = rnd.Next(0, allLengthUnits.Length - 1);
            var newLengthUnit = allLengthUnits[elevationIndex];
            var formatter = NumericFormatterLibrary.GetElevationFormatter($"F{elevationIndex + 1}");
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("elevation"), newLengthUnit, formatter);
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private void buttonPressure_Click(object sender, EventArgs e)
        {
            var allPressureUnits = Units.Pressure.All;
            Random rnd = new Random(DateTime.Now.Millisecond);
            var pressureIndex = rnd.Next(0, allPressureUnits.Length - 1);
            var newPressureUnit = allPressureUnits[pressureIndex];
            var formatter = NumericFormatterLibrary.GetPressureFormatter($"F{pressureIndex + 1}");
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("pressure"), newPressureUnit, formatter);
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private HashSet<AxisId> hiddenAxes = new HashSet<AxisId>();
        private HashSet<SeriesId> hiddenSeries = new HashSet<SeriesId>();

        private void ShowHideAxis(AxisId axisId, bool hide)
        {
            if (hide)
            {
                hiddenAxes.Add(axisId);
            }
            else
            {
                hiddenAxes.Remove(axisId);
            }
        }

        private void ShowHideSeries(SeriesId seriesId, bool hide)
        {
            if (hide)
            {
                hiddenSeries.Add(seriesId);
            }
            else
            {
                hiddenSeries.Remove(seriesId);
            }
        }

        private GraphPresentationOptions CreateOptions()
        {
            LegendPlacement legend = LegendPlacement.None;
            if (checkBoxShowLegend.Checked)
            {
                if (radioButtonBottom.Checked)
                {
                    legend = LegendPlacement.Bottom;
                }
                else if (radioButtonLeft.Checked)
                {
                    legend = LegendPlacement.Left;
                }
                else if (radioButtonTop.Checked)
                {
                    legend = LegendPlacement.Top;
                }
                else if (radioButtonRight.Checked)
                {
                    legend = LegendPlacement.Right;
                }
            }

            ShowHideAxis(new AxisId("time"), !checkBoxXAxis.Checked);
            ShowHideAxis(new AxisId("elevation"), !checkBoxElevationYAxis.Checked);
            ShowHideAxis(new AxisId("pressure"), !checkBoxPressureYAxis.Checked);

            ShowHideSeries(new SeriesId("pressure-126"), !checkBoxPressure.Checked);
            ShowHideSeries(new SeriesId("hgl-126"), !checkBoxHGL.Checked);

            return new GraphPresentationOptions(hiddenAxisIds: hiddenAxes.ToArray(),
                hiddenSeriesIds: hiddenSeries.ToArray(), graphTitle: textBoxTitle.Text,
                graphSubtitle: textBoxSubTitle.Text, resizeChart: checkBoxResizeChart.Checked,
                legendPlacement: legend);
        }

        private void ApplyOptions()
        {
            ShowHideAxis(new AxisId("time"), !checkBoxXAxis.Checked);
            ShowHideAxis(new AxisId("elevation"), !checkBoxElevationYAxis.Checked);
            ShowHideAxis(new AxisId("pressure"), !checkBoxPressureYAxis.Checked);

            ShowHideSeries(new SeriesId("pressure-126"), !checkBoxPressure.Checked);
            ShowHideSeries(new SeriesId("hgl-126"), !checkBoxHGL.Checked);

            graphControl.SetGraphSource(graphControl.GraphModel, CreateOptions());
        }

        private void checkBoxXAxis_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxElevationYAxis_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxPressureYAxis_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxHGL_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxPressure_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            buttonScenarioA.PerformClick();
            ApplyOptions();
        }

        private void buttonApplyTitles_Click(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxResizeChart_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void radioButtonBottom_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void radioButtonLeft_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void radioButtonTop_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void radioButtonRight_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxShowLegend_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }
    }
}
