namespace UnBox3D.Views
{
    partial class Form1
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
            button1 = new Button();
            blenderDlProgress = new ProgressBar();
            downloadLabel = new Label();
            linkLabel1 = new LinkLabel();
            textBox1 = new TextBox();
            label1 = new Label();
            textBox2 = new TextBox();
            label2 = new Label();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            progressLabel = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(125, 155);
            button1.Name = "button1";
            button1.Size = new Size(92, 27);
            button1.TabIndex = 0;
            button1.Text = "Accept";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // blenderDlProgress
            // 
            blenderDlProgress.Location = new Point(127, 111);
            blenderDlProgress.Name = "blenderDlProgress";
            blenderDlProgress.Size = new Size(487, 23);
            blenderDlProgress.TabIndex = 1;
            // 
            // downloadLabel
            // 
            downloadLabel.AutoSize = true;
            downloadLabel.Location = new Point(127, 93);
            downloadLabel.Name = "downloadLabel";
            downloadLabel.Size = new Size(276, 15);
            downloadLabel.TabIndex = 2;
            downloadLabel.Text = "We need to download Blender. Please press accept.";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(37, 84);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(0, 15);
            linkLabel1.TabIndex = 3;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(127, 41);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(424, 23);
            textBox1.TabIndex = 4;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(127, 23);
            label1.Name = "label1";
            label1.Size = new Size(90, 15);
            label1.TabIndex = 5;
            label1.Text = "link (will delete)";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(117, 252);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(434, 23);
            textBox2.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(117, 234);
            label2.Name = "label2";
            label2.Size = new Size(52, 15);
            label2.TabIndex = 7;
            label2.Text = "File path";
            // 
            // backgroundWorker1
            // 
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            // 
            // progressLabel
            // 
            progressLabel.AutoSize = true;
            progressLabel.Location = new Point(127, 137);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(13, 15);
            progressLabel.TabIndex = 8;
            progressLabel.Text = "0";
            // 
            // Form1
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            ClientSize = new Size(725, 316);
            Controls.Add(progressLabel);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(linkLabel1);
            Controls.Add(downloadLabel);
            Controls.Add(blenderDlProgress);
            Controls.Add(button1);
            DoubleBuffered = true;
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private ProgressBar blenderDlProgress;
        private Label downloadLabel;
        private LinkLabel linkLabel1;
        private TextBox textBox1;
        private Label label1;
        private TextBox textBox2;
        private Label label2;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Label progressLabel;
    }
}