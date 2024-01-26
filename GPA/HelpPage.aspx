<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HelpPage.aspx.cs" Inherits="GPA.HelpPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="Styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
		<table width="100%" class="InfoPageHeader">
			<tr>
				<td align="left"><asp:Label ID="lblPageHeading" runat="server" Text=""></asp:Label>
				</td>
				<td align="right">
					<i>Operations Logger</i>
				</td>
			</tr>
		</table>
    <p></p><p></p>    
    <div>
        <asp:Table ID="tblHeaderTitle" runat="server" Visible="false">
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Left">
				    <b><asp:Label ID="lblHeaderTitle" runat="server" Text="Page Description"></asp:Label></b>
				</asp:TableCell>
            </asp:TableRow>
        </asp:Table>
		<table>
			<tr>
				<td align="left">
				    <asp:Label ID="lblHeader" runat="server" Text=""></asp:Label>
				</td>
			</tr>
		</table>
    </div>
    <p></p><p></p>
    <div>
        <asp:Table ID="tblCommonTitle" runat="server" Visible="false">
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Left">
				    <b><asp:Label ID="lblCommonTitle" runat="server" Text="Common Page Functionality"></asp:Label></b>
				</asp:TableCell>
            </asp:TableRow>
        </asp:Table>
  		<table>
			<tr>
				<td align="left">
                    <asp:Label ID="lblCommon" runat="server" Text=""></asp:Label>
				</td>
			</tr>
		</table>                    
    </div>
    <p></p><p></p>
    <asp:Table ID="tblItemsTitle" runat="server" Visible="false">
        <asp:TableRow>
            <asp:TableCell HorizontalAlign="Left">
			    <b><asp:Label ID="lblItemsTitle" runat="server" Text="Data Items Descriptions"></asp:Label></b>
			</asp:TableCell>
        </asp:TableRow>
    </asp:Table>
    <asp:Panel ID="GridViewContent" runat="server" Visible="false" >
  		<table width="100%">
			<tr>
				<td align="left">
                    <asp:GridView ID="GridView1" runat="server" CellPadding="4">
                    </asp:GridView>
				</td>
			</tr>
		</table>                                        
    </asp:Panel>
    </form>
</body>
</html>