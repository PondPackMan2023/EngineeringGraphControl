using Graphing.TestScenarios.AxisUnits;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Graphing.TestScenarios
{
    internal sealed class AxisUnitAndNumericFormatDialog : Form
    {
        private readonly AxisUnitAndNumericFormatPresentationModel presentationModel;
        private bool isBinding;
        private bool isPrecisionModeActive;

        private readonly ComboBox comboUnit;
        private readonly ComboBox comboFormat;
        private readonly TextBox textPrecision;
        private readonly TextBox textPreview;
        private readonly Label labelFormat;
        private readonly Label labelPrecision;
        private readonly Label labelPreviewUnit;
        private readonly Label labelPreviewBase;
        private readonly Button buttonOk;
        private readonly ErrorProvider precisionErrorProvider;

        public AxisUnitAndNumericFormatDialog(string axisDisplayName, AxisUnitAndNumericFormatPresentationModel presentationModel)
        {
            if (axisDisplayName == null)
            {
                throw new ArgumentNullException(nameof(axisDisplayName));
            }

            this.presentationModel = presentationModel ?? throw new ArgumentNullException(nameof(presentationModel));

            Text = "Set Field Options - " + axisDisplayName;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 250);

            var groupPreview = new GroupBox
            {
                Text = "Preview",
                Location = new Point(12, 12),
                Size = new Size(396, 84)
            };

            var labelBaseCaption = new Label
            {
                Text = "Base Value:",
                AutoSize = true,
                Location = new Point(14, 27)
            };

            labelPreviewBase = new Label
            {
                AutoSize = true,
                Location = new Point(94, 27)
            };

            var labelPreviewCaption = new Label
            {
                Text = "Formatted:",
                AutoSize = true,
                Location = new Point(14, 53)
            };

            textPreview = new TextBox
            {
                ReadOnly = true,
                Location = new Point(94, 50),
                Size = new Size(180, 23)
            };

            labelPreviewUnit = new Label
            {
                AutoSize = true,
                Location = new Point(288, 53)
            };

            groupPreview.Controls.Add(labelBaseCaption);
            groupPreview.Controls.Add(labelPreviewBase);
            groupPreview.Controls.Add(labelPreviewCaption);
            groupPreview.Controls.Add(textPreview);
            groupPreview.Controls.Add(labelPreviewUnit);

            var labelUnit = new Label
            {
                Text = "Unit:",
                AutoSize = true,
                Location = new Point(20, 114)
            };

            comboUnit = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(140, 110),
                Size = new Size(268, 23),
                DisplayMember = "Label"
            };

            labelFormat = new Label
            {
                Text = "Format:",
                AutoSize = true,
                Location = new Point(20, 147)
            };

            comboFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(140, 143),
                Size = new Size(268, 23),
                DisplayMember = "Name",
                ValueMember = "Kind"
            };

            labelPrecision = new Label
            {
                Text = "Display Precision:",
                AutoSize = true,
                Location = new Point(20, 180)
            };

            textPrecision = new TextBox
            {
                Location = new Point(140, 176),
                Size = new Size(80, 23)
            };

            buttonOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(252, 214),
                Size = new Size(75, 28)
            };

            var buttonCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(333, 214),
                Size = new Size(75, 28)
            };

            precisionErrorProvider = new ErrorProvider
            {
                BlinkStyle = ErrorBlinkStyle.NeverBlink,
                ContainerControl = this
            };

            Controls.Add(groupPreview);
            Controls.Add(labelUnit);
            Controls.Add(comboUnit);
            Controls.Add(labelFormat);
            Controls.Add(comboFormat);
            Controls.Add(labelPrecision);
            Controls.Add(textPrecision);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;

            comboUnit.SelectedIndexChanged += comboUnit_SelectedIndexChanged;
            comboFormat.SelectedIndexChanged += comboFormat_SelectedIndexChanged;
            textPrecision.TextChanged += textPrecision_TextChanged;
            buttonOk.Click += buttonOk_Click;

            BindPresentationModel();
            RefreshPreview();
        }

        private void BindPresentationModel()
        {
            isBinding = true;

            var units = presentationModel.AvailableUnits.ToList();
            comboUnit.DataSource = units;

            var selectedUnitIndex = units.FindIndex(unit => unit.Equals(presentationModel.SelectedUnit));
            comboUnit.SelectedIndex = selectedUnitIndex >= 0 ? selectedUnitIndex : 0;

            labelPreviewBase.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0:G} {1}",
                presentationModel.PreviewValue,
                presentationModel.PreviewSourceUnitLabel);

            if (presentationModel.IsDateTimeMode)
            {
                BindDateTimeMode();
            }
            else
            {
                BindNumericMode();
            }

            isBinding = false;

            ValidateAndUpdatePrecision();
            RefreshPreview();
        }

        private void BindNumericMode()
        {
            var formats = new List<NumericFormatOption>
            {
                new NumericFormatOption("Scientific", AxisNumericFormatKind.Scientific),
                new NumericFormatOption("Fixed Point", AxisNumericFormatKind.FixedPoint),
                new NumericFormatOption("General", AxisNumericFormatKind.General),
                new NumericFormatOption("Number", AxisNumericFormatKind.Number)
            };

            labelFormat.Text = "Format:";
            comboFormat.DisplayMember = "Name";
            comboFormat.ValueMember = "Kind";
            comboFormat.DataSource = formats;

            var selectedFormatIndex = formats.FindIndex(option => option.Kind == presentationModel.SelectedFormatKind);
            comboFormat.SelectedIndex = selectedFormatIndex >= 0 ? selectedFormatIndex : 0;

            textPrecision.Text = presentationModel.DisplayPrecision.ToString(CultureInfo.InvariantCulture);
            labelPrecision.Visible = true;
            textPrecision.Visible = true;
            isPrecisionModeActive = true;
        }

        private void BindDateTimeMode()
        {
            var formats = presentationModel.AvailableDateTimeFormats
                .Select(format => new DateTimeFormatOption(presentationModel.GetDateTimeFormatDisplayName(format), format))
                .ToList();

            labelFormat.Text = "Date/Time Format:";
            comboFormat.DisplayMember = "Name";
            comboFormat.ValueMember = "Format";
            comboFormat.DataSource = formats;

            var selectedFormatIndex = formats.FindIndex(option => option.Format == presentationModel.SelectedDateTimeFormat);
            comboFormat.SelectedIndex = selectedFormatIndex >= 0 ? selectedFormatIndex : 0;

            labelPrecision.Visible = false;
            textPrecision.Visible = false;
            isPrecisionModeActive = false;
            precisionErrorProvider.SetError(textPrecision, string.Empty);
            buttonOk.Enabled = true;
        }

        private void comboUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isBinding)
            {
                return;
            }

            if (comboUnit.SelectedItem is UnitRegistry.Unit selectedUnit)
            {
                presentationModel.SelectUnit(selectedUnit);
                RefreshPreview();
            }
        }

        private void comboFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isBinding)
            {
                return;
            }

            if (presentationModel.IsDateTimeMode)
            {
                if (comboFormat.SelectedValue is DateTimeFormats selectedDateTimeFormat)
                {
                    presentationModel.SetDateTimeFormat(selectedDateTimeFormat);
                    RefreshPreview();
                }

                return;
            }

            if (comboFormat.SelectedValue is AxisNumericFormatKind selectedKind)
            {
                presentationModel.SetFormatKind(selectedKind);
                RefreshPreview();
            }
        }

        private void textPrecision_TextChanged(object sender, EventArgs e)
        {
            if (isBinding)
            {
                return;
            }

            ValidateAndUpdatePrecision();
            RefreshPreview();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!ValidateAndUpdatePrecision())
            {
                DialogResult = DialogResult.None;
            }
        }

        private bool ValidateAndUpdatePrecision()
        {
            if (presentationModel.IsDateTimeMode)
            {
                precisionErrorProvider.SetError(textPrecision, string.Empty);
                buttonOk.Enabled = true;
                return true;
            }

            if (presentationModel.TrySetDisplayPrecision(textPrecision.Text))
            {
                precisionErrorProvider.SetError(textPrecision, string.Empty);
                buttonOk.Enabled = true;
                return true;
            }

            precisionErrorProvider.SetError(textPrecision, "Enter a non-negative integer precision.");
            buttonOk.Enabled = false;
            return false;
        }

        private void RefreshPreview()
        {
            textPreview.Text = presentationModel.BuildPreviewText();
            labelPreviewUnit.Text = presentationModel.PreviewUnitLabel;
        }

        internal bool IsDateTimeModeForTesting => presentationModel.IsDateTimeMode;

        internal string ActiveFormatLabelTextForTesting => labelFormat.Text;

        internal bool IsPrecisionVisibleForTesting => isPrecisionModeActive;

        internal string PreviewTextForTesting => textPreview.Text;

        internal ComboBox FormatComboForTesting => comboFormat;

        private sealed class NumericFormatOption
        {
            public NumericFormatOption(string name, AxisNumericFormatKind kind)
            {
                Name = name;
                Kind = kind;
            }

            public string Name { get; }

            public AxisNumericFormatKind Kind { get; }
        }

        private sealed class DateTimeFormatOption
        {
            public DateTimeFormatOption(string name, DateTimeFormats format)
            {
                Name = name;
                Format = format;
            }

            public string Name { get; }

            public DateTimeFormats Format { get; }
        }
    }
}
