using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace POTrackingV2.CustomAuthentication
{
    /// <summary>
    /// Atribut otorisasi user untuk keamanan User yang diakses pada Controller
    /// </summary>
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Mengembalikan data User yang sedang menggnakan aplikasi/login
        /// </summary>
        protected virtual CustomPrincipal CurrentUser
        {
            get { return HttpContext.Current.User as CustomPrincipal; }
        }

        /// <summary>
        /// Method AuthorizeCore, method inti dari otorisasi terhadap User untuk pengecekan apakah sebuah Username termasuk ke dalam salah satu Role
        /// </summary>
        /// <param name="httpContext">Parameter httpContext yang didapat dari browser</param>
        /// <returns>Return 'True' jika sebuah Username termasuk ke dalam sebuah Role, 'False' jika tidak</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            return (CurrentUser == null) ? false : true; //(CurrentUser != null && !CurrentUser.IsInRole(Roles)) ||
        }

        /// <summary>
        /// Method HandleUnauthorizedRequest, method handling untuk mengembalikan Login Error
        /// </summary>
        /// <param name="filterContext">Parameter filterContext yang didapat dari browser</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            RedirectToRouteResult routeData = null;

            if (CurrentUser == null)
            //if (HttpContext.Current.User.Identity.Name == null)
            {
                routeData = new RedirectToRouteResult
                    (new System.Web.Routing.RouteValueDictionary
                    (new
                    {
                        //controller = "Error",
                        //action = "Unauthorized",
                        controller = "Account",
                        //action = "LoginExternal",
                        action = "Login",
                        ReturnUrl = filterContext.HttpContext.Request.Url
                    }
                    ));
            }
            else
            {
                routeData = new RedirectToRouteResult
                (new System.Web.Routing.RouteValueDictionary
                 (new
                 {
                     controller = "Error",
                     action = "AccessDenied"
                 }
                 ));
            }

            filterContext.Result = routeData;
        }

    }
}