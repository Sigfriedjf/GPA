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
    public class PCSSecurity
    {
        public static int Permission(string UserID, string PageID)
        {
            int pagePermission = (_Session(_Session("CurPageName") + "Permission").Length > 0) ? int.Parse(_Session(_Session("CurPageName") + "Permission")) : 0;
            if (PCSSecurity.PageHasOrderStateLogic)
            {
                string state = CurrentOrderState;
                if (state.Length > 0)
                    pagePermission = OrderStateLogicPermission(state, pagePermission);
            }
            return pagePermission;
        }

        public static bool PageHasOrderStateLogic
        {
            get
            {
                // We don't do Order State Logic for archive databases
                if (_Session("DatabaseName").ToLower() != _Session("PrimaryDatabase").ToLower())
                    return false;

                if (HttpContext.Current.Session["OrderStateLogic"] == null)
                    LoadOrderStateLogic();

                Dictionary<string, Dictionary<string, OrderStateLogicEntry>> osl = HttpContext.Current.Session["OrderStateLogic"] as Dictionary<string, Dictionary<string, OrderStateLogicEntry>>;
                return osl.ContainsKey(_Session("CurPageName"));
            }
        }

        public static int OrderStateLogicPermission(string orderState, int pagePermission)
        {
            if (HttpContext.Current.Session["OrderStateLogic"] == null)
                LoadOrderStateLogic();

            int oslPerm = -1;
            Dictionary<string, Dictionary<string, OrderStateLogicEntry>> osl = HttpContext.Current.Session["OrderStateLogic"] as Dictionary<string, Dictionary<string, OrderStateLogicEntry>>;
            if (osl.ContainsKey(_Session("CurPageName")))
            {
                Dictionary<string, OrderStateLogicEntry> oslColl = osl[_Session("CurPageName")] as Dictionary<string, OrderStateLogicEntry>;
                string[] roleTypes = _Session("RoleTypes").Split(new char[] { ',' });
                foreach (string roleType in roleTypes)
                {
                    if (oslColl.ContainsKey(OrderStateLogicEntry.GetKey(roleType.Trim(), orderState)))
                    {
                        OrderStateLogicEntry entry = oslColl[OrderStateLogicEntry.GetKey(roleType.Trim(), orderState)];
                        if (oslPerm < entry.Permission)
                            oslPerm = entry.Permission;
                    }
                }
            }

            return (oslPerm >= 0 && oslPerm < pagePermission) ? oslPerm : pagePermission;
        }

        public static Dictionary<string, int> PermittedStates()
        {
            return PermittedStates(CurrentOrderState);
        }

        public static Dictionary<string, int> PermittedStates(string orderState)
        {
            Dictionary<string, int> permStates = new Dictionary<string, int>();
            permStates[orderState] = 1;  // add current state

            if (HttpContext.Current.Session["OrderStateLogic"] == null)
                LoadOrderStateLogic();

            Dictionary<string, Dictionary<string, OrderStateLogicEntry>> osl = HttpContext.Current.Session["OrderStateLogic"] as Dictionary<string, Dictionary<string, OrderStateLogicEntry>>;
            if (osl.ContainsKey(_Session("CurPageName")))
            {
                Dictionary<string, OrderStateLogicEntry> oslColl = osl[_Session("CurPageName")] as Dictionary<string, OrderStateLogicEntry>;
                string[] roleTypes = _Session("RoleTypes").Split(new char[] { ',' });
                foreach (string roleType in roleTypes)
                {
                    if (oslColl.ContainsKey(OrderStateLogicEntry.GetKey(roleType.Trim(), orderState)))
                    {
                        OrderStateLogicEntry entry = oslColl[OrderStateLogicEntry.GetKey(roleType.Trim(), orderState)];
                        foreach (string state in entry.PermittedStates.Keys)
                            permStates[state] = 1;
                    }
                }
            }

            return permStates;
        }

        public static string GetOrderStateOfSelectedItem()
        {
            return CurrentOrderState;
        }

        private static string CurrentOrderState
        {
            get
            {
                string state = "";


                if (PCSTranslation.ColumnDefinitions.ContainsKey("status"))
                {
                    string filter = string.Format("{0}='{1}'", BasePage._Session("QueryKeyColumn"), BasePage._Session("SelectedItemKey"));
                    DataSet ds = SASWrapper.QueryData(BasePage._Session("DatabaseName"), "", "Status", filter);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        state = ds.Tables[0].Rows[0][0].ToString();
                        DropDownColumn dd = new DropDownColumn() { TorV = "v_OrderStatus_lb" };
                        state = dd.GetValueFromText(state);
                    }
                }
                else
                {
                    // Didn't find a Status field... see if one was passed from a parent...
                    if (PCSSecurity.PageHasOrderStateLogic && BasePage._Session("PassedParentState").Length > 0)
                        state = BasePage._Session("PassedParentState");
                }

                return state;
            }
        }

        private static void LoadOrderStateLogic()
        {
            LoadOrderFieldLogic(); // load this guy at the same time
            Dictionary<string, Dictionary<string, OrderStateLogicEntry>> osl = new Dictionary<string, Dictionary<string, OrderStateLogicEntry>>();

            DataSet ds = SASWrapper.QueryData(_Session("DatabaseName"), "AppsOrderStateLogic", "*", "");
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string pageID = row["PageID"].ToString();
                    string roleType = row["RoleType"].ToString();
                    int permission = (int)row["Permission"];

                    Dictionary<string, int> permittedStates = new Dictionary<string, int>();
                    string[] pStates = row["PermittedStates"].ToString().Split(',');
                    foreach (string state in pStates)
                        permittedStates[state] = 1;

                    string[] stateList = row["OrderState"].ToString().Split(',');
                    foreach (string state in stateList)
                    {
                        OrderStateLogicEntry osle = new OrderStateLogicEntry
                        {
                            PageID = pageID,
                            RoleType = roleType,
                            State = state.Trim(),
                            Permission = permission,
                            PermittedStates = permittedStates
                        };

                        if (!osl.ContainsKey(osle.PageID))
                            osl[osle.PageID] = new Dictionary<string, OrderStateLogicEntry>();
                        osl[osle.PageID][osle.Key] = osle;
                    }
                }
            }

            HttpContext.Current.Session["OrderStateLogic"] = osl;
        }

        private class OrderStateLogicEntry
        {
            public string PageID { get; set; }
            public string RoleType { get; set; }
            public string State { get; set; }
            public int Permission { get; set; }
            public Dictionary<string, int> PermittedStates { get; set; }
            public string Key { get { return GetKey(RoleType, State); } }
            public static string GetKey(string roleType, string state) { return string.Format("{0}:{1}", roleType.ToLower(), state.ToLower()); }
        }

        public static List<string> GetOrderFieldLogic(string pageID, string state)
        {
            Dictionary<string, List<string>> ofl = (Dictionary<string, List<string>>)HttpContext.Current.Session["OrderFieldLogic"];
            if (ofl.ContainsKey(GetOFLKey(pageID, state)))
                return ofl[GetOFLKey(pageID, state)];
            return new List<string>();
        }

        private static void LoadOrderFieldLogic()
        {
            Dictionary<string, List<string>> ofl = new Dictionary<string, List<string>>();

            DataSet ds = SASWrapper.QueryData(_Session("DatabaseName"), "AppsOrderFieldLogic", "*", "");
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string pageID = row["PageID"].ToString();
                    string[] states = row["Status"].ToString().Split(new char[] { ',' });
                    string columns = row["ROColumns"].ToString();

                    foreach (string state in states)
                    {
                        List<string> list = new List<string>();
                        foreach (string col in columns.Split(new char[] { ',' }))
                            list.Add(col.Trim().ToLower());

                        ofl[GetOFLKey(pageID, state)] = list;
                    }
                }
            }

            HttpContext.Current.Session["OrderFieldLogic"] = ofl;
        }

        private static string GetOFLKey(string pageID, string state)
        {
            return string.Format("{0}~~{1}", pageID, state);
        }

        private static string _Session(string id)
        {
            return (HttpContext.Current.Session[id] != null) ? HttpContext.Current.Session[id].ToString() : "";
        }
    }
}
