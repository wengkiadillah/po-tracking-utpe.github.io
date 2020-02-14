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

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleVendor + "," + LoginConstants.RoleProcurement + "," + LoginConstants.RoleSubcontDev)]
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
                ViewBag.Token = getToken();
            }
            catch (Exception ex)
            {
                ViewBag.Token = "";
                ViewBag.Message = ex.Message + ex.StackTrace;
            }
            ViewBag.ReportID = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportId"]) ? WebConfigurationManager.AppSettings["ReportId"] : "asd";
            ViewBag.ReportSectionSubcontdevProgress = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportSectionSubcontdevProgress"]) ? WebConfigurationManager.AppSettings["ReportSectionSubcontdevProgress"] : "";

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
            ViewBag.ReportSectionProcurementProgress = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportSectionProcurementProgress"]) ? WebConfigurationManager.AppSettings["ReportSectionProcurementProgress"] : "";

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
            ViewBag.ReportSectionReminderComponentInspection = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportSectionReminderComponentInspection"]) ? WebConfigurationManager.AppSettings["ReportSectionReminderComponentInspection"] : "";

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
            ViewBag.ReportSectionItemArrivalStatus = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportSectionItemArrivalStatus"]) ? WebConfigurationManager.AppSettings["ReportSectionItemArrivalStatus"] : "";

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
            ViewBag.ReportSectionLeadTimeFromPR_PORelease = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportSectionLeadTimeFromPR_PORelease"]) ? WebConfigurationManager.AppSettings["ReportSectionLeadTimeFromPR_PORelease"] : "";

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
            ViewBag.ReportSectionPerformanceVendor = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ReportSectionPerformanceVendor"]) ? WebConfigurationManager.AppSettings["ReportSectionPerformanceVendor"] : "";

            return View();
        }


        public string getToken()
        {
            string token = "";
            using (PowerShell ps = PowerShell.Create())
            {
                string ClientID = WebConfigurationManager.AppSettings["ClientID"];
                string AuthEndPoint = WebConfigurationManager.AppSettings["AuthEndPoint"];
                string TenantId = WebConfigurationManager.AppSettings["TenantId"];

                //RedirectUri you used when you register your app.
                //For a client app, a redirect uri gives Azure AD more details on the application that it will authenticate.
                // You can use this redirect uri for your client app
                //string redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";

                //Resource Uri for Power BI API
                string resourceUri = "https://analysis.windows.net/powerbi/api";

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
                //token = authContext.AcquireTokenAsync(resourceUri, clientID, new Uri(redirectUri), new PlatformParameters(PromptBehavior.Auto)).Result.AccessToken;
                token = authContext.AcquireTokenAsync(resourceUri, ClientID, credential).Result.AccessToken;
                
            }
            return token;
        }
    }
}