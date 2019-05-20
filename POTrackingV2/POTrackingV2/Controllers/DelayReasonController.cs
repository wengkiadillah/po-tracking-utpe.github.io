using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using POTrackingV2.Models;

namespace POTracking.Controllers
{
    public class DelayReasonController : Controller
    {
        POTrackingEntities db = new POTrackingEntities();

        // GET: DelayReason
        public ActionResult Index()
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.DelayReasons.ToList());

            }
        }

        public JsonResult List()
        {
            return Json(db.DelayReasons.ToList());
        }

        public JsonResult Add(DelayReason delayReason)
        {
            return Json(db.DelayReasons.Add(delayReason), JsonRequestBehavior.AllowGet);
        }

        // GET: DelayReason/Details/5
        public ActionResult Details(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.DelayReasons.Where(x => x.ID == id).FirstOrDefault());
            }
                
        }

        // GET: DelayReason/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DelayReason/Create
        [HttpPost]
        public ActionResult Create(DelayReason delayReason)
        {
            try
            {
                // TODO: Add insert logic here
                string userName = User.Identity.Name;
                DateTime now = DateTime.Now;
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    DelayReason delay = new DelayReason
                    {
                        Name = delayReason.Name,
                        Created = now,
                        CreatedBy = userName,
                        LastModified = now,
                        LastModifiedBy = userName
                    };

                    db.DelayReasons.Add(delay);
                    db.SaveChanges();
                }
                    return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DelayReason/Edit/5
        public ActionResult Edit(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.DelayReasons.Where(x => x.ID == id).FirstOrDefault());
            }
        }

        // POST: DelayReason/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, DelayReason delayReason)
        {
            DateTime now = DateTime.Now;
            var userName = User.Identity.Name;
            try
            {
                // TODO: Add update logic here
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    DelayReason selectedDelayReason = db.DelayReasons.SingleOrDefault(x => x.ID == id);
                    selectedDelayReason.Name = delayReason.Name;
                    selectedDelayReason.LastModified = now;
                    selectedDelayReason.LastModifiedBy = userName;
                    db.SaveChanges();
                }
                    return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DelayReason/Delete/5
        public ActionResult Delete(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.DelayReasons.Where(x => x.ID == id).FirstOrDefault());
            }
        }

        // POST: DelayReason/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, DelayReason delayReason)
        {
            try
            {
                // TODO: Add delete logic here
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    delayReason = db.DelayReasons.Where(x => x.ID == id).FirstOrDefault();
                    db.DelayReasons.Remove(delayReason);
                    db.SaveChanges();
                }

                    return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
