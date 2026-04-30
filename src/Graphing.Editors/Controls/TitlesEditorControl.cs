using System.Windows.Forms;
using Graphing.Editors.EditorModels;

namespace Graphing.Editors.Controls
{
    public partial class TitlesEditorControl : UserControl
    {
        private TitlesEditorModel _model;
        private bool _isUpdatingUi;

        public TitlesEditorControl()
        {
            InitializeComponent();
        }

        public void LoadControl(TitlesEditorModel model)
        {
            _model = model;

            _titleTextBox.TextChanged -= titleTextBox_TextChanged;
            _subtitleTextBox.TextChanged -= subtitleTextBox_TextChanged;

            _isUpdatingUi = true;
            try
            {
                _titleTextBox.Text = _model != null ? _model.TitleText ?? string.Empty : string.Empty;
                _subtitleTextBox.Text = _model != null ? _model.SubtitleText ?? string.Empty : string.Empty;
                _titleTextBox.Enabled = true;
                _subtitleTextBox.Enabled = true;
            }
            finally
            {
                _isUpdatingUi = false;
            }

            _titleTextBox.TextChanged += titleTextBox_TextChanged;
            _subtitleTextBox.TextChanged += subtitleTextBox_TextChanged;
        }

        private void titleTextBox_TextChanged(object sender, System.EventArgs e)
        {
            if (_isUpdatingUi || _model == null)
            {
                return;
            }

            _model.TitleText = _titleTextBox.Text;
        }

        private void subtitleTextBox_TextChanged(object sender, System.EventArgs e)
        {
            if (_isUpdatingUi || _model == null)
            {
                return;
            }

            _model.SubtitleText = _subtitleTextBox.Text;
        }
    }
}
