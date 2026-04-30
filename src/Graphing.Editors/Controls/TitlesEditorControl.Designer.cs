namespace Graphing.Editors.Controls
{
    partial class TitlesEditorControl
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
            this._titleGroupBox = new System.Windows.Forms.GroupBox();
            this._titleTextBox = new System.Windows.Forms.TextBox();
            this._subtitleGroupBox = new System.Windows.Forms.GroupBox();
            this._subtitleTextBox = new System.Windows.Forms.TextBox();
            this._titleGroupBox.SuspendLayout();
            this._subtitleGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _titleGroupBox
            // 
            this._titleGroupBox.Controls.Add(this._titleTextBox);
            this._titleGroupBox.Location = new System.Drawing.Point(12, 12);
            this._titleGroupBox.Name = "_titleGroupBox";
            this._titleGroupBox.Size = new System.Drawing.Size(522, 86);
            this._titleGroupBox.TabIndex = 0;
            this._titleGroupBox.TabStop = false;
            this._titleGroupBox.Text = "Title";
            // 
            // _titleTextBox
            // 
            this._titleTextBox.Location = new System.Drawing.Point(6, 33);
            this._titleTextBox.Name = "_titleTextBox";
            this._titleTextBox.Size = new System.Drawing.Size(510, 23);
            this._titleTextBox.TabIndex = 0;
            // 
            // _subtitleGroupBox
            // 
            this._subtitleGroupBox.Controls.Add(this._subtitleTextBox);
            this._subtitleGroupBox.Location = new System.Drawing.Point(12, 110);
            this._subtitleGroupBox.Name = "_subtitleGroupBox";
            this._subtitleGroupBox.Size = new System.Drawing.Size(522, 86);
            this._subtitleGroupBox.TabIndex = 1;
            this._subtitleGroupBox.TabStop = false;
            this._subtitleGroupBox.Text = "Subtitle";
            // 
            // _subtitleTextBox
            // 
            this._subtitleTextBox.Location = new System.Drawing.Point(6, 33);
            this._subtitleTextBox.Name = "_subtitleTextBox";
            this._subtitleTextBox.Size = new System.Drawing.Size(510, 23);
            this._subtitleTextBox.TabIndex = 0;
            // 
            // TitlesEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._subtitleGroupBox);
            this.Controls.Add(this._titleGroupBox);
            this.Name = "TitlesEditorControl";
            this.Size = new System.Drawing.Size(546, 346);
            this._titleGroupBox.ResumeLayout(false);
            this._titleGroupBox.PerformLayout();
            this._subtitleGroupBox.ResumeLayout(false);
            this._subtitleGroupBox.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox _titleGroupBox;
        private System.Windows.Forms.TextBox _titleTextBox;
        private System.Windows.Forms.GroupBox _subtitleGroupBox;
        private System.Windows.Forms.TextBox _subtitleTextBox;
    }
}
