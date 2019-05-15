using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using POTracking.Models;
using POTracking.ViewModels;
using System.Globalization;
using System.IO;

namespace POTrackingV2.Controllers
{
    [Authorize]
    public class SubcontController : Controller
    {
        POTrackingEntities db = new POTrackingEntities();
        DateTime now = DateTime.Now;
        // GET: POSubcont
        public ActionResult Index(string role, string searchData, string filterBy, string searchStartPODate, string searchEndPODate, int? page)
        {
            try
            {
                //var pOes = db.POes.OrderBy(x => x.Number).AsQueryable();
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x=> x.VendorCode).Distinct();

                var pOes = db.POes.AsQueryable();

                if (role == "procurement")
                {
                    //pOes = pOes.Where(po => po.PurchasingDocumentItems.Any(x => x.ConfirmedQuantity > 0 && x.Material != "" && x.Material != null && x.ParentID == null)).OrderBy(x => x.Number);
                    pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.ConfirmedQuantity > 0 && x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode)).OrderBy(x => x.Number);
                }
                else
                {
                    //pOes = pOes.Where(po => po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && x.ParentID == null)).OrderBy(x => x.Number);
                    pOes = pOes.Where(po => (po.Type.ToLower()=="zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode)).OrderBy(x => x.Number);
                }

                ViewBag.CurrentData = searchData;
                ViewBag.CurrentStartPODate = searchStartPODate;
                ViewBag.CurrentEndPODate = searchEndPODate;

                #region Filter
                if (!String.IsNullOrEmpty(searchData))
                {
                    if (filterBy == "poNumber")
                    {
                        pOes = pOes.Where(po => po.Number.Contains(searchData));
                    }
                    else if (filterBy == "vendor")
                    {
                        pOes = pOes.Where(po => po.Vendor.Name.Contains(searchData));
                    }
                    else if (filterBy == "material")
                    {
                        pOes = pOes.Where(po => po.PurchasingDocumentItems.Any(pdi => pdi.Material.Contains(searchData) || pdi.Description.Contains(searchData)));
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
                //return (RedirectToAction("Index", "Error", new { ErrorList = "hi there!" }));
                return View(pOes.ToPagedList(page ?? 1, Constants.PageSize));
            }
            catch (Exception ex)
            {
                return View(ex.Message + "-----" + ex.StackTrace);
            }
        }

        #region Template
        public ActionResult Template(string searchPoNumber, string searchStartPODate, string searchEndPODate, int? page)
        {
            var viewModels = db.POes.OrderBy(x => x.Number).AsQueryable();
            ViewBag.CurrentPONumber = searchPoNumber;
            ViewBag.CurrentStartPODate = searchStartPODate;
            ViewBag.CurrentEndPODate = searchEndPODate;

            if (!String.IsNullOrEmpty(searchPoNumber))
            {
                viewModels = viewModels.Where(x => x.Number.Contains(searchPoNumber));
            }

            if (!String.IsNullOrEmpty(searchStartPODate))
            {
                DateTime startDate = DateTime.ParseExact(searchStartPODate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                viewModels = viewModels.Where(x => x.Date >= startDate);
            }

            if (!String.IsNullOrEmpty(searchEndPODate))
            {
                DateTime endDate = DateTime.ParseExact(searchEndPODate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                viewModels = viewModels.Where(x => x.Date <= endDate);
            }

            int pageNumber = (page ?? 1);
            return View(viewModels.ToPagedList(pageNumber, Constants.PageSize));
        }
        #endregion

        #region Stage 1
        
        [HttpPost]
        public ActionResult SaveAllPOItem(string role, List<PurchasingDocumentItem> purchasingDocumentItems, List<PurchasingDocumentItem> purchasingDocumentItemChilds)
        {
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                if (purchasingDocumentItems != null)
                {
                    foreach (PurchasingDocumentItem item in purchasingDocumentItems)
                    {
                        PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == item.ID).FirstOrDefault();

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = Existed_PDI.ID;
                        notification.StatusID = 3;

                        if (role == "vendor")
                        {
                            Existed_PDI.ActiveStage = "1";
                            Existed_PDI.ConfirmedDate = item.ConfirmedDate;
                            Existed_PDI.ConfirmedQuantity = item.ConfirmedQuantity;

                            notification.Stage = "1";
                            notification.Role = "procurement";
                        }
                        else
                        {
                            SubcontComponentCapability scc = db.SubcontComponentCapabilities.Where(x => x.VendorCode == item.PO.VendorCode && x.Material == item.Material).FirstOrDefault();
                            int totalItemGR = Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;
                            
                            Existed_PDI.ConfirmedItem = true;
                            if (scc.isNeedSequence==true)
                            {
                                Existed_PDI.ActiveStage = "2";
                                notification.Stage = "2";
                            }
                            else
                            {
                                if(item.ConfirmedQuantity>0 && item.ConfirmedQuantity <= totalItemGR)
                                {
                                    Existed_PDI.ActiveStage = "5";
                                    notification.Stage = "5";
                                }
                                else
                                {
                                    Existed_PDI.ActiveStage = "4";
                                    notification.Stage = "4";
                                }
                            }

                            Notification Existed_notification = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID).FirstOrDefault();
                            if (Existed_notification != null)
                            {
                                Existed_notification.isActive = false;
                                Existed_notification.Modified = now;
                                Existed_notification.ModifiedBy = User.Identity.Name;
                            }
                            
                            notification.Role = "vendor";
                        }
                        
                        Existed_PDI.LastModified = now;
                        Existed_PDI.LastModifiedBy = User.Identity.Name;

                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;
                        
                        if (role == "vendor")
                        {
                            // Child clean-up
                            List<PurchasingDocumentItem> childDatabasePurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == item.ID).ToList();
                            foreach (var childDatabasePurchasingDocumentItem in childDatabasePurchasingDocumentItems)
                            {
                                if (childDatabasePurchasingDocumentItem.ID != item.ID)
                                {
                                    db.PurchasingDocumentItems.Remove(childDatabasePurchasingDocumentItem);
                                }
                            }

                            List<Notification> notifications = db.Notifications.Where(x => x.PurchasingDocumentItem.ParentID == item.ID || x.PurchasingDocumentItemID == item.ID).ToList();
                            foreach (var childDatabasePurchasingDocumentItem in notifications)
                            {
                                db.Notifications.Remove(childDatabasePurchasingDocumentItem);
                            }
                            // finish
                        }

                        db.Notifications.Add(notification);
                    }
                }

                //List<PurchasingDocumentItem> arrayDataChild = new List<PurchasingDocumentItem>();
                if (purchasingDocumentItemChilds != null)
                {
                    foreach (PurchasingDocumentItem item in purchasingDocumentItemChilds)
                    {
                        PurchasingDocumentItem parent = db.PurchasingDocumentItems.Where(x => x.ID == item.ID).FirstOrDefault();

                        Notification notificationChild = new Notification();

                        if (role == "vendor")
                        {
                            PurchasingDocumentItem purchasingDocumentItem = new PurchasingDocumentItem();
                            purchasingDocumentItem.POID = parent.POID;
                            purchasingDocumentItem.ItemNumber = parent.ItemNumber;
                            purchasingDocumentItem.Currency = parent.Currency;
                            purchasingDocumentItem.Quantity = parent.Quantity;
                            purchasingDocumentItem.NetPrice = parent.NetPrice;
                            purchasingDocumentItem.NetValue = parent.NetValue;
                            purchasingDocumentItem.Material = parent.Material;
                            purchasingDocumentItem.Description = parent.Description;
                            purchasingDocumentItem.ActiveStage = "1";
                            purchasingDocumentItem.ParentID = item.ParentID;
                            purchasingDocumentItem.ConfirmedQuantity = item.ConfirmedQuantity;
                            purchasingDocumentItem.ConfirmedDate = item.ConfirmedDate;
                            purchasingDocumentItem.WorkTime = item.WorkTime;
                            purchasingDocumentItem.LeadTimeItem = item.LeadTimeItem;
                            purchasingDocumentItem.Created = now;
                            purchasingDocumentItem.CreatedBy = User.Identity.Name;
                            purchasingDocumentItem.LastModified = now;
                            purchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                            db.PurchasingDocumentItems.Add(purchasingDocumentItem);
                            //arrayDataChild.Add(purchasingDocumentItem);
                            
                            notificationChild.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                            notificationChild.StatusID = 3;
                            notificationChild.Stage = "1";
                            notificationChild.Role = "procurement";
                        }
                        else
                        {
                            Notification Existed_notificationChild = db.Notifications.Where(x => x.PurchasingDocumentItem.ParentID == item.ParentID).FirstOrDefault();
                            if (Existed_notificationChild != null)
                            {
                                Existed_notificationChild.isActive = false;
                                Existed_notificationChild.Modified = now;
                                Existed_notificationChild.ModifiedBy = User.Identity.Name;
                            }

                            PurchasingDocumentItem Existed_child = db.PurchasingDocumentItems.Where(x => x.ID == item.ID).FirstOrDefault();
                            SubcontComponentCapability scc = db.SubcontComponentCapabilities.Where(x => x.VendorCode == item.PO.VendorCode && x.Material == item.Material).FirstOrDefault();
                            int totalItemGR = Existed_child.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_child.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;

                            Existed_child.ConfirmedItem = true;
                            
                            if (scc.isNeedSequence == true)
                            {
                                Existed_child.ActiveStage = "2";
                                notificationChild.Stage = "2";
                            }
                            else
                            {
                                if (item.ConfirmedQuantity > 0 && item.ConfirmedQuantity <= totalItemGR)
                                {
                                    Existed_child.ActiveStage = "6";
                                    notificationChild.Stage = "6";
                                }
                                else
                                {
                                    Existed_child.ActiveStage = "4";
                                    notificationChild.Stage = "2";
                                }
                            }
                            
                            notificationChild.Role = "vendor";
                        }

                        notificationChild.isActive = true;
                        notificationChild.Created = now;
                        notificationChild.CreatedBy = User.Identity.Name;
                        notificationChild.Modified = now;
                        notificationChild.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notificationChild);
                    }
                }

                db.SaveChanges();
                return Json(new { success = true, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SavePartialPurchasingDocumentItems(string role, List<PurchasingDocumentItem> purchasingDocumentItems)
        {
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                foreach (PurchasingDocumentItem item in purchasingDocumentItems)
                {
                    PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == item.ID).FirstOrDefault();

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = Existed_PDI.ID;
                    notification.StatusID = 3;

                    if (purchasingDocumentItems.First() == item)
                    {
                        if (role == "vendor")
                        {
                            Existed_PDI.ActiveStage = "1";
                            Existed_PDI.ConfirmedQuantity = item.ConfirmedQuantity;
                            Existed_PDI.ConfirmedDate = item.ConfirmedDate;

                            notification.Stage = "1";
                            notification.Role = "procurement";
                        }
                        else
                        {
                            SubcontComponentCapability scc = db.SubcontComponentCapabilities.Where(x => x.VendorCode == item.PO.VendorCode && x.Material == item.Material).FirstOrDefault();
                            int totalItemGR = Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;

                            Existed_PDI.ConfirmedItem = true;

                            if (scc.isNeedSequence == true)
                            {
                                Existed_PDI.ActiveStage = "2";
                                notification.Stage = "2";
                            }
                            else
                            {
                                if (item.ConfirmedQuantity > 0 && item.ConfirmedQuantity <= totalItemGR)
                                {
                                    Existed_PDI.ActiveStage = "6";
                                    notification.Stage = "6";
                                }
                                else
                                {
                                    Existed_PDI.ActiveStage = "4";
                                    notification.Stage = "4";
                                }
                            }
                            
                            notification.Role = "vendor";
                        }
                        Existed_PDI.LastModified = now;
                        Existed_PDI.LastModifiedBy = User.Identity.Name;

                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        if (role == "vendor")
                        {
                            // Child clean-up
                            List<PurchasingDocumentItem> childDatabasePurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == item.ID).ToList();
                            foreach (var childDatabasePurchasingDocumentItem in childDatabasePurchasingDocumentItems)
                            {
                                if (childDatabasePurchasingDocumentItem.ID != item.ID)
                                {
                                    db.PurchasingDocumentItems.Remove(childDatabasePurchasingDocumentItem);
                                }
                            }

                            List<Notification> notifications = db.Notifications.Where(x => x.PurchasingDocumentItem.ParentID == item.ID || x.PurchasingDocumentItemID == item.ID).ToList();
                            foreach (var childDatabasePurchasingDocumentItem in notifications)
                            {
                                db.Notifications.Remove(childDatabasePurchasingDocumentItem);
                            }
                            // finish
                        }

                        db.Notifications.Add(notification);
                    }
                    else
                    {
                        if (role == "vendor")
                        {
                            Notification notificationChild = new Notification();

                            PurchasingDocumentItem purchasingDocumentItem = new PurchasingDocumentItem();
                            purchasingDocumentItem.POID = Existed_PDI.POID;
                            purchasingDocumentItem.ItemNumber = Existed_PDI.ItemNumber;
                            purchasingDocumentItem.Currency = Existed_PDI.Currency;
                            purchasingDocumentItem.Quantity = Existed_PDI.Quantity;
                            purchasingDocumentItem.NetPrice = Existed_PDI.NetPrice;
                            purchasingDocumentItem.NetValue = Existed_PDI.NetValue;
                            purchasingDocumentItem.Material = Existed_PDI.Material;
                            purchasingDocumentItem.Description = Existed_PDI.Description;
                            purchasingDocumentItem.ActiveStage = "1";
                            purchasingDocumentItem.ParentID = item.ParentID;
                            purchasingDocumentItem.ConfirmedQuantity = item.ConfirmedQuantity;
                            purchasingDocumentItem.ConfirmedDate = item.ConfirmedDate;
                            purchasingDocumentItem.WorkTime = item.WorkTime;
                            purchasingDocumentItem.LeadTimeItem = item.LeadTimeItem;
                            purchasingDocumentItem.Created = now;
                            purchasingDocumentItem.CreatedBy = User.Identity.Name;
                            purchasingDocumentItem.LastModified = now;
                            purchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                            db.PurchasingDocumentItems.Add(purchasingDocumentItem);

                            notificationChild.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                            notificationChild.StatusID = 3;
                            notificationChild.Stage = "1";
                            notificationChild.Role = "procurement";
                            notificationChild.isActive = true;
                            notificationChild.Created = now;
                            notificationChild.CreatedBy = User.Identity.Name;
                            notificationChild.Modified = now;
                            notificationChild.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notificationChild);
                        }
                    }
                }
                db.SaveChanges();
                return Json(new { success = true, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ViewBag.Exception(ex.Message + ex.StackTrace);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult SavePOItem(string role, int pdItemID, int confirmedItemQty, DateTime confirmedDate, bool isCanceledPOItem)
        {
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).FirstOrDefault();

                Notification notification = new Notification();
                notification.PurchasingDocumentItemID = Existed_PDI.ID;

                if (isCanceledPOItem)
                {
                    Existed_PDI.ConfirmedItem = false;
                    notification.StatusID = 2;
                    notification.Stage = "1";
                    notification.Role = role != "vendor" ? role : "procurement";
                }
                else
                {
                    notification.StatusID = 3;
                    if (role == "vendor")
                    {
                        Existed_PDI.ActiveStage = "1";
                        Existed_PDI.ConfirmedDate = confirmedDate;
                        Existed_PDI.ConfirmedQuantity = confirmedItemQty;

                        notification.Stage = "1";
                        notification.Role = "procurement";
                    }
                    else
                    {
                        SubcontComponentCapability scc = db.SubcontComponentCapabilities.Where(x => x.VendorCode == Existed_PDI.PO.VendorCode && x.Material == Existed_PDI.Material).FirstOrDefault();
                        int totalItemGR = Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;

                        Existed_PDI.ConfirmedItem = true;

                        if (scc.isNeedSequence == true)
                        {
                            Existed_PDI.ActiveStage = "2";
                            notification.Stage = "2";
                        }
                        else
                        {
                            if (Existed_PDI.ConfirmedQuantity > 0 && Existed_PDI.ConfirmedQuantity <= totalItemGR)
                            {
                                Existed_PDI.ActiveStage = "5";
                                notification.Stage = "5";
                            }
                            else
                            {
                                Existed_PDI.ActiveStage = "4";
                                notification.Stage = "2";
                            }
                        }
                        
                        notification.Role = "vendor";
                    }
                }
                Existed_PDI.LastModified = now;
                Existed_PDI.LastModifiedBy = User.Identity.Name;

                notification.isActive = true;
                notification.Created = now;
                notification.CreatedBy = User.Identity.Name;
                notification.Modified = now;
                notification.ModifiedBy = User.Identity.Name;

                db.Notifications.Add(notification);

                db.SaveChanges();
                return Json(new { success = true, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ViewBag.Exception(ex.Message + ex.StackTrace);
                return View("Error");
            }
        }
        #endregion

        #region Stage 2
        [HttpPost]
        public ActionResult GetSequenceData(int pdItemID)
        {
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).FirstOrDefault();
                SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material && x.VendorCode == purchasingDocumentItem.PO.VendorCode).FirstOrDefault();
                bool isDisabled = false;
                bool isEditable = false;
                int pb = 0;
                int setting = 0;
                int fullweld = 0;
                int primer = 0;
                if (purchasingDocumentItem.PB.HasValue || purchasingDocumentItem.Setting.HasValue || purchasingDocumentItem.Fullweld.HasValue || purchasingDocumentItem.Primer.HasValue)
                {
                    pb = purchasingDocumentItem.PB.HasValue ? purchasingDocumentItem.PB.Value : 0;
                    setting = purchasingDocumentItem.Setting.HasValue ? purchasingDocumentItem.Setting.Value : 0;
                    fullweld = purchasingDocumentItem.Fullweld.HasValue ? purchasingDocumentItem.Fullweld.Value : 0;
                    primer = purchasingDocumentItem.Primer.HasValue ? purchasingDocumentItem.Primer.Value : 0;
                }
                else if (subcontComponentCapability != null)
                {
                    pb = subcontComponentCapability.PB;
                    setting = subcontComponentCapability.Setting;
                    fullweld = subcontComponentCapability.Fullweld;
                    primer = subcontComponentCapability.Primer;
                }

                int leadTime = subcontComponentCapability != null ? (subcontComponentCapability.PB + subcontComponentCapability.Setting + subcontComponentCapability.Fullweld + subcontComponentCapability.Primer) : 0;


                if (purchasingDocumentItem.ActiveStage == "3" && purchasingDocumentItem.PBActualDate == null)
                {
                    isEditable = true;
                }

                if (purchasingDocumentItem.ActiveStage == "3")
                {
                    isDisabled = true;
                }


                if (subcontComponentCapability != null)
                {
                    return Json(new { success = true, responseCode = "200", responseText = "OK", arrayDataTime = new { LeadTime = leadTime ,PB = pb, Setting = setting, Fullweld = fullweld, Primer = primer }, isDisabled = isDisabled, isEditable = isEditable }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseCode = "404", responseText = "Not Found", arrayDataTime = new { LeadTime = leadTime }, isDisabled = isDisabled, isEditable = isEditable }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                ViewBag.Exception(ex.Message + ex.StackTrace);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult SaveSequenceData(int pdItemID, int pb, int setting, int fullweld, int primer)
        {
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).FirstOrDefault();
                //SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material).FirstOrDefault();

                Existed_PDI.ActiveStage = "3";
                Existed_PDI.PB = pb;
                Existed_PDI.Setting = setting;
                Existed_PDI.Fullweld = fullweld;
                Existed_PDI.Primer = primer;
                Existed_PDI.LastModified = now;
                Existed_PDI.LastModifiedBy = User.Identity.Name;
                db.SaveChanges();
                return Json(new { success = true, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ViewBag.Exception(ex.Message + ex.StackTrace);
                return View("Error");
            }
        }
        #endregion

        #region Stage 3
        [HttpPost]
        public ActionResult GetProgressData(int pdItemID)
        {
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).FirstOrDefault();
                //double primerDays = Convert.ToDouble(purchasingDocumentItem.PB) * -1;

                SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material && x.VendorCode == purchasingDocumentItem.PO.VendorCode).FirstOrDefault();
                bool isDisabled = false;
                bool isEditable = false;
                int pb = purchasingDocumentItem.PB.HasValue ? purchasingDocumentItem.PB.Value : 0;
                int setting = purchasingDocumentItem.PB.HasValue ? purchasingDocumentItem.PB.Value : 0;
                int fullweld = purchasingDocumentItem.PB.HasValue ? purchasingDocumentItem.PB.Value : 0;
                int primer = purchasingDocumentItem.PB.HasValue ? purchasingDocumentItem.PB.Value : 0;

                int leadTime = subcontComponentCapability != null ? (subcontComponentCapability.PB + subcontComponentCapability.Setting + subcontComponentCapability.Fullweld + subcontComponentCapability.Primer) : 0;
                
                int fullweldDays = primer;
                int settingDays = fullweld + fullweldDays;
                int pbDays = setting + settingDays;

                string primerDate = purchasingDocumentItem.ConfirmedDate.HasValue ? purchasingDocumentItem.ConfirmedDate.Value.ToString("dd/MM/yyyy") : "";
                string fullweldDate = purchasingDocumentItem.ConfirmedDate.HasValue ? purchasingDocumentItem.ConfirmedDate.Value.AddDays(fullweldDays * -1).ToString("dd/MM/yyyy") : "";
                string settingDate = purchasingDocumentItem.ConfirmedDate.HasValue ? purchasingDocumentItem.ConfirmedDate.Value.AddDays(settingDays * -1).ToString("dd/MM/yyyy") : "";
                string pbDate = purchasingDocumentItem.ConfirmedDate.HasValue ? purchasingDocumentItem.ConfirmedDate.Value.AddDays(pbDays * -1).ToString("dd/MM/yyyy") : "";

                string ATAPB = purchasingDocumentItem.PBActualDate.HasValue ? purchasingDocumentItem.PBActualDate.Value.ToString("dd/MM/yyyy") : "";
                string ATASetting = purchasingDocumentItem.SettingActualDate.HasValue ? purchasingDocumentItem.SettingActualDate.Value.ToString("dd/MM/yyyy") : "";
                string ATAFullweld = purchasingDocumentItem.FullweldActualDate.HasValue ? purchasingDocumentItem.FullweldActualDate.Value.ToString("dd/MM/yyyy") : "";
                string ATAPrimer = purchasingDocumentItem.PrimerActualDate.HasValue ? purchasingDocumentItem.PrimerActualDate.Value.ToString("dd/MM/yyyy") : "";

                int ATAPBReasonID = purchasingDocumentItem.PBALateReasonID.HasValue ? purchasingDocumentItem.PBALateReasonID.Value : 0;
                int ATASettingReasonID = purchasingDocumentItem.SettingLateReasonID.HasValue ? purchasingDocumentItem.SettingLateReasonID.Value : 0;
                int ATAFullweldReasonID = purchasingDocumentItem.FullweldLateReasonID.HasValue ? purchasingDocumentItem.FullweldLateReasonID.Value : 0;
                int ATAPrimerReasonID = purchasingDocumentItem.PremierLateReasonID.HasValue ? purchasingDocumentItem.PremierLateReasonID.Value : 0;


                if (purchasingDocumentItem != null)
                {
                    return Json(new { success = true, responseCode = "200", responseText = "OK", arrayDataTime = new { LeadTime = leadTime, PBDays = pb, SettingDays = setting, FullweldDays = fullweld, PrimerDays = primer, PB = pbDate, Setting = settingDate, Fullweld = fullweldDate, Primer = primerDate, ATAPB = ATAPB, ATASetting = ATASetting, ATAFullweld = ATAFullweld, ATAPrimer = ATAPrimer, ATAPBReasonID = ATAPBReasonID, ATASettingReasonID = ATASettingReasonID, ATAFullweldReasonID = ATAFullweldReasonID, ATAPrimerReasonID = ATAPrimerReasonID} }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseCode = "404", responseText = "Not Found", arrayDataTime = new { LeadTime = leadTime } }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Exception(ex.Message + ex.StackTrace);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult SaveSequencesProgress(int pdItemID, DateTime? PBActualDate, DateTime? settingActualDate, DateTime? fullweldActualDate, DateTime? primerActualDate, string PBActualReason, string settingActualReason, string fullweldActualReason, string primerActualReason, HttpPostedFileBase[] invoices)
        {
            //HttpPostedFileBase file = Request.Files["FileUpload"];
            try
            {
                PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).SingleOrDefault();
                int totalItemGR = Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;
                //List<ProgressPhoto> Existed_Attachments = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == pdItemID).ToList();

                //SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material).FirstOrDefault();

                foreach (var invoice in invoices)
                {
                    var processName = "";

                    if (settingActualDate == null)
                    {
                        processName = "PB";
                    }
                    else if (fullweldActualDate == null)
                    {
                        processName = "Setting";
                    }
                    else if (primerActualDate == null)
                    {
                        processName = "Fullweld";
                    }
                    else
                    {
                        processName = "Primer";
                    }
                    string fileName = $"{pdItemID.ToString()}_{processName}_{Path.GetFileName(invoice.FileName)}";
                    string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Subcont/SequencesProgress"), fileName);
                    ProgressPhoto progress = new ProgressPhoto();
                    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                    {
                        invoice.InputStream.CopyTo(fileStream);
                    }

                    progress.PurchasingDocumentItemID = pdItemID;
                    progress.FileName = fileName;
                    progress.Created = now;
                    progress.CreatedBy = User.Identity.Name;
                    progress.LastModified = now;
                    progress.LastModifiedBy = User.Identity.Name;
                    progress.ProcessName = processName;
                    
                    db.ProgressPhotoes.Add(progress);
                }

                Existed_PDI.PBActualDate = PBActualDate;
                Existed_PDI.SettingActualDate = settingActualDate;
                Existed_PDI.FullweldActualDate = fullweldActualDate;
                Existed_PDI.PrimerActualDate = primerActualDate;
                //Existed_PDI.PBALateReason = PBActualReason == "null"? null: PBActualReason;
                //Existed_PDI.SettingLateReason = settingActualReason == "null" ? null : settingActualReason;
                //Existed_PDI.FullweldLateReason = fullweldActualReason == "null" ? null : fullweldActualReason;
                //Existed_PDI.PremierLateReason = primerActualReason == "null" ? null : primerActualReason;
                Existed_PDI.LastModified = now;
                Existed_PDI.LastModifiedBy = User.Identity.Name;

                //if (Existed_PDIHistory != null)
                //{
                if (Existed_PDI.ConfirmedQuantity == totalItemGR && (Existed_PDI.PBActualDate != null && Existed_PDI.SettingActualDate != null && Existed_PDI.FullweldActualDate != null && Existed_PDI.PrimerActualDate != null))
                {
                    Existed_PDI.ActiveStage = "6";
                }
                else if(Existed_PDI.PBActualDate != null && Existed_PDI.SettingActualDate != null && Existed_PDI.FullweldActualDate != null && Existed_PDI.PrimerActualDate != null)
                {
                    Existed_PDI.ActiveStage = "4";
                }
                //}

                db.SaveChanges();
                return Json(new { success = true, responseCode = "200", responseText = "data updated" }, JsonRequestBehavior.AllowGet);

                //if (fileSequencesProgress.ContentLength > 0)
                //{
                //    string user = User.Identity.Name;

                //    string fileName = $"{pdItemID.ToString()}_{Path.GetFileName(fileSequencesProgress.FileName)}";
                //    string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Subcont/SequencesProgress"), fileName);

                //    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                //    {
                //        fileSequencesProgress.InputStream.CopyTo(fileStream);
                //    }

                //    //databasePurchasingDocumentItem.ProformaInvoiceDocument = fileName;
                //    //databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = null;
                //    //databasePurchasingDocumentItem.LastModified = now;
                //    //databasePurchasingDocumentItem.LastModifiedBy = user;

                //    //db.SaveChanges();

                //    //return Json(new { responseText = $"Item number {databasePurchasingDocumentItem.ItemNumber} affected" }, JsonRequestBehavior.AllowGet);
                //    return Json(new { success = true, responseCode = "200", responseText = "data updated" }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    //return Json(new { responseText = $"There is no File" }, JsonRequestBehavior.AllowGet);
                //    return Json(new { success = true, responseCode = "404", responseText = "not found" }, JsonRequestBehavior.AllowGet);
                //}
            }
            catch (Exception ex)
            {
                //string errorMessage = ex.Message + " --- " + ex.StackTrace;
                //return View(errorMessage);
                return Json(new { success = true, responseCode = "400", responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Stage 4

        [HttpPost]
        public ActionResult SaveInvoiceMethod(int pdItemID, string invoiceMethod)
        {
            try
            {
                PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).SingleOrDefault();
                List<PurchasingDocumentItem> Existed_PDIChilds = db.PurchasingDocumentItems.Where(x => x.ParentID == pdItemID).ToList();
                
                Existed_PDI.InvoiceMethod = invoiceMethod;
                Existed_PDI.LastModified = now;
                Existed_PDI.LastModifiedBy = User.Identity.Name;

                foreach (var existed_PDIChild in Existed_PDIChilds)
                {
                    existed_PDIChild.InvoiceMethod = invoiceMethod;
                    existed_PDIChild.LastModified = now;
                    existed_PDIChild.LastModifiedBy = User.Identity.Name;
                }
                
                db.SaveChanges();
                return Json(new { success = true, responseCode = "200", responseText = "data updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = true, responseCode = "200", responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Stage 6
        [HttpPost]
        public ActionResult VendorUploadInvoice(int inputPurchasingDocumentItemID, HttpPostedFileBase fileInvoice)
        {
            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStageToNumber > 3 && databasePurchasingDocumentItem.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity >= databasePurchasingDocumentItem.ConfirmedQuantity)
                {
                    if (fileInvoice.ContentLength > 0)
                    {
                        if (databasePurchasingDocumentItem.InvoiceDocument == null)
                        {

                            string user = User.Identity.Name;

                            string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{Path.GetFileName(fileInvoice.FileName)}";
                            string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Subcont/Invoice"), fileName);

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

                            string downloadUrl = Path.Combine("..\\Files\\Subcont\\Invoice", fileName);

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
            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                //if (databasePurchasingDocumentItem.ActiveStage != "2a")
                //{
                    if (databasePurchasingDocumentItem.ActiveStageToNumber > 3)
                    {
                        if (databasePurchasingDocumentItem.InvoiceDocument != null)
                        {
                            string user = User.Identity.Name;

                            string pathWithfileName = Path.Combine(Server.MapPath("~/Files/Subcont/Invoice"), databasePurchasingDocumentItem.InvoiceDocument);

                            System.IO.File.Delete(pathWithfileName);

                            databasePurchasingDocumentItem.InvoiceDocument = null;
                            databasePurchasingDocumentItem.LastModified = now;
                            databasePurchasingDocumentItem.LastModifiedBy = user;

                            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID && x.StatusID==3).ToList();
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
                            return Json(new { responseText = $"File not uploaded 4" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { responseText = $"File not uploaded 5" }, JsonRequestBehavior.AllowGet);
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
    }
}