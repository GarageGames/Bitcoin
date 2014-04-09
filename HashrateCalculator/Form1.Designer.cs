namespace HashrateCalculator
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
            this.Calculate = new System.Windows.Forms.Button();
            this.lblDifficulty = new System.Windows.Forms.Label();
            this.lblHashrate = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.CurrencyBox = new System.Windows.Forms.ComboBox();
            this.difficultyBox = new System.Windows.Forms.TextBox();
            this.hashrateBox = new System.Windows.Forms.TextBox();
            this.blocksPerDayBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Calculate
            // 
            this.Calculate.Location = new System.Drawing.Point(143, 300);
            this.Calculate.Name = "Calculate";
            this.Calculate.Size = new System.Drawing.Size(75, 23);
            this.Calculate.TabIndex = 4;
            this.Calculate.Text = "Calculate";
            this.Calculate.UseVisualStyleBackColor = true;
            this.Calculate.Click += new System.EventHandler(this.Calculate_Click);
            // 
            // lblDifficulty
            // 
            this.lblDifficulty.AutoSize = true;
            this.lblDifficulty.Location = new System.Drawing.Point(437, 30);
            this.lblDifficulty.Name = "lblDifficulty";
            this.lblDifficulty.Size = new System.Drawing.Size(50, 13);
            this.lblDifficulty.TabIndex = 5;
            this.lblDifficulty.Text = "Difficulty:";
            // 
            // lblHashrate
            // 
            this.lblHashrate.AutoSize = true;
            this.lblHashrate.Location = new System.Drawing.Point(416, 56);
            this.lblHashrate.Name = "lblHashrate";
            this.lblHashrate.Size = new System.Drawing.Size(71, 13);
            this.lblHashrate.TabIndex = 6;
            this.lblHashrate.Text = "Est Hashrate:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Currency";
            // 
            // CurrencyBox
            // 
            this.CurrencyBox.FormattingEnabled = true;
            this.CurrencyBox.Location = new System.Drawing.Point(86, 30);
            this.CurrencyBox.Name = "CurrencyBox";
            this.CurrencyBox.Size = new System.Drawing.Size(230, 21);
            this.CurrencyBox.TabIndex = 8;
            // 
            // difficultyBox
            // 
            this.difficultyBox.Location = new System.Drawing.Point(493, 23);
            this.difficultyBox.Name = "difficultyBox";
            this.difficultyBox.ReadOnly = true;
            this.difficultyBox.Size = new System.Drawing.Size(316, 20);
            this.difficultyBox.TabIndex = 9;
            // 
            // hashrateBox
            // 
            this.hashrateBox.Location = new System.Drawing.Point(493, 49);
            this.hashrateBox.Name = "hashrateBox";
            this.hashrateBox.ReadOnly = true;
            this.hashrateBox.Size = new System.Drawing.Size(316, 20);
            this.hashrateBox.TabIndex = 10;
            // 
            // blocksPerDayBox
            // 
            this.blocksPerDayBox.Location = new System.Drawing.Point(493, 75);
            this.blocksPerDayBox.Name = "blocksPerDayBox";
            this.blocksPerDayBox.ReadOnly = true;
            this.blocksPerDayBox.Size = new System.Drawing.Size(316, 20);
            this.blocksPerDayBox.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(404, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Blocks Per Day:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(966, 335);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.blocksPerDayBox);
            this.Controls.Add(this.hashrateBox);
            this.Controls.Add(this.difficultyBox);
            this.Controls.Add(this.CurrencyBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblHashrate);
            this.Controls.Add(this.lblDifficulty);
            this.Controls.Add(this.Calculate);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Calculate;
        private System.Windows.Forms.Label lblDifficulty;
        private System.Windows.Forms.Label lblHashrate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CurrencyBox;
        private System.Windows.Forms.TextBox difficultyBox;
        private System.Windows.Forms.TextBox hashrateBox;
        private System.Windows.Forms.TextBox blocksPerDayBox;
        private System.Windows.Forms.Label label2;
    }
}

