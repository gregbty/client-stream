namespace ClientStream.Forms
{
    partial class MainForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.routerBtn = new System.Windows.Forms.Button();
            this.clientBox = new System.Windows.Forms.RadioButton();
            this.routerBox = new System.Windows.Forms.RadioButton();
            this.startBtn = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.outputTxt = new System.Windows.Forms.RichTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.addressLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.downloadProgressLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.downloadProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.routerBtn);
            this.groupBox1.Controls.Add(this.clientBox);
            this.groupBox1.Controls.Add(this.routerBox);
            this.groupBox1.Controls.Add(this.startBtn);
            this.groupBox1.Location = new System.Drawing.Point(12, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(413, 99);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Endpoint Type";
            // 
            // routerBtn
            // 
            this.routerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.routerBtn.Enabled = false;
            this.routerBtn.Location = new System.Drawing.Point(217, 70);
            this.routerBtn.Name = "routerBtn";
            this.routerBtn.Size = new System.Drawing.Size(92, 23);
            this.routerBtn.TabIndex = 3;
            this.routerBtn.Text = "Routers...";
            this.routerBtn.UseVisualStyleBackColor = true;
            this.routerBtn.Visible = false;
            this.routerBtn.Click += new System.EventHandler(this.routerBtn_Click);
            // 
            // clientBox
            // 
            this.clientBox.AutoSize = true;
            this.clientBox.Location = new System.Drawing.Point(18, 49);
            this.clientBox.Name = "clientBox";
            this.clientBox.Size = new System.Drawing.Size(52, 17);
            this.clientBox.TabIndex = 2;
            this.clientBox.TabStop = true;
            this.clientBox.Text = "Client";
            this.clientBox.UseVisualStyleBackColor = true;
            // 
            // routerBox
            // 
            this.routerBox.AutoSize = true;
            this.routerBox.Checked = true;
            this.routerBox.Location = new System.Drawing.Point(18, 26);
            this.routerBox.Name = "routerBox";
            this.routerBox.Size = new System.Drawing.Size(58, 17);
            this.routerBox.TabIndex = 1;
            this.routerBox.TabStop = true;
            this.routerBox.Text = "Router";
            this.routerBox.UseVisualStyleBackColor = true;
            // 
            // startBtn
            // 
            this.startBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.startBtn.Location = new System.Drawing.Point(315, 70);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(92, 23);
            this.startBtn.TabIndex = 0;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.outputTxt);
            this.groupBox2.Location = new System.Drawing.Point(12, 113);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(413, 236);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output";
            // 
            // outputTxt
            // 
            this.outputTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTxt.Location = new System.Drawing.Point(6, 19);
            this.outputTxt.Name = "outputTxt";
            this.outputTxt.Size = new System.Drawing.Size(401, 211);
            this.outputTxt.TabIndex = 0;
            this.outputTxt.Text = "";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addressLbl,
            this.downloadProgressLbl,
            this.downloadProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 363);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(437, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // addressLbl
            // 
            this.addressLbl.Name = "addressLbl";
            this.addressLbl.Size = new System.Drawing.Size(113, 17);
            this.addressLbl.Text = "IP Address: 127.0.0.1";
            // 
            // downloadProgressLbl
            // 
            this.downloadProgressLbl.Name = "downloadProgressLbl";
            this.downloadProgressLbl.Size = new System.Drawing.Size(112, 17);
            this.downloadProgressLbl.Text = "Download Progress:";
            // 
            // downloadProgress
            // 
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(100, 16);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 385);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ClientStream";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox outputTxt;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.RadioButton clientBox;
        private System.Windows.Forms.RadioButton routerBox;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel downloadProgressLbl;
        private System.Windows.Forms.ToolStripProgressBar downloadProgress;
        private System.Windows.Forms.ToolStripStatusLabel addressLbl;
        private System.Windows.Forms.Button routerBtn;
    }
}

