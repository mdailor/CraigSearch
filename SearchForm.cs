using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using mshtml;
using System.Configuration;
using System.Web;

namespace CraigSearch
{
    public partial class SearchForm : Form
    {
        ArrayList m_alAllLocations = null;
        ArrayList m_alAllCategories = null;
        ArrayList m_alAllSubcategories = null;

        public SearchForm()
        {
            InitializeComponent();

            // Get the list of all US Craigslist cities
            //
            m_alAllLocations = GetLocationList();

            // Get the list of all categories
            //
            m_alAllCategories = GetCategoryList();

            // Get the list of all subcategories
            //
            m_alAllSubcategories = GetSubcategoryList(m_alAllCategories);

            // Debug: display categories
            //
            string sHTMLContent = "";
            sHTMLContent += "<table>\r\n";
            foreach (CategoryInfo ciCategory in m_alAllCategories)
            {
                sHTMLContent += "<tr>";
                sHTMLContent += "<td>" + ciCategory.Title + "</td>";
                sHTMLContent += "<td>" + ciCategory.ID + "</td>";
                sHTMLContent += "</tr>\r\n";
            }
            sHTMLContent += "</table>\r\n";

            // Debug: display subcategories
            //
            sHTMLContent += "<br><table>\r\n";
            foreach (SubcategoryInfo ciSubcategory in m_alAllSubcategories)
            {
                sHTMLContent += "<tr>";
                sHTMLContent += "<td>" + ciSubcategory.CategoryID + "</td>";
                sHTMLContent += "<td>" + ciSubcategory.Title + "</td>";
                sHTMLContent += "<td>" + ciSubcategory.ID + "</td>";
                sHTMLContent += "</tr>\r\n";
            }
            sHTMLContent += "</table>\r\n";
            // webBrowser.DocumentText = sHTMLContent;
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
        }

        private void menuFileExit_Clicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuConfig_Clicked(object sender, EventArgs e)
        {
            ConfigDialog dlgConfig = new ConfigDialog(m_alAllLocations, m_alAllCategories, m_alAllSubcategories);
            dlgConfig.ShowDialog();
        }

        private void menuTestSearch_Clicked(object sender, EventArgs e)
        {
            SearchLocations(10);
        }

        private void menuSearch_Clicked(object sender, EventArgs e)
        {
            SearchLocations(m_alAllLocations.Count);
        }

        private ArrayList GetLocationList()
        {
            ArrayList alLocations = new ArrayList();

            // Get the "Cities" page from Craigslist with the US listed at the top
            //
            WebClient wc = new WebClient();
            byte[] abData = wc.DownloadData("http://www.craigslist.org/about/sites/#US");
            string sHTMLIn = Encoding.ASCII.GetString(abData);
            IHTMLDocument2 htmlDocIn = (IHTMLDocument2)new HTMLDocument();
            htmlDocIn.write(sHTMLIn);

            // Add each US location on the page to the output list
            //
            string sState = "";
            IHTMLElementCollection elementList = htmlDocIn.all;
            foreach (IHTMLElement element in elementList)
            {
                if (element.className == "state_delimiter")
                {
                    sState = element.innerHTML;
                }
                else if (!(element.getAttribute("href") is System.DBNull))
                {
                    if (element.innerHTML == null)
                        continue;
                    else if (element.getAttribute("href").Contains("#"))    // Links to "US", "Canada", etc.
                        continue;
                    else if (element.getAttribute("href").Contains("micronesia"))   // We just passed Wyoming, all done
                        break;

                    string sCity = element.innerText;
                    string sURL = element.getAttribute("href");
                    alLocations.Add(new LocationInfo(sState, sCity, sURL));
                }
            }

            // Return the list
            //
            return (alLocations);
        }

