using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LMS2.components;

namespace GPAutomation
{
    public partial class TagSummaryDetail : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ibConfigFilter_Click(object sender, ImageClickEventArgs e)
        {
            //string redirUrl = string.Format("ColumnSettings.aspx?PageID={0}", Server.UrlEncode(_Session("CurPageName")));
            string redirUrl = "#";
            Response.Redirect(redirUrl);
        }

        protected void ibExcel_Click(object sender, ImageClickEventArgs e)
        {
            //ExportToExcel(Master.SQLText.Text);
        }

        protected void ibHist_Click(object sender, ImageClickEventArgs e)
        {
            //eHistoryPage();
        }

        protected void ibDetails_Click(object sender, ImageClickEventArgs e)
        {
            /*
            string redirUrl = "";
            if (_Session("SpecialDetail").Length > 0)
            {
                string detailsPage = _Session("SpecialDetail");
                redirUrl = string.Format("{0}{1}Selection={2}", detailsPage, (detailsPage.IndexOf('?') >= 0) ? "&" : "?", Server.UrlEncode(_Session("SelectedItemKey")));
            }
            else
                redirUrl = string.Format("Detail.aspx?PageID={0}&NodeID={1}&Selection={2}", Server.UrlEncode(_Session("CurPageName")), Request.QueryString["NodeID"], Server.UrlEncode(_Session("SelectedItemKey")));
            if (redirUrl.Length > 0)
                Response.Redirect(redirUrl);

            // */
        }

        protected void ibBulkEdit_Click(object sender, ImageClickEventArgs e)
        {
            /*
            Session["BulkEditQuickFilter"] = Master.QuickFilter.Text;
            string redirUrl = "";
            if (_Session("BulkEdit").Length > 0)
            {
                string editPage = _Session("BulkEdit");
                redirUrl = string.Format("{0}{1}", editPage, (editPage.IndexOf('?') >= 0) ? "&" : "?");
            }
            if (redirUrl.Length > 0)
                Response.Redirect(redirUrl);

            // */
        }

        protected void OnInsertButtonClick(object sender, ImageClickEventArgs e)
        {
            //DetailsView1.ChangeMode(DetailsViewMode.Insert);
        }

        protected void IbRetrieve_Click(object sender, ImageClickEventArgs e)
        {
           
        }
    }
}