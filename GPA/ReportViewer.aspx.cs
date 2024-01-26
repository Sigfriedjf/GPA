using LMS2.components;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace GPA
{
    public partial class ReportViewer : BasePage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // Report pages need to disable partial rendering to eliminate JavaScript error that happens when the View Report button is clicked
            var ScriptManager1 = (ScriptManager)Page.Master.FindControl("ScriptManager1");
            ScriptManager1.EnablePartialRendering = false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["TabStrip"] != null)
                TabStrip.Visible = true;

            // if we are looking at an archive database, then hide the report printing b/c it is not supported.
            if (DatabaseName.ToLower() != "lms2")
                ReportPrintPanel.Visible = false;

            lbPrintMsg.Visible = false;
            lbNoPrinter.Visible = false;

            if (!IsPostBack)
            {
                ReportViewer1.ProcessingMode = ProcessingMode.Remote;
                ServerReport sr = ReportViewer1.ServerReport;
                //sr.ReportServerCredentials = new PCSReportViewerCredentials(((BasePage)Page).FacilityID);
                sr.ReportServerCredentials = new PCSReportViewerCredentials();
                sr.ReportServerUrl = new Uri(Session["ReportServerURL"].ToString());

                if (UseAltLanguage)
                    sr.ReportPath = "/" + DatabaseName + "/Reports_ALT/" + Request.QueryString["PageID"];
                else
                    sr.ReportPath = "/" + DatabaseName + "/Reports/" + Request.QueryString["PageID"];

                // Pack any query string values not PageID or NodeID off as report parameters
                List<ReportParameter> paramList = new List<ReportParameter>();
                bool Collapsed = true;
                foreach (string key in Request.QueryString.Keys)
                {
                    if (key.ToLower() != "pageid" && key.ToLower() != "nodeid" && key.ToLower() != "tabstrip")
                        paramList.Add(new ReportParameter(key, Request.QueryString[key]));
                    if (key.ToLower() == "pageid" && (Request.QueryString["pageid"].ToLower() == "rpt102" || Request.QueryString["pageid"].ToLower() == "rpt106"))
                    {
                        paramList.Add(new ReportParameter("UserID", UserID));
                        paramList.Add(new ReportParameter("FilterCriteria", _Session("TagFilterSQL")));
                        Session["TagFilterSQL"] = string.Empty;
                    }
                    if (key.ToLower() == "pageid" && Request.QueryString["pageid"].ToLower() == "rpt105")
                    {
                        Collapsed = false;
                        paramList.Add(new ReportParameter("UserID", UserID));
                    }
                }
                if (paramList.Count > 0)
                    sr.SetParameters(paramList);

                ReportViewer1.PromptAreaCollapsed = ((paramList.Count > 0) && Collapsed);
                //ReportViewer1.ShowParameterPrompts = (paramList.Count > 0);

                ReportViewer1.ShowPrintButton = false;

                DataTable dt = DropDownColumn.GetComboValues("v_ReportPrinters_lb", "");
                if (dt != null)
                {
                    foreach (DataRow r in dt.Rows)
                        ddlPrinter.Items.Add(new ListItem(r["DisplayText"].ToString(), r["Value"].ToString()));
                    ddlPrinter.SelectedValue = _Session("DefaultPrinter");
                }
            }
        }

        protected void ibPrint_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlPrinter.Text.Length > 0)
            {
                ServerReport sr = ReportViewer1.ServerReport;
                ReportParameterInfoCollection pic = sr.GetParameters();

                string xml = "";
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlTextWriter xtw = new XmlTextWriter(sw))
                    {
                        xtw.WriteStartElement("RP");
                        foreach (ReportParameterInfo rp in pic)
                        {
                            xtw.WriteStartElement("P");
                            xtw.WriteAttributeString("name", rp.Name);
                            xtw.WriteAttributeString("val", (rp.Values.Count > 0) ? rp.Values[0] : "");
                            xtw.WriteEndElement();
                        }
                        xtw.WriteEndElement();
                        xtw.Close();
                    }
                    sw.Flush();
                    xml = sw.ToString();
                    sw.Close();
                }

                SASWrapper.ExecuteQuery("LMS2", string.Format("INSERT INTO ReportsToPrint (Report, Printer, Parameters) VALUES ('{0}','{1}','{2}')",
                    Request.QueryString["PageID"], Utilities.PrepSQLStringParam(ddlPrinter.Text), xml));

                if (_Session("DefaultPrinter").ToLower() != ddlPrinter.Text.ToLower())
                    SASWrapper.ExecuteQuery("LMS2", string.Format("UPDATE AppsUserPreference SET DefaultPrinter=('{0}') WHERE UserID='{1}'", ddlPrinter.Text, UserID));

                lbPrintMsg.Visible = true;
            }
            else
                lbNoPrinter.Visible = true;
        }


        //public class PCSReportViewerCredentials : IReportServerCredentials
        //{
        //  private readonly string _domain;

        //  public PCSReportViewerCredentials(string domain)
        //  {
        //    _domain = domain;
        //  }

        //  public bool GetFormsCredentials(out System.Net.Cookie authCookie, out string userName, out string password, out string authority)
        //  {
        //    authCookie = null;
        //    userName = password = authority = null;
        //    return false;
        //  }

        //  public System.Security.Principal.WindowsIdentity ImpersonationUser
        //  {
        //    get { return null; }
        //  }

        //  public System.Net.ICredentials NetworkCredentials
        //  {
        //    get
        //    {
        //      return new System.Net.NetworkCredential("john.fairbanks", "Beaumont3", _domain);
        //    }
        //  }
        //}

    }
}