        private ArrayList GetCategoryList()
        {
            ArrayList alCategories = new ArrayList();

            // Get the "Alabama -> Auburn" page from Craigslist (first available location page)
            //
            WebClient wc = new WebClient();
            byte[] abData = wc.DownloadData("http://auburn.craigslist.org/");
            string sHTMLIn = Encoding.ASCII.GetString(abData);
            IHTMLDocument2 htmlDocIn = (IHTMLDocument2)new HTMLDocument();
            htmlDocIn.write(sHTMLIn);

            // Add each category on the page to the output list
            //
            bool bInCategoryList = false;
            IHTMLElementCollection elementList = htmlDocIn.all;
            foreach (IHTMLElement element in elementList)
            {
                if (element.tagName.ToLower() == "select")
                {
                    if (!(element.getAttribute("name") is DBNull) && element.getAttribute("name") == "catAbb")
                        bInCategoryList = true;
                }
                else if (element.tagName.ToLower() == "option")
                {
                    if (bInCategoryList && !"personals.resumes".Contains(element.innerText))
                    {
                        string sTitle = element.innerText;
                        string sID = element.getAttribute("value");
                        alCategories.Add(new CategoryInfo(sTitle, sID));
                    }
                }
                else if (alCategories.Count > 0)
                {
                    break;  // We just passed the last category, all done.
                }
            }

            // Return the list
            //
            return (alCategories);
        }

        private ArrayList GetSubcategoryList(ArrayList alAllCategories)
        {
            ArrayList alSubcategories = new ArrayList();

            // Get the "Alabama -> Auburn" page from Craigslist (first available location page)
            //
            WebClient wc = new WebClient();
            byte[] abData = wc.DownloadData("http://auburn.craigslist.org/");
            string sHTMLIn = Encoding.ASCII.GetString(abData);
            IHTMLDocument2 htmlDocIn = (IHTMLDocument2)new HTMLDocument();
            htmlDocIn.write(sHTMLIn);

            // Add each subcategory on the page to the output list
            //
            foreach (CategoryInfo ciCategory in alAllCategories)
            {
                bool bInSubcategoryList = false;
                IHTMLElementCollection elementList = htmlDocIn.all;
                foreach (IHTMLElement element in elementList)
                {
                    if (element.tagName.ToLower() == "ul")
                    {
                        if (!(element.getAttribute("id") is System.DBNull) && (element.getAttribute("id") != null))
                        {
                            string sID = element.getAttribute("id");
                            if (sID.StartsWith(ciCategory.ID))
                                bInSubcategoryList = true;
                        }
                    }
                    else if (element.tagName.ToLower() == "a")
                    {
                        if (bInSubcategoryList)
                        {
                            string sCategoryID = ciCategory.ID;
                            string sTitle = element.innerText;
                            string sID = element.getAttribute("href");
                            if (sID.StartsWith("about:"))
                                sID = sID.Substring("about:".Length);   // "about:act/" -> "act/"
                            if (sID.EndsWith("/"))
                                sID = sID.Substring(0, sID.Length - 1); // "act/" -> "act"

                            // Cars+trucks is actually 3 categories preceded by a landing page
                            //
                            if (sTitle == "cars+trucks")
                            {
                                alSubcategories.Add(new SubcategoryInfo(sCategoryID, "cars+trucks (all)", "cta"));
                                alSubcategories.Add(new SubcategoryInfo(sCategoryID, "cars+trucks (by owner)", "cto"));
                                alSubcategories.Add(new SubcategoryInfo(sCategoryID, "cars+trucks (by dealer)", "ctd"));
                            }
                            else
                            {
                                alSubcategories.Add(new SubcategoryInfo(sCategoryID, sTitle, sID));
                            }
                        }
                    }
                    else if ((element.tagName.ToLower() == "div") && bInSubcategoryList)
                    {
                        bInSubcategoryList = false;
                    }
                }
            }

            // Return the list
            //
            return (alSubcategories);
        }

