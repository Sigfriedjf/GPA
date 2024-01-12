<%@ Page Title="ThreadInfo" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ThreadInfo.aspx.cs" Inherits="GPAutomation.ThreadInfo" %>
<%@ MasterType VirtualPath="~/Site.Master" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="cc1" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    div>
<asp:Panel ID="Summary" runat="server" CssClass="SectionPanel">
	<asp:Panel ID="SummaryHeader" runat="server" CssClass="CollapsibleHeader">
		<asp:Timer ID="tmUserChangeTimer" runat="server" Interval="1000" Enabled="false" EnableViewState="true" />
		<table width="100%">
			<tr>
				<td align="left" width="80%">
					<asp:Label ID="SummaryHeaderLabel" runat="server" Text="$$PageHeading$$" style="margin-left:10px;float:left"/>
					<asp:Label ID="Label3" runat="server" Text="$$PageScope$$" style="margin-left:30px;float:left"/>
				</td>
				<td nowrap="nowrap" align="right" width="20%" valign="middle">
					<asp:UpdatePanel ID="userPanel" runat="server">
						<ContentTemplate>
							<asp:Label ID="lbCurrentUser" runat="server" Text="User:" /> <asp:Label ID="lbUserID" runat="server" Text="" CSSClass="UserIDText"/>
							<asp:ImageButton runat="server" ID="ImageButton2" ImageUrl="~/Images/User.gif"  ToolTip="Change user"/>
						</ContentTemplate>
					</asp:UpdatePanel>
				</td>
			</tr>
		</table>
	</asp:Panel>

	<asp:Panel ID="SummaryContent" runat="server" CssClass="UncollapsibleCollapsibleContent">
			<asp:Table ID="Table7" CssClass="ToolBar" runat="server">
				<asp:TableRow>
					<asp:TableCell Height="25">
					    <div class="noPrint">
						<asp:Panel ID="GridToolPanel1" runat="server" CssClass="GridToolPanel">
						<asp:ImageButton runat="server" ID="ibRetrieve" ImageUrl="~/Images/filter_SQL_run.gif" CssClass="toolbarimage"  ToolTip="Refresh Grid"/>
						<asp:ImageButton runat="server" ID="ibConfigFilter" ImageUrl="~/Images/filter_config.gif" CssClass="toolbarimage" ToolTip="Configure Grid"/>
						<asp:ImageButton runat="server" ID="ibExcel" ImageUrl="~/Images/excel_sml.gif" CssClass="toolbarimage" ToolTip="Export to Excel"/>
						<asp:UpdatePanel ID="DetailPanel" runat="server" RenderMode="Inline"><ContentTemplate><asp:ImageButton runat="server" ID="ibDetails" ImageUrl="~/Images/filter_details.gif" CssClass="toolbarimage" ToolTip="View Details for selected item" /></ContentTemplate></asp:UpdatePanel>
						<asp:ImageButton runat="server" ID="ibHist" ImageUrl="~/Images/hist_sml.gif" CssClass="toolbarimage" ToolTip="View History"/>
						<asp:ImageButton runat="server" ID="ibMarkAsRead" ImageUrl="~/Images/markasread.gif" CssClass="toolbarimage" ToolTip="Mark all as Read"/>
						<asp:Image runat="server" ID="ibPagePrint" style="cursor:hand" ImageUrl="~/Images/print_sml.gif" CssClass="toolbarimage" ToolTip="Print this page"></asp:Image>
						<asp:Image runat="server" ID="ibPageInfo" style="cursor:hand" ImageUrl="~/Images/info_sml.gif" CssClass="toolbarimage" ToolTip="Help on this page"></asp:Image>
						</asp:Panel>
						</div>
						<asp:UpdatePanel ID="TotalRecPanel" runat="server"><ContentTemplate>
						<asp:Label ID="lblNumOfRows" runat="server" CssClass="toolbarlabel" Text="No. Rows:"/><asp:TextBox runat="server" ID="tbNumOfRows" ToolTip="Number of Rows" MaxLength="4" Columns="4" Width="30px" style="margin-left:4px"/>
						<asp:Label ID="lbTotalRecords" runat="server" Text=""/>
						</ContentTemplate></asp:UpdatePanel>
					</asp:TableCell>
					<asp:TableCell Width="50">
			      <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel_Summary"  DisplayAfter="200" >
							<ProgressTemplate>
								<asp:Image CssClass="UpdateImage" ID="LoadImage" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
							</ProgressTemplate>
						</asp:UpdateProgress>
					</asp:TableCell>
				</asp:TableRow>
			</asp:Table>
		<asp:Panel ID="Panel1" CssClass="ThreadListPanel" Width="99.7%" runat="server">
			<asp:Panel ID="SummaryPanel" CssClass="SummaryPanel" runat="server">
				<asp:UpdatePanel ID="UpdatePanel_Summary" runat="server">
					<ContentTemplate>
						<asp:Panel ID="FilteredMsgPanel" runat="server">
							<asp:Label ID="FilteredSelectionMsg" runat="server" Text="" Font-Bold="true" ForeColor="Red"/>
						</asp:Panel> 
						<%--
						<lms2:PCSGridView ID="GridView_Summary" runat="server" 
								CellPadding="4" GridLines="Horizontal" AllowPaging="True" AllowSorting="True" 
							EnableViewState="False" DataSourceID="ObjectDataSource1" PageSize="20" >
							<PagerSettings Mode="NumericFirstLast" FirstPageText="<<" LastPageText=">>" />
							<Columns>
								<asp:ImageField DataImageUrlField="ThumbnailImageUrl" NullImageUrl="~/Images/unknown.jpg" HeaderText="Img" SortExpression="ThumbnailImageUrl"/>
							</Columns>
						</lms2:PCSGridView>
						--%>
						
						<asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
							SelectMethod="QueryDataUUQP" TypeName="LMS2.components.SASWrapper"
							CacheDuration="30" CacheExpirationPolicy="Absolute" EnableCaching="true" CacheKeyDependency="LMSKey" >
							<SelectParameters>
								<asp:SessionParameter Name="pageID" SessionField="CurPageName" Type="String" />
							  <asp:SessionParameter DefaultValue="LMS2" Name="dbname" SessionField="DatabaseName" Type="String" />
								<asp:Parameter ConvertEmptyStringToNull="false" DefaultValue="" Name="table" Type="String" />  
							  <asp:SessionParameter DefaultValue="1" Name="OQL" SessionField="OverrideQueryLimit" Type="Int32" />
							  <asp:SessionParameter DefaultValue="*" Name="columns" SessionField="ColumnShown" Type="String" />
							  <asp:SessionParameter DefaultValue="15" Name="numRows" SessionField="NumOfRows" Type="Int32" />
							  <asp:SessionParameter DefaultValue="" Name="filter" SessionField="Filter" Type="String" />
								<asp:SessionParameter Name="sortString" SessionField="SortString" Type="String" />
								<asp:SessionParameter DefaultValue="0" Name="showObsolete" SessionField="ShowObsolete" Type="String" />
								<asp:SessionParameter DefaultValue="" Name="hiddenFilter" SessionField="HiddenFilter" Type="String" />
							</SelectParameters>
						</asp:ObjectDataSource>
					</ContentTemplate>
					<Triggers>
						<asp:AsyncPostBackTrigger ControlID="ibRetrieve" EventName="Click" />
					</Triggers>
				</asp:UpdatePanel>
				<br />
			</asp:Panel>

			<asp:Panel ID="DetailContent" runat="server" CssClass="DetailPanel">
				<asp:UpdatePanel ID="UpdatePanel_Detail" runat="server">
					<ContentTemplate>
                              <asp:Panel ID="SilverlightPanel" runat="server">
                           <asp:HiddenField ID="HiddenThreadSelected" runat="server" Value="" />	
                           <asp:HiddenField ID="HiddenThread" runat="server" Value="" />
                           <asp:HiddenField ID="HiddenPath" runat="server" Value="" />
                           <asp:HiddenField ID="HiddenTag" runat="server" Value="" />
                           <asp:HiddenField ID="HiddenLocation" runat="server" Value="" />
                           <asp:HiddenField ID="HiddenRegion" runat="server" Value="" />
                           <asp:HiddenField ID="HiddenArea" runat="server" Value="" />
                              <asp:HiddenField ID="HiddenInstanceDatabase" runat="server" Value="" />
                           <asp:HiddenField ID="HiddenDetailPage" runat="server" Value="" />
                              <div id="silverlightControlHost" style="width: 810px; height:1px">
							<object data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
						<param name="source" value="ClientBin/ThreadInfo.xap"/>
						<param name="onError" value="onSilverlightError" />
						<%-- 
						<param name="background" value="<%= ColorManager.ColorBody %>" />
								--%>
						<param name="minRuntimeVersion" value="5.0.61118.0" />
						<param name="autoUpgrade" value="true" />
						<a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.61118.0" style="text-decoration:none">
								<img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight" style="border-style:none"/>
						</a>
						</object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe></div>
                              </asp:Panel>
					</ContentTemplate> 
				</asp:UpdatePanel> 
			</asp:Panel>
		</asp:Panel>
	</asp:Panel>
