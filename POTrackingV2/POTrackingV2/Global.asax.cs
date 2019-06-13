using Newtonsoft.Json;
using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace POTrackingV2
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies["Cookie1"];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                CustomRole customRole = new CustomRole();

                var serializeModel = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);

                //if (serializeModel.Roles.ToLower() == LoginConstants.RoleVendor.ToLower())
                //{
                //    CustomPrincipal principal = new CustomPrincipal(authTicket.Name);

                //    principal.UserName = serializeModel.UserName;
                //    principal.Name = serializeModel.Name;
                //    principal.Roles = serializeModel.Roles;

                //    HttpContext.Current.User = principal;
                //}
                //else
                //{
                    //if (customRole.IsUserInApplicaiton(authTicket.Name, ApplicationConstants.POTracking))
                    //{
                        CustomPrincipal principal = new CustomPrincipal(authTicket.Name);

                        principal.UserName = serializeModel.UserName;
                        principal.Name = serializeModel.Name;
                        principal.Roles = serializeModel.Roles.FirstOrDefault();

                        HttpContext.Current.User = principal;
                    //}
                //}              
            }
            else
            {
                HttpContext.Current.User = null;
            }
        }

    }
}