        private void SearchLocations(int iLocationCount)
        {
            // Search specified number of locations
            //
            int iLocationCounter = 0;
            int iResultCounter = 0;
            string sHTMLHeader = "";
            sHTMLHeader += "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">\r\n";
            sHTMLHeader += "<html>\r\n";
            sHTMLHeader += "<head>\r\n";
            sHTMLHeader += "<link type=\"text/css\" rel=\"stylesheet\" media=\"all\" href=\"http://www.craigslist.org/styles/craigslist.css?v=a64eefe319b8a553ddf0df3549209610\">\r\n";
            sHTMLHeader += "</head>\r\n";
            sHTMLHeader += "<body>\r\n";

            string sHTMLContent = "";
            sHTMLContent += "<table width=\"100%\">\r\n";
            foreach (LocationInfo liLocation in m_alAllLocations)
            {
                // Get the search result list for this location
                //
                if (++iLocationCounter > iLocationCount)
                    break;
                this.Text = "Searching " + liLocation.State.ToUpper() + ": " + liLocation.City.ToUpper() + " (" + iLocationCounter.ToString() + " of " + iLocationCount.ToString() + " cities)";
                ArrayList alResults = SearchLocation(liLocation);
                if (alResults.Count == 0)
                    continue;

                // Display the city/state/result count header for this location
                //
                sHTMLContent += "<tr><td colspan=\"2\"><h4 class=\"ban\" style=\"text-align:left;\"><a href=\"" + liLocation.URL + "\" target=\"_blank\">" + liLocation.City.ToUpper() + ", " + liLocation.State.ToUpper() + "</a>: " + alResults.Count.ToString() + " item(s)</h4></td></tr>\r\n";

                // Display the results
                //
                foreach (SearchResultInfo srResult in alResults)
                {
                    sHTMLContent += "<tr>";
                    sHTMLContent += "<td width=\"5%\" nowrap>" + srResult.Date + "</td>";
                    sHTMLContent += "<td><a href=\"" + srResult.URL + "\" target=\"_blank\">" + srResult.Title + "</a></td>";
                    sHTMLContent += "</tr>\r\n";
                    ++iResultCounter;
                }
            }
            sHTMLContent += "</table>\r\n";
            sHTMLContent += "</body>\r\n";

            // Display the results
            //
            string sResultMessage = "Displaying " + iResultCounter.ToString() + " item(s) in " + iLocationCount.ToString() + " cities";
            this.Text = "CraigSearch: " + sResultMessage;
            webBrowser.DocumentText = sHTMLHeader + sResultMessage + "<br /><br />\r\n" + sHTMLContent;
        }

        private ArrayList SearchLocation(LocationInfo liLocation)
        {
            ArrayList alResults = new ArrayList();

            // Set up the Craigslist URL with the user's search criteria
            //
            string sSearchURL = liLocation.URL + "search/";                             // http://auburn.craigslist.org/search/
            sSearchURL += ConfigurationManager.AppSettings["SearchSubcategory"];        // http://auburn.craigslist.org/search/jjj
            sSearchURL += "?srchType=" + ConfigurationManager.AppSettings["SearchType"];// http://auburn.craigslist.org/search/jjj?srchType=A
            if (ConfigurationManager.AppSettings["SearchKeywords"] != "")
                sSearchURL += "&query=" + HttpUtility.UrlEncode(ConfigurationManager.AppSettings["SearchKeywords"]);  // http://auburn.craigslist.org/search/jjj?srchType=A&query=MyKeywords
            if (ConfigurationManager.AppSettings["SearchHasImage"] == "Y")
                sSearchURL += "&hasPic=1";
            if (ConfigurationManager.AppSettings["SearchCategory"] == "jjj")
            {
                if (ConfigurationManager.AppSettings["SearchTelecommute"] == "Y")
                    sSearchURL += "&addOne=telecommuting";
                if (ConfigurationManager.AppSettings["SearchContract"] == "Y")
                    sSearchURL += "&addTwo=contract";
                if (ConfigurationManager.AppSettings["SearchInternship"] == "Y")
                    sSearchURL += "&addThree=internship";
                if (ConfigurationManager.AppSettings["SearchPartTime"] == "Y")
                    sSearchURL += "&addFour=part-time";
                if (ConfigurationManager.AppSettings["SearchNonprofit"] == "Y")
                    sSearchURL += "&addFive=non-profit";
            }
            // MessageBox.Show(sSearchURL, "CraigSearch", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Search the location and get the results into a DOM document
            //
            WebClient wc = new WebClient();
            byte[] abData = wc.DownloadData(sSearchURL);
            string sHTMLIn = Encoding.ASCII.GetString(abData);
            IHTMLDocument2 htmlDocIn = (IHTMLDocument2)new HTMLDocument();
            htmlDocIn.write(sHTMLIn);

            // Pull out any results that were found for the location
            //
            bool bInResult = false;
            string sDate = "";
            IHTMLElementCollection elementList = htmlDocIn.all;
            foreach (IHTMLElement element in elementList)
            {
                if (element.className == "itemdate")
                {
                    sDate = element.innerHTML;  // "Oct 1"
                    bInResult = true;
                }
                else if (bInResult && !(element.getAttribute("href") is System.DBNull))
                {
                    string sTitle = element.innerText;
                    string sURL = element.getAttribute("href");
                    if (!sURL.Contains(liLocation.URL)) // Just passed the end of the local results, into nearby results now.
                        break;

                    alResults.Add(new SearchResultInfo(sDate, sTitle, sURL));
                    bInResult = false;
                }
            }

            // Return the results
            //
            return (alResults);
        }
    }

