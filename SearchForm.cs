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
using System.Threading;

namespace CraigSearch
{
    public partial class SearchForm : Form
    {
        ArrayList m_alAllLocations = null;
        ArrayList m_alAllCategories = null;
        ArrayList m_alAllSubcategories = null;
        bool m_bConnectionTimeoutError = false;
        bool m_bDisplayingConnectionTimerDialog = false;
        bool m_bSearchInProgress = false;
        bool m_bSearchCompleted = false;
        bool m_bUserStopRequested = false;
        string m_sSearchProgressMessage = "CraigSearch";
        string m_sHTMLSearchResults = "";
        System.Windows.Forms.Timer m_tmrUIUpdateTimer = null;

        public SearchForm()
        {
            InitializeComponent();
            
            // initialize menu items
            //
            searchToolStripMenuItem.DropDownItems["settingsToolStripMenuItem"].Enabled = true;
            searchToolStripMenuItem.DropDownItems["searchTestToolStripMenuItem"].Enabled = true;
            searchToolStripMenuItem.DropDownItems["searchNowToolStripMenuItem"].Enabled = true;
            searchToolStripMenuItem.DropDownItems["searchStopToolStripMenuItem"].Enabled = false;

            // Get the list of all US Craigslist cities
            //
            m_alAllLocations = GetLocationList();

            // Get the list of all categories
            //
            m_alAllCategories = GetCategoryList();

            // Get the list of all subcategories
            //
            m_alAllSubcategories = GetSubcategoryList(m_alAllCategories);

            // Start the UI update timer
            //
            m_tmrUIUpdateTimer = new System.Windows.Forms.Timer();
            m_tmrUIUpdateTimer.Interval = 1000;	    // Once per second
            m_tmrUIUpdateTimer.Tick += new EventHandler(UpdateUI);
            m_tmrUIUpdateTimer.Start();

            // Debug: display categories
            //
            string sHTMLBody = "";
            sHTMLBody += "<table>\r\n";
            foreach (CategoryInfo ciCategory in m_alAllCategories)
            {
                sHTMLBody += "<tr>";
                sHTMLBody += "<td>" + ciCategory.Title + "</td>";
                sHTMLBody += "<td>" + ciCategory.ID + "</td>";
                sHTMLBody += "</tr>\r\n";
            }
            sHTMLBody += "</table>\r\n";

            // Debug: display subcategories
            //
            sHTMLBody += "<br><table>\r\n";
            foreach (SubcategoryInfo ciSubcategory in m_alAllSubcategories)
            {
                sHTMLBody += "<tr>";
                sHTMLBody += "<td>" + ciSubcategory.CategoryID + "</td>";
                sHTMLBody += "<td>" + ciSubcategory.Title + "</td>";
                sHTMLBody += "<td>" + ciSubcategory.ID + "</td>";
                sHTMLBody += "</tr>\r\n";
            }
            sHTMLBody += "</table>\r\n";
            // webBrowser.DocumentText = sHTMLBody;
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
            searchToolStripMenuItem.DropDownItems["settingsToolStripMenuItem"].Enabled = false;
            searchToolStripMenuItem.DropDownItems["searchTestToolStripMenuItem"].Enabled = false;
            searchToolStripMenuItem.DropDownItems["searchNowToolStripMenuItem"].Enabled = false;
            searchToolStripMenuItem.DropDownItems["searchStopToolStripMenuItem"].Enabled = true;
            Thread thread = new Thread(new ThreadStart(DoTestSearchThread));
            thread.Start();
        }

        private void menuSearch_Clicked(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem toolItem in searchToolStripMenuItem.DropDownItems)
            {
                searchToolStripMenuItem.DropDownItems["settingsToolStripMenuItem"].Enabled = false;
                searchToolStripMenuItem.DropDownItems["searchTestToolStripMenuItem"].Enabled = false;
                searchToolStripMenuItem.DropDownItems["searchNowToolStripMenuItem"].Enabled = false;
                searchToolStripMenuItem.DropDownItems["searchStopToolStripMenuItem"].Enabled = true;
            }
            Thread thread = new Thread(new ThreadStart(DoFullSearchThread));
            thread.Start();
        }

