
namespace vnebo.mobi.bot
{
    partial class AuthForm
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
            this.webAuth = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webAuth
            // 
            this.webAuth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webAuth.Location = new System.Drawing.Point(0, 0);
            this.webAuth.MinimumSize = new System.Drawing.Size(20, 20);
            this.webAuth.Name = "webAuth";
            this.webAuth.Size = new System.Drawing.Size(800, 450);
            this.webAuth.TabIndex = 0;
            // 
            // Auth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.webAuth);
            this.Name = "Auth";
            this.Text = "Auth";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.WebBrowser webAuth;
    }
}