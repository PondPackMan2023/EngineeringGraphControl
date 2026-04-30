using System.Windows.Forms;
using Graphing.Editors.EditorModels;

namespace Graphing.Editors.Controls
{
    public partial class SeriesEditorControl : UserControl
    {
        private SeriesEditorModel _model;

        public SeriesEditorControl()
        {
            InitializeComponent();
        }

        public void LoadControl(SeriesEditorModel model)
        {
            _model = model;
        }
    }
}
