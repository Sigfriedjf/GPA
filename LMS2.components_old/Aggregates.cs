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

/// <summary>
/// Summary description for Aggregates
/// </summary>

namespace LMS2.components
{
    public class Aggregates
    {
        public enum AggTypeEnum { Sum }
        public Dictionary<string, Aggregate> aggList = new Dictionary<string, Aggregate>();
        public bool hasAggregateColumns = false;

        public Aggregates(string aggDef)
        {
            string[] aggs = aggDef.Split(';');
            foreach (string agg in aggs)
            {
                if (agg.Trim().Length > 0)
                {
                    string[] aggParts = agg.Split(',');
                    aggList[aggParts[0].Trim().ToLower()] = new Aggregate() { ColumnName = aggParts[0].Trim(), AggregateType = (AggTypeEnum)Enum.Parse(typeof(AggTypeEnum), aggParts[1].Trim(), true) };
                    hasAggregateColumns = true;
                }
            }
        }

        public void AddValue(string colName, double val)
        {
            if (aggList.ContainsKey(colName.ToLower()))
                aggList[colName.ToLower()].AddValue(val);
        }

        public void Reset()
        {
            foreach (Aggregate a in aggList.Values)
                a.Reset();
        }

        public string[] ColumnList
        {
            get
            {
                List<string> cols = new List<string>();
                foreach (Aggregate agg in aggList.Values)
                    cols.Add(agg.ColumnName);
                return cols.ToArray();
            }
        }

        public string GetFinalAnswer(string colName)
        {
            Dictionary<string, string> colFormats = HttpContext.Current.Session["ColumnFormats"] as Dictionary<string, string>;
            string formatStr = (colFormats.ContainsKey(colName.ToLower())) ? colFormats[colName.ToLower()] : "";
            return (aggList.ContainsKey(colName.ToLower()) && !double.IsNaN(aggList[colName.ToLower()].FinalAnswer)) ? aggList[colName.ToLower()].FinalAnswer.ToString(formatStr) : "";
        }

        public class Aggregate
        {
            public AggTypeEnum AggregateType { get; set; }
            public string ColumnName { get; set; }

            private double _accum = 0.0;

            public void AddValue(double val)
            {
                switch (AggregateType)
                {
                    case AggTypeEnum.Sum: _accum += val; break;
                }
            }

            public void Reset()
            {
                _accum = 0;
            }

            public double FinalAnswer
            {
                get
                {
                    FinalizeAnswer();
                    return _accum;
                }
            }

            private void FinalizeAnswer()
            {
                switch (AggregateType)
                {
                    case AggTypeEnum.Sum: /* no finalization */ break;
                }
            }
        }
    }
}