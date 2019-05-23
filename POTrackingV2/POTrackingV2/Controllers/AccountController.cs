using Newtonsoft.Json;
using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;
using POTrackingV2.Constants;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace POTrackingV2.Controllers
{
    public class AccountController : Controller
    {
        POTrackingEntities db = new POTrackingEntities();

        // GET: Account
        //[Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(string ReturnUrl = "")
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginView loginView, string ReturnUrl = "")
        {
            if (ModelState.IsValid)
            {
                string domain = WebConfigurationManager.AppSettings["ActiveDirectoryUrl"];
                string ldapUser = loginView.UserName;// WebConfigurationManager.AppSettings["ADUsername"];
                string ldapPassword = loginView.Password;// WebConfigurationManager.AppSettings["ADPassword"];
                //bool isExternal = db.Users.Any(x => x.Username == loginView.UserName && x.Role.Name.ToLower() == LoginConstants.RoleVendor.ToLower() && x.IsActive == true);

                //if (isExternal)
                //{
                //    if (Membership.ValidateUser(loginView.UserName, loginView.Password))
                //    {
                //        var user = (CustomMembershipUser)Membership.GetUser(loginView.UserName, false);
                //        if (user != null)
                //        {
                //            CustomSerializeModel userModel = new Models.CustomSerializeModel()
                //            {
                //                Username = user.UserName,
                //                Name = user.Name,
                //                Roles = user.Roles
                //                //RolesType = user.RolesType,
                //                //VendorCode = user.VendorCode
                //            };

                //            string userData = JsonConvert.SerializeObject(userModel);
                //            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                //                (
                //                1, loginView.UserName, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData
                //                );

                //            string enTicket = FormsAuthentication.Encrypt(authTicket);

                //            DateTime now = DateTime.Now;
                //            HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                //            faCookie.Expires = now.AddMinutes(30);
                //            Response.Cookies.Add(faCookie);
                //        }
                //    }
                //    else
                //    {
                //        ModelState.AddModelError("", "Sorry your account not register yet in our system, please contact the administrator to register your account.");
                //        return View();
                //    }
                //}
                //else
                //{
                    using (DirectoryEntry entry = new DirectoryEntry(domain, ldapUser, ldapPassword))
                    {
                        try
                        {
                            if (entry.Guid == null)
                            {
                                ModelState.AddModelError("", "Username or Password invalid");
                                return View();
                            }
                            else
                            {
                                if (Membership.ValidateUser(loginView.UserName, loginView.Password))
                                {
                                    var user = (CustomMembershipUser)Membership.GetUser(loginView.UserName, false);
                                    if (user != null)
                                    {
                                        CustomSerializeModel userModel = new Models.CustomSerializeModel()
                                        {
                                            UserName = user.UserName,
                                            Name = user.Name,
                                            Roles = user.Roles
                                            //RolesType = user.RolesType,
                                            //VendorCode = user.VendorCode

                                        };

                                        string userData = JsonConvert.SerializeObject(userModel);
                                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                                            (
                                            1, loginView.UserName, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData
                                            );

                                        string enTicket = FormsAuthentication.Encrypt(authTicket);
                                        HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                                        Response.Cookies.Add(faCookie);
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("", "Sorry your account not register yet in our system, please contact the administrator to register your account.");
                                    return View();
                                }
                            }
                        }
                        catch
                        {
                            ModelState.AddModelError("", "Username or Password invalid");
                            return View();
                        }

                        if (!string.IsNullOrEmpty(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }

                //}

                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(loginView);
        }


        public ActionResult LoginExternal()
        {
            string loginURL = WebConfigurationManager.AppSettings["LoginURL"];
            return Redirect(loginURL);
        }

        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie("Cookie1", "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();

            var myrole = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);

            if (myrole.Roles.ToLower() == LoginConstants.RoleVendor.ToLower())
            {
                return RedirectToAction("Login", "Account", null);
            }
            else
            {
                string loginURL = WebConfigurationManager.AppSettings["LoginURL"];
                return Redirect(loginURL);
            }
        }
    }
}