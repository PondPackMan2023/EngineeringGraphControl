using System;
using System.Windows.Forms;
using Graphing.Controls.Presentation;
using Graphing.Editors.Controls;
using Graphing.Editors.Presentation;

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
