using System.Windows.Forms;
using Graphing.Editors.EditorModels;

namespace Graphing.Editors.Controls
{
    public partial class AxesEditorControl : UserControl
    {
        private AxesEditorModel _model;

        public AxesEditorControl()
        {
            InitializeComponent();
        }

        public void LoadControl(AxesEditorModel model)
        {
            _model = model;
        }
    }
}
