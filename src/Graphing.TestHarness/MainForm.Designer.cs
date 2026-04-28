using System.Drawing;

namespace Graphing.TestHarness
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.graphControl = new Graphing.Controls.EngineeringGraphControl();
            this.groupBoxUnits = new System.Windows.Forms.GroupBox();
            this.buttonPressure = new System.Windows.Forms.Button();
            this.buttonElevation = new System.Windows.Forms.Button();
            this.buttonTime = new System.Windows.Forms.Button();
            this.buttonScenarioA = new System.Windows.Forms.Button();
            this.buttonScenarioB = new System.Windows.Forms.Button();
            this.groupBoxSeriesAxesVisibility = new System.Windows.Forms.GroupBox();
            this.checkBoxPressure = new System.Windows.Forms.CheckBox();
            this.checkBoxHGL = new System.Windows.Forms.CheckBox();
            this.checkBoxPressureYAxis = new System.Windows.Forms.CheckBox();
            this.checkBoxElevationYAxis = new System.Windows.Forms.CheckBox();
            this.checkBoxXAxis = new System.Windows.Forms.CheckBox();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.textBoxSubTitle = new System.Windows.Forms.TextBox();
            this.buttonApplyTitles = new System.Windows.Forms.Button();
            this.groupBoxUnits.SuspendLayout();
            this.groupBoxSeriesAxesVisibility.SuspendLayout();
            this.SuspendLayout();
            // 
            // graphControl
            // 
            this.graphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphControl.Location = new System.Drawing.Point(12, 12);
            this.graphControl.Name = "graphControl";
            this.graphControl.Size = new System.Drawing.Size(670, 371);
            this.graphControl.TabIndex = 0;
            // 
            // groupBoxUnits
            // 
            this.groupBoxUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxUnits.Controls.Add(this.buttonPressure);
            this.groupBoxUnits.Controls.Add(this.buttonElevation);
            this.groupBoxUnits.Controls.Add(this.buttonTime);
            this.groupBoxUnits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxUnits.Location = new System.Drawing.Point(12, 389);
            this.groupBoxUnits.Name = "groupBoxUnits";
            this.groupBoxUnits.Size = new System.Drawing.Size(224, 49);
            this.groupBoxUnits.TabIndex = 3;
            this.groupBoxUnits.TabStop = false;
            this.groupBoxUnits.Text = "Units";
            // 
            // buttonPressure
            // 
            this.buttonPressure.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonPressure.Location = new System.Drawing.Point(152, 20);
            this.buttonPressure.Name = "buttonPressure";
            this.buttonPressure.Size = new System.Drawing.Size(67, 25);
            this.buttonPressure.TabIndex = 2;
            this.buttonPressure.Text = "Pressure";
            this.buttonPressure.UseVisualStyleBackColor = true;
            this.buttonPressure.Click += new System.EventHandler(this.buttonPressure_Click);
            // 
            // buttonElevation
            // 
            this.buttonElevation.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonElevation.Location = new System.Drawing.Point(79, 20);
            this.buttonElevation.Name = "buttonElevation";
            this.buttonElevation.Size = new System.Drawing.Size(67, 25);
            this.buttonElevation.TabIndex = 1;
            this.buttonElevation.Text = "Elevation";
            this.buttonElevation.UseVisualStyleBackColor = true;
            this.buttonElevation.Click += new System.EventHandler(this.buttonElevation_Click);
            // 
            // buttonTime
            // 
            this.buttonTime.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonTime.Location = new System.Drawing.Point(6, 20);
            this.buttonTime.Name = "buttonTime";
            this.buttonTime.Size = new System.Drawing.Size(67, 25);
            this.buttonTime.TabIndex = 0;
            this.buttonTime.Text = "Time";
            this.buttonTime.UseVisualStyleBackColor = true;
            this.buttonTime.Click += new System.EventHandler(this.buttonTime_Click);
            // 
            // buttonScenarioA
            // 
            this.buttonScenarioA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScenarioA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScenarioA.Location = new System.Drawing.Point(688, 12);
            this.buttonScenarioA.Name = "buttonScenarioA";
            this.buttonScenarioA.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioA.TabIndex = 1;
            this.buttonScenarioA.Text = "Scenario A";
            this.buttonScenarioA.UseVisualStyleBackColor = true;
            this.buttonScenarioA.Click += new System.EventHandler(this.buttonScenarioA_Click);
            // 
            // buttonScenarioB
            // 
            this.buttonScenarioB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScenarioB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScenarioB.Location = new System.Drawing.Point(688, 43);
            this.buttonScenarioB.Name = "buttonScenarioB";
            this.buttonScenarioB.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioB.TabIndex = 2;
            this.buttonScenarioB.Text = "Scenario B";
            this.buttonScenarioB.UseVisualStyleBackColor = true;
            this.buttonScenarioB.Click += new System.EventHandler(this.buttonScenarioB_Click);
            // 
            // groupBoxSeriesAxesVisibility
            // 
            this.groupBoxSeriesAxesVisibility.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxPressure);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxHGL);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxPressureYAxis);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxElevationYAxis);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxXAxis);
            this.groupBoxSeriesAxesVisibility.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxSeriesAxesVisibility.Location = new System.Drawing.Point(242, 389);
            this.groupBoxSeriesAxesVisibility.Name = "groupBoxSeriesAxesVisibility";
            this.groupBoxSeriesAxesVisibility.Size = new System.Drawing.Size(440, 49);
            this.groupBoxSeriesAxesVisibility.TabIndex = 4;
            this.groupBoxSeriesAxesVisibility.TabStop = false;
            this.groupBoxSeriesAxesVisibility.Text = "Axes and Series Visibility";
            // 
            // checkBoxPressure
            // 
            this.checkBoxPressure.AutoSize = true;
            this.checkBoxPressure.Checked = true;
            this.checkBoxPressure.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPressure.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxPressure.Location = new System.Drawing.Point(358, 24);
            this.checkBoxPressure.Name = "checkBoxPressure";
            this.checkBoxPressure.Size = new System.Drawing.Size(73, 18);
            this.checkBoxPressure.TabIndex = 4;
            this.checkBoxPressure.Text = "Pressure";
            this.checkBoxPressure.UseVisualStyleBackColor = true;
            this.checkBoxPressure.Visible = false;
            this.checkBoxPressure.CheckedChanged += new System.EventHandler(this.checkBoxPressure_CheckedChanged);
            // 
            // checkBoxHGL
            // 
            this.checkBoxHGL.AutoSize = true;
            this.checkBoxHGL.Checked = true;
            this.checkBoxHGL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxHGL.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxHGL.Location = new System.Drawing.Point(298, 24);
            this.checkBoxHGL.Name = "checkBoxHGL";
            this.checkBoxHGL.Size = new System.Drawing.Size(54, 18);
            this.checkBoxHGL.TabIndex = 3;
            this.checkBoxHGL.Text = "HGL";
            this.checkBoxHGL.UseVisualStyleBackColor = true;
            this.checkBoxHGL.CheckedChanged += new System.EventHandler(this.checkBoxHGL_CheckedChanged);
            // 
            // checkBoxPressureYAxis
            // 
            this.checkBoxPressureYAxis.AutoSize = true;
            this.checkBoxPressureYAxis.Checked = true;
            this.checkBoxPressureYAxis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPressureYAxis.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxPressureYAxis.Location = new System.Drawing.Point(187, 24);
            this.checkBoxPressureYAxis.Name = "checkBoxPressureYAxis";
            this.checkBoxPressureYAxis.Size = new System.Drawing.Size(105, 18);
            this.checkBoxPressureYAxis.TabIndex = 2;
            this.checkBoxPressureYAxis.Text = "Pressure Y-Axis";
            this.checkBoxPressureYAxis.UseVisualStyleBackColor = true;
            this.checkBoxPressureYAxis.Visible = false;
            this.checkBoxPressureYAxis.CheckedChanged += new System.EventHandler(this.checkBoxPressureYAxis_CheckedChanged);
            // 
            // checkBoxElevationYAxis
            // 
            this.checkBoxElevationYAxis.AutoSize = true;
            this.checkBoxElevationYAxis.Checked = true;
            this.checkBoxElevationYAxis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxElevationYAxis.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxElevationYAxis.Location = new System.Drawing.Point(73, 24);
            this.checkBoxElevationYAxis.Name = "checkBoxElevationYAxis";
            this.checkBoxElevationYAxis.Size = new System.Drawing.Size(108, 18);
            this.checkBoxElevationYAxis.TabIndex = 1;
            this.checkBoxElevationYAxis.Text = "Elevation Y-Axis";
            this.checkBoxElevationYAxis.UseVisualStyleBackColor = true;
            this.checkBoxElevationYAxis.CheckedChanged += new System.EventHandler(this.checkBoxElevationYAxis_CheckedChanged);
            // 
            // checkBoxXAxis
            // 
            this.checkBoxXAxis.AutoSize = true;
            this.checkBoxXAxis.Checked = true;
            this.checkBoxXAxis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxXAxis.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxXAxis.Location = new System.Drawing.Point(6, 25);
            this.checkBoxXAxis.Name = "checkBoxXAxis";
            this.checkBoxXAxis.Size = new System.Drawing.Size(61, 18);
            this.checkBoxXAxis.TabIndex = 0;
            this.checkBoxXAxis.Text = "X-Axis";
            this.checkBoxXAxis.UseVisualStyleBackColor = true;
            this.checkBoxXAxis.CheckedChanged += new System.EventHandler(this.checkBoxXAxis_CheckedChanged);
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTitle.Location = new System.Drawing.Point(688, 85);
            this.textBoxTitle.Multiline = true;
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(100, 126);
            this.textBoxTitle.TabIndex = 5;
            this.textBoxTitle.Text = "Title";
            // 
            // textBoxSubTitle
            // 
            this.textBoxSubTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSubTitle.Location = new System.Drawing.Point(688, 217);
            this.textBoxSubTitle.Multiline = true;
            this.textBoxSubTitle.Name = "textBoxSubTitle";
            this.textBoxSubTitle.Size = new System.Drawing.Size(100, 135);
            this.textBoxSubTitle.TabIndex = 6;
            this.textBoxSubTitle.Text = "Subtitle";
            // 
            // buttonApplyTitles
            // 
            this.buttonApplyTitles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApplyTitles.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonApplyTitles.Location = new System.Drawing.Point(688, 358);
            this.buttonApplyTitles.Name = "buttonApplyTitles";
            this.buttonApplyTitles.Size = new System.Drawing.Size(67, 25);
            this.buttonApplyTitles.TabIndex = 7;
            this.buttonApplyTitles.Text = "Apply";
            this.buttonApplyTitles.UseVisualStyleBackColor = true;
            this.buttonApplyTitles.Click += new System.EventHandler(this.buttonApplyTitles_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonApplyTitles);
            this.Controls.Add(this.textBoxSubTitle);
            this.Controls.Add(this.textBoxTitle);
            this.Controls.Add(this.groupBoxSeriesAxesVisibility);
            this.Controls.Add(this.buttonScenarioB);
            this.Controls.Add(this.buttonScenarioA);
            this.Controls.Add(this.groupBoxUnits);
            this.Controls.Add(this.graphControl);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBoxUnits.ResumeLayout(false);
            this.groupBoxSeriesAxesVisibility.ResumeLayout(false);
            this.groupBoxSeriesAxesVisibility.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.EngineeringGraphControl graphControl;
        private System.Windows.Forms.GroupBox groupBoxUnits;
        private System.Windows.Forms.Button buttonTime;
        private System.Windows.Forms.Button buttonScenarioA;
        private System.Windows.Forms.Button buttonScenarioB;
        private System.Windows.Forms.Button buttonElevation;
        private System.Windows.Forms.Button buttonPressure;
        private System.Windows.Forms.GroupBox groupBoxSeriesAxesVisibility;
        private System.Windows.Forms.CheckBox checkBoxPressure;
        private System.Windows.Forms.CheckBox checkBoxHGL;
        private System.Windows.Forms.CheckBox checkBoxPressureYAxis;
        private System.Windows.Forms.CheckBox checkBoxElevationYAxis;
        private System.Windows.Forms.CheckBox checkBoxXAxis;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.TextBox textBoxSubTitle;
        private System.Windows.Forms.Button buttonApplyTitles;
    }
}
