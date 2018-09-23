﻿namespace ManiacEditor
{
    partial class TilesToolbar
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.option1CheckBox = new System.Windows.Forms.CheckBox();
            this.option2CheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tileOption4 = new System.Windows.Forms.CheckBox();
            this.tileOption3 = new System.Windows.Forms.CheckBox();
            this.tileOption2 = new System.Windows.Forms.CheckBox();
            this.tileOption1 = new System.Windows.Forms.CheckBox();
            this.option6CheckBox = new System.Windows.Forms.CheckBox();
            this.option5CheckBox = new System.Windows.Forms.CheckBox();
            this.option4CheckBox = new System.Windows.Forms.CheckBox();
            this.option3CheckBox = new System.Windows.Forms.CheckBox();
            this.tilesList = new ManiacEditor.TilesList();
            this.selectedTileLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBar1
            // 
            this.trackBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar1.LargeChange = 1;
            this.trackBar1.Location = new System.Drawing.Point(3, 3);
            this.trackBar1.Maximum = 3;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(249, 45);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar1.Value = 1;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // option1CheckBox
            // 
            this.option1CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.option1CheckBox.AutoSize = true;
            this.option1CheckBox.Location = new System.Drawing.Point(12, 346);
            this.option1CheckBox.Name = "option1CheckBox";
            this.option1CheckBox.Size = new System.Drawing.Size(116, 17);
            this.option1CheckBox.TabIndex = 2;
            this.option1CheckBox.Text = "Flip Horizontal (Ctrl)";
            this.option1CheckBox.UseVisualStyleBackColor = true;
            this.option1CheckBox.CheckedChanged += new System.EventHandler(this.option1CheckBox_CheckedChanged);
            // 
            // option2CheckBox
            // 
            this.option2CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.option2CheckBox.AutoSize = true;
            this.option2CheckBox.Location = new System.Drawing.Point(134, 346);
            this.option2CheckBox.Name = "option2CheckBox";
            this.option2CheckBox.Size = new System.Drawing.Size(110, 17);
            this.option2CheckBox.TabIndex = 3;
            this.option2CheckBox.Text = "Flip Vertical (Shift)";
            this.option2CheckBox.UseVisualStyleBackColor = true;
            this.option2CheckBox.CheckedChanged += new System.EventHandler(this.option2CheckBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(0, 426);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(255, 2);
            this.label1.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 428);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Selected Tiles:";
            // 
            // tileOption4
            // 
            this.tileOption4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tileOption4.Location = new System.Drawing.Point(134, 475);
            this.tileOption4.Name = "tileOption4";
            this.tileOption4.Size = new System.Drawing.Size(112, 36);
            this.tileOption4.TabIndex = 13;
            this.tileOption4.Text = "Solid (All excl. top) (Plane 2)";
            this.tileOption4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.tileOption4.UseVisualStyleBackColor = true;
            this.tileOption4.CheckedChanged += new System.EventHandler(this.tileOption4_CheckedChanged);
            // 
            // tileOption3
            // 
            this.tileOption3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tileOption3.Location = new System.Drawing.Point(12, 474);
            this.tileOption3.Name = "tileOption3";
            this.tileOption3.Size = new System.Drawing.Size(77, 37);
            this.tileOption3.TabIndex = 12;
            this.tileOption3.Text = "Solid (Top) (Plane 2)";
            this.tileOption3.UseVisualStyleBackColor = true;
            this.tileOption3.CheckedChanged += new System.EventHandler(this.tileOption3_CheckedChanged);
            // 
            // tileOption2
            // 
            this.tileOption2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tileOption2.AutoSize = true;
            this.tileOption2.Location = new System.Drawing.Point(134, 452);
            this.tileOption2.Name = "tileOption2";
            this.tileOption2.Size = new System.Drawing.Size(112, 17);
            this.tileOption2.TabIndex = 11;
            this.tileOption2.Text = "Solid (All excl. top)";
            this.tileOption2.UseVisualStyleBackColor = true;
            this.tileOption2.CheckedChanged += new System.EventHandler(this.tileOption2_CheckedChanged);
            // 
            // tileOption1
            // 
            this.tileOption1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tileOption1.AutoSize = true;
            this.tileOption1.Location = new System.Drawing.Point(12, 452);
            this.tileOption1.Name = "tileOption1";
            this.tileOption1.Size = new System.Drawing.Size(77, 17);
            this.tileOption1.TabIndex = 10;
            this.tileOption1.Text = "Solid (Top)";
            this.tileOption1.UseVisualStyleBackColor = true;
            this.tileOption1.CheckedChanged += new System.EventHandler(this.tileOption1_CheckedChanged);
            // 
            // option6CheckBox
            // 
            this.option6CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.option6CheckBox.Checked = global::ManiacEditor.Properties.Settings.Default.Unkown2Default;
            this.option6CheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ManiacEditor.Properties.Settings.Default, "Unkown2Default", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.option6CheckBox.Location = new System.Drawing.Point(134, 392);
            this.option6CheckBox.Name = "option6CheckBox";
            this.option6CheckBox.Size = new System.Drawing.Size(112, 31);
            this.option6CheckBox.TabIndex = 7;
            this.option6CheckBox.Text = "Solid (All excl. top) (Plane 2)";
            this.option6CheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.option6CheckBox.UseVisualStyleBackColor = true;
            // 
            // option5CheckBox
            // 
            this.option5CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.option5CheckBox.Checked = global::ManiacEditor.Properties.Settings.Default.Unkown1Default;
            this.option5CheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ManiacEditor.Properties.Settings.Default, "Unkown1Default", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.option5CheckBox.Location = new System.Drawing.Point(12, 394);
            this.option5CheckBox.Name = "option5CheckBox";
            this.option5CheckBox.Size = new System.Drawing.Size(77, 31);
            this.option5CheckBox.TabIndex = 6;
            this.option5CheckBox.Text = "Solid (Top) (Plane 2)";
            this.option5CheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.option5CheckBox.UseVisualStyleBackColor = true;
            // 
            // option4CheckBox
            // 
            this.option4CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.option4CheckBox.AutoSize = true;
            this.option4CheckBox.Checked = global::ManiacEditor.Properties.Settings.Default.SolidAllButTopDefault;
            this.option4CheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ManiacEditor.Properties.Settings.Default, "SolidAllButTopDefault", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.option4CheckBox.Location = new System.Drawing.Point(134, 369);
            this.option4CheckBox.Name = "option4CheckBox";
            this.option4CheckBox.Size = new System.Drawing.Size(112, 17);
            this.option4CheckBox.TabIndex = 5;
            this.option4CheckBox.Text = "Solid (All excl. top)";
            this.option4CheckBox.UseVisualStyleBackColor = true;
            // 
            // option3CheckBox
            // 
            this.option3CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.option3CheckBox.AutoSize = true;
            this.option3CheckBox.Checked = global::ManiacEditor.Properties.Settings.Default.SolidTopDefault;
            this.option3CheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ManiacEditor.Properties.Settings.Default, "SolidTopDefault", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.option3CheckBox.Location = new System.Drawing.Point(12, 371);
            this.option3CheckBox.Name = "option3CheckBox";
            this.option3CheckBox.Size = new System.Drawing.Size(77, 17);
            this.option3CheckBox.TabIndex = 4;
            this.option3CheckBox.Text = "Solid (Top)";
            this.option3CheckBox.UseVisualStyleBackColor = true;
            // 
            // tilesList
            // 
            this.tilesList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tilesList.BackColor = System.Drawing.Color.White;
            this.tilesList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tilesList.FlipHorizontal = false;
            this.tilesList.FlipVertical = false;
            this.tilesList.Location = new System.Drawing.Point(3, 73);
            this.tilesList.Name = "tilesList";
            this.tilesList.Size = new System.Drawing.Size(249, 267);
            this.tilesList.TabIndex = 0;
            this.tilesList.TileScale = 2;
            // 
            // selectedTileLabel
            // 
            this.selectedTileLabel.AutoSize = true;
            this.selectedTileLabel.Location = new System.Drawing.Point(3, 51);
            this.selectedTileLabel.Name = "selectedTileLabel";
            this.selectedTileLabel.Padding = new System.Windows.Forms.Padding(3);
            this.selectedTileLabel.Size = new System.Drawing.Size(112, 19);
            this.selectedTileLabel.TabIndex = 14;
            this.selectedTileLabel.Text = "Selected Tile: NULL ";
            // 
            // TilesToolbar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.selectedTileLabel);
            this.Controls.Add(this.tileOption4);
            this.Controls.Add(this.tileOption3);
            this.Controls.Add(this.tileOption2);
            this.Controls.Add(this.tileOption1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.option6CheckBox);
            this.Controls.Add(this.option5CheckBox);
            this.Controls.Add(this.option4CheckBox);
            this.Controls.Add(this.option3CheckBox);
            this.Controls.Add(this.option2CheckBox);
            this.Controls.Add(this.option1CheckBox);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.tilesList);
            this.Name = "TilesToolbar";
            this.Size = new System.Drawing.Size(255, 525);
            this.Load += new System.EventHandler(this.TilesToolbar_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TilesList tilesList;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.CheckBox option1CheckBox;
        private System.Windows.Forms.CheckBox option2CheckBox;
        private System.Windows.Forms.CheckBox option3CheckBox;
        private System.Windows.Forms.CheckBox option4CheckBox;
        private System.Windows.Forms.CheckBox option5CheckBox;
        private System.Windows.Forms.CheckBox option6CheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox tileOption4;
        private System.Windows.Forms.CheckBox tileOption3;
        private System.Windows.Forms.CheckBox tileOption2;
        private System.Windows.Forms.CheckBox tileOption1;
        private System.Windows.Forms.Label selectedTileLabel;
    }
}
