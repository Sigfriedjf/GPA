<%@ Page Language="C#" %>
<%@ Register Assembly="LMS2.components" namespace="LMS2.components" tagprefix="lms2" %>
A:link
{	
	text-decoration: none;
}	

A:visited
{	
	text-decoration:	none;
	color:	#333399;
}	
		
A:active
{	
	text-decoration:	none;
	color:	#333399;
}	
		
A:hover
{	
	text-decoration:	underline;
}

html
{
	height:99.9%;
	padding: 0px;
	margin: 0px;
	overflow: auto;
}

body
{
	font-weight: normal;
	font-size: 0.8em;
	word-spacing: normal;
	text-transform: none;
	font-family: Arial, Verdana, Helvetica, sans-serif;
	letter-spacing: normal;
	background-color: #ffffff;
	margin:0px;
	padding: 0px;
	height:99.9%;
	/*width:expression(document.documentElement.clientWidth <= 1258? "1258px": "auto" );*/
}

.Announcements
{
	height:180px;
	overflow: auto;
	border:Solid 1px #F37421;
}

.Announcements table th
{
	color: white;
	background-image: url( 'Images/header_gradient_orange.jpg' );
	background-repeat: repeat-x;
	font-family: Arial;
	font-size:medium;
	position:relative;
	white-space: nowrap;
}

.Announcements table tbody {overflow-x:hidden;} 

.clear
{
	clear:both;
}

#contentColumn
{
	/*padding:5px;*/
	width:auto;
	background-color: #ffffff;
	height:100%;
}

.errorRow
{
	width:100%;
	overflow: auto;
	font-weight: bold;
	color: Red;
}

.version
{
	padding-top:30px;
	font-weight: normal;
	color: #5A9A98;
}

.version td
{
	padding-right:80px;
}

.GridFilter
{
	background-color: <%= ColorManager.ColorBody %>;
	padding:5px;
	border:Solid 1px <%= ColorManager.ColorBorder %>;
}

.GridView
{
	font-family: Arial;
	font-weight: normal;
	color: black;
	border-style: solid;
	border-color: <%= ColorManager.ColorGridHeader %>;
	border-width: 1px;
	background-color: white;
}

.GridViewColor
{
	font-family: Arial;
	font-weight: normal;
	color: black;
	border-style: solid;
	border-color: <%= ColorManager.ColorGridHeader %>;
	border-width: 1px;
	background-color: white;
}

.GridViewColor td
{
	border-width: 2px;
}

.GridViewAlternateItem
{
	background-color: #DEE8F5;
}

.GridViewColorAlternateItem
{
	background-color: #FFFFFF;
}

.GridViewHeader
{
	font-weight: bold;
	background-color: <%= ColorManager.ColorGridHeader %>;
	text-align:left;
}

.GridViewHeader a:link
{
	color: White;
	text-decoration: none;
}

.GridViewHeader a:visited
{
	color: White;
	text-decoration: none;
}

.GridViewHeader a:active
{
	color: White;
	text-decoration: none;
}

.GridViewHeader a:hover
{
	color: White;
	text-decoration: underline;
}
		
.GridViewSelectedRow
{
	background-color: <%= ColorManager.ColorGridSelectedRow %>;
	color: Black;
}

.GridViewEditRow
{
	color: <%= ColorManager.ColorGridSelectedRow %>;
}

.GridViewColorSelectedRow
{
	border: 2px solid #000000;
	color: Black;
}

.GridViewFooter
{
	background-color: #FFFFCC;
	text-align: right;
}

.GridViewPager
{
	background-color: <%= ColorManager.ColorGridHeader %>;
	font-weight: bold;
	color: White;
	text-align: left;
}

.GridViewLabelInfo
{
	font-weight:bold;
	color: Black;
	font-family: Arial;
	height: 20px;
	background-color: transparent;
}

.navColumn
{
	float: left;
	padding-right: 8px; /* width:24%; */
	border-right: Solid 1px #397371;
	height: 100%;
	background-color: <%= ColorManager.ColorNavigation %>;
	overflow-y: auto;
	overflow-x: hidden;
}

.navColumn A:link
{	
	text-decoration: none;
}	

.navColumn A:visited
{	
	text-decoration:	none;
	color:	white;
}	
		
.navColumn A:active
{	
	text-decoration: none;
	/*color:	yellow;*/
}	
		
.navColumn A:hover
{	
	text-decoration:	underline;
}

#MastheadHR {
	BACKGROUND: #397371; OVERFLOW: hidden; HEIGHT: 1px
}

.PageName
{
	font-weight: bold;
	font-size: large;
	width: 384px;
	color: #0E2C69;
	font-family: Arial;
	height: 40px;
	background-color: transparent;
	text-align: center;
}

