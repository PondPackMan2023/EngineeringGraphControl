using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Editors;
using Graphing.TestHarness.Libraries;
using Graphing.TestHarness.AxisUnits;
using Graphing.TestHarness.Scenarios;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using UnitRegistry;
using UnitRegistry.Formatting;

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

            NumericFormatterLibrary.ChangeFormat(FormatterId.Time_Extended, NumericFormat.Fixed(timeIndex + 1));

            var newTimeUnit = allTimeUnits[timeIndex];
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("time"), newTimeUnit, NumericFormatterLibrary.TimeFormatter);
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private void buttonElevation_Click(object sender, EventArgs e)
        {
            var allLengthUnits = Units.Length.All;
            Random rnd = new Random(DateTime.Now.Millisecond);
            var elevationIndex = rnd.Next(0, allLengthUnits.Length - 1);
            var newLengthUnit = allLengthUnits[elevationIndex];
            NumericFormatterLibrary.ChangeFormat(FormatterId.Elevation, NumericFormat.Fixed(elevationIndex + 1));
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("elevation"), newLengthUnit, NumericFormatterLibrary.ElevationFormatter);
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private void buttonPressure_Click(object sender, EventArgs e)
        {
            var allPressureUnits = Units.Pressure.All;
            Random rnd = new Random(DateTime.Now.Millisecond);
            var pressureIndex = rnd.Next(0, allPressureUnits.Length - 1);
            var newPressureUnit = allPressureUnits[pressureIndex];
            NumericFormatterLibrary.ChangeFormat(FormatterId.Pressure, NumericFormat.Fixed(pressureIndex + 1));
            var graph = graphControl.GraphModel.ChangeAxisUnitAndFormat(new AxisId("pressure"), newPressureUnit, NumericFormatterLibrary.PressureFormatter);
            graphControl.SetGraphSource(graph, CreateOptions());
        }

        private HashSet<AxisId> hiddenAxes = new HashSet<AxisId>();
        private HashSet<SeriesId> hiddenSeries = new HashSet<SeriesId>();
        private HashSet<AxisId> hiddenGridLines = new HashSet<AxisId>();

        private void ShowHideGridLines(AxisId axisId, bool hide)
        {
            if (hide)
            {
                hiddenGridLines.Add(axisId);
            }
            else
            {
                hiddenGridLines.Remove(axisId);
            }
        }

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

            ShowHideGridLines(new AxisId("time"), !checkBoxTimeGrid.Checked);
            ShowHideGridLines(new AxisId("elevation"), !checkBoxElevationGrid.Checked);
            ShowHideGridLines(new AxisId("pressure"), !checkBoxPressureGrid.Checked);
            ShowHideGridLines(new AxisId("pressure2"), !checkBoxPressure2Grid.Checked);

            ShowHideSeries(new SeriesId("pressure-126"), !checkBoxPressure.Checked);
            ShowHideSeries(new SeriesId("hgl-126"), !checkBoxHGL.Checked);

            return new GraphPresentationOptions(hiddenAxisIds: hiddenAxes.ToArray(),
                hiddenSeriesIds: hiddenSeries.ToArray(), hiddenAxisGridLineIds: hiddenGridLines.ToArray(),
                graphTitle: textBoxTitle.Text, graphSubtitle: textBoxSubTitle.Text, resizeChart: checkBoxResizeChart.Checked,
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

        private void checkBoxTimeGrid_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxElevationGrid_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxPressureGrid_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void checkBoxPressure2Grid_CheckedChanged(object sender, EventArgs e)
        {
            ApplyOptions();
        }

        private void buttonOptions_Click(object sender, EventArgs e)
        {
            var options = EngineeringGraphOptionsEditorForm.OpenOptions(graphControl.GraphModel,
                graphControl.ActiveOptions, graphControl.ActiveSnapshot, this);
            graphControl.SetGraphSource(graphControl.GraphModel, options);
        }

        private void graphControl_AxisContextRequested(object sender, Controls.Interaction.AxisInteractionMouseEventArgs e)
        {
            if (e == null || e.Descriptor == null)
            {
                return;
            }

            var menu = new ContextMenuStrip();
            var item = new ToolStripMenuItem("Set Field Options...");
            item.Click += (_, __) => ShowAxisUnitAndNumericFormatDialog(e.Descriptor);
            menu.Items.Add(item);
            menu.Show(graphControl, e.ClientPosition);
        }

        private void ShowAxisUnitAndNumericFormatDialog(AxisInteractionDescriptor descriptor)
        {
            if (descriptor == null || descriptor.DisplayUnit == null || descriptor.NumericFormatter == null)
            {
                return;
            }

            var axisId = new AxisId(descriptor.AxisId);
            var presentationModel = new AxisUnitAndNumericFormatPresentationModel(
                axisId,
                descriptor.DisplayUnit,
                descriptor.NumericFormatter);

            using (var dialog = new AxisUnitAndNumericFormatDialog(CreateAxisDisplayName(descriptor), presentationModel))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }

            var updatedGraph = graphControl.GraphModel.ChangeAxisUnitAndFormat(
                axisId,
                presentationModel.SelectedUnit,
                presentationModel.BuildFormatterToApply());

            graphControl.SetGraphSource(updatedGraph, CreateOptions());
        }

        private static string CreateAxisDisplayName(AxisInteractionDescriptor descriptor)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.AxisId))
            {
                return descriptor.NumericFormatter.Label;
            }

            return descriptor.Orientation + " " + descriptor.Side + " Axis";
        }
    }
}
