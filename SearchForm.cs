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

        // Search thread variables
        //
        // Test search execution times for "all software telecommute jobs":
        // 1 thread:   4:25
        // 4 threads:  1:15
        // 6 threads:  1.01
        // 8 threads:  0:55
        // 12 threads: 0:55
        //
        static int m_iMaxThreads = 8;   // Create this many search threads
        SearchInfo[] m_asiSearchList = new SearchInfo[m_iMaxThreads];
        int m_iCurrentSearchIndex = 0;

        public SearchForm()
        {
            InitializeComponent();
            
            // Initialize menu items
            //
            EnableMenuItems();

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

        private void menuSearchStart_Clicked(object sender, EventArgs e)
        {
            m_bSearchInProgress = true;
            m_bConnectionTimeoutError = false;
            m_bSearchCompleted = false;
            m_bUserStopRequested = false;
            m_sHTMLSearchResults = "";
            EnableMenuItems();

            // We will start multiple search threads here and assign each one to its own range of the location list
            //
            int iMaxLocationsPerThread = m_alAllLocations.Count / m_iMaxThreads;
            int iLocationIndexAll = 0;
            for (int iThreadIndex = 0; iThreadIndex < m_iMaxThreads; iThreadIndex++)
            {
                m_asiSearchList[iThreadIndex] = new SearchInfo();
                for (int i = 0; i < iMaxLocationsPerThread; i++)
                {
                    if (iLocationIndexAll < m_alAllLocations.Count)
                        m_asiSearchList[iThreadIndex].Locations.Add(m_alAllLocations[iLocationIndexAll++]);
                }

                Thread thread = new Thread(DoSearchThread);
                thread.Start(m_asiSearchList[iThreadIndex]);
            }
        }

        private void menuSearchStop_Clicked(object sender, EventArgs e)
        {
            // Signal the search thread to stop
            //
            m_bUserStopRequested = true;
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            // Set the titlebar text to reflect our current state
            //
            if (m_bSearchInProgress)
            {
                // Compile totals for all of the search threads
                //
                int iLocationCount = 0;
                int iSearchResultCount = 0;
                int iCompletedCount = 0;
                for (int i = 0; i < m_iMaxThreads; i++)
                {
                    iLocationCount += m_asiSearchList[i].CurrentLocationNumber;
                    iSearchResultCount += m_asiSearchList[i].ResultCount;
                    if (m_asiSearchList[i].SearchCompleted)
                        ++iCompletedCount;
                }
                string sSearchProgressMessage = "Searching " + m_asiSearchList[m_iCurrentSearchIndex].CurrentLocationName;
                sSearchProgressMessage += " (" + iLocationCount.ToString() + " of " + m_alAllLocations.Count.ToString() + " locations, " + iSearchResultCount.ToString() + " found)";
                this.Text = sSearchProgressMessage;

                if (++m_iCurrentSearchIndex == m_iMaxThreads)
                    m_iCurrentSearchIndex = 0;

                // If all threads have completed their search, we're done.
                //
                if (iCompletedCount == m_iMaxThreads)
                    m_bSearchCompleted = true;
            }

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
                EnableMenuItems();
                
                // Set up the output document header (this is not specifically the HTML <head></head>, but rather the "static" first portion of our output)
                //
                m_sHTMLSearchResults = "";
                m_sHTMLSearchResults += "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">\r\n";
                m_sHTMLSearchResults += "<html>\r\n";
                m_sHTMLSearchResults += "<head>\r\n";
                m_sHTMLSearchResults += "<link type=\"text/css\" rel=\"stylesheet\" media=\"all\" href=\"http://www.craigslist.org/styles/craigslist.css?v=a64eefe319b8a553ddf0df3549209610\">\r\n";
                m_sHTMLSearchResults += "</head>\r\n";
                m_sHTMLSearchResults += "<body>\r\n";

                // Append the search results HTML to the document
                //
                int iLocationCount = 0;
                int iSearchResultCount = 0;
                for (int i = 0; i < m_iMaxThreads; i++)
                {
                    iLocationCount += m_asiSearchList[i].CurrentLocationNumber;
                    iSearchResultCount += m_asiSearchList[i].ResultCount;
                    m_sHTMLSearchResults += m_asiSearchList[i].HTMLSearchResults;
                }

                // Finish the document
                //
                m_sHTMLSearchResults += "</body>\r\n";

                // Display the document
                //
                webBrowser.DocumentText = m_sHTMLSearchResults;

                // Display the status in the titlebar
                //
                this.Text = "CraigSearch: Displaying " + iSearchResultCount.ToString() + " item(s) in " + (iLocationCount + 1).ToString() + " locations";
            }
        }

        private void EnableMenuItems()
        {
            searchToolStripMenuItem.DropDownItems["searchSettingsToolStripMenuItem"].Enabled = !m_bSearchInProgress;
            searchToolStripMenuItem.DropDownItems["searchStartToolStripMenuItem"].Enabled = !m_bSearchInProgress;
            searchToolStripMenuItem.DropDownItems["searchStopToolStripMenuItem"].Enabled = m_bSearchInProgress;
        }

        private void DoSearchThread(Object objSearch)
        {
            SearchLocationList((SearchInfo)objSearch);
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

        private void SearchLocationList(SearchInfo siSearch)
        {
            // Search each location in the list and append the results to our output HTML document
            //
            siSearch.HTMLSearchResults = "";
            siSearch.HTMLSearchResults += "<table width=\"100%\">\r\n";
            foreach (LocationInfo liLocation in siSearch.Locations)
            {
                // Get the search result list for this location
                //
                ++siSearch.CurrentLocationNumber;
                siSearch.CurrentLocationName = liLocation.State.ToUpper() + ": " + liLocation.City.ToUpper();
                ArrayList alResults = SearchLocation(liLocation);

                // Stop if the user has requested us to do so
                //
                if (m_bUserStopRequested)
                    break;

                // Stop if we are having connection issues after multiple attempts (this is set in SearchLocation())
                //
                if (m_bConnectionTimeoutError)
                    break;

                // If no results were found at this location, skip the output logic.
                //
                if (alResults.Count == 0)
                    continue;

                // Output the city/state/result count header for this location
                //
                siSearch.HTMLSearchResults += "<tr><td colspan=\"2\"><h4 class=\"ban\" style=\"text-align:left;\"><a href=\"" + liLocation.URL + "\" target=\"_blank\">" + liLocation.City.ToUpper() + ", " + liLocation.State.ToUpper() + "</a>: " + alResults.Count.ToString() + " item(s)</h4></td></tr>\r\n";

                // Output the result lines
                //
                foreach (SearchResultInfo srResult in alResults)
                {
                    siSearch.HTMLSearchResults += "<tr>";
                    siSearch.HTMLSearchResults += "<td width=\"5%\" nowrap>" + srResult.Date + "</td>";
                    siSearch.HTMLSearchResults += "<td><a href=\"" + srResult.URL + "\" target=\"_blank\">" + srResult.Title + "</a></td>";
                    siSearch.HTMLSearchResults += "</tr>\r\n";
                    ++siSearch.ResultCount;
                }
            }
            siSearch.HTMLSearchResults += "</table>\r\n";

            // Signal to the UI thread that we're done
            //
            siSearch.SearchCompleted = true;
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

    public class SearchInfo
    {
        private ArrayList m_alLocations = new ArrayList();
        private int m_iCurrentLocationNumber = 0;
        private string m_sCurrentLocationName = "";
        private int m_iResultCount = 0;
        private bool m_bSearchCompleted = false;
        private string m_sHTMLSearchResults = "";

        public SearchInfo() { }
        public SearchInfo(ArrayList alLocations)
        {
            m_alLocations = alLocations;
        }

        public ArrayList Locations
        {
            get { return m_alLocations; }
            set { m_alLocations = value; }
        }

        public int CurrentLocationNumber
        {
            get { return m_iCurrentLocationNumber; }
            set { m_iCurrentLocationNumber = value; }
        }

        public string CurrentLocationName
        {
            get { return m_sCurrentLocationName; }
            set { m_sCurrentLocationName = value; }
        }

        public int ResultCount
        {
            get { return m_iResultCount; }
            set { m_iResultCount = value; }
        }

        public bool SearchCompleted
        {
            get { return m_bSearchCompleted; }
            set { m_bSearchCompleted = value; }
        }

        public string HTMLSearchResults
        {
            get { return m_sHTMLSearchResults; }
            set { m_sHTMLSearchResults = value; }
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
