using Microsoft.Reporting.WebForms;
using System;
using System.Configuration;

namespace LMS2.components
{
    public class PCSReportViewerCredentials : IReportServerCredentials 
	{
		public PCSReportViewerCredentials()
		{
		}

		public bool GetFormsCredentials(out System.Net.Cookie authCookie, out string userName, out string password, out string authority)
		{
			authCookie = null;
			userName = password = authority = null;
			return false;
		}

		public System.Security.Principal.WindowsIdentity ImpersonationUser
		{
			get { return null; }
		}

		public System.Net.ICredentials NetworkCredentials
		{
			get 
			{
				string userName =
					ConfigurationManager.AppSettings
						["ReportViewerUser"];
				if (string.IsNullOrEmpty(userName))
					throw new Exception(
						"Missing user name from web.config file");

				string password =
					ConfigurationManager.AppSettings
						["ReportViewerPassword"];
				if (string.IsNullOrEmpty(password))
					throw new Exception(
						"Missing password from web.config file");

				string domain =
					ConfigurationManager.AppSettings
						["ReportViewerDomain"];
				if (string.IsNullOrEmpty(domain))
					throw new Exception(
						"Missing domain from web.config file");

				//return new System.Net.NetworkCredential("lms2reports", "R3476r3476R", _domain);
				//return new System.Net.NetworkCredential("gcontrol", "gcntrl09", "ATMOS");
				return new System.Net.NetworkCredential(userName, password, domain);
			}
		}
	}


}