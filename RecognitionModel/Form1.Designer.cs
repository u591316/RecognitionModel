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
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // prepPic
            // 
            this.prepPic.Location = new System.Drawing.Point(560, 23);
            this.prepPic.Name = "prepPic";
            this.prepPic.Size = new System.Drawing.Size(110, 31);
            this.prepPic.TabIndex = 0;
            this.prepPic.Text = "Prepair Pictures";
            this.prepPic.UseVisualStyleBackColor = true;
            this.prepPic.Click += new System.EventHandler(this.prepPic_Click);
            // 
            // btn_train
            // 
            this.btn_train.Location = new System.Drawing.Point(560, 69);
            this.btn_train.Name = "btn_train";
            this.btn_train.Size = new System.Drawing.Size(110, 28);
            this.btn_train.TabIndex = 1;
            this.btn_train.Text = "Train Model";
            this.btn_train.UseVisualStyleBackColor = true;
            this.btn_train.Click += new System.EventHandler(this.btn_train_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(560, 120);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.PredictLabels_btn);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_train);
            this.Controls.Add(this.prepPic);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button prepPic;
        private System.Windows.Forms.Button btn_train;
        private System.Windows.Forms.Button button1;
    }
}

