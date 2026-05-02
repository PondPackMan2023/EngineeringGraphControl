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
            var graph = ScenarioDefinitions.BuildScenarioA();
            graphControl.SetGraphSource(graph);
        }

        private void buttonScenarioB_Click(object sender, System.EventArgs e)
        {
            var graph = ScenarioDefinitions.BuildScenarioB();
            graphControl.SetGraphSource(graph);
        }

        private void buttonScenarioC_Click(object sender, EventArgs e)
        {
            var graph = ScenarioDefinitions.BuildScenarioC();
            graphControl.SetGraphSource(graph);
        }

        private void buttonScenarioD_Click(object sender, EventArgs e)
        {
            var graph = ScenarioDefinitions.BuildScenarioD();
            graphControl.SetGraphSource(graph);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            buttonScenarioA.PerformClick();
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

            graphControl.SetGraphSource(updatedGraph, graphControl.ActiveOptions);
        }

        private static string CreateAxisDisplayName(AxisInteractionDescriptor descriptor)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.AxisId))
            {
                return descriptor.NumericFormatter.Label;
            }

            return descriptor.Orientation + " " + descriptor.Side + " Axis";
        }

        private void checkBoxResizeChart_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
