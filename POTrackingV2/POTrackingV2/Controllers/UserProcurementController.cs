using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using System;
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
        public DateTime now = DateTime.Now;

        public ActionResult Index(string searchUser)
        {
            List<UserProcurementSuperior> userProcurementSuperiors = dbPOTracking.UserProcurementSuperiors.ToList();

            if (!string.IsNullOrEmpty(searchUser))
            {
                userProcurementSuperiors = userProcurementSuperiors.Where(x => x.Username.ToLower().Contains(searchUser)).ToList();
            }

            ViewBag.CurrentSearchUser = searchUser;

            return View(userProcurementSuperiors.OrderBy(x => x.Username));
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
                users = users.Where(x => x.Username != item.Username);
            }

            var ViewModel = new UserProcurementViewModelCreate
            {
                SuperiorUsernames = new SelectList(users, "Username", "Name"),
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

                string description = GetNRPByUsername(username);

                UserProcurementSuperior userProcurementSuperior = new UserProcurementSuperior();
                userProcurementSuperior.Username = username;
                userProcurementSuperior.NRP = description;
                userProcurementSuperior.Created = now;
                userProcurementSuperior.CreatedBy = User.Identity.Name;
                userProcurementSuperior.LastModified = now;
                userProcurementSuperior.LastModifiedBy = User.Identity.Name;

                dbPOTracking.UserProcurementSuperiors.Add(userProcurementSuperior);

                dbPOTracking.SaveChanges();

                int id = userProcurementSuperior.ID;

                foreach (var item in inferiorUsernames)
                {
                    UserProcurementSuperior userProcurementInferior = new UserProcurementSuperior();

                    if (databaseUserProcurementSuperiors.Any(x => x.Username == item))
                    {
                        userProcurementInferior = databaseUserProcurementSuperiors.Where(x => x.Username == item).SingleOrDefault();

                        userProcurementInferior.ParentID = id;
                        userProcurementInferior.LastModified = now;
                        userProcurementInferior.LastModifiedBy = User.Identity.Name;
                    }
                    else
                    {
                        description = GetNRPByUsername(item);

                        userProcurementInferior.ParentID = id;
                        userProcurementInferior.Username = item;
                        userProcurementInferior.NRP = description;
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

                foreach (var item in inferiorUsernames)
                {
                    UserProcurementSuperior userProcurementInferior = new UserProcurementSuperior();

                    if (databaseUserProcurementSuperiors.Any(x => x.Username == item))
                    {
                        userProcurementInferior = databaseUserProcurementSuperiors.Where(x => x.Username == item).SingleOrDefault();

                        userProcurementInferior.ParentID = userSuperiorID;
                        userProcurementInferior.LastModified = now;
                        userProcurementInferior.LastModifiedBy = User.Identity.Name;
                    }
                    else
                    {
                        string description = GetNRPByUsername(item);

                        userProcurementInferior.ParentID = userSuperiorID;
                        userProcurementInferior.Username = item;
                        userProcurementInferior.NRP = description;
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
            List<User> users = dbUserManagement.Users.Where(x => x.UserRoles.Any(y => y.Role.Application.Name.ToLower() == ApplicationConstants.POTracking.ToLower()) && x.Username != username).OrderBy(x => x.Username).ToList();

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
                    foreach (var item in userProcurementInferiors)
                    {
                        dbPOTracking.UserProcurementSuperiors.Remove(item);
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

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
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
    }
}
