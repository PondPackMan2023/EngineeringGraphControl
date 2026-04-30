using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Graphing.Editors.EditorModels;

namespace Graphing.Editors.Controls
{
    public partial class SeriesEditorControl : UserControl
    {
        private SeriesEditorModel _model;
        private bool _isUpdatingUi;

        public SeriesEditorControl()
        {
            InitializeComponent();
            WireEvents();
            UpdateEditorState();
        }

        public void LoadControl(SeriesEditorModel model)
        {
            _model = model;

            _seriesListBox.DataSource = null;
            _seriesListBox.DataSource = _model.Series;
            _seriesListBox.SelectedIndex = _model.Series.Count > 0 ? 0 : -1;

            UpdateEditorState();
        }

        private void WireEvents()
        {
            _seriesListBox.Format += seriesListBox_Format;
            _seriesListBox.SelectedIndexChanged += seriesListBox_SelectedIndexChanged;

            _moveUpButton.Click += moveUpButton_Click;
            _moveDownButton.Click += moveDownButton_Click;

            _isVisibleCheckBox.CheckedChanged += isVisibleCheckBox_CheckedChanged;

            _hasLabelOverrideCheckBox.CheckedChanged += hasLabelOverrideCheckBox_CheckedChanged;
            _labelTextBox.TextChanged += labelTextBox_TextChanged;

            _hasColorOverrideCheckBox.CheckedChanged += hasColorOverrideCheckBox_CheckedChanged;
            _pickColorButton.Click += pickColorButton_Click;
        }

        private void seriesListBox_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is SeriesItemEditorModel item)
            {
                e.Value = string.IsNullOrWhiteSpace(item.Label) ? "(label not set)" : item.Label;
            }
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEditorState();
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null || _model == null)
            {
                return;
            }

            _model.MoveUp(selectedItem);
            _seriesListBox.SelectedItem = selectedItem;
            UpdateMoveButtonsState(selectedItem);
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null || _model == null)
            {
                return;
            }

            _model.MoveDown(selectedItem);
            _seriesListBox.SelectedItem = selectedItem;
            UpdateMoveButtonsState(selectedItem);
        }

        private void isVisibleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
            {
                return;
            }

            selectedItem.IsVisible = _isVisibleCheckBox.Checked;
        }

        private void hasLabelOverrideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
            {
                return;
            }

            selectedItem.HasLabelOverride = _hasLabelOverrideCheckBox.Checked;
            _labelTextBox.Enabled = selectedItem.HasLabelOverride;
        }

        private void labelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
            {
                return;
            }

            selectedItem.Label = _labelTextBox.Text;
            _seriesListBox.Refresh();
        }

        private void hasColorOverrideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
            {
                return;
            }

            selectedItem.HasColorOverride = _hasColorOverrideCheckBox.Checked;
            _pickColorButton.Enabled = selectedItem.HasColorOverride;
        }

        private void pickColorButton_Click(object sender, EventArgs e)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
            {
                return;
            }

            _colorDialog.Color = selectedItem.Color;
            if (_colorDialog.ShowDialog(this) == DialogResult.OK)
            {
                selectedItem.Color = _colorDialog.Color;
                _colorSwatchPanel.BackColor = selectedItem.Color;
            }
        }

        private void UpdateEditorState()
        {
            var selectedItem = GetSelectedItem();
            var hasSelection = selectedItem != null;

            _rightPanel.Enabled = hasSelection;

            _isUpdatingUi = true;
            try
            {
                if (!hasSelection)
                {
                    _isVisibleCheckBox.Checked = false;
                    _hasLabelOverrideCheckBox.Checked = false;
                    _labelTextBox.Text = string.Empty;
                    _labelTextBox.Enabled = false;
                    _hasColorOverrideCheckBox.Checked = false;
                    _pickColorButton.Enabled = false;
                    _colorSwatchPanel.BackColor = SystemColors.Control;
                }
                else
                {
                    _isVisibleCheckBox.Checked = selectedItem.IsVisible;
                    _hasLabelOverrideCheckBox.Checked = selectedItem.HasLabelOverride;
                    _labelTextBox.Text = selectedItem.Label ?? string.Empty;
                    _labelTextBox.Enabled = selectedItem.HasLabelOverride;
                    _hasColorOverrideCheckBox.Checked = selectedItem.HasColorOverride;
                    _pickColorButton.Enabled = selectedItem.HasColorOverride;
                    _colorSwatchPanel.BackColor = selectedItem.Color;
                }
            }
            finally
            {
                _isUpdatingUi = false;
            }

            UpdateMoveButtonsState(selectedItem);
        }

        private void UpdateMoveButtonsState(SeriesItemEditorModel selectedItem)
        {
            if (_model == null || selectedItem == null)
            {
                _moveUpButton.Enabled = false;
                _moveDownButton.Enabled = false;
                return;
            }

            int index = _model.Series.IndexOf(selectedItem);
            _moveUpButton.Enabled = index > 0;
            _moveDownButton.Enabled = index >= 0 && index < _model.Series.Count - 1;
        }

        private SeriesItemEditorModel GetSelectedItem()
        {
            return _seriesListBox.SelectedItem as SeriesItemEditorModel;
        }
    }
}
