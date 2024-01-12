using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace LMS2.components
{
    public class PCSDetailsView : DetailsView
    {
        protected string pageID
        {
            get { return (ViewState["DetailsPageID"] != null) ? ViewState["DetailsPageID"].ToString() : ""; }
            set { ViewState["DetailsPageID"] = value; }
        }

        public PCSDetailsView()
        {
        }

        protected string Session(string id)
        {
            return Session(id, "");
        }

        protected string Session(string id, string def)
        {
            object o = HttpContext.Current.Session[id];
            return (o != null) ? o.ToString() : def;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SetupDataFields();
        }

        private Dictionary<string, int> _readOnlyColumnList = null;
        protected virtual Dictionary<string, int> GetReadOnlyColumns()
        {
            if (_readOnlyColumnList == null)
            {
                Dictionary<string, int> roList = (Page.Session["ReadOnlyColumns"] != null) ? (Dictionary<string, int>)Page.Session["ReadOnlyColumns"] : new Dictionary<string, int>();
                Dictionary<string, DataColumn> cols = PCSTranslation.ColumnDefinitions;

                if (PCSSecurity.PageHasOrderStateLogic)
                {
                    roList = new Dictionary<string, int>(((Dictionary<string, int>)Page.Session["ReadOnlyColumns"]));  // clone it so we only modify a copy
                    string state = PCSSecurity.GetOrderStateOfSelectedItem();

                    List<string> roColList = PCSSecurity.GetOrderFieldLogic(((BasePage)Page).PageID, state);
                    if (roColList.Count == 1 && roColList[0] == "*")
                    {
                        // Lock down everything except state
                        foreach (string s in cols.Keys)
                            if (s != "status")
                                roList[s] = 1;
                    }
                    else
                    {
                        foreach (string s in roColList)
                            if (cols.ContainsKey(s))
                                roList[s] = 1;
                    }
                }
                else
                    roList = (Dictionary<string, int>)Page.Session["ReadOnlyColumns"];

                _readOnlyColumnList = roList;
            }

            return _readOnlyColumnList;
        }

        protected void AddDataField(string fieldName, Type colType, string headerText)
        {
            Dictionary<string, int> readOnlyColumns = GetReadOnlyColumns();
            DropDownColumns dropDownColumns = (DropDownColumns)Page.Session["DropDownColumns"];
            Dictionary<string, int> insertExcludedColumns = (Page.Session["InsertExcludedColumns"] != null) ? (Dictionary<string, int>)Page.Session["InsertExcludedColumns"] : new Dictionary<string, int>();
            Dictionary<string, ColumnStyle> colStyles = Page.Session["ColumnStyles"] as Dictionary<string, ColumnStyle>;
            bool readOnly = readOnlyColumns.ContainsKey(fieldName.ToLower());
            bool insertVisible = !insertExcludedColumns.ContainsKey(fieldName.ToLower());

            // Special handling for combo box fields.
            if (dropDownColumns.ContainsKey(fieldName.ToLower()))
            {
                dropDownColumns[fieldName.ToLower()].ReadOnly = readOnly;
                Fields.Add(new TemplateField()
                {
                    HeaderText = headerText,
                    InsertVisible = insertVisible,
                    ItemTemplate = new ROFieldTemplate() { ColumnDef = dropDownColumns[fieldName.ToLower()] },
                    EditItemTemplate = new DropDownFieldTemplate() { ColumnDef = dropDownColumns[fieldName.ToLower()] },
                    InsertItemTemplate = new DropDownFieldTemplate() { ColumnDef = dropDownColumns[fieldName.ToLower()] }
                });
            }
            else
            {
                FieldType ft = TypeToFieldType(colType, fieldName);
                switch (ft)
                {
                    case FieldType.DateTime:
                        {
                            string colFormat = (colStyles.ContainsKey(fieldName.ToLower())) ? colStyles[fieldName.ToLower()].format : "";
                            Fields.Add(new TemplateField()
                            {
                                HeaderText = headerText,
                                InsertVisible = insertVisible,
                                ItemTemplate = new ROFieldTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = true, ColumnFormat = colFormat } },
                                EditItemTemplate = new CalendarTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = readOnly, ColumnFormat = colFormat } },
                                InsertItemTemplate = new CalendarTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = false, ColumnFormat = colFormat } }
                            });
                        }
                        break;

                    case FieldType.TimeOnly:
                        {
                            string colFormat = (colStyles.ContainsKey(fieldName.ToLower())) ? colStyles[fieldName.ToLower()].format : "";
                            Fields.Add(new TemplateField()
                            {
                                HeaderText = headerText,
                                InsertVisible = insertVisible,
                                ItemTemplate = new TimeTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = true, ColumnFormat = colFormat } },
                                EditItemTemplate = new TimeTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = readOnly, ColumnFormat = colFormat } },
                                InsertItemTemplate = new TimeTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = false, ColumnFormat = colFormat } }
                            });
                        }
                        break;

                    case FieldType.MultiLine:
                        {
                            int iWidth = colStyles[fieldName.ToLower()].width;
                            int iHeight = colStyles[fieldName.ToLower()].height > 0 ? colStyles[fieldName.ToLower()].height : 100;
                            Fields.Add(new TemplateField()
                            {
                                HeaderText = headerText,
                                InsertVisible = insertVisible,
                                ItemTemplate = new MultiLineTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = true }, Width = iWidth, Height = iHeight },
                                EditItemTemplate = new MultiLineTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = readOnly }, Width = iWidth, Height = iHeight },
                                InsertItemTemplate = new MultiLineTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = false }, Width = iWidth, Height = iHeight }
                            });
                        }
                        break;

                    case FieldType.Numeric:
                    case FieldType.Int:
                        {
                            bool onlyIntegers = (ft == FieldType.Int);
                            string colFormat = (colStyles.ContainsKey(fieldName.ToLower())) ? colStyles[fieldName.ToLower()].format : "";
                            int iWidth = (colStyles.ContainsKey(fieldName.ToLower())) ? colStyles[fieldName.ToLower()].width : 150;
                            Fields.Add(new TemplateField()
                            {
                                HeaderText = headerText,
                                InsertVisible = insertVisible,
                                ItemTemplate = new NumericTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = true, ColumnFormat = colFormat }, OnlyIntegers = onlyIntegers, UnderlyingType = colType, Width = iWidth },
                                EditItemTemplate = new NumericTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = readOnly }, OnlyIntegers = onlyIntegers, UnderlyingType = colType, Width = iWidth },
                                InsertItemTemplate = new NumericTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = false }, OnlyIntegers = onlyIntegers, UnderlyingType = colType, Width = iWidth }
                            });
                        }
                        break;

                    default:
                        {
                            if (fieldName.ToLower() == "colorvalue")
                            {
                                string colFormat = (colStyles.ContainsKey(fieldName.ToLower())) ? string.Format("{{0:{0}}}", colStyles[fieldName.ToLower()].format) : "";
                                int iWidth = (colStyles.ContainsKey(fieldName.ToLower())) ? colStyles[fieldName.ToLower()].width : 100;
                                Fields.Add(new TemplateField()
                                {
                                    HeaderText = headerText,
                                    InsertVisible = insertVisible,
                                    ItemTemplate = new ColorTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = true }, Width = iWidth },
                                    EditItemTemplate = new ColorTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = readOnly }, Width = iWidth },
                                    InsertItemTemplate = new ColorTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = false }, Width = iWidth }
                                });
                            }
                            else
                            {
                                string colFormat = (colStyles.ContainsKey(fieldName.ToLower())) ? string.Format("{{0:{0}}}", colStyles[fieldName.ToLower()].format) : "";
                                int iWidth = (colStyles.ContainsKey(fieldName.ToLower())) ? colStyles[fieldName.ToLower()].width : 150;
                                bool bReadOnly = false;
                                // Special case for handling the Lessons Learned.  We want tagname to display as a Label when inserting.
                                if ((((BasePage)Page).CanInsert) && (fieldName.ToLower() == "tagname" || fieldName.ToLower() == "tl1name" || fieldName.ToLower() == "tl2name" || fieldName.ToLower() == "tl3name")) bReadOnly = true;
                                Fields.Add(new TemplateField()
                                {
                                    HeaderText = headerText,
                                    InsertVisible = insertVisible,
                                    ItemTemplate = new SingleLineTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = true }, Width = iWidth },
                                    EditItemTemplate = new SingleLineTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = readOnly }, Width = iWidth },
                                    InsertItemTemplate = new SingleLineTextTemplate() { ColumnDef = new DropDownColumn { Name = fieldName, ReadOnly = bReadOnly }, Width = iWidth }
                                });
                            }
                        }
                        break;
                }
            }
        }

        protected void SetupDataFields()
        {
            // Command field is always last so remove from the front until we have only 1 left...
            while (Fields.Count > 1)
                Fields.RemoveAt(1);

            // force a blanking of the RO Column List...
            _readOnlyColumnList = null;

            Dictionary<string, DataColumn> columns = PCSTranslation.ColumnDefinitions;
            Dictionary<string, string> columnTrans = PCSTranslation.ColumnTranslations;

            // Add visible columns first...
            foreach (string colName in VisibleFieldList.Keys)
                if (columns.ContainsKey(colName)) // keys from VisibleFieldList already lower cased
                    AddDataField(columns[colName].ColumnName, columns[colName].DataType, columnTrans[colName]);

            // Add all other columns that weren't in the visible fields list
            foreach (string colName in columns.Keys)
                if (!VisibleFieldList.ContainsKey(colName))
                    AddDataField(columns[colName].ColumnName, columns[colName].DataType, columnTrans[colName]);
        }

        public bool IsChangingMode { get { return _isChangingMode; } }
        private bool _isChangingMode = false;
        protected override void OnModeChanging(DetailsViewModeEventArgs e)
        {
            base.OnModeChanging(e);
            _isChangingMode = true;
        }

        protected override void OnModeChanged(EventArgs e)
        {
            base.OnModeChanged(e);
            _isChangingMode = false;
        }

        private Dictionary<string, int> _visibleFields = null;
        protected Dictionary<string, int> VisibleFieldList
        {
            get
            {
                if (_visibleFields == null)
                {
                    Dictionary<string, int> cols = new Dictionary<string, int>();
                    foreach (string col in Regex.Split(Session("DetailsColumnShown"), ","))
                        cols[col.Trim().ToLower()] = 1;

                    // If anything other than read-only mode, remove UpdateDate and UpdateBy from the visible fields
                    if (CurrentMode != DetailsViewMode.ReadOnly)
                    {
                        cols.Remove("updateddate");
                        cols.Remove("updatedby");
                    }

                    // Add any columns that were listed to be hidden to the removal list...
                    Dictionary<string, int> hideCols = Page.Session["HiddenColumns"] as Dictionary<string, int>;
                    foreach (string col in hideCols.Keys)
                        if (cols.ContainsKey(col))
                            cols.Remove(col);

                    _visibleFields = cols;
                }

                return _visibleFields;
            }
        }

        protected override void OnDataBound(EventArgs e)
        {
            base.OnDataBound(e);

            // Save values on which combo box filtering is being performed...
            if (!((BasePage)Page).IsHistoryPage)
            {
                DropDownColumns ddcList = Page.Session["DropDownColumns"] as DropDownColumns;
                ddcList.SaveFilterValues(DataSourceObject as SASDetailDataSource);
            }

            if (Rows.Count > 1)
            {
                Dictionary<string, int> cols = VisibleFieldList;

                // Hide any fields that need to be hidden...
                // Since now we have moved the hidden fields to the bottom we only need to find the first one and then we can 
                // hide the rest.
                bool hiddenFound = false;
                for (int i = 0; i < Fields.Count; i++)
                {
                    if (!hiddenFound)
                    {
                        string colName = "";
                        BoundField bf = null;
                        if ((bf = Fields[i] as BoundField) != null)
                            colName = bf.DataField;
                        else if ((Fields[i] as TemplateField) != null && ((Fields[i] as TemplateField).EditItemTemplate as DropDownFieldTemplate) != null)
                            colName = ((Fields[i] as TemplateField).EditItemTemplate as DropDownFieldTemplate).ColumnDef.Name;
                        else if ((Fields[i] as TemplateField) != null && ((Fields[i] as TemplateField).EditItemTemplate as CalendarTextTemplate) != null)
                            colName = ((Fields[i] as TemplateField).EditItemTemplate as CalendarTextTemplate).ColumnDef.Name;
                        else if ((Fields[i] as TemplateField) != null && ((Fields[i] as TemplateField).EditItemTemplate as TimeTextTemplate) != null)
                            colName = ((Fields[i] as TemplateField).EditItemTemplate as TimeTextTemplate).ColumnDef.Name;
                        else if ((Fields[i] as TemplateField) != null && ((Fields[i] as TemplateField).EditItemTemplate as MultiLineTextTemplate) != null)
                            colName = ((Fields[i] as TemplateField).EditItemTemplate as MultiLineTextTemplate).ColumnDef.Name;
                        else if ((Fields[i] as TemplateField) != null && ((Fields[i] as TemplateField).EditItemTemplate as NumericTextTemplate) != null)
                            colName = ((Fields[i] as TemplateField).EditItemTemplate as NumericTextTemplate).ColumnDef.Name;
                        else if ((Fields[i] as TemplateField) != null && ((Fields[i] as TemplateField).EditItemTemplate as SingleLineTextTemplate) != null)
                            colName = ((Fields[i] as TemplateField).EditItemTemplate as SingleLineTextTemplate).ColumnDef.Name;
                        else if ((Fields[i] as TemplateField) != null && ((Fields[i] as TemplateField).EditItemTemplate as ColorTextTemplate) != null)
                            colName = ((Fields[i] as TemplateField).EditItemTemplate as ColorTextTemplate).ColumnDef.Name;

                        if (colName.Length > 0 && !cols.ContainsKey(colName.ToLower()))
                        {
                            Rows[i].Visible = false;
                            hiddenFound = true;
                        }
                    }
                    else
                        Rows[i].Visible = false;
                }
            }
        }

        protected override void OnItemUpdating(DetailsViewUpdateEventArgs e)
        {
            base.OnItemUpdating(e);

            // We need to do our own processing of the NewValues array since we are using it a little differently in our data source.
            // We are including all values, whether changed or not, whether visibile or not (sometimes the primary key will not be visible
            // if the user didn't request to see it).

            for (int i = 0; i < Fields.Count; i++)
            {
                if ((Fields[i] as BoundField) != null)
                {
                    BoundField bf = Fields[i] as BoundField;
                    if (!e.NewValues.Contains(bf.DataField))
                        e.NewValues[bf.DataField] = e.OldValues[bf.DataField];
                }
                else if ((Fields[i] as TemplateField) != null)
                {
                    TemplateField tf = Fields[i] as TemplateField;

                    DropDownFieldTemplate df = tf.InsertItemTemplate as DropDownFieldTemplate;
                    if (df != null)
                    {
                        DropDownList ddl = FindControl("dtls" + df.ColumnDef.Name) as DropDownList;
                        //AjaxControlToolkit.ComboBox ddl = FindControl("dtls" + df.ColumnDef.Name) as AjaxControlToolkit.ComboBox;
                        if (ddl != null)
                            e.NewValues[df.ColumnDef.Name] = ddl.SelectedValue;
                    }

                    CalendarTextTemplate cal = tf.InsertItemTemplate as CalendarTextTemplate;
                    if (cal != null)
                    {
                        TextBox txt = FindControl("dtls" + cal.ColumnDef.Name) as TextBox;
                        if (txt != null)
                        {
                            if (txt.Text.Length > 0)
                                e.NewValues[cal.ColumnDef.Name] = DateTime.Parse(txt.Text);
                            else
                                e.NewValues[cal.ColumnDef.Name] = DBNull.Value;
                        }

                        Label lbl = FindControl("dtls" + cal.ColumnDef.Name) as Label;
                        if (lbl != null)
                        {
                            if (lbl.Text.Length > 0)
                                e.NewValues[cal.ColumnDef.Name] = DateTime.Parse(lbl.Text);
                            else
                                e.NewValues[cal.ColumnDef.Name] = DBNull.Value;
                        }
                    }

                    TimeTextTemplate ttt = tf.InsertItemTemplate as TimeTextTemplate;
                    if (ttt != null)
                    {
                        TextBox txt = FindControl("dtls" + ttt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                        {
                            if (txt.Text.Length > 0)
                                e.NewValues[ttt.ColumnDef.Name] = DateTime.Parse(txt.Text);
                            else
                                e.NewValues[ttt.ColumnDef.Name] = DBNull.Value;
                        }

                        Label lbl = FindControl("dtls" + ttt.ColumnDef.Name) as Label;
                        if (lbl != null)
                        {
                            if (lbl.Text.Length > 0)
                                e.NewValues[ttt.ColumnDef.Name] = DateTime.Parse(lbl.Text);
                            else
                                e.NewValues[ttt.ColumnDef.Name] = DBNull.Value;
                        }
                    }

                    MultiLineTextTemplate mtt = tf.InsertItemTemplate as MultiLineTextTemplate;
                    if (mtt != null)
                    {
                        TextBox txt = FindControl("dtls" + mtt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                            e.NewValues[mtt.ColumnDef.Name] = txt.Text;
                    }

                    NumericTextTemplate ntt = tf.InsertItemTemplate as NumericTextTemplate;
                    if (ntt != null)
                    {
                        TextBox txt = FindControl("dtls" + ntt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                            e.NewValues[ntt.ColumnDef.Name] = NumericTextTemplate.GetTypedValue(ntt.UnderlyingType, txt.Text);
                        else
                        {
                            Label lb = FindControl("dtls" + ntt.ColumnDef.Name) as Label;
                            if (lb != null)
                                e.NewValues[ntt.ColumnDef.Name] = NumericTextTemplate.GetTypedValue(ntt.UnderlyingType, lb.Text);
                        }
                    }

                    SingleLineTextTemplate stt = tf.InsertItemTemplate as SingleLineTextTemplate;
                    if (stt != null)
                    {
                        TextBox txt = FindControl("dtls" + stt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                        {
                            if (txt.Text.Length > 0)
                                e.NewValues[stt.ColumnDef.Name] = txt.Text;
                            else
                                e.NewValues[stt.ColumnDef.Name] = DBNull.Value;
                        }
                        else
                        {
                            Label lbl = FindControl("dtls" + stt.ColumnDef.Name) as Label;
                            if (lbl != null)
                            {
                                if (lbl.Text.Length > 0)
                                    e.NewValues[stt.ColumnDef.Name] = lbl.Text;
                                else
                                    e.NewValues[stt.ColumnDef.Name] = DBNull.Value;
                            }
                        }
                    }

                    ColorTextTemplate ctt = tf.InsertItemTemplate as ColorTextTemplate;
                    if (ctt != null)
                    {
                        TextBox txt = FindControl("dtls" + ctt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                        {
                            if (txt.Text.Length > 0)
                                e.NewValues[ctt.ColumnDef.Name] = txt.Text;
                            else
                                e.NewValues[ctt.ColumnDef.Name] = DBNull.Value;
                        }
                        else
                        {
                            Label lbl = FindControl("dtls" + ctt.ColumnDef.Name) as Label;
                            if (lbl != null)
                            {
                                if (lbl.Text.Length > 0)
                                    e.NewValues[ctt.ColumnDef.Name] = lbl.Text;
                                else
                                    e.NewValues[ctt.ColumnDef.Name] = DBNull.Value;
                            }
                        }
                    }
                }
            }

            // Check to see if we are creating from a template...
            if (Page.Request.QueryString["SelectColumn"] != null)
                Page.Session["SelectedItemKey"] = e.NewValues[Page.Request.QueryString["SelectColumn"]];

            HttpContext.Current.Session["RecalcSelection"] = true;
        }

        protected override void OnItemInserting(DetailsViewInsertEventArgs e)
        {
            base.OnItemInserting(e);

            // If we are on a list that is filtered for a particular scope
            bool scoped = ((BasePage)Page).IsPageFilterScoped;

            for (int i = 0; i < Fields.Count; i++)
            {
                if (scoped && (Fields[i] as BoundField != null))
                {
                    BoundField bf = Fields[i] as BoundField;
                    if (bf.DataField.ToLower() == Session("PageScopeColumn").ToLower())
                        e.Values[bf.DataField] = Session("PageScopeValue");
                }
                else if ((Fields[i] as TemplateField) != null)
                {
                    TemplateField tf = Fields[i] as TemplateField;

                    DropDownFieldTemplate df = tf.InsertItemTemplate as DropDownFieldTemplate;
                    if (df != null)
                    {
                        DropDownList ddl = FindControl("dtls" + df.ColumnDef.Name) as DropDownList;
                        //AjaxControlToolkit.ComboBox ddl = FindControl("dtls" + df.ColumnDef.Name) as AjaxControlToolkit.ComboBox;
                        if (ddl != null)
                        {
                            if (df.ColumnDef.Name.ToLower() == Session("PageScopeColumn").ToLower())
                                e.Values[df.ColumnDef.Name] = Session("PageScopeValue");
                            else
                                e.Values[df.ColumnDef.Name] = ddl.SelectedValue;
                        }
                    }

                    CalendarTextTemplate cal = tf.InsertItemTemplate as CalendarTextTemplate;
                    if (cal != null)
                    {
                        TextBox txt = FindControl("dtls" + cal.ColumnDef.Name) as TextBox;
                        if (txt != null)
                        {
                            if (txt.Text.Length > 0)
                            {
                                DateTime date;
                                if (DateTime.TryParse(txt.Text, out date))
                                    e.Values[cal.ColumnDef.Name] = DateTime.Parse(txt.Text);
                                else
                                {
                                    e.Cancel = true;
                                    AppError.LogError("PCSDetailsView:OnItemInserting", string.Format(PCSTranslation.GetMessage("ErrDateInvalid"), txt.Text));
                                }
                            }
                            else
                                e.Values[cal.ColumnDef.Name] = DBNull.Value;
                        }
                    }

                    TimeTextTemplate ttt = tf.InsertItemTemplate as TimeTextTemplate;
                    if (ttt != null)
                    {
                        TextBox txt = FindControl("dtls" + ttt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                        {
                            if (txt.Text.Length > 0)
                                e.Values[ttt.ColumnDef.Name] = DateTime.Parse(txt.Text);
                            else
                                e.Values[ttt.ColumnDef.Name] = DBNull.Value;
                        }
                    }

                    MultiLineTextTemplate mtt = tf.InsertItemTemplate as MultiLineTextTemplate;
                    if (mtt != null)
                    {
                        TextBox txt = FindControl("dtls" + mtt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                            e.Values[mtt.ColumnDef.Name] = txt.Text;
                    }

                    NumericTextTemplate ntt = tf.InsertItemTemplate as NumericTextTemplate;
                    if (ntt != null)
                    {
                        TextBox txt = FindControl("dtls" + ntt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                            e.Values[ntt.ColumnDef.Name] = NumericTextTemplate.GetTypedValue(ntt.UnderlyingType, txt.Text);
                        else
                        {
                            Label lb = FindControl("dtls" + ntt.ColumnDef.Name) as Label;
                            if (lb != null)
                                e.Values[ntt.ColumnDef.Name] = NumericTextTemplate.GetTypedValue(ntt.UnderlyingType, lb.Text);
                        }
                        if (Fields[i].ToString().ToLower() == Session("PageScopeColumn").ToLower())
                            e.Values[tf.ToString()] = Session("PageScopeValue");
                    }

                    SingleLineTextTemplate stt = tf.InsertItemTemplate as SingleLineTextTemplate;
                    if (stt != null)
                    {
                        TextBox txt = FindControl("dtls" + stt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                        {
                            if (txt.Text.Length > 0)
                                e.Values[stt.ColumnDef.Name] = txt.Text;
                            else
                                e.Values[stt.ColumnDef.Name] = DBNull.Value;
                        }
                        else
                        {
                            Label lbl = FindControl("dtls" + stt.ColumnDef.Name) as Label;
                            if (lbl != null)
                            {
                                if (lbl.Text.Length > 0)
                                    e.Values[stt.ColumnDef.Name] = lbl.Text;
                                else
                                    e.Values[stt.ColumnDef.Name] = DBNull.Value;
                            }
                        }
                        if (Fields[i].ToString().ToLower() == Session("PageScopeColumn").ToLower())
                            e.Values[tf.ToString()] = Session("PageScopeValue");
                    }

                    ColorTextTemplate ctt = tf.InsertItemTemplate as ColorTextTemplate;
                    if (ctt != null)
                    {
                        TextBox txt = FindControl("dtls" + ctt.ColumnDef.Name) as TextBox;
                        if (txt != null)
                        {
                            if (txt.Text.Length > 0)
                                e.Values[ctt.ColumnDef.Name] = txt.Text;
                            else
                                e.Values[ctt.ColumnDef.Name] = DBNull.Value;
                        }
                        else
                        {
                            Label lbl = FindControl("dtls" + ctt.ColumnDef.Name) as Label;
                            if (lbl != null)
                            {
                                if (lbl.Text.Length > 0)
                                    e.Values[ctt.ColumnDef.Name] = lbl.Text;
                                else
                                    e.Values[ctt.ColumnDef.Name] = DBNull.Value;
                            }
                        }
                        if (Fields[i].ToString().ToLower() == Session("PageScopeColumn").ToLower())
                            e.Values[tf.ToString()] = Session("PageScopeValue");
                    }
                }
            }

            string col = Session("QueryKeyColumn");
            if (col.Length > 0)
            {
                HttpContext.Current.Session["SelectedItemKey"] = e.Values[col];
                HttpContext.Current.Session[Session("CurPageName") + "SelectedItemKey"] = HttpContext.Current.Session["SelectedItemKey"];
            }
            HttpContext.Current.Session["RecalcSelection"] = true;
        }

        protected override void OnItemDeleting(DetailsViewDeleteEventArgs e)
        {
            base.OnItemDeleting(e);

            // Make sure we add in the primary key so that deletion works... if it was a "BoundField" then we're good, but if it is an
            // integer then we need to specifically add the value in
            string primaryKey = Session("PrimaryKeyColumn", "");
            for (int i = 0; i < Fields.Count; i++)
            {
                if ((Fields[i] as TemplateField) != null)
                {
                    TemplateField tf = Fields[i] as TemplateField;
                    NumericTextTemplate ntt = tf.ItemTemplate as NumericTextTemplate;
                    if (ntt != null && ntt.ColumnDef.Name.ToLower() == primaryKey.ToLower())
                    {
                        Label lb = FindControl("dtls" + ntt.ColumnDef.Name) as Label;
                        if (lb != null)
                            e.Values[ntt.ColumnDef.Name] = NumericTextTemplate.GetTypedValue(ntt.UnderlyingType, lb.Text);
                    }
                }
            }

        }

        protected override void OnItemDeleted(DetailsViewDeletedEventArgs e)
        {
            base.OnItemDeleted(e);

            Page.Session["SelectedItemKey"] = "";
            HttpContext.Current.Session["RecalcSelection"] = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Rows.Count > 0)
            {
                DetailsViewRow headerRow = this.Rows[0];

                // Look for the DELETE button
                DataControlFieldCell cell = (DataControlFieldCell)headerRow.Controls[0];

                bool cancelInd = false;
                for (int i = 0; i < cell.Controls.Count; i++)
                {
                    Control ctl = cell.Controls[i];

                    string FileName = System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath);

                    if ((System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath.ToUpper()) == "BULKEDIT") && (ctl != null) && (ctl.GetType() == typeof(ImageButton)))
                    {
                        ImageButton ib = (ImageButton)ctl;
                        if (ib.CommandName.ToUpper() == "UPDATE")
                            ib.OnClientClick = BasePage.UseCancelConfirmation ? "if(!confirm('" + string.Format(PCSTranslation.GetMessage("BulkEditMsg"), HttpContext.Current.Session["GridRecordCount"]) + "')) return false;" : null;
                    }

                    if ((ctl != null) && (ctl.ToString().Equals("System.Web.UI.WebControls.DataControlImageButton")))
                    {
                        ImageButton ib = (ImageButton)ctl;
                        ib.CssClass = "toolbarimage";

                        if (ib.CommandName.ToUpper() == "CANCEL")
                        {
                            if (DefaultMode == DetailsViewMode.Edit)
                            {
                                ib.ImageUrl = "~/Images/undo_sm.gif";
                                ib.ToolTip = "Undo";

                                Image ibPagePrint = new Image();
                                ibPagePrint.ImageUrl = "~/Images/print_sml.gif";
                                ibPagePrint.ToolTip = "Print this page";
                                ibPagePrint.CssClass = "toolbarHref";
                                ibPagePrint.Style.Add("cursor", "hand");
                                ibPagePrint.Attributes.Add("onClick", "window.print()");
                                cell.Controls.Add(ibPagePrint);

                                if (((BasePage)Page).HelpFilePath.Length > 0)
                                {
                                    ImageButton ibPageInfo = new ImageButton();
                                    ibPageInfo.ImageUrl = "~/Images/info_sml.gif";
                                    ibPageInfo.ToolTip = "Help on this page";
                                    ibPageInfo.CssClass = "toolbarHref";
                                    ((BasePage)Page).OpenPopUp(ibPageInfo, ((BasePage)Page).HelpFilePath, "_blank", 600, 600);
                                    cell.Controls.Add(ibPageInfo);
                                }
                            }
                            else
                            {
                                ib.OnClientClick = BasePage.UseCancelConfirmation ? "if(!confirm('" + PCSTranslation.GetMessage("CancelEdit") + "')) return false;" : null;
                                // Literalcontrols
                                LiteralControl lc = (LiteralControl)cell.Controls[i - 1];
                                lc.Text = "</td><td Width=\"50%\" align=\"right\">";
                                cancelInd = true;
                            }
                        }
                        else if (ib.CommandName.ToUpper() == "EDIT")
                        {
                            ib.Visible = (((BasePage)Page).CanEdit && !((BasePage)Page).IsHistoryPage);
                        }
                        else if (ib.CommandName.ToUpper() == "DELETE")
                        {
                            if (System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath.ToUpper()) == "BULKEDIT")
                                ib.OnClientClick = "if(!confirm('" + string.Format(PCSTranslation.GetMessage("BulkDeleteMsg"), HttpContext.Current.Session["GridRecordCount"]) + "')) return false;";
                            else
                                ib.OnClientClick = "if(!confirm('" + PCSTranslation.GetMessage("DeleteRecord") + "')) return false;";
                            ib.Visible = (((BasePage)Page).CanDelete && !((BasePage)Page).IsHistoryPage);
                        }
                        else if (ib.CommandName.ToUpper() == "NEW")
                        {
                            ib.Visible = (((BasePage)Page).CanInsert && !((BasePage)Page).IsHistoryPage);
                            ib.Style.Value = "margin-right:10px"; // For some reason the last button (new button) doesn't have spacing to the right like the others so I'm adding it here...
                            if (!((BasePage)Page).IsHistoryPage)
                            {
                                HyperLink hlConfigFilter = new HyperLink();
                                hlConfigFilter.ImageUrl = "~/Images/filter_config.gif";
                                hlConfigFilter.ToolTip = "Configure Detail";
                                hlConfigFilter.CssClass = "toolbarHref";
                                hlConfigFilter.NavigateUrl = "ColumnSettings.aspx?PageID=" + Page.Server.UrlEncode(BasePage._Session("CurPageName")) + "&DetailsColumn=1";
                                hlConfigFilter.Target = "_self";
                                cell.Controls.Add(hlConfigFilter);
                            }
                            // This is for the admin help pages
                            if ((Page as BasePage).PageID == "WA71F1P15" || (Page as BasePage).PageID == "WA71F1P16")
                            {
                                Button btnPopup = new Button();
                                btnPopup.Text = "View Help";
                                TranslateRequest tr = new TranslateRequest();
                                tr.Add("", "button", btnPopup.Text);
                                DataTable translationTable = tr.GetTable();
                                translationTable = SASWrapper.TranslateStrings(Session("DatabaseName"), PCSTranslation.UseAltLang, translationTable);
                                string text = translationTable.Rows[0][2].ToString().Trim();
                                string altText = translationTable.Rows[0][3].ToString().Trim();
                                if (altText.Length == 0)
                                    altText = text;
                                btnPopup.Text = altText;

                                btnPopup.CssClass = "toolbarlabel";
                                if ((Page as BasePage).PageID == "WA71F1P15")
                                    ((BasePage)Page).OpenPopUp(btnPopup, "HelpPage.aspx?FunctionID=" + Session("SelectedItemKey"), "_blank", 600, 600);
                                else
                                    ((BasePage)Page).OpenPopUp(btnPopup, "HelpPage.aspx?PageID=" + Session("SelectedItemKey"), "_blank", 600, 600);
                                cell.Controls.Add(btnPopup);
                            }
                        }
                    }
                }
                if (cancelInd)
                {
                    LiteralControl lit1 = new LiteralControl();
                    lit1.Text = "<table Width=\"100%\"><tr><td Width=\"50%\">";
                    cell.Controls.AddAt(0, lit1);
                    LiteralControl lit2 = new LiteralControl();
                    lit2.Text = "</td></tr></table>";
                    cell.Controls.AddAt(cell.Controls.Count, lit2);
                }
            }
        }

        protected enum FieldType { Byte, Int, Numeric, Text, DateTime, TimeOnly, MultiLine }
        protected FieldType TypeToFieldType(Type colType, string colName)
        {
            Dictionary<string, ColumnStyle> colStyles = Page.Session["ColumnStyles"] as Dictionary<string, ColumnStyle>;
            FieldType retType = FieldType.Text;

            if (colType == typeof(byte))
                retType = FieldType.Byte;

            else if (colType == typeof(double) || colType == typeof(decimal) || colType == typeof(float))
                retType = FieldType.Numeric;

            else if (colType == typeof(int) || colType == typeof(Int16) || colType == typeof(Int32) || colType == typeof(Int64))
                retType = FieldType.Int;

            else if (colType == typeof(DateTime))
            {
                if (colName.ToLower().Contains("time"))
                    retType = FieldType.TimeOnly;
                else
                    retType = FieldType.DateTime;
            }

            //else if (multiLine.ContainsKey(colName.ToLower()))
            else if (colStyles.ContainsKey(colName.ToLower()) && colStyles[colName.ToLower()].multiline)
                retType = FieldType.MultiLine;

            return retType;
        }
    }

}