.PanelGridContents
{
	width: 100%;
	overflow-x: auto;
}

.ReportViewerHeader
{
	color: #808080;
	vertical-align: bottom;
	height: 30px;
	background-color: #ECE9D8;
}

.shadetabs
{
	border-bottom: 1px solid gray;
	width: 100%;
	margin-bottom: 1em;
}

.shadetabs ul
{
	padding: 3px 0;
	margin-left: 0;
	margin-top: 1px;
	margin-bottom: 0;
	font: bold 12px Verdana;
	list-style-type: none;
	text-align: left; /*set to left, center, or right to align the menu as desired*/
}

.shadetabs li
{
	display: inline;
	margin: 0;
	white-space:nowrap;
}

.shadetabs li a
{
	text-decoration: none;
	padding: 3px 7px;
	margin-right: 3px;
	border: 1px solid #778;
	color: #2d2b2b;
	background: white url(Images/shade.gif) top left repeat-x;
}

.shadetabs li a:visited
{
	color: #2d2b2b;
}

.shadetabs li a:hover
{
	text-decoration: underline;
	color: #2d2b2b;
}

.shadetabs li.selected
{
	position: relative;
	top: 1px;
}

.shadetabs li.selected a
{ /*selected main tab style */
	background-image: url(Images/shadeactive.gif);
	border-bottom-color: white;
}

.shadetabs li.selected a:hover
{ /*selected main tab style */
	text-decoration: none;
}

.TableFilter
{
	font-weight: normal;
	font-family: Arial;
	background-color: white;
}

.TableFilter td
{
	border:1px solid <%= ColorManager.ColorGridHeader %>;
	padding: 4px;
}

.TableFilter th
{
	padding: 4px;
}

.TableHeader
{
	font-weight: normal;
	color: white;
	font-family: Arial;
	/*background-color: #000080;*/
	border-style:none;
}

.TreeHeaderPanel
{
	float: left;
	width: 20px;
	height: 100%;
}

.TreeContentPanel
{
	float: left;
	/* width: 200px;*/
	height: 100%;
	overflow: hidden;
	margin-right: 5px;
}

th 
{
	color: white;
	background-color: <%= ColorManager.ColorGridHeader %>;
}

.treeNode
{
	color:White;
	font:Arial;
	font-size:small;
	font-weight:bold;
}

a.toolbarHref img
{
	margin-left:10px;
}

.toolbarimage
{
	margin-left:10px;
}

.toolbarlabel
{
	margin-left:15px;
}

.tabs
{
	position:relative;
	top:1px;
	left:10px;
}
.tab
{
	border:solid 1px #777788;
	background: white url(Images/shade.gif) top left repeat-x;
	padding:2px 3px;
	color:Black;
	font: bold 12px Verdana;
}
.selectedTab
{
	background-color: White;
	border-bottom:solid 1px white;
	background-image: url(Images/shadeactive.gif);
	color:Silver;
	font: bold 12px Verdana;
}
.labelcaption
{
	font-family: Arial;
	font-size: small;
	font-weight: bold;
	font-style: normal;
	color: #000080;
}

td.ReportNoWrapRightJustify, span.ReportNoWrapRightJustify
{
	white-space: nowrap;
	text-align: right;
}

.CollapsibleHeader
{
    min-height: 24px;
	color: #FFFFFF;
	font-weight: bold;
	background-color: <%= ColorManager.ColorTitleBar %>;
}

.CollapsibleContent
{
	height: 0;
	z-index: 1;
	overflow: hidden;
}

.UncollapsibleCollapsibleContent
{
	z-index: 1;
	overflow: hidden;
}

.SectionPanel
{
	border: thin solid #FFFFFF;
	padding: 5px;
	background-color: <%= ColorManager.ColorBackground %>;
}

.SummaryPanel
{
	float: left;
	margin-left: 4px;
	margin-top: 8px;
}

.SummaryPanelHist
{
	overflow-x: auto;
	margin-left: 4px;
	margin-top: 8px;
}

.GridToolPanel
{
	float: left;
}

.ListPanel
{
	overflow-x: auto;
	width: 99.9%;
	background-color: <%= ColorManager.ColorBody %>;
	border-color: <%= ColorManager.ColorBorder %>;
	border-style: solid;
	border-width: 1px;
}

.ThreadListPanel
{
	overflow-x: scroll;
	width: 99.9%;
	background-color: <%= ColorManager.ColorBody %>;
	border-color: <%= ColorManager.ColorBorder %>;
	border-style: solid;
	border-width: 1px;
}

