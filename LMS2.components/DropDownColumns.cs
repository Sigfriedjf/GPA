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
using System.Text.RegularExpressions;
using System.Net.Http;


namespace LMS2.components
{
    public class DropDownColumns : Dictionary<string, DropDownColumn>
    {
        public DropDownColumns(string pageID, string config)
        {
            // Special case handling for security pages
            if (pageID == "WA72F1P1")
            {
                Dictionary<string, DataColumn> cols = PCSTranslation.ColumnDefinitions;
                foreach (DataColumn dc in cols.Values)
                    if (dc.ColumnName != "PageID" && dc.ColumnName != "PageHeading")
                        this[dc.ColumnName.ToLower()] = new DropDownColumn { Name = dc.ColumnName, TorV = "v_Permission_lb", ReadOnly = false };
            }
            else if (pageID == "WA72F1P3")
            {
                Dictionary<string, DataColumn> cols = PCSTranslation.ColumnDefinitions;
                foreach (DataColumn dc in cols.Values)
                    if (dc.ColumnName != "UserID")
                        this[dc.ColumnName.ToLower()] = new DropDownColumn { Name = dc.ColumnName, TorV = "v_UserInRole_lb", ReadOnly = false };
            }

            else if (config.Trim().Length > 0)
            {
                foreach (string col in Regex.Split(config, ";"))
                {
                    if (col.Trim().Length > 0)
                    {
                        MatchCollection stuff = Regex.Matches(col.Trim(), "(\"[^" + @"\t\r\n]*" + "\")" + @"|({ *\w+ *, *\w+ *})|({ *\w+ *, *\w+ *, *\w+ *})|(\w+)");
                        DropDownColumn ddcol = new DropDownColumn { Name = stuff[0].Value, TorV = stuff[1].Value, ReadOnly = false };

                        // see if there is additional filter info...
                        for (int i = 2; i < stuff.Count; i++)
                        {
                            // if it starts with a " then it is a query string
                            if (stuff[i].Value.Trim().StartsWith("\""))
                            {
                                DropDownColumn.ValueFilter vf = new DropDownColumn.ValueFilter()
                                {
                                    ColumnName = "",
                                    TieField = "",
                                    Value = Regex.Replace(stuff[i].Value, "[\"]+", ""),
                                    ValueField = ""
                                };
                                ddcol.FilterList[(i - 1).ToString()] = vf;
                            }
                            else if (stuff[i].Value.Trim().StartsWith("{"))
                            {
                                string[] parts = Regex.Replace(stuff[i].Value, @"[{}]", "").Split(',');
                                DropDownColumn.ValueFilter vf = new DropDownColumn.ValueFilter()
                                {
                                    ColumnName = parts[0].Trim(),
                                    TieField = parts[1].Trim(),
                                    Value = "",
                                    ValueField = (parts.Length > 2) ? parts[2].Trim() : ""
                                };
                                ddcol.FilterList[vf.ColumnName.ToLower()] = vf;
                            }
                        }

                        this[stuff[0].Value.Trim().ToLower()] = ddcol;
                    }
                }

            }
        }

