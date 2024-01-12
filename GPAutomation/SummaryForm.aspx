<%@ Page Title="SummaryForm" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SummaryForm.aspx.cs" Inherits="GPAutomation.SummaryForm" %>
<%@ MasterType VirtualPath="~/Site.Master" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="cc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<div>
		<asp:Panel ID="Summary" runat="server" CssClass="SectionPanel">
			<asp:Panel ID="SummaryHeader" runat="server" CssClass="CollapsibleHeader">
				<asp:Timer ID="tmUserChangeTimer" runat="server" Interval="1000" OnTick="ChangeUserPostbackTimer_Tick" Enabled="false" EnableViewState="true" />
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
									<asp:ImageButton runat="server" ID="ImageButton2" ImageUrl="~/Images/User.gif" OnClick="ChangeUser" ToolTip="Change user" ImageAlign="Middle"/>	
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
								<asp:ImageButton runat="server" ID="ibRetrieve" ImageUrl="~/Images/filter_SQL_run.gif" CssClass="toolbarimage" OnClick="ibRetrieve_Click" ToolTip="Refresh Grid"/>
								<asp:ImageButton runat="server" ID="ibConfigFilter" ImageUrl="~/Images/filter_config.gif" CssClass="toolbarimage" OnClick="ibConfigFilter_Click" ToolTip="Configure Grid"/>
								<asp:ImageButton runat="server" ID="ibExcel" ImageUrl="~/Images/excel_sml.gif" CssClass="toolbarimage" OnClick="ibExcel_Click" ToolTip="Export to Excel"/>
								<asp:UpdatePanel ID="DetailPanel" runat="server" RenderMode="Inline"><ContentTemplate><asp:ImageButton runat="server" ID="ibDetails" ImageUrl="~/Images/filter_details.gif" CssClass="toolbarimage" OnClick="ibDetails_Click" ToolTip="View Details for selected item" /></ContentTemplate></asp:UpdatePanel>
								<asp:ImageButton runat="server" ID="ibHist" ImageUrl="~/Images/hist_sml.gif" CssClass="toolbarimage" OnClick="ibHist_Click" ToolTip="View History"/>
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
					      <asp:UpdateProgress ID="UpdateProgress2" runat="server" AssociatedUpdatePanelID="UpdatePanel_Detail"  DisplayAfter="200" >
									<ProgressTemplate>
										<asp:Image CssClass="UpdateImage" ID="LoadImage2" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
									</ProgressTemplate>
								</asp:UpdateProgress>
							</asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				<asp:Panel ID="Panel1" CssClass="ListPanel" Width="99.7%" runat="server">
					<asp:Panel ID="SummaryPanel" CssClass="SummaryPanel" runat="server">
						<asp:UpdatePanel ID="UpdatePanel_Summary" runat="server">
							<ContentTemplate>
								<asp:Panel ID="UpdateCounter" runat="server">
									<asp:Label ID="lblUpdated" runat="server" Text="Updated"></asp:Label> <asp:Label ID="lbAjaxCounter" runat="server" Text="0"></asp:Label> <asp:Label ID="lblSecondsAgo" runat="server" Text="Seconds ago"></asp:Label>
								</asp:Panel>
								<asp:Panel ID="FilteredMsgPanel" runat="server">
									<asp:Label ID="FilteredSelectionMsg" runat="server" Text="" Font-Bold="true" ForeColor="Red"/>
								</asp:Panel> 
								<lms2:PCSGridView ID="GridView_Summary" runat="server" 
										CellPadding="4" GridLines="Horizontal" AllowPaging="True" AllowSorting="True" 
									EnableViewState="False" DataSourceID="ObjectDataSource1" PageSize="20" >
									<PagerSettings Mode="NumericFirstLast" FirstPageText="<<" LastPageText=">>" />
									<Columns>
										<asp:ImageField DataImageUrlField="ThumbnailImageUrl" NullImageUrl="~/Images/unknown.jpg" HeaderText="" ReadOnly="true" ControlStyle-Height="75px"/>
									</Columns>
								</lms2:PCSGridView>
								<asp:Timer ID="Timer1" runat="server" Interval="10000" Enabled="false"></asp:Timer>
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
								<asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
								<asp:AsyncPostBackTrigger ControlID="FormView1" EventName="ItemInserted" />
								<asp:AsyncPostBackTrigger ControlID="ibRetrieve" EventName="Click" />
							</Triggers>
						</asp:UpdatePanel>
						<br />
					</asp:Panel>
				</asp:Panel>
					<asp:Panel ID="DetailContent" runat="server" CssClass="PanelGridContents">
						<asp:UpdatePanel ID="UpdatePanel_Detail" runat="server" >
							<ContentTemplate>
								<lms2:PCSFormView ID="FormView1" runat="server" DataSourceID="SASDetailDataSource1" CSSClass="DetailFormView" 
								OnItemInserted="MarkCacheStale" OnItemDeleted="MarkCacheStale" OnItemUpdated="MarkCacheStale">
								  <EmptyDataTemplate>
									  <asp:Panel ID="EmptyControlPanelID" runat="server" CssClass="DetailViewControlPanel">
										  <asp:ImageButton ID="InsertButton" runat="server" ImageUrl="~/images/new_sm.gif" BackColor="Transparent" OnClick="OnInsertButtonClick"/>
										</asp:Panel>
								  </EmptyDataTemplate>
								</lms2:PCSFormView> 
							</ContentTemplate>
							<Triggers>
								<asp:AsyncPostBackTrigger ControlID="GridView_Summary" EventName="SelectedIndexChanged" />
								<asp:AsyncPostBackTrigger ControlID="QuickFilterBox" EventName="TextChanged" />
								<asp:AsyncPostBackTrigger ControlID="ibApplyQuickFilter" EventName="Click" />
							</Triggers>
						</asp:UpdatePanel>
						<lms2:SASDetailDataSource ID="SASDetailDataSource1" runat="server" />
						<br />
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