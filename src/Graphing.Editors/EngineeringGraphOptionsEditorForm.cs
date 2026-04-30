using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Editors.Controls;
using Graphing.Editors.Presentation;
using System;
using System.Windows.Forms;

namespace Graphing.Editors
{
    public partial class EngineeringGraphOptionsEditorForm : Form
    {
        private readonly GraphOptionsPresentationModel _presentationModel;

        public GraphPresentationOptions ResultOptions { get; private set; }

        public EngineeringGraphOptionsEditorForm()
        {
            InitializeComponent();
        }

        public EngineeringGraphOptionsEditorForm(GraphOptionsPresentationModel presentationModel)
        {
            _presentationModel = presentationModel ?? throw new ArgumentNullException(nameof(presentationModel));
            InitializeComponent();
        }

        public static GraphPresentationOptions OpenOptions(IGraphModel graphModel,
            GraphPresentationOptions existingOptions, IWin32Window ownerWindow = null)
        {
            if (graphModel == null)
                throw new ArgumentNullException(nameof(graphModel));

            if (existingOptions == null)
                throw new ArgumentNullException(nameof(existingOptions));

            var presentationModel =
                new GraphOptionsPresentationModel(graphModel, existingOptions);

            using (var dialog =
                   new EngineeringGraphOptionsEditorForm(presentationModel))
            {
                var result = ownerWindow != null
                    ? dialog.ShowDialog(ownerWindow)
                    : dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    return dialog.ResultOptions;
                }

                // Cancel → return original options unchanged
                return existingOptions;
            }
        }

        private void EngineeringGraphOptionsEditorForm_Load(object sender, System.EventArgs e)
        {
            _titlesEditorControl.LoadControl(_presentationModel.Titles);
            _axesEditorControl.LoadControl(_presentationModel.Axes);
            _seriesEditorControl.LoadControl(_presentationModel.Series);
            _legendEditorControl.LoadControl(_presentationModel.Legend);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            ResultOptions = _presentationModel.BuildGraphPresentationOptions();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
