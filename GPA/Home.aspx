<%@ Page Language="C#" MasterPageFile="~/LMSiframe.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="GPA.Home" %>
<%@ MasterType VirtualPath="~/LMSiframe.Master" %>
<%@ Register Assembly="LMS2.components" namespace="LMS2.components" tagprefix="lms2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <%--<asp:Timer ID="tmUserChangeTimer" runat="server" Interval="1000" OnTick="ChangeUserPostbackTimer_Tick" Enabled="false" EnableViewState="true" ></asp:Timer>--%>
    <div>
        <noscript>
            <asp:Table ID="tblNoSript" runat="server" Width="700" Height="300" BorderWidth="1" BorderColor="#990000" CellSpacing="0" CellPadding="1" HorizontalAlign="Center">
                <asp:TableRow ID="TableRow3" Height="100%" runat="server">
                    <asp:TableCell ID="TableCell3" Height="100%" runat="server">
                        <asp:Table ID="Table1" runat="server" Width="100%" Height="100%" BorderStyle="None" CellSpacing="0" CellPadding="8">
                            <asp:TableRow ID="TableRow4" Height="100%" runat="server">
                                <asp:TableCell ID="TableCell4" Height="100%" BackColor="#FFFFD5" runat="server">
                                    <asp:Label ID="lblNoScript1" runat="server" CssClass="PageName" ForeColor="#990000" Text="To use LMS2 you must enable scripting (or JavaScript) in your browser."></asp:Label><br />
                                    <asp:Label ID="lblNoScript2" runat="server" ForeColor="#990000" Text="Please enable scripting and then reload this page."></asp:Label><br />
                                </asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </noscript>
        <div>
            <asp:Table ID="tblBackground" runat="server" Width="100%">
                <asp:TableRow ID="TableRow1" runat="server">
                    <asp:TableCell ID="TableCell2" runat="server" HorizontalAlign="Center">
                        <asp:Image ID="imgBackground" ImageUrl="~/Images/defaultbkgrd.jpg" Enabled="false" runat="server" />
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </div>
        <asp:Table ID="tblVersion" runat="server" CssClass="version">
            <asp:TableRow ID="TableRow2" runat="server">
                <asp:TableCell ID="TableCell1" runat="server">
                    <asp:Label ID="lblVersion" runat="server" Text="Version"></asp:Label>
                    <asp:Label ID="lblVersionval" Font-Bold="true" runat="server"></asp:Label>
                    <asp:Label ID="Label1" Visible="false" Font-Bold="true" runat="server" Text="2.0.11006" />&nbsp;&nbsp;&nbsp;
							<asp:Label ID="lblUserID" runat="server" Text="User"></asp:Label>
                    <asp:Label ID="lblUserIDval" Font-Bold="true" runat="server"></asp:Label>&nbsp;
							<asp:ImageButton runat="server" ID="ImageButton2" ImageUrl="~/Images/User.gif" OnClick="ChangeUser" ToolTip="Change user" ImageAlign="Bottom" BorderStyle="Solid" BorderWidth="1px" BorderColor="LightGray" />
                    &nbsp;&nbsp;&nbsp;
							<asp:Label ID="lblDBNameServer" runat="server" Text="Database"></asp:Label>
                    <asp:Label ID="lblDBNameServerval" Font-Bold="true" runat="server"></asp:Label><br />
                    <asp:Label ID="lbPasswordExp" runat="server" Text=""></asp:Label>
                    <asp:Button ID="ChgPassword" runat="server" Text="PWDChgPwdLink" OnClick="OnChgPassword" />
                </asp:TableCell></asp:TableRow></asp:Table><div class="Announcements">
            <lms2:PCSGridView ID="GridView1" BorderWidth="0" runat="server" GridLines="None" Width="100%" CellPadding="4">
							<RowStyle VerticalAlign="Top" />
							<HeaderStyle HorizontalAlign="Left" />
					</lms2:PCSGridView>
        </div>
    </div>
</asp:Content>
