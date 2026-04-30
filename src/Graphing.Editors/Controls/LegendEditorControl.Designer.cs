namespace Graphing.Editors.Controls
{
    partial class LegendEditorControl
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
            this._showLegendCheckBox = new System.Windows.Forms.CheckBox();
            this._placementLabel = new System.Windows.Forms.Label();
            this._topRadioButton = new System.Windows.Forms.RadioButton();
            this._bottomRadioButton = new System.Windows.Forms.RadioButton();
            this._leftRadioButton = new System.Windows.Forms.RadioButton();
            this._rightRadioButton = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // _showLegendCheckBox
            // 
            this._showLegendCheckBox.AutoSize = true;
            this._showLegendCheckBox.Location = new System.Drawing.Point(12, 12);
            this._showLegendCheckBox.Name = "_showLegendCheckBox";
            this._showLegendCheckBox.Size = new System.Drawing.Size(92, 19);
            this._showLegendCheckBox.TabIndex = 0;
            this._showLegendCheckBox.Text = "Show Legend";
            this._showLegendCheckBox.UseVisualStyleBackColor = true;
            // 
            // _placementLabel
            // 
            this._placementLabel.AutoSize = true;
            this._placementLabel.Location = new System.Drawing.Point(12, 42);
            this._placementLabel.Name = "_placementLabel";
            this._placementLabel.Size = new System.Drawing.Size(59, 15);
            this._placementLabel.TabIndex = 1;
            this._placementLabel.Text = "Placement";
            // 
            // _topRadioButton
            // 
            this._topRadioButton.AutoSize = true;
            this._topRadioButton.Location = new System.Drawing.Point(12, 64);
            this._topRadioButton.Name = "_topRadioButton";
            this._topRadioButton.Size = new System.Drawing.Size(45, 19);
            this._topRadioButton.TabIndex = 2;
            this._topRadioButton.TabStop = true;
            this._topRadioButton.Text = "Top";
            this._topRadioButton.UseVisualStyleBackColor = true;
            // 
            // _bottomRadioButton
            // 
            this._bottomRadioButton.AutoSize = true;
            this._bottomRadioButton.Location = new System.Drawing.Point(12, 89);
            this._bottomRadioButton.Name = "_bottomRadioButton";
            this._bottomRadioButton.Size = new System.Drawing.Size(67, 19);
            this._bottomRadioButton.TabIndex = 3;
            this._bottomRadioButton.TabStop = true;
            this._bottomRadioButton.Text = "Bottom";
            this._bottomRadioButton.UseVisualStyleBackColor = true;
            // 
            // _leftRadioButton
            // 
            this._leftRadioButton.AutoSize = true;
            this._leftRadioButton.Location = new System.Drawing.Point(12, 114);
            this._leftRadioButton.Name = "_leftRadioButton";
            this._leftRadioButton.Size = new System.Drawing.Size(43, 19);
            this._leftRadioButton.TabIndex = 4;
            this._leftRadioButton.TabStop = true;
            this._leftRadioButton.Text = "Left";
            this._leftRadioButton.UseVisualStyleBackColor = true;
            // 
            // _rightRadioButton
            // 
            this._rightRadioButton.AutoSize = true;
            this._rightRadioButton.Location = new System.Drawing.Point(12, 139);
            this._rightRadioButton.Name = "_rightRadioButton";
            this._rightRadioButton.Size = new System.Drawing.Size(54, 19);
            this._rightRadioButton.TabIndex = 5;
            this._rightRadioButton.TabStop = true;
            this._rightRadioButton.Text = "Right";
            this._rightRadioButton.UseVisualStyleBackColor = true;
            // 
            // LegendEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._rightRadioButton);
            this.Controls.Add(this._leftRadioButton);
            this.Controls.Add(this._bottomRadioButton);
            this.Controls.Add(this._topRadioButton);
            this.Controls.Add(this._placementLabel);
            this.Controls.Add(this._showLegendCheckBox);
            this.Name = "LegendEditorControl";
            this.Size = new System.Drawing.Size(546, 346);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.CheckBox _showLegendCheckBox;
        private System.Windows.Forms.Label _placementLabel;
        private System.Windows.Forms.RadioButton _topRadioButton;
        private System.Windows.Forms.RadioButton _bottomRadioButton;
        private System.Windows.Forms.RadioButton _leftRadioButton;
        private System.Windows.Forms.RadioButton _rightRadioButton;
    }
}
