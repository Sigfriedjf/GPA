using LMS2.components;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GPA
{

    public partial class Controls_TabStripUC : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set defaults for the tabstrip
                PopulateTabStrip();
            }
        }

        /// <summary>
        /// Tab clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Menu1_MenuItemClick(object sender, MenuEventArgs e)
        {
            string sValue = string.Empty;
            string sURL = string.Empty;
            string sFilterID = string.Empty;
            string sRefFilterID = string.Empty;
            sValue = e.Item.Value;
            DataSet dsWork = new DataSet();
            DataTable dt = new DataTable();
            string error = string.Empty;

            // Run stored proc to get the page to redirect to
            dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetTabStripURL", new string[] { "@TabValue" }, new string[] { sValue }, Session["DatabaseName"].ToString(), ref error);

            if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
            {
                if (dsWork.Tables[0].Rows[0].ItemArray[1] != null) Session["PageScopePageID"] = dsWork.Tables[0].Rows[0][1].ToString();
                if (dsWork.Tables[0].Rows[0].ItemArray[3] != null) sURL = (string)dsWork.Tables[0].Rows[0].ItemArray[3];
                if (dsWork.Tables[0].Rows[0].ItemArray[4] != null) sFilterID = (string)dsWork.Tables[0].Rows[0].ItemArray[4];
                if (dsWork.Tables[0].Rows[0].ItemArray[5] != null) sRefFilterID = (string)dsWork.Tables[0].Rows[0].ItemArray[5];
                if (sRefFilterID.Length > 0 && Request.QueryString["PageID"].Length > 0)
                {
                    string[] strArray = sRefFilterID.ToLower().Split(",".ToCharArray());
                    if (strArray.Contains<String>(Request.QueryString["PageID"].ToLower()))
                        sRefFilterID = Request.QueryString["PageID"];
                }
                sFilterID = GetFilterableValue(sRefFilterID.Trim(), sFilterID);
                if (!string.IsNullOrEmpty(sFilterID)) sURL = sURL + sFilterID;
                if (!string.IsNullOrEmpty(sURL)) Response.Redirect(sURL);
            }
        }

        private void PopulateTabStrip()
        {
            DataSet dsWork = new DataSet();
            DataTable dt = new DataTable();
            string error = string.Empty;
            string sFile = Request.QueryString["NodeID"];
            if (string.IsNullOrEmpty(sFile))
                sFile = Path.GetFileNameWithoutExtension(Request.FilePath);

            dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetTabNodes", new string[] { "@UserID", "@NodeID" }, new string[] { (Session["UserID"] != null) ? Session["UserID"].ToString() : "", sFile }, (Session["DatabaseName"] != null) ? Session["DatabaseName"].ToString() : "LMS2", ref error);
            if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
                PopulateNodes(dsWork.Tables[0].Copy(), sFile);
            else
                divTabs.Visible = false;
        }

        private void PopulateNodes(DataTable dt, string file)
        {
            foreach (DataRow dr in dt.Rows)
            {
                MenuItem miTab = new MenuItem();
                miTab.Text = dr["Alias"].ToString();
                miTab.Value = dr["NodeID"].ToString();
                if (miTab.Value == file) miTab.Selected = true;
                miTab.Selectable = true;
                Menu1.Items.Add(miTab);
            }
        }

        private string GetFilterableValue(string pageID, string colName)
        {
            string val = ""; string filterCol = "";
            bool applyAsFilter = false;
            Session["PageIsFilterScoped"] = false;
            Session["PageScopeColumn"] = "";
            Session["PageScopeFilterColumn"] = "";
            Session["PageScopeValue"] = "";

            if (pageID.Length > 0 && BasePage._Session(pageID + "QueryKeyColumn").Length > 0 && BasePage._Session(pageID + "SelectedItemKey").Length > 0)
            {
                if (colName.StartsWith("~~")) // This should be applied as a filter rather than a selection...
                {
                    Session["PageIsFilterScoped"] = true;
                    applyAsFilter = true;
                    string[] cols = colName.Substring(2).Split(new char[] { ',' });
                    colName = cols[0];
                    filterCol = cols[1];

                    // When moving to child objects, carry security in case they don't have their own...
                    Session["PassedParentState"] = (PCSSecurity.PageHasOrderStateLogic) ? PCSSecurity.GetOrderStateOfSelectedItem() : Session["PassedParentState"];
                }

                string filter = string.Format("({0}='{1}')", Utilities.PrepSQLStringParam(BasePage._Session(pageID + "QueryKeyColumn")), Utilities.PrepSQLStringParam(BasePage._Session(pageID + "SelectedItemKey")));
                bool isStoredProc = (BasePage._Session(pageID + "StoredProcForSelect").Length > 0);
                string displayView = (isStoredProc) ? BasePage._Session(pageID + "StoredProcForSelect") : BasePage._Session(pageID + "DisplayView");
                DataSet ds = SASWrapper.QueryData(pageID, BasePage._Session("DatabaseName", "LMS2"), displayView, isStoredProc, colName, filter, "");
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    val = ds.Tables[0].Rows[0][0].ToString();
                    Session["PageScopeColumn"] = filterCol; // BasePage._Session(pageID + "QueryKeyColumn");
                    Session["PageScopeFilterColumn"] = filterCol;
                    Session["PageScopeValue"] = val; // BasePage._Session(pageID + "SelectedItemKey");
                    Session["PageScopeRefPageID"] = pageID;

                    Dictionary<string, DropDownColumn> ddCols = (Session[pageID + "DropDownColumns"] != null) ? (Dictionary<string, DropDownColumn>)Session[pageID + "DropDownColumns"] : new Dictionary<string, DropDownColumn>();
                    if (ddCols.ContainsKey(colName.ToLower()))
                        val = ddCols[colName.ToLower()].GetValueFromText(val);

                    if (applyAsFilter)
                        val = string.Format("{0}='{1}'", filterCol, val);

                    val = string.Format("{0}={1}", (applyAsFilter) ? "&Filter" : "&Selection", Server.UrlEncode(val));
                }

            }

            return val;
        }

    }
}