using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace CraigSearch
{
    public partial class ConfigDialog : Form
    {
        ArrayList m_alAllLocations = null;
        ArrayList m_alAllCategories = null;
        ArrayList m_alAllSubcategories = null;

        public ConfigDialog(ArrayList alAllLocations, ArrayList alAllCategories, ArrayList alAllSubcategories)
        {
            InitializeComponent();

            m_alAllLocations = alAllLocations;
            m_alAllCategories = alAllCategories;
            m_alAllSubcategories = alAllSubcategories;
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
            // Load the keywords textbox
            //
            tbKeywords.Text = ConfigurationManager.AppSettings["SearchKeywords"];

            // Load the categories combobox
            //
            int iCategoryIndex = 0;
            string sSelectedCategoryID = ConfigurationManager.AppSettings["SearchCategory"];
            if (sSelectedCategoryID == "")
                sSelectedCategoryID = "jjj";
            cbCategories.DisplayMember = "Title";
            cbCategories.ValueMember = "ID";
            foreach (CategoryInfo ciCategory in m_alAllCategories)
            {
                cbCategories.Items.Add(new CategoryInfo(ciCategory.Title, ciCategory.ID));
                if (ciCategory.ID == sSelectedCategoryID)
                    cbCategories.SelectedIndex = iCategoryIndex;
                iCategoryIndex++;
            }

            // The subcategories combobox has already been loaded via cbCategories_SelectionChanged,
            // set the selected item.
            //
            int iSubcategoryIndex = 0;
            string sSelectedSubcategoryID = ConfigurationManager.AppSettings["SearchSubcategory"];
            if (sSelectedSubcategoryID == "")
                sSelectedSubcategoryID = "sof";
            foreach (SubcategoryInfo siSubcategory in cbSubcategories.Items)
            {
                if (siSubcategory.ID == sSelectedSubcategoryID)
                    cbSubcategories.SelectedIndex = iSubcategoryIndex;
                iSubcategoryIndex++;
            }

            // Set title only / entire post radio buttons
            //
            if (ConfigurationManager.AppSettings["SearchType"] == "T")
                rbSearchTitle.Checked = true;
            else
                rbSearchPost.Checked = true;
        }

        private void btnConfigSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["SearchKeywords"].Value = tbKeywords.Text;
            config.AppSettings.Settings["SearchCategory"].Value = ((CategoryInfo)cbCategories.SelectedItem).ID;
            config.AppSettings.Settings["SearchSubcategory"].Value = ((SubcategoryInfo)cbSubcategories.SelectedItem).ID;
            config.AppSettings.Settings["SearchType"].Value = (string)(rbSearchTitle.Checked ? "T" : "A");
            config.AppSettings.Settings["SearchHasImage"].Value = (string)(cbHasImage.Checked ? "Y" : "N");
            config.AppSettings.Settings["SearchTelecommute"].Value = (string)(cbTelecommute.Checked ? "Y" : "N");
            config.AppSettings.Settings["SearchContract"].Value = (string)(cbContract.Checked ? "Y" : "N");
            config.AppSettings.Settings["SearchNonprofit"].Value = (string)(cbNonprofit.Checked ? "Y" : "N");
            config.AppSettings.Settings["SearchInternship"].Value = (string)(cbInternship.Checked ? "Y" : "N");
            config.AppSettings.Settings["SearchPartTime"].Value = (string)(cbPartTime.Checked ? "Y" : "N");
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void btnConfigCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cbCategories_SelectionChanged(object sender, EventArgs e)
        {
            // Load the subcategories combobox with selections belonging to the currently selected category
            //
            CategoryInfo ciSelectedCategory = (CategoryInfo)cbCategories.SelectedItem;
            cbSubcategories.Items.Clear();
            cbSubcategories.DisplayMember = "Title";
            cbSubcategories.ValueMember = "ID";
            cbSubcategories.Items.Add(new SubcategoryInfo(ciSelectedCategory.ID, "All", ciSelectedCategory.ID));
            foreach (SubcategoryInfo siSubcategory in m_alAllSubcategories)
            {
                if (siSubcategory.CategoryID == ciSelectedCategory.ID)
                    cbSubcategories.Items.Add(new SubcategoryInfo(siSubcategory.CategoryID, siSubcategory.Title, siSubcategory.ID));
            }
            cbSubcategories.SelectedIndex = 0;

            // Set show only checkboxes based on the currently selected category
            //
            cbHasImage.Visible = true;
            cbHasImage.Checked = (ConfigurationManager.AppSettings["SearchHasImage"] == "Y");
            cbTelecommute.Visible = (ciSelectedCategory.ID == "jjj");
            cbTelecommute.Checked = (ConfigurationManager.AppSettings["SearchTelecommute"] == "Y");
            cbContract.Visible = (ciSelectedCategory.ID == "jjj");
            cbContract.Checked = (ConfigurationManager.AppSettings["SearchContract"] == "Y");
            cbNonprofit.Visible = (ciSelectedCategory.ID == "jjj");
            cbNonprofit.Checked = (ConfigurationManager.AppSettings["SearchNonprofit"] == "Y");
            cbInternship.Visible = (ciSelectedCategory.ID == "jjj");
            cbInternship.Checked = (ConfigurationManager.AppSettings["SearchInternship"] == "Y");
            cbPartTime.Visible = (ciSelectedCategory.ID == "jjj");
            cbPartTime.Checked = (ConfigurationManager.AppSettings["SearchPartTime"] == "Y");
        }
    }
}
