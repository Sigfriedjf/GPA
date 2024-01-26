<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TabStripUC.ascx.cs" Inherits="GPA.Controls_TabStripUC" %>
<div id="divTabs" class="shadetabs" runat="server">
    <asp:Menu ID="Menu1" Orientation="Horizontal" ItemWrap="true"
        StaticMenuItemStyle-CssClass="tab" StaticSelectedStyle-CssClass="selectedTab"
        CssClass="tabs" runat="server" OnMenuItemClick="Menu1_MenuItemClick">
    </asp:Menu>
</div>
