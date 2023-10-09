
namespace DanTup.BrowserSelector
{
    partial class frmNag
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNag));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.cmdRegBrowser = new System.Windows.Forms.Button();
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdUnreg = new System.Windows.Forms.Button();
            this.cmdCheckSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(480, 243);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // cmdRegBrowser
            // 
            this.cmdRegBrowser.Location = new System.Drawing.Point(12, 261);
            this.cmdRegBrowser.Name = "cmdRegBrowser";
            this.cmdRegBrowser.Size = new System.Drawing.Size(156, 31);
            this.cmdRegBrowser.TabIndex = 1;
            this.cmdRegBrowser.Text = "Register as browser";
            this.cmdRegBrowser.UseVisualStyleBackColor = true;
            this.cmdRegBrowser.Click += new System.EventHandler(this.cmdRegBrowser_Click);
            // 
            // cmdClose
            // 
            this.cmdClose.Location = new System.Drawing.Point(336, 298);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(156, 31);
            this.cmdClose.TabIndex = 0;
            this.cmdClose.Text = "Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // cmdUnreg
            // 
            this.cmdUnreg.Location = new System.Drawing.Point(12, 298);
            this.cmdUnreg.Name = "cmdUnreg";
            this.cmdUnreg.Size = new System.Drawing.Size(156, 31);
            this.cmdUnreg.TabIndex = 2;
            this.cmdUnreg.Text = "Unregister as browser";
            this.cmdUnreg.UseVisualStyleBackColor = true;
            this.cmdUnreg.Click += new System.EventHandler(this.cmdUnreg_Click);
            // 
            // cmdCheckSettings
            // 
            this.cmdCheckSettings.Location = new System.Drawing.Point(174, 261);
            this.cmdCheckSettings.Name = "cmdCheckSettings";
            this.cmdCheckSettings.Size = new System.Drawing.Size(156, 31);
            this.cmdCheckSettings.TabIndex = 3;
            this.cmdCheckSettings.Text = "Check settings";
            this.cmdCheckSettings.UseVisualStyleBackColor = true;
            this.cmdCheckSettings.Click += new System.EventHandler(this.cmdCheckSettings_Click);
            // 
            // frmNag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 334);
            this.Controls.Add(this.cmdCheckSettings);
            this.Controls.Add(this.cmdUnreg);
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.cmdRegBrowser);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNag";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Browser Selector";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button cmdRegBrowser;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button cmdUnreg;
        private System.Windows.Forms.Button cmdCheckSettings;
    }
}