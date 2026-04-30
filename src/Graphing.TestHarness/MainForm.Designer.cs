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
            this.groupBoxUnits = new System.Windows.Forms.GroupBox();
            this.buttonPressure = new System.Windows.Forms.Button();
            this.buttonElevation = new System.Windows.Forms.Button();
            this.buttonTime = new System.Windows.Forms.Button();
            this.buttonScenarioA = new System.Windows.Forms.Button();
            this.buttonScenarioB = new System.Windows.Forms.Button();
            this.groupBoxSeriesAxesVisibility = new System.Windows.Forms.GroupBox();
            this.checkBoxPressure2Grid = new System.Windows.Forms.CheckBox();
            this.checkBoxTimeGrid = new System.Windows.Forms.CheckBox();
            this.checkBoxPressureGrid = new System.Windows.Forms.CheckBox();
            this.checkBoxElevationGrid = new System.Windows.Forms.CheckBox();
            this.checkBoxPressure = new System.Windows.Forms.CheckBox();
            this.checkBoxHGL = new System.Windows.Forms.CheckBox();
            this.checkBoxPressureYAxis = new System.Windows.Forms.CheckBox();
            this.checkBoxElevationYAxis = new System.Windows.Forms.CheckBox();
            this.checkBoxXAxis = new System.Windows.Forms.CheckBox();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.textBoxSubTitle = new System.Windows.Forms.TextBox();
            this.buttonApplyTitles = new System.Windows.Forms.Button();
            this.checkBoxResizeChart = new System.Windows.Forms.CheckBox();
            this.groupBoxLegend = new System.Windows.Forms.GroupBox();
            this.checkBoxShowLegend = new System.Windows.Forms.CheckBox();
            this.radioButtonRight = new System.Windows.Forms.RadioButton();
            this.radioButtonTop = new System.Windows.Forms.RadioButton();
            this.radioButtonLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonBottom = new System.Windows.Forms.RadioButton();
            this.graphControl = new Graphing.Controls.EngineeringGraphControl();
            this.buttonScenarioC = new System.Windows.Forms.Button();
            this.buttonScenarioD = new System.Windows.Forms.Button();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.groupBoxUnits.SuspendLayout();
            this.groupBoxSeriesAxesVisibility.SuspendLayout();
            this.groupBoxLegend.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxUnits
            // 
            this.groupBoxUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxUnits.Controls.Add(this.buttonPressure);
            this.groupBoxUnits.Controls.Add(this.buttonElevation);
            this.groupBoxUnits.Controls.Add(this.buttonTime);
            this.groupBoxUnits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxUnits.Location = new System.Drawing.Point(12, 341);
            this.groupBoxUnits.Name = "groupBoxUnits";
            this.groupBoxUnits.Size = new System.Drawing.Size(224, 49);
            this.groupBoxUnits.TabIndex = 10;
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
            this.buttonScenarioA.Location = new System.Drawing.Point(672, 12);
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
            this.buttonScenarioB.Location = new System.Drawing.Point(672, 43);
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
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxPressure2Grid);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxTimeGrid);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxPressureGrid);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxElevationGrid);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxPressure);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxHGL);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxPressureYAxis);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxElevationYAxis);
            this.groupBoxSeriesAxesVisibility.Controls.Add(this.checkBoxXAxis);
            this.groupBoxSeriesAxesVisibility.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxSeriesAxesVisibility.Location = new System.Drawing.Point(242, 341);
            this.groupBoxSeriesAxesVisibility.Name = "groupBoxSeriesAxesVisibility";
            this.groupBoxSeriesAxesVisibility.Size = new System.Drawing.Size(424, 108);
            this.groupBoxSeriesAxesVisibility.TabIndex = 11;
            this.groupBoxSeriesAxesVisibility.TabStop = false;
            this.groupBoxSeriesAxesVisibility.Text = "Axes and Series Visibility";
            // 
            // checkBoxPressure2Grid
            // 
            this.checkBoxPressure2Grid.AutoSize = true;
            this.checkBoxPressure2Grid.Checked = true;
            this.checkBoxPressure2Grid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPressure2Grid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxPressure2Grid.Location = new System.Drawing.Point(298, 48);
            this.checkBoxPressure2Grid.Name = "checkBoxPressure2Grid";
            this.checkBoxPressure2Grid.Size = new System.Drawing.Size(110, 18);
            this.checkBoxPressure2Grid.TabIndex = 8;
            this.checkBoxPressure2Grid.Text = "Pressure (2) Grid";
            this.checkBoxPressure2Grid.UseVisualStyleBackColor = true;
            this.checkBoxPressure2Grid.CheckedChanged += new System.EventHandler(this.checkBoxPressure2Grid_CheckedChanged);
            // 
            // checkBoxTimeGrid
            // 
            this.checkBoxTimeGrid.AutoSize = true;
            this.checkBoxTimeGrid.Checked = true;
            this.checkBoxTimeGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTimeGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxTimeGrid.Location = new System.Drawing.Point(6, 49);
            this.checkBoxTimeGrid.Name = "checkBoxTimeGrid";
            this.checkBoxTimeGrid.Size = new System.Drawing.Size(77, 18);
            this.checkBoxTimeGrid.TabIndex = 5;
            this.checkBoxTimeGrid.Text = "Time Grid";
            this.checkBoxTimeGrid.UseVisualStyleBackColor = true;
            this.checkBoxTimeGrid.CheckedChanged += new System.EventHandler(this.checkBoxTimeGrid_CheckedChanged);
            // 
            // checkBoxPressureGrid
            // 
            this.checkBoxPressureGrid.AutoSize = true;
            this.checkBoxPressureGrid.Checked = true;
            this.checkBoxPressureGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPressureGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxPressureGrid.Location = new System.Drawing.Point(197, 48);
            this.checkBoxPressureGrid.Name = "checkBoxPressureGrid";
            this.checkBoxPressureGrid.Size = new System.Drawing.Size(95, 18);
            this.checkBoxPressureGrid.TabIndex = 7;
            this.checkBoxPressureGrid.Text = "Pressure Grid";
            this.checkBoxPressureGrid.UseVisualStyleBackColor = true;
            this.checkBoxPressureGrid.CheckedChanged += new System.EventHandler(this.checkBoxPressureGrid_CheckedChanged);
            // 
            // checkBoxElevationGrid
            // 
            this.checkBoxElevationGrid.AutoSize = true;
            this.checkBoxElevationGrid.Checked = true;
            this.checkBoxElevationGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxElevationGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxElevationGrid.Location = new System.Drawing.Point(89, 48);
            this.checkBoxElevationGrid.Name = "checkBoxElevationGrid";
            this.checkBoxElevationGrid.Size = new System.Drawing.Size(98, 18);
            this.checkBoxElevationGrid.TabIndex = 6;
            this.checkBoxElevationGrid.Text = "Elevation Grid";
            this.checkBoxElevationGrid.UseVisualStyleBackColor = true;
            this.checkBoxElevationGrid.CheckedChanged += new System.EventHandler(this.checkBoxElevationGrid_CheckedChanged);
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
            this.textBoxTitle.Location = new System.Drawing.Point(672, 192);
            this.textBoxTitle.Multiline = true;
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(100, 25);
            this.textBoxTitle.TabIndex = 5;
            this.textBoxTitle.Text = "Title";
            // 
            // textBoxSubTitle
            // 
            this.textBoxSubTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSubTitle.Location = new System.Drawing.Point(672, 223);
            this.textBoxSubTitle.Multiline = true;
            this.textBoxSubTitle.Name = "textBoxSubTitle";
            this.textBoxSubTitle.Size = new System.Drawing.Size(100, 25);
            this.textBoxSubTitle.TabIndex = 6;
            this.textBoxSubTitle.Text = "Subtitle";
            // 
            // buttonApplyTitles
            // 
            this.buttonApplyTitles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApplyTitles.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonApplyTitles.Location = new System.Drawing.Point(672, 254);
            this.buttonApplyTitles.Name = "buttonApplyTitles";
            this.buttonApplyTitles.Size = new System.Drawing.Size(67, 25);
            this.buttonApplyTitles.TabIndex = 7;
            this.buttonApplyTitles.Text = "Apply";
            this.buttonApplyTitles.UseVisualStyleBackColor = true;
            this.buttonApplyTitles.Click += new System.EventHandler(this.buttonApplyTitles_Click);
            // 
            // checkBoxResizeChart
            // 
            this.checkBoxResizeChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxResizeChart.AutoSize = true;
            this.checkBoxResizeChart.Checked = true;
            this.checkBoxResizeChart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxResizeChart.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxResizeChart.Location = new System.Drawing.Point(672, 431);
            this.checkBoxResizeChart.Name = "checkBoxResizeChart";
            this.checkBoxResizeChart.Size = new System.Drawing.Size(92, 18);
            this.checkBoxResizeChart.TabIndex = 9;
            this.checkBoxResizeChart.Text = "Resize Chart";
            this.checkBoxResizeChart.UseVisualStyleBackColor = true;
            this.checkBoxResizeChart.CheckedChanged += new System.EventHandler(this.checkBoxResizeChart_CheckedChanged);
            // 
            // groupBoxLegend
            // 
            this.groupBoxLegend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLegend.Controls.Add(this.checkBoxShowLegend);
            this.groupBoxLegend.Controls.Add(this.radioButtonRight);
            this.groupBoxLegend.Controls.Add(this.radioButtonTop);
            this.groupBoxLegend.Controls.Add(this.radioButtonLeft);
            this.groupBoxLegend.Controls.Add(this.radioButtonBottom);
            this.groupBoxLegend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxLegend.Location = new System.Drawing.Point(672, 296);
            this.groupBoxLegend.Name = "groupBoxLegend";
            this.groupBoxLegend.Size = new System.Drawing.Size(100, 122);
            this.groupBoxLegend.TabIndex = 8;
            this.groupBoxLegend.TabStop = false;
            // 
            // checkBoxShowLegend
            // 
            this.checkBoxShowLegend.AutoSize = true;
            this.checkBoxShowLegend.Checked = true;
            this.checkBoxShowLegend.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowLegend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxShowLegend.Location = new System.Drawing.Point(6, -2);
            this.checkBoxShowLegend.Name = "checkBoxShowLegend";
            this.checkBoxShowLegend.Size = new System.Drawing.Size(68, 18);
            this.checkBoxShowLegend.TabIndex = 0;
            this.checkBoxShowLegend.Text = "Legend";
            this.checkBoxShowLegend.UseVisualStyleBackColor = true;
            this.checkBoxShowLegend.CheckedChanged += new System.EventHandler(this.checkBoxShowLegend_CheckedChanged);
            // 
            // radioButtonRight
            // 
            this.radioButtonRight.AutoSize = true;
            this.radioButtonRight.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonRight.Location = new System.Drawing.Point(6, 90);
            this.radioButtonRight.Name = "radioButtonRight";
            this.radioButtonRight.Size = new System.Drawing.Size(56, 18);
            this.radioButtonRight.TabIndex = 4;
            this.radioButtonRight.Text = "Right";
            this.radioButtonRight.UseVisualStyleBackColor = true;
            this.radioButtonRight.CheckedChanged += new System.EventHandler(this.radioButtonRight_CheckedChanged);
            // 
            // radioButtonTop
            // 
            this.radioButtonTop.AutoSize = true;
            this.radioButtonTop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonTop.Location = new System.Drawing.Point(6, 66);
            this.radioButtonTop.Name = "radioButtonTop";
            this.radioButtonTop.Size = new System.Drawing.Size(50, 18);
            this.radioButtonTop.TabIndex = 3;
            this.radioButtonTop.Text = "Top";
            this.radioButtonTop.UseVisualStyleBackColor = true;
            this.radioButtonTop.CheckedChanged += new System.EventHandler(this.radioButtonTop_CheckedChanged);
            // 
            // radioButtonLeft
            // 
            this.radioButtonLeft.AutoSize = true;
            this.radioButtonLeft.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonLeft.Location = new System.Drawing.Point(6, 42);
            this.radioButtonLeft.Name = "radioButtonLeft";
            this.radioButtonLeft.Size = new System.Drawing.Size(49, 18);
            this.radioButtonLeft.TabIndex = 2;
            this.radioButtonLeft.Text = "Left";
            this.radioButtonLeft.UseVisualStyleBackColor = true;
            this.radioButtonLeft.CheckedChanged += new System.EventHandler(this.radioButtonLeft_CheckedChanged);
            // 
            // radioButtonBottom
            // 
            this.radioButtonBottom.AutoSize = true;
            this.radioButtonBottom.Checked = true;
            this.radioButtonBottom.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonBottom.Location = new System.Drawing.Point(6, 19);
            this.radioButtonBottom.Name = "radioButtonBottom";
            this.radioButtonBottom.Size = new System.Drawing.Size(64, 18);
            this.radioButtonBottom.TabIndex = 1;
            this.radioButtonBottom.TabStop = true;
            this.radioButtonBottom.Text = "Bottom";
            this.radioButtonBottom.UseVisualStyleBackColor = true;
            this.radioButtonBottom.CheckedChanged += new System.EventHandler(this.radioButtonBottom_CheckedChanged);
            // 
            // graphControl
            // 
            this.graphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphControl.Location = new System.Drawing.Point(12, 12);
            this.graphControl.Name = "graphControl";
            this.graphControl.Size = new System.Drawing.Size(654, 323);
            this.graphControl.TabIndex = 0;
            // 
            // buttonScenarioC
            // 
            this.buttonScenarioC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScenarioC.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonScenarioC.Location = new System.Drawing.Point(672, 74);
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
            this.buttonScenarioD.Location = new System.Drawing.Point(672, 105);
            this.buttonScenarioD.Name = "buttonScenarioD";
            this.buttonScenarioD.Size = new System.Drawing.Size(67, 25);
            this.buttonScenarioD.TabIndex = 4;
            this.buttonScenarioD.Text = "Scenario D";
            this.buttonScenarioD.UseVisualStyleBackColor = true;
            this.buttonScenarioD.Click += new System.EventHandler(this.buttonScenarioD_Click);
            // 
            // buttonOptions
            // 
            this.buttonOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOptions.Location = new System.Drawing.Point(12, 424);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(67, 25);
            this.buttonOptions.TabIndex = 12;
            this.buttonOptions.Text = "Options";
            this.buttonOptions.UseVisualStyleBackColor = true;
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.buttonOptions);
            this.Controls.Add(this.buttonScenarioD);
            this.Controls.Add(this.buttonScenarioC);
            this.Controls.Add(this.groupBoxLegend);
            this.Controls.Add(this.checkBoxResizeChart);
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
            this.groupBoxLegend.ResumeLayout(false);
            this.groupBoxLegend.PerformLayout();
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
        private System.Windows.Forms.CheckBox checkBoxResizeChart;
        private System.Windows.Forms.GroupBox groupBoxLegend;
        private System.Windows.Forms.RadioButton radioButtonRight;
        private System.Windows.Forms.RadioButton radioButtonTop;
        private System.Windows.Forms.RadioButton radioButtonLeft;
        private System.Windows.Forms.RadioButton radioButtonBottom;
        private System.Windows.Forms.CheckBox checkBoxShowLegend;
        private System.Windows.Forms.Button buttonScenarioC;
        private System.Windows.Forms.Button buttonScenarioD;
        private System.Windows.Forms.CheckBox checkBoxTimeGrid;
        private System.Windows.Forms.CheckBox checkBoxPressureGrid;
        private System.Windows.Forms.CheckBox checkBoxElevationGrid;
        private System.Windows.Forms.CheckBox checkBoxPressure2Grid;
        private System.Windows.Forms.Button buttonOptions;
    }
}
