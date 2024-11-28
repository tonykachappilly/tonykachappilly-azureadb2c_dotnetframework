using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace azureadpoc_framework48.Account
{
    public partial class SignOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            System.Diagnostics.Trace.TraceInformation("Signout was called from .NET Framework app.");
            
            HttpContext.Current.GetOwinContext().Authentication.SignOut(
            CookieAuthenticationDefaults.AuthenticationType,
            OpenIdConnectAuthenticationDefaults.AuthenticationType);

            string postLogoutRedirectUri = ResolveUrl(ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"]);
            string signOutUrl = $"https://tonykachappillyb2c.b2clogin.com/tonykachappillyb2c.onmicrosoft.com/{ConfigurationManager.AppSettings["ida:SignUpSignInPolicyId"]}/oauth2/v2.0/logout?post_logout_redirect_uri={postLogoutRedirectUri}";

            Response.Redirect(signOutUrl);

            //if (Request.IsAuthenticated)
            //{
            //    // Redirect to home page if the user is authenticated.
            //    Response.Redirect("~/");
            //}            
        }
    }
}