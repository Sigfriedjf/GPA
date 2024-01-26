<%@ Page Title="ShiftHand" Language="C#" MasterPageFile="~/LMSGridViewFilter.master" AutoEventWireup="true" CodeBehind="DetailForm.aspx.cs" Inherits="GPA.DetailForm" %>

<%@ MasterType VirtualPath="~/LMSGridViewFilter.Master" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register Assembly="LMS2.components" Namespace="LMS2.components" TagPrefix="lms2" %>


<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <asp:ImageButton ID="BackButton" runat="server" ImageUrl="~/Images/back.gif" OnClick="BackButtonClick" />

    <asp:Panel ID="Summary" runat="server" CssClass="SectionPanel">
        <asp:Panel ID="SummaryHeader" runat="server" CssClass="CollapsibleHeader">
            <asp:Timer ID="tmUserChangeTimer" runat="server" Interval="1000" Enabled="false" EnableViewState="true" />
            <table width="100%">
                <tr>
                    <td align="left" width="80%">
                        <asp:Label ID="SummaryHeaderLabel" runat="server" Text="$$PageHeading$$" Style="margin-left: 10px; float: left" />
                        <asp:Label ID="Label3" runat="server" Text="$$PageScope$$" Style="margin-left: 30px; float: left" />
                    </td>
                    <td nowrap="nowrap" align="right" width="20%" valign="middle">
                        <asp:UpdatePanel ID="userPanel" runat="server">
                            <ContentTemplate>
                                <asp:Label ID="lbCurrentUser" runat="server" Text="User:" />
                                <asp:Label ID="lbUserID" runat="server" Text="" />
                                <asp:ImageButton runat="server" ID="ImageButton2" ImageUrl="~/Images/User.gif" OnClick="ChangeUser" ToolTip="Change user" ImageAlign="Middle" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </td>
                </tr>
            </table>
        </asp:Panel>

        <asp:UpdatePanel ID="TransferPanel" runat="server">
            <ContentTemplate>
                <asp:Table ID="LabXferTable" runat="server" Width="100%" Visible="false">
                    <asp:TableRow ID="LabXferTableRow" Width="100%" runat="server">
                        <asp:TableCell ID="LabXferTableCell" runat="server" Width="100%" HorizontalAlign="Right">
                            <asp:Button ID="LabXferButton" runat="server" OnClick="OnLabXfer" Text="LabXferButton" />
                            <asp:TextBox ID="tbTankName" runat="server" Width="100px" />
                            <asp:Label ID="lbLabXferOk" runat="server" Text="Transfer Complete" ForeColor="Green" Font-Bold="true" Visible="false" />
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
            </ContentTemplate>
        </asp:UpdatePanel>

        <asp:Panel ID="DetailContent" runat="server" CssClass="DetailFormPanel">
            <asp:UpdatePanel ID="DetailUpdatePanel" runat="server">
                <ContentTemplate>
                    <lms2:PCSFormView ID="FormView1" runat="server" DataSourceID="SASDetailDataSource1" CssClass="DetailFormView" OnModeChanging="FormViewModeChanging" OnItemDeleted="FormViewItemDeleted" OnItemInserted="FormViewItemInserted" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>
        <lms2:SASDetailDataSource ID="SASDetailDataSource1" runat="server" />
    </asp:Panel>

    <asp:Panel ID="DebugPanel" runat="server" Style="float: left" Visible="false" Width="100%">
        <asp:Panel ID="DebugPanelHeader" runat="server" CssClass="CollapsibleHeader">
            <table width="100%">
                <tr>
                    <td align="left" width="50%">
                        <asp:Label ID="Label1" runat="server" Text="&nbsp&nbsp *** Page Debug Information ***" /></td>
                    <td align="right" width="50%">
                        <asp:Image ID="DebugHeaderImage" runat="server" ImageUrl="images/collapse.jpg" Style="margin-right: 8px" /></td>
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
            <br />
            <br />
            <div>
                <asp:GridView ID="Debug_Gridview1" runat="server" CellPadding="4" ForeColor="#333333"
                    GridLines="None">
                    <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                    <RowStyle BackColor="#E3EAEB" />
                    <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                    <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                    <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                    <EditRowStyle BackColor="#7C6F57" />
                    <AlternatingRowStyle BackColor="White" />
                </asp:GridView>
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
