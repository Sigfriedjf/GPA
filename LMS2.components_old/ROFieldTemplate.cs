using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


namespace LMS2.components
{

    public class ROFieldTemplate : ITemplate
    {
        public DropDownColumn ColumnDef { get; set; }

        public void InstantiateIn(Control container)
        {
            Label lb = new Label();
            lb.ID = "dtls" + ColumnDef.Name;
            lb.DataBinding += new EventHandler(lb_DataBinding);
            container.Controls.Add(lb);
        }

        void lb_DataBinding(object sender, EventArgs e)
        {
            Label l = sender as Label;
            PCSDetailsView dv = l.NamingContainer as PCSDetailsView;
            object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);

            if (ColumnDef.ColumnFormat.Length > 0)
            {
                if (val.GetType() == typeof(double)) l.Text = ((double)val).ToString(ColumnDef.ColumnFormat);
                else if (val.GetType() == typeof(float)) l.Text = ((float)val).ToString(ColumnDef.ColumnFormat);
                else if (val.GetType() == typeof(decimal)) l.Text = ((decimal)val).ToString(ColumnDef.ColumnFormat);
                else if (val.GetType() == typeof(int)) l.Text = ((int)val).ToString(ColumnDef.ColumnFormat);
                else if (val.GetType() == typeof(DateTime)) l.Text = ((DateTime)val).ToString(ColumnDef.ColumnFormat);
                else l.Text = val.ToString();
            }
            else if (val.GetType() == typeof(DateTime) && ColumnDef.Name.ToLower() != "updateddate")
                l.Text = ((DateTime)val).ToString(BasePage.DateFormat);
            else if (val.GetType() == typeof(DateTime))
                l.Text = ((DateTime)val).ToString(BasePage.DateTimeFormat);
            else
                l.Text = val.ToString();
        }
    }

    public class DropDownFieldTemplate : ITemplate
    {
        public DropDownColumn ColumnDef { get; set; }
        public void InstantiateIn(Control container)
        {
            DropDownList ddl = new DropDownList();
            ddl.ID = "dtls" + ColumnDef.Name;
            ddl.DataBinding += new EventHandler(ddl_DataBinding);

            //AjaxControlToolkit.ComboBox ddl = new AjaxControlToolkit.ComboBox()
            //{
            //  ID = "dtls" + ColumnDef.Name,
            //  DropDownStyle = AjaxControlToolkit.ComboBoxStyle.DropDownList,
            //  AutoCompleteMode = AjaxControlToolkit.ComboBoxAutoCompleteMode.Append,
            //  CssClass = "WindowsStyle"
            //};
            //ddl.DataBinding += new EventHandler(ddl_DataBinding);


            // Add event for selection changed only if this is a tie value for another drop down...
            bool trackSelection = false;
            DropDownColumns ddcList = HttpContext.Current.Session["DropDownColumns"] as DropDownColumns;
            foreach (DropDownColumn ddc in ddcList.Values)
            {
                foreach (DropDownColumn.ValueFilter vf in ddc.FilterList.Values)
                {
                    if (vf.TieField.ToLower() == ColumnDef.Name.ToLower())
                    {
                        trackSelection = true;
                        break;
                    }
                }
                if (trackSelection)
                    break;
            }
            if (trackSelection)
            {
                ddl.SelectedIndexChanged += new EventHandler(ddl_SelectedIndexChanged);
                ddl.AutoPostBack = true;
            }

            if (ColumnDef.ReadOnly)
            {
                Label lb = new Label();
                lb.DataBinding += new EventHandler(lb_DataBinding);
                container.Controls.Add(lb);
                ddl.Visible = false;
            }
            container.Controls.Add(ddl);
        }

        void lb_DataBinding(object sender, EventArgs e)
        {
            Label l = sender as Label;
            PCSDetailsView dv = l.NamingContainer as PCSDetailsView;
            if (dv.DataItem != null)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);

                if (val.GetType() == typeof(DateTime) && ColumnDef.Name.ToLower() != "updateddate")
                    l.Text = ((DateTime)val).ToString(BasePage.DateFormat);
                else if (val.GetType() == typeof(DateTime))
                    l.Text = ((DateTime)val).ToString(BasePage.DateTimeFormat);
                else
                    l.Text = val.ToString();
            }

            if (dv.CurrentMode == DetailsViewMode.Insert && ColumnDef.ReadOnly)
                l.Visible = false;
        }

        void ddl_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = sender as DropDownList;
            //AjaxControlToolkit.ComboBox ddl = sender as AjaxControlToolkit.ComboBox;
            DropDownColumns ddcList = HttpContext.Current.Session["DropDownColumns"] as DropDownColumns;
            ddcList.SaveFilterValue(ColumnDef.Name, (ColumnDef.SecondaryValueList.Count > 0) ? ColumnDef.SecondaryValueList[ddl.SelectedValue] : ddl.SelectedValue);
            ddcList.RebindDependencies(ddl.NamingContainer, ColumnDef.Name);
        }

        void ddl_DataBinding(object sender, EventArgs e)
        {
            DropDownList ddl = sender as DropDownList;
            //AjaxControlToolkit.ComboBox ddl = sender as AjaxControlToolkit.ComboBox;

            // If ddl already has values, whack all of them except the current selection... this should only happen in the case
            // where we are updating our potential list based on another selection
            if (ddl.Items.Count > 0)
                for (int i = ddl.Items.Count - 1; i >= 0; i--)
                    if (i != ddl.SelectedIndex)
                        ddl.Items.RemoveAt(i);

            DataTable dt = ColumnDef.GetComboValues();
            if (PCSSecurity.PageHasOrderStateLogic && ColumnDef.Name.ToLower() == "status")
            {
                Dictionary<string, int> permStates = PCSSecurity.PermittedStates();
                foreach (DataRow r in dt.Rows)
                    if (permStates.ContainsKey(r["Value"].ToString()))
                        ddl.Items.Add(new ListItem(r["DisplayText"].ToString(), r["Value"].ToString()));
            }
            else
            {
                foreach (DataRow r in dt.Rows)
                {
                    ddl.Items.Add(new ListItem(r["DisplayText"].ToString(), r["Value"].ToString()));
                    //foreach (DropDownColumn.PropagateValue pgv in ColumnDef.PropagationList.Values)
                    //  pgv.Values[r["Value"].ToString()] = r[pgv.ColumnName].ToString();
                }
            }

            // Bind current value if in Edit mode.  If in insert mode, then there won't be one...
            PCSDetailsView dv = ddl.NamingContainer as PCSDetailsView;
            if (dv.CurrentMode == DetailsViewMode.Edit && dv.DataItem != null)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);
                if (val != null)
                {
                    ListItem li = ddl.Items.FindByText(val.ToString());
                    if (li != null)
                        ddl.SelectedValue = li.Value;
                    else
                    {
                        // We really shouldn't get here, but if we do, try and match on the value... we're probably here b/c of bad configuration
                        li = ddl.Items.FindByValue(val.ToString());
                        if (li != null)
                            ddl.SelectedValue = li.Value;
                        else
                        {
                            string value = val.ToString();
                            // Try and resolve the selected item against the unfiltered item list (in case the guy we need to select is being filtered out)
                            dt = ColumnDef.GetComboValues(false);
                            DataRow[] rows = dt.Select(string.Format("DisplayText='{0}'", val.ToString()));
                            if (rows.Length > 0)
                                value = rows[0]["Value"].ToString();

                            // If the value doesn't match any of the list items, go ahead and except it anyway to keep it from getting blanked out
                            ddl.Items.Add(new ListItem(val.ToString(), value));
                            ddl.SelectedValue = value;
                        }
                    }
                }
            }
            else if (dv.CurrentMode == DetailsViewMode.Insert && ColumnDef.ReadOnly)
                ddl.Visible = true;
        }
    }

    public abstract class TextTemplate
    {
        public DropDownColumn ColumnDef { get; set; }
    }

    public class MultiLineTextTemplate : TextTemplate, ITemplate
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public void InstantiateIn(Control container)
        {
            TextBox txt = new TextBox();
            txt.ID = "dtls" + ColumnDef.Name;
            txt.TextMode = TextBoxMode.MultiLine;
            //txt.Width = Unit.Pixel(800);
            txt.Width = Unit.Pixel(Width);

            if (ColumnDef.Name.ToLower() == "contents")
            {
                txt.Font.Name = "Courier New";
                txt.Height = Unit.Pixel(300);
                txt.Wrap = true;
            }
            else
                //txt.Height = Unit.Pixel(100);
                txt.Height = Unit.Pixel(Height);

            txt.ReadOnly = ColumnDef.ReadOnly;
            if (txt.ReadOnly)
                txt.BackColor = System.Drawing.ColorTranslator.FromHtml("#DEE8F5");
            txt.DataBinding += new EventHandler(mltxt_DataBinding);
            container.Controls.Add(txt);
        }

        void mltxt_DataBinding(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            PCSDetailsView dv = txt.NamingContainer as PCSDetailsView;
            if (dv.CurrentMode != DetailsViewMode.Insert)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);
                if (val != null)
                    txt.Text = val.ToString();
            }
        }
    }

    public class NumericTextTemplate : TextTemplate, ITemplate
    {
        public bool OnlyIntegers { get; set; }
        public Type UnderlyingType { get; set; }
        public int Width { get; set; }

        public void InstantiateIn(Control container)
        {
            if (ColumnDef.ReadOnly)
            {
                Label lb = new Label();
                lb.ID = "dtls" + ColumnDef.Name;
                lb.DataBinding += new EventHandler(lb_DataBinding);
                container.Controls.Add(lb);
            }
            else
            {
                TextBox txt = new TextBox();
                txt.ID = "dtls" + ColumnDef.Name;
                txt.ReadOnly = ColumnDef.ReadOnly;
                txt.Width = Unit.Pixel(Width);
                if (txt.ReadOnly)
                    txt.BackColor = System.Drawing.ColorTranslator.FromHtml("#DEE8F5");
                txt.DataBinding += new EventHandler(txt_DataBinding);
                container.Controls.Add(txt);

                AjaxControlToolkit.FilteredTextBoxExtender ftbe = new AjaxControlToolkit.FilteredTextBoxExtender()
                {
                    ID = "ftbe" + ColumnDef.Name,
                    TargetControlID = "dtls" + ColumnDef.Name,
                    FilterType = AjaxControlToolkit.FilterTypes.Custom | AjaxControlToolkit.FilterTypes.Numbers,
                    ValidChars = (OnlyIntegers) ? "-" : "-." // need to add locale specific separator eventually, but just '.' for now.
                };
                container.Controls.Add(ftbe);
            }
        }

        void txt_DataBinding(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            PCSDetailsView dv = txt.NamingContainer as PCSDetailsView;
            if (dv.CurrentMode != DetailsViewMode.Insert)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);
                if (val != null && val != DBNull.Value)
                    txt.Text = (ColumnDef.ColumnFormat.Length > 0) ? ((double)val).ToString(ColumnDef.ColumnFormat) : val.ToString();
            }
        }

        void lb_DataBinding(object sender, EventArgs e)
        {
            Label lb = sender as Label;
            PCSDetailsView dv = lb.NamingContainer as PCSDetailsView;
            if (dv.CurrentMode != DetailsViewMode.Insert)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);
                if (val != null && val != DBNull.Value)
                    lb.Text = (ColumnDef.ColumnFormat.Length > 0) ? ((double)val).ToString(ColumnDef.ColumnFormat) : val.ToString();
            }
        }

        public static object GetTypedValue(Type underlyingType, string val)
        {
            try
            {
                if (val.Trim().Length == 0) return DBNull.Value;

                if (underlyingType == typeof(double)) return double.Parse(val);
                if (underlyingType == typeof(float)) return float.Parse(val);
                if (underlyingType == typeof(decimal)) return decimal.Parse(val);
                if (underlyingType == typeof(int)) return int.Parse(val);
                if (underlyingType == typeof(Int16)) return Int16.Parse(val);
                if (underlyingType == typeof(Int32)) return Int32.Parse(val);
                if (underlyingType == typeof(Int64)) return Int64.Parse(val);
            }
            catch { }
            return val;
        }

    }

    public class CalendarTextTemplate : ITemplate
    {
        public DropDownColumn ColumnDef { get; set; }

        public void InstantiateIn(Control container)
        {
            if (ColumnDef.ReadOnly)
            {
                Label lbl = new Label();
                lbl.ID = "dtls" + ColumnDef.Name;
                lbl.DataBinding += new EventHandler(my_DataBinding);
                container.Controls.Add(lbl);
            }
            else
            {
                TextBox txt = new TextBox();
                txt.ID = "dtls" + ColumnDef.Name;
                txt.DataBinding += new EventHandler(my_DataBinding);
                container.Controls.Add(txt);

                ImageButton ib = new ImageButton();
                ib.ID = txt.ID + "button";
                ib.ImageUrl = "~/Images/calendar.png";
                ib.Enabled = !ColumnDef.ReadOnly;
                container.Controls.Add(ib);

                AjaxControlToolkit.CalendarExtender ce = new AjaxControlToolkit.CalendarExtender();
                ce.ID = txt.ID + "calExt";
                ce.TargetControlID = txt.ID;
                ce.PopupButtonID = ib.ID;
                container.Controls.Add(ce);
            }
        }

        void my_DataBinding(object sender, EventArgs e)
        {
            PCSDetailsView dv = (sender as Control).NamingContainer as PCSDetailsView;
            if (dv.CurrentMode != DetailsViewMode.Insert)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);
                if (val != null && val != DBNull.Value)
                {
                    DateTime dt = (DateTime)val;
                    string formatted = (ColumnDef.ColumnFormat.Length > 0) ? dt.ToString(ColumnDef.ColumnFormat) : dt.ToString(BasePage.DateFormat);
                    if (sender as Label != null)
                        (sender as Label).Text = formatted;
                    else
                        (sender as TextBox).Text = formatted;
                }
            }
        }
    }

    public class ColorTextTemplate : TextTemplate, ITemplate
    {
        public int Width { get; set; }
        public void InstantiateIn(Control container)
        {
            if (ColumnDef.ReadOnly)
            {
                Label lbl = new Label();
                lbl.ID = "dtls" + ColumnDef.Name;
                lbl.DataBinding += new EventHandler(my_DataBinding);
                container.Controls.Add(lbl);
            }
            else
            {
                TextBox txt = new TextBox();
                txt.ID = "dtls" + ColumnDef.Name;
                txt.DataBinding += new EventHandler(my_DataBinding);
                txt.Width = Unit.Pixel(100);
                container.Controls.Add(txt);

                ImageButton ib = new ImageButton();
                ib.ID = txt.ID + "button";
                ib.ImageUrl = "~/Images/cp_button.png";
                ib.Enabled = !ColumnDef.ReadOnly;
                ib.Style.Value = "margin-right:5px";
                container.Controls.Add(ib);

                TextBox txt1 = new TextBox();
                txt1.ID = txt.ID + "textbox";
                txt1.Width = Unit.Pixel(15);
                txt1.Enabled = false;

                AjaxControlToolkit.ColorPickerExtender cpe = new AjaxControlToolkit.ColorPickerExtender();
                cpe.ID = txt.ID + "colExt";
                cpe.TargetControlID = txt.ID;
                cpe.PopupButtonID = ib.ID;
                cpe.SampleControlID = txt1.ID;
                container.Controls.Add(cpe);

                container.Controls.Add(txt1);
            }
        }

        void my_DataBinding(object sender, EventArgs e)
        {
            PCSDetailsView dv = (sender as Control).NamingContainer as PCSDetailsView;
            if (dv.CurrentMode != DetailsViewMode.Insert)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);
                if (val != null && val != DBNull.Value)
                {
                    //string txt = (ColumnDef.ColumnFormat.Length > 0) ? ((DateTime)val).ToString(ColumnDef.ColumnFormat) : ((DateTime)val).ToString(BasePage.TimeFormat);
                    if (sender as Label != null)
                        (sender as Label).Text = HttpUtility.HtmlEncode(val.ToString());
                    else
                        (sender as TextBox).Text = val.ToString();
                }
            }
        }
    }

    public class TimeTextTemplate : ITemplate
    {
        public DropDownColumn ColumnDef { get; set; }

        public void InstantiateIn(Control container)
        {
            if (ColumnDef.ReadOnly)
            {
                Label lbl = new Label();
                lbl.ID = "dtls" + ColumnDef.Name;
                lbl.DataBinding += new EventHandler(my_DataBinding);
                container.Controls.Add(lbl);
            }
            else
            {
                TextBox txt = new TextBox();
                txt.ID = "dtls" + ColumnDef.Name;
                txt.DataBinding += new EventHandler(my_DataBinding);
                container.Controls.Add(txt);
            }
        }

        void my_DataBinding(object sender, EventArgs e)
        {
            PCSDetailsView dv = (sender as Control).NamingContainer as PCSDetailsView;
            if (dv.CurrentMode != DetailsViewMode.Insert)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);
                if (val != null && val != DBNull.Value)
                {
                    string txt = (ColumnDef.ColumnFormat.Length > 0) ? ((DateTime)val).ToString(ColumnDef.ColumnFormat) : ((DateTime)val).ToString(BasePage.TimeFormat);
                    if (sender as Label != null)
                        (sender as Label).Text = txt;
                    else
                        (sender as TextBox).Text = txt;
                }
            }
        }
    }

    public class SingleLineTextTemplate : TextTemplate, ITemplate
    {
        public int Width { get; set; }
        public void InstantiateIn(Control container)
        {
            if (ColumnDef.ReadOnly)
            {
                Label lbl = new Label();
                lbl.ID = "dtls" + ColumnDef.Name;
                lbl.DataBinding += new EventHandler(my_DataBinding);
                container.Controls.Add(lbl);
            }
            else
            {
                TextBox txt = new TextBox();
                txt.ID = "dtls" + ColumnDef.Name;
                txt.Width = Unit.Pixel(Width);
                txt.DataBinding += new EventHandler(my_DataBinding);
                container.Controls.Add(txt);
            }
        }

        void my_DataBinding(object sender, EventArgs e)
        {
            PCSDetailsView dv = (sender as Control).NamingContainer as PCSDetailsView;
            if (dv.CurrentMode != DetailsViewMode.Insert)
            {
                object val = DataBinder.GetPropertyValue(dv.DataItem, ColumnDef.Name);
                if (val != null && val != DBNull.Value)
                {
                    //string txt = (ColumnDef.ColumnFormat.Length > 0) ? ((DateTime)val).ToString(ColumnDef.ColumnFormat) : ((DateTime)val).ToString(BasePage.TimeFormat);
                    if (sender as Label != null)
                        (sender as Label).Text = HttpUtility.HtmlEncode(val.ToString());
                    else
                        (sender as TextBox).Text = val.ToString();
                }
            }
        }
    }

    public class ColumnStyle
    {
        public int height { get; set; }
        public int width { get; set; }
        public bool multiline { get; set; }
        public string format { get; set; }

        public ColumnStyle()
        {
            width = 150;
        }
    }
}