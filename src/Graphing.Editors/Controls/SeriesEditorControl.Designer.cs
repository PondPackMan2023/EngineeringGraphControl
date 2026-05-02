namespace Graphing.Editors.Controls
{
    partial class SeriesEditorControl
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
            this._seriesListBox = new System.Windows.Forms.ListBox();
            this._orderingPanel = new System.Windows.Forms.Panel();
            this._moveDownButton = new System.Windows.Forms.Button();
            this._moveUpButton = new System.Windows.Forms.Button();
            this._rightPanel = new System.Windows.Forms.Panel();
            this._colorSwatchPanel = new System.Windows.Forms.Panel();
            this._pickColorButton = new System.Windows.Forms.Button();
            this._labelTextBox = new System.Windows.Forms.TextBox();
            this._hasLabelOverrideCheckBox = new System.Windows.Forms.CheckBox();
            this._isVisibleCheckBox = new System.Windows.Forms.CheckBox();
            this._colorDialog = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._orderingPanel.SuspendLayout();
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
            this._splitContainer.Panel1.Controls.Add(this._seriesListBox);
            this._splitContainer.Panel1.Controls.Add(this._orderingPanel);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            this._splitContainer.Size = new System.Drawing.Size(468, 300);
            this._splitContainer.SplitterDistance = 188;
            this._splitContainer.SplitterWidth = 3;
            this._splitContainer.TabIndex = 0;
            // 
            // _seriesListBox
            // 
            this._seriesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._seriesListBox.FormattingEnabled = true;
            this._seriesListBox.HorizontalScrollbar = true;
            this._seriesListBox.Location = new System.Drawing.Point(0, 0);
            this._seriesListBox.Name = "_seriesListBox";
            this._seriesListBox.Size = new System.Drawing.Size(188, 270);
            this._seriesListBox.TabIndex = 0;
            // 
            // _orderingPanel
            // 
            this._orderingPanel.Controls.Add(this._moveDownButton);
            this._orderingPanel.Controls.Add(this._moveUpButton);
            this._orderingPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._orderingPanel.Location = new System.Drawing.Point(0, 270);
            this._orderingPanel.Name = "_orderingPanel";
            this._orderingPanel.Size = new System.Drawing.Size(188, 30);
            this._orderingPanel.TabIndex = 1;
            // 
            // _moveDownButton
            // 
            this._moveDownButton.Location = new System.Drawing.Point(42, 5);
            this._moveDownButton.Name = "_moveDownButton";
            this._moveDownButton.Size = new System.Drawing.Size(34, 20);
            this._moveDownButton.TabIndex = 1;
            this._moveDownButton.Text = "-";
            this._moveDownButton.UseVisualStyleBackColor = true;
            // 
            // _moveUpButton
            // 
            this._moveUpButton.Location = new System.Drawing.Point(3, 5);
            this._moveUpButton.Name = "_moveUpButton";
            this._moveUpButton.Size = new System.Drawing.Size(34, 20);
            this._moveUpButton.TabIndex = 0;
            this._moveUpButton.Text = "+";
            this._moveUpButton.UseVisualStyleBackColor = true;
            // 
            // _rightPanel
            // 
            this._rightPanel.Controls.Add(this._colorSwatchPanel);
            this._rightPanel.Controls.Add(this._pickColorButton);
            this._rightPanel.Controls.Add(this._labelTextBox);
            this._rightPanel.Controls.Add(this._hasLabelOverrideCheckBox);
            this._rightPanel.Controls.Add(this._isVisibleCheckBox);
            this._rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightPanel.Location = new System.Drawing.Point(0, 0);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Size = new System.Drawing.Size(277, 300);
            this._rightPanel.TabIndex = 0;
            // 
            // _colorSwatchPanel
            // 
            this._colorSwatchPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._colorSwatchPanel.Location = new System.Drawing.Point(102, 97);
            this._colorSwatchPanel.Name = "_colorSwatchPanel";
            this._colorSwatchPanel.Size = new System.Drawing.Size(35, 25);
            this._colorSwatchPanel.TabIndex = 4;
            // 
            // _pickColorButton
            // 
            this._pickColorButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._pickColorButton.Location = new System.Drawing.Point(10, 97);
            this._pickColorButton.Name = "_pickColorButton";
            this._pickColorButton.Size = new System.Drawing.Size(86, 25);
            this._pickColorButton.TabIndex = 3;
            this._pickColorButton.Text = "Choose color...";
            this._pickColorButton.UseVisualStyleBackColor = true;
            // 
            // _labelTextBox
            // 
            this._labelTextBox.Location = new System.Drawing.Point(10, 63);
            this._labelTextBox.Name = "_labelTextBox";
            this._labelTextBox.Size = new System.Drawing.Size(253, 20);
            this._labelTextBox.TabIndex = 2;
            // 
            // _hasLabelOverrideCheckBox
            // 
            this._hasLabelOverrideCheckBox.AutoSize = true;
            this._hasLabelOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._hasLabelOverrideCheckBox.Location = new System.Drawing.Point(10, 42);
            this._hasLabelOverrideCheckBox.Name = "_hasLabelOverrideCheckBox";
            this._hasLabelOverrideCheckBox.Size = new System.Drawing.Size(97, 18);
            this._hasLabelOverrideCheckBox.TabIndex = 1;
            this._hasLabelOverrideCheckBox.Text = "Override label";
            this._hasLabelOverrideCheckBox.UseVisualStyleBackColor = true;
            // 
            // _isVisibleCheckBox
            // 
            this._isVisibleCheckBox.AutoSize = true;
            this._isVisibleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._isVisibleCheckBox.Location = new System.Drawing.Point(10, 12);
            this._isVisibleCheckBox.Name = "_isVisibleCheckBox";
            this._isVisibleCheckBox.Size = new System.Drawing.Size(62, 18);
            this._isVisibleCheckBox.TabIndex = 0;
            this._isVisibleCheckBox.Text = "Visible";
            this._isVisibleCheckBox.UseVisualStyleBackColor = true;
            // 
            // SeriesEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Name = "SeriesEditorControl";
            this.Size = new System.Drawing.Size(468, 300);
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._orderingPanel.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this._rightPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.ListBox _seriesListBox;
        private System.Windows.Forms.Panel _orderingPanel;
        private System.Windows.Forms.Button _moveUpButton;
        private System.Windows.Forms.Button _moveDownButton;
        private System.Windows.Forms.Panel _rightPanel;
        private System.Windows.Forms.CheckBox _isVisibleCheckBox;
        private System.Windows.Forms.CheckBox _hasLabelOverrideCheckBox;
        private System.Windows.Forms.TextBox _labelTextBox;
        private System.Windows.Forms.Button _pickColorButton;
        private System.Windows.Forms.Panel _colorSwatchPanel;
        private System.Windows.Forms.ColorDialog _colorDialog;
    }
}
