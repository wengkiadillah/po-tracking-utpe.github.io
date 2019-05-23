using PagedList;
using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POTrackingV2.Controllers
{


    public class RoleTypeController : Controller
    {
        List<RolesType> listRoleType = new List<RolesType>();
        List<UserRoleTypeProxy> listUserRoleType = new List<UserRoleTypeProxy>();
        List<UserProxy> listUser = new List<UserProxy>();



        // GET: RoleType
        public ActionResult Index(string search, int? page)
        {
            ViewBag.CurrentSearchString = search;
            using (POTrackingEntities db = new POTrackingEntities())
            {
                using (UserManagementEntities dbUser = new UserManagementEntities())
                {
                    List<string> listUsername = new List<string>();
                    listUsername = db.UserRoleTypes.Select(x => x.Username).ToList();
                    var userList = (from ur in dbUser.UserRoles
                                    join us in dbUser.Users on ur.Username equals us.Username
                                    join role in dbUser.Roles on ur.RoleID equals role.ID
                                    where role.ApplicationID == 3
                                    select new { Username = us.Username, Name = us.Name, RoleName = role.Name }).ToList();

                    foreach (var item in userList)
                    {
                        if(listUsername.Any(x => x.Contains(item.Username)))
                        listUserRoleType.Add(new UserRoleTypeProxy(item.Username, item.Name, item.RoleName));
                    }
                    if (search == null)
                    {
                        listUserRoleType = listUserRoleType.Where(x => search == null).ToList();
                    }
                    else
                    {
                        listUserRoleType = listUserRoleType.Where(x => x.Username.Contains(search) || x.Name.Contains(search)).ToList();
                    }
                    return View(listUserRoleType.ToPagedList(page ?? 1, 3)); 
                }
            }
        }

        // GET: RoleType/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RoleType/Create
        public ActionResult Create()
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                using (UserManagementEntities dbUser = new UserManagementEntities())
                {

                    var userList = (from ur in dbUser.UserRoles
                                    join us in dbUser.Users on ur.Username equals us.Username
                                    join role in dbUser.Roles on ur.RoleID equals role.ID
                                    where role.ApplicationID == 3
                                    select new { Username = us.Username, Name = us.Name, RoleName = role.Name }).ToList();

                    foreach (var item in userList)
                    {
                        listUser.Add(new UserProxy(item.Username, item.Name, item.RoleName));
                    }

                    listRoleType = db.RolesTypes.ToList();


                    ViewBag.Username = new SelectList(listUser, "Username", "UserProp");
                    ViewBag.RolesTypeID = new SelectList(listRoleType, "ID", "Name");

                    return View();
                }
            }
        }

        // POST: RoleType/Create
        [HttpPost]
        public ActionResult Create(UserRoleType objNewUser)
        {
            try
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    using (UserManagementEntities dbUser = new UserManagementEntities())
                    {

                        var userList = (from ur in dbUser.UserRoles
                                        join us in dbUser.Users on ur.Username equals us.Username
                                        join role in dbUser.Roles on ur.RoleID equals role.ID
                                        where role.ApplicationID == 3
                                        select new { Username = us.Username, Name = us.Name, RoleName = role.Name }).ToList();

                        foreach (var item in userList)
                        {
                            listUser.Add(new UserProxy(item.Username, item.Name, item.RoleName));
                        }

                        listRoleType = db.RolesTypes.ToList();


                        ViewBag.Username = new SelectList(listUser, "Username", "UserProp");
                        ViewBag.RolesTypeID = new SelectList(listRoleType, "ID", "Name");


                        var chkUser = (db.UserRoleTypes.FirstOrDefault(x => x.Username == objNewUser.Username));
                        if (chkUser == null)
                        {
                            objNewUser.Username = objNewUser.Username;
                            objNewUser.RolesTypeID = objNewUser.RolesTypeID;
                            objNewUser.Created = DateTime.Now;
                            objNewUser.CreatedBy = User.Identity.Name;
                            objNewUser.LastModified = DateTime.Now;
                            objNewUser.LastModifiedBy = User.Identity.Name;
                            db.UserRoleTypes.Add(objNewUser);
                            db.SaveChanges();
                            ModelState.Clear();
                            return RedirectToAction("Index", "RoleType");
                        }
                        ViewBag.ErrorMessage = "User Already Exist!";
                        return View();
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: RoleType/Edit/5
        public ActionResult Edit(int ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                using (UserManagementEntities dbUser = new UserManagementEntities())
                {
                    var selectedUserRoleType = db.UserRoleTypes.Find(ID);

                    var userList = (from ur in dbUser.UserRoles
                                    join us in dbUser.Users on ur.Username equals us.Username
                                    join role in dbUser.Roles on ur.RoleID equals role.ID
                                    where role.ApplicationID == 3
                                    select new { Username = us.Username, Name = us.Name, RoleName = role.Name }).ToList();

                    foreach (var item in userList)
                    {
                        listUser.Add(new UserProxy(item.Username, item.Name, item.RoleName));
                    }

                    listRoleType = db.RolesTypes.ToList();


                    ViewBag.Username = new SelectList(listUser, "Username", "UserProp",selectedUserRoleType.Username);
                    ViewBag.RolesTypeID = new SelectList(listRoleType, "ID", "Name", selectedUserRoleType.RolesTypeID);

                 
                    return View(selectedUserRoleType);
                }
            }
        }

        // POST: RoleType/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: RoleType/Delete/5
        public ActionResult Delete(int ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                var userRoleType = db.UserRoleTypes.Find(ID);
                db.UserRoleTypes.Remove(userRoleType);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        
    }
}
