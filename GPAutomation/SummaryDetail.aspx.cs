using LMS2.components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LMS2.components;

namespace GPAutomation
{
    public partial class SummaryDetail : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ChangeUserPostbackTimer_Tick(object sender, EventArgs e)
        {
            /*
            if (!UserChanged)
            {
                Session["UserID"] = null;
                UserChanged = true;
                Response.StatusCode = 401;
                Response.StatusDescription = "Unauthorized";
                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            // */
        }

        protected void ChangeUser(object sender, EventArgs e)
        {
            /*
            if (!UserChanged)
            {
                UserChangeTimer = true;
            }
            // */
        }
    }
}