        private void menuStopSearch_Clicked(object sender, EventArgs e)
        {
            // Signal the search thread to stop
            //
            m_bUserStopRequested = true;
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            // Set the titlebar text to reflect our current state
            //
            this.Text = m_sSearchProgressMessage;

            // Notify the user if we are having trouble
            //
            if (m_bConnectionTimeoutError && !m_bDisplayingConnectionTimerDialog)
            {
                m_bDisplayingConnectionTimerDialog = true;
                MessageBox.Show("Sorry, we encountered multiple timeout errors while trying to connect to Craigslist.org. Please try your search again.", "CraigSearch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_bDisplayingConnectionTimerDialog = false;
                m_bConnectionTimeoutError = false;
            }

            // If a search has just been completed, display the results
            //
            if (m_bSearchCompleted)
            {
                m_bSearchInProgress = false;
                m_bSearchCompleted = false;
                this.Text = m_sSearchProgressMessage;
                webBrowser.DocumentText = m_sHTMLSearchResults;
                searchToolStripMenuItem.DropDownItems["settingsToolStripMenuItem"].Enabled = true;
                searchToolStripMenuItem.DropDownItems["searchTestToolStripMenuItem"].Enabled = true;
                searchToolStripMenuItem.DropDownItems["searchNowToolStripMenuItem"].Enabled = true;
                searchToolStripMenuItem.DropDownItems["searchStopToolStripMenuItem"].Enabled = false;
            }
        }

        private void DoTestSearchThread()
        {
            SearchLocationList(10);
        }

        private void DoFullSearchThread()
        {
            SearchLocationList(m_alAllLocations.Count);
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

        private void SearchLocationList(int iLocationCount)
        {
            // Tell the UI thread what we're up to
            //
            m_bSearchInProgress = true;

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

            string sHTMLBody = "";
            sHTMLBody += "<table width=\"100%\">\r\n";
            foreach (LocationInfo liLocation in m_alAllLocations)
            {
                // Get the search result list for this location
                //
                if (++iLocationCounter > iLocationCount)
                    break;
                m_sSearchProgressMessage = "Searching " + liLocation.State.ToUpper() + ": " + liLocation.City.ToUpper() + " (" + iLocationCounter.ToString() + " of " + iLocationCount.ToString() + " cities, " + iResultCounter.ToString() + " found)";
                ArrayList alResults = SearchLocation(liLocation);

                // Stop if the user has requested us to do so
                //
                if (m_bUserStopRequested)
                    break;

                // Stop if we are having connection issues after multiple attempts
                //
                if (m_bConnectionTimeoutError)
                    break;

                // If no results were found at this location, skip the output logic.
                //
                if (alResults.Count == 0)
                    continue;

                // Display the city/state/result count header for this location
                //
                sHTMLBody += "<tr><td colspan=\"2\"><h4 class=\"ban\" style=\"text-align:left;\"><a href=\"" + liLocation.URL + "\" target=\"_blank\">" + liLocation.City.ToUpper() + ", " + liLocation.State.ToUpper() + "</a>: " + alResults.Count.ToString() + " item(s)</h4></td></tr>\r\n";

                // Display the results
                //
                foreach (SearchResultInfo srResult in alResults)
                {
                    sHTMLBody += "<tr>";
                    sHTMLBody += "<td width=\"5%\" nowrap>" + srResult.Date + "</td>";
                    sHTMLBody += "<td><a href=\"" + srResult.URL + "\" target=\"_blank\">" + srResult.Title + "</a></td>";
                    sHTMLBody += "</tr>\r\n";
                    ++iResultCounter;
                }
            }
            sHTMLBody += "</table>\r\n";
            sHTMLBody += "</body>\r\n";

            // Display the results
            //
            string sResultMessage = "Displaying " + iResultCounter.ToString() + " item(s) in " + (iLocationCounter - 1).ToString() + " cities";
            m_sSearchProgressMessage = "CraigSearch: " + sResultMessage;
            m_sHTMLSearchResults = sHTMLHeader + sResultMessage + "<br /><br />\r\n" + sHTMLBody;

            // Signal to the UI thread that it's time to display the search results
            //
            m_bSearchCompleted = true;
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

            // Search the location and get the results into an array
            //
            m_bConnectionTimeoutError = false;
            bool bConnectionSuccessful = false;
            int iMaxTries = 3;
            int iTryCounter = 0;
            while (!bConnectionSuccessful)
            {
                // Post to the Craigslist URL and get back the HTML response page
                //
                // byte[] abData = null;
                HttpWebResponse response = null;
                try
                {
                    // Note: I initially used a WebClient to retrieve the web page, but there is no way to control the timeout and I
                    // wanted to be able to retry a failed attempt more quickly than the WebClient's default 30-second timeout,
                    // so I switched to HttpWebRequest instead. Commenting & leaving the old code here "just in case"...
                    //
                    // WebClient wc = new WebClient();
                    // abData = wc.DownloadData(sSearchURL);

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sSearchURL);
                    request.Method = "POST";
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.ContentType = "text/html";
                    request.Timeout = 5000;  // 5 seconds
                    response = (HttpWebResponse)request.GetResponse();
                    bConnectionSuccessful = true;
                }
                catch (WebException ex)
                {
                    if (++iTryCounter == iMaxTries)
                        break;
                }

                // WebClient logic
                //
                // if (abData == null) // Try again
                //     continue;
                // string sHTMLIn = Encoding.ASCII.GetString(abData);

                if (response == null) // Try again
                    continue;

                // Get the response into an HTMLDocument so that we can parse it
                //
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string sHTMLIn = streamReader.ReadToEnd();
                response.Close();
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
            }

            // If we were unable to retrieve results from Craigslist.org, raise an error.
            //
            if (iTryCounter == iMaxTries)
                m_bConnectionTimeoutError = true;

            // Return the results
            //
            return (alResults);
        }
    }

    #region Data structure classes
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
    #endregion
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