.ListPanelHist
{
	width: 99.9%;
	background-color: <%= ColorManager.ColorBody %>;
	border-color: <%= ColorManager.ColorBorder %>;
	border-style: solid;
	border-width: 1px;
}

.DetailPanel
{
    float: left;
	min-height: 100%;
	z-index: 1;
	margin-top: 8px;
	margin-left: 4px;
	margin-right: 15px;
}

.DetailPanelHist
{
    overflow-x: auto;
	min-height: 100%;
	z-index: 1;
	margin-top: 8px;
	margin-left: 4px;
	margin-right: 15px;
}

.DetailFormPanel
{
	min-height: 100%;
	z-index: 1;
	margin-top: 8px;
	margin-left: 4px;
	margin-right: 15px;
}

.DivSummaryColumn
{
	float: left;
	height: 0;
	background-color: #EFEFEF;
	z-index: 1;
	margin-right: 2px;
}

.DivDetailColumn
{
	left:inherit;
}

.DurationSpan
{
	padding: 5px;
	border-width: 0px;
}

.TagGridFilter
{
	border-color: <%= ColorManager.ColorGridHeader %>;
}

.TagGridFilter1
{
	border-color: <%= ColorManager.ColorGridHeader %>;
    background-color: #DEE8F5;
}

.TagGridFilter2
{
	border-color: <%= ColorManager.ColorGridHeader %>;
    background-color: <%= ColorManager.ColorGridHeader %>;
}

.ToolBar
{
	height: 34px;
	width: 100%;
	padding: 0px;
	border-width: 0px;
	background-color: #EFEFEF;
}

.InfoPageHeader
{
	padding: 8px;
	height: 35px;
	color: #FFFFFF;
	font-weight: bold;
	background-color: <%= ColorManager.ColorTitleBar %>;
}

.InfoContents
{
	padding: 8px;
}

.DetailFormView
{
	min-height: 100%;
	overflow: auto;
	margin: 0px;
	padding: 5px 10px 5px 10px;
	width: 100%;
}

.DetailViewControlPanel
{
	height: 24px;
	width: 25px;
	vertical-align: middle;
	padding: 0px;
	margin: 0px;
	border: none;
	background-color: #D1DDF1;
}

.FormViewControlPanel
{
	height: 29px;
	vertical-align: middle;
	padding: 0px;
	margin: 0px;
	border-width: 1px;
	border-color:Gray;
	border-style: solid;
	background-color: #D1DDF1;
	width: auto;
}

.FormViewControlPanelImage
{
	padding-left: 10px;
	padding-right: 10px;
}

.FormViewLayoutTable
{
	width: auto;
	padding: 2px;
	margin: 0px;
	border-width: 1px;
	background-color: <%= ColorManager.ColorBackground %>;
	overflow-x: auto;
}

.UpdateImage
{
	margin-right: 10px;
	float: left;
	height: 25px;
}

.QuickFilterBox
{
	font-size: small;
	padding: 0 0 0 3;
	margin: 0;
	vertical-align: middle;
	background-color: #EEEEEE;
	width: 75px;
}

.NumericTextBox
{
	vertical-align: middle;
	text-align: right;	
	width: 60px;
}

.UserIDText
{
	color: #FFFFCC;
}

.CollapseImage
{
	margin-left: 5px;
	margin-right: 10px;
}

.WindowsStyle .ajax__combobox_inputcontainer .ajax__combobox_textboxcontainer INPUT {
	BORDER-RIGHT: #7f9db9 0px; PADDING-RIGHT: 0px; BORDER-TOP: #7f9db9 1px solid; PADDING-LEFT: 5px; FONT-SIZE: 13px; PADDING-BOTTOM: 0px; MARGIN: 0px; BORDER-LEFT: #7f9db9 1px solid; PADDING-TOP: 1px; BORDER-BOTTOM: #7f9db9 1px solid; HEIGHT: 18px
}
.WindowsStyle .ajax__combobox_inputcontainer .ajax__combobox_buttoncontainer BUTTON {
	BORDER-RIGHT: 0px; PADDING-RIGHT: 0px; BACKGROUND-POSITION: left top; BORDER-TOP: 0px; PADDING-LEFT: 0px; BACKGROUND-IMAGE: url(Images/windows-arrow.gif); PADDING-BOTTOM: 0px; MARGIN: 0px; BORDER-LEFT: 0px; WIDTH: 21px; PADDING-TOP: 0px; BORDER-BOTTOM: 0px; HEIGHT: 21px
}
.WindowsStyle .ajax__combobox_itemlist {
	BORDER-LEFT-COLOR: #7f9db9; BORDER-BOTTOM-COLOR: #7f9db9; BORDER-TOP-COLOR: #7f9db9; BORDER-RIGHT-COLOR: #7f9db9
}
