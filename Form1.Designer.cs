namespace CentralMine.NET
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTarget = new System.Windows.Forms.Label();
            this.lblHash1 = new System.Windows.Forms.Label();
            this.lblData = new System.Windows.Forms.Label();
            this.lblMidstate = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblClients = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblHashrate = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.protocol = new System.Windows.Forms.ComboBox();
            this.lblSubmitted = new System.Windows.Forms.Label();
            this.lblAccepted = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblTarget);
            this.groupBox1.Controls.Add(this.lblHash1);
            this.groupBox1.Controls.Add(this.lblData);
            this.groupBox1.Controls.Add(this.lblMidstate);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(943, 123);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Block Info";
            // 
            // lblTarget
            // 
            this.lblTarget.Location = new System.Drawing.Point(61, 95);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(876, 13);
            this.lblTarget.TabIndex = 7;
            this.lblTarget.Text = "label8";
            // 
            // lblHash1
            // 
            this.lblHash1.Location = new System.Drawing.Point(61, 75);
            this.lblHash1.Name = "lblHash1";
            this.lblHash1.Size = new System.Drawing.Size(876, 13);
            this.lblHash1.TabIndex = 6;
            this.lblHash1.Text = "label7";
            // 
            // lblData
            // 
            this.lblData.Location = new System.Drawing.Point(61, 40);
            this.lblData.Name = "lblData";
            this.lblData.Size = new System.Drawing.Size(876, 35);
            this.lblData.TabIndex = 5;
            this.lblData.Text = "lblData";
            // 
            // lblMidstate
            // 
            this.lblMidstate.Location = new System.Drawing.Point(61, 20);
            this.lblMidstate.Name = "lblMidstate";
            this.lblMidstate.Size = new System.Drawing.Size(876, 13);
            this.lblMidstate.TabIndex = 4;
            this.lblMidstate.Text = "lblMidstate";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "target:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "hash1:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "data:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "midstate:";
            // 
            // lblClients
            // 
            this.lblClients.AutoSize = true;
            this.lblClients.Location = new System.Drawing.Point(18, 138);
            this.lblClients.Name = "lblClients";
            this.lblClients.Size = new System.Drawing.Size(41, 13);
            this.lblClients.TabIndex = 1;
            this.lblClients.Text = "Clients:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(9, 182);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(946, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 2;
            // 
            // lblProgress
            // 
            this.lblProgress.Location = new System.Drawing.Point(9, 208);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(946, 20);
            this.lblProgress.TabIndex = 3;
            this.lblProgress.Text = "label5";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblHashrate
            // 
            this.lblHashrate.AutoSize = true;
            this.lblHashrate.Location = new System.Drawing.Point(18, 161);
            this.lblHashrate.Name = "lblHashrate";
            this.lblHashrate.Size = new System.Drawing.Size(53, 13);
            this.lblHashrate.TabIndex = 4;
            this.lblHashrate.Text = "Hashrate:";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // protocol
            // 
            this.protocol.FormattingEnabled = true;
            this.protocol.Location = new System.Drawing.Point(834, 138);
            this.protocol.Name = "protocol";
            this.protocol.Size = new System.Drawing.Size(121, 21);
            this.protocol.TabIndex = 5;
            this.protocol.SelectedIndexChanged += new System.EventHandler(this.protocol_SelectedIndexChanged);
            // 
            // lblSubmitted
            // 
            this.lblSubmitted.AutoSize = true;
            this.lblSubmitted.Location = new System.Drawing.Point(352, 141);
            this.lblSubmitted.Name = "lblSubmitted";
            this.lblSubmitted.Size = new System.Drawing.Size(41, 13);
            this.lblSubmitted.TabIndex = 6;
            this.lblSubmitted.Text = "Clients:";
            // 
            // lblAccepted
            // 
            this.lblAccepted.AutoSize = true;
            this.lblAccepted.Location = new System.Drawing.Point(352, 161);
            this.lblAccepted.Name = "lblAccepted";
            this.lblAccepted.Size = new System.Drawing.Size(41, 13);
            this.lblAccepted.TabIndex = 7;
            this.lblAccepted.Text = "Clients:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(967, 234);
            this.Controls.Add(this.lblAccepted);
            this.Controls.Add(this.lblSubmitted);
            this.Controls.Add(this.protocol);
            this.Controls.Add(this.lblHashrate);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblClients);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "CentralMine.NET";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Label lblHash1;
        private System.Windows.Forms.Label lblData;
        private System.Windows.Forms.Label lblMidstate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblClients;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblHashrate;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ComboBox protocol;
        private System.Windows.Forms.Label lblSubmitted;
        private System.Windows.Forms.Label lblAccepted;
    }
}

