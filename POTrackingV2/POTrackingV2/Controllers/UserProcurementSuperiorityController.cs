using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POTrackingV2.Controllers
{
    public class UserProcurementSuperiorityController : Controller
    {
        // GET: UserProcurementSuperiority
        public ActionResult Index()
        {
            return View();
        }

        // GET: UserProcurementSuperiority/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UserProcurementSuperiority/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserProcurementSuperiority/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: UserProcurementSuperiority/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UserProcurementSuperiority/Edit/5
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

        // GET: UserProcurementSuperiority/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserProcurementSuperiority/Delete/5
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
