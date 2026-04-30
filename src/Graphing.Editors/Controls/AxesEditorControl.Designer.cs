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
            this.components = new System.ComponentModel.Container();
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
            this._rangeLabel = new System.Windows.Forms.Label();
            this._titleTextBox = new System.Windows.Forms.TextBox();
            this._hasTitleOverrideCheckBox = new System.Windows.Forms.CheckBox();
            this._isVisibleCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._rightPanel.SuspendLayout();
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
            this._splitContainer.Size = new System.Drawing.Size(546, 346);
            this._splitContainer.SplitterDistance = 220;
            this._splitContainer.TabIndex = 0;
            // 
            // _axesListBox
            // 
            this._axesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._axesListBox.FormattingEnabled = true;
            this._axesListBox.ItemHeight = 15;
            this._axesListBox.Location = new System.Drawing.Point(0, 0);
            this._axesListBox.Name = "_axesListBox";
            this._axesListBox.Size = new System.Drawing.Size(220, 346);
            this._axesListBox.TabIndex = 0;
            // 
            // _rightPanel
            // 
            this._rightPanel.Controls.Add(this._incrementUnitLabel);
            this._rightPanel.Controls.Add(this._incrementTextBox);
            this._rightPanel.Controls.Add(this._fixedIncrementRadioButton);
            this._rightPanel.Controls.Add(this._autoIncrementRadioButton);
            this._rightPanel.Controls.Add(this._incrementLabel);
            this._rightPanel.Controls.Add(this._maximumUnitLabel);
            this._rightPanel.Controls.Add(this._minimumUnitLabel);
            this._rightPanel.Controls.Add(this._maximumTextBox);
            this._rightPanel.Controls.Add(this._minimumTextBox);
            this._rightPanel.Controls.Add(this._fixedRangeRadioButton);
            this._rightPanel.Controls.Add(this._autoRangeRadioButton);
            this._rightPanel.Controls.Add(this._rangeLabel);
            this._rightPanel.Controls.Add(this._titleTextBox);
            this._rightPanel.Controls.Add(this._hasTitleOverrideCheckBox);
            this._rightPanel.Controls.Add(this._isVisibleCheckBox);
            this._rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightPanel.Location = new System.Drawing.Point(0, 0);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Size = new System.Drawing.Size(322, 346);
            this._rightPanel.TabIndex = 0;
            // 
            // _incrementUnitLabel
            // 
            this._incrementUnitLabel.AutoSize = true;
            this._incrementUnitLabel.Location = new System.Drawing.Point(238, 268);
            this._incrementUnitLabel.Name = "_incrementUnitLabel";
            this._incrementUnitLabel.Size = new System.Drawing.Size(0, 15);
            this._incrementUnitLabel.TabIndex = 14;
            // 
            // _incrementTextBox
            // 
            this._incrementTextBox.Location = new System.Drawing.Point(129, 265);
            this._incrementTextBox.Name = "_incrementTextBox";
            this._incrementTextBox.Size = new System.Drawing.Size(103, 23);
            this._incrementTextBox.TabIndex = 13;
            // 
            // _fixedIncrementRadioButton
            // 
            this._fixedIncrementRadioButton.AutoSize = true;
            this._fixedIncrementRadioButton.Location = new System.Drawing.Point(71, 239);
            this._fixedIncrementRadioButton.Name = "_fixedIncrementRadioButton";
            this._fixedIncrementRadioButton.Size = new System.Drawing.Size(52, 19);
            this._fixedIncrementRadioButton.TabIndex = 12;
            this._fixedIncrementRadioButton.TabStop = true;
            this._fixedIncrementRadioButton.Text = "Fixed";
            this._fixedIncrementRadioButton.UseVisualStyleBackColor = true;
            // 
            // _autoIncrementRadioButton
            // 
            this._autoIncrementRadioButton.AutoSize = true;
            this._autoIncrementRadioButton.Location = new System.Drawing.Point(12, 239);
            this._autoIncrementRadioButton.Name = "_autoIncrementRadioButton";
            this._autoIncrementRadioButton.Size = new System.Drawing.Size(52, 19);
            this._autoIncrementRadioButton.TabIndex = 11;
            this._autoIncrementRadioButton.TabStop = true;
            this._autoIncrementRadioButton.Text = "Auto";
            this._autoIncrementRadioButton.UseVisualStyleBackColor = true;
            // 
            // _incrementLabel
            // 
            this._incrementLabel.AutoSize = true;
            this._incrementLabel.Location = new System.Drawing.Point(12, 268);
            this._incrementLabel.Name = "_incrementLabel";
            this._incrementLabel.Size = new System.Drawing.Size(62, 15);
            this._incrementLabel.TabIndex = 10;
            this._incrementLabel.Text = "Increment";
            // 
            // _maximumUnitLabel
            // 
            this._maximumUnitLabel.AutoSize = true;
            this._maximumUnitLabel.Location = new System.Drawing.Point(238, 198);
            this._maximumUnitLabel.Name = "_maximumUnitLabel";
            this._maximumUnitLabel.Size = new System.Drawing.Size(0, 15);
            this._maximumUnitLabel.TabIndex = 9;
            // 
            // _minimumUnitLabel
            // 
            this._minimumUnitLabel.AutoSize = true;
            this._minimumUnitLabel.Location = new System.Drawing.Point(238, 169);
            this._minimumUnitLabel.Name = "_minimumUnitLabel";
            this._minimumUnitLabel.Size = new System.Drawing.Size(0, 15);
            this._minimumUnitLabel.TabIndex = 8;
            // 
            // _maximumTextBox
            // 
            this._maximumTextBox.Location = new System.Drawing.Point(129, 195);
            this._maximumTextBox.Name = "_maximumTextBox";
            this._maximumTextBox.Size = new System.Drawing.Size(103, 23);
            this._maximumTextBox.TabIndex = 7;
            // 
            // _minimumTextBox
            // 
            this._minimumTextBox.Location = new System.Drawing.Point(129, 166);
            this._minimumTextBox.Name = "_minimumTextBox";
            this._minimumTextBox.Size = new System.Drawing.Size(103, 23);
            this._minimumTextBox.TabIndex = 6;
            // 
            // _fixedRangeRadioButton
            // 
            this._fixedRangeRadioButton.AutoSize = true;
            this._fixedRangeRadioButton.Location = new System.Drawing.Point(71, 140);
            this._fixedRangeRadioButton.Name = "_fixedRangeRadioButton";
            this._fixedRangeRadioButton.Size = new System.Drawing.Size(52, 19);
            this._fixedRangeRadioButton.TabIndex = 5;
            this._fixedRangeRadioButton.TabStop = true;
            this._fixedRangeRadioButton.Text = "Fixed";
            this._fixedRangeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _autoRangeRadioButton
            // 
            this._autoRangeRadioButton.AutoSize = true;
            this._autoRangeRadioButton.Location = new System.Drawing.Point(12, 140);
            this._autoRangeRadioButton.Name = "_autoRangeRadioButton";
            this._autoRangeRadioButton.Size = new System.Drawing.Size(52, 19);
            this._autoRangeRadioButton.TabIndex = 4;
            this._autoRangeRadioButton.TabStop = true;
            this._autoRangeRadioButton.Text = "Auto";
            this._autoRangeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _rangeLabel
            // 
            this._rangeLabel.AutoSize = true;
            this._rangeLabel.Location = new System.Drawing.Point(12, 114);
            this._rangeLabel.Name = "_rangeLabel";
            this._rangeLabel.Size = new System.Drawing.Size(39, 15);
            this._rangeLabel.TabIndex = 3;
            this._rangeLabel.Text = "Range";
            // 
            // _titleTextBox
            // 
            this._titleTextBox.Location = new System.Drawing.Point(12, 78);
            this._titleTextBox.Name = "_titleTextBox";
            this._titleTextBox.Size = new System.Drawing.Size(295, 23);
            this._titleTextBox.TabIndex = 2;
            // 
            // _hasTitleOverrideCheckBox
            // 
            this._hasTitleOverrideCheckBox.AutoSize = true;
            this._hasTitleOverrideCheckBox.Location = new System.Drawing.Point(12, 53);
            this._hasTitleOverrideCheckBox.Name = "_hasTitleOverrideCheckBox";
            this._hasTitleOverrideCheckBox.Size = new System.Drawing.Size(98, 19);
            this._hasTitleOverrideCheckBox.TabIndex = 1;
            this._hasTitleOverrideCheckBox.Text = "Override title";
            this._hasTitleOverrideCheckBox.UseVisualStyleBackColor = true;
            // 
            // _isVisibleCheckBox
            // 
            this._isVisibleCheckBox.AutoSize = true;
            this._isVisibleCheckBox.Location = new System.Drawing.Point(12, 20);
            this._isVisibleCheckBox.Name = "_isVisibleCheckBox";
            this._isVisibleCheckBox.Size = new System.Drawing.Size(68, 19);
            this._isVisibleCheckBox.TabIndex = 0;
            this._isVisibleCheckBox.Text = "Visible";
            this._isVisibleCheckBox.UseVisualStyleBackColor = true;
            // 
            // AxesEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Name = "AxesEditorControl";
            this.Size = new System.Drawing.Size(546, 346);
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this._rightPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.ListBox _axesListBox;
        private System.Windows.Forms.Panel _rightPanel;
        private System.Windows.Forms.CheckBox _isVisibleCheckBox;
        private System.Windows.Forms.CheckBox _hasTitleOverrideCheckBox;
        private System.Windows.Forms.TextBox _titleTextBox;
        private System.Windows.Forms.Label _rangeLabel;
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
    }
}
