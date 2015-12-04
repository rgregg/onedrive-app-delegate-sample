<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="OneDriveAppDelegateSample._default" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <h1>OneDrive App Delegated Permissions Sample</h1>
        <p>This sample uses app delegated permissions to access tenent data without requiring every user to sign into the application.</p>
    
        <asp:Panel runat="server" ID="panelSignIn">
            <p>To get started, you need to sign into this application and authorize it to access your tenant using a tenant admin account.<br />
                <asp:HyperLink runat="server" ID="signInLink">Enroll my tenant</asp:HyperLink></p>    
        </asp:Panel>
        
        <asp:Panel runat="server" ID="panelAuthenticated">
            <p>You've authenticated this app to work in your tenant. Great.</p>
            
            <p>
                Now we need an access token to actually make APIs for your app. Enter the resource URL you are attempting to access:<br />
                <asp:TextBox runat="server" ID="textBoxResourceUri" Width="500px" Text="https://graph.microsoft.com"></asp:TextBox><asp:Button runat="server" ID="buttonGetAccessToken" Text="Request token" OnClick="buttonGetAccessToken_Click" />
            </p>
            
            <asp:TextBox runat="server" ID="accessToken" Font-Names="Consolas" Height="128px" TextMode="MultiLine" Width="591px"></asp:TextBox>
        </asp:Panel>
    </form>
</body>
</html>
