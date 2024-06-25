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
            btn_cargarImagen = new Button();
            pictureBox1 = new PictureBox();
            btn_analisis = new Button();
            pictureBox2 = new PictureBox();
            btn_cargarVideo = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // btn_cargarImagen
            // 
            btn_cargarImagen.Location = new Point(843, 1069);
            btn_cargarImagen.Name = "btn_cargarImagen";
            btn_cargarImagen.Size = new Size(203, 34);
            btn_cargarImagen.TabIndex = 0;
            btn_cargarImagen.Text = "Cargar Imagen";
            btn_cargarImagen.UseVisualStyleBackColor = true;
            btn_cargarImagen.Click += btn_cargarImagen_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(2, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1200, 800);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // btn_analisis
            // 
            btn_analisis.Location = new Point(1069, 1069);
            btn_analisis.Name = "btn_analisis";
            btn_analisis.Size = new Size(112, 34);
            btn_analisis.TabIndex = 2;
            btn_analisis.Text = "Análisis";
            btn_analisis.UseVisualStyleBackColor = true;
            btn_analisis.Click += btn_analisis_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(1205, 2);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(1200, 800);
            pictureBox2.TabIndex = 3;
            pictureBox2.TabStop = false;
            // 
            // btn_cargarVideo
            // 
            btn_cargarVideo.Location = new Point(581, 1069);
            btn_cargarVideo.Name = "btn_cargarVideo";
            btn_cargarVideo.Size = new Size(203, 34);
            btn_cargarVideo.TabIndex = 4;
            btn_cargarVideo.Text = "Cargar Video";
            btn_cargarVideo.UseVisualStyleBackColor = true;
            btn_cargarVideo.Click += btn_cargarVideo_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2478, 1144);
            Controls.Add(btn_cargarVideo);
            Controls.Add(pictureBox2);
            Controls.Add(btn_analisis);
            Controls.Add(pictureBox1);
            Controls.Add(btn_cargarImagen);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btn_cargarImagen;
        private PictureBox pictureBox1;
        private Button btn_analisis;
        private PictureBox pictureBox2;
        private Button btn_cargarVideo;
    }
}
