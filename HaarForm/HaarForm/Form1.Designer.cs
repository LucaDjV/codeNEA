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
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // Loader
            // 
            this.Loader.Location = new System.Drawing.Point(45, 29);
            this.Loader.Name = "Loader";
            this.Loader.Size = new System.Drawing.Size(94, 29);
            this.Loader.TabIndex = 0;
            this.Loader.Text = "button1";
            this.Loader.UseVisualStyleBackColor = true;
            this.Loader.Click += new System.EventHandler(this.Loader_Click_1);
            // 
            // Back
            // 
            this.Back.Location = new System.Drawing.Point(45, 88);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(94, 29);
            this.Back.TabIndex = 1;
            this.Back.Text = "button2";
            this.Back.UseVisualStyleBackColor = true;
            this.Back.Click += new System.EventHandler(this.Back_Click_1);
            // 
            // ConvToInt
            // 
            this.ConvToInt.Location = new System.Drawing.Point(45, 144);
            this.ConvToInt.Name = "ConvToInt";
            this.ConvToInt.Size = new System.Drawing.Size(94, 29);
            this.ConvToInt.TabIndex = 2;
            this.ConvToInt.Text = "button3";
            this.ConvToInt.UseVisualStyleBackColor = true;
            this.ConvToInt.Click += new System.EventHandler(this.ConvToInt_Click_1);
            // 
            // startProcess
            // 
            this.startProcess.Location = new System.Drawing.Point(45, 199);
            this.startProcess.Name = "startProcess";
            this.startProcess.Size = new System.Drawing.Size(94, 29);
            this.startProcess.TabIndex = 3;
            this.startProcess.Text = "Start";
            this.startProcess.UseVisualStyleBackColor = true;
            this.startProcess.Click += new System.EventHandler(this.startProcess_Click_1);
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(177, 29);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(122, 105);
            this.pictureBox.TabIndex = 4;
            this.pictureBox.TabStop = false;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(25, 256);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(125, 120);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
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

        }

        #endregion

        private Button Loader;
        private Button Back;
        private Button ConvToInt;
        private Button startProcess;
        private PictureBox pictureBox;
        private RichTextBox richTextBox1;
    }
}