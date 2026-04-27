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
            this.buttonTime = new System.Windows.Forms.Button();
            this.buttonScenarioA = new System.Windows.Forms.Button();
            this.buttonScenarioB = new System.Windows.Forms.Button();
            this.groupBoxUnits.SuspendLayout();
            this.SuspendLayout();
            // 
            // graphControl
            // 
            this.graphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.graphControl.Location = new System.Drawing.Point(12, 12);
            this.graphControl.Name = "graphControl";
            this.graphControl.Size = new System.Drawing.Size(703, 371);
            this.graphControl.TabIndex = 0;
            // 
            // groupBoxUnits
            // 
            this.groupBoxUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxUnits.Controls.Add(this.buttonTime);
            this.groupBoxUnits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxUnits.Location = new System.Drawing.Point(12, 389);
            this.groupBoxUnits.Name = "groupBoxUnits";
            this.groupBoxUnits.Size = new System.Drawing.Size(776, 49);
            this.groupBoxUnits.TabIndex = 1;
            this.groupBoxUnits.TabStop = false;
            this.groupBoxUnits.Text = "Units";
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
            this.buttonScenarioA.Location = new System.Drawing.Point(721, 12);
            this.buttonScenarioA.Name = "buttonScenarioA";
            this.buttonScenarioA.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioA.TabIndex = 2;
            this.buttonScenarioA.Text = "Scenario A";
            this.buttonScenarioA.UseVisualStyleBackColor = true;
            this.buttonScenarioA.Click += new System.EventHandler(this.buttonScenarioA_Click);
            // 
            // buttonScenarioB
            // 
            this.buttonScenarioB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScenarioB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScenarioB.Location = new System.Drawing.Point(721, 43);
            this.buttonScenarioB.Name = "buttonScenarioB";
            this.buttonScenarioB.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioB.TabIndex = 3;
            this.buttonScenarioB.Text = "Scenario B";
            this.buttonScenarioB.UseVisualStyleBackColor = true;
            this.buttonScenarioB.Click += new System.EventHandler(this.buttonScenarioB_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonScenarioB);
            this.Controls.Add(this.buttonScenarioA);
            this.Controls.Add(this.groupBoxUnits);
            this.Controls.Add(this.graphControl);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.groupBoxUnits.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.EngineeringGraphControl graphControl;
        private System.Windows.Forms.GroupBox groupBoxUnits;
        private System.Windows.Forms.Button buttonTime;
        private System.Windows.Forms.Button buttonScenarioA;
        private System.Windows.Forms.Button buttonScenarioB;
    }
}
