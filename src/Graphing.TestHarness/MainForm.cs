using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Controls.Utilities;
using Graphing.Editors;
using Graphing.TestScenarios.AxisUnits;
using Graphing.TestScenarios.Scenarios;
using System;
using System.IO;
using System.Windows.Forms;
using UnitRegistry.Formatting;

namespace Graphing.TestScenarios
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
            if (descriptor == null || descriptor.DisplayUnit == null || descriptor.Formatter == null)
            {
                return;
            }

            var axisId = new AxisId(descriptor.AxisId);
            var presentationModel = new AxisUnitAndNumericFormatPresentationModel(
                axisId,
                descriptor.DisplayUnit,
                descriptor.Formatter);

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
            var numericFormatter = descriptor != null ? descriptor.Formatter as NumericFormatter : null;
            if (!string.IsNullOrWhiteSpace(descriptor.AxisId))
            {
                return numericFormatter != null ? numericFormatter.Label : descriptor.AxisId;
            }

            return descriptor.Orientation + " " + descriptor.Side + " Axis";
        }

        private void checkBoxResizeChart_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonExportPng_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG Image|*.png";
                saveDialog.Title = "Export Graph as PNG";
                saveDialog.FileName = "GraphExport.png";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var stream = File.Create(saveDialog.FileName))
                        {
                            GraphExport.ExportPng(graphControl.Size, graphControl.ActivePresentation, stream, graphControl.ActiveOptions);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export graph: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void buttonExportEmf_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "EMF Image|*.emf";
                saveDialog.Title = "Export Graph as EMF";
                saveDialog.FileName = "GraphExport.emf";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var stream = File.Create(saveDialog.FileName))
                        {
                            GraphExport.ExportMetafile(graphControl.Size, graphControl.ActivePresentation, stream, graphControl.ActiveOptions);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export graph: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
