using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

/// <summary>
/// Summary description for ContentHelper
/// </summary>
public class ContentHelper
{
    private static string MasterErrorMessage = string.Empty;
    private static MasterPage mp;

    /// <summary>
    /// Set Error Message
    /// </summary>
    public static string ErrorMessage
    {
        set { MasterErrorMessage = value; }
    }

    public static void BuildMainContent(MasterPage page)
    {
        ((IBasePageDefault)page).ErrorMessage = MasterErrorMessage;
        mp = page;
    }

    public static void SetMainContent(string error)
    {
        if (mp != null)
            ((IBasePageDefault)mp).ErrorMessage = error;
        else
            throw new ApplicationException(error);
    }
}
