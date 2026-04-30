using System.Windows.Forms;
using Graphing.Editors.EditorModels;

namespace Graphing.Editors.Controls
{
    public partial class TitlesEditorControl : UserControl
    {
        private TitlesEditorModel _model;

        public TitlesEditorControl()
        {
            InitializeComponent();
        }

        public void LoadControl(TitlesEditorModel model)
        {
            _model = model;
        }
    }
}
