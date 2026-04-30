using Graphing.Editors.Controls;

namespace Graphing.Editors
{
    partial class EngineeringGraphOptionsEditorForm
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
            this._tabControl = new System.Windows.Forms.TabControl();
            this._tabPageTitles = new System.Windows.Forms.TabPage();
            this._tabPageAxes = new System.Windows.Forms.TabPage();
            this._tabPageSeries = new System.Windows.Forms.TabPage();
            this._tabPageLegend = new System.Windows.Forms.TabPage();
            this._titlesEditorControl = new TitlesEditorControl();
            this._axesEditorControl = new AxesEditorControl();
            this._seriesEditorControl = new SeriesEditorControl();
            this._legendEditorControl = new LegendEditorControl();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._tabControl.SuspendLayout();
            this._tabPageTitles.SuspendLayout();
            this._tabPageAxes.SuspendLayout();
            this._tabPageSeries.SuspendLayout();
            this._tabPageLegend.SuspendLayout();
            this.SuspendLayout();
            //
            // _tabControl
            //
            this._tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right));
            this._tabControl.Controls.Add(this._tabPageTitles);
            this._tabControl.Controls.Add(this._tabPageAxes);
            this._tabControl.Controls.Add(this._tabPageSeries);
            this._tabControl.Controls.Add(this._tabPageLegend);
            this._tabControl.Location = new System.Drawing.Point(12, 12);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(560, 380);
            this._tabControl.TabIndex = 0;
            //
            // _tabPageTitles
            //
            this._tabPageTitles.Controls.Add(this._titlesEditorControl);
            this._tabPageTitles.Location = new System.Drawing.Point(4, 24);
            this._tabPageTitles.Name = "_tabPageTitles";
            this._tabPageTitles.Padding = new System.Windows.Forms.Padding(3);
            this._tabPageTitles.Size = new System.Drawing.Size(552, 352);
            this._tabPageTitles.TabIndex = 0;
            this._tabPageTitles.Text = "Titles";
            this._tabPageTitles.UseVisualStyleBackColor = true;
            //
            // _titlesEditorControl
            //
            this._titlesEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._titlesEditorControl.Location = new System.Drawing.Point(3, 3);
            this._titlesEditorControl.Name = "_titlesEditorControl";
            this._titlesEditorControl.Size = new System.Drawing.Size(546, 346);
            this._titlesEditorControl.TabIndex = 0;
            //
            // _tabPageAxes
            //
            this._tabPageAxes.Controls.Add(this._axesEditorControl);
            this._tabPageAxes.Location = new System.Drawing.Point(4, 24);
            this._tabPageAxes.Name = "_tabPageAxes";
            this._tabPageAxes.Padding = new System.Windows.Forms.Padding(3);
            this._tabPageAxes.Size = new System.Drawing.Size(552, 352);
            this._tabPageAxes.TabIndex = 1;
            this._tabPageAxes.Text = "Axes";
            this._tabPageAxes.UseVisualStyleBackColor = true;
            //
            // _axesEditorControl
            //
            this._axesEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._axesEditorControl.Location = new System.Drawing.Point(3, 3);
            this._axesEditorControl.Name = "_axesEditorControl";
            this._axesEditorControl.Size = new System.Drawing.Size(546, 346);
            this._axesEditorControl.TabIndex = 0;
            //
            // _tabPageSeries
            //
            this._tabPageSeries.Controls.Add(this._seriesEditorControl);
            this._tabPageSeries.Location = new System.Drawing.Point(4, 24);
            this._tabPageSeries.Name = "_tabPageSeries";
            this._tabPageSeries.Padding = new System.Windows.Forms.Padding(3);
            this._tabPageSeries.Size = new System.Drawing.Size(552, 352);
            this._tabPageSeries.TabIndex = 2;
            this._tabPageSeries.Text = "Series";
            this._tabPageSeries.UseVisualStyleBackColor = true;
            //
            // _seriesEditorControl
            //
            this._seriesEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._seriesEditorControl.Location = new System.Drawing.Point(3, 3);
            this._seriesEditorControl.Name = "_seriesEditorControl";
            this._seriesEditorControl.Size = new System.Drawing.Size(546, 346);
            this._seriesEditorControl.TabIndex = 0;
            //
            // _tabPageLegend
            //
            this._tabPageLegend.Controls.Add(this._legendEditorControl);
            this._tabPageLegend.Location = new System.Drawing.Point(4, 24);
            this._tabPageLegend.Name = "_tabPageLegend";
            this._tabPageLegend.Padding = new System.Windows.Forms.Padding(3);
            this._tabPageLegend.Size = new System.Drawing.Size(552, 352);
            this._tabPageLegend.TabIndex = 3;
            this._tabPageLegend.Text = "Legend";
            this._tabPageLegend.UseVisualStyleBackColor = true;
            //
            // _legendEditorControl
            //
            this._legendEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._legendEditorControl.Location = new System.Drawing.Point(3, 3);
            this._legendEditorControl.Name = "_legendEditorControl";
            this._legendEditorControl.Size = new System.Drawing.Size(546, 346);
            this._legendEditorControl.TabIndex = 0;
            //
            // _buttonOk
            //
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Right));
            this._buttonOk.Location = new System.Drawing.Point(416, 404);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 25);
            this._buttonOk.TabIndex = 1;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            //
            // _buttonCancel
            //
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Right));
            this._buttonCancel.Location = new System.Drawing.Point(497, 404);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 25);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // EngineeringGraphOptionsEditorForm
            //
            this.AcceptButton = this._buttonOk;
            this.CancelButton = this._buttonCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 441);
            this.Controls.Add(this._tabControl);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Name = "EngineeringGraphOptionsEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Graph Options";
            this.Load += EngineeringGraphOptionsEditorForm_Load;
            this._tabControl.ResumeLayout(false);
            this._tabPageTitles.ResumeLayout(false);
            this._tabPageAxes.ResumeLayout(false);
            this._tabPageSeries.ResumeLayout(false);
            this._tabPageLegend.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _tabPageTitles;
        private System.Windows.Forms.TabPage _tabPageAxes;
        private System.Windows.Forms.TabPage _tabPageSeries;
        private System.Windows.Forms.TabPage _tabPageLegend;
        private TitlesEditorControl _titlesEditorControl;
        private AxesEditorControl _axesEditorControl;
        private SeriesEditorControl _seriesEditorControl;
        private LegendEditorControl _legendEditorControl;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
    }
}
