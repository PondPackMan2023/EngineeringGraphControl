using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Graphing.Editors.EditorModels;
using UnitRegistry.Formatting;

namespace Graphing.Editors.Controls
{
    public partial class AxesEditorControl : UserControl
    {
        private AxesEditorModel _model;
        private bool _isUpdatingUi;
        private readonly Dictionary<string, string> _axisDisplayLabels;

        public AxesEditorControl()
        {
            InitializeComponent();
            _axisDisplayLabels = new Dictionary<string, string>(StringComparer.Ordinal);
            WireEvents();
            UpdateEditorState();
        }

        public void LoadControl(AxesEditorModel model)
        {
            _model = model;

            BuildAxisDisplayLabels();

            _axesListBox.DataSource = null;
            _axesListBox.DataSource = _model.Axes;
            _axesListBox.SelectedIndex = _model.Axes.Count > 0 ? 0 : -1;

            UpdateEditorState();
        }

        private void WireEvents()
        {
            _axesListBox.Format += axesListBox_Format;
            _axesListBox.SelectedIndexChanged += axesListBox_SelectedIndexChanged;

            _isVisibleCheckBox.CheckedChanged += isVisibleCheckBox_CheckedChanged;

            _hasTitleOverrideCheckBox.CheckedChanged += hasTitleOverrideCheckBox_CheckedChanged;
            _titleTextBox.TextChanged += titleTextBox_TextChanged;

            _autoRangeRadioButton.CheckedChanged += autoRangeRadioButton_CheckedChanged;
            _fixedRangeRadioButton.CheckedChanged += fixedRangeRadioButton_CheckedChanged;
            _minimumTextBox.TextChanged += minimumTextBox_TextChanged;
            _minimumTextBox.Leave += minimumTextBox_Leave;
            _minimumTextBox.Enter += numericTextBox_Enter;
            _minimumTextBox.MouseUp += numericTextBox_MouseUp;
            _maximumTextBox.TextChanged += maximumTextBox_TextChanged;
            _maximumTextBox.Leave += maximumTextBox_Leave;
            _maximumTextBox.Enter += numericTextBox_Enter;
            _maximumTextBox.MouseUp += numericTextBox_MouseUp;

            _autoIncrementRadioButton.CheckedChanged += autoIncrementRadioButton_CheckedChanged;
            _fixedIncrementRadioButton.CheckedChanged += fixedIncrementRadioButton_CheckedChanged;
            _incrementTextBox.TextChanged += incrementTextBox_TextChanged;
            _incrementTextBox.Leave += incrementTextBox_Leave;
            _incrementTextBox.Enter += numericTextBox_Enter;
            _incrementTextBox.MouseUp += numericTextBox_MouseUp;
        }

        private void BuildAxisDisplayLabels()
        {
            _axisDisplayLabels.Clear();
            if (_model == null)
            {
                return;
            }

            var sideCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _model.Axes.Count; i++)
            {
                var item = _model.Axes[i];
                var sideName = item.Side.ToString();

                int currentCount = 0;
                sideCounts.TryGetValue(sideName, out currentCount);
                currentCount++;
                sideCounts[sideName] = currentCount;

                var display = currentCount == 1
                    ? sideName
                    : sideName + " (" + (currentCount - 1).ToString(CultureInfo.InvariantCulture) + ")";

                _axisDisplayLabels[item.AxisId != null ? item.AxisId.Value : string.Empty] = display;
            }
        }

