using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api;
using Newtonsoft.Json;
using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Web.Configuration;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;

namespace POTrackingV2.Controllers
{
    //[CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleVendor + "," + LoginConstants.RoleProcurement + "," + LoginConstants.RoleSubcontDev)]
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleProcurement + "," + LoginConstants.RoleSubcontDev)]
    public class DashboardController : Controller
    {
        //WebConfigurationManager.AppSettings["ActiveDirectoryUrl"];
        // GET: Dashboard
        public ActionResult Index()
        {
            try
            {
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            return View();
        }

        public ActionResult DashboardCommandCenterPOTracking()
        {
            try
            {
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            return View();
        }

        public ActionResult DashboardCommandCenterAlert()
        {
            try
            {
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            return View();
        }

        public ActionResult DashboardSubcontdevProgress()
        {
            try
            {
                //var tokenCredentials = new Microsoft.Rest.TokenCredentials("", "Bearer");
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                //ViewBag.Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkhsQzBSMTJza3hOWjFXUXdtak9GXzZ0X3RERSIsImtpZCI6IkhsQzBSMTJza3hOWjFXUXdtak9GXzZ0X3RERSJ9.eyJhdWQiOiJodHRwczovL21hbmFnZW1lbnQuYXp1cmUuY29tIiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvNjlhMDFkMDQtZWE5MS00YzkxLWI0NmUtODM2OTY2NzU0MWMwLyIsImlhdCI6MTU4MTkxOTk2NywibmJmIjoxNTgxOTE5OTY3LCJleHAiOjE1ODE5MjM4NjcsImFpbyI6IjQyTmdZQ2k3Y1hEV25XTnp0dmcvc2RiNitMWlZGQUE9IiwiYXBwaWQiOiIzMDU1ODAzYy1kNWI2LTQ4MWEtOGM5OC1hM2IyMmUwZjFkM2QiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC82OWEwMWQwNC1lYTkxLTRjOTEtYjQ2ZS04MzY5NjY3NTQxYzAvIiwib2lkIjoiYjE4M2UwNjItNDQ4MC00YWQ1LWE5MzEtNGM1YTA1YTE5OWVlIiwic3ViIjoiYjE4M2UwNjItNDQ4MC00YWQ1LWE5MzEtNGM1YTA1YTE5OWVlIiwidGlkIjoiNjlhMDFkMDQtZWE5MS00YzkxLWI0NmUtODM2OTY2NzU0MWMwIiwidXRpIjoiQ0xLaE1ma1dPMEthWmdNY1BEeGVBQSIsInZlciI6IjEuMCJ9.rr7iJxi9LwdukZuo-xvAuSLyulrVqAT9tmU68Y-y3SJP8kpqe-d6X0H9fkQBHPdJpWu93pE1275xmQZZEQItHX5PCOr5dPyHYysEQwsbUetxHQ-VPzMuX-5FhUB9KHRE7XjMv-Cd4ZYRJUeYJg3WIrPXRfwtandgdG9MDIcgWgGd-4cjHS9X9AvUfIFPUM0Vq3UWeSN0f3iYe1G6YlUKcuQ96EfFO-Lk8JAdc7s3UTYwqjPHOJpI89Hp7Y96IM9faa1IH5DBGynWFFw5NgUpbRmz2g2CCWamW-j5IJrBEQ4tYmJ_esvlSzZcaCQEB_seYXZRslVVrxVFoCImBG7Pdg";
                //ViewBag.Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkhsQzBSMTJza3hOWjFXUXdtak9GXzZ0X3RERSIsImtpZCI6IkhsQzBSMTJza3hOWjFXUXdtak9GXzZ0X3RERSJ9.eyJhdWQiOiJodHRwczovL21hbmFnZW1lbnQuYXp1cmUuY29tIiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvNjlhMDFkMDQtZWE5MS00YzkxLWI0NmUtODM2OTY2NzU0MWMwLyIsImlhdCI6MTU4MTkxOTk2NywibmJmIjoxNTgxOTE5OTY3LCJleHAiOjE1ODE5MjM4NjcsImFpbyI6IjQyTmdZQ2k3Y1hEV25XTnp0dmcvc2RiNitMWlZGQUE9IiwiYXBwaWQiOiIzMDU1ODAzYy1kNWI2LTQ4MWEtOGM5OC1hM2IyMmUwZjFkM2QiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC82OWEwMWQwNC1lYTkxLTRjOTEtYjQ2ZS04MzY5NjY3NTQxYzAvIiwib2lkIjoiYjE4M2UwNjItNDQ4MC00YWQ1LWE5MzEtNGM1YTA1YTE5OWVlIiwic3ViIjoiYjE4M2UwNjItNDQ4MC00YWQ1LWE5MzEtNGM1YTA1YTE5OWVlIiwidGlkIjoiNjlhMDFkMDQtZWE5MS00YzkxLWI0NmUtODM2OTY2NzU0MWMwIiwidXRpIjoiQ0xLaE1ma1dPMEthWmdNY1BEeGVBQSIsInZlciI6IjEuMCJ9.rr7iJxi9LwdukZuo-xvAuSLyulrVqAT9tmU68Y-y3SJP8kpqe-d6X0H9fkQBHPdJpWu93pE1275xmQZZEQItHX5PCOr5dPyHYysEQwsbUetxHQ-VPzMuX-5FhUB9KHRE7XjMv-Cd4ZYRJUeYJg3WIrPXRfwtandgdG9MDIcgWgGd-4cjHS9X9AvUfIFPUM0Vq3UWeSN0f3iYe1G6YlUKcuQ96EfFO-Lk8JAdc7s3UTYwqjPHOJpI89Hp7Y96IM9faa1IH5DBGynWFFw5NgUpbRmz2g2CCWamW-j5IJrBEQ4tYmJ_esvlSzZcaCQEB_seYXZRslVVrxVFoCImBG7Pdg";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            ViewBag.ReportID = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportId"]) ? WebConfigurationManager.AppSettings["ReportId"] : "";
            ViewBag.ReportURL = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportSubcontdevProgress"]) ? WebConfigurationManager.AppSettings["ReportSubcontdevProgress"] : "";

            return View();
        }

        public ActionResult DashboardProcurementProgress()
        {
            try
            {
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            ViewBag.ReportID = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportId"]) ? WebConfigurationManager.AppSettings["ReportId"] : "";
            ViewBag.ReportURL = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportProcurementProgress"]) ? WebConfigurationManager.AppSettings["ReportProcurementProgress"] : "";

            return View();
        }

        public ActionResult DashboardReminderComponentInspection()
        {
            try
            {
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            ViewBag.ReportID = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportId"]) ? WebConfigurationManager.AppSettings["ReportId"] : "";
            ViewBag.ReportURL = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportReminderComponentInspection"]) ? WebConfigurationManager.AppSettings["ReportReminderComponentInspection"] : "";

            return View();
        }

        public ActionResult DashboardItemArrivalStatus()
        {
            try
            {
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            ViewBag.ReportID = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportId"]) ? WebConfigurationManager.AppSettings["ReportId"] : "";
            ViewBag.ReportURL = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportItemArrivalStatus"]) ? WebConfigurationManager.AppSettings["ReportItemArrivalStatus"] : "";

            return View();
        }

        public ActionResult DashboardLeadTimeFromPR_PORelease()
        {
            try
            {
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            ViewBag.ReportID = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportId"]) ? WebConfigurationManager.AppSettings["ReportId"] : "";
            ViewBag.ReportURL = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportLeadTimeFromPR_PORelease"]) ? WebConfigurationManager.AppSettings["ReportLeadTimeFromPR_PORelease"] : "";

            return View();
        }

        public ActionResult DashboardPerformanceVendor()
        {
            try
            {
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            ViewBag.ReportID = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportId"]) ? WebConfigurationManager.AppSettings["ReportId"] : "";
            ViewBag.ReportURL = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportPerformanceVendor"]) ? WebConfigurationManager.AppSettings["ReportPerformanceVendor"] : "";

            return View();
        }

        //private static async Task<AuthenticationResult> getToken2()
        ////private string getToken2()
        //{

        //    string token = "";
        //    string userName = WebConfigurationManager.AppSettings["PowerbiUsername"];
        //    string password = WebConfigurationManager.AppSettings["PowerbiPassword"];
        //    string clientId = WebConfigurationManager.AppSettings["ClientID"];
        //    var credentials = new UserPasswordCredential(userName, password);
        //    var authenticationContext = new AuthenticationContext("https://login.windows.net/common/oauth2/authorize/");
        //    var result = await authenticationContext.AcquireTokenAsync("https://analysis.windows.net/powerbi/api", clientId, credentials);
        //    //token = authenticationContext.AcquireTokenAsync("https://analysis.windows.net/powerbi/api", clientId, credentials).Result.AccessToken;
        //    return result;
        //}

        public string getTokenOld()
        {
            string token = "";
            using (PowerShell ps = PowerShell.Create())
            {
                //string ClientID = WebConfigurationManager.AppSettings["ClientID"];
                string ClientID = WebConfigurationManager.AppSettings["ClientID"];
                //string AuthEndPoint = WebConfigurationManager.AppSettings["AuthEndPoint"];
                string AuthEndPoint = "https://login.microsoftonline.com/{0}/oauth2/nativeclient";
                string TenantId = WebConfigurationManager.AppSettings["TenantId"];

                //RedirectUri you used when you register your app.
                //For a client app, a redirect uri gives Azure AD more details on the application that it will authenticate.
                // You can use this redirect uri for your client app
                string redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";

                //Resource Uri for Power BI API
                string resourceUri = "https://analysis.windows.net/powerbi/api/";

                //Get access token:
                // To call a Power BI REST operation, create an instance of AuthenticationContext and call AcquireToken
                // AuthenticationContext is part of the Active Directory Authentication Library NuGet package
                // To install the Active Directory Authentication Library NuGet package in Visual Studio,
                //  run "Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory" from the nuget Package Manager Console.

                // AcquireToken will acquire an Azure access token
                // Call AcquireToken to get an Azure token from Azure Active Directory token issuance endpoint
                string authority = string.Format(CultureInfo.InvariantCulture, AuthEndPoint, TenantId);
                AuthenticationContext authContext = new AuthenticationContext(authority);
                var credential = new UserPasswordCredential(WebConfigurationManager.AppSettings["PowerbiUsername"], WebConfigurationManager.AppSettings["PowerbiPassword"]);
                //token = authContext.AcquireTokenAsync(resourceUri, ClientID, new Uri(redirectUri), new PlatformParameters(PromptBehavior.Auto)).Result.AccessToken;
                token = authContext.AcquireTokenAsync(resourceUri, ClientID, credential).Result.AccessToken;

            }
            return token;
        }

        public string getToken()
        {
            string token = string.Empty;
            var request = (HttpWebRequest)WebRequest.Create("https://login.microsoftonline.com/common/oauth2/token");
            var postData = "grant_type=" + Uri.EscapeDataString("password");
            postData += "&client_id=" + Uri.EscapeDataString(WebConfigurationManager.AppSettings["ClientID"]);
            postData += "&resource=" + Uri.EscapeDataString("https://analysis.windows.net/powerbi/api");
            postData += "&username=" + Uri.EscapeDataString(WebConfigurationManager.AppSettings["PowerbiUsername"]);
            postData += "&password=" + Uri.EscapeDataString(WebConfigurationManager.AppSettings["PowerbiPassword"]);
            var data = Encoding.ASCII.GetBytes(postData);
            
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            PowerBIModel arrToken = JsonConvert.DeserializeObject<PowerBIModel>(responseString);

            //return token.access_token;
            token = !string.IsNullOrEmpty(arrToken.access_token) ? arrToken.access_token : "";
            ViewBag.Message = token;
            return token;
        }
    }

    public class PowerBIModel
    {
        public string token_type { get; set; }
        public string scope { get; set; }
        public string expires_in { get; set; }
        public string ext_expires_in { get; set; }
        public string expires_on { get; set; }
        public string not_before { get; set; }
        public string resource { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }
}