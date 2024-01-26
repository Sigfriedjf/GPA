using LMS2.components;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GPA
{
    public partial class Home : BasePage
    {
        protected override string UserIDDisplay { set { lblUserIDval.Text = value; } }
        //protected override bool UserChangeTimer { get { return tmUserChangeTimer.Enabled; } set { tmUserChangeTimer.Enabled = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int MajorVersion = 2;
                int MinorVersion = 0;
                int Revision = 0;
                //lblVersionval.Text = "2.0.01214";
                //Assembly web = Assembly.Load("App_Code");
                //Version would be in AssemblyInfo.cs but we are not using it
                //AssemblyName webName = web.GetName();
                //lblVersionval.Text = webName.Version.ToString();

                //FileInfo TheFile = new FileInfo(web.Location);
                //if (TheFile.Exists)
                //    Revision = Convert.ToInt32(TheFile.LastWriteTime.ToString("yyMMdd"));
                //lblVersionval.Text = MajorVersion + "." + MinorVersion + "." + Revision;

                string sUserID = "AdminRick";
                Session["UserID"] = sUserID;
                lblUserIDval.Text = Session["UserID"].ToString();

                lblDBNameServerval.Text = Session["ServerName"].ToString() + " > " + Session["DatabaseName"].ToString();

                int daysToExp = 100;// (int)Session["PasswordDaysToExpire"];
                if (daysToExp >= 0)
                    lbPasswordExp.Text = string.Format(PCSTranslation.GetMessage("PWDAlmostExp"), daysToExp);
                lbPasswordExp.Text += "&nbsp;&nbsp;";

                if (NoPasswordExpiration)
                {
                    lbPasswordExp.Visible = false;
                    ChgPassword.Visible = false;
                }

                if (Session["FacilityID"] != null && Session["FacilityID"].ToString().IndexOf("WAK") >= 0)
                {
                    if (UseAltLanguage)
                        imgBackground.ImageUrl = "~/Images/defaultbkgrd.jpg";
                    else
                        imgBackground.ImageUrl = "~/Images/defaultbkgrd.jpg";
                }
                else
                    if (Session["HomePageLogoFile"] != null)
                    imgBackground.ImageUrl = "~/Images/defaultbkgrd_WBI.jpg";// + Session["HomePageLogoFile"];
                else
                    imgBackground.ImageUrl = "~/Images/defaultbkgrd.jpg";
                BindData();
            }
        }

        private void BindData()
        {
            string sError = "";
            DataSet dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetAnnouncements", null, null, Session["DatabaseName"].ToString(), ref sError);
            GridView1.DataSource = dsWork;
            GridView1.DataBind();

            for (int i = 0; i < GridView1.Rows.Count; i++)
            {
                for (int j = 0; j < GridView1.Rows[i].Cells.Count; j++)
                {
                    GridView1.Rows[i].Cells[j].Text = Server.HtmlDecode(GridView1.Rows[i].Cells[j].Text);
                }
            }
        }

        protected void OnChgPassword(object sender, EventArgs e)
        {
            RedirectToSecurePasswordChange();
        }
    }
}