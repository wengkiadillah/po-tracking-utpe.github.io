﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;

namespace POTracking.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator)]
    public class ReasonSubContController : Controller
    {
        // GET: ReasonSubCont
        public ActionResult Index(string search, int? page)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                //return View(db.SequencesProgressReasons.ToList());
                return View(db.SequencesProgressReasons.Where(x => x.Name.Contains(search) || search == null).OrderBy(x=> x.Name).ToList().ToPagedList(page ?? 1, 10));
            }
                
        }

        // GET: ReasonSubCont/Details/5
        public ActionResult Details(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.SequencesProgressReasons.Where(x => x.ID == id).FirstOrDefault());
            }
                
        }

        // GET: ReasonSubCont/getById/5
        //[HttpGet]
        //public JsonResult getById(int id) {
        //    if (id == null)
        //    {
        //        return Json(new { status = false}, JsonRequestBehavior.AllowGet);
        //    }

        //    using (POTrackingEntities db = new POTrackingEntities())
        //    {
        //        var result = db.SequencesProgressReasons.Where(z => z.ID == id).FirstOrDefault();
        //        return Json(new { result.PurchasingDocumentItems1}, JsonRequestBehavior.AllowGet);
        //    }
        //}

        // GET: ReasonSubCont/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReasonSubCont/Create
        [HttpPost]
        public ActionResult Create(SequencesProgressReason sequencesProgress)
        {
            try
            {
                // TODO: Add insert logic here
                string userName = User.Identity.Name;
                DateTime now = DateTime.Now;
                using (POTrackingEntities db = new POTrackingEntities())
                {

                    SequencesProgressReason sequencesProgressReason = new SequencesProgressReason
                    {

                        Name = sequencesProgress.Name,
                        Created = now,
                        CreatedBy = userName,
                        Modified = now,
                        ModifiedBy = userName
                    };
                    db.SequencesProgressReasons.Add(sequencesProgressReason);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
                //return Json(new { status = true, data = sequencesProgress.Name });
            }
            catch
            {

                return View();
                //DateTime now = DateTime.Now;
                //string userName = User.Identity.Name;
                //return Json(new { status = false, textMessage = ex.Message, data = sequencesProgress, username = userName, now = now });
            }   
        }

        // GET: ReasonSubCont/Edit/5
        public ActionResult Edit(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.SequencesProgressReasons.Where(x => x.ID == id).FirstOrDefault());
            }
        }

        // POST: ReasonSubCont/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, SequencesProgressReason sequencesProgress)
        {
            DateTime now = DateTime.Now;
            var userName = User.Identity.Name;
            try
            {
                //// TODO: Add update logic here
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    SequencesProgressReason selectedSequenceProgress = db.SequencesProgressReasons.SingleOrDefault(x => x.ID == id);
                    selectedSequenceProgress.Name = sequencesProgress.Name;
                    selectedSequenceProgress.Modified = now;
                    selectedSequenceProgress.ModifiedBy = userName;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // POST: ReasonSubCont/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                var reasonSubCont = db.SequencesProgressReasons.Find(id);
                db.SequencesProgressReasons.Remove(reasonSubCont);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }
    }
}
