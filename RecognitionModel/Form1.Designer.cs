namespace RecognitionModel
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
            this.prepPic = new System.Windows.Forms.Button();
            this.btn_train = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // prepPic
            // 
            this.prepPic.Location = new System.Drawing.Point(765, 19);
            this.prepPic.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.prepPic.Name = "prepPic";
            this.prepPic.Size = new System.Drawing.Size(82, 25);
            this.prepPic.TabIndex = 0;
            this.prepPic.Text = "Prepair Pictures";
            this.prepPic.UseVisualStyleBackColor = true;
            this.prepPic.Click += new System.EventHandler(this.prepPic_Click);
            // 
            // btn_train
            // 
            this.btn_train.Location = new System.Drawing.Point(765, 66);
            this.btn_train.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btn_train.Name = "btn_train";
            this.btn_train.Size = new System.Drawing.Size(82, 23);
            this.btn_train.TabIndex = 1;
            this.btn_train.Text = "Train Model";
            this.btn_train.UseVisualStyleBackColor = true;
            this.btn_train.Click += new System.EventHandler(this.btn_train_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(27, 19);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(676, 472);
            //this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 550);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btn_train);
            this.Controls.Add(this.prepPic);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button prepPic;
        private System.Windows.Forms.Button btn_train;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

