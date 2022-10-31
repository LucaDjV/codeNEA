namespace HaarForm
{
    partial class Form1
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
            this.Loader = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.ConvToInt = new System.Windows.Forms.Button();
            this.startProcess = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox = new System.Windows.Forms.CheckBox();
            this.loadSave = new System.Windows.Forms.CheckBox();
            this.saveFile = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // Loader
            // 
            this.Loader.Location = new System.Drawing.Point(38, 29);
            this.Loader.Name = "Loader";
            this.Loader.Size = new System.Drawing.Size(101, 29);
            this.Loader.TabIndex = 0;
            this.Loader.Text = "Load Next";
            this.Loader.UseVisualStyleBackColor = true;
            this.Loader.Click += new System.EventHandler(this.Loader_Click_1);
            // 
            // Back
            // 
            this.Back.Location = new System.Drawing.Point(38, 88);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(101, 29);
            this.Back.TabIndex = 1;
            this.Back.Text = "Go Back";
            this.Back.UseVisualStyleBackColor = true;
            this.Back.Click += new System.EventHandler(this.Back_Click_1);
            // 
            // ConvToInt
            // 
            this.ConvToInt.Location = new System.Drawing.Point(38, 144);
            this.ConvToInt.Name = "ConvToInt";
            this.ConvToInt.Size = new System.Drawing.Size(101, 29);
            this.ConvToInt.TabIndex = 2;
            this.ConvToInt.Text = "To Int Img";
            this.ConvToInt.UseVisualStyleBackColor = true;
            this.ConvToInt.Click += new System.EventHandler(this.ConvToInt_Click_1);
            // 
            // startProcess
            // 
            this.startProcess.Location = new System.Drawing.Point(38, 194);
            this.startProcess.Name = "startProcess";
            this.startProcess.Size = new System.Drawing.Size(101, 29);
            this.startProcess.TabIndex = 3;
            this.startProcess.Text = "Start Model";
            this.startProcess.UseVisualStyleBackColor = true;
            this.startProcess.Click += new System.EventHandler(this.startProcess_Click_1);
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(263, 29);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(162, 167);
            this.pictureBox.TabIndex = 4;
            this.pictureBox.TabStop = false;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(34, 290);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(116, 120);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(157, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 29);
            this.button1.TabIndex = 6;
            this.button1.Text = "To Grayscale";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox
            // 
            this.checkBox.AutoSize = true;
            this.checkBox.Location = new System.Drawing.Point(34, 260);
            this.checkBox.Name = "checkBox";
            this.checkBox.Size = new System.Drawing.Size(116, 24);
            this.checkBox.TabIndex = 7;
            this.checkBox.Text = "Auto Process";
            this.checkBox.UseVisualStyleBackColor = true;
            // 
            // loadSave
            // 
            this.loadSave.AutoSize = true;
            this.loadSave.Location = new System.Drawing.Point(185, 263);
            this.loadSave.Name = "loadSave";
            this.loadSave.Size = new System.Drawing.Size(165, 24);
            this.loadSave.TabIndex = 8;
            this.loadSave.Text = "Load Previous Save?";
            this.loadSave.UseVisualStyleBackColor = true;
            this.loadSave.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // saveFile
            // 
            this.saveFile.Location = new System.Drawing.Point(183, 291);
            this.saveFile.Name = "saveFile";
            this.saveFile.Size = new System.Drawing.Size(125, 30);
            this.saveFile.TabIndex = 9;
            this.saveFile.Text = "";
            this.saveFile.Visible = false;
            this.saveFile.TextChanged += new System.EventHandler(this.richTextBox2_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 505);
            this.Controls.Add(this.saveFile);
            this.Controls.Add(this.loadSave);
            this.Controls.Add(this.checkBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.startProcess);
            this.Controls.Add(this.ConvToInt);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Loader);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button Loader;
        private Button Back;
        private Button ConvToInt;
        private Button startProcess;
        private PictureBox pictureBox;
        private RichTextBox richTextBox1;
        private Button button1;
        private CheckBox checkBox;
        private CheckBox loadSave;
        private RichTextBox saveFile;
    }
}