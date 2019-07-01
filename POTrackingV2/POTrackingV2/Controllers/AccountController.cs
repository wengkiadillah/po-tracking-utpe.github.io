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
        string cookieName = WebConfigurationManager.AppSettings["CookieName"];
        int expired = Convert.ToInt32(WebConfigurationManager.AppSettings["SessionExpired"]);

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

                //using (DirectoryEntry entry = new DirectoryEntry(domain, ldapUser, ldapPassword))
                //{
                try
                {
                    //        if (entry.Guid == null)
                    //        {
                    //            ModelState.AddModelError("", "Username or Password invalid");
                    //            return View();
                    //        }
                    //        else
                    //        {
                    if (Membership.ValidateUser(ldapUser, ldapPassword))
                    {
                        var user = (CustomMembershipUser)Membership.GetUser(ldapUser, false);
                        if (user != null)
                        {
                            List<string> listRole = new List<string>();
                            listRole.Add(user.Roles);
                            CustomSerializeModel userModel = new Models.CustomSerializeModel()
                            {
                                UserName = user.UserName,
                                Name = user.Name,
                                Roles = listRole
                                //RolesType = user.RolesType,
                                //VendorCode = user.VendorCode

                            };

                            string userData = JsonConvert.SerializeObject(userModel);
                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                                (
                                1, loginView.UserName, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData
                                );

                            string enTicket = FormsAuthentication.Encrypt(authTicket);
                            HttpCookie faCookie = new HttpCookie(cookieName, enTicket);
                            faCookie.Expires = DateTime.Now.AddMinutes(expired);
                            Response.Cookies.Add(faCookie);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username or Password invalid.");
                        return View();
                    }
                    //}
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Username or Password invalid " + ex.Message);
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
                //}
            }
            return View(loginView);
        }

        [CustomAuthorize(Roles = LoginConstants.RoleVendor)]
        public ActionResult ChangePassword()
        {
            ChangePassword changePassword = new ChangePassword();
            changePassword.Username = User.Identity.Name;
            return View(changePassword);
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePassword changePassword)
        {
            try
            {
                // TODO: Add update logic here
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    UserVendor selectedUserVendor = db.UserVendors.SingleOrDefault(x => x.Username == changePassword.Username);
                    var newPassword = Helper.EncodePassword(changePassword.OldPassword, selectedUserVendor.Salt);

                    if (selectedUserVendor.Hash == newPassword)
                    {
                        var keyNew = Helper.GeneratePassword(10);
                        var password = Helper.EncodePassword(changePassword.NewPassword, keyNew);
                        selectedUserVendor.Salt = keyNew;
                        selectedUserVendor.Hash = password;
                        selectedUserVendor.LastModified = DateTime.Now.Date;
                        selectedUserVendor.LastModifiedBy = changePassword.Username;

                        db.SaveChanges();
                        db.SaveChanges();
                        return RedirectToAction("Index", "Home");
                    }
                    ViewBag.ErrorMessage = "Old Password is wrong";
                    return View();
                }
            }
            catch
            {
                return View();
            }
        }



        public ActionResult LoginInternal()
        {
            string loginURL = WebConfigurationManager.AppSettings["LoginURL"];
            return Redirect(loginURL);
        }

        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie(cookieName, "");
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