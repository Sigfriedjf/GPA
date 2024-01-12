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
using System.Xml.XPath;
using System.IO;

namespace LMS2.components
{
    public class DetailLayout
    {
        protected List<LayoutColumn> _columns = new List<LayoutColumn>();
        protected List<LayoutRow> _rows = new List<LayoutRow>();
        protected BasePage _page;
        protected FormViewMode _defMode;
        protected string deleteMsg { get { return PCSTranslation.GetMessage("DeleteRecord"); } }
        protected string cancelMsg { get { return PCSTranslation.GetMessage("CancelEdit"); } }
        protected string bulkEditMsg { get { return PCSTranslation.GetMessage("BulkEdit"); } }

        public string cssClass { get; set; }
        public List<LayoutColumn> Columns { get { return _columns; } }
        public List<LayoutRow> Rows { get { return _rows; } }
        public Dictionary<string, DropDownColumn> DropDownColumns { get { return (Dictionary<string, DropDownColumn>)HttpContext.Current.Session["DropDownColumns"]; } }
        public Dictionary<string, int> ExcludeColumnsOnInsert { get { return (Dictionary<string, int>)HttpContext.Current.Session["InsertExcludedColumns"]; } }
        public Dictionary<string, int> ReadOnlyColumns { get { return (Dictionary<string, int>)HttpContext.Current.Session["ReadOnlyColumns"]; } }

        public DetailLayout(BasePage page, string layoutURI, FormViewMode defMode)
        {
            _page = page;
            _defMode = defMode;
            LoadLayout(layoutURI);
        }

        #region Layout / Xml Processing

        protected void LoadLayout(string layoutURI)
        {
            try
            {
                XPathDocument doc = new XPathDocument(AppDomain.CurrentDomain.BaseDirectory + layoutURI);
                XPathNavigator nav = doc.CreateNavigator();

                Dictionary<string, string> translations = PCSTranslation.ColumnTranslations;

                cssClass = nav.SelectSingleNode("//Table").GetAttribute("CSSClass", "");

                XPathNodeIterator iter = nav.Select("//Column");
                while (iter.MoveNext())
                    _columns.Add(new LayoutColumn(iter.Current));

                iter = nav.Select("//Row");
                while (iter.MoveNext())
                    _rows.Add(new LayoutRow(iter.Current, translations));
            }
            catch (Exception ex)
            {
                AppError.LogError("LoadLayout", ex);
            }
        }

        public class LayoutColumn
        {
            public Unit Width { get; set; }

            public LayoutColumn(XPathNavigator nav)
            {
                Width = Unit.Parse(nav.GetAttribute("size", ""));
            }
        }

        public class LayoutRow
        {
            private List<RowItem> _rowItems = new List<RowItem>();
            public List<RowItem> RowItems { get { return _rowItems; } }

            public LayoutRow(XPathNavigator nav, Dictionary<string, string> translations)
            {
                Dictionary<string, string> colFormats = HttpContext.Current.Session["ColumnFormats"] as Dictionary<string, string>;

                XPathNodeIterator iter = nav.Select("Item");
                while (iter.MoveNext())
                {
                    XPathNavigator x = iter.Current;
                    RowItem r = new RowItem();
                    r.RowType = x.GetAttribute("type", "").Length > 0 ? (RowItemType)Enum.Parse(typeof(RowItemType), x.GetAttribute("type", ""), true) : RowItemType.Spacer;
                    r.DBColumnName = x.GetAttribute("dbColumnName", "");
                    r.ColumnSpan = x.GetAttribute("columnSpan", "").Length > 0 ? int.Parse(x.GetAttribute("columnSpan", "")) : 1;
                    r.Alignment = x.GetAttribute("alignment", "").Length > 0 ? (HorizontalAlign)Enum.Parse(typeof(HorizontalAlign), x.GetAttribute("alignment", ""), true) : HorizontalAlign.Left;
                    r.Valignment = x.GetAttribute("valignment", "").Length > 0 ? (VerticalAlign)Enum.Parse(typeof(VerticalAlign), x.GetAttribute("valignment", ""), true) : VerticalAlign.Middle;
                    r.MultiLine = x.GetAttribute("multiLine", "").Length > 0 ? bool.Parse(x.GetAttribute("multiLine", "")) : false;
                    r.ReplaceText = x.GetAttribute("ReplaceText", "").Length > 0 ? x.GetAttribute("ReplaceText", "") : "";
                    if (x.GetAttribute("style", "").Length > 0)
                    {
                        r.Underline = x.GetAttribute("style", "").Contains("underline") ? true : false;
                        r.Bold = x.GetAttribute("style", "").Contains("bold") ? true : false;
                    }
                    if (r.ReplaceText.Length > 0)
                        r.DisplayText = r.ReplaceText;
                    else
                        r.DisplayText = (r.DBColumnName.Length > 0 && translations.ContainsKey(r.DBColumnName.ToLower())) ? translations[r.DBColumnName.ToLower()] : "";
                    r.Visible = x.GetAttribute("visible", "").Length > 0 ? bool.Parse(x.GetAttribute("visible", "")) : true;
                    //r.DataFormat = (colFormats.ContainsKey(r.DBColumnName.ToLower())) ? colFormats[r.DBColumnName.ToLower()] : "";
                    r.PixelHeight = x.GetAttribute("pixelHeight", "").Length > 0 ? int.Parse(x.GetAttribute("pixelHeight", "")) : 50;


                    _rowItems.Add(r);
                }
            }
        }

