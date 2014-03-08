namespace BitCoinClient
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
            this.components = new System.ComponentModel.Container();
            this.lblThreads = new System.Windows.Forms.Label();
            this.lblHashrate = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lblThreads
            // 
            this.lblThreads.AutoSize = true;
            this.lblThreads.Location = new System.Drawing.Point(12, 9);
            this.lblThreads.Name = "lblThreads";
            this.lblThreads.Size = new System.Drawing.Size(49, 13);
            this.lblThreads.TabIndex = 0;
            this.lblThreads.Text = "Threads:";
            // 
            // lblHashrate
            // 
            this.lblHashrate.AutoSize = true;
            this.lblHashrate.Location = new System.Drawing.Point(12, 33);
            this.lblHashrate.Name = "lblHashrate";
            this.lblHashrate.Size = new System.Drawing.Size(53, 13);
            this.lblHashrate.TabIndex = 1;
            this.lblHashrate.Text = "Hashrate:";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 60);
            this.Controls.Add(this.lblHashrate);
            this.Controls.Add(this.lblThreads);
            this.Name = "Form1";
            this.Text = "Bitcoin Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblThreads;
        private System.Windows.Forms.Label lblHashrate;
        private System.Windows.Forms.Timer timer1;
    }
}