</asp:Panel>    
</div>


	
	<asp:Panel ID="DebugPanel" runat="server" style="float:left" Visible="false" Width="100%">
<asp:Panel ID="DebugPanelHeader" runat="server" CssClass="CollapsibleHeader">
	<table width="100%">
		<tr>
			<td align="left" width="50%">
				<asp:Label ID="Label1" runat="server" Text="&nbsp&nbsp *** Page Debug Information ***" /></td>
			<td align="right" width="50%">
				<asp:Image ID="DebugHeaderImage" runat="server" ImageUrl="images/collapse.jpg" style="margin-right:8px"/></td>
		</tr>
	</table>
</asp:Panel>

<asp:Panel ID="DebugPanelContent" runat="server" CssClass="CollapsibleContent">
	<div>
		<asp:Label ID="Debug_lPageID" runat="server" Text="" /><br />
		<asp:Label ID="Debug_lUserID" runat="server" Text="" /><br />
		<asp:Label ID="Debug_lPermissions" runat="server" Text="" /><br />
		<asp:Label ID="Debug_lServer" runat="server" Text="" /><br />
		<asp:Label ID="Debug_lDatabaseName" runat="server" Text="" /><br />
		<asp:Label ID="Debug_lCulture" runat="server" Text="" /><br />
		<asp:Label ID="Debug_lSessionCached" runat="server" Text="" /><br />
	</div>
	<br /><br />
	<div>
		<asp:gridview ID="Debug_Gridview1" runat="server" CellPadding="4" ForeColor="#333333" 
			GridLines="None">
			<FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
			<RowStyle BackColor="#E3EAEB" />
			<PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
			<SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
			<HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
			<EditRowStyle BackColor="#7C6F57" />
			<AlternatingRowStyle BackColor="White" />
		</asp:gridview>
	</div>
	<br></br>
	<div>
  		<asp:GridView ID="Debug_GridView2" runat="server" BackColor="White" 
			BorderColor="#E7E7FF" BorderStyle="None" BorderWidth="1px" CellPadding="3" 
			GridLines="Horizontal">
			<FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" />
			<RowStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" />
			<PagerStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" HorizontalAlign="Right" />
			<SelectedRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="#F7F7F7" />
			<HeaderStyle BackColor="#4A3C8C" Font-Bold="True" ForeColor="#F7F7F7" />
			<AlternatingRowStyle BackColor="#F7F7F7" />
		</asp:GridView>
    </div>
</asp:Panel> 

<cc1:CollapsiblePanelExtender ID="CollapsiblePanelExtender2" runat="server" 
	 ImageControlID="DebugHeaderImage" 
	 TargetControlID="DebugPanelContent" 
	 ExpandControlID="DebugPanelHeader"
	 CollapseControlID="DebugPanelHeader"
	 Collapsed="True" 
	 CollapsedImage="images/expand.jpg"
	 ExpandedImage="images/collapse.jpg" />

	</asp:Panel>	

</asp:Content>