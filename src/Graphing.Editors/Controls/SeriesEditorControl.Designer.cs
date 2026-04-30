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
            this.components = new System.ComponentModel.Container();
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._seriesListBox = new System.Windows.Forms.ListBox();
            this._orderingPanel = new System.Windows.Forms.Panel();
            this._moveUpButton = new System.Windows.Forms.Button();
            this._moveDownButton = new System.Windows.Forms.Button();
            this._rightPanel = new System.Windows.Forms.Panel();
            this._isVisibleCheckBox = new System.Windows.Forms.CheckBox();
            this._hasLabelOverrideCheckBox = new System.Windows.Forms.CheckBox();
            this._labelTextBox = new System.Windows.Forms.TextBox();
            this._hasColorOverrideCheckBox = new System.Windows.Forms.CheckBox();
            this._pickColorButton = new System.Windows.Forms.Button();
            this._colorSwatchPanel = new System.Windows.Forms.Panel();
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
            this._splitContainer.Size = new System.Drawing.Size(546, 346);
            this._splitContainer.SplitterDistance = 220;
            this._splitContainer.TabIndex = 0;
            // 
            // _seriesListBox
            // 
            this._seriesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._seriesListBox.FormattingEnabled = true;
            this._seriesListBox.ItemHeight = 15;
            this._seriesListBox.Location = new System.Drawing.Point(0, 0);
            this._seriesListBox.Name = "_seriesListBox";
            this._seriesListBox.Size = new System.Drawing.Size(220, 311);
            this._seriesListBox.TabIndex = 0;
            // 
            // _orderingPanel
            // 
            this._orderingPanel.Controls.Add(this._moveDownButton);
            this._orderingPanel.Controls.Add(this._moveUpButton);
            this._orderingPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._orderingPanel.Location = new System.Drawing.Point(0, 311);
            this._orderingPanel.Name = "_orderingPanel";
            this._orderingPanel.Size = new System.Drawing.Size(220, 35);
            this._orderingPanel.TabIndex = 1;
            // 
            // _moveUpButton
            // 
            this._moveUpButton.Location = new System.Drawing.Point(3, 6);
            this._moveUpButton.Name = "_moveUpButton";
            this._moveUpButton.Size = new System.Drawing.Size(40, 23);
            this._moveUpButton.TabIndex = 0;
            this._moveUpButton.Text = "+";
            this._moveUpButton.UseVisualStyleBackColor = true;
            // 
            // _moveDownButton
            // 
            this._moveDownButton.Location = new System.Drawing.Point(49, 6);
            this._moveDownButton.Name = "_moveDownButton";
            this._moveDownButton.Size = new System.Drawing.Size(40, 23);
            this._moveDownButton.TabIndex = 1;
            this._moveDownButton.Text = "-";
            this._moveDownButton.UseVisualStyleBackColor = true;
            // 
            // _rightPanel
            // 
            this._rightPanel.Controls.Add(this._colorSwatchPanel);
            this._rightPanel.Controls.Add(this._pickColorButton);
            this._rightPanel.Controls.Add(this._hasColorOverrideCheckBox);
            this._rightPanel.Controls.Add(this._labelTextBox);
            this._rightPanel.Controls.Add(this._hasLabelOverrideCheckBox);
            this._rightPanel.Controls.Add(this._isVisibleCheckBox);
            this._rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightPanel.Location = new System.Drawing.Point(0, 0);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Size = new System.Drawing.Size(322, 346);
            this._rightPanel.TabIndex = 0;
            // 
            // _isVisibleCheckBox
            // 
            this._isVisibleCheckBox.AutoSize = true;
            this._isVisibleCheckBox.Location = new System.Drawing.Point(12, 14);
            this._isVisibleCheckBox.Name = "_isVisibleCheckBox";
            this._isVisibleCheckBox.Size = new System.Drawing.Size(68, 19);
            this._isVisibleCheckBox.TabIndex = 0;
            this._isVisibleCheckBox.Text = "Visible";
            this._isVisibleCheckBox.UseVisualStyleBackColor = true;
            // 
            // _hasLabelOverrideCheckBox
            // 
            this._hasLabelOverrideCheckBox.AutoSize = true;
            this._hasLabelOverrideCheckBox.Location = new System.Drawing.Point(12, 48);
            this._hasLabelOverrideCheckBox.Name = "_hasLabelOverrideCheckBox";
            this._hasLabelOverrideCheckBox.Size = new System.Drawing.Size(102, 19);
            this._hasLabelOverrideCheckBox.TabIndex = 1;
            this._hasLabelOverrideCheckBox.Text = "Override label";
            this._hasLabelOverrideCheckBox.UseVisualStyleBackColor = true;
            // 
            // _labelTextBox
            // 
            this._labelTextBox.Location = new System.Drawing.Point(12, 73);
            this._labelTextBox.Name = "_labelTextBox";
            this._labelTextBox.Size = new System.Drawing.Size(295, 23);
            this._labelTextBox.TabIndex = 2;
            // 
            // _hasColorOverrideCheckBox
            // 
            this._hasColorOverrideCheckBox.AutoSize = true;
            this._hasColorOverrideCheckBox.Location = new System.Drawing.Point(12, 112);
            this._hasColorOverrideCheckBox.Name = "_hasColorOverrideCheckBox";
            this._hasColorOverrideCheckBox.Size = new System.Drawing.Size(104, 19);
            this._hasColorOverrideCheckBox.TabIndex = 3;
            this._hasColorOverrideCheckBox.Text = "Override color";
            this._hasColorOverrideCheckBox.UseVisualStyleBackColor = true;
            // 
            // _pickColorButton
            // 
            this._pickColorButton.Location = new System.Drawing.Point(12, 137);
            this._pickColorButton.Name = "_pickColorButton";
            this._pickColorButton.Size = new System.Drawing.Size(100, 23);
            this._pickColorButton.TabIndex = 4;
            this._pickColorButton.Text = "Choose color...";
            this._pickColorButton.UseVisualStyleBackColor = true;
            // 
            // _colorSwatchPanel
            // 
            this._colorSwatchPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._colorSwatchPanel.Location = new System.Drawing.Point(118, 137);
            this._colorSwatchPanel.Name = "_colorSwatchPanel";
            this._colorSwatchPanel.Size = new System.Drawing.Size(40, 23);
            this._colorSwatchPanel.TabIndex = 5;
            // 
            // SeriesEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Name = "SeriesEditorControl";
            this.Size = new System.Drawing.Size(546, 346);
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
        private System.Windows.Forms.CheckBox _hasColorOverrideCheckBox;
        private System.Windows.Forms.Button _pickColorButton;
        private System.Windows.Forms.Panel _colorSwatchPanel;
        private System.Windows.Forms.ColorDialog _colorDialog;
    }
}
