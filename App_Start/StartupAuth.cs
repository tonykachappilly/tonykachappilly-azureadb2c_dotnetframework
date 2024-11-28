using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace azureadpoc_framework48
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string instance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:Instance"]);
        private static string domain = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:Domain"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string signUpSignInPolicyId = ConfigurationManager.AppSettings["ida:SignUpSignInPolicyId"];
        private static string signedOutCallbackPath = ConfigurationManager.AppSettings["ida:SignedOutCallbackPath"];

        //string authority = aadInstance + tenantId + "/v2.0";

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication
                (
                    new CookieAuthenticationOptions
                    {
                        AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                        //LoginPath = new PathString(postLogoutRedirectUri),
                        //LogoutPath = new PathString(signedOutCallbackPath),
                        //ExpireTimeSpan = TimeSpan.FromMinutes(15), // Set cookie expiration time
                        //SlidingExpiration = true, // Renew the cookie on each request
                        Provider = new CookieAuthenticationProvider()
                        {
                            OnValidateIdentity = (context) =>
                            {
                                return System.Threading.Tasks.Task.FromResult(0);
                            }
                        }
                    }
                );

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,                       
                    },
                    ProtocolValidator = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectProtocolValidator() 
                    { 
                        RequireNonce = false,
                        RequireStateValidation = false,
                    },
                    ClientId = clientId,
                    ClientSecret = "759f3f16-f160-4c74-bbd1-70f65c7fd236",
                    Authority = $"{instance}{domain}{signUpSignInPolicyId}/v2.0/" ,
                    RedirectUri = GetRedirectUri(),
                    ResponseType = "code id_token",
                    Scope = $"openid offline_access",
                    PostLogoutRedirectUri = GetPostLogoutRedirectUri(),
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthenticationFailed = (context) =>
                        {
                            return System.Threading.Tasks.Task.FromResult(0);
                        },
                        SecurityTokenReceived = (context) => 
                        {
                            //await CallForRefreshToken(context);
                            return System.Threading.Tasks.Task.FromResult(0); 
                        },
                        SecurityTokenValidated = (context) =>
                        {
                            var authenticationContext = new AuthenticationContext();
                            string name = context.AuthenticationTicket.Identity.FindFirst("name")?.Value ?? context.AuthenticationTicket.Identity.FindFirst("displayName")?.Value;
                            context.AuthenticationTicket.Identity.AddClaim(new Claim(ClaimTypes.Name, name, string.Empty));
                            return System.Threading.Tasks.Task.FromResult(0);
                        }
                    }
                });

            // This makes any middleware defined above this line run before the Authorization rule is applied in web.config
            app.UseStageMarker(PipelineStage.Authenticate);
        }

        string GetRedirectUri()
        {
#if DEBUG
            return "https://localhost:44332/signin-oidc";
#else
    return ConfigurationManager.AppSettings["ida:RedirectUri"];
#endif
        }

        string GetPostLogoutRedirectUri()
        {
#if DEBUG
            return "https://localhost:44332/";
#else
    return ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
#endif
        }

        private async Task<Task> CallForRefreshToken(SecurityTokenReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://tonykachappillyb2c.b2clogin.com/tonykachappillyb2c.onmicrosoft.com/B2C_1A_ContosoCustomPolicy/oauth2/v2.0/token");
            //request.Headers.Add("Cookie", "x-ms-cpim-cache|czyo3naydkubj9cbhtz_bq_0=m1.MgpnSMSnJgl2A8mK.rmctnj+a9aNFOn8CBBCDLQ==.0.yT/KpaLiQxTR/4elIJTfp/03MW4WwwbMFqgD6q982NR4akddY3IM9kScii/CXazmCctXZf0NDqkCBiA6KfF3iyU0oK5PzY6HepqzPDd0DLbwVGJGY9MutxUYXg4sFMM6/v3csimeljyq5jc9ojvfOOFfMkCKSrxZSqkjcA5xINzxg+dTlA8+aEiokV6L3lMGL+ua/2ImXE/XtLotwmudI1bFVsAqwnBTyaFB5Me7aPa12yQsnYb9UIUhyREo3IrXjtL7pvozzHsfTjo3B3TRCIOI1X1oSpZuVHMSV8ej6CAVUxok1LhrNYxTVvu2Hu3oDvIZof96tIr/unnUdaUpLcP5nPW68x/+ezll9RQGhEe/226O4GgI8QxLWNdgP7dtG3hlPxEiEJ3poXLP5rcD4XMhlf1pFVAyyQMfFK0ic3j8Qa4SNbjuNgx6MVhsJcxFsydAJClsYhFIJ2hMCIF2ZBLjK93ivKhHQ39aBF+32XFQb2tr+WmSdbHds1lZZIyz97HbCAPUok80G/UxTcWrQwQW+S5uM3lQ0CxJUMZNke2tqy3HzsrL8kMxig0e2EdxegdPP7bTazDuGR0CKA3iHhV0hNgvh+5yxN85S/8mCC/1DXa5VNawr/LwsORVXmVrU7RvAhQZZCd+G6SngfRSjh+30Wrmh1nVEH9Wl5Pj/MmTYJ25SfKwd6r1x2srYUVSteioBhX932Ge55hhtACknaE/vfWTYGmFUGbZMypcJB6t3mT188OOpri5aiekr5ibHvRYhvUWW2Sn33hf+rMIeEyOYzvlVe4v5DdT5zvz1ZQ80rDIlOAb7K9Cni2+QqBbtpjz83eQkKmVpfvlgEmKT404grkHf3UyW2DpEtFThQU7rqmu; x-ms-cpim-csrf=WnNJTFh6aCtUOThnWURxOTRRdVVjNVV5aWZ2UUp6cThCVW1PT012WmtzZ2U2MVJrQ1FBUUdOVFVWdzlOWHBpczJzNlIxTUdRU3h5b2o0ZmhnbU96Z0E9PTsyMDI0LTExLTIyVDA2OjI3OjM0LjM3NjY0NThaO2VOVHBHQlFHRjZvWW9DN0IwdFZXMmc9PTt7Ik9yY2hlc3RyYXRpb25TdGVwIjoxfQ==; x-ms-cpim-trans=eyJUX0RJQyI6W3siSSI6ImRjMGUzNjBiLTk4ZDYtNGI3Ni05YjI3LWQ3MDE4NmRjZmY2ZCIsIlQiOiJ0b255a2FjaGFwcGlsbHliMmMub25taWNyb3NvZnQuY29tIiwiUCI6IkIyQ18xQV9EZW1vX3NpZ251cF9zaWduaW5fUmVmcmVzaFRva2VuSm91cm5leSIsIkMiOiI1MjE3MjNlZi1jMjQxLTQ1ZGYtOTU0Ny05MWNhYWQ4ODBmMzQiLCJTIjoxLCJNIjp7fSwiRCI6MCwiRSI6IiJ9XX0=");
            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            collection.Add(new KeyValuePair<string, string>("client_id", "7dd8bdd8-1b9a-4ba9-a726-4173d79c18b9"));
            collection.Add(new KeyValuePair<string, string>("scope", "openid offline_access"));
            collection.Add(new KeyValuePair<string, string>("code", context.ProtocolMessage.Code));
            collection.Add(new KeyValuePair<string, string>("redirect_uri", "https://localhost:44332/signin-oidc"));
            collection.Add(new KeyValuePair<string, string>("client_secret", ".028Q~puzS74BurcvPsEjKoijAm2l.vreBda~cPq"));
            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return Task.CompletedTask;

        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}
