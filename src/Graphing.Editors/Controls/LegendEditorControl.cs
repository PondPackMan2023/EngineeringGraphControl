using System.Windows.Forms;
using Graphing.Editors.EditorModels;

namespace Graphing.Editors.Controls
{
    public partial class LegendEditorControl : UserControl
    {
        private LegendEditorModel _model;

        public LegendEditorControl()
        {
            InitializeComponent();
        }

        public void LoadControl(LegendEditorModel model)
        {
            _model = model;
        }
    }
}
