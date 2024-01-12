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
using System.Collections.Generic;
using System.Net.Http;

namespace LMS2.components
{
    public class PCSTranslation
    {
        public static string GetMessage(string id)
        {
            string msg = "";
            if (Messages.ContainsKey(id.ToLower()))
                msg = (UseAltLang) ? Messages[id.ToLower()].AltMsg : Messages[id.ToLower()].Msg;
            return msg;
        }

        public static Dictionary<string, string> ColumnTranslations
        {
            get
            {
                Dictionary<string, string> _colTrans = new Dictionary<string, string>();
                DataTable dt = HttpContext.Current.Session["ColumnTranslations"] as DataTable;

                if (dt != null)
                    foreach (DataRow r in dt.Rows)
                        _colTrans[r["Text"].ToString().ToLower()] = (r["AltText"].ToString().Length > 0) ? r["AltText"].ToString() : r["Text"].ToString();

                return _colTrans;
            }
        }

        public static Dictionary<string, DataColumn> ColumnDefinitions
        {
            get
            {
                Dictionary<string, DataColumn> _cols = new Dictionary<string, DataColumn>();
                DataTable dt = HttpContext.Current.Session["ColumnDefinitions"] as DataTable;

                if (dt != null)
                    foreach (DataColumn col in dt.Columns)
                        _cols[col.ColumnName.ToLower()] = col;

                return _cols;
            }
        }


        public static bool UseAltLang { get { return (Session("AltLanguageIndicator") == "1") ? true : false; } }

        private class MessageEntry
        {
            public string Key { get; set; }
            public string Msg { get; set; }
            public string AltMsg { get; set; }
        }

        private static Dictionary<string, MessageEntry> Messages
        {
            get
            {
                Dictionary<string, MessageEntry> _messages = HttpContext.Current.Session["MessageTranslations"] as Dictionary<string, MessageEntry>;
                if (_messages == null)
                {
                    _messages = new Dictionary<string, MessageEntry>();
                    DataSet ds = SASWrapper.QueryData(Session("DatabaseName"), "AppsAlias", "Name, Alias, AltLanguageAlias", "ObjectType='message'");
                    if (ds != null && ds.Tables.Count > 0)
                        foreach (DataRow r in ds.Tables[0].Rows)
                            _messages.Add(r[0].ToString().ToLower(), new MessageEntry { Key = r[0].ToString(), Msg = r[1].ToString(), AltMsg = r[2].ToString() });

                    HttpContext.Current.Session["MessageTranslations"] = _messages;
                }

                return _messages;
            }
        }

        private static string Session(string id)
        {
            return (HttpContext.Current.Session[id] != null) ? HttpContext.Current.Session[id].ToString() : "";
        }
    }

    public class TranslateRequest
    {
        private class RequestRow
        {
            public string ID { get; set; }
            public string iType { get; set; }
            public string Text { get; set; }
        }

        List<RequestRow> _rows = new List<RequestRow>();

        public void Add(string id, string type, string text)
        {
            _rows.Add(new RequestRow() { ID = id, iType = type, Text = text });
        }

        public DataTable GetTable()
        {
            DataTable dt = new DataTable("TranslationRequest");
            dt.Columns.Add("ID");
            dt.Columns.Add("Type");
            dt.Columns.Add("Text");
            dt.Columns.Add("AltText");

            foreach (RequestRow r in _rows)
            {
                DataRow row = dt.NewRow();
                row.ItemArray = new string[] { r.ID, r.iType, r.Text, "" };
                dt.Rows.Add(row);
            }

            return dt;
        }
    }

}