namespace ClientStream.Forms
{
    partial class RouterEdit
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
            this.routersBox = new System.Windows.Forms.ListBox();
            this.addBtn = new System.Windows.Forms.Button();
            this.routerTxt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // routersBox
            // 
            this.routersBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.routersBox.FormattingEnabled = true;
            this.routersBox.Location = new System.Drawing.Point(12, 10);
            this.routersBox.Name = "routersBox";
            this.routersBox.Size = new System.Drawing.Size(348, 238);
            this.routersBox.TabIndex = 0;
            // 
            // addBtn
            // 
            this.addBtn.Enabled = false;
            this.addBtn.Location = new System.Drawing.Point(12, 255);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(75, 23);
            this.addBtn.TabIndex = 1;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // routerTxt
            // 
            this.routerTxt.Location = new System.Drawing.Point(93, 257);
            this.routerTxt.Name = "routerTxt";
            this.routerTxt.Size = new System.Drawing.Size(267, 21);
            this.routerTxt.TabIndex = 2;
            this.routerTxt.TextChanged += new System.EventHandler(this.routerInput_TextChanged);
            // 
            // RouterEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 290);
            this.Controls.Add(this.routerTxt);
            this.Controls.Add(this.addBtn);
            this.Controls.Add(this.routersBox);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.MaximumSize = new System.Drawing.Size(388, 329);
            this.MinimumSize = new System.Drawing.Size(388, 329);
            this.Name = "RouterEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Routers";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox routersBox;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.TextBox routerTxt;
    }
}