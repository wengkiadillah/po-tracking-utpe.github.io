using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POTrackingV2.Controllers
{
    public class UserProcurementController : Controller
    {
        private POTrackingEntities dbPOTracking = new POTrackingEntities();
        private UserManagementEntities dbUserManagement = new UserManagementEntities();
        public DateTime now = DateTime.Now;

        public ActionResult Index()
        {
            List<UserProcurementSuperior> userProcurementSuperiors = dbPOTracking.UserProcurementSuperiors.ToList();

            return View(userProcurementSuperiors);
        }

        public ActionResult Details(int id)
        {
            UserProcurementSuperior superiorUser = dbPOTracking.UserProcurementSuperiors.Find(id);
            List<UserProcurementInferior> inferiorUsers = dbPOTracking.UserProcurementInferiors.Where(x => x.UserProcurementSuperiorID == id).ToList();

            var ViewModel = new UserProcurementViewModelDetails
            {
                UserProcurementSuperior = superiorUser,
                UserProcurementInferiors = inferiorUsers
            };

            return View(ViewModel);
        }

        public ActionResult Create()
        {
            IQueryable<User> users = dbUserManagement.Users.Where(x => x.UserRoles.Any(y => y.Role.ApplicationID == 3));
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
                UserProcurementSuperior userProcurementSuperior = new UserProcurementSuperior();
                userProcurementSuperior.Username = username;

                int id = dbPOTracking.UserProcurementSuperiors.Add(userProcurementSuperior).ID;

                foreach (var item in inferiorUsernames)
                {
                    UserProcurementInferior userProcurementInferior = new UserProcurementInferior();
                    userProcurementInferior.UserProcurementSuperiorID = id;
                    userProcurementInferior.Username = item;

                    dbPOTracking.UserProcurementInferiors.Add(userProcurementInferior);
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
                foreach (var item in inferiorUsernames)
                {
                    UserProcurementInferior userProcurementInferior = new UserProcurementInferior();
                    userProcurementInferior.UserProcurementSuperiorID = userSuperiorID;
                    userProcurementInferior.Username = item;

                    dbPOTracking.UserProcurementInferiors.Add(userProcurementInferior);
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
            List<User> users = dbUserManagement.Users.Where(x => x.UserRoles.Any(y => y.Role.ApplicationID == 3) && x.Username != username).ToList();

            UserProcurementSuperior userProcurementSuperior = dbPOTracking.UserProcurementSuperiors.Where(x => x.Username == username).SingleOrDefault();

            if (userProcurementSuperior != null)
            {
                List<UserProcurementInferior> userProcurementInferiors = dbPOTracking.UserProcurementInferiors.Where(x => x.UserProcurementSuperiorID == userProcurementSuperior.ID).ToList();

                foreach (var item in userProcurementInferiors)
                {
                    users = users.Where(x => x.Username != item.Username).ToList();
                }
            }

            SelectList selectListusers = new SelectList(users, "Username", "Name");

            return Json(new { success = true, selectListusers }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteSuperiorUser(int userSuperiorID)
        {
            try
            {
                UserProcurementSuperior userProcurementSuperior = dbPOTracking.UserProcurementSuperiors.Find(userSuperiorID);
                List<UserProcurementInferior> userProcurementInferiors = userProcurementSuperior.UserProcurementInferiors.ToList();

                if (userProcurementInferiors.Count > 0)
                {
                    foreach (var item in userProcurementInferiors)
                    {
                        dbPOTracking.UserProcurementInferiors.Remove(item);
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
                UserProcurementInferior userProcurementInferior = dbPOTracking.UserProcurementInferiors.Find(userInferiorID);

                if (userProcurementInferior != null)
                {
                    dbPOTracking.UserProcurementInferiors.Remove(userProcurementInferior);

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
    }
}
