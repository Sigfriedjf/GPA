using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
//using System.Web.UI.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using System.Net.Http;


namespace LMS2.components
{
    public class SASDetailDataSource : DataSourceControl
    {
        public SASDetailDataSourceView _dataView = null;

        public SASDetailDataSource()
            : base()
        {
        }

        protected override DataSourceView GetView(string viewName)
        {
            if (_dataView == null)
                _dataView = new SASDetailDataSourceView(this);

            return _dataView;
        }

        protected override ICollection GetViewNames()
        {
            return (new string[] { SASDetailDataSourceView.DefaultViewName } as ICollection);
        }
    }

    public class SASDetailDataSourceView : DataSourceView
    {
        public DataSet _ds = null;
        private SASDetailDataSource _owner = null;

        public static string DefaultViewName = "SASTableView";  // For now we will leave the view name hard coded since we only have a single view

        public SASDetailDataSourceView(IDataSource owner)
            : base(owner, DefaultViewName)
        {
            _owner = owner as SASDetailDataSource;
        }

        // Hard coded "Can" attributes
        public override bool CanPage { get { return false; } }
        public override bool CanSort { get { return false; } }
        public override bool CanRetrieveTotalRowCount { get { return true; } }
        public override bool CanDelete { get { return true; } }
        public override bool CanInsert { get { return true; } }
        public override bool CanUpdate { get { return true; } }

        public Dictionary<string, string> specialSelectColumns = new Dictionary<string, string>() {
          {"productcodeshort", "WA11F1P1A"},
            {"blendtankshort", "WA22F1P1B"},
            {"sourceshort", "WA22F1P1B"},
            {"destinationshort", "WA22F1P1B"},
            {"blendprocessnumber", "WA31F1P1"},
            {"loadprocessnumber", "WA31F6P1"},
            {"fillershort", "WA21F1P16A"},
            {"location", "LogLocationDetail"}
        };

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments selectArgs)
        {
            DataView dataView = new DataView();

            if (Session("QueryKeyColumn", "").Length > 0 && Session("SelectedItemKey", "").Length > 0)
            {
                string filter = string.Format("({0}='{1}')", Utilities.PrepSQLStringParam(Session("QueryKeyColumn", "")), Utilities.PrepSQLStringParam(Session("SelectedItemKey", "")));
                _ds = SASWrapper.QueryData(Session("DatabaseName", "LMS2"), "", Session("DetailsColumnRequested", "*"), filter);
                if (_ds != null && _ds.Tables.Count > 0)
                {
                    dataView = new DataView(_ds.Tables[0]);

                    // if we are loading data that should set scope for another page, then do it...
                    if (_ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataColumn dc in _ds.Tables[0].Columns)
                            if (specialSelectColumns.ContainsKey(dc.ColumnName.ToLower()))
                                HttpContext.Current.Session[specialSelectColumns[dc.ColumnName.ToLower()] + "SelectedItemKey"] = _ds.Tables[0].Rows[0][dc].ToString();
                    }
                }
            }

            // Should always be 1, but ya never know... ;-)
            if (selectArgs.RetrieveTotalRowCount)
                selectArgs.TotalRowCount = dataView.Count;

