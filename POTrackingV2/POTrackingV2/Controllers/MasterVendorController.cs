using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using POTrackingV2.Models;
using POTrackingV2.ViewModels;

namespace POTracking.Controllers
{
    public class MasterVendorController : Controller
    {
        private POTrackingEntities db = new POTrackingEntities();

        // GET: MasterVendor
        public ActionResult Index(string searchBy, string search, int? page)
        {
            ViewBag.CurrentSearchFilterBy = searchBy;
            ViewBag.CurrentSearchString = search;
            using (POTrackingEntities db = new POTrackingEntities())
            {
                if (searchBy == "description")
                {
                    return View(db.SubcontComponentCapabilities.Where(x => x.Description == search || search == null).ToList().ToPagedList(page ?? 1, 10));
                }
                else
                {
                    return View(db.SubcontComponentCapabilities.Where(x => x.VendorCode.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 10));
                }
                
            }
                
        }

        //public string index(IPagedList<POTracking.Models.SubcontComponentCapability> subcontComponents)
        //{

        //}

        // GET: MasterVendor/Details/5
        public ActionResult Details(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.SubcontComponentCapabilities.Where(x => x.ID == id).FirstOrDefault());
            }

        }

        // GET: MasterVendor/Create
        public ActionResult Create()
        {
            POTrackingEntities db = new POTrackingEntities();
            var ViewModel = new MasterVendorViewModel
            {
                ListName = new SelectList(db.Vendors.OrderBy(x => x.Code), "Code", "Name")
            };
            return View(ViewModel);
        }

        //public ActionResult CreateDropdown()
        //{
        //    POTrackingEntities db = new POTrackingEntities();
        //    var getVendorList = db.Vendors.ToList();
        //    SelectList list = new SelectList(getVendorList, "Vendor", "Name");
        //    ViewBag.VendorListName = list;
        //    return View();
        //}

        // POST: MasterVendor/Create
        [HttpPost]
        public ActionResult Create(MasterVendorViewModel masterVendorViewModel)
        {
            try
            {
                string userName = User.Identity.Name;
                DateTime now = DateTime.Now;
                string VendorCode = masterVendorViewModel.SelectedName;
                decimal dailyCapacity = masterVendorViewModel.subCont.MonthlyCapacity / 22;
                // TODO: Add insert logic here
                using (POTrackingEntities db = new POTrackingEntities())
                {

                    SubcontComponentCapability subcontComponentCapability = new SubcontComponentCapability
                    {
                        VendorCode = VendorCode,
                        Material = masterVendorViewModel.subCont.Material,
                        Description = masterVendorViewModel.subCont.Description,
                        DailyLeadTime = masterVendorViewModel.subCont.DailyLeadTime,
                        MonthlyLeadTime = masterVendorViewModel.subCont.MonthlyLeadTime,
                        PB = masterVendorViewModel.subCont.PB,
                        Setting = masterVendorViewModel.subCont.Setting,
                        Fullweld = masterVendorViewModel.subCont.Fullweld,
                        Primer = masterVendorViewModel.subCont.Primer,
                        MonthlyCapacity = masterVendorViewModel.subCont.MonthlyCapacity,
                        DailyCapacity = dailyCapacity,
                        CreatedBy = userName,
                        Created = now,
                        LastModified = now,
                        LastModifiedBy = userName

                    };


                    db.SubcontComponentCapabilities.Add(subcontComponentCapability);
                    db.SaveChanges();
                }
                
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MasterVendor/Edit/5
        public ActionResult Edit(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.SubcontComponentCapabilities.Where(x => x.ID == id).FirstOrDefault());
            }
        }

        // POST: MasterVendor/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, SubcontComponentCapability subcontComponent)
        {
            DateTime now = DateTime.Now;
            var userName = User.Identity.Name;
            try
            {
                // TODO: Add update logic here
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    SubcontComponentCapability selectedSubContComponent = db.SubcontComponentCapabilities.SingleOrDefault(x => x.ID == id);
                    selectedSubContComponent.isNeedSequence = subcontComponent.isNeedSequence;
                    selectedSubContComponent.Material = subcontComponent.Material;
                    selectedSubContComponent.Description = subcontComponent.Description;
                    selectedSubContComponent.DailyLeadTime = subcontComponent.DailyLeadTime;
                    selectedSubContComponent.MonthlyLeadTime = subcontComponent.MonthlyLeadTime;
                    selectedSubContComponent.PB = subcontComponent.PB;
                    selectedSubContComponent.Setting = subcontComponent.Setting;
                    selectedSubContComponent.Fullweld = subcontComponent.Fullweld;
                    selectedSubContComponent.Primer = subcontComponent.Primer;
                    selectedSubContComponent.MonthlyCapacity = subcontComponent.MonthlyCapacity;
                    db.SaveChanges();
                }
                    return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MasterVendor/Delete/5
        public ActionResult Delete(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.SubcontComponentCapabilities.Where(x => x.ID == id).FirstOrDefault());
            }
        }

        // POST: MasterVendor/Delete/5
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

        //[HttpGet]
        //public ActionResult getById(string Code, MasterVendorViewModel masterVendorView) {

        //    try
        //    {
        //        Vendor vendor = db.Vendors.Find("100050");
        //        //MasterVendorViewModel dataMasterVendorViewModel = db.
        //        return Json(new { responseText = masterVendorView }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        string errorMessage = (ex.Message + ex.StackTrace);
        //        return View(errorMessage);
        //    }
        //}

        [HttpGet]
        public ActionResult getById(string code)
        {
            var test = (from x in db.Vendors
                        where x.Code == code
                        select x).FirstOrDefault();

            Vendor vendor = db.Vendors.Where(x => x.Code == code).FirstOrDefault();


            return Json(new { vendorCode = vendor.Code }, JsonRequestBehavior.AllowGet);
        }
    }
}
