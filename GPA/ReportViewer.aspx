<%@ Page Language="C#" MasterPageFile="~/LMSiframe.master" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="GPA.ReportViewer" %>
<%@ Register TagPrefix="uc1" TagName="TabStrip" Src="~/Controls/TabStripUC.ascx" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<%@ MasterType VirtualPath="~/LMSiframe.master" %>
<%@ Register Assembly="LMS2.components" Namespace="LMS2.components" TagPrefix="lms2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <uc1:TabStrip id="TabStrip" runat="server" Visible="false"></uc1:TabStrip>
		<asp:Panel ID="ReportPrintPanel" runat="server" HorizontalAlign="Right" Width="98%">
			<asp:Label ID="lbPrintMsg" runat="server" Text="Your report has been submitted for printing" style="vertical-align:top; text-align:left;" Font-Bold="true" Visible="false" ForeColor="DarkGreen"/>
			<asp:Label ID="lbNoPrinter" runat="server" Text="You must select a printer from the list before printing" style="vertical-align:top; text-align:left;" Font-Bold="true" Visible="false" ForeColor="Red"/>
			&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
			<asp:Label ID="Label2" runat="server" Text="Select a printer:" style="vertical-align: top" />&nbsp;
			<asp:DropDownList ID="ddlPrinter" runat="server" Width="150px"/>&nbsp;
			<asp:ImageButton ID="ibPrinter" runat="server" Height="20px" ImageUrl="~/Images/print_sml.gif" Width="20px" onclick="ibPrint_Click" />
		</asp:Panel>
		<table style="width:99.9%; height:99.9%; margin:0; padding:0; overflow:auto">
			<tr>
				<td valign="top" colspan="3">
					<rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" AsyncRendering="False" 
						Font-Size="8pt" Height="100%" ProcessingMode="Remote" Width="100%" >
						<ServerReport ReportServerUrl="" ReportPath="" />
					</rsweb:ReportViewer> 
				</td>
			</tr>
		</table>
</asp:Content>