using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace LMS2.components
{
    public class ColorManager
    {
        static public string ColorBackground
        {
            get
            {
                if (HttpContext.Current.Session["ColorBackground"] == null)
                    return "#CCEBE6";
                return "#" + (string)HttpContext.Current.Session["ColorBackground"];
            }
        }

        static public string ColorBody
        {
            get
            {
                if (HttpContext.Current.Session["ColorBody"] == null)
                    return "#C6E0DF";
                return "#" + (string)HttpContext.Current.Session["ColorBody"];
            }
        }

        static public string ColorBorder
        {
            get
            {
                if (HttpContext.Current.Session["ColorBorder"] == null)
                    return "#77B3B1";
                return "#" + (string)HttpContext.Current.Session["ColorBorder"];
            }
        }

        static public string ColorGridHeader
        {
            get
            {
                if (HttpContext.Current.Session["ColorGridHeader"] == null)
                    return "#3E9788";
                return "#" + (string)HttpContext.Current.Session["ColorGridHeader"];
            }
        }

        static public string ColorGridSelectedRow
        {
            get
            {
                if (HttpContext.Current.Session["ColorGridSelectedRow"] == null)
                    return "#96d4c9";
                return "#" + (string)HttpContext.Current.Session["ColorGridSelectedRow"];
            }
        }

        static public string ColorNavigation
        {
            get
            {
                if (HttpContext.Current.Session["ColorNavigation"] == null)
                    return "#5A9A98";
                return "#" + (string)HttpContext.Current.Session["ColorNavigation"];
            }
        }

        static public string ColorTitleBar
        {
            get
            {
                if (HttpContext.Current.Session["ColorTitleBar"] == null)
                    return "#3E9788";
                return "#" + (string)HttpContext.Current.Session["ColorTitleBar"];
            }
        }

    }
}
