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
        private static readonly Size CompactDialogSize = new Size(420, 250);
        private static readonly Size ExpandedDialogSize = new Size(420, 285);
        private static readonly Point NumericFormatLabelDefaultLocation = new Point(20, 147);
        private static readonly Point NumericFormatComboDefaultLocation = new Point(140, 143);
        private static readonly Point DateTimeFormatLabelLocation = new Point(20, 147);
        private static readonly Point DateTimeFormatComboLocation = new Point(140, 143);
        private static readonly Point NumericFormatLabelExpandedLocation = new Point(20, 180);
        private static readonly Point NumericFormatComboExpandedLocation = new Point(140, 176);
        private static readonly Point PrecisionLabelDefaultLocation = new Point(20, 180);
        private static readonly Point PrecisionTextDefaultLocation = new Point(140, 176);
        private static readonly Point PrecisionLabelExpandedLocation = new Point(20, 213);
        private static readonly Point PrecisionTextExpandedLocation = new Point(140, 209);
        private static readonly Point ButtonOkDefaultLocation = new Point(252, 214);
        private static readonly Point ButtonCancelDefaultLocation = new Point(333, 214);
        private static readonly Point ButtonOkExpandedLocation = new Point(252, 247);
        private static readonly Point ButtonCancelExpandedLocation = new Point(333, 247);

        private readonly AxisUnitAndNumericFormatPresentationModel presentationModel;
        private bool isBinding;
        private bool isPrecisionModeActive;

        private readonly ComboBox comboUnit;
        private readonly ComboBox comboFormat;
        private readonly ComboBox comboDateTimeFormat;
        private readonly TextBox textPrecision;
        private readonly TextBox textPreview;
        private readonly Label labelFormat;
        private readonly Label labelDateTimeFormat;
        private readonly Label labelPrecision;
        private readonly Label labelPreviewUnit;
        private readonly Label labelPreviewBase;
        private readonly Button buttonOk;
        private readonly Button buttonCancel;
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
            ClientSize = CompactDialogSize;

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
                Location = NumericFormatLabelDefaultLocation
            };

            comboFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = NumericFormatComboDefaultLocation,
                Size = new Size(268, 23),
                DisplayMember = "Name",
                ValueMember = "Kind"
            };

            labelDateTimeFormat = new Label
            {
                Text = "Date/Time Format:",
                AutoSize = true,
                Location = DateTimeFormatLabelLocation,
                Visible = false
            };

            comboDateTimeFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = DateTimeFormatComboLocation,
                Size = new Size(268, 23),
                DisplayMember = "Name",
                ValueMember = "Format",
                Visible = false
            };

            labelPrecision = new Label
            {
                Text = "Display Precision:",
                AutoSize = true,
                Location = PrecisionLabelDefaultLocation
            };

            textPrecision = new TextBox
            {
                Location = PrecisionTextDefaultLocation,
                Size = new Size(80, 23)
            };

            buttonOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = ButtonOkDefaultLocation,
                Size = new Size(75, 28)
            };

            buttonCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = ButtonCancelDefaultLocation,
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
            Controls.Add(labelDateTimeFormat);
            Controls.Add(comboDateTimeFormat);
            Controls.Add(labelPrecision);
            Controls.Add(textPrecision);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;

            comboUnit.SelectedIndexChanged += comboUnit_SelectedIndexChanged;
            comboFormat.SelectedIndexChanged += comboFormat_SelectedIndexChanged;
            comboDateTimeFormat.SelectedIndexChanged += comboDateTimeFormat_SelectedIndexChanged;
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
            ConfigureLayoutForMode(isDateTimeMode: false, showNumericControls: true);
            BindNumericControlsFromModel();
            labelPrecision.Visible = true;
            textPrecision.Visible = true;
            isPrecisionModeActive = true;
        }

        private void BindDateTimeMode()
        {
            var formats = presentationModel.AvailableDateTimeFormats
                .Select(format => new DateTimeFormatOption(presentationModel.GetDateTimeFormatDisplayName(format), format))
                .ToList();

            comboDateTimeFormat.DataSource = formats;

            var selectedFormatIndex = formats.FindIndex(option => option.Format == presentationModel.SelectedDateTimeFormat);
            comboDateTimeFormat.SelectedIndex = selectedFormatIndex >= 0 ? selectedFormatIndex : 0;

            UpdateDateTimeNumericControlVisibility();
        }

        private void UpdateDateTimeNumericControlVisibility()
        {
            var showNumericControls = presentationModel.ShouldShowNumericFormattingControls;
            ConfigureLayoutForMode(isDateTimeMode: true, showNumericControls: showNumericControls);
            isPrecisionModeActive = showNumericControls;

            if (showNumericControls)
            {
                BindNumericControlsFromModel();
            }

            if (!showNumericControls)
            {
                precisionErrorProvider.SetError(textPrecision, string.Empty);
                buttonOk.Enabled = true;
            }
        }

        private void BindNumericControlsFromModel()
        {
            var previousBinding = isBinding;
            isBinding = true;

            var formats = new List<NumericFormatOption>
            {
                new NumericFormatOption("Scientific", AxisNumericFormatKind.Scientific),
                new NumericFormatOption("Fixed Point", AxisNumericFormatKind.FixedPoint),
                new NumericFormatOption("General", AxisNumericFormatKind.General),
                new NumericFormatOption("Number", AxisNumericFormatKind.Number)
            };

            labelFormat.Text = "Format:";
            comboFormat.DataSource = formats;

            var selectedFormatIndex = formats.FindIndex(option => option.Kind == presentationModel.SelectedFormatKind);
            comboFormat.SelectedIndex = selectedFormatIndex >= 0 ? selectedFormatIndex : 0;
            textPrecision.Text = presentationModel.DisplayPrecision.ToString(CultureInfo.InvariantCulture);

            isBinding = previousBinding;
        }

        private void ConfigureLayoutForMode(bool isDateTimeMode, bool showNumericControls)
        {
            labelDateTimeFormat.Visible = isDateTimeMode;
            comboDateTimeFormat.Visible = isDateTimeMode;

            labelFormat.Text = "Format:";
            labelFormat.Visible = showNumericControls;
            comboFormat.Visible = showNumericControls;
            labelPrecision.Visible = showNumericControls;
            textPrecision.Visible = showNumericControls;

            if (isDateTimeMode && showNumericControls)
            {
                ClientSize = ExpandedDialogSize;
                labelFormat.Location = NumericFormatLabelExpandedLocation;
                comboFormat.Location = NumericFormatComboExpandedLocation;
                labelPrecision.Location = PrecisionLabelExpandedLocation;
                textPrecision.Location = PrecisionTextExpandedLocation;
                buttonOk.Location = ButtonOkExpandedLocation;
                buttonCancel.Location = ButtonCancelExpandedLocation;
                return;
            }

            ClientSize = CompactDialogSize;
            labelFormat.Location = NumericFormatLabelDefaultLocation;
            comboFormat.Location = NumericFormatComboDefaultLocation;
            labelPrecision.Location = PrecisionLabelDefaultLocation;
            textPrecision.Location = PrecisionTextDefaultLocation;
            buttonOk.Location = ButtonOkDefaultLocation;
            buttonCancel.Location = ButtonCancelDefaultLocation;
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

            if (presentationModel.IsDateTimeMode && !presentationModel.ShouldShowNumericFormattingControls)
            {
                return;
            }

            if (comboFormat.SelectedValue is AxisNumericFormatKind selectedKind)
            {
                presentationModel.SetFormatKind(selectedKind);
                RefreshPreview();
            }
        }

        private void comboDateTimeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isBinding)
            {
                return;
            }

            if (!(comboDateTimeFormat.SelectedValue is DateTimeFormats selectedDateTimeFormat))
            {
                return;
            }

            presentationModel.SetDateTimeFormat(selectedDateTimeFormat);
            UpdateDateTimeNumericControlVisibility();
            ValidateAndUpdatePrecision();
            RefreshPreview();
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
            if (!presentationModel.ShouldShowNumericFormattingControls)
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

        internal ComboBox DateTimeFormatComboForTesting => comboDateTimeFormat;

        internal int DateTimeFormatCountForTesting => comboDateTimeFormat.Items.Count;

        internal string PrecisionTextForTesting => textPrecision.Text;

        internal int NumericFormatCountForTesting => comboFormat.Items.Count;

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
