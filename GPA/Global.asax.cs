using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace GPA
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            Application["LicenseManager"] = new LMS2.components.LicenseManager();

            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_End(object sender, EventArgs e)
        {
        }

        void Application_Error(object sender, EventArgs e)
        {
        }

        void Session_Start(object sender, EventArgs e)
        {
            LMS2.components.LicenseManager lm = (LMS2.components.LicenseManager)Application["LicenseManager"];
            if (lm == null) // should never happen, but I've seen it occur in the debugger before
            {
                lm = new LMS2.components.LicenseManager();
                Application["LicenseManager"] = lm;
            }

            lm.ConnectSession(Session.SessionID,
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.Request.UserHostName,
                HttpContext.Current.Request.UserHostAddress,
                HttpContext.Current.Request.UserAgent
                );

            Session.Timeout = 30;
        }

        void Session_End(object sender, EventArgs e)
        {
            LMS2.components.LicenseManager lm = (LMS2.components.LicenseManager)Application["LicenseManager"];
            lm.DisconnectSession(Session.SessionID);
        }
    }
}