using Newtonsoft.Json;
using PagedList;
using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleVendor + "," + LoginConstants.RoleProcurement)]
    public class ImportController : Controller
    {

        private POTrackingEntities db = new POTrackingEntities();
        private DateTime now = DateTime.Now;
        private string iisAppName = WebConfigurationManager.AppSettings["IISAppName"];

        // GET: Import
        public ActionResult Index(string searchPONumber, string searchVendorName, string searchMaterial, string searchStartPODate, string searchEndPODate, int? page)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles.ToLower();
            var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();

            if (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "local")
            {
                return RedirectToAction("Index", "Local");
            }
            if (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "subcont")
            {
                return RedirectToAction("Index", "SubCont");
            }
            if (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower())
            {
                return RedirectToAction("Index", "SubCont");
            }

            var pOes = db.POes.Include(x => x.PurchasingDocumentItems)
                            .Where(x => x.Type.ToLower() == "zo04" || x.Type.ToLower() == "zo07" || x.Type.ToLower() == "zo08")
                            .Where(x => x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material)))
                            .AsQueryable();

            if (role == LoginConstants.RoleProcurement.ToLower())
            {
                pOes = pOes.Include(x => x.PurchasingDocumentItems)
                                .Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null))
                                .AsQueryable();

                List<string> myUserNRPs = new List<string>();
                myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
                myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

                var noShowPOes = db.POes.Where(x => x.Type.ToLower() == "zo04" || x.Type.ToLower() == "zo07" || x.Type.ToLower() == "zo08")
                                        .Where(x => x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material)))
                                        .Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null));

                if (myUserNRPs.Count > 0)
                {
                    foreach (var myUserNRP in myUserNRPs)
                    {
                        noShowPOes = noShowPOes.Where(x => x.CreatedBy != myUserNRP);
                    }
                }

                pOes = pOes.Except(noShowPOes);
            }
            else if (role == LoginConstants.RoleAdministrator.ToLower())
            {
                pOes = pOes.Include(x => x.PurchasingDocumentItems)
                                .Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null))
                                .AsQueryable();
            }
            else
            {
                pOes = pOes.Where(x => x.VendorCode == db.UserVendors.Where(y => y.Username == myUser.UserName).FirstOrDefault().VendorCode);
            }

            ViewBag.CurrentSearchPONumber = searchPONumber;
            ViewBag.CurrentSearchVendorName = searchVendorName;
            ViewBag.CurrentSearchMaterial = searchMaterial;
            ViewBag.CurrentStartPODate = searchStartPODate;
            ViewBag.CurrentEndPODate = searchEndPODate;
            ViewBag.CurrentRoleID = role.ToLower();
            ViewBag.IISAppName = iisAppName;

            List<DelayReason> delayReasons = db.DelayReasons.ToList();

            ViewBag.DelayReasons = delayReasons;

            #region Filter
            if (!String.IsNullOrEmpty(searchPONumber))
            {
                pOes = pOes.Where(x => x.Number.ToLower().Contains(searchPONumber.ToLower()));
            }

            if (!String.IsNullOrEmpty(searchVendorName))
            {
                pOes = pOes.Where(x => x.Vendor.Name.ToLower().Contains(searchVendorName.ToLower()));
            }

            if (!String.IsNullOrEmpty(searchMaterial))
            {
                pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.Material.ToLower().Contains(searchMaterial.ToLower()) || y.Description.ToLower().Contains(searchMaterial.ToLower())));
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

            return View(pOes.OrderBy(x => x.Number).ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
        }

        [HttpGet]
        public JsonResult GetDataForSearch(string searchFilterBy, string value)
        {
            try
            {
                object data = null;
                value = value.ToLower();

                IEnumerable<Vendor> vendors = db.Vendors.Where(x => x.POes.Any(y => y.Type.ToLower() == "zo04" || y.Type.ToLower() == "zo07" || y.Type.ToLower() == "zo08"));
                IEnumerable<PurchasingDocumentItem> purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.PO.Type.ToLower() == "zo04" || x.PO.Type.ToLower() == "zo07" || x.PO.Type.ToLower() == "zo08");


                if (searchFilterBy == "poNumber")
                {
                    data = db.POes.Where(x => x.Number.Contains(value) && (x.Type.ToLower() == "zo04" || x.Type.ToLower() == "zo07" || x.Type.ToLower() == "zo08")).Select(x =>
                     new
                     {
                         Data = x.Number,
                         MatchEvaluation = x.Number.ToLower().IndexOf(value)
                     }).Distinct().OrderBy(x => x.MatchEvaluation).Take(10);
                }
                else if (searchFilterBy == "vendor")
                {
                    data = vendors.Where(x => x.Name.ToLower().Contains(value.ToLower())).Select(x =>
                    new
                    {
                        Data = x.Name,
                        MatchEvaluation = x.Name.ToLower().IndexOf(value)
                    }).Distinct().OrderBy(x => x.MatchEvaluation).Take(10);
                }
                else if (searchFilterBy == "material")
                {
                    data = purchasingDocumentItems.Where(x => x.Material.ToLower().Contains(value.ToLower()) || x.Description.ToLower().Contains(value.ToLower())).Select(x =>
                    new
                    {
                        Data = x.Material.ToLower().StartsWith(value) ? x.Material : x.Description.ToLower().StartsWith(value) ? x.Description : x.Material.ToLower().Contains(value) ? x.Material : x.Description,
                        MatchEvaluation = (x.Material.ToLower().StartsWith(value) ? 1 : 0) + (x.Description.ToLower().StartsWith(value) ? 1 : 0)
                    }).Distinct().OrderByDescending(x => x.MatchEvaluation).Take(10);
                }

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

        public string GetNRPByUsername(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                SearchResult sResultSet;

                string domain = WebConfigurationManager.AppSettings["ActiveDirectoryUrl"];
                string ldapUser = WebConfigurationManager.AppSettings["ADUsername"];
                string ldapPassword = WebConfigurationManager.AppSettings["ADPassword"];
                using (DirectoryEntry entry = new DirectoryEntry(domain, ldapUser, ldapPassword))
                {
                    DirectorySearcher dSearch = new DirectorySearcher(entry);
                    dSearch.Filter = "(&(objectClass=user)(samaccountname=" + username + "))";
                    sResultSet = dSearch.FindOne();
                }

                try
                {
                    string description = sResultSet.Properties["description"][0].ToString();
                    return description;
                }
                catch (Exception)
                {
                    return "-";
                }
            }

            return null;
        }

        public List<string> GetChildNRPsByUsername(string username)
        {
            List<string> userNRPs = new List<string>();

            if (!string.IsNullOrEmpty(username))
            {
                UserProcurementSuperior userProcurementSuperior = db.UserProcurementSuperiors.Where(x => x.Username.ToLower() == username.ToLower()).SingleOrDefault();

                if (userProcurementSuperior != null)
                {
                    List<UserProcurementSuperior> childUsers = db.UserProcurementSuperiors.Where(x => x.ParentID == userProcurementSuperior.ID).ToList();

                    foreach (var childUser in childUsers)
                    {
                        foreach (var item in db.UserProcurementSuperiors)
                        {
                            if (item.ParentID == childUser.ID)
                            {
                                userNRPs.Add(item.NRP);
                            }
                        }

                        userNRPs.Add(childUser.NRP);
                    }
                }
            }

            return userNRPs;
        }

        #region STAGE 1

        [HttpPost]
        public ActionResult VendorConfirmItem(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
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
                        List<PurchasingDocumentItem> childPurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == inputPurchasingDocumentItem.ID && x.ID != inputPurchasingDocumentItem.ID).ToList();

                        if (childPurchasingDocumentItems.Count > 0)
                        {
                            if (childPurchasingDocumentItems.Any(x => x.ActiveStage != "1"))
                            {
                                return Json(new { responseText = $"Cannot edit progressed data" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                //Child Clean Up
                                foreach (var childPurchasingDocumentItem in childPurchasingDocumentItems)
                                {
                                    if (childPurchasingDocumentItem.ID != inputPurchasingDocumentItem.ID)
                                    {
                                        List<Notification> notifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == childPurchasingDocumentItem.ID).ToList();

                                        foreach (var notification in notifications)
                                        {
                                            db.Notifications.Remove(notification);
                                        }

                                        db.PurchasingDocumentItems.Remove(childPurchasingDocumentItem);
                                    }
                                }
                            }
                        }

                        databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ID).FirstOrDefault();

                        if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0")
                        {
                            databasePurchasingDocumentItem.ConfirmedQuantity = inputPurchasingDocumentItem.ConfirmedQuantity;
                            databasePurchasingDocumentItem.ConfirmedDate = inputPurchasingDocumentItem.ConfirmedDate;
                            databasePurchasingDocumentItem.LastModified = now;
                            databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                            counter++;

                            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                            foreach (var previousNotification in previousNotifications)
                            {
                                previousNotification.isActive = false;
                            }

                            if (inputPurchasingDocumentItem.ConfirmedQuantity == databasePurchasingDocumentItem.Quantity && inputPurchasingDocumentItem.ConfirmedDate == databasePurchasingDocumentItem.DeliveryDate)
                            {
                                databasePurchasingDocumentItem.ConfirmedItem = true;
                                databasePurchasingDocumentItem.ActiveStage = "2";
                                isSameAsProcs.Add(true);

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
                            else
                            {
                                databasePurchasingDocumentItem.ConfirmedItem = null;
                                databasePurchasingDocumentItem.ActiveStage = "1";
                                isSameAsProcs.Add(false);

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
                            //inputPurchasingDocumentItem.NetValue = databasePurchasingDocumentItem.NetValue;
                            //inputPurchasingDocumentItem.WorkTime = databasePurchasingDocumentItem.WorkTime;
                            //inputPurchasingDocumentItem.DeliveryDate = databasePurchasingDocumentItem.DeliveryDate;

                            inputPurchasingDocumentItem.ActiveStage = "1";
                            inputPurchasingDocumentItem.Created = now;
                            inputPurchasingDocumentItem.CreatedBy = User.Identity.Name;
                            inputPurchasingDocumentItem.LastModified = now;
                            inputPurchasingDocumentItem.LastModifiedBy = User.Identity.Name;

                            int idNewPDI = db.PurchasingDocumentItems.Add(inputPurchasingDocumentItem).ID;

                            Notification notification = new Notification();
                            notification.PurchasingDocumentItemID = idNewPDI;
                            notification.StatusID = 3;
                            notification.Stage = "1";
                            notification.Role = "procurement";
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);

                            counter++;
                        }
                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{counter} Item succesfully affected", isSameAsProcs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public ActionResult VendorEditItem(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        //{
        //    CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
        //    if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
        //    {
        //        return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
        //    }

        //    if (inputPurchasingDocumentItems == null)
        //    {
        //        return Json(new { responseText = $"No Item affected" }, JsonRequestBehavior.AllowGet);
        //    }

        //    DateTime now = DateTime.Now;
        //    int counter = 0;
        //    List<bool> isSameAsProcs = new List<bool>();

        //    try
        //    {
        //        foreach (var inputPurchasingDocumentItem in inputPurchasingDocumentItems)
        //        {
        //            PurchasingDocumentItem databasePurchasingDocumentItem = new PurchasingDocumentItem();
        //            List<PurchasingDocumentItem> childDatabasePurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == inputPurchasingDocumentItem.ID && x.ID != inputPurchasingDocumentItem.ID).ToList();

        //            if (childDatabasePurchasingDocumentItems.Count > 0)
        //            {
        //                if (childDatabasePurchasingDocumentItems.Any(x => x.ActiveStage != "1"))
        //                {
        //                    return Json(new { responseText = $"Cannot edit progressed data" }, JsonRequestBehavior.AllowGet);
        //                }
        //            }

        //            if (!inputPurchasingDocumentItem.ParentID.HasValue)
        //            {
        //                // Child clean-up

        //                if (childDatabasePurchasingDocumentItems.Count > 0)
        //                {
        //                    foreach (var childDatabasePurchasingDocumentItem in childDatabasePurchasingDocumentItems)
        //                    {
        //                        if (childDatabasePurchasingDocumentItem.ID != inputPurchasingDocumentItem.ID)
        //                        {
        //                            db.PurchasingDocumentItems.Remove(childDatabasePurchasingDocumentItem);
        //                        }
        //                    }
        //                }
        //                // finish

        //                databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ID).FirstOrDefault();

        //                if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0")
        //                {
        //                    //databasePurchasingDocumentItem.ParentID = databasePurchasingDocumentItem.ID;
        //                    databasePurchasingDocumentItem.ConfirmedQuantity = inputPurchasingDocumentItem.ConfirmedQuantity;
        //                    databasePurchasingDocumentItem.ConfirmedDate = inputPurchasingDocumentItem.ConfirmedDate;
        //                    databasePurchasingDocumentItem.LastModified = now;
        //                    databasePurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
        //                    counter++;

        //                    if (inputPurchasingDocumentItem.ConfirmedQuantity == databasePurchasingDocumentItem.Quantity && inputPurchasingDocumentItem.ConfirmedDate == databasePurchasingDocumentItem.DeliveryDate)
        //                    {
        //                        databasePurchasingDocumentItem.ConfirmedItem = true;
        //                        databasePurchasingDocumentItem.ActiveStage = "2";
        //                        isSameAsProcs.Add(true);
        //                    }
        //                    else
        //                    {
        //                        databasePurchasingDocumentItem.ConfirmedItem = null;
        //                        databasePurchasingDocumentItem.ActiveStage = "1";
        //                        isSameAsProcs.Add(false);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                databasePurchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == inputPurchasingDocumentItem.ParentID).FirstOrDefault();

        //                if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0")
        //                {
        //                    inputPurchasingDocumentItem.POID = databasePurchasingDocumentItem.POID;
        //                    inputPurchasingDocumentItem.ItemNumber = databasePurchasingDocumentItem.ItemNumber;
        //                    inputPurchasingDocumentItem.Material = databasePurchasingDocumentItem.Material;
        //                    inputPurchasingDocumentItem.Description = databasePurchasingDocumentItem.Description;
        //                    inputPurchasingDocumentItem.NetPrice = databasePurchasingDocumentItem.NetPrice;
        //                    inputPurchasingDocumentItem.Currency = databasePurchasingDocumentItem.Currency;
        //                    inputPurchasingDocumentItem.Quantity = databasePurchasingDocumentItem.Quantity;
        //                    //inputPurchasingDocumentItem.NetValue = databasePurchasingDocumentItem.NetValue;
        //                    //inputPurchasingDocumentItem.WorkTime = databasePurchasingDocumentItem.WorkTime;
        //                    //inputPurchasingDocumentItem.DeliveryDate = databasePurchasingDocumentItem.DeliveryDate;
        //                    inputPurchasingDocumentItem.IsClosed = "";

        //                    inputPurchasingDocumentItem.ActiveStage = "1";
        //                    inputPurchasingDocumentItem.Created = now;
        //                    inputPurchasingDocumentItem.CreatedBy = User.Identity.Name;
        //                    inputPurchasingDocumentItem.LastModified = now;
        //                    inputPurchasingDocumentItem.LastModifiedBy = User.Identity.Name;

        //                    db.PurchasingDocumentItems.Add(inputPurchasingDocumentItem);
        //                    counter++;
        //                }
        //            }
        //        }

        //        db.SaveChanges();

        //        return Json(new { responseText = $"{counter} Item succesfully affected", isSameAsProcs }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMessage = (ex.Message + ex.StackTrace);
        //        return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]
        public ActionResult ProcurementConfirmItem(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleProcurement.ToLower())
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
                    databasePurchasingDocumentItem.ActiveStage = "1";

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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleProcurement.ToLower())
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

                    if (databasePurchasingDocumentItem.ActiveStage == "2" || (databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ProformaInvoiceDocument == null))
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
            if (myUser.Roles.ToLower() != LoginConstants.RoleProcurement.ToLower())
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

                    if (databasePurchasingDocumentItem.ActiveStage == "2" || (databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ProformaInvoiceDocument == null))
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
                    databasePurchasingDocumentItem.ActiveStage = "3";
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
                    notification.StatusID = 1;
                    notification.Stage = "2a";
                    notification.Role = "procurement";
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;

                    db.Notifications.Add(notification);

                    db.SaveChanges();

                    string downloadUrl = Path.Combine("/", iisAppName, "Files/Import/ProformaInvoice", fileName);

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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage == "2a" || databasePurchasingDocumentItem.ActiveStage == "3")
                {
                    string user = User.Identity.Name;

                    databasePurchasingDocumentItem.ActiveStage = "3";
                    databasePurchasingDocumentItem.ProformaInvoiceDocument = null;
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
                    notification.StatusID = 1;
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

        //// POST: Import/ProcurementApprovePI
        //[HttpPost]
        //public ActionResult ProcurementApprovePI([Bind(Include = "ID")] PurchasingDocumentItem inputPurchasingDocumentItem)
        //{
        //    CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
        //    if (myUser.Roles.ToLower() != "procurement")
        //    {
        //        return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
        //    }

        //    try
        //    {
        //        PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

        //        if (databasePurchasingDocumentItem.ActiveStage == "2a")
        //        {
        //            string user = User.Identity.Name;

        //            databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = true;
        //            databasePurchasingDocumentItem.ActiveStage = "3";
        //            databasePurchasingDocumentItem.LastModified = now;
        //            databasePurchasingDocumentItem.LastModifiedBy = user;

        //            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
        //            foreach (var previousNotification in previousNotifications)
        //            {
        //                previousNotification.isActive = false;
        //            }

        //            Notification notification = new Notification();
        //            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
        //            notification.StatusID = 3;
        //            notification.Stage = "2a";
        //            notification.Role = "vendor";
        //            notification.isActive = true;
        //            notification.Created = now;
        //            notification.CreatedBy = User.Identity.Name;
        //            notification.Modified = now;
        //            notification.ModifiedBy = User.Identity.Name;

        //            db.Notifications.Add(notification);

        //            db.SaveChanges();

        //            return Json(new { responseText = $"Proforma Invoice successfully accepted" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { responseText = $"Process failed" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMessage = (ex.Message + ex.StackTrace);
        //        return View(errorMessage);
        //    }
        //}

        //// POST: Import/ProcurementDisapprovePI
        //[HttpPost]
        //public ActionResult ProcurementDisapprovePI([Bind(Include = "ID")] PurchasingDocumentItem inputPurchasingDocumentItem)
        //{
        //    CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
        //    if (myUser.Roles.ToLower() != "procurement")
        //    {
        //        return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
        //    }

        //    try
        //    {
        //        PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

        //        if (databasePurchasingDocumentItem.ActiveStage == "2a")
        //        {
        //            string user = User.Identity.Name;

        //            string pathWithfileName = Path.Combine(Server.MapPath("~/Files/Import/ProformaInvoice"), databasePurchasingDocumentItem.ProformaInvoiceDocument);

        //            System.IO.File.Delete(pathWithfileName);

        //            databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = false;
        //            databasePurchasingDocumentItem.ProformaInvoiceDocument = null;
        //            databasePurchasingDocumentItem.ActiveStage = "2a";
        //            databasePurchasingDocumentItem.LastModified = now;
        //            databasePurchasingDocumentItem.LastModifiedBy = user;

        //            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
        //            foreach (var previousNotification in previousNotifications)
        //            {
        //                previousNotification.isActive = false;
        //            }

        //            Notification notification = new Notification();
        //            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
        //            notification.StatusID = 2;
        //            notification.Stage = "2a";
        //            notification.Role = "vendor";
        //            notification.isActive = true;
        //            notification.Created = now;
        //            notification.CreatedBy = User.Identity.Name;
        //            notification.Modified = now;
        //            notification.ModifiedBy = User.Identity.Name;

        //            db.Notifications.Add(notification);

        //            db.SaveChanges();

        //            return Json(new { responseText = $"Proforma Invoice successfully declined" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { responseText = $"Process failed" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMessage = (ex.Message + ex.StackTrace);
        //        return View(errorMessage);
        //    }
        //}

        #endregion

        #region STAGE 3

        // POST: Import/VendorConfirmPaymentReceived
        [HttpPost]
        public ActionResult VendorConfirmPaymentReceived(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
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
                        notification.StatusID = 1;
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage == "3" || databasePurchasingDocumentItem.ActiveStage == "4")
                {
                    string user = User.Identity.Name;

                    databasePurchasingDocumentItem.ConfirmReceivedPaymentDate = null;
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
                    notification.StatusID = 1;
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
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
                        notification.StatusID = 1;
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

                    return Json(new { responseText = $"1 data affected" }, JsonRequestBehavior.AllowGet);
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
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
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

            databasePurchasingDocumentItem.LastModified = now;
            databasePurchasingDocumentItem.LastModifiedBy = user;

            db.SaveChanges();

            List<ProgressPhoto> progressPhotoes = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == inputPurchasingDocumentItemID).ToList();
            List<string> imageSources = new List<string>();

            foreach (var progressPhoto in progressPhotoes)
            {
                string downloadurl = Path.Combine("/", iisAppName, "Files/Import/ProgressPhotos", progressPhoto.FileName);
                imageSources.Add(downloadurl);
            }

            return Json(new { responseText = $"Files successfully uploaded", imageSources }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region STAGE 5

        // POST: Import/VendorConfirmShipmentBookingDate
        [HttpPost]
        public ActionResult VendorConfirmShipmentBookingDate(List<Shipment> inputShipmentBookDates)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
                        notification.StatusID = 1;
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
                        notification.StatusID = 1;
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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

            return Json(new { responseText = $"1 data affected" }, JsonRequestBehavior.AllowGet);

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
                    string dokumenCopyBL = Path.Combine("/", iisAppName, "Files/Import/Shipping/CopyBL", shipment.CopyBLDocument);
                    string dokumenPackingList = Path.Combine("/", iisAppName, "Files/Import/Shipping/PackingList", shipment.PackingListDocument);
                    string dokumenInvoice = Path.Combine("/", iisAppName, "Files/Import/Shipping/Invoice", shipment.InvoiceDocument);

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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleProcurement.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
                        notification.StatusID = 1;
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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
                            notification.StatusID = 1;
                            notification.Stage = "9";
                            notification.Role = "procurement";
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);

                            db.SaveChanges();

                            string downloadUrl = Path.Combine("/", iisAppName, "Files/Import/Invoice", fileName);

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
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

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
                            notification.StatusID = 3;
                            notification.Stage = "9";
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
                            return Json(new { responseText = $"File not removed" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { responseText = $"File not removed" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { responseText = $"File not removed" }, JsonRequestBehavior.AllowGet);
                }
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
