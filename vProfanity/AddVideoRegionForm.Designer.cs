namespace vProfanity
{
    partial class AddVideoRegionForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.videoDurationLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.secondCaptionLabel = new System.Windows.Forms.Label();
            this.endTextBox = new System.Windows.Forms.TextBox();
            this.endLabel = new System.Windows.Forms.Label();
            this.startTextBox = new System.Windows.Forms.TextBox();
            this.startLabel = new System.Windows.Forms.Label();
            this.rangeCaptionLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.secondTextBox = new System.Windows.Forms.TextBox();
            this.secondLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(16, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(62, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Second";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(84, 19);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(57, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Range";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.videoDurationLabel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.secondCaptionLabel);
            this.groupBox1.Controls.Add(this.endTextBox);
            this.groupBox1.Controls.Add(this.endLabel);
            this.groupBox1.Controls.Add(this.startTextBox);
            this.groupBox1.Controls.Add(this.startLabel);
            this.groupBox1.Controls.Add(this.rangeCaptionLabel);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.secondTextBox);
            this.groupBox1.Controls.Add(this.secondLabel);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(353, 167);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "";
            // 
            // videoDurationLabel
            // 
            this.videoDurationLabel.AutoSize = true;
            this.videoDurationLabel.Location = new System.Drawing.Point(275, 23);
            this.videoDurationLabel.Name = "videoDurationLabel";
            this.videoDurationLabel.Size = new System.Drawing.Size(0, 13);
            this.videoDurationLabel.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(147, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Video Duration (seconds): ";
            // 
            // secondCaptionLabel
            // 
            this.secondCaptionLabel.AutoSize = true;
            this.secondCaptionLabel.Location = new System.Drawing.Point(21, 48);
            this.secondCaptionLabel.Name = "secondCaptionLabel";
            this.secondCaptionLabel.Size = new System.Drawing.Size(91, 13);
            this.secondCaptionLabel.TabIndex = 10;
            this.secondCaptionLabel.Text = "Enter the second:";
            // 
            // endTextBox
            // 
            this.endTextBox.Location = new System.Drawing.Point(210, 77);
            this.endTextBox.Name = "endTextBox";
            this.endTextBox.Size = new System.Drawing.Size(100, 20);
            this.endTextBox.TabIndex = 9;
            this.endTextBox.Visible = false;
            // 
            // endLabel
            // 
            this.endLabel.AutoSize = true;
            this.endLabel.Location = new System.Drawing.Point(175, 80);
            this.endLabel.Name = "endLabel";
            this.endLabel.Size = new System.Drawing.Size(29, 13);
            this.endLabel.TabIndex = 8;
            this.endLabel.Text = "End:";
            this.endLabel.Visible = false;
            // 
            // startTextBox
            // 
            this.startTextBox.Location = new System.Drawing.Point(59, 77);
            this.startTextBox.Name = "startTextBox";
            this.startTextBox.Size = new System.Drawing.Size(100, 20);
            this.startTextBox.TabIndex = 7;
            this.startTextBox.Visible = false;
            // 
            // startLabel
            // 
            this.startLabel.AutoSize = true;
            this.startLabel.Location = new System.Drawing.Point(21, 80);
            this.startLabel.Name = "startLabel";
            this.startLabel.Size = new System.Drawing.Size(32, 13);
            this.startLabel.TabIndex = 6;
            this.startLabel.Text = "Start:";
            this.startLabel.Visible = false;
            // 
            // rangeCaptionLabel
            // 
            this.rangeCaptionLabel.AutoSize = true;
            this.rangeCaptionLabel.Location = new System.Drawing.Point(18, 48);
            this.rangeCaptionLabel.Name = "rangeCaptionLabel";
            this.rangeCaptionLabel.Size = new System.Drawing.Size(148, 13);
            this.rangeCaptionLabel.TabIndex = 5;
            this.rangeCaptionLabel.Text = "Enter the start and end range:";
            this.rangeCaptionLabel.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(235, 128);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Add";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // secondTextBox
            // 
            this.secondTextBox.Location = new System.Drawing.Point(84, 77);
            this.secondTextBox.Name = "secondTextBox";
            this.secondTextBox.Size = new System.Drawing.Size(100, 20);
            this.secondTextBox.TabIndex = 3;
            // 
            // secondLabel
            // 
            this.secondLabel.AutoSize = true;
            this.secondLabel.Location = new System.Drawing.Point(21, 80);
            this.secondLabel.Name = "secondLabel";
            this.secondLabel.Size = new System.Drawing.Size(47, 13);
            this.secondLabel.TabIndex = 2;
            this.secondLabel.Text = "Second:";
            // 
            // AddVideoRegionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(377, 191);
            this.Controls.Add(this.groupBox1);
            this.Name = "AddVideoRegionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "vProfanity";
            this.Load += new System.EventHandler(this.AddVideoRegionForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label secondLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox secondTextBox;
        private System.Windows.Forms.Label secondCaptionLabel;
        private System.Windows.Forms.TextBox endTextBox;
        private System.Windows.Forms.Label endLabel;
        private System.Windows.Forms.TextBox startTextBox;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.Label rangeCaptionLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label videoDurationLabel;
    }
}