        public void SaveFilterValues(SASDetailDataSource dataSource)
        {
            // Ultra sneaky back door I built into our SASDetailDataSource since I couldn't find a way in to get at the data.
            // I could tell you more about it, but then I'd have to kill you.
            if (dataSource != null && dataSource._dataView != null && dataSource._dataView._ds != null && dataSource._dataView._ds.Tables.Count > 0)
            {
                DataTable dt = dataSource._dataView._ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DropDownColumn ddc in Values)
                    {
                        foreach (DropDownColumn.ValueFilter vf in ddc.FilterList.Values)
                        {
                            if (vf.TieField.Length > 0)
                            {
                                if (ContainsKey(vf.TieField.ToLower()))
                                {
                                    // Since we are tied to another drop down, and the value is the DisplayText and we need to convert it to the actual value
                                    // by retriving the value list for the tie drop down.  If ValueField.Length > 0 then we are filtering off of a different
                                    // column that Value
                                    DataTable vdt = this[vf.TieField.ToLower()].GetComboValues();
                                    DataRow[] rows = vdt.Select(string.Format("DisplayText='{0}'", dt.Rows[0][vf.TieField].ToString()));
                                    if (rows.Length > 0)
                                        vf.Value = (vf.ValueField.Length > 0) ? rows[0][vf.ValueField].ToString() : rows[0]["Value"].ToString();
                                    else
                                        vf.Value = dt.Rows[0][vf.TieField].ToString(); // ok not found in list, use as is
                                }
                                else
                                    vf.Value = dt.Rows[0][vf.TieField].ToString();
                            }
                        }
                    }
                }
            }
            else if (dataSource != null && dataSource._dataView != null)
            {
                foreach (DropDownColumn ddc in Values)
                {
                    foreach (DropDownColumn.ValueFilter vf in ddc.FilterList.Values)
                    {
                        if (vf.TieField.Length > 0)
                        {
                            DataTable vdt = this[vf.TieField.ToLower()].GetComboValues();
                            vf.Value = vdt.Rows[0]["Value"].ToString();
                        }
                    }
                }
            }
        }

        public void SaveFilterValue(string columnName, string val)
        {
            foreach (DropDownColumn ddc in Values)
            {
                foreach (DropDownColumn.ValueFilter vf in ddc.FilterList.Values)
                {
                    if (vf.TieField.ToLower() == columnName.ToLower())
                        vf.Value = val;
                }
            }
        }

        public void RebindColumn(Control namingContainer, string columnName)
        {
            DropDownList ddl = BasePage.FindControlRecursive(namingContainer, "dtls" + columnName) as DropDownList;
            //AjaxControlToolkit.ComboBox ddl = BasePage.FindControlRecursive(namingContainer, "dtls" + columnName) as AjaxControlToolkit.ComboBox;		
            if (ddl != null)
                ddl.DataBind();
        }

        public void RebindDependencies(Control namingContainer, string changedColumn)
        {
            List<string> rebindList = new List<string>();
            foreach (DropDownColumn ddc in Values)
            {
                foreach (DropDownColumn.ValueFilter vf in ddc.FilterList.Values)
                {
                    if (vf.TieField.ToLower() == changedColumn.ToLower())
                        rebindList.Add(ddc.Name);
                }
            }

            foreach (string col in rebindList)
                RebindColumn(namingContainer, col);
        }


    }

    public class DropDownColumn
    {
        public string Name { get; set; }
        public string TorV { get; set; }
        public bool ReadOnly { get; set; }
        public string ColumnFormat { get; set; }
        public Dictionary<string, ValueFilter> FilterList { get; set; }
        public Dictionary<string, string> SecondaryValueList { get; set; }


        public DropDownColumn()
        {
            ColumnFormat = "";
            FilterList = new Dictionary<string, ValueFilter>();
            SecondaryValueList = new Dictionary<string, string>();
        }
        private string DBName { get { return (HttpContext.Current.Session["DatabaseName"] != null) ? HttpContext.Current.Session["DatabaseName"].ToString() : "LMS2"; } }

        public static DataTable GetComboValues(string view, string filter)
        {
            DropDownColumn ddc = new DropDownColumn { TorV = view };
            return ddc.GetComboValues(true, filter);
        }

        public DataTable GetComboValues()
        {
            return GetComboValues(true);
        }

        public DataTable GetComboValues(bool filterValues)
        {
            string filter = "";
            if (filterValues)
            {
                foreach (string column in FilterList.Keys)
                {
                    ValueFilter vf = FilterList[column];
                    if (vf.ColumnName.Length > 0)
                    {
                        if (vf.Value.Length > 0)
                            filter += string.Format("{0}{1}='{2}'", (filter.Length > 0) ? " AND " : "", (vf.ValueField.Length > 0) ? vf.ValueField : vf.ColumnName, Regex.Replace(vf.Value, "'", "''"));
                    }
                    else
                        filter += string.Format("{0}{1}", (filter.Length > 0) ? " AND " : "", vf.Value);
                }
            }

            return GetComboValues(filterValues, filter);
        }

        public DataTable GetComboValues(bool filterValues, string filter)
        {
            if (filter.Length > 0)
                filter = string.Format("({0}) OR (DisplayText='') ORDER BY 1,2", filter);
            else
                filter = "ORDER BY 1,2";

            DataSet ds = SASWrapper.QueryData(DBName, TorV, "*", filter);

            // Scan and see if any secondary values need to be loaded because we are being referenced as a filter from another combo box
            DropDownColumns ddcList = HttpContext.Current.Session["DropDownColumns"] as DropDownColumns;
            foreach (DropDownColumn ddc in ddcList.Values)
            {
                foreach (DropDownColumn.ValueFilter vf in ddc.FilterList.Values)
                {
                    if (vf.ValueField.Length > 0 && vf.TieField.ToLower() == Name.ToLower())
                    {
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            DataColumn valCol = ds.Tables[0].Columns["Value"];
                            DataColumn pvCol = ds.Tables[0].Columns[vf.ValueField];
                            foreach (DataRow row in ds.Tables[0].Rows)
                                SecondaryValueList[row[valCol].ToString()] = row[pvCol].ToString();
                        }
                    }
                }
            }

            // Remove any extra blanks...
            DataTable dt = (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : new DataTable();
            bool blankFound = false;
            foreach (DataRow row in dt.Rows)
            {
                if (row["DisplayText"].ToString().Trim().Length == 0)
                {
                    if (blankFound)
                        row.Delete();
                    blankFound = true;
                }
            }
            dt.AcceptChanges();

            return dt;
        }

        public string GetValueFromText(string text)
        {
            DataSet ds = SASWrapper.QueryData(DBName, TorV, "*", string.Format("DisplayText='{0}'", Utilities.PrepSQLStringParam(text)));
            return (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) ? ds.Tables[0].Rows[0]["Value"].ToString() : "";
        }

        public class ValueFilter
        {
            public string ColumnName { get; set; }
            public string Value { get; set; }
            public string TieField { get; set; }
            public string ValueField { get; set; }
            public Dictionary<string, string> ValueCollection = new Dictionary<string, string>();
        }

        public void LoadSecondaryValues()
        {

        }


        //public class SecondaryValue
        //{
        //  public string ColumnName = "";
        //  public Dictionary<string, string> Values = new Dictionary<string, string>();
        //}
    }
}