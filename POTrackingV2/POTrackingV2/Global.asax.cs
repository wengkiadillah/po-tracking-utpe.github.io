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

                if (serializeModel.Roles.Any(x=>x.ToLower()== LoginConstants.RoleVendor.ToLower()))
                {
                    CustomPrincipal principal = new CustomPrincipal(authTicket.Name);

                    principal.UserName = serializeModel.UserName;
                    principal.Name = serializeModel.Name;
                    principal.Roles = serializeModel.Roles.FirstOrDefault();

                    HttpContext.Current.User = principal;
                }
                else
                {
                    //if (customRole.IsUserInApplicaiton(authTicket.Name, ApplicationConstants.POTracking))
                    //{
                    using (UserManagementEntities db = new UserManagementEntities())
                    {
                        CustomPrincipal principal = new CustomPrincipal(authTicket.Name);

                        principal.UserName = serializeModel.UserName;
                        principal.Name = serializeModel.Name;
                        //principal.Roles = serializeModel.Roles.FirstOrDefault();

                        principal.Roles = (from dbUser in db.Users
                                           join userRole in db.UserRoles on dbUser.Username equals userRole.Username 
                                           join role in db.Roles on userRole.RoleID equals role.ID
                                           join ap in db.Applications on role.ApplicationID equals ap.ID
                                           where ap.Name.ToLower() == ApplicationConstants.POTracking.ToLower() && userRole.Username== serializeModel.UserName
                                           select role.Name).FirstOrDefault();
                        HttpContext.Current.User = principal;
                    }
                    //}
                }
            }
            else
            {
                HttpContext.Current.User = null;
            }
        }

    }
}
