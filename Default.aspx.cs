using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace azureadpoc_framework48
{
    public partial class _Default : Page
    {
        protected string ClaimsListHtml { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.TraceInformation("Page load called from .NET Framework app.");
            litClaimsList.Text = string.Empty;

            if (!IsPostBack)
            {
                BuildClaimsList();
            }
        }

        private void BuildClaimsList()
        {
            var claims = ((ClaimsPrincipal)User).Claims;
            var sb = new StringBuilder();

            sb.Append("<ul class='text-start list-unstyled'>");

            foreach (var claim in claims)
            {
                sb.AppendFormat(
                    "<li><span class='fw-bold mx-sm-1'>{0}:</span><span>{1}</span></li>",
                    claim.Type, claim.Value);
            }

            sb.Append("</ul>");

            litClaimsList.Text = sb.ToString();
        }
    }
}