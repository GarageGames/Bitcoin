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
            this.currencyList = new System.Windows.Forms.ListView();
            this.columnHeader_Name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_Symbol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_USDPerDay = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_USDPerCoin = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_BTCPerCoin = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_Hashrate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_BlockTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_BlocksPerDay = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_CoinsPerBlock = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.hashrate = new System.Windows.Forms.TextBox();
            this.columnHeader_Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // currencyList
            // 
            this.currencyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_Name,
            this.columnHeader_Symbol,
            this.columnHeader_USDPerDay,
            this.columnHeader_USDPerCoin,
            this.columnHeader_BTCPerCoin,
            this.columnHeader_Hashrate,
            this.columnHeader_BlockTime,
            this.columnHeader_BlocksPerDay,
            this.columnHeader_CoinsPerBlock,
            this.columnHeader_Status});
            this.currencyList.FullRowSelect = true;
            this.currencyList.GridLines = true;
            this.currencyList.Location = new System.Drawing.Point(12, 44);
            this.currencyList.Name = "currencyList";
            this.currencyList.Size = new System.Drawing.Size(1294, 384);
            this.currencyList.TabIndex = 0;
            this.currencyList.UseCompatibleStateImageBehavior = false;
            this.currencyList.View = System.Windows.Forms.View.Details;
            this.currencyList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.currencyList_MouseDoubleClick);
            // 
            // columnHeader_Name
            // 
            this.columnHeader_Name.Text = "Name";
            this.columnHeader_Name.Width = 100;
            // 
            // columnHeader_Symbol
            // 
            this.columnHeader_Symbol.Text = "Symbol";
            this.columnHeader_Symbol.Width = 48;
            // 
            // columnHeader_USDPerDay
            // 
            this.columnHeader_USDPerDay.Text = "$/day";
            this.columnHeader_USDPerDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_USDPerDay.Width = 115;
            // 
            // columnHeader_USDPerCoin
            // 
            this.columnHeader_USDPerCoin.Text = "$ value";
            this.columnHeader_USDPerCoin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_USDPerCoin.Width = 145;
            // 
            // columnHeader_BTCPerCoin
            // 
            this.columnHeader_BTCPerCoin.Text = "BTC Value";
            this.columnHeader_BTCPerCoin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_BTCPerCoin.Width = 170;
            // 
            // columnHeader_Hashrate
            // 
            this.columnHeader_Hashrate.Text = "Hashrate";
            this.columnHeader_Hashrate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_Hashrate.Width = 100;
            // 
            // columnHeader_BlockTime
            // 
            this.columnHeader_BlockTime.Text = "BlockTime";
            this.columnHeader_BlockTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_BlockTime.Width = 99;
            // 
            // columnHeader_BlocksPerDay
            // 
            this.columnHeader_BlocksPerDay.Text = "Blocks/Day";
            this.columnHeader_BlocksPerDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_BlocksPerDay.Width = 112;
            // 
            // columnHeader_CoinsPerBlock
            // 
            this.columnHeader_CoinsPerBlock.Text = "Coins/Block";
            this.columnHeader_CoinsPerBlock.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_CoinsPerBlock.Width = 114;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Hashrate";
            // 
            // hashrate
            // 
            this.hashrate.Location = new System.Drawing.Point(68, 12);
            this.hashrate.Name = "hashrate";
            this.hashrate.Size = new System.Drawing.Size(169, 20);
            this.hashrate.TabIndex = 2;
            this.hashrate.Text = "225000";
            // 
            // columnHeader_Status
            // 
            this.columnHeader_Status.Text = "Status";
            this.columnHeader_Status.Width = 200;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1318, 440);
            this.Controls.Add(this.hashrate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.currencyList);
            this.Name = "Form1";
            this.Text = "Coin Values";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView currencyList;
        private System.Windows.Forms.ColumnHeader columnHeader_Name;
        private System.Windows.Forms.ColumnHeader columnHeader_Symbol;
        private System.Windows.Forms.ColumnHeader columnHeader_USDPerDay;
        private System.Windows.Forms.ColumnHeader columnHeader_USDPerCoin;
        private System.Windows.Forms.ColumnHeader columnHeader_BTCPerCoin;
        private System.Windows.Forms.ColumnHeader columnHeader_Hashrate;
        private System.Windows.Forms.ColumnHeader columnHeader_BlockTime;
        private System.Windows.Forms.ColumnHeader columnHeader_BlocksPerDay;
        private System.Windows.Forms.ColumnHeader columnHeader_CoinsPerBlock;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox hashrate;
        private System.Windows.Forms.ColumnHeader columnHeader_Status;

    }
}

