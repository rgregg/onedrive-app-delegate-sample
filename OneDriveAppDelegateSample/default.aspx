<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="OneDriveAppDelegateSample._default" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>OneDrive App Delegate Permissions Example</title>
    <link rel="stylesheet" href="https://appsforoffice.microsoft.com/fabric/1.0/fabric.min.css" />
    <link rel="stylesheet" href="https://appsforoffice.microsoft.com/fabric/1.0/fabric.components.min.css" />
    <link rel="stylesheet" href="Styles.css" />
    <script src="jquery-2.2.0.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">

        <div class="ms-NavBar" runat="server" id="navBarMenu" visible="false">
            <div class="ms-NavBar-openMenu js-openMenu"></div>
            <ul class="ms-NavBar-items">
            <li class="ms-NavBar-item">
                <a class="ms-NavBar-link" href="default.aspx">Home</a>
            </li>
            <li class="ms-NavBar-item">
                <a class="ms-NavBar-link" href="Users.html"><i class="ms-Icon ms-Icon--people"></i>&nbsp;Tenant Users</a>
            </li>
            <li class="ms-NavBar-item ms-NavBar-item--right">
                <a class="ms-NavBar-link" href="default.aspx?logout=1">Logout</a>
            </li>
            </ul>
        </div>

        <h1 class="ms-font-su">OneDrive App Delegated Permissions Sample</h1>
        <p class="ms-font-m-plus">This sample uses app delegated permissions to access tenent data without requiring every user to sign into the application.</p>
        <asp:Label runat="server" ID="labelServiceTarget" CssClass="ms-font-s">Target: Production</asp:Label>

        <asp:Panel runat="server" ID="panelSignIn" CssClass="ms-font-m-plus">
            <p>To get started, you need to sign into this application and authorize it to access your tenant using a tenant admin account.</p>
            <asp:HyperLink runat="server" ID="signInLink">Sign in and enroll my tenant</asp:HyperLink>
        </asp:Panel>

        <asp:Panel runat="server" ID="panelAuthenticated" CssClass="ms-font-m-plus">
            <p>You've authenticated this app to work in your tenant. Great.</p>
            <p><a href="Users.html">Browse tenant users</a></p>

            <h3 class="ms-font-xl">Generate Access Token</h3>
            <p class="ms-font-m">
                For testing scenarios, you can generate an access token as this application for a particular resource:<br />
                <asp:TextBox runat="server" ID="textBoxResourceUri" Width="500px" Text="https://graph.microsoft.com"></asp:TextBox><asp:Button runat="server" ID="buttonGetAccessToken" Text="Request token" OnClick="buttonGetAccessToken_Click" />
            </p>
            <asp:TextBox runat="server" ID="accessToken" Font-Names="Consolas" Height="230px" TextMode="MultiLine" Width="627px"></asp:TextBox>
        </asp:Panel>
    </form>
</body>
<script src="fabric/Jquery.NavBar.js"></script>
</html>
