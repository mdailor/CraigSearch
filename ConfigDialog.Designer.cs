namespace CraigSearch
{
    partial class ConfigDialog
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
            this.btnConfigSave = new System.Windows.Forms.Button();
            this.btnConfigCancel = new System.Windows.Forms.Button();
            this.lblSearchFor = new System.Windows.Forms.Label();
            this.tbKeywords = new System.Windows.Forms.TextBox();
            this.lblIn = new System.Windows.Forms.Label();
            this.lblSearchIn = new System.Windows.Forms.Label();
            this.rbSearchTitle = new System.Windows.Forms.RadioButton();
            this.rbSearchPost = new System.Windows.Forms.RadioButton();
            this.lblShowOnly = new System.Windows.Forms.Label();
            this.cbPartTime = new System.Windows.Forms.CheckBox();
            this.cbNonprofit = new System.Windows.Forms.CheckBox();
            this.cbHasImage = new System.Windows.Forms.CheckBox();
            this.cbCategories = new System.Windows.Forms.ComboBox();
            this.cbSubcategories = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbContract = new System.Windows.Forms.CheckBox();
            this.cbInternship = new System.Windows.Forms.CheckBox();
            this.cbTelecommute = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConfigSave
            // 
            this.btnConfigSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConfigSave.Location = new System.Drawing.Point(12, 146);
            this.btnConfigSave.Name = "btnConfigSave";
            this.btnConfigSave.Size = new System.Drawing.Size(75, 29);
            this.btnConfigSave.TabIndex = 0;
            this.btnConfigSave.Text = "Ok";
            this.btnConfigSave.UseVisualStyleBackColor = true;
            this.btnConfigSave.Click += new System.EventHandler(this.btnConfigSave_Click);
            // 
            // btnConfigCancel
            // 
            this.btnConfigCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnConfigCancel.Location = new System.Drawing.Point(93, 146);
            this.btnConfigCancel.Name = "btnConfigCancel";
            this.btnConfigCancel.Size = new System.Drawing.Size(75, 29);
            this.btnConfigCancel.TabIndex = 1;
            this.btnConfigCancel.Text = "Cancel";
            this.btnConfigCancel.UseVisualStyleBackColor = true;
            this.btnConfigCancel.Click += new System.EventHandler(this.btnConfigCancel_Click);
            // 
            // lblSearchFor
            // 
            this.lblSearchFor.AutoSize = true;
            this.lblSearchFor.Location = new System.Drawing.Point(15, 15);
            this.lblSearchFor.Name = "lblSearchFor";
            this.lblSearchFor.Size = new System.Drawing.Size(78, 17);
            this.lblSearchFor.TabIndex = 2;
            this.lblSearchFor.Text = "Search for:";
            // 
            // tbKeywords
            // 
            this.tbKeywords.Location = new System.Drawing.Point(96, 14);
            this.tbKeywords.Name = "tbKeywords";
            this.tbKeywords.Size = new System.Drawing.Size(151, 22);
            this.tbKeywords.TabIndex = 3;
            // 
            // lblIn
            // 
            this.lblIn.AutoSize = true;
            this.lblIn.Location = new System.Drawing.Point(248, 17);
            this.lblIn.Name = "lblIn";
            this.lblIn.Size = new System.Drawing.Size(23, 17);
            this.lblIn.TabIndex = 4;
            this.lblIn.Text = "in:";
            // 
            // lblSearchIn
            // 
            this.lblSearchIn.AutoSize = true;
            this.lblSearchIn.Location = new System.Drawing.Point(5, 4);
            this.lblSearchIn.Name = "lblSearchIn";
            this.lblSearchIn.Size = new System.Drawing.Size(72, 17);
            this.lblSearchIn.TabIndex = 6;
            this.lblSearchIn.Text = "Search in:";
            // 
            // rbSearchTitle
            // 
            this.rbSearchTitle.AutoSize = true;
            this.rbSearchTitle.Location = new System.Drawing.Point(80, 4);
            this.rbSearchTitle.Name = "rbSearchTitle";
            this.rbSearchTitle.Size = new System.Drawing.Size(86, 21);
            this.rbSearchTitle.TabIndex = 7;
            this.rbSearchTitle.TabStop = true;
            this.rbSearchTitle.Text = "Title only";
            this.rbSearchTitle.UseVisualStyleBackColor = true;
            // 
            // rbSearchPost
            // 
            this.rbSearchPost.AutoSize = true;
            this.rbSearchPost.Location = new System.Drawing.Point(174, 4);
            this.rbSearchPost.Name = "rbSearchPost";
            this.rbSearchPost.Size = new System.Drawing.Size(97, 21);
            this.rbSearchPost.TabIndex = 8;
            this.rbSearchPost.TabStop = true;
            this.rbSearchPost.Text = "Entire post";
            this.rbSearchPost.UseVisualStyleBackColor = true;
            // 
            // lblShowOnly
            // 
            this.lblShowOnly.AutoSize = true;
            this.lblShowOnly.Location = new System.Drawing.Point(18, 80);
            this.lblShowOnly.Name = "lblShowOnly";
            this.lblShowOnly.Size = new System.Drawing.Size(76, 17);
            this.lblShowOnly.TabIndex = 9;
            this.lblShowOnly.Text = "Show only:";
            // 
            // cbPartTime
            // 
            this.cbPartTime.AutoSize = true;
            this.cbPartTime.Location = new System.Drawing.Point(335, 107);
            this.cbPartTime.Name = "cbPartTime";
            this.cbPartTime.Size = new System.Drawing.Size(86, 21);
            this.cbPartTime.TabIndex = 13;
            this.cbPartTime.Text = "Part time";
            this.cbPartTime.UseVisualStyleBackColor = true;
            // 
            // cbNonprofit
            // 
            this.cbNonprofit.AutoSize = true;
            this.cbNonprofit.Location = new System.Drawing.Point(96, 107);
            this.cbNonprofit.Name = "cbNonprofit";
            this.cbNonprofit.Size = new System.Drawing.Size(88, 21);
            this.cbNonprofit.TabIndex = 14;
            this.cbNonprofit.Text = "Nonprofit";
            this.cbNonprofit.UseVisualStyleBackColor = true;
            // 
            // cbHasImage
            // 
            this.cbHasImage.AutoSize = true;
            this.cbHasImage.Location = new System.Drawing.Point(96, 80);
            this.cbHasImage.Name = "cbHasImage";
            this.cbHasImage.Size = new System.Drawing.Size(97, 21);
            this.cbHasImage.TabIndex = 15;
            this.cbHasImage.Text = "Has image";
            this.cbHasImage.UseVisualStyleBackColor = true;
            // 
            // cbCategories
            // 
            this.cbCategories.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCategories.FormattingEnabled = true;
            this.cbCategories.Location = new System.Drawing.Point(272, 13);
            this.cbCategories.Name = "cbCategories";
            this.cbCategories.Size = new System.Drawing.Size(164, 24);
            this.cbCategories.TabIndex = 16;
            this.cbCategories.SelectedIndexChanged += new System.EventHandler(this.cbCategories_SelectionChanged);
            // 
            // cbSubcategories
            // 
            this.cbSubcategories.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSubcategories.FormattingEnabled = true;
            this.cbSubcategories.Location = new System.Drawing.Point(462, 12);
            this.cbSubcategories.Name = "cbSubcategories";
            this.cbSubcategories.Size = new System.Drawing.Size(170, 24);
            this.cbSubcategories.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(440, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 17);
            this.label1.TabIndex = 18;
            this.label1.Text = "->";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbSearchPost);
            this.panel1.Controls.Add(this.lblSearchIn);
            this.panel1.Controls.Add(this.rbSearchTitle);
            this.panel1.Location = new System.Drawing.Point(18, 42);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(279, 29);
            this.panel1.TabIndex = 19;
            // 
            // cbContract
            // 
            this.cbContract.AutoSize = true;
            this.cbContract.Location = new System.Drawing.Point(335, 80);
            this.cbContract.Name = "cbContract";
            this.cbContract.Size = new System.Drawing.Size(83, 21);
            this.cbContract.TabIndex = 11;
            this.cbContract.Text = "Contract";
            this.cbContract.UseVisualStyleBackColor = true;
            // 
            // cbInternship
            // 
            this.cbInternship.AutoSize = true;
            this.cbInternship.Location = new System.Drawing.Point(207, 107);
            this.cbInternship.Name = "cbInternship";
            this.cbInternship.Size = new System.Drawing.Size(92, 21);
            this.cbInternship.TabIndex = 12;
            this.cbInternship.Text = "Internship";
            this.cbInternship.UseVisualStyleBackColor = true;
            // 
            // cbTelecommute
            // 
            this.cbTelecommute.AutoSize = true;
            this.cbTelecommute.Location = new System.Drawing.Point(207, 80);
            this.cbTelecommute.Name = "cbTelecommute";
            this.cbTelecommute.Size = new System.Drawing.Size(115, 21);
            this.cbTelecommute.TabIndex = 10;
            this.cbTelecommute.Text = "Telecommute";
            this.cbTelecommute.UseVisualStyleBackColor = true;
            // 
            // ConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 180);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbSubcategories);
            this.Controls.Add(this.cbCategories);
            this.Controls.Add(this.cbHasImage);
            this.Controls.Add(this.cbNonprofit);
            this.Controls.Add(this.cbPartTime);
            this.Controls.Add(this.cbInternship);
            this.Controls.Add(this.cbContract);
            this.Controls.Add(this.cbTelecommute);
            this.Controls.Add(this.lblShowOnly);
            this.Controls.Add(this.lblIn);
            this.Controls.Add(this.tbKeywords);
            this.Controls.Add(this.lblSearchFor);
            this.Controls.Add(this.btnConfigCancel);
            this.Controls.Add(this.btnConfigSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigDialog";
            this.Text = "Search settings";
            this.Load += new System.EventHandler(this.ConfigDialog_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConfigSave;
        private System.Windows.Forms.Button btnConfigCancel;
        private System.Windows.Forms.Label lblSearchFor;
        private System.Windows.Forms.TextBox tbKeywords;
        private System.Windows.Forms.Label lblIn;
        private System.Windows.Forms.Label lblSearchIn;
        private System.Windows.Forms.RadioButton rbSearchTitle;
        private System.Windows.Forms.RadioButton rbSearchPost;
        private System.Windows.Forms.Label lblShowOnly;
        private System.Windows.Forms.CheckBox cbPartTime;
        private System.Windows.Forms.CheckBox cbNonprofit;
        private System.Windows.Forms.CheckBox cbHasImage;
        private System.Windows.Forms.ComboBox cbCategories;
        private System.Windows.Forms.ComboBox cbSubcategories;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbContract;
        private System.Windows.Forms.CheckBox cbInternship;
        private System.Windows.Forms.CheckBox cbTelecommute;
    }
}