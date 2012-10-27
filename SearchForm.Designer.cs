namespace CraigSearch
{
    partial class SearchForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchStartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.searchToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1059, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(102, 24);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.menuFileExit_Clicked);
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchSettingsToolStripMenuItem,
            this.searchStartToolStripMenuItem,
            this.searchStopToolStripMenuItem});
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(65, 24);
            this.searchToolStripMenuItem.Text = "Search";
            // 
            // searchSettingsToolStripMenuItem
            // 
            this.searchSettingsToolStripMenuItem.Name = "searchSettingsToolStripMenuItem";
            this.searchSettingsToolStripMenuItem.Size = new System.Drawing.Size(155, 24);
            this.searchSettingsToolStripMenuItem.Text = "Settings...";
            this.searchSettingsToolStripMenuItem.Click += new System.EventHandler(this.menuConfig_Clicked);
            // 
            // searchStartToolStripMenuItem
            // 
            this.searchStartToolStripMenuItem.Name = "searchStartToolStripMenuItem";
            this.searchStartToolStripMenuItem.Size = new System.Drawing.Size(155, 24);
            this.searchStartToolStripMenuItem.Text = "Start search";
            this.searchStartToolStripMenuItem.Click += new System.EventHandler(this.menuSearchStart_Clicked);
            // 
            // searchStopToolStripMenuItem
            // 
            this.searchStopToolStripMenuItem.Name = "searchStopToolStripMenuItem";
            this.searchStopToolStripMenuItem.Size = new System.Drawing.Size(155, 24);
            this.searchStopToolStripMenuItem.Text = "Stop search";
            this.searchStopToolStripMenuItem.Click += new System.EventHandler(this.menuSearchStop_Clicked);
            // 
            // webBrowser
            // 
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser.Location = new System.Drawing.Point(0, 28);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(1059, 529);
            this.webBrowser.TabIndex = 2;
            this.webBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
            // 
            // SearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 557);
            this.Controls.Add(this.webBrowser);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SearchForm";
            this.Text = "CraigSearch";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchStartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchStopToolStripMenuItem;
    }
}

