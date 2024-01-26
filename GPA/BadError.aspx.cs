using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using LMS2.components;

namespace GPA
{
        public partial class BadError : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((LMSGridViewFilter)(Master)).FilteringEnabled = false;

            if (Request.QueryString["PageID"] != null)
            {
                // Clean out the users filter value in the very, very, very, rare case that there might be a problem with the filter value
                SASWrapper.ExecuteQuery(Session["DatabaseName"].ToString(), "UPDATE AppsQueryPreference SET Filter = '' WHERE PageID = '" + Request.QueryString["PageID"] + "' AND UserID = '" + UserID + "'");
                Session["Filter"] = string.Empty;
            }

            foreach (string key in Request.QueryString.AllKeys)
            {
                if (key != "NodeID")
                {
                    TableRow tr = new TableRow();
                    tr.Cells.Add(new TableHeaderCell { Text = key });
                    tr.Cells.Add(new TableCell { Text = Request.QueryString[key] });
                    Table1.Rows.Add(tr);
                }
            }
        }
    }
}