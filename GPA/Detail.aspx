<%@ Page Language="C#" MasterPageFile="~/LMSGridViewFilter.master" AutoEventWireup="true" CodeBehind="Detail.aspx.cs" Inherits="GPA.Detail" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/LMSGridViewFilter.Master" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="cc1" %>
<%@ Register namespace="LMS2.components" tagprefix="lms2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

		<asp:Panel ID="Summary" runat="server" CssClass="SectionPanel" >
			<asp:Panel ID="SummaryHeader" runat="server" CssClass="CollapsibleHeader" >
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
									<asp:Label ID="lbCurrentUser" runat="server" Text="User:" /> <asp:Label ID="lbUserID" runat="server" Text="" />
									<asp:ImageButton runat="server" ID="ImageButton2" ImageUrl="~/Images/User.gif" OnClick="ChangeUser" ToolTip="Change user" ImageAlign="Middle"/>	
								</ContentTemplate>
							</asp:UpdatePanel>
						</td>
					</tr>
				</table>
			</asp:Panel>

			<asp:Panel ID="DetailContent" runat="server" CssClass="DetailPanel">
				<asp:UpdatePanel ID="DetailUpdatePanel" runat="server">
					<ContentTemplate>
							<lms2:PCSDetailsView ID="DetailsView1" runat="server" CellPadding="4" BorderStyle="Solid" BorderWidth="1px" CssClass="TagGridFilter"
								ForeColor="#333333" GridLines="Horizontal" DataSourceID="SASDetailDataSource1" AutoGenerateRows="false" OnItemDeleted="ItemDeleted" OnItemInserted="ItemInserted"
								OnItemUpdated="ItemUpdated" DefaultMode="ReadOnly" >
								<FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
								<CommandRowStyle BackColor="#D1DDF1" Font-Bold="True" />
								<RowStyle BackColor="#EFF3FB" />
								<FieldHeaderStyle BackColor="#DEE8F5" Font-Bold="True" />
								<PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
								<Fields>
									<asp:CommandField ControlStyle-CssClass="toolbarimage" ButtonType="Image" CancelImageUrl="~/images/cancel_sm.gif" 
										DeleteImageUrl="~/images/delete_sm.gif" EditImageUrl="~/images/edit_sm.gif"
										ShowDeleteButton="True" ShowEditButton="True" ShowInsertButton="True" 
										UpdateImageUrl="~/images/update_sm.gif" NewImageUrl="~/images/new_sm.gif"
										InsertImageUrl="~/images/update_sm.gif" />
								</Fields>
								<HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
								<EditRowStyle BackColor="#96D4C9" />
								<AlternatingRowStyle BackColor="White" />
							</lms2:PCSDetailsView>

					</ContentTemplate>
				</asp:UpdatePanel>
			</asp:Panel>
			<lms2:SASDetailDataSource ID="SASDetailDataSource1" runat="server" />
		</asp:Panel> 
	
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

