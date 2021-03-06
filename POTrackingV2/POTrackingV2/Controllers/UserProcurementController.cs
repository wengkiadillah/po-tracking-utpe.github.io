﻿using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using System;
using PagedList;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator)]
    public class UserProcurementController : Controller
    {
        private POTrackingEntities dbPOTracking = new POTrackingEntities();
        private UserManagementEntities dbUserManagement = new UserManagementEntities();
        private DateTime now = DateTime.Now;

        public ActionResult Index(string searchUser, int? page)
        {
            List<UserProcurementSuperior> userProcurementSuperiors = dbPOTracking.UserProcurementSuperiors.ToList();

            if (!string.IsNullOrEmpty(searchUser))
            {
                userProcurementSuperiors = userProcurementSuperiors.Where(x => x.Username.ToLower().Contains(searchUser)).ToList();
            }

            ViewBag.CurrentSearchUser = searchUser;
            //return View(userProcurementSuperiors.OrderBy(x => x.Username));
            return View(userProcurementSuperiors.OrderBy(x => x.Username).ToList().ToPagedList(page ?? 1, 10));
        }

        public ActionResult Details(int id)
        {
            UserProcurementSuperior superiorUser = dbPOTracking.UserProcurementSuperiors.Find(id);
            List<UserProcurementSuperior> inferiorUsers = dbPOTracking.UserProcurementSuperiors.Where(x => x.ParentID == id).OrderBy(x => x.Username).ToList();

            var ViewModel = new UserProcurementViewModelDetails
            {
                UserProcurementSuperior = superiorUser,
                UserProcurementInferiors = inferiorUsers
            };

            return View(ViewModel);
        }

        public ActionResult Create()
        {
            IQueryable<User> users = dbUserManagement.Users.Where(x => x.UserRoles.Any(y => y.Role.Application.Name.ToLower() == ApplicationConstants.POTracking.ToLower())).OrderBy(x => x.Username);
            IQueryable<User> inferiorUsers = users;

            foreach (var item in dbPOTracking.UserProcurementSuperiors)
            {
                users = users.Where(x => x.Username.ToLower() != item.Username.ToLower());
            }

            var ViewModel = new UserProcurementViewModelCreate
            {
                SuperiorUsernames = new SelectList(users.OrderBy(x => x.Name), "Username", "Name"),
                InferiorUsernames = new SelectList(inferiorUsers, "Username", "Name")
            };

            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult CreateSuperiorUser(string username, List<string> inferiorUsernames)
        {
            try
            {
                List<UserProcurementSuperior> databaseUserProcurementSuperiors = dbPOTracking.UserProcurementSuperiors.ToList();

                string description = GetNRPByUsername(username.ToLower());
                string fullName = GetFullNameByUsername(username.ToLower());
                string email = GetEmailByUsername(username.ToLower());

                UserProcurementSuperior userProcurementSuperior = new UserProcurementSuperior();
                userProcurementSuperior.Username = username;
                userProcurementSuperior.NRP = description;
                userProcurementSuperior.FullName = fullName;
                userProcurementSuperior.Email = email;
                userProcurementSuperior.Created = now;
                userProcurementSuperior.CreatedBy = User.Identity.Name;
                userProcurementSuperior.LastModified = now;
                userProcurementSuperior.LastModifiedBy = User.Identity.Name;

                dbPOTracking.UserProcurementSuperiors.Add(userProcurementSuperior);

                dbPOTracking.SaveChanges();

                int id = userProcurementSuperior.ID;

                foreach (var inferiorUsername in inferiorUsernames)
                {
                    UserProcurementSuperior userProcurementInferior = new UserProcurementSuperior();

                    if (databaseUserProcurementSuperiors.Any(x => x.Username.ToLower() == inferiorUsername.ToLower()))
                    {
                        userProcurementInferior = databaseUserProcurementSuperiors.Where(x => x.Username.ToLower() == inferiorUsername.ToLower()).SingleOrDefault();

                        userProcurementInferior.ParentID = id;
                        userProcurementInferior.LastModified = now;
                        userProcurementInferior.LastModifiedBy = User.Identity.Name;
                    }
                    else
                    {
                        description = GetNRPByUsername(inferiorUsername.ToLower());
                        fullName = GetFullNameByUsername(inferiorUsername.ToLower());
                        email = GetEmailByUsername(inferiorUsername.ToLower());

                        userProcurementInferior.ParentID = id;
                        userProcurementInferior.Username = inferiorUsername;
                        userProcurementInferior.NRP = description;
                        userProcurementInferior.FullName = fullName;
                        userProcurementInferior.Email = email;
                        userProcurementInferior.Created = now;
                        userProcurementInferior.CreatedBy = User.Identity.Name;
                        userProcurementInferior.LastModified = now;
                        userProcurementInferior.LastModifiedBy = User.Identity.Name;

                        dbPOTracking.UserProcurementSuperiors.Add(userProcurementInferior);
                    }
                }

                dbPOTracking.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddInferiorUser(int userSuperiorID, List<string> inferiorUsernames)
        {
            try
            {
                List<UserProcurementSuperior> databaseUserProcurementSuperiors = dbPOTracking.UserProcurementSuperiors.ToList();

                foreach (var inferiorUsername in inferiorUsernames)
                {
                    UserProcurementSuperior userProcurementInferior = new UserProcurementSuperior();

                    if (databaseUserProcurementSuperiors.Any(x => x.Username.ToLower() == inferiorUsername.ToLower()))
                    {
                        userProcurementInferior = databaseUserProcurementSuperiors.Where(x => x.Username.ToLower() == inferiorUsername.ToLower()).SingleOrDefault();

                        userProcurementInferior.ParentID = userSuperiorID;
                        userProcurementInferior.LastModified = now;
                        userProcurementInferior.LastModifiedBy = User.Identity.Name;
                    }
                    else
                    {
                        string description = GetNRPByUsername(inferiorUsername.ToLower());
                        string fullName = GetFullNameByUsername(inferiorUsername.ToLower());
                        string email = GetEmailByUsername(inferiorUsername.ToLower());

                        userProcurementInferior.ParentID = userSuperiorID;
                        userProcurementInferior.Username = inferiorUsername;
                        userProcurementInferior.NRP = description;
                        userProcurementInferior.FullName = fullName;
                        userProcurementInferior.Email = email;
                        userProcurementInferior.Created = now;
                        userProcurementInferior.CreatedBy = User.Identity.Name;
                        userProcurementInferior.LastModified = now;
                        userProcurementInferior.LastModifiedBy = User.Identity.Name;

                        dbPOTracking.UserProcurementSuperiors.Add(userProcurementInferior);
                    }
                }

                dbPOTracking.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateUser(string username)
        {
            List<User> users = dbUserManagement.Users.Where(x => x.UserRoles.Any(y => y.Role.Application.Name.ToLower() == ApplicationConstants.POTracking.ToLower()) && x.Username.ToLower() != username.ToLower()).OrderBy(x => x.Username).ToList();

            List<UserProcurementSuperior> userProcurementInferiors = dbPOTracking.UserProcurementSuperiors.Where(x => x.ParentID != null).ToList();

            foreach (var userProcurementInferior in userProcurementInferiors)
            {
                users = users.Where(x => x.Username.ToLower() != userProcurementInferior.Username.ToLower()).ToList();
            }

            SelectList selectListusers = new SelectList(users.OrderBy(x => x.Name), "Username", "Name");

            return Json(new { success = true, selectListusers }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteSuperiorUser(int userSuperiorID)
        {
            try
            {
                UserProcurementSuperior userProcurementSuperior = dbPOTracking.UserProcurementSuperiors.Find(userSuperiorID);
                List<UserProcurementSuperior> userProcurementInferiors = dbPOTracking.UserProcurementSuperiors.Where(x => x.ParentID == userSuperiorID).ToList();

                if (userProcurementInferiors.Count > 0)
                {
                    foreach (var userProcurementInferior in userProcurementInferiors)
                    {
                        userProcurementInferior.ParentID = null;
                    }
                }

                dbPOTracking.UserProcurementSuperiors.Remove(userProcurementSuperior);

                dbPOTracking.SaveChanges();

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteInferiorUser(int userInferiorID)
        {
            try
            {
                UserProcurementSuperior userProcurementInferior = dbPOTracking.UserProcurementSuperiors.Find(userInferiorID);

                if (userProcurementInferior != null)
                {
                    dbPOTracking.UserProcurementSuperiors.Remove(userProcurementInferior);

                    dbPOTracking.SaveChanges();

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditUserSuperior(int userSuperiorID, string inputEditNRP, string inputEditFullName, string inputEditEmail)
        {
            try
            {
                UserProcurementSuperior userProcurementSuperior = dbPOTracking.UserProcurementSuperiors.Find(userSuperiorID);

                if (userProcurementSuperior != null)
                {
                    userProcurementSuperior.NRP = inputEditNRP;
                    userProcurementSuperior.FullName = inputEditFullName;
                    userProcurementSuperior.Email = inputEditEmail;
                    userProcurementSuperior.LastModified = now;
                    userProcurementSuperior.LastModifiedBy = User.Identity.Name;

                    dbPOTracking.SaveChanges();

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public string GetNRPByUsername(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                SearchResult sResultSet;

                string domain = WebConfigurationManager.AppSettings["ActiveDirectoryUrl"];
                string ldapUser = WebConfigurationManager.AppSettings["ADUsername"];
                string ldapPassword = WebConfigurationManager.AppSettings["ADPassword"];
                using (DirectoryEntry entry = new DirectoryEntry(domain, ldapUser, ldapPassword))
                {
                    DirectorySearcher dSearch = new DirectorySearcher(entry);
                    dSearch.Filter = "(&(objectClass=user)(samaccountname=" + username + "))";
                    sResultSet = dSearch.FindOne();
                }

                try
                {
                    string description = sResultSet.Properties["description"][0].ToString();
                    return description;
                }
                catch (Exception)
                {
                    return "-";
                }
            }

            return null;
        }

        private string GetFullNameByUsername(string username)
        {
            string fullname = dbUserManagement.Users.Where(x => x.UserRoles.Any(y => y.Role.Application.Name.ToLower() == ApplicationConstants.POTracking.ToLower()) && x.Username.ToLower() == username.ToLower()).FirstOrDefault().Name;

            return fullname;
        }

        private string GetEmailByUsername(string username)
        {
            string email = dbUserManagement.Users.Where(x => x.UserRoles.Any(y => y.Role.Application.Name.ToLower() == ApplicationConstants.POTracking.ToLower()) && x.Username.ToLower() == username.ToLower()).FirstOrDefault().Email;

            return email;
        }
    }
}
