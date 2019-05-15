﻿using PagedList;
using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POTrackingV2.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        private POTrackingEntities db = new POTrackingEntities();
        public DateTime now = DateTime.Now;

        // GET: Import
        public ActionResult Index(string searchData, string filterBy, string searchStartPODate, string searchEndPODate, int? page, string role)
        {
            var pOes = db.POes.Include(x => x.PurchasingDocumentItems)
                            .Where(x => x.Type.ToLower() == "zo04" || x.Type.ToLower() == "zo07" || x.Type.ToLower() == "zo08")
                            .Where(x => x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material)))
                            .OrderBy(x => x.Number)
                            .AsQueryable();

            //pOes = pOes.Except(pOes.Where(x => x.PurchasingDocumentItems.All(y => y.IsClosed.ToLower() == "x" || y.IsClosed.ToLower() == "l" || y.IsClosed.ToLower() == "lx") == true));

            if (role == "procurement")
            {
                pOes = pOes.Include(x => x.PurchasingDocumentItems)
                                .Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null))
                                .AsQueryable();
            }

            ViewBag.CurrentSearchFilterBy = filterBy;
            ViewBag.CurrentSearchData = searchData;
            ViewBag.CurrentStartPODate = searchStartPODate;
            ViewBag.CurrentEndPODate = searchEndPODate;
            ViewBag.CurrentRole = role;

            List<DelayReason> delayReasons = db.DelayReasons.ToList();

            ViewBag.DelayReasons = delayReasons;

            #region Filter

            if (!String.IsNullOrEmpty(searchData))
            {
                if (filterBy == "poNumber")
                {
                    pOes = pOes.Where(x => x.Number.Contains(searchData));
                }
                else if (filterBy == "vendor")
                {
                    pOes = pOes.Where(x => x.Vendor.Name.Contains(searchData));
                }
                else if (filterBy == "material")
                {
                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.Material.Contains(searchData) || y.Description.Contains(searchData)));
                }
            }

            if (!String.IsNullOrEmpty(searchStartPODate))
            {
                DateTime startDate = DateTime.ParseExact(searchStartPODate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                pOes = pOes.Where(x => x.Date >= startDate);
            }

            if (!String.IsNullOrEmpty(searchEndPODate))
            {
                DateTime endDate = DateTime.ParseExact(searchEndPODate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                pOes = pOes.Where(x => x.Date <= endDate);
            }
            #endregion

            return View(pOes.ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
        }

        #region STAGE 1

        [HttpPost]
        public ActionResult VendorConfirmItem(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            if (inputPurchasingDocumentItems == null)
            {
                return Json(new { responseText = $"No Item affected" }, JsonRequestBehavior.AllowGet);
            }

            DateTime now = DateTime.Now;
            int counter = 0;

            try
            {
                foreach (var inputPurchasingDocumentItem in inputPurchasingDocumentItems)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = new PurchasingDocumentItem();
                    if (!inputPurchasingDocumentItem.ParentID.HasValue)
                    {
                        databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ID).FirstOrDefault();

                        if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0")
                        {
                            databasePurchasingDocumentItem.ParentID = databasePurchasingDocumentItem.ID;
                            databasePurchasingDocumentItem.ConfirmedItem = null;
                            databasePurchasingDocumentItem.ConfirmedQuantity = inputPurchasingDocumentItem.ConfirmedQuantity;
                            databasePurchasingDocumentItem.ConfirmedDate = inputPurchasingDocumentItem.ConfirmedDate;
                            databasePurchasingDocumentItem.ActiveStage = "1";
                            databasePurchasingDocumentItem.LastModified = now;
                            databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                            counter++;

                            Notification notification = new Notification();
                            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                            notification.StatusID = 3;
                            notification.Stage = "1";
                            notification.Role = "procurement";
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);

                        }
                    }
                    else
                    {
                        databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ParentID).FirstOrDefault();

                        if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0")
                        {
                            inputPurchasingDocumentItem.POID = databasePurchasingDocumentItem.POID;
                            inputPurchasingDocumentItem.ItemNumber = databasePurchasingDocumentItem.ItemNumber;
                            inputPurchasingDocumentItem.Material = databasePurchasingDocumentItem.Material;
                            inputPurchasingDocumentItem.Description = databasePurchasingDocumentItem.Description;
                            inputPurchasingDocumentItem.NetPrice = databasePurchasingDocumentItem.NetPrice;
                            inputPurchasingDocumentItem.Currency = databasePurchasingDocumentItem.Currency;
                            inputPurchasingDocumentItem.Quantity = databasePurchasingDocumentItem.Quantity;
                            inputPurchasingDocumentItem.NetValue = databasePurchasingDocumentItem.NetValue;

                            inputPurchasingDocumentItem.ActiveStage = "1";
                            inputPurchasingDocumentItem.Created = now;
                            inputPurchasingDocumentItem.CreatedBy = User.Identity.Name;
                            inputPurchasingDocumentItem.LastModified = now;
                            inputPurchasingDocumentItem.LastModifiedBy = User.Identity.Name;

                            db.PurchasingDocumentItems.Add(inputPurchasingDocumentItem);
                            counter++;
                        }
                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{counter} Item succesfully affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult VendorEditItem(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            if (inputPurchasingDocumentItems == null)
            {
                return Json(new { responseText = $"No Item affected" }, JsonRequestBehavior.AllowGet);
            }

            DateTime now = DateTime.Now;
            int counter = 0;

            try
            {
                foreach (var inputPurchasingDocumentItem in inputPurchasingDocumentItems)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = new PurchasingDocumentItem();
                    List<PurchasingDocumentItem> childDatabasePurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == inputPurchasingDocumentItem.ID && x.ID != inputPurchasingDocumentItem.ID).ToList();

                    if (!childDatabasePurchasingDocumentItems.Any(x => x.ActiveStage != "1"))
                    {
                        if (!inputPurchasingDocumentItem.ParentID.HasValue)
                        {
                            // Child clean-up
                            foreach (var childDatabasePurchasingDocumentItem in childDatabasePurchasingDocumentItems)
                            {
                                if (childDatabasePurchasingDocumentItem.ID != inputPurchasingDocumentItem.ID)
                                {
                                    db.PurchasingDocumentItems.Remove(childDatabasePurchasingDocumentItem);
                                }
                            }
                            // finish

                            databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ID).FirstOrDefault();

                            if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0")
                            {
                                databasePurchasingDocumentItem.ParentID = databasePurchasingDocumentItem.ID;
                                databasePurchasingDocumentItem.ConfirmedItem = null;
                                databasePurchasingDocumentItem.ConfirmedQuantity = inputPurchasingDocumentItem.ConfirmedQuantity;
                                databasePurchasingDocumentItem.ConfirmedDate = inputPurchasingDocumentItem.ConfirmedDate;
                                databasePurchasingDocumentItem.ActiveStage = "1";
                                databasePurchasingDocumentItem.LastModified = now;
                                databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                                counter++;
                            }
                        }
                        else
                        {
                            databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ParentID).FirstOrDefault();

                            if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0")
                            {
                                inputPurchasingDocumentItem.POID = databasePurchasingDocumentItem.POID;
                                inputPurchasingDocumentItem.ItemNumber = databasePurchasingDocumentItem.ItemNumber;
                                inputPurchasingDocumentItem.Material = databasePurchasingDocumentItem.Material;
                                inputPurchasingDocumentItem.Description = databasePurchasingDocumentItem.Description;
                                inputPurchasingDocumentItem.NetPrice = databasePurchasingDocumentItem.NetPrice;
                                inputPurchasingDocumentItem.Currency = databasePurchasingDocumentItem.Currency;
                                inputPurchasingDocumentItem.Quantity = databasePurchasingDocumentItem.Quantity;
                                inputPurchasingDocumentItem.NetValue = databasePurchasingDocumentItem.NetValue;

                                inputPurchasingDocumentItem.ActiveStage = "1";
                                inputPurchasingDocumentItem.Created = now;
                                inputPurchasingDocumentItem.CreatedBy = User.Identity.Name;
                                inputPurchasingDocumentItem.LastModified = now;
                                inputPurchasingDocumentItem.LastModifiedBy = User.Identity.Name;

                                db.PurchasingDocumentItems.Add(inputPurchasingDocumentItem);
                                counter++;
                            }
                        }

                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{counter} Item succesfully affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ProcurementConfirmItem(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            if (inputPurchasingDocumentItems == null)
            {
                return Json(new { responseText = $"No Item affected" }, JsonRequestBehavior.AllowGet);
            }

            DateTime now = DateTime.Now;
            int counter = 0;

            try
            {
                foreach (var inputPurchasingDocumentItem in inputPurchasingDocumentItems)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

                    if (databasePurchasingDocumentItem.ActiveStage == "1")
                    {
                        databasePurchasingDocumentItem.ConfirmedItem = true;
                        //databasePurchasingDocumentItem.OpenQuantity = inputPurchasingDocumentItem.OpenQuantity;
                        databasePurchasingDocumentItem.ActiveStage = "2";
                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                        counter++;

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notification.StatusID = 3;
                        notification.Stage = "1";
                        notification.Role = "vendor";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{counter} Item affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return View(errorMessage);
            }
        }

        [HttpPost]
        public ActionResult CancelItem([Bind(Include = "ID")] PurchasingDocumentItem inputPurchasingDocumentItem)
        {
            DateTime now = DateTime.Now;

            try
            {
                PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

                if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "2" || databasePurchasingDocumentItem.ActiveStage == "0")
                {
                    databasePurchasingDocumentItem.ConfirmedItem = false;
                    databasePurchasingDocumentItem.OpenQuantity = null;

                    if (!databasePurchasingDocumentItem.ParentID.HasValue || databasePurchasingDocumentItem.ID == databasePurchasingDocumentItem.ParentID)
                    {
                        databasePurchasingDocumentItem.OpenQuantity = databasePurchasingDocumentItem.Quantity;
                    }

                    databasePurchasingDocumentItem.LastModified = now;
                    databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notificationProc = new Notification();
                    notificationProc.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                    notificationProc.StatusID = 2;
                    notificationProc.Stage = "1";
                    notificationProc.Role = "procurement";
                    notificationProc.isActive = true;
                    notificationProc.Created = now;
                    notificationProc.CreatedBy = User.Identity.Name;
                    notificationProc.Modified = now;
                    notificationProc.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notificationProc);

                    Notification notificationVend = new Notification();
                    notificationVend.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                    notificationVend.StatusID = 2;
                    notificationVend.Stage = "1";
                    notificationVend.Role = "vendor";
                    notificationVend.isActive = true;
                    notificationVend.Created = now;
                    notificationVend.CreatedBy = User.Identity.Name;
                    notificationVend.Modified = now;
                    notificationVend.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notificationVend);
                }

                db.SaveChanges();

                return Json(new { responseText = $"Item is canceled" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return View(errorMessage);
            }
        }

        #endregion

        #region STAGE 2

        // POST: Import/VendorConfirmFirstETA
        [HttpPost]
        public ActionResult VendorConfirmFirstETA(List<ETAHistory> inputETAHistories)
        {
            if (inputETAHistories == null)
            {
                return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                int count = 0;
                string user = User.Identity.Name;

                foreach (var inputETAHistory in inputETAHistories)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputETAHistory.PurchasingDocumentItemID);

                    if (databasePurchasingDocumentItem.ActiveStage == "2")
                    {
                        inputETAHistory.Created = now;
                        inputETAHistory.CreatedBy = user;
                        inputETAHistory.LastModified = now;
                        inputETAHistory.LastModifiedBy = user;

                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = user;

                        if (!databasePurchasingDocumentItem.HasETAHistory)
                        {
                            db.ETAHistories.Add(inputETAHistory);

                            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                            foreach (var previousNotification in previousNotifications)
                            {
                                previousNotification.isActive = false;
                            }

                            Notification notification = new Notification();
                            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                            notification.StatusID = 3;
                            notification.Stage = "2";
                            notification.Role = "procurement";
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);
                        }

                        count++;
                    }
                    else if (databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ProformaInvoiceDocument == null) // EDIT
                    {
                        List<ETAHistory> databaseEtaHistories = db.ETAHistories.Where(x => x.PurchasingDocumentItemID == inputETAHistory.PurchasingDocumentItemID).ToList();

                        foreach (var databaseEtaHistory in databaseEtaHistories)
                        {
                            db.ETAHistories.Remove(databaseEtaHistory);
                        }

                        inputETAHistory.Created = now;
                        inputETAHistory.CreatedBy = user;
                        inputETAHistory.LastModified = now;
                        inputETAHistory.LastModifiedBy = user;

                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = user;

                        if (!databasePurchasingDocumentItem.HasETAHistory)
                        {
                            db.ETAHistories.Add(inputETAHistory);
                        }

                        count++;
                    }
                }

                db.SaveChanges();
                return Json(new { responseText = $"{count} item affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Import/ProcurementAcceptFirstEta
        [HttpPost]
        public ActionResult ProcurementAcceptFirstEta(List<int> inputPurchasingDocumentItemIDs)
        {
            if (inputPurchasingDocumentItemIDs.Count < 1)
            {
                return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                int count = 0;
                string user = User.Identity.Name;

                foreach (var inputPurchasingDocumentItemID in inputPurchasingDocumentItemIDs)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

                    if (databasePurchasingDocumentItem.ActiveStage == "2" || databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ProformaInvoiceDocument == null)
                    {
                        ETAHistory firstETAHistory = databasePurchasingDocumentItem.FirstETAHistory;

                        firstETAHistory.AcceptedByProcurement = true;
                        firstETAHistory.LastModified = now;
                        firstETAHistory.LastModifiedBy = user;

                        databasePurchasingDocumentItem.ActiveStage = "2a";
                        databasePurchasingDocumentItem.ConfirmedItem = true;
                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = user;

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notification.StatusID = 3;
                        notification.Stage = "2";
                        notification.Role = "vendor";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

                        count++;
                    }
                }

                db.SaveChanges();
                return Json(new { responseText = $"{count} item affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Import/ProcurementDeclineFirstEta
        [HttpPost]
        public ActionResult ProcurementDeclineFirstEta(List<int> inputPurchasingDocumentItemIDs)
        {
            if (inputPurchasingDocumentItemIDs.Count < 1)
            {
                return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                int count = 0;
                string user = User.Identity.Name;

                foreach (var inputPurchasingDocumentItemID in inputPurchasingDocumentItemIDs)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

                    if (databasePurchasingDocumentItem.ActiveStage == "2" || databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ProformaInvoiceDocument == null)
                    {
                        ETAHistory firstETAHistory = databasePurchasingDocumentItem.FirstETAHistory;

                        firstETAHistory.AcceptedByProcurement = false;
                        firstETAHistory.LastModified = now;
                        firstETAHistory.LastModifiedBy = user;

                        databasePurchasingDocumentItem.ActiveStage = "2";
                        databasePurchasingDocumentItem.ConfirmedItem = false;
                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = user;

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notificationVendor = new Notification();
                        notificationVendor.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notificationVendor.StatusID = 2;
                        notificationVendor.Stage = "2";
                        notificationVendor.Role = "vendor";
                        notificationVendor.isActive = true;
                        notificationVendor.Created = now;
                        notificationVendor.CreatedBy = User.Identity.Name;
                        notificationVendor.Modified = now;
                        notificationVendor.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notificationVendor);

                        Notification notificationProc = new Notification();
                        notificationProc.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notificationProc.StatusID = 2;
                        notificationProc.Stage = "2";
                        notificationProc.Role = "procurement";
                        notificationProc.isActive = true;
                        notificationProc.Created = now;
                        notificationProc.CreatedBy = User.Identity.Name;
                        notificationProc.Modified = now;
                        notificationProc.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notificationProc);

                        count++;
                    }
                }

                db.SaveChanges();
                return Json(new { responseText = $"{count} item affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region STAGE 2a

        // POST: Import/VendorUploadProformaInvoice
        [HttpPost]
        public ActionResult VendorUploadProformaInvoice(int inputPurchasingDocumentItemID, HttpPostedFileBase fileProformaInvoice)
        {
            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (fileProformaInvoice.ContentLength > 0 || databasePurchasingDocumentItem.ActiveStage == "2a")
                {
                    string user = User.Identity.Name;

                    string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{Path.GetFileName(fileProformaInvoice.FileName)}";
                    string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/ProformaInvoice"), fileName);

                    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                    {
                        fileProformaInvoice.InputStream.CopyTo(fileStream);
                    }

                    databasePurchasingDocumentItem.ProformaInvoiceDocument = fileName;
                    databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = null;
                    databasePurchasingDocumentItem.LastModified = now;
                    databasePurchasingDocumentItem.LastModifiedBy = user;

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                    notification.StatusID = 3;
                    notification.Stage = "2a";
                    notification.Role = "procurement";
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notification);

                    db.SaveChanges();

                    string downloadUrl = Path.Combine("..\\Files\\Import\\ProformaInvoice", fileName);

                    return Json(new { responseText = $"File successfully uploaded", proformaInvoiceUrl = downloadUrl }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { responseText = $"File not uploaded" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        // POST: Import/VendorSkipProformaInvoice
        [HttpPost]
        public ActionResult VendorSkipProformaInvoice(int inputPurchasingDocumentItemID)
        {
            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage == "2a")
                {
                    string user = User.Identity.Name;

                    databasePurchasingDocumentItem.ActiveStage = "3";
                    databasePurchasingDocumentItem.LastModified = now;
                    databasePurchasingDocumentItem.LastModifiedBy = user;

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                    notification.StatusID = 2;
                    notification.Stage = "2a";
                    notification.Role = "procurement";
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notification);

                    db.SaveChanges();

                    return Json(new { responseText = $"Stage successfully Skipped" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { responseText = $"Stage failed to skip" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        // POST: Import/ProcurementApprovePI
        [HttpPost]
        public ActionResult ProcurementApprovePI([Bind(Include = "ID")] PurchasingDocumentItem inputPurchasingDocumentItem)
        {
            try
            {
                PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

                if (databasePurchasingDocumentItem.ActiveStage == "2a")
                {
                    string user = User.Identity.Name;

                    databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = true;
                    databasePurchasingDocumentItem.ActiveStage = "3";
                    databasePurchasingDocumentItem.LastModified = now;
                    databasePurchasingDocumentItem.LastModifiedBy = user;

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                    notification.StatusID = 3;
                    notification.Stage = "2a";
                    notification.Role = "vendor";
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notification);

                    db.SaveChanges();

                    return Json(new { responseText = $"Proforma Invoice successfully accepted" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { responseText = $"Process failed" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return View(errorMessage);
            }
        }

        // POST: Import/ProcurementDisapprovePI
        [HttpPost]
        public ActionResult ProcurementDisapprovePI([Bind(Include = "ID")] PurchasingDocumentItem inputPurchasingDocumentItem)
        {
            try
            {
                PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

                if (databasePurchasingDocumentItem.ActiveStage == "2a")
                {
                    string user = User.Identity.Name;

                    string pathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/ProformaInvoice"), databasePurchasingDocumentItem.ProformaInvoiceDocument);

                    System.IO.File.Delete(pathWithfileName);

                    databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = false;
                    databasePurchasingDocumentItem.ProformaInvoiceDocument = null;
                    databasePurchasingDocumentItem.ActiveStage = "2a";
                    databasePurchasingDocumentItem.LastModified = now;
                    databasePurchasingDocumentItem.LastModifiedBy = user;

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                    notification.StatusID = 2;
                    notification.Stage = "2a";
                    notification.Role = "vendor";
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notification);

                    db.SaveChanges();

                    return Json(new { responseText = $"Proforma Invoice successfully declined" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { responseText = $"Process failed" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return View(errorMessage);
            }
        }

        #endregion

        #region STAGE 3

        // POST: Import/VendorConfirmPaymentReceived
        [HttpPost]
        public ActionResult VendorConfirmPaymentReceived(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            if (inputPurchasingDocumentItems == null)
            {
                return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                int count = 0;
                string user = User.Identity.Name;

                foreach (var inputPurchasingDocumentItem in inputPurchasingDocumentItems)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

                    if (databasePurchasingDocumentItem.ActiveStage == "3" || databasePurchasingDocumentItem.ActiveStage == "4")
                    {
                        databasePurchasingDocumentItem.ConfirmReceivedPaymentDate = inputPurchasingDocumentItem.ConfirmReceivedPaymentDate;
                        databasePurchasingDocumentItem.ActiveStage = "4";
                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = user;

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notification.StatusID = 2;
                        notification.Stage = "3";
                        notification.Role = "procurement";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

                        count++;
                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{count} data affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return View(errorMessage);
            }
        }

        // POST: Import/VendorSkipConfirmPayment
        [HttpPost]
        public ActionResult VendorSkipConfirmPayment(int inputPurchasingDocumentItemID)
        {
            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage == "3")
                {
                    string user = User.Identity.Name;

                    databasePurchasingDocumentItem.ActiveStage = "4";
                    databasePurchasingDocumentItem.LastModified = now;
                    databasePurchasingDocumentItem.LastModifiedBy = user;

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                    notification.StatusID = 2;
                    notification.Stage = "3";
                    notification.Role = "procurement";
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notification);

                    db.SaveChanges();

                    return Json(new { responseText = $"Stage successfully Skipped" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { responseText = $"Stage failed to skip" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        #endregion

        #region STAGE 4

        // POST: Import/VendorUpdateETA
        [HttpPost]
        public ActionResult VendorUpdateETA([Bind(Include = "PurchasingDocumentItemID,ETADate,DelayReasonID")]ETAHistory inputETAHistory)
        {
            PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Find(inputETAHistory.PurchasingDocumentItemID);

            if (purchasingDocumentItem == null)
            {
                return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                if (purchasingDocumentItem.ActiveStage == "4")
                {
                    string user = User.Identity.Name;

                    if (inputETAHistory.DelayReasonID == 0)
                    {
                        inputETAHistory.DelayReasonID = null;
                    }

                    inputETAHistory.Created = now;
                    inputETAHistory.CreatedBy = user;
                    inputETAHistory.LastModified = now;
                    inputETAHistory.LastModifiedBy = user;

                    purchasingDocumentItem.ActiveStage = "5";
                    purchasingDocumentItem.LastModified = now;
                    purchasingDocumentItem.LastModifiedBy = user;

                    if (purchasingDocumentItem.ETAHistories.Count < 2)
                    {
                        db.ETAHistories.Add(inputETAHistory);

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == purchasingDocumentItem.ID).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                        notification.StatusID = 2;
                        notification.Stage = "4";
                        notification.Role = "procurement";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

                        db.SaveChanges();
                    }

                    return Json(new { responseText = $"One data affected" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult VendorUploadProgressPhotoes(int inputPurchasingDocumentItemID, HttpPostedFileBase[] fileProgressPhotoes)
        {
            string user = User.Identity.Name;
            int count = 1;

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            if (databasePurchasingDocumentItem.ProgressPhotoes.Count < 1)
            {
                foreach (var fileProgressPhoto in fileProgressPhotoes)
                {
                    string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{count}_{Path.GetFileName(fileProgressPhoto.FileName)}";
                    string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/ProgressPhotos"), fileName);

                    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                    {
                        fileProgressPhoto.InputStream.CopyTo(fileStream);
                    }

                    ProgressPhoto progressPhoto = new ProgressPhoto();

                    progressPhoto.FileName = fileName;
                    progressPhoto.PurchasingDocumentItemID = inputPurchasingDocumentItemID;
                    progressPhoto.Created = now;
                    progressPhoto.CreatedBy = user;
                    progressPhoto.LastModified = now;
                    progressPhoto.LastModifiedBy = user;

                    db.ProgressPhotoes.Add(progressPhoto);

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                    notification.StatusID = 2;
                    notification.Stage = "4";
                    notification.Role = "procurement";
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notification);

                    count++;
                }
            }

            databasePurchasingDocumentItem.LastModified = now;
            databasePurchasingDocumentItem.LastModifiedBy = user;

            db.SaveChanges();

            List<ProgressPhoto> progressPhotoes = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
            List<string> imageSources = new List<string>();

            foreach (var progressPhoto in progressPhotoes)
            {
                string path = $"../Files/Import/ProgressPhotos/{progressPhoto.FileName}";
                imageSources.Add(path);
            }

            return Json(new { responseText = $"Files successfully uploaded", imageSources }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region STAGE 5

        // POST: Import/VendorConfirmShipmentBookingDate
        [HttpPost]
        public ActionResult VendorConfirmShipmentBookingDate(List<Shipment> inputShipmentBookDates)
        {
            int count = 0;

            foreach (var inputShipment in inputShipmentBookDates)
            {

                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Find(inputShipment.PurchasingDocumentItemID);

                if (purchasingDocumentItem == null)
                {
                    return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
                }

                try
                {
                    if (purchasingDocumentItem.ActiveStage == "5" && purchasingDocumentItem.Shipments.Count < 1)
                    {
                        string user = User.Identity.Name;

                        inputShipment.Created = now;
                        inputShipment.CreatedBy = user;
                        inputShipment.LastModified = now;
                        inputShipment.LastModifiedBy = user;

                        db.Shipments.Add(inputShipment);

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == purchasingDocumentItem.ID).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                        notification.StatusID = 2;
                        notification.Stage = "5";
                        notification.Role = "procurement";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

                        count++;
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = (ex.Message + ex.StackTrace);
                    return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
                }
            }

            db.SaveChanges();

            return Json(new { responseText = $"{count} data affected", count = count }, JsonRequestBehavior.AllowGet);

        }

        // POST: Import/VendorConfirmShipmentBookingDate
        [HttpPost]
        public ActionResult VendorConfirmATD(List<Shipment> inputShipmentATDs)
        {
            int count = 0;
            string user = User.Identity.Name;

            foreach (var inputShipment in inputShipmentATDs)
            {

                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Find(inputShipment.PurchasingDocumentItemID);

                if (purchasingDocumentItem == null)
                {
                    return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
                }

                try
                {
                    if (purchasingDocumentItem.ActiveStage == "5" && purchasingDocumentItem.Shipments.Count > 0)
                    {
                        Shipment databaseShipment = purchasingDocumentItem.FirstShipment;

                        databaseShipment.ATDDate = inputShipment.ATDDate;
                        databaseShipment.Created = now;
                        databaseShipment.CreatedBy = user;
                        databaseShipment.LastModified = now;
                        databaseShipment.LastModifiedBy = user;

                        purchasingDocumentItem.ActiveStage = "6";
                        purchasingDocumentItem.LastModified = now;
                        purchasingDocumentItem.LastModifiedBy = user;

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == purchasingDocumentItem.ID).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                        notification.StatusID = 2;
                        notification.Stage = "5";
                        notification.Role = "procurement";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

                        count++;
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = (ex.Message + ex.StackTrace);
                    return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
                }
            }

            db.SaveChanges();
            return Json(new { responseText = $"{count} data affected", count = count }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region STAGE 6

        // POST: Import/VendorFillInShipmentForm
        [HttpPost]
        public ActionResult VendorFillInShipmentForm(int inputPurchasingDocumentItemID, DateTime inputCopyBLDate, HttpPostedFileBase fileCopyBL, HttpPostedFileBase filePackingList, HttpPostedFileBase fileInvoice, string inputAWB, string inputCourierName)
        {
            PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);
            Shipment shipment = purchasingDocumentItem.FirstShipment;

            if (shipment == null || purchasingDocumentItem == null)
            {
                return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                if (purchasingDocumentItem.ActiveStage == "6" && purchasingDocumentItem.Shipments.Count > 0 && fileCopyBL.ContentLength > 0 && filePackingList.ContentLength > 0 && fileInvoice.ContentLength > 0)
                {
                    string user = User.Identity.Name;

                    // CopyBL
                    string fileName = $"{shipment.ID.ToString()}_CopyBL_{Path.GetFileName(fileCopyBL.FileName)}";
                    string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/Shipping/CopyBL"), fileName);
                    shipment.CopyBLDocument = fileName;

                    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                    {
                        fileCopyBL.InputStream.CopyTo(fileStream);
                    }

                    // PackingList
                    fileName = $"{shipment.ID.ToString()}_PackingList_{Path.GetFileName(filePackingList.FileName)}";
                    uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/Shipping/PackingList"), fileName);
                    shipment.PackingListDocument = fileName;

                    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                    {
                        filePackingList.InputStream.CopyTo(fileStream);
                    }

                    // Invoice
                    fileName = $"{shipment.ID.ToString()}_Invoice_{Path.GetFileName(fileInvoice.FileName)}";
                    uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/Shipping/Invoice"), fileName);
                    shipment.InvoiceDocument = fileName;

                    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                    {
                        fileInvoice.InputStream.CopyTo(fileStream);
                    }

                    shipment.CopyBLDate = inputCopyBLDate;
                    shipment.AWB = inputAWB;
                    shipment.CourierName = inputCourierName;
                    shipment.LastModified = now;
                    shipment.LastModifiedBy = user;

                    purchasingDocumentItem.ActiveStage = "7";
                    purchasingDocumentItem.LastModified = now;
                    purchasingDocumentItem.LastModifiedBy = user;

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == purchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                    notification.StatusID = 2;
                    notification.Stage = "6";
                    notification.Role = "procurement";
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notification);

                    db.SaveChanges();
                }
                else
                {
                    return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { responseText = $"One data affected" }, JsonRequestBehavior.AllowGet);

        }

        // POST: Import/GetShippingInformation
        [HttpPost]
        public ActionResult GetShippingInformation(int myPurchasingDocumentItemId)
        {
            PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Find(myPurchasingDocumentItemId);
            Shipment shipment = purchasingDocumentItem.FirstShipment;

            if (shipment == null || purchasingDocumentItem == null)
            {
                return Json(new { isCompleted = false }, JsonRequestBehavior.AllowGet);
            }

            if (purchasingDocumentItem.ActiveStage != "2a")
            {
                if (Convert.ToInt32(purchasingDocumentItem.ActiveStage) > 6)
                {
                    string dokumenCopyBL = Path.Combine("../Files/Import/Shipping/CopyBL", shipment.CopyBLDocument);
                    string dokumenPackingList = Path.Combine("../Files/Import/Shipping/PackingList", shipment.PackingListDocument);
                    string dokumenInvoice = Path.Combine("../Files/Import/Shipping/Invoice", shipment.InvoiceDocument);

                    return Json(new { isCompleted = true, activeStage = purchasingDocumentItem.ActiveStageView, copyBLDate = shipment.CopyBLDateView, dokumenCopyBL, dokumenPackingList, dokumenInvoice, awb = shipment.AWB, courierName = shipment.CourierName }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isCompleted = false }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region STAGE 7

        // POST: Import/ProcurementConfirmOnAirport
        [HttpPost]
        public ActionResult ProcurementConfirmOnAirport(List<Shipment> inputShipments)
        {
            if (inputShipments.Count < 1)
            {
                return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                int count = 0;
                string user = User.Identity.Name;

                foreach (var inputShipment in inputShipments)
                {
                    PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Find(inputShipment.PurchasingDocumentItemID);
                    Shipment shipment = purchasingDocumentItem.FirstShipment;

                    if (purchasingDocumentItem.ActiveStage == "7")
                    {
                        shipment.ATADate = inputShipment.ATADate;
                        shipment.ETADate = inputShipment.ETADate;
                        shipment.LastModified = now;
                        shipment.LastModifiedBy = user;

                        purchasingDocumentItem.ActiveStage = "8";
                        purchasingDocumentItem.LastModified = now;
                        purchasingDocumentItem.LastModifiedBy = user;

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == purchasingDocumentItem.ID).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                        notification.StatusID = 2;
                        notification.Stage = "7";
                        notification.Role = "vendor";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

                        count++;
                    }
                }

                db.SaveChanges();
                return Json(new { responseText = $"{count} data affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region STAGE 10


        [HttpPost]
        public ActionResult VendorUploadInvoice(int inputPurchasingDocumentItemID, HttpPostedFileBase fileInvoice)
        {
            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage != "2a")
                {
                    if (fileInvoice.ContentLength > 0 || Convert.ToInt32(databasePurchasingDocumentItem.ActiveStage) > 7)
                    {
                        if (databasePurchasingDocumentItem.InvoiceDocument == null)
                        {

                            string user = User.Identity.Name;

                            string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{Path.GetFileName(fileInvoice.FileName)}";
                            string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/Invoice"), fileName);

                            using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                            {
                                fileInvoice.InputStream.CopyTo(fileStream);
                            }

                            databasePurchasingDocumentItem.InvoiceDocument = fileName;
                            databasePurchasingDocumentItem.LastModified = now;
                            databasePurchasingDocumentItem.LastModifiedBy = user;

                            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                            foreach (var previousNotification in previousNotifications)
                            {
                                previousNotification.isActive = false;
                            }

                            Notification notification = new Notification();
                            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                            notification.StatusID = 2;
                            notification.Stage = "10";
                            notification.Role = "procurement";
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);

                            db.SaveChanges();

                            string downloadUrl = Path.Combine("..\\Files\\Import\\Invoice", fileName);

                            return Json(new { responseText = $"File successfully uploaded", invoiceUrl = downloadUrl }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { responseText = $"File not uploaded" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { responseText = $"File not uploaded" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { responseText = $"File not uploaded" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }
        [HttpPost]
        public ActionResult VendorRemoveUploadInvoice(int inputPurchasingDocumentItemID)
        {
            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage != "2a")
                {
                    if (Convert.ToInt32(databasePurchasingDocumentItem.ActiveStage) > 7)
                    {
                        if (databasePurchasingDocumentItem.InvoiceDocument != null)
                        {
                            string user = User.Identity.Name;

                            string pathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/Invoice"), databasePurchasingDocumentItem.InvoiceDocument);

                            System.IO.File.Delete(pathWithfileName);

                            databasePurchasingDocumentItem.InvoiceDocument = null;
                            databasePurchasingDocumentItem.LastModified = now;
                            databasePurchasingDocumentItem.LastModifiedBy = user;

                            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                            foreach (var previousNotification in previousNotifications)
                            {
                                previousNotification.isActive = false;
                            }

                            Notification notification = new Notification();
                            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                            notification.StatusID = 2;
                            notification.Stage = "10";
                            notification.Role = "procurement";
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);

                            db.SaveChanges();

                            return Json(new { responseText = $"File successfully removed" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { responseText = $"File not uploaded" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { responseText = $"File not uploaded" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { responseText = $"File not uploaded" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        #endregion

        #region WHITEBOARD
        // GET: Import/ReturnBharasTestPO (116-131)
        public ActionResult ReturnBharasTestPO()
        {
            List<PurchasingDocumentItem> purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ID >= 116 && x.ID <= 131).ToList();

            foreach (var purchasingDocumentItem in purchasingDocumentItems)
            {
                purchasingDocumentItem.ActiveStage = null;
                purchasingDocumentItem.ParentID = null;
                purchasingDocumentItem.ConfirmedItem = null;
                purchasingDocumentItem.ConfirmedQuantity = null;
                purchasingDocumentItem.ConfirmedDate = null;
                purchasingDocumentItem.OpenQuantity = null;
                purchasingDocumentItem.ProformaInvoiceDocument = null;
                purchasingDocumentItem.ApproveProformaInvoiceDocument = null;

                List<PurchasingDocumentItem> childPurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ID != purchasingDocumentItem.ID && x.ParentID == purchasingDocumentItem.ID).ToList();

                foreach (var childPurchasingDocumentItem in childPurchasingDocumentItems)
                {
                    List<ETAHistory> childETAHistories = db.ETAHistories.Where(x => x.PurchasingDocumentItemID == childPurchasingDocumentItem.ID).ToList();

                    foreach (var childETAHistory in childETAHistories)
                    {
                        db.ETAHistories.Remove(childETAHistory);
                    }

                    db.PurchasingDocumentItems.Remove(childPurchasingDocumentItem);
                }

                List<ETAHistory> eTAHistories = db.ETAHistories.Where(x => x.PurchasingDocumentItemID == purchasingDocumentItem.ID).ToList();

                foreach (var eTAHistory in eTAHistories)
                {
                    db.ETAHistories.Remove(eTAHistory);
                }

            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }
        #endregion
    }
}