        private void axesListBox_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is AxisItemEditorModel item)
            {
                var key = item.AxisId != null ? item.AxisId.Value : string.Empty;
                string display;
                if (_axisDisplayLabels.TryGetValue(key, out display))
                {
                    e.Value = display;
                }
                else
                {
                    e.Value = "Other";
                }
            }
        }

        private void axesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEditorState();
        }

        private void isVisibleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            selected.IsVisible = _isVisibleCheckBox.Checked;
        }

        private void hasTitleOverrideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            selected.HasTitleOverride = _hasTitleOverrideCheckBox.Checked;
            _titleTextBox.Enabled = selected.HasTitleOverride;
        }

        private void titleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            selected.Title = _titleTextBox.Text;
        }

        private void autoRangeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi || !_autoRangeRadioButton.Checked)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            selected.HasFixedRange = false;
            UpdateRangeControlsEnabled(false);
        }

        private void fixedRangeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi || !_fixedRangeRadioButton.Checked)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            selected.HasFixedRange = true;
            UpdateRangeControlsEnabled(true);
        }

        private void minimumTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            var formatter = GetNumericFormatter(selected);
            double parsed;
            if (formatter.TryInterpret(_minimumTextBox.Text, out parsed))
            {
                selected.Minimum = parsed;
            }
        }

        private void maximumTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            var formatter = GetNumericFormatter(selected);
            double parsed;
            if (formatter.TryInterpret(_maximumTextBox.Text, out parsed))
            {
                selected.Maximum = parsed;
            }
        }

        private void minimumTextBox_Leave(object sender, EventArgs e)
        {
            CommitNumericText(
                _minimumTextBox,
                axis => axis.Minimum,
                (axis, value) => axis.Minimum = value);
        }

        private void maximumTextBox_Leave(object sender, EventArgs e)
        {
            CommitNumericText(
                _maximumTextBox,
                axis => axis.Maximum,
                (axis, value) => axis.Maximum = value);
        }

        private void autoIncrementRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi || !_autoIncrementRadioButton.Checked)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            selected.HasFixedIncrement = false;
            UpdateIncrementControlsEnabled(false);
        }

        private void fixedIncrementRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi || !_fixedIncrementRadioButton.Checked)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            selected.HasFixedIncrement = true;
            UpdateIncrementControlsEnabled(true);
        }

        private void incrementTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            var formatter = GetNumericFormatter(selected);
            double parsed;
            if (formatter.TryInterpret(_incrementTextBox.Text, out parsed))
            {
                selected.Increment = parsed;
            }
        }

        private void incrementTextBox_Leave(object sender, EventArgs e)
        {
            CommitNumericText(
                _incrementTextBox,
                axis => axis.Increment,
                (axis, value) => axis.Increment = value);
        }

        private void numericTextBox_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void numericTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is TextBox textBox && textBox.SelectionLength == 0)
            {
                textBox.SelectAll();
            }
        }

        private void UpdateEditorState()
        {
            var selected = GetSelectedAxis();
            var hasSelection = selected != null;
            _rightPanel.Enabled = hasSelection;

            _isUpdatingUi = true;
            try
            {
                if (!hasSelection)
                {
                    _isVisibleCheckBox.Checked = false;
                    _hasTitleOverrideCheckBox.Checked = false;
                    _titleTextBox.Text = string.Empty;
                    _titleTextBox.Enabled = false;

                    _autoRangeRadioButton.Checked = true;
                    _fixedRangeRadioButton.Checked = false;
                    _minimumTextBox.Text = string.Empty;
                    _maximumTextBox.Text = string.Empty;

                    _autoIncrementRadioButton.Checked = true;
                    _fixedIncrementRadioButton.Checked = false;
                    _incrementTextBox.Text = string.Empty;

                    _minimumUnitLabel.Text = string.Empty;
                    _maximumUnitLabel.Text = string.Empty;
                    _incrementUnitLabel.Text = string.Empty;
                }
                else
                {
                    _isVisibleCheckBox.Checked = selected.IsVisible;
                    _hasTitleOverrideCheckBox.Checked = selected.HasTitleOverride;
                    _titleTextBox.Text = selected.Title ?? string.Empty;
                    _titleTextBox.Enabled = selected.HasTitleOverride;

                    var formatter = GetNumericFormatter(selected);

                    _fixedRangeRadioButton.Checked = selected.HasFixedRange;
                    _autoRangeRadioButton.Checked = !selected.HasFixedRange;
                    _minimumTextBox.Text = formatter.Format(selected.Minimum);
                    _maximumTextBox.Text = formatter.Format(selected.Maximum);

                    _fixedIncrementRadioButton.Checked = selected.HasFixedIncrement;
                    _autoIncrementRadioButton.Checked = !selected.HasFixedIncrement;
                    _incrementTextBox.Text = formatter.Format(selected.Increment);

                    var unitText = selected.DisplayUnit != null && selected.DisplayUnit.Id != null
                        ? selected.DisplayUnit.Label
                        : string.Empty;
                    _minimumUnitLabel.Text = unitText;
                    _maximumUnitLabel.Text = unitText;
                    _incrementUnitLabel.Text = unitText;

                    UpdateRangeControlsEnabled(selected.HasFixedRange);
                    UpdateIncrementControlsEnabled(selected.HasFixedIncrement);
                }
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private void UpdateRangeControlsEnabled(bool enabled)
        {
            _minimumTextBox.Enabled = enabled;
            _maximumTextBox.Enabled = enabled;
        }

        private void UpdateIncrementControlsEnabled(bool enabled)
        {
            _incrementTextBox.Enabled = enabled;
        }

        private AxisItemEditorModel GetSelectedAxis()
        {
            return _axesListBox.SelectedItem as AxisItemEditorModel;
        }

        private static NumericFormatter GetNumericFormatter(AxisItemEditorModel selected)
        {
            if (selected.NumericFormatter != null)
            {
                return selected.NumericFormatter;
            }

            var formatterLabel = selected.DisplayUnit != null ? selected.DisplayUnit.Label : "Axis";
            selected.NumericFormatter = new NumericFormatter(
                "axis-editor-fallback-" + (selected.AxisId != null ? selected.AxisId.Value : "axis"),
                UnitRegistry.UnitsRegistry.Default,
                formatterLabel,
                "R",
                CultureInfo.CurrentCulture);

            return selected.NumericFormatter;
        }

        private void CommitNumericText(
            TextBox textBox,
            Func<AxisItemEditorModel, double> getCurrentValue,
            Action<AxisItemEditorModel, double> setValue)
        {
            if (_isUpdatingUi)
            {
                return;
            }

            var selected = GetSelectedAxis();
            if (selected == null)
            {
                return;
            }

            var formatter = GetNumericFormatter(selected);
            double parsed;
            if (formatter.TryInterpret(textBox.Text, out parsed))
            {
                setValue(selected, parsed);
                SetTextWithoutUpdatingUi(textBox, formatter.Format(parsed));
                return;
            }

            SetTextWithoutUpdatingUi(textBox, formatter.Format(getCurrentValue(selected)));
        }

        private void SetTextWithoutUpdatingUi(TextBox textBox, string text)
        {
            _isUpdatingUi = true;
            try
            {
                textBox.Text = text;
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }
    }
}
