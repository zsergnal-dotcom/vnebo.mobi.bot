
namespace vnebo.mobi.bot
{
    partial class Web
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Web));
            this.WebB = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // WebB
            // 
            this.WebB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WebB.Location = new System.Drawing.Point(0, 0);
            this.WebB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.WebB.MinimumSize = new System.Drawing.Size(27, 25);
            this.WebB.Name = "WebB";
            this.WebB.Size = new System.Drawing.Size(488, 862);
            this.WebB.TabIndex = 0;
            this.WebB.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.webBrowser1_Navigating);
            // 
            // Web
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 862);
            this.Controls.Add(this.WebB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Web";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Web";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Web_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser WebB;
    }
}