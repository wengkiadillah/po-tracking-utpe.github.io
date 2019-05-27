using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POTrackingV2.Controllers
{
    public class ProcurementVendorRelationshipController : Controller
    {

        private POTrackingEntities dbPOTracking = new POTrackingEntities();
        private UserManagementEntities dbUserManagementEntities = new UserManagementEntities();
        public DateTime now = DateTime.Now;

        // GET: ProcurementVendorRelationship
        public ActionResult Index()
        {
            List<ProcurementVendorRelationship> procurementVendorRelationships = dbPOTracking.ProcurementVendorRelationships.ToList();


            return View(procurementVendorRelationships);
        }

        // GET: ProcurementVendorRelationship/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProcurementVendorRelationship/Create
        public ActionResult Create()
        {

            List<UserRole> userRoles = dbUserManagementEntities.UserRoles.Where(x => x.Role.ApplicationID == 3).ToList();
            List<User> users = dbUserManagementEntities.Users.ToList();

            foreach (var user in users)
            {
                foreach (var userRole in userRoles)
                {
                    List<UserRole> spesificOneUserRoles
                }
            }

            ProcurementVendorRelationshipCreateViewModel ViewModel = new ProcurementVendorRelationshipCreateViewModel
            {
                selectListProcurementUsernames = new SelectList(users, "Username", "Name"),
                selectListVendors = new SelectList(dbPOTracking.Vendors.OrderBy(x => x.Name), "Code", "Name")
            };

            return View(ViewModel);
        }

        // POST: ProcurementVendorRelationship/Create
        [HttpPost]
        public ActionResult Create(ProcurementVendorRelationshipCreateViewModel procurementVendorRelationshipCreateViewModel)
        {
            try
            {
                dbPOTracking.ProcurementVendorRelationships.Add(procurementVendorRelationshipCreateViewModel.procurementVendorRelationship);
                dbPOTracking.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ProcurementVendorRelationship/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProcurementVendorRelationship/Edit/5
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

        // GET: ProcurementVendorRelationship/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProcurementVendorRelationship/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
