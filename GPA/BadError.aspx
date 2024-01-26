<%@ Page Language="C#" MasterPageFile="~/LMSGridViewFilter.master" AutoEventWireup="true" CodeBehind="BadError.aspx.cs" Inherits="GPA.BadError" %>
<%@ MasterType VirtualPath="~/LMSGridViewFilter.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<div style="height: 332px; width:100%; text-align:center; vertical-align:middle"> 
		<br />
		<hr />   
  	<asp:Label ID="Label1" runat="server" 
			Text="BadError" 
			style="font-family: Arial, Helvetica, sans-serif; font-size: large; color: #FF3300;" />
		<br />
		<hr />
		<br />
		<center>
        <asp:Table ID="Table1" runat="server" style="width:70%" CellPadding="3" CellSpacing="5"></asp:Table>
        </center>
  </div>
</asp:Content>
