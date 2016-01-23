<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="OneDriveAppDelegateSample._default" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        body {
            font-family: 'Segoe UI', sans-serif;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h1>OneDrive App Delegated Permissions Sample</h1>
        <p>This sample uses app delegated permissions to access tenent data without requiring every user to sign into the application.</p>
        <asp:Label runat="server" ID="labelServiceTarget">Target: Production</asp:Label>
    
        <asp:Panel runat="server" ID="panelSignIn">
            <p>To get started, you need to sign into this application and authorize it to access your tenant using a tenant admin account.</p>
            <asp:HyperLink runat="server" ID="signInLink">Sign in and enroll my tenant</asp:HyperLink>
        </asp:Panel>
        
        <asp:Panel runat="server" ID="panelAuthenticated">
            <p>You've authenticated this app to work in your tenant. Great.</p>
            <p><a href="Users.html">Browse tenant users</a></p>
            
            
            <h3>Generate Access Token</h3>
            <p>
                For testing scenarios, you can generate an access token as this application for a particular resource:<br />
                <asp:TextBox runat="server" ID="textBoxResourceUri" Width="500px" Text="https://graph.microsoft.com"></asp:TextBox><asp:Button runat="server" ID="buttonGetAccessToken" Text="Request token" OnClick="buttonGetAccessToken_Click" />
            </p>
            <asp:TextBox runat="server" ID="accessToken" Font-Names="Consolas" Height="230px" TextMode="MultiLine" Width="627px"></asp:TextBox>
        </asp:Panel>
    </form>
</body>
</html>
