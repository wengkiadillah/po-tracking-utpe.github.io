using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using PagedList;
using System.Globalization;
using POTrackingV2.ViewModels;
using System.IO;
using POTrackingV2.CustomAuthentication;
using System.Web.Security;
using POTrackingV2.Constants;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleVendor + "," + LoginConstants.RoleProcurement)]
    public class LocalController : Controller
    {
        public DateTime now = DateTime.Now;
        private POTrackingEntities db = new POTrackingEntities();

        #region PAGELIST
        // GET: Local
        public ActionResult Index(string searchData, string filterBy, string searchStartPODate, string searchEndPODate, int? page, string role)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string roleID = myUser.Roles;

            var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
            var pOes = db.POes.Include(x => x.PurchasingDocumentItems)
                                //.Where(x => x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material)))
                                .Where(x => (x.Type.ToLower() == "zo05" || x.Type.ToLower() == "zo09" || x.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.VendorCode) && x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material)))
                                .Include(x => x.Vendor)
                                .OrderBy(x => x.Number)
                                .AsQueryable();

            if (roleID == "procurement")
            {
                pOes = pOes.Include(x => x.PurchasingDocumentItems)
                                .Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null))
                                .Include(x => x.Vendor)
                                .OrderBy(x => x.Number)
                                .AsQueryable();
            }

            ViewBag.CurrentSearchFilterBy = filterBy;
            ViewBag.CurrentSearchData = searchData;
            ViewBag.CurrentStartPODate = searchStartPODate;
            ViewBag.CurrentEndPODate = searchEndPODate;
            ViewBag.CurrentRoleID = roleID.ToLower();
            ViewBag.POCount = pOes.Count(); // DEBUG 

            List<DelayReason> delayReasons = db.DelayReasons.ToList();

            ViewBag.DelayReasons = delayReasons;

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

                pOes = pOes.Where(x => x.Date >= startDate)
                                .Include(x => x.PurchasingDocumentItems)
                                .Include(x => x.Vendor);
            }

            if (!String.IsNullOrEmpty(searchEndPODate))
            {

                DateTime endDate = DateTime.ParseExact(searchEndPODate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                pOes = pOes.Where(x => x.Date <= endDate)
                                .Include(x => x.PurchasingDocumentItems)
                                .Include(x => x.Vendor);
            }

            //IndexLocalViewModel indexLocalViewModel = new IndexLocalViewModel();
            //indexLocalViewModel.POes = pOes.ToPagedList(page ?? 1, Constants.PageSize);
            //indexLocalViewModel.InputPurchasingDocumentItem = new PurchasingDocumentItem();

            return View(pOes.ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
        }

        
        #endregion

        #region stage1
        // GET: Local/VendorConfirmItem
        [HttpPost]
        public ActionResult VendorConfirmQuantity(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            if (inputPurchasingDocumentItems == null)
            {
                return Json(new { responseText = $"No Item affected" }, JsonRequestBehavior.AllowGet);
            }

            DateTime now = DateTime.Now;
            int counter = 0;
            List<bool> isSameAsProcs = new List<bool>();

            try
            {
                foreach (var inputPurchasingDocumentItem in inputPurchasingDocumentItems)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = new PurchasingDocumentItem();

                    if (!inputPurchasingDocumentItem.ParentID.HasValue)
                    {
                        databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ID).FirstOrDefault();

                        List<PurchasingDocumentItem> childPurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == inputPurchasingDocumentItem.ID && x.ID != inputPurchasingDocumentItem.ID).ToList();
                        if (childPurchasingDocumentItems.Count > 0)
                        {
                            //Child Clean Up
                            foreach (var childPurchasingDocumentItem in childPurchasingDocumentItems)
                            {
                                if (childPurchasingDocumentItem.ID != inputPurchasingDocumentItem.ID)
                                {
                                    db.PurchasingDocumentItems.Remove(childPurchasingDocumentItem);
                                }
                            }
                        }

                        if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0")
                        {

                            //databasePurchasingDocumentItem.ParentID = databasePurchasingDocumentItem.ID;
                            databasePurchasingDocumentItem.ConfirmedQuantity = inputPurchasingDocumentItem.ConfirmedQuantity;
                            databasePurchasingDocumentItem.ConfirmedDate = inputPurchasingDocumentItem.ConfirmedDate;
                            databasePurchasingDocumentItem.LastModified = now;
                            databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                            counter++;

                            if (inputPurchasingDocumentItem.ConfirmedQuantity == databasePurchasingDocumentItem.Quantity && inputPurchasingDocumentItem.ConfirmedDate == databasePurchasingDocumentItem.DeliveryDate)
                            {
                                databasePurchasingDocumentItem.ConfirmedItem = true;
                                databasePurchasingDocumentItem.ActiveStage = "2";
                                isSameAsProcs.Add(true);
                            }
                            else
                            {
                                databasePurchasingDocumentItem.ConfirmedItem = null;
                                databasePurchasingDocumentItem.ActiveStage = "1";
                                isSameAsProcs.Add(false);
                            }

                            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                            foreach (var previousNotification in previousNotifications)
                            {
                                previousNotification.isActive = false;
                            }

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
                            inputPurchasingDocumentItem.WorkTime = databasePurchasingDocumentItem.WorkTime;
                            inputPurchasingDocumentItem.DeliveryDate = databasePurchasingDocumentItem.DeliveryDate;
                            inputPurchasingDocumentItem.IsClosed = "";

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

                return Json(new { responseText = $"{counter} Item succesfully affected",isSameAsProcs }, JsonRequestBehavior.AllowGet);
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            if (inputPurchasingDocumentItems == null)
            {
                return Json(new { responseText = $"No Item affected" }, JsonRequestBehavior.AllowGet);
            }

            DateTime now = DateTime.Now;
            int counter = 0;
            List<bool> isSameAsProcs = new List<bool>();

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
                                //databasePurchasingDocumentItem.ParentID = databasePurchasingDocumentItem.ID;
                                //databasePurchasingDocumentItem.ConfirmedItem = null;
                                databasePurchasingDocumentItem.ConfirmedQuantity = inputPurchasingDocumentItem.ConfirmedQuantity;
                                databasePurchasingDocumentItem.ConfirmedDate = inputPurchasingDocumentItem.ConfirmedDate;
                                //databasePurchasingDocumentItem.ActiveStage = "1";
                                databasePurchasingDocumentItem.LastModified = now;
                                databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                                counter++;

                                if (inputPurchasingDocumentItem.ConfirmedQuantity == databasePurchasingDocumentItem.Quantity && inputPurchasingDocumentItem.ConfirmedDate == databasePurchasingDocumentItem.DeliveryDate)
                                {
                                    databasePurchasingDocumentItem.ConfirmedItem = true;
                                    databasePurchasingDocumentItem.ActiveStage = "2";
                                    isSameAsProcs.Add(true);
                                }
                                else
                                {
                                    databasePurchasingDocumentItem.ConfirmedItem = null;
                                    databasePurchasingDocumentItem.ActiveStage = "1";
                                    isSameAsProcs.Add(false);
                                }
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

                return Json(new { responseText = $"{counter} Item succesfully affected",isSameAsProcs }, JsonRequestBehavior.AllowGet);
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "procurement")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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

                    if (databasePurchasingDocumentItem.ActiveStage == "1" || (databasePurchasingDocumentItem.ActiveStage == "2" && !databasePurchasingDocumentItem.HasETAHistory))
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

        /*[HttpPost]
        public ActionResult ProcurementConfirmAllItem(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            if (inputPurchasingDocumentItems == null)
            {
                return Json(new { responseText = $"No data affected" }, JsonRequestBehavior.AllowGet);
            }

            DateTime now = DateTime.Now;

            try
            {
                foreach (var inputPurchasingDocumentItem in inputPurchasingDocumentItems)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ID).FirstOrDefault();

                    databasePurchasingDocumentItem.ConfirmedItem = true;
                    databasePurchasingDocumentItem.OpenQuantity = databasePurchasingDocumentItem.Quantity - databasePurchasingDocumentItem.ConfirmedQuantity;
                    databasePurchasingDocumentItem.ActiveStage = "2";
                    databasePurchasingDocumentItem.LastModified = now;
                    databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                }

                db.SaveChanges();

                return Json(new { responseText = $"{inputPurchasingDocumentItems.Count()} data affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return View(errorMessage);
            }
        }*/

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

        #region stage 2 VendorConfirmETA
         // POST: Import/VendorConfirmAllFirstETA
        [HttpPost]
        public ActionResult VendorConfirmFirstETA(List<ETAHistory> inputETAHistories)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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

                        //databasePurchasingDocumentItem.ActiveStage = "2a";
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

                        //databasePurchasingDocumentItem.ActiveStage = "2a";
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "procurement")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "procurement")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notification.StatusID = 2;
                        notification.Stage = "2";
                        notification.Role = "vendor";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

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

        #region stage 2a (uploadproformainvoice)
        [HttpPost]
        public ActionResult VendorUploadInvoice (int inputPurchasingDocumentItemID,HttpPostedFileBase fileProformaInvoice)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);
            //string errorMessage = string.Empty;
                  
            try
            {
                if (fileProformaInvoice.ContentLength > 0 || purchasingDocumentItem.ActiveStage == "2a")
                {
                    string user = User.Identity.Name;

                    string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{Path.GetFileName(fileProformaInvoice.FileName)}";
                    //pathLOcal
                    string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/ProformaInvoice"), fileName);

                    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                    {
                        fileProformaInvoice.InputStream.CopyTo(fileStream);
                    }

                    purchasingDocumentItem.ProformaInvoiceDocument = fileName;
                    purchasingDocumentItem.LastModified = now;
                    purchasingDocumentItem.LastModifiedBy = user;
                    purchasingDocumentItem.ApproveProformaInvoiceDocument = null;
                    //purchasingDocumentItem.ActiveStage = "4";

                    List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == purchasingDocumentItem.ID).ToList();
                    foreach (var previousNotification in previousNotifications)
                    {
                        previousNotification.isActive = false;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = purchasingDocumentItem.ID;
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

                    string downloadUrl = Path.Combine("..\\Files\\Local\\ProformaInvoice", fileName);

                    return Json(new { responseText = $"File successfully uploaded", proformaInvoiceUrl = downloadUrl }, JsonRequestBehavior.AllowGet);
                    //return Json();
                }
                else
                {
                    return Json(new { responseText = $"There is no File" }, JsonRequestBehavior.AllowGet);
                    //return Json 
                }

            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
                    
            }
            
            //return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ProcurementApprovePI([Bind(Include = "ID")] PurchasingDocumentItem inputPurchasingDocumentItem)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "procurement")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "procurement")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

                if (databasePurchasingDocumentItem.ActiveStage == "2a")
                {
                    string user = User.Identity.Name;

                    string pathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/ProformaInvoice"), databasePurchasingDocumentItem.ProformaInvoiceDocument);

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

        [HttpPost]
        public ActionResult VendorSkipPI([Bind(Include = "ID")] PurchasingDocumentItem inputPurchasingDocumentItem)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

                if (databasePurchasingDocumentItem.ActiveStage == "2a" || databasePurchasingDocumentItem.ActiveStage == "3")
                {
                    string user = User.Identity.Name;

                    //databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = false;
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

                    return Json(new { responseText = $"Proforma Invoice successfully skipped" }, JsonRequestBehavior.AllowGet);
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

        #region stage 3 (vendorconfirmpaymentreceived)
        [HttpPost]
        public ActionResult VendorConfirmPaymentReceived(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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

        [HttpPost]
        public ActionResult VendorSkipConfirmPayment(int inputPurchasingDocumentItemID)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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

        #region stage 4 (ETAOntimeDelay)
        [HttpPost]
        public ActionResult VendorUpdateETA([Bind(Include = "PurchasingDocumentItemID,ETADate,DelayReasonID")]ETAHistory inputETAHistory)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            string user = User.Identity.Name;
            int count = 1;

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            if (databasePurchasingDocumentItem.ProgressPhotoes.Count < 1)
            {

                foreach (var fileProgressPhoto in fileProgressPhotoes)
                {
                    string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{count}_{Path.GetFileName(fileProgressPhoto.FileName)}";
                    string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/ProgressProforma"), fileName);

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
                    notification.StatusID = 1;
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
            //PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            //databasePurchasingDocumentItem.ActiveStage = "5";
            databasePurchasingDocumentItem.LastModified = now;
            databasePurchasingDocumentItem.LastModifiedBy = user;

            db.SaveChanges();

            List<ProgressPhoto> progressPhotoes = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
            List<string> imageSources = new List<string>();

            foreach (var progressPhoto in progressPhotoes)
            {
                string path = $"../Files/Local/ProgressProforma/{progressPhoto.FileName}";
                imageSources.Add(path);
            }

            return Json(new { responseText = $"Files successfully uploaded", imageSources }, JsonRequestBehavior.AllowGet);
        }

        /*[HttpPost]
        public ActionResult VendorCheckDelayReason(List<int> inputPurchasingDocumentItemIDs)
        {
            List<int> delayReasonIDs = new List<int>();

            foreach (var inputPurchasingDocumentItemID in inputPurchasingDocumentItemIDs)
            {
                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

                delayReasonIDs.Add(purchasingDocumentItem.LastETAHistory.DelayReasonID.GetValueOrDefault());
            }

            return Json(new { delayReasonIDs }, JsonRequestBehavior.AllowGet);
        }*/

        #endregion


        #region STAGE 6
        [HttpPost]
        public ActionResult VendorUploadInvoice_2(int inputPurchasingDocumentItemID, HttpPostedFileBase fileInvoice)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStageToNumber > 3 && databasePurchasingDocumentItem.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity >= databasePurchasingDocumentItem.ConfirmedQuantity)
                {
                    if (fileInvoice.ContentLength > 0 )
                    {
                        if (databasePurchasingDocumentItem.InvoiceDocument == null)
                        {

                            string user = User.Identity.Name;

                            string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{Path.GetFileName(fileInvoice.FileName)}";
                            string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/Invoice"), fileName);

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
                            notification.StatusID = 3;
                            notification.Stage = "6";
                            notification.Role = "procurement";
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);

                            db.SaveChanges();

                            string downloadUrl = Path.Combine("..\\Files\\Local\\Invoice", fileName);

                            return Json(new { responseCode = "200", responseText = $"File successfully uploaded", invoiceUrl = downloadUrl }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { responseCode = "400", responseText = $"File not uploaded 1" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { responseCode = "400", responseText = $"File not uploaded 2" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { responseCode = "400", responseText = $"File not uploaded 3" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = true, responseCode = "400", responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult VendorRemoveUploadInvoice(int inputPurchasingDocumentItemID)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles != "vendor")
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                //if (databasePurchasingDocumentItem.ActiveStage != "2a")
                //{
                if (Convert.ToInt32(databasePurchasingDocumentItem.ActiveStage) > 3)
                {
                    if (databasePurchasingDocumentItem.InvoiceDocument != null)
                    {
                        string user = User.Identity.Name;

                        string pathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/Invoice"), databasePurchasingDocumentItem.InvoiceDocument);

                        System.IO.File.Delete(pathWithfileName);

                        databasePurchasingDocumentItem.InvoiceDocument = null;
                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = user;

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID && x.StatusID == 3).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
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
                //}
                //else
                //{
                //    return Json(new { responseText = $"File not uploaded" }, JsonRequestBehavior.AllowGet);
                //}
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        #endregion

        // GET: Local/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Local/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Local/Create
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

        // GET: Local/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Local/Edit/5
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

        // GET: Local/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Local/Delete/5
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
