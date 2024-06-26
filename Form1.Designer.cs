namespace Analitico2_Elementia
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
            components = new System.ComponentModel.Container();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            btn_anaVideo = new Button();
            btn_slowVideo = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            timer2 = new System.Windows.Forms.Timer(components);
            pictureBox3 = new PictureBox();
            btn_OpenCamara = new Button();
            btn_stop = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(12, 93);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1200, 800);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(1218, 93);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(1200, 800);
            pictureBox2.TabIndex = 3;
            pictureBox2.TabStop = false;
            // 
            // btn_anaVideo
            // 
            btn_anaVideo.BackColor = Color.FromArgb(56, 56, 56);
            btn_anaVideo.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_anaVideo.ForeColor = Color.White;
            btn_anaVideo.Location = new Point(436, 947);
            btn_anaVideo.Name = "btn_anaVideo";
            btn_anaVideo.Size = new Size(315, 61);
            btn_anaVideo.TabIndex = 4;
            btn_anaVideo.Text = "Análisis ";
            btn_anaVideo.UseVisualStyleBackColor = false;
            btn_anaVideo.Click += Btn_anaVideo_Click;
            // 
            // btn_slowVideo
            // 
            btn_slowVideo.BackColor = Color.FromArgb(56, 56, 56);
            btn_slowVideo.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_slowVideo.ForeColor = Color.White;
            btn_slowVideo.Location = new Point(1790, 947);
            btn_slowVideo.Name = "btn_slowVideo";
            btn_slowVideo.Size = new Size(315, 61);
            btn_slowVideo.TabIndex = 5;
            btn_slowVideo.Text = "Abrir Video ";
            btn_slowVideo.UseVisualStyleBackColor = false;
            btn_slowVideo.Click += Btn_slowVideo_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 33;
            timer1.Tick += Timer1_Tick;
            // 
            // timer2
            // 
            timer2.Enabled = true;
            timer2.Interval = 33;
            timer2.Tick += Timer2_Tick;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = Properties.Resources.ditran;
            pictureBox3.Location = new Point(12, 12);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(313, 75);
            pictureBox3.TabIndex = 6;
            pictureBox3.TabStop = false;
            // 
            // btn_OpenCamara
            // 
            btn_OpenCamara.BackColor = Color.FromArgb(56, 56, 56);
            btn_OpenCamara.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_OpenCamara.ForeColor = Color.White;
            btn_OpenCamara.Location = new Point(62, 945);
            btn_OpenCamara.Name = "btn_OpenCamara";
            btn_OpenCamara.Size = new Size(315, 61);
            btn_OpenCamara.TabIndex = 7;
            btn_OpenCamara.Text = "Abrir Cámara";
            btn_OpenCamara.UseVisualStyleBackColor = false;
            btn_OpenCamara.Click += Btn_OpenCamara_Click;
            // 
            // btn_stop
            // 
            btn_stop.BackColor = Color.FromArgb(56, 56, 56);
            btn_stop.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_stop.ForeColor = Color.White;
            btn_stop.Location = new Point(1027, 947);
            btn_stop.Name = "btn_stop";
            btn_stop.Size = new Size(315, 61);
            btn_stop.TabIndex = 8;
            btn_stop.Text = "Paro";
            btn_stop.UseVisualStyleBackColor = false;
            btn_stop.Click += Btn_stop_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(37, 37, 38);
            ClientSize = new Size(2478, 1144);
            Controls.Add(btn_stop);
            Controls.Add(btn_OpenCamara);
            Controls.Add(pictureBox3);
            Controls.Add(btn_slowVideo);
            Controls.Add(btn_anaVideo);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Button btn_anaVideo;
        private Button btn_slowVideo;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private PictureBox pictureBox3;
        private Button btn_OpenCamara;
        private Button btn_stop;
    }
}
