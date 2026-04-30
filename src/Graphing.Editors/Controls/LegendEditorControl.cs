using System;
using Graphing.Controls.Presentation;
using System.Windows.Forms;
using Graphing.Editors.EditorModels;

namespace Graphing.Editors.Controls
{
    public partial class LegendEditorControl : UserControl
    {
        private LegendEditorModel _model;
        private bool _isUpdatingUi;
        private LegendPlacement _lastNonNonePlacement;

        public LegendEditorControl()
        {
            InitializeComponent();
            _lastNonNonePlacement = LegendPlacement.Bottom;
            WireEvents();
            UpdateUiFromModel();
        }

        public void LoadControl(LegendEditorModel model)
        {
            _model = model;

            if (_model.Position != LegendPlacement.None)
            {
                _lastNonNonePlacement = _model.Position;
            }

            UpdateUiFromModel();
        }

        private void WireEvents()
        {
            _showLegendCheckBox.CheckedChanged += showLegendCheckBox_CheckedChanged;
            _topRadioButton.CheckedChanged += placementRadioButton_CheckedChanged;
            _bottomRadioButton.CheckedChanged += placementRadioButton_CheckedChanged;
            _leftRadioButton.CheckedChanged += placementRadioButton_CheckedChanged;
            _rightRadioButton.CheckedChanged += placementRadioButton_CheckedChanged;
        }

        private void showLegendCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi || _model == null)
            {
                return;
            }

            if (_showLegendCheckBox.Checked)
            {
                if (_lastNonNonePlacement == LegendPlacement.None)
                {
                    _lastNonNonePlacement = LegendPlacement.Bottom;
                }

                _model.Position = _lastNonNonePlacement;
            }
            else
            {
                if (_model.Position != LegendPlacement.None)
                {
                    _lastNonNonePlacement = _model.Position;
                }

                _model.Position = LegendPlacement.None;
            }

            UpdateUiFromModel();
        }

        private void placementRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi || _model == null)
            {
                return;
            }

            var button = sender as RadioButton;
            if (button == null || !button.Checked || !_showLegendCheckBox.Checked)
            {
                return;
            }

            if (button == _topRadioButton)
            {
                _model.Position = LegendPlacement.Top;
            }
            else if (button == _bottomRadioButton)
            {
                _model.Position = LegendPlacement.Bottom;
            }
            else if (button == _leftRadioButton)
            {
                _model.Position = LegendPlacement.Left;
            }
            else if (button == _rightRadioButton)
            {
                _model.Position = LegendPlacement.Right;
            }

            if (_model.Position != LegendPlacement.None)
            {
                _lastNonNonePlacement = _model.Position;
            }
        }

        private void UpdateUiFromModel()
        {
            _isUpdatingUi = true;
            try
            {
                var position = _model != null ? _model.Position : LegendPlacement.None;
                var showLegend = position != LegendPlacement.None;

                _showLegendCheckBox.Checked = showLegend;

                _topRadioButton.Enabled = showLegend;
                _bottomRadioButton.Enabled = showLegend;
                _leftRadioButton.Enabled = showLegend;
                _rightRadioButton.Enabled = showLegend;

                var selectedPlacement = showLegend
                    ? position
                    : (_lastNonNonePlacement == LegendPlacement.None ? LegendPlacement.Bottom : _lastNonNonePlacement);

                _topRadioButton.Checked = selectedPlacement == LegendPlacement.Top;
                _bottomRadioButton.Checked = selectedPlacement == LegendPlacement.Bottom;
                _leftRadioButton.Checked = selectedPlacement == LegendPlacement.Left;
                _rightRadioButton.Checked = selectedPlacement == LegendPlacement.Right;
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }
    }
}