            return dataView as IEnumerable;
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary values)
        {
            string spName = Session(Session("CurPageName", "") + "StoredProcForDelete", "");
            string dbName = Session("DatabaseName", "LMS2");
            string primaryKey = Session("PrimaryKeyColumn", "");
            if (spName.Length > 0 && primaryKey.Length > 0 && values.Contains(primaryKey))
            {
                DataSet ds = new DataSet();
                DataTable tbl = ds.Tables.Add("Delete");
                tbl.Columns.Add(primaryKey);
                tbl.Columns.Add("UpdatedBy");
                tbl.Columns.Add("UpdatedDate", typeof(DateTime));

                DateTime ts = DateTime.Now;
                ts = new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second);

                tbl.Rows.Add(tbl.NewRow().ItemArray = new object[] { values[primaryKey], Session("UserID", "Unknown") + "-Deleted", ts });

                // Special case for product page. Deleted product should delete the image files on server.
                string ProductPath = string.Empty;
                if (Session("CurPageName", "") == "WA11F1P1")
                {
                    DataSet ds1 = SASWrapper.ExecuteQuery(dbName, "SELECT ProductCode FROM [dbo].[Product] WHERE IDProd = " + ds.Tables[0].Rows[0][0]);
                    if (ds1 != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        string code = ds1.Tables[0].Rows[0][0].ToString();
                        ProductPath = HttpContext.Current.Server.MapPath("ProductImages") + "\\" + code;
                    }
                }

                // Special case for package material page. Deleted product should delete the image files on server.
                string PkgMaterialPath = string.Empty;
                if (Session("CurPageName", "") == "WA11F1P7B")
                {
                    DataSet ds1 = SASWrapper.ExecuteQuery(dbName, "SELECT PkgMaterialCode FROM [dbo].[PackageMaterial] WHERE PkgMaterialID = " + ds.Tables[0].Rows[0][0]);
                    if (ds1 != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        string code = ds1.Tables[0].Rows[0][0].ToString();
                        PkgMaterialPath = HttpContext.Current.Server.MapPath("PackageMaterialImages") + "\\" + code;
                    }
                }

                int returnCode = string.IsNullOrEmpty(SASWrapper.UpdateData(spName, dbName, ds)) ? 0 : 1;
                if (ProductPath.Length > 0)
                {
                    DirectoryInfo theDir = new DirectoryInfo(ProductPath);
                    if (theDir.Exists)
                        Directory.Delete(ProductPath, true);
                }
                if (PkgMaterialPath.Length > 0)
                {
                    DirectoryInfo theDir = new DirectoryInfo(PkgMaterialPath);
                    if (theDir.Exists)
                        Directory.Delete(PkgMaterialPath, true);
                }
                return returnCode;
            }
            else
                AppError.LogError("ExecuteDelete()", "Configuration Error: Missing delete sp name or primary key to perform deletion.");

            return 0;
        }

        protected override int ExecuteInsert(IDictionary values)
        {
            //string logDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\IPS\\LMS\\";
            //if (!Directory.Exists(logDir))
            //    Directory.CreateDirectory(logDir);
            //using (StreamWriter sw = File.CreateText(logDir + "LastInsert.txt"))
            //{
            //    foreach (string key in values.Keys)
            //        sw.WriteLine("{0}   '{1}'", key, (values[key] != null) ? values[key].ToString() : "NULL");
            //    sw.Flush();
            //    sw.Close();
            //}

            return UpdateOrInsert(values);
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            //string logDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\IPS\\LMS\\";
            //if (!Directory.Exists(logDir))
            //    Directory.CreateDirectory(logDir);
            //using (StreamWriter sw = File.CreateText(logDir + "LastUpdate.txt"))
            //{
            //    foreach (string key in values.Keys)
            //        sw.WriteLine("{0}   '{1}'", key, (values[key] != null) ? values[key].ToString() : "NULL");
            //    sw.Flush();
            //    sw.Close();
            //}

            return UpdateOrInsert(values);
        }

        protected int UpdateOrInsert(IDictionary values)
        {
            //string displayView = Session("DisplayView", "");
            string spName = Session(Session("CurPageName", "") + "StoredProcForUpdate", "");
            string dbName = Session("DatabaseName", "LMS2");

            // Get an data set with an empty table that has the right column types... ;-)
            DataSet ds = SASWrapper.QueryData(dbName, "", "*", "0=1");
            DataRow row = ds.Tables[0].NewRow();

            if (ds != null && ds.Tables.Count > 0)
            {
                // If UpdateDate one of the columns, set to Now rounded to the nearest second.
                DataColumn col = ds.Tables[0].Columns["UpdatedDate"];
                if (col != null)
                {
                    values.Remove("UpdatedDate");
                    DateTime ts = DateTime.Now;
                    row[col] = new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second);
                }

                col = ds.Tables[0].Columns["UpdatedBy"];
                if (col != null)
                {
                    values.Remove("UpdatedBy");
                    row[col] = Session("UserID", "Unknown");
                }
            }

            foreach (string key in values.Keys)
            {
                object o = (values[key] != null) ? values[key] : DBNull.Value;
                if (values[key] != null && o.GetType() == typeof(string))
                    o = HttpUtility.HtmlDecode(o.ToString().Trim());
                row[key] = o;
            }

            ds.Tables[0].Rows.Add(row);

            if (Session("CurPageName", "") != "WA72F1P1" && Session("CurPageName", "") != "WA72F1P3")
                return string.IsNullOrEmpty(SASWrapper.UpdateData(spName, dbName, ds)) ? 0 : 1;
            else
            {
                string colNames = "", colValues = "";
                foreach (DataColumn col in ds.Tables[0].Columns)
                    colNames += (((colNames.Length > 0) ? "|" : "") + col.ColumnName);
                foreach (object o in ds.Tables[0].Rows[0].ItemArray)
                    colValues += (((colValues.Length > 0) ? "|" : "") + o.ToString());

                return string.IsNullOrEmpty(SASWrapper.UpdateData(spName, dbName, colNames, colValues)) ? 0 : 1;
            }
        }

        public void RaiseChangedEvent()
        {
            OnDataSourceViewChanged(EventArgs.Empty);
        }

        private string Session(string id, string def)
        {
            return (HttpContext.Current.Session[id] != null) ? HttpContext.Current.Session[id].ToString() : def;
        }
    }
}

