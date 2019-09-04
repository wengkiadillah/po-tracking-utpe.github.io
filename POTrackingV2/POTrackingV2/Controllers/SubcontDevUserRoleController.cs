using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using POTrackingV2.Constants;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using POTrackingV2.CustomAuthentication;
using Newtonsoft.Json;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator)]
    public class SubcontDevUserRoleController : Controller
    {
        List<SubcontDevRole> listSubcontDevRole = new List<SubcontDevRole>();
        List<SubcontDevUserRole> listSubcontDevUserRole = new List<SubcontDevUserRole>();

        [HttpGet]
        public JsonResult GetUserFromValue(string value)
        {
            UserManagementEntities dbUserManagement = new UserManagementEntities();
            try
            {
                object data = null;
                value = value.ToLower();

                data = dbUserManagement.Users.Where(x => x.Username.Contains(value)).Select(x =>
                    new
                    {
                        Data = x.Username,
                        MatchEvaluation = x.Username.ToLower().IndexOf(value)
                    }).Distinct().OrderBy(x => x.MatchEvaluation).Take(10);

                if (data != null)
                {
                    return Json(new { success = true, responseCode = "200", data = JsonConvert.SerializeObject(data) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseCode = "404", responseText = "Not Found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseCode = "500", responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CreateSubcontDevUserRole()
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                listSubcontDevRole = db.SubcontDevRoles.ToList();
                //listSubcontDevUserRole = db.SubcontDevUserRoles.ToList();
                ViewBag.RolesTypeID = new SelectList(listSubcontDevRole, "ID", "Name");
                //ViewBag.VendorCode = new SelectList(listSubcontDevUserRole, "Code", "CodeName");
                return View();
            }
        }

        [HttpPost]
        public ActionResult CreateSubcontDevUserRole(SubcontDevUserRole objNewUser)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                listSubcontDevRole = db.SubcontDevRoles.ToList();
                
                ViewBag.RolesTypeID = new SelectList(listSubcontDevRole, "ID", "Name");
                var chkUserRole = (db.SubcontDevUserRoles.FirstOrDefault(x => x.Username == objNewUser.Username));
                if (chkUserRole == null)
                {
                    UserManagementEntities dbUserManagement = new UserManagementEntities();
                    var checkUsername = dbUserManagement.Users.FirstOrDefault(x => x.Username == objNewUser.Username);
                    if (checkUsername != null)
                    {
                        objNewUser.Username = objNewUser.Username;
                        objNewUser.RoleID = objNewUser.RolesTypeID;
                        objNewUser.Created = DateTime.Now;
                        objNewUser.CreatedBy = User.Identity.Name;
                        objNewUser.Modified = DateTime.Now;
                        objNewUser.ModifiedBy = User.Identity.Name;
                        db.SubcontDevUserRoles.Add(objNewUser);

                        db.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("ViewSubcontDevUserRole", "SubcontDevUserRole");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "User does not Exist!";
                        return View();
                    }

                }
                ViewBag.ErrorMessage = "User Already Exist!";
                return View();
            }
        }

        public ActionResult ViewSubcontDevUserRole(string search, int? page)
        {
            ViewBag.CurrentSearchString = search;
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.SubcontDevUserRoles.Where(x => x.Username.Contains(search) || x.SubcontDevRole.Name.Contains(search) || search == null).OrderBy(x => x.Username).ToList().ToPagedList(page ?? 1, 10));
            }
        }

        [HttpGet]
        public ActionResult EditSubcontDevUserRole(int ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                listSubcontDevRole = db.SubcontDevRoles.ToList();
                listSubcontDevUserRole = db.SubcontDevUserRoles.ToList();
                var selectedUserRole = db.SubcontDevUserRoles.Find(ID);
                ViewBag.RolesTypeID = new SelectList(listSubcontDevRole, "ID", "Name", selectedUserRole.RoleID);
                return View(selectedUserRole);
            }
        }

        [HttpPost]
        public ActionResult EditSubcontDevUserRole(int ID, SubcontDevUserRole objEditUser)
        {
            try
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    listSubcontDevRole = db.SubcontDevRoles.ToList();
                    listSubcontDevUserRole = db.SubcontDevUserRoles.ToList();
                    var selectedUserRole = db.SubcontDevUserRoles.Find(ID);

                    ViewBag.RolesTypeID = new SelectList(listSubcontDevRole, "ID", "Name", selectedUserRole.RoleID);

                    var chkUser = (db.SubcontDevUserRoles.FirstOrDefault(x => x.Username == objEditUser.Username && x.ID != ID));
                    if (chkUser == null)
                    {
                        //selectedUserRole.Username = objEditUser.Username;
                        selectedUserRole.RoleID = objEditUser.RolesTypeID;
                        selectedUserRole.IsHead = objEditUser.IsHead;
                        //selectedUserRole.RolesTypeID = objEditUser.RolesTypeID;
                        selectedUserRole.Modified = DateTime.Now;
                        selectedUserRole.ModifiedBy = User.Identity.Name;

                        db.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("ViewSubcontDevUserRole", "SubcontDevUserRole");
                    }
                    ViewBag.ErrorMessage = "User Already Exist!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        public ActionResult DeleteSubcontDevUserRole(int ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                var userRole = db.SubcontDevUserRoles.Find(ID);
                db.SubcontDevUserRoles.Remove(userRole);
                db.SaveChanges();

                return RedirectToAction("ViewSubcontDevUserRole");
            }
        }
    }
}
