using System.Drawing;

namespace Graphing.TestScenarios
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
            this.buttonScenarioA = new System.Windows.Forms.Button();
            this.buttonScenarioB = new System.Windows.Forms.Button();
            this.checkBoxResizeChart = new System.Windows.Forms.CheckBox();
            this.graphControl = new Graphing.Controls.EngineeringGraphControl();
            this.buttonScenarioC = new System.Windows.Forms.Button();
            this.buttonScenarioD = new System.Windows.Forms.Button();
            this.buttonScenarioE = new System.Windows.Forms.Button();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.buttonExportPng = new System.Windows.Forms.Button();
            this.buttonExportEmf = new System.Windows.Forms.Button();
            this.checkBoxAnimation = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableZoom = new System.Windows.Forms.CheckBox();
            this.buttonZoomExtents = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonScenarioA
            // 
            this.buttonScenarioA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScenarioA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScenarioA.Location = new System.Drawing.Point(689, 12);
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
            this.buttonScenarioB.Location = new System.Drawing.Point(689, 43);
            this.buttonScenarioB.Name = "buttonScenarioB";
            this.buttonScenarioB.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioB.TabIndex = 2;
            this.buttonScenarioB.Text = "Scenario B";
            this.buttonScenarioB.UseVisualStyleBackColor = true;
            this.buttonScenarioB.Click += new System.EventHandler(this.buttonScenarioB_Click);
            // 
            // checkBoxResizeChart
            // 
            this.checkBoxResizeChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxResizeChart.AutoSize = true;
            this.checkBoxResizeChart.Checked = true;
            this.checkBoxResizeChart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxResizeChart.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxResizeChart.Location = new System.Drawing.Point(680, 167);
            this.checkBoxResizeChart.Name = "checkBoxResizeChart";
            this.checkBoxResizeChart.Size = new System.Drawing.Size(92, 18);
            this.checkBoxResizeChart.TabIndex = 9;
            this.checkBoxResizeChart.Text = "Resize Chart";
            this.checkBoxResizeChart.UseVisualStyleBackColor = true;
            this.checkBoxResizeChart.CheckedChanged += new System.EventHandler(this.checkBoxResizeChart_CheckedChanged);
            // 
            // graphControl
            // 
            this.graphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphControl.AnimationBarColor = System.Drawing.Color.OrangeRed;
            this.graphControl.AnimationBarEnabled = false;
            this.graphControl.AnimationBarXIndex = 0;
            this.graphControl.Location = new System.Drawing.Point(12, 12);
            this.graphControl.Name = "graphControl";
            this.graphControl.Size = new System.Drawing.Size(662, 437);
            this.graphControl.TabIndex = 0;
            this.graphControl.ZoomEnabled = false;
            this.graphControl.AxisContextRequested += new System.EventHandler<Graphing.Controls.Interaction.AxisInteractionMouseEventArgs>(this.graphControl_AxisContextRequested);
            // 
            // buttonScenarioC
            // 
            this.buttonScenarioC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScenarioC.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScenarioC.Location = new System.Drawing.Point(689, 74);
            this.buttonScenarioC.Name = "buttonScenarioC";
            this.buttonScenarioC.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioC.TabIndex = 3;
            this.buttonScenarioC.Text = "Scenario C";
            this.buttonScenarioC.UseVisualStyleBackColor = true;
            this.buttonScenarioC.Click += new System.EventHandler(this.buttonScenarioC_Click);
            // 
            // buttonScenarioD
            // 
            this.buttonScenarioD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScenarioD.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScenarioD.Location = new System.Drawing.Point(689, 105);
            this.buttonScenarioD.Name = "buttonScenarioD";
            this.buttonScenarioD.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioD.TabIndex = 4;
            this.buttonScenarioD.Text = "Scenario D";
            this.buttonScenarioD.UseVisualStyleBackColor = true;
            this.buttonScenarioD.Click += new System.EventHandler(this.buttonScenarioD_Click);
            // 
            // buttonScenarioE
            // 
            this.buttonScenarioE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScenarioE.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScenarioE.Location = new System.Drawing.Point(689, 136);
            this.buttonScenarioE.Name = "buttonScenarioE";
            this.buttonScenarioE.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioE.TabIndex = 5;
            this.buttonScenarioE.Text = "Scenario E";
            this.buttonScenarioE.UseVisualStyleBackColor = true;
            this.buttonScenarioE.Click += new System.EventHandler(this.buttonScenarioE_Click);
            // 
            // buttonOptions
            // 
            this.buttonOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOptions.Location = new System.Drawing.Point(689, 177);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(67, 25);
            this.buttonOptions.TabIndex = 12;
            this.buttonOptions.Text = "Options";
            this.buttonOptions.UseVisualStyleBackColor = true;
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
            // 
            // buttonExportPng
            // 
            this.buttonExportPng.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExportPng.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonExportPng.Location = new System.Drawing.Point(689, 218);
            this.buttonExportPng.Name = "buttonExportPng";
            this.buttonExportPng.Size = new System.Drawing.Size(67, 25);
            this.buttonExportPng.TabIndex = 13;
            this.buttonExportPng.Text = "PNG";
            this.buttonExportPng.UseVisualStyleBackColor = true;
            this.buttonExportPng.Click += new System.EventHandler(this.buttonExportPng_Click);
            // 
            // buttonExportEmf
            // 
            this.buttonExportEmf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExportEmf.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonExportEmf.Location = new System.Drawing.Point(689, 249);
            this.buttonExportEmf.Name = "buttonExportEmf";
            this.buttonExportEmf.Size = new System.Drawing.Size(67, 25);
            this.buttonExportEmf.TabIndex = 14;
            this.buttonExportEmf.Text = "EMF";
            this.buttonExportEmf.UseVisualStyleBackColor = true;
            this.buttonExportEmf.Click += new System.EventHandler(this.buttonExportEmf_Click);
            // 
            // checkBoxAnimation
            // 
            this.checkBoxAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAnimation.AutoSize = true;
            this.checkBoxAnimation.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxAnimation.Location = new System.Drawing.Point(680, 280);
            this.checkBoxAnimation.Name = "checkBoxAnimation";
            this.checkBoxAnimation.Size = new System.Drawing.Size(78, 18);
            this.checkBoxAnimation.TabIndex = 15;
            this.checkBoxAnimation.Text = "Animation";
            this.checkBoxAnimation.UseVisualStyleBackColor = true;
            this.checkBoxAnimation.CheckedChanged += new System.EventHandler(this.checkBoxAnimation_CheckedChanged);
            // 
            // checkBoxEnableZoom
            // 
            this.checkBoxEnableZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxEnableZoom.AutoSize = true;
            this.checkBoxEnableZoom.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxEnableZoom.Location = new System.Drawing.Point(689, 431);
            this.checkBoxEnableZoom.Name = "checkBoxEnableZoom";
            this.checkBoxEnableZoom.Size = new System.Drawing.Size(73, 18);
            this.checkBoxEnableZoom.TabIndex = 16;
            this.checkBoxEnableZoom.Text = "Zooming";
            this.checkBoxEnableZoom.UseVisualStyleBackColor = true;
            this.checkBoxEnableZoom.CheckedChanged += new System.EventHandler(this.checkBoxEnableZoom_CheckedChanged);
            // 
            // buttonZoomExtents
            // 
            this.buttonZoomExtents.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonZoomExtents.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonZoomExtents.Location = new System.Drawing.Point(689, 400);
            this.buttonZoomExtents.Name = "buttonZoomExtents";
            this.buttonZoomExtents.Size = new System.Drawing.Size(67, 25);
            this.buttonZoomExtents.TabIndex = 17;
            this.buttonZoomExtents.Text = "Extents";
            this.buttonZoomExtents.UseVisualStyleBackColor = true;
            this.buttonZoomExtents.Click += new System.EventHandler(this.buttonZoomExtents_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.buttonZoomExtents);
            this.Controls.Add(this.checkBoxEnableZoom);
            this.Controls.Add(this.checkBoxAnimation);
            this.Controls.Add(this.buttonExportEmf);
            this.Controls.Add(this.buttonExportPng);
            this.Controls.Add(this.buttonOptions);
            this.Controls.Add(this.buttonScenarioE);
            this.Controls.Add(this.buttonScenarioD);
            this.Controls.Add(this.buttonScenarioC);
            this.Controls.Add(this.checkBoxResizeChart);
            this.Controls.Add(this.buttonScenarioB);
            this.Controls.Add(this.buttonScenarioA);
            this.Controls.Add(this.graphControl);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.EngineeringGraphControl graphControl;
        private System.Windows.Forms.Button buttonScenarioA;
        private System.Windows.Forms.Button buttonScenarioB;
        private System.Windows.Forms.CheckBox checkBoxResizeChart;
        private System.Windows.Forms.Button buttonScenarioC;
        private System.Windows.Forms.Button buttonScenarioD;
        private System.Windows.Forms.Button buttonScenarioE;
        private System.Windows.Forms.Button buttonOptions;
        private System.Windows.Forms.Button buttonExportPng;
        private System.Windows.Forms.Button buttonExportEmf;
        private System.Windows.Forms.CheckBox checkBoxAnimation;
        private System.Windows.Forms.CheckBox checkBoxEnableZoom;
        private System.Windows.Forms.Button buttonZoomExtents;
    }
}