    public class LocationInfo
    {
        private string m_sState;
        private string m_sCity;
        private string m_sURL;

        public LocationInfo() { }
        public LocationInfo(string sState, string sCity, string sURL)
        {
            m_sState = sState;
            m_sCity = sCity;
            m_sURL = sURL;
        }

        public string State
        {
            get { return m_sState; }
            set { m_sState = value; }
        }

        public string City
        {
            get { return m_sCity; }
            set { m_sCity = value; }
        }

        public string URL
        {
            get { return m_sURL; }
            set { m_sURL = value; }
        }
    }

    public class CategoryInfo
    {
        private string m_sTitle;
        private string m_sID;

        public CategoryInfo() { }
        public CategoryInfo(string sTitle, string sID)
        {
            m_sTitle = sTitle;
            m_sID = sID;
        }

        public string Title
        {
            get { return m_sTitle; }
            set { m_sTitle = value; }
        }

        public string ID
        {
            get { return m_sID; }
            set { m_sID = value; }
        }
    }

    public class SubcategoryInfo
    {
        private string m_sCategoryID;
        private string m_sTitle;
        private string m_sID;

        public SubcategoryInfo() { }
        public SubcategoryInfo(string sCategoryID, string sTitle, string sID)
        {
            m_sCategoryID = sCategoryID;
            m_sTitle = sTitle;
            m_sID = sID;
        }

        public string CategoryID
        {
            get { return m_sCategoryID; }
            set { m_sCategoryID = value; }
        }

        public string Title
        {
            get { return m_sTitle; }
            set { m_sTitle = value; }
        }

        public string ID
        {
            get { return m_sID; }
            set { m_sID = value; }
        }
    }

    public class SearchResultInfo
    {
        private string m_sDate;
        private string m_sTitle;
        private string m_sURL;

        public SearchResultInfo() { }
        public SearchResultInfo(string sDate, string sTitle, string sURL)
        {
            m_sDate = sDate;
            m_sTitle = sTitle;
            m_sURL = sURL;
        }

        public string Date
        {
            get { return m_sDate; }
            set { m_sDate = value; }
        }

        public string Title
        {
            get { return m_sTitle; }
            set { m_sTitle = value; }
        }

        public string URL
        {
            get { return m_sURL; }
            set { m_sURL = value; }
        }
    }
}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using mshtml;
using ObjectLayerLib.Utils;

namespace ObjectLayerLib
{
    public class Webform
    {
        public BoxObject Box;

        public string Method = "get";
        public string ActionString = "";
        public string InternalName = "";
        public string InternalID = "";
        public int FormIndex = 0;
        public int WebformId = 0;
        public int WebpageId = 0;

        public int Verified = (int)Extracter.NONE;

        public string HTMLTextContent = "";

        //Corresponding Webform HtmlElement
        public HTMLFormElement FormElement;
        public List<WebformInstance> Instances = new List<WebformInstance>();

