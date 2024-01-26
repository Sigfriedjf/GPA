using LMS2.components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GPA
{
    public partial class HelpPage : System.Web.UI.Page
    {
        string sDatabaseName = string.Empty;
        string UserID = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            string sFunctionID = Request.QueryString["FunctionID"];
            string sPageID = Request.QueryString["PageID"];
            DataSet dsWork = new DataSet();
            string error = string.Empty;
            bool includeDetail = false;

            if (Session["UserID"] == null)
            {
                dsWork = SASWrapper.InitializeWebPage(string.Empty, string.Empty, string.Empty);
                if (dsWork != null && dsWork.Tables.Count > 0)
                {
                    DataTable dt = dsWork.Tables[0];
                    if (dt.Columns.Contains("Result"))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["Name"].ToString() == "DatabaseName") sDatabaseName = row["Result"].ToString();
                            if (row["Name"].ToString() == "UserID") UserID = row["Result"].ToString();
                        }
                    }
                }
            }
            else
            {
                sDatabaseName = Session["DatabaseName"].ToString();
                UserID = Session["UserID"].ToString();
            }

            if (sFunctionID != null)
            {
                dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetHelpCommon", new string[] { "@UserID", "@FunctionID" }, new string[] { (UserID != null) ? UserID : "", sFunctionID }, sDatabaseName, ref error);

                if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
                {
                    lblCommon.Text = ConvertTags(dsWork.Tables[0].Rows[0][0].ToString());
                    lblPageHeading.Text = sFunctionID;
                    this.Title = sFunctionID;
                }
            }

            if (sPageID != null)
            {
                dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetHelpPage", new string[] { "@UserID", "@PageID" }, new string[] { (UserID != null) ? UserID : "", sPageID }, sDatabaseName, ref error);

                if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
                {
                    lblCommon.Text = ConvertTags(dsWork.Tables[0].Rows[0][0].ToString());
                    lblHeader.Text = ConvertTags(dsWork.Tables[0].Rows[0][1].ToString());
                    lblPageHeading.Text = dsWork.Tables[0].Rows[0][2].ToString() + " [" + sPageID + "]";
                    includeDetail = int.Parse(dsWork.Tables[0].Rows[0][3].ToString()) == 1 ? true : false;
                    this.Title = sPageID + " " + lblPageHeading.Text;
                }
                if (includeDetail)
                {
                    GridViewContent.Visible = true;
                    dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetHelpDetails", new string[] { "@UserID", "@PageID" }, new string[] { (UserID != null) ? UserID : "", sPageID }, sDatabaseName, ref error);
                    GridView1.DataSource = dsWork;
                    GridView1.DataBind();
                    GridView1.HeaderRow.Visible = false;
                }
                TranslateTitles();
            }
        }

        private string ConvertTags(string content)
        {
            content = content.Replace("\r\n", "<br/>");

            int indexOpenTag = content.IndexOf("[img]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/img]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[img]", "<img src=\"Images/");
                    content = content.Replace("[/img]", "\" />");
                }
            }
            indexOpenTag = content.IndexOf("[ img ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ img ]", "[img]");
            indexOpenTag = content.IndexOf("[ /img ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /img ]", "[/img]");

            indexOpenTag = content.IndexOf("[b]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/b]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[b]", "<b>");
                    content = content.Replace("[/b]", "</b>");
                }
            }

            indexOpenTag = content.IndexOf("[ b ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ b ]", "[b]");
            indexOpenTag = content.IndexOf("[ /b ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /b ]", "[/b]");

            indexOpenTag = content.IndexOf("[i]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/i]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[i]", "<i>");
                    content = content.Replace("[/i]", "</i>");
                }
            }

            indexOpenTag = content.IndexOf("[ i ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ i ]", "[i]");
            indexOpenTag = content.IndexOf("[ /i ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /i ]", "[/i]");

            indexOpenTag = content.IndexOf("[u]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/u]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[u]", "<u>");
                    content = content.Replace("[/u]", "</u>");
                }
            }

            indexOpenTag = content.IndexOf("[ u ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ u ]", "[u]");
            indexOpenTag = content.IndexOf("[ /u ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /u ]", "[/u]");

            indexOpenTag = content.IndexOf("[hr]");
            if (indexOpenTag > -1)
                content = content.Replace("[hr]", "<hr>");
            indexOpenTag = content.IndexOf("[ hr ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ hr ]", "[hr]");

            indexOpenTag = content.IndexOf("[red]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/red]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[red]", "<font color=\"red\">");
                    content = content.Replace("[/red]", "</font>");
                }
            }
            indexOpenTag = content.IndexOf("[ red ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ red ]", "[red]");
            indexOpenTag = content.IndexOf("[ /red ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /red ]", "[/red]");

            indexOpenTag = content.IndexOf("[blue]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/blue]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[blue]", "<font color=\"blue\">");
                    content = content.Replace("[/blue]", "</font>");
                }
            }
            indexOpenTag = content.IndexOf("[ blue ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ blue ]", "[blue]");
            indexOpenTag = content.IndexOf("[ /blue ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /blue ]", "[/blue]");

            indexOpenTag = content.IndexOf("[brown]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/brown]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[brown]", "<font color=\"brown\">");
                    content = content.Replace("[/brown]", "</font>");
                }
            }
            indexOpenTag = content.IndexOf("[ brown ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ brown ]", "[brown]");
            indexOpenTag = content.IndexOf("[ /brown ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /brown ]", "[/brown]");

            indexOpenTag = content.IndexOf("[green]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/green]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[green]", "<font color=\"green\">");
                    content = content.Replace("[/green]", "</font>");
                }
            }
            indexOpenTag = content.IndexOf("[ green ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ green ]", "[green]");
            indexOpenTag = content.IndexOf("[ /green ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /green ]", "[/green]");

            indexOpenTag = content.IndexOf("[purple]");
            if (indexOpenTag > -1)
            {
                int indexEndTag = content.IndexOf("[/purple]", indexOpenTag);
                if (indexEndTag > -1)
                {
                    content = content.Replace("[purple]", "<font color=\"purple\">");
                    content = content.Replace("[/purple]", "</font>");
                }
            }
            indexOpenTag = content.IndexOf("[ purple ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ purple ]", "[purple]");
            indexOpenTag = content.IndexOf("[ /purple ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /purple ]", "[/purple]");

            int loopcnt = 1;
            while (loopcnt > 0)
            {
                indexOpenTag = content.IndexOf("[functionID=\"");
                if (indexOpenTag > -1)
                {
                    int indexEndTag = content.IndexOf("[/functionID]", indexOpenTag);
                    if (indexEndTag > -1)
                    {
                        int indexEndQuote = content.IndexOf("\"]", indexOpenTag);
                        if (indexEndQuote > -1)
                        {
                            string tempcontent = content.Substring(0, indexEndTag + 13);
                            string url = content.Substring(indexOpenTag + 13, indexEndQuote - (indexOpenTag + 13));
                            tempcontent = tempcontent.Replace("[functionID=\"" + url + "\"]", "<a href=\"HelpPage.aspx?FunctionID=" + url + "\"><b>");
                            tempcontent = tempcontent.Replace("[/functionID]", "</b></a>");
                            content = tempcontent + content.Substring(indexEndTag + 13, content.Length - (indexEndTag + 13));
                        }
                    }
                    else loopcnt = 0;
                }
                else loopcnt = 0;
            }

            loopcnt = 1;
            while (loopcnt > 0)
            {
                indexOpenTag = content.IndexOf("[pageID=\"");
                if (indexOpenTag > -1)
                {
                    int indexEndTag = content.IndexOf("[/pageID]", indexOpenTag);
                    if (indexEndTag > -1)
                    {
                        int indexEndQuote = content.IndexOf("\"]", indexOpenTag);
                        if (indexEndQuote > -1)
                        {
                            string tempcontent = content.Substring(0, indexEndTag + 9);
                            string url = content.Substring(indexOpenTag + 9, indexEndQuote - (indexOpenTag + 9));
                            tempcontent = tempcontent.Replace("[pageID=\"" + url + "\"]", "<a href=\"HelpPage.aspx?PageID=" + url + "\"><b>");
                            tempcontent = tempcontent.Replace("[/pageID]", "</b></a>");
                            content = tempcontent + content.Substring(indexEndTag + 9, content.Length - (indexEndTag + 9));
                        }
                    }
                    else loopcnt = 0;
                }
                else loopcnt = 0;
            }

            indexOpenTag = content.IndexOf("[ pageID=");
            if (indexOpenTag > -1)
                content = content.Replace("[ pageID=", "[pageID=");
            indexOpenTag = content.IndexOf("[ /pageID ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /pageID ]", "[/pageID]");

            indexOpenTag = content.IndexOf("[ functionID=");
            if (indexOpenTag > -1)
                content = content.Replace("[ functionID=", "[functionID=");
            indexOpenTag = content.IndexOf("[ /functionID ]");
            if (indexOpenTag > -1)
                content = content.Replace("[ /functionID ]", "[/functionID]");

            return Server.UrlDecode(content);
        }

        private void TranslateTitles()
        {
            tblHeaderTitle.Visible = true;
            tblCommonTitle.Visible = true;
            tblItemsTitle.Visible = true;
            TranslateRequest tr = new TranslateRequest();
            Label[] lblArray = { lblHeaderTitle, lblCommonTitle, lblItemsTitle };

            foreach (Label lbl in lblArray) tr.Add("", "label", lbl.Text);

            DataTable translationTable = tr.GetTable();
            translationTable = SASWrapper.TranslateStrings(sDatabaseName, PCSTranslation.UseAltLang, translationTable);
            int idx = 0;
            foreach (DataRow row in translationTable.Rows)
            {
                string text = row[2].ToString().Trim();
                string altText = row[3].ToString().Trim();
                if (altText.Length == 0)
                    altText = text;
                lblArray[idx].Text = altText;
                idx = idx + 1;
            }
        }
    }
}