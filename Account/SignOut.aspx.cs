using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
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
            //if (Request.IsAuthenticated)
            //{
            //    // Redirect to home page if the user is authenticated.
            //    Response.Redirect("~/");
            //}
            Response.Redirect("~/");
        }
    }
}