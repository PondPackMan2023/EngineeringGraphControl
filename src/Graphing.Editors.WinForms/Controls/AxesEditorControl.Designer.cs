namespace Graphing.Editors.Controls
{
    partial class AxesEditorControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._axesListBox = new System.Windows.Forms.ListBox();
            this._rightPanel = new System.Windows.Forms.Panel();
            this._incrementUnitLabel = new System.Windows.Forms.Label();
            this._incrementTextBox = new System.Windows.Forms.TextBox();
            this._fixedIncrementRadioButton = new System.Windows.Forms.RadioButton();
            this._autoIncrementRadioButton = new System.Windows.Forms.RadioButton();
            this._incrementLabel = new System.Windows.Forms.Label();
            this._maximumUnitLabel = new System.Windows.Forms.Label();
            this._minimumUnitLabel = new System.Windows.Forms.Label();
            this._maximumTextBox = new System.Windows.Forms.TextBox();
            this._minimumTextBox = new System.Windows.Forms.TextBox();
            this._fixedRangeRadioButton = new System.Windows.Forms.RadioButton();
            this._autoRangeRadioButton = new System.Windows.Forms.RadioButton();
            this._titleTextBox = new System.Windows.Forms.TextBox();
            this._hasTitleOverrideCheckBox = new System.Windows.Forms.CheckBox();
            this._isVisibleCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBoxRange = new System.Windows.Forms.GroupBox();
            this.groupBoxIncrement = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._rightPanel.SuspendLayout();
            this.groupBoxRange.SuspendLayout();
            this.groupBoxIncrement.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location = new System.Drawing.Point(0, 0);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._axesListBox);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            this._splitContainer.Size = new System.Drawing.Size(468, 300);
            this._splitContainer.SplitterDistance = 188;
            this._splitContainer.SplitterWidth = 3;
            this._splitContainer.TabIndex = 0;
            // 
            // _axesListBox
            // 
            this._axesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._axesListBox.FormattingEnabled = true;
            this._axesListBox.Location = new System.Drawing.Point(0, 0);
            this._axesListBox.Name = "_axesListBox";
            this._axesListBox.Size = new System.Drawing.Size(188, 300);
            this._axesListBox.TabIndex = 0;
            // 
            // _rightPanel
            // 
            this._rightPanel.Controls.Add(this.groupBoxIncrement);
            this._rightPanel.Controls.Add(this.groupBoxRange);
            this._rightPanel.Controls.Add(this._titleTextBox);
            this._rightPanel.Controls.Add(this._hasTitleOverrideCheckBox);
            this._rightPanel.Controls.Add(this._isVisibleCheckBox);
            this._rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightPanel.Location = new System.Drawing.Point(0, 0);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Size = new System.Drawing.Size(277, 300);
            this._rightPanel.TabIndex = 0;
            // 
            // _incrementUnitLabel
            // 
            this._incrementUnitLabel.AutoSize = true;
            this._incrementUnitLabel.Location = new System.Drawing.Point(161, 55);
            this._incrementUnitLabel.Name = "_incrementUnitLabel";
            this._incrementUnitLabel.Size = new System.Drawing.Size(0, 13);
            this._incrementUnitLabel.TabIndex = 4;
            // 
            // _incrementTextBox
            // 
            this._incrementTextBox.Location = new System.Drawing.Point(66, 50);
            this._incrementTextBox.Name = "_incrementTextBox";
            this._incrementTextBox.Size = new System.Drawing.Size(89, 20);
            this._incrementTextBox.TabIndex = 3;
            // 
            // _fixedIncrementRadioButton
            // 
            this._fixedIncrementRadioButton.AutoSize = true;
            this._fixedIncrementRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._fixedIncrementRadioButton.Location = new System.Drawing.Point(65, 19);
            this._fixedIncrementRadioButton.Name = "_fixedIncrementRadioButton";
            this._fixedIncrementRadioButton.Size = new System.Drawing.Size(56, 18);
            this._fixedIncrementRadioButton.TabIndex = 1;
            this._fixedIncrementRadioButton.TabStop = true;
            this._fixedIncrementRadioButton.Text = "Fixed";
            this._fixedIncrementRadioButton.UseVisualStyleBackColor = true;
            // 
            // _autoIncrementRadioButton
            // 
            this._autoIncrementRadioButton.AutoSize = true;
            this._autoIncrementRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._autoIncrementRadioButton.Location = new System.Drawing.Point(6, 19);
            this._autoIncrementRadioButton.Name = "_autoIncrementRadioButton";
            this._autoIncrementRadioButton.Size = new System.Drawing.Size(53, 18);
            this._autoIncrementRadioButton.TabIndex = 0;
            this._autoIncrementRadioButton.TabStop = true;
            this._autoIncrementRadioButton.Text = "Auto";
            this._autoIncrementRadioButton.UseVisualStyleBackColor = true;
            // 
            // _incrementLabel
            // 
            this._incrementLabel.AutoSize = true;
            this._incrementLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._incrementLabel.Location = new System.Drawing.Point(3, 53);
            this._incrementLabel.Name = "_incrementLabel";
            this._incrementLabel.Size = new System.Drawing.Size(54, 13);
            this._incrementLabel.TabIndex = 2;
            this._incrementLabel.Text = "Increment";
            // 
            // _maximumUnitLabel
            // 
            this._maximumUnitLabel.AutoSize = true;
            this._maximumUnitLabel.Location = new System.Drawing.Point(101, 78);
            this._maximumUnitLabel.Name = "_maximumUnitLabel";
            this._maximumUnitLabel.Size = new System.Drawing.Size(0, 13);
            this._maximumUnitLabel.TabIndex = 5;
            // 
            // _minimumUnitLabel
            // 
            this._minimumUnitLabel.AutoSize = true;
            this._minimumUnitLabel.Location = new System.Drawing.Point(101, 52);
            this._minimumUnitLabel.Name = "_minimumUnitLabel";
            this._minimumUnitLabel.Size = new System.Drawing.Size(0, 13);
            this._minimumUnitLabel.TabIndex = 3;
            // 
            // _maximumTextBox
            // 
            this._maximumTextBox.Location = new System.Drawing.Point(6, 75);
            this._maximumTextBox.Name = "_maximumTextBox";
            this._maximumTextBox.Size = new System.Drawing.Size(89, 20);
            this._maximumTextBox.TabIndex = 4;
            // 
            // _minimumTextBox
            // 
            this._minimumTextBox.Location = new System.Drawing.Point(6, 49);
            this._minimumTextBox.Name = "_minimumTextBox";
            this._minimumTextBox.Size = new System.Drawing.Size(89, 20);
            this._minimumTextBox.TabIndex = 2;
            // 
            // _fixedRangeRadioButton
            // 
            this._fixedRangeRadioButton.AutoSize = true;
            this._fixedRangeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._fixedRangeRadioButton.Location = new System.Drawing.Point(65, 19);
            this._fixedRangeRadioButton.Name = "_fixedRangeRadioButton";
            this._fixedRangeRadioButton.Size = new System.Drawing.Size(56, 18);
            this._fixedRangeRadioButton.TabIndex = 1;
            this._fixedRangeRadioButton.TabStop = true;
            this._fixedRangeRadioButton.Text = "Fixed";
            this._fixedRangeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _autoRangeRadioButton
            // 
            this._autoRangeRadioButton.AutoSize = true;
            this._autoRangeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._autoRangeRadioButton.Location = new System.Drawing.Point(6, 19);
            this._autoRangeRadioButton.Name = "_autoRangeRadioButton";
            this._autoRangeRadioButton.Size = new System.Drawing.Size(53, 18);
            this._autoRangeRadioButton.TabIndex = 0;
            this._autoRangeRadioButton.TabStop = true;
            this._autoRangeRadioButton.Text = "Auto";
            this._autoRangeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _titleTextBox
            // 
            this._titleTextBox.Location = new System.Drawing.Point(10, 68);
            this._titleTextBox.Name = "_titleTextBox";
            this._titleTextBox.Size = new System.Drawing.Size(253, 20);
            this._titleTextBox.TabIndex = 2;
            // 
            // _hasTitleOverrideCheckBox
            // 
            this._hasTitleOverrideCheckBox.AutoSize = true;
            this._hasTitleOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._hasTitleOverrideCheckBox.Location = new System.Drawing.Point(10, 46);
            this._hasTitleOverrideCheckBox.Name = "_hasTitleOverrideCheckBox";
            this._hasTitleOverrideCheckBox.Size = new System.Drawing.Size(91, 18);
            this._hasTitleOverrideCheckBox.TabIndex = 1;
            this._hasTitleOverrideCheckBox.Text = "Override title";
            this._hasTitleOverrideCheckBox.UseVisualStyleBackColor = true;
            // 
            // _isVisibleCheckBox
            // 
            this._isVisibleCheckBox.AutoSize = true;
            this._isVisibleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._isVisibleCheckBox.Location = new System.Drawing.Point(10, 17);
            this._isVisibleCheckBox.Name = "_isVisibleCheckBox";
            this._isVisibleCheckBox.Size = new System.Drawing.Size(62, 18);
            this._isVisibleCheckBox.TabIndex = 0;
            this._isVisibleCheckBox.Text = "Visible";
            this._isVisibleCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBoxRange
            // 
            this.groupBoxRange.Controls.Add(this._maximumUnitLabel);
            this.groupBoxRange.Controls.Add(this._minimumUnitLabel);
            this.groupBoxRange.Controls.Add(this._minimumTextBox);
            this.groupBoxRange.Controls.Add(this._maximumTextBox);
            this.groupBoxRange.Controls.Add(this._fixedRangeRadioButton);
            this.groupBoxRange.Controls.Add(this._autoRangeRadioButton);
            this.groupBoxRange.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxRange.Location = new System.Drawing.Point(10, 94);
            this.groupBoxRange.Name = "groupBoxRange";
            this.groupBoxRange.Size = new System.Drawing.Size(219, 101);
            this.groupBoxRange.TabIndex = 3;
            this.groupBoxRange.TabStop = false;
            this.groupBoxRange.Text = "Range";
            // 
            // groupBoxIncrement
            // 
            this.groupBoxIncrement.Controls.Add(this._incrementTextBox);
            this.groupBoxIncrement.Controls.Add(this._incrementLabel);
            this.groupBoxIncrement.Controls.Add(this._incrementUnitLabel);
            this.groupBoxIncrement.Controls.Add(this._fixedIncrementRadioButton);
            this.groupBoxIncrement.Controls.Add(this._autoIncrementRadioButton);
            this.groupBoxIncrement.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxIncrement.Location = new System.Drawing.Point(10, 201);
            this.groupBoxIncrement.Name = "groupBoxIncrement";
            this.groupBoxIncrement.Size = new System.Drawing.Size(219, 76);
            this.groupBoxIncrement.TabIndex = 4;
            this.groupBoxIncrement.TabStop = false;
            this.groupBoxIncrement.Text = "Increment";
            // 
            // AxesEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Name = "AxesEditorControl";
            this.Size = new System.Drawing.Size(468, 300);
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this._rightPanel.PerformLayout();
            this.groupBoxRange.ResumeLayout(false);
            this.groupBoxRange.PerformLayout();
            this.groupBoxIncrement.ResumeLayout(false);
            this.groupBoxIncrement.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.ListBox _axesListBox;
        private System.Windows.Forms.Panel _rightPanel;
        private System.Windows.Forms.CheckBox _isVisibleCheckBox;
        private System.Windows.Forms.CheckBox _hasTitleOverrideCheckBox;
        private System.Windows.Forms.TextBox _titleTextBox;
        private System.Windows.Forms.RadioButton _autoRangeRadioButton;
        private System.Windows.Forms.RadioButton _fixedRangeRadioButton;
        private System.Windows.Forms.TextBox _minimumTextBox;
        private System.Windows.Forms.TextBox _maximumTextBox;
        private System.Windows.Forms.Label _minimumUnitLabel;
        private System.Windows.Forms.Label _maximumUnitLabel;
        private System.Windows.Forms.Label _incrementLabel;
        private System.Windows.Forms.RadioButton _autoIncrementRadioButton;
        private System.Windows.Forms.RadioButton _fixedIncrementRadioButton;
        private System.Windows.Forms.TextBox _incrementTextBox;
        private System.Windows.Forms.Label _incrementUnitLabel;
        private System.Windows.Forms.GroupBox groupBoxIncrement;
        private System.Windows.Forms.GroupBox groupBoxRange;
    }
}