        public enum RowItemType { Label, Value, DateValue, Spacer, Numeric, Integer, Image, LabelValue }

        public class RowItem
        {
            public RowItemType RowType { get; set; }
            public string DBColumnName { get; set; }
            public string DisplayText { get; set; }
            public string ReplaceText { get; set; }
            public int ColumnSpan { get; set; }
            public HorizontalAlign Alignment { get; set; }
            public VerticalAlign Valignment { get; set; }
            public bool MultiLine { get; set; }
            public bool Visible { get; set; }
            public string DataFormat { get; set; }
            public int PixelHeight { get; set; }
            public bool Bold { get; set; }
            public bool Underline { get; set; }
        }
        #endregion

        #region Template Creation

        #region Item Template
        public ITemplate CreateItemTemplate() { return new ItemTemplate(this); }

        protected class ItemTemplate : ITemplate
        {
            private DetailLayout layout = null;

            public ItemTemplate(DetailLayout detailLayout)
            {
                layout = detailLayout;
            }

            public void InstantiateIn(Control container)
            {
                container.Controls.Add(CreateControlPanel());
                container.Controls.Add(CreateLayoutTable());
            }

            protected Panel CreateControlPanel()
            {
                Panel ctrlPanel = new Panel();
                ctrlPanel.CssClass = "FormViewControlPanel";
                Table ctrlPanelButtons = new Table() { BackColor = System.Drawing.Color.Transparent };

                if (layout._page.CanEdit)
                    ctrlPanel.Controls.Add(new ImageButton() { CommandName = "Edit", ImageUrl = "Images/edit_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle });
                if (layout._page.CanDelete)
                    ctrlPanel.Controls.Add(new ImageButton() { CommandName = "Delete", ImageUrl = "Images/delete_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle, OnClientClick = "if(!confirm('" + layout.deleteMsg + "')) return false;" });
                if (layout._page.CanInsert)
                    ctrlPanel.Controls.Add(new ImageButton() { CommandName = "New", ImageUrl = "Images/new_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle });
                ctrlPanel.Controls.Add(new ImageButton() { CommandName = "Print", ImageUrl = "Images/print_sml.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle, OnClientClick = "window.print()" });

                return ctrlPanel;
            }

            protected Table CreateLayoutTable()
            {
                Table tbl = new Table() { CssClass = layout.cssClass };

                // Add a basic first row with 0 height which sets the column sizes for all future rows
                TableRow row = new TableRow { Height = Unit.Pixel(0) };
                foreach (LayoutColumn col in layout.Columns)
                    row.Cells.Add(new TableCell { Width = col.Width });
                tbl.Rows.Add(row);

                foreach (LayoutRow lr in layout.Rows)
                {
                    row = new TableRow();

                    foreach (RowItem item in lr.RowItems)
                    {
                        TableCell cell = new TableCell { VerticalAlign = item.Valignment, HorizontalAlign = item.Alignment, ColumnSpan = item.ColumnSpan, Height = Unit.Pixel(10) };
                        if (item.ReplaceText.Length == 0)
                        {
                            switch (item.RowType)
                            {
                                case RowItemType.LabelValue:
                                    cell.Controls.Add(new LayoutLabelValue(item));
                                    break;
                                case RowItemType.Label:
                                    cell.Controls.Add(new LayoutLabel(item));
                                    break;

                                case RowItemType.Value:
                                case RowItemType.DateValue:
                                case RowItemType.Integer:
                                case RowItemType.Numeric:
                                    cell.Controls.Add(new LayoutTextBox(item) { ReadOnly = true });
                                    break;

                                case RowItemType.Image:
                                    cell.Controls.Add(new LayoutImage(item));
                                    break;

                                case RowItemType.Spacer: // do nothing in this case
                                    break;
                            }

                            row.Cells.Add(cell);
                        }
                    }

                    tbl.Rows.Add(row);
                }

                return tbl;
            }
        }
        #endregion

        #region Edit Template
        public ITemplate CreateEditTemplate() { return new EditTemplate(this); }

        protected class EditTemplate : ITemplate
        {
            private DetailLayout layout = null;

            public EditTemplate(DetailLayout detailLayout)
            {
                layout = detailLayout;
            }

            public void InstantiateIn(Control container)
            {
                container.Controls.Add(CreateControlPanel());
                container.Controls.Add(CreateLayoutTable());
            }

            protected Panel CreateControlPanel()
            {
                Panel ctrlPanel = new Panel();
                ctrlPanel.CssClass = "FormViewControlPanel";
                Table ctrlPanelButtons = new Table() { BackColor = System.Drawing.Color.Transparent };
                ctrlPanelButtons.Width = Unit.Percentage(100);
                TableRow tr1 = new TableRow();
                TableCell tc1 = new TableCell();
                tc1.Controls.Add(new ImageButton() { CommandName = "Update", ImageUrl = "Images/update_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle });
                if (layout._page.DetailMode.Length > 0)
                {
                    tc1.Width = Unit.Percentage(100);
                    tc1.Controls.Add(new ImageButton() { CommandName = "Cancel", ImageUrl = "Images/undo_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle });
                    tr1.Controls.Add(tc1);
                }
                else
                {
                    tc1.Width = Unit.Percentage(50);
                    TableCell tc2 = new TableCell();
                    tc2.Width = Unit.Percentage(50);
                    tc2.HorizontalAlign = HorizontalAlign.Right;
                    tc2.Controls.Add(new ImageButton() { CommandName = "Cancel", ImageUrl = "Images/cancel_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle, OnClientClick = BasePage.UseCancelConfirmation ? "if(!confirm('" + layout.cancelMsg + "')) return false;" : null });
                    tr1.Controls.Add(tc1);
                    tr1.Controls.Add(tc2);
                }
                ctrlPanelButtons.Controls.Add(tr1);
                ctrlPanel.Controls.Add(ctrlPanelButtons);

                return ctrlPanel;
            }

            protected Table CreateLayoutTable()
            {
                Table tbl = new Table() { CssClass = layout.cssClass };

                // Add a basic first row with 0 height which sets the column sizes for all future rows
                TableRow row = new TableRow { Height = Unit.Pixel(0) };
                foreach (LayoutColumn col in layout.Columns)
                    row.Cells.Add(new TableCell { Width = col.Width });
                tbl.Rows.Add(row);

                foreach (LayoutRow lr in layout.Rows)
                {
                    row = new TableRow();

                    foreach (RowItem item in lr.RowItems)
                    {
                        TableCell cell = new TableCell { VerticalAlign = item.Valignment, HorizontalAlign = item.Alignment, ColumnSpan = item.ColumnSpan, Height = Unit.Pixel(20) };

                        if (item.ReplaceText.Length == 0)
                        {
                            switch (item.RowType)
                            {
                                case RowItemType.LabelValue:
                                    cell.Controls.Add(new LayoutLabelValue(item));
                                    break;

                                case RowItemType.Label:
                                    cell.Controls.Add(new LayoutLabel(item));
                                    break;

                                case RowItemType.Value:
                                    {
                                        bool readOnly = layout.ReadOnlyColumns.ContainsKey(item.DBColumnName.ToLower());
                                        if (layout.DropDownColumns.ContainsKey(item.DBColumnName.ToLower()))
                                            cell.Controls.Add(new LayoutDropDownList(item, layout.DropDownColumns[item.DBColumnName.ToLower()]) { Enabled = !readOnly });
                                        else
                                            cell.Controls.Add(new LayoutTextBox(item) { ReadOnly = readOnly, TabIndex = (readOnly) ? Convert.ToInt16(-1) : Convert.ToInt16(0) });
                                    }
                                    break;

                                case RowItemType.Numeric:
                                case RowItemType.Integer:
                                    {
                                        bool readOnly = layout.ReadOnlyColumns.ContainsKey(item.DBColumnName.ToLower());
                                        bool onlyIntegers = (item.RowType == RowItemType.Integer);
                                        cell.Controls.Add(new LayoutTextBox(item) { ID = "dtls" + item.DBColumnName, ReadOnly = readOnly, TabIndex = (readOnly) ? Convert.ToInt16(-1) : Convert.ToInt16(0) });
                                        cell.Controls.Add(new AjaxControlToolkit.FilteredTextBoxExtender()
                                        {
                                            ID = "ftbe" + item.DBColumnName,
                                            TargetControlID = "dtls" + item.DBColumnName,
                                            FilterType = AjaxControlToolkit.FilterTypes.Custom | AjaxControlToolkit.FilterTypes.Numbers,
                                            ValidChars = (onlyIntegers) ? "-" : "-."
                                        });
                                    }
                                    break;

                                case RowItemType.DateValue:
                                    {
                                        bool readOnly = layout.ReadOnlyColumns.ContainsKey(item.DBColumnName.ToLower());
                                        LayoutTextBox tb = new LayoutTextBox(item) { ReadOnly = readOnly, ID = item.DBColumnName + "text" };
                                        cell.Controls.Add(tb);

                                        if (!readOnly)
                                        {
                                            tb.Style.Add("float", "left");
                                            tb.Width = Unit.Percentage(80.0);

                                            ImageButton ib = new ImageButton { ID = item.DBColumnName + "button", ImageUrl = "~/Images/calendar.png" };
                                            ib.Style.Add("float", "left");
                                            cell.Controls.Add(ib);
                                            cell.Wrap = false;
                                            cell.Controls.Add(new AjaxControlToolkit.CalendarExtender { ID = item.DBColumnName + "calExt", PopupButtonID = ib.ID, TargetControlID = tb.ID });
                                        }
                                    }
                                    break;

                                case RowItemType.Image:
                                    cell.Controls.Add(new LayoutImage(item));
                                    break;

                                case RowItemType.Spacer: // do nothing in this case
                                    break;
                            }

                            row.Cells.Add(cell);
                        }
                    }
                    tbl.Rows.Add(row);
                }

                return tbl;
            }
        }
        #endregion

        #region Insert Template
        public ITemplate CreateInsertTemplate() { return new InsertTemplate(this); }

        protected class InsertTemplate : ITemplate
        {
            private DetailLayout layout = null;

            public InsertTemplate(DetailLayout detailLayout)
            {
                layout = detailLayout;
            }

            public void InstantiateIn(Control container)
            {
                container.Controls.Add(CreateControlPanel());
                container.Controls.Add(CreateLayoutTable());
            }

            protected Panel CreateControlPanel()
            {
                Panel ctrlPanel = new Panel();
                ctrlPanel.CssClass = "FormViewControlPanel";
                Table ctrlPanelButtons = new Table() { BackColor = System.Drawing.Color.Transparent };
                ctrlPanelButtons.Width = Unit.Percentage(100);
                TableRow tr1 = new TableRow();
                TableCell tc1 = new TableCell();
                tc1.Controls.Add(new ImageButton() { CommandName = "Insert", ImageUrl = "Images/update_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle });
                if (layout._page.DetailMode.Length > 0)
                {
                    tc1.Width = Unit.Percentage(100);
                    tc1.Controls.Add(new ImageButton() { CommandName = "Cancel", ImageUrl = "Images/undo_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle });
                    tr1.Controls.Add(tc1);
                }
                else
                {
                    tc1.Width = Unit.Percentage(50);
                    TableCell tc2 = new TableCell();
                    tc2.Width = Unit.Percentage(50);
                    tc2.HorizontalAlign = HorizontalAlign.Right;
                    tc2.Controls.Add(new ImageButton() { CommandName = "Cancel", ImageUrl = "Images/cancel_sm.gif", CssClass = "FormViewControlPanelImage", ImageAlign = ImageAlign.Middle, OnClientClick = BasePage.UseCancelConfirmation ? "if(!confirm('" + layout.cancelMsg + "')) return false;" : null });
                    tr1.Controls.Add(tc1);
                    tr1.Controls.Add(tc2);
                }
                ctrlPanelButtons.Controls.Add(tr1);
                ctrlPanel.Controls.Add(ctrlPanelButtons);

                return ctrlPanel;
            }

            protected Table CreateLayoutTable()
            {
                Table tbl = new Table() { CssClass = layout.cssClass };

                // Add a basic first row with 0 height which sets the column sizes for all future rows
                TableRow row = new TableRow { Height = Unit.Pixel(0) };
                foreach (LayoutColumn col in layout.Columns)
                    row.Cells.Add(new TableCell { Width = col.Width });
                tbl.Rows.Add(row);

                foreach (LayoutRow lr in layout.Rows)
                {
                    row = new TableRow();

                    foreach (RowItem item in lr.RowItems)
                    {
                        TableCell cell = new TableCell { VerticalAlign = item.Valignment, HorizontalAlign = item.Alignment, ColumnSpan = item.ColumnSpan, Height = Unit.Pixel(10) };

                        if (!layout.ExcludeColumnsOnInsert.ContainsKey(item.DBColumnName.ToLower()))
                        {
                            switch (item.RowType)
                            {
                                case RowItemType.LabelValue:
                                    cell.Controls.Add(new LayoutLabelValue(item));
                                    break;

                                case RowItemType.Label:
                                    cell.Controls.Add(new LayoutLabel(item));
                                    break;

                                case RowItemType.Value:
                                    {
                                        bool readOnly = layout.ReadOnlyColumns.ContainsKey(item.DBColumnName.ToLower());
                                        if (layout.DropDownColumns.ContainsKey(item.DBColumnName.ToLower()))
                                            //cell.Controls.Add(new LayoutDropDownList(item, layout.DropDownColumns[item.DBColumnName.ToLower()]) { Enabled = !readOnly });
                                            cell.Controls.Add(new LayoutDropDownList(item, layout.DropDownColumns[item.DBColumnName.ToLower()]));
                                        else
                                            //cell.Controls.Add(new LayoutTextBox(item) { ReadOnly = readOnly });
                                            cell.Controls.Add(new LayoutTextBox(item));
                                    }
                                    break;

                                case RowItemType.Numeric:
                                case RowItemType.Integer:
                                    {
                                        bool readOnly = layout.ReadOnlyColumns.ContainsKey(item.DBColumnName.ToLower());
                                        bool onlyIntegers = (item.RowType == RowItemType.Integer);
                                        cell.Controls.Add(new LayoutTextBox(item) { ID = "dtls" + item.DBColumnName, ReadOnly = readOnly });
                                        cell.Controls.Add(new AjaxControlToolkit.FilteredTextBoxExtender()
                                        {
                                            ID = "ftbe" + item.DBColumnName,
                                            TargetControlID = "dtls" + item.DBColumnName,
                                            FilterType = AjaxControlToolkit.FilterTypes.Custom | AjaxControlToolkit.FilterTypes.Numbers,
                                            ValidChars = (onlyIntegers) ? "-" : "-."
                                        });
                                    }
                                    break;

                                case RowItemType.DateValue:
                                    {
                                        bool readOnly = layout.ReadOnlyColumns.ContainsKey(item.DBColumnName.ToLower());
                                        LayoutTextBox tb = new LayoutTextBox(item) { ReadOnly = readOnly, ID = item.DBColumnName + "text" };
                                        cell.Controls.Add(tb);

                                        if (!readOnly)
                                        {
                                            tb.Style.Add("float", "left");
                                            tb.Width = Unit.Percentage(80.0);

                                            ImageButton ib = new ImageButton { ID = item.DBColumnName + "button", ImageUrl = "~/Images/calendar.png" };
                                            ib.Style.Add("float", "left");
                                            cell.Controls.Add(ib);

                                            cell.Controls.Add(new AjaxControlToolkit.CalendarExtender { ID = item.DBColumnName + "calExt", PopupButtonID = ib.ID, TargetControlID = tb.ID });
                                        }
                                    }
                                    break;

                                case RowItemType.Image:
                                    cell.Controls.Add(new LayoutImage(item));
                                    break;

                                case RowItemType.Spacer: // do nothing in this case
                                    break;
                            }


                            row.Cells.Add(cell);
                        }
                    }

                    tbl.Rows.Add(row);
                }

                return tbl;
            }
        }
        #endregion

        #endregion

        #region Template Control Item SubClasses
        public class LayoutLabelValue : Label
        {
            public readonly RowItem _rowItem;
            public LayoutLabelValue(RowItem item)
            {
                _rowItem = item;
                Visible = item.Visible;
                if (_rowItem.Underline)
                    Style.Add("text-decoration", "underline");
                if (_rowItem.Bold)
                    Style.Add("font-weight", "bold");
            }

            protected override void OnDataBinding(EventArgs e)
            {
                base.OnDataBinding(e);

                try
                {
                    PCSFormView fv = NamingContainer as PCSFormView;

                    object val = null;
                    val = DataBinder.GetPropertyValue(fv.DataItem, _rowItem.DBColumnName);
                    Text = PCSTranslation.ColumnDefinitions.ContainsKey(val.ToString().ToLower()) ? PCSTranslation.ColumnTranslations[val.ToString().ToLower()] : val.ToString();
                }
                catch (Exception ex)
                {
                    AppError.LogError("LayoutLabelValue:Binding", ex);
                }
            }
        }

        public class LayoutLabel : Label
        {
            public readonly RowItem _rowItem;
            public LayoutLabel(RowItem item)
            {
                _rowItem = item;
                Text = item.DisplayText;
                Visible = item.Visible;
                if (_rowItem.Underline)
                    Style.Add("text-decoration", "underline");
                if (_rowItem.Bold)
                    Style.Add("font-weight", "bold");
            }
        }

        public class LayoutImage : Image
        {
            public readonly RowItem _rowItem;
            public LayoutImage(RowItem item)
            {
                _rowItem = item;
                ImageUrl = "~/Images/unknown.jpg";
            }

            protected override void OnDataBinding(EventArgs e)
            {
                base.OnDataBinding(e);

                try
                {
                    PCSFormView fv = NamingContainer as PCSFormView;
                    object val = DataBinder.GetPropertyValue(fv.DataItem, _rowItem.DBColumnName);
                    if (val != null)
                    {
                        string filename = HttpContext.Current.Server.MapPath(val.ToString());
                        if (File.Exists(filename))
                            ImageUrl = val.ToString();
                        else
                            ImageUrl = "~/Images/unknown.jpg";
                    }
                }
                catch { }
            }
        }

        public class LayoutTextBox : TextBox
        {
            public readonly RowItem _rowItem;
            public LayoutTextBox(RowItem item) { _rowItem = item; InitStyle(); }

            private void InitStyle()
            {
                Style.Add("text-align", _rowItem.Alignment.ToString().ToLower()); // only left, right, and center will work here
                Width = Unit.Percentage(99.0);
                Visible = _rowItem.Visible;

                if (_rowItem.MultiLine)
                {
                    TextMode = TextBoxMode.MultiLine;
                    Height = Unit.Pixel(_rowItem.PixelHeight);
                    Wrap = true;
                }
                //if (_rowItem.DBColumnName == "APIGravity")
                //    Attributes.Add("onChange", "return APIChanged(this)");
            }

            protected override void OnDataBinding(EventArgs e)
            {
                base.OnDataBinding(e);

                try
                {
                    PCSFormView fv = NamingContainer as PCSFormView;

                    object val = null;
                    if (fv.CurrentMode != FormViewMode.Insert)
                    {
                        val = DataBinder.GetPropertyValue(fv.DataItem, _rowItem.DBColumnName);
                    }
                    else
                    {
                        Dictionary<string, object> initVals = new Dictionary<string, object>();
                        if (Page.Session["FormInitialValues"] != null)
                            initVals = Page.Session["FormInitialValues"] as Dictionary<string, object>;
                        if (initVals.ContainsKey(_rowItem.DBColumnName.ToLower()))
                            val = initVals[_rowItem.DBColumnName.ToLower()];
                    }

                    if (val != null)
                    {
                        if (_rowItem.DataFormat.Length > 0)
                        {
                            if (val.GetType() == typeof(double)) Text = ((double)val).ToString(_rowItem.DataFormat);
                            else if (val.GetType() == typeof(float)) Text = ((float)val).ToString(_rowItem.DataFormat);
                            else if (val.GetType() == typeof(decimal)) Text = ((decimal)val).ToString(_rowItem.DataFormat);
                            else if (val.GetType() == typeof(int)) Text = ((int)val).ToString(_rowItem.DataFormat);
                            else if (val.GetType() == typeof(DateTime)) Text = ((DateTime)val).ToString(_rowItem.DataFormat);
                            else Text = val.ToString();
                        }
                        else if (val.GetType() == typeof(DateTime) && _rowItem.DBColumnName.ToLower() != "updateddate")
                            Text = ((DateTime)val).ToString(BasePage.DateFormat);
                        else if (val.GetType() == typeof(DateTime))
                            Text = ((DateTime)val).ToString(BasePage.DateTimeFormat);
                        else
                            Text = val.ToString();
                    }

                    if (fv.CurrentMode == FormViewMode.Insert)
                        ReadOnly = false; // read-only flag is always false when in Insert mode.
                                          // if a field isn't enterable it should be excluded from the screen.
                    if (ReadOnly)
                        BackColor = System.Drawing.ColorTranslator.FromHtml("#DEE8F5");
                }
                catch (Exception ex)
                {
                    AppError.LogError("LayoutTextBox:Binding", ex);
                }
            }
        };

        public class LayoutDropDownList : DropDownList
        {
            public readonly RowItem _rowItem;
            public readonly DropDownColumn ColumnDef;
            public LayoutDropDownList(RowItem rowItem, DropDownColumn ddColumn)
            {
                _rowItem = rowItem;
                ColumnDef = ddColumn;
                Style.Add("text-align", _rowItem.Alignment.ToString().ToLower()); // only left, right, and center will work here
                Width = Unit.Percentage(100.0);
                Visible = rowItem.Visible;
            }

            protected override void OnDataBinding(EventArgs e)
            {
                base.OnDataBinding(e);

                try
                {
                    PCSFormView fv = NamingContainer as PCSFormView;

                    DataTable dt = ColumnDef.GetComboValues();
                    if (PCSSecurity.PageHasOrderStateLogic && _rowItem.DBColumnName.ToLower() == "status")
                    {
                        Dictionary<string, int> permStates = PCSSecurity.PermittedStates();
                        foreach (DataRow r in dt.Rows)
                            if (permStates.ContainsKey(r["Value"].ToString()))
                                Items.Add(new ListItem(r["DisplayText"].ToString(), r["Value"].ToString()));
                    }
                    else
                    {
                        foreach (DataRow r in dt.Rows)
                            Items.Add(new ListItem(r["DisplayText"].ToString(), r["Value"].ToString()));
                    }

                    object val = null;
                    if (fv.CurrentMode == FormViewMode.Edit)
                    {
                        val = DataBinder.GetPropertyValue(fv.DataItem, _rowItem.DBColumnName);

                        ListItem li = Items.FindByText(val.ToString());
                        if (li == null)
                        {
                            string value = val.ToString();
                            // Try and resolve the selected item against the unfiltered item list (in case the guy we need to select is being filtered out)
                            dt = ColumnDef.GetComboValues(false);
                            DataRow[] rows = dt.Select(string.Format("DisplayText='{0}'", val.ToString()));
                            if (rows.Length > 0)
                                value = rows[0]["Value"].ToString();

                            // If the value doesn't match any of the list items, go ahead and except it anyway to keep it from getting blanked out
                            Items.Add(new ListItem(val.ToString(), value));
                            SelectedValue = value;
                        }
                    }
                    else if (fv.CurrentMode == FormViewMode.Insert)
                    {
                        Dictionary<string, object> initVals = new Dictionary<string, object>();
                        if (Page.Session["FormInitialValues"] != null)
                            initVals = Page.Session["FormInitialValues"] as Dictionary<string, object>;
                        if (initVals.ContainsKey(_rowItem.DBColumnName.ToLower()))
                            val = initVals[_rowItem.DBColumnName.ToLower()];
                    }

                    if (val != null)
                    {
                        ListItem li = Items.FindByText(val.ToString());
                        if (li != null)
                            SelectedValue = li.Value;
                    }

                }
                catch (Exception ex)
                {
                    AppError.LogError("LayoutDropDownList:Binding", ex);
                }
            }
        }

        #endregion
    }

}