        public Webform(string method, string action, string name, string formId, int index)
        {

            this.Method = method;
            ActionString = action;
            InternalName = name;
            InternalID = formId;
            FormIndex = index;
        }

        public Webform(BoxObject box, string method, string action, string name, string formId, int index)
        {
            this.Box = box;
            this.Method = method;
            ActionString = action;
            InternalName = name;
            InternalID = formId;
            FormIndex = index;
        }

        public Webform(HTMLFormElement element, string method, string action, string name, string formId, int index)
        {
            
            this.Method = method;
            ActionString = action;
            InternalName = name;
            InternalID = formId;
            FormIndex = index;

            FormElement = element;

            ComputeBoxObject();
        }

        public void ComputeBoxObject()
        {
            RenderingUtils util = new RenderingUtils();
            Box = util.GetOffsetPosition((IHTMLElement)FormElement);// .GetScreenLocation((IHTMLElement)FormElement);
        }

        public void ParseFormMetadata()
        {
            string formContent = HTMLTextContent.ToUpper();
            XmlDocument xmlDoc = new XmlDocument();
            Match m = Regex.Match(formContent, "<FORM[^>]*>");
            if (!m.Success)
                return;

            formContent = m.ToString() + "</FORM>";
            formContent = formContent.Replace("&", "").Replace("XMLNS:", "");
            formContent = formContent.Replace(";", "");

            string formName = "";
            string formAction = "";
            string formID = "";

            try
            {
                xmlDoc.LoadXml(formContent);
                XmlElement root = (XmlElement)xmlDoc.GetElementsByTagName("FORM")[0];
                InternalName = (string)root.GetAttribute("NAME").Replace("\"", "");
                ActionString = (string)root.GetAttribute("ACTION").Replace("\"", "");
                InternalID = (string)root.GetAttribute("ID").Replace("\"", "");
                Method = (string)root.GetAttribute("METHOD").Replace("\"", "");
            }
            catch (Exception e) { return; }
        }

        /// <summary>
        /// Obtain webformfields and their values of currrent webform instance
        /// </summary>
        /// <returns></returns>
        public WebformInstance GetWebformInstance()
        {
            WebformInstance instance = new WebformInstance();
            try
            {
                //First obtaining Webformfields
                foreach (IHTMLElement element in (HTMLElementCollection)FormElement.elements)
                {
                    //now get Name/Type of each element
                    string ElementName = "";
                    string ElementID = "";
                    string ElementType = "";
                    if (element.getAttribute("name", 0) != DBNull.Value && element.getAttribute("name", 0) != null)
                        ElementName = (string)element.getAttribute("name", 0);
                    if (element.getAttribute("id", 0) != DBNull.Value && element.getAttribute("id", 0) != null)
                        ElementID = (string)element.getAttribute("id", 0);

                    if (element.GetType().FullName == "mshtml.HTMLInputElementClass" || element.GetType().FullName == "mshtml.HTMLInputButtonClass"
                        || element.GetType().FullName == "mshtml.HTMLSelectElementClass" || element.GetType().FullName == "mshtml.HTMLTextAreaElementClass")
                        ElementType = (string)element.getAttribute("type", 0);
                    else continue;

                    
                    
                    if (ElementType.Contains("select")) ElementType = "select"; //it can be select-one or select-multiple
                    if (ElementType == "hidden" || ElementType == "submit" || ElementType == "button" || ElementType == "reset") continue;
                    string NonNumeric = System.Text.RegularExpressions.Regex.Replace(ElementName.Trim(), @"[0-9]", "");
                    if (NonNumeric.Trim() == "")
                        continue;
                    Webformfield wff = new Webformfield(element);
                    wff.Id = ElementID;
                    wff.Name = ElementName;
                    wff.Type = ElementType;
                    wff.GetValues(); //Obtain values of this webformfield

                    instance.Elements.Add(wff);

                }
            }
            catch (Exception e) {
                return null; 
            }
            return instance;
        }
    }
}
*/
