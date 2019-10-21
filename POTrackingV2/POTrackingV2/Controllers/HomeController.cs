using Newtonsoft.Json;
using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleVendor + "," + LoginConstants.RoleProcurement + "," + LoginConstants.RoleSubcontDev)]
    public class HomeController : Controller
    {
        POTrackingEntities db = new POTrackingEntities();

        public ActionResult Index()
        {
            List<ViewModelUserManagement> userData = new List<ViewModelUserManagement>();


            //UserRole userRole = DBUser.UserRoles.FirstOrDefault(x => x.ID == userSalesTools.UserRoleID);

            //if (userRole != null)
            //{
            //    ViewModelUserManagement tmp = new ViewModelUserManagement
            //    {
            //        Name = userRole.User.Name,
            //        UserName = userRole.Username,
            //        RoleID = userRole.RoleID,
            //        RoleName = userRole.Role.Name,
            //    };

            //    userData.Add(tmp);
            //}

            var myRole = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);

            var roleType = db.UserRoleTypes.Where(x => x.Username == myRole.UserName).FirstOrDefault();
            if (myRole.Roles.ToLower() == LoginConstants.RoleVendor.ToLower())
            {
                if (roleType.RolesType.Name.ToLower() == "import")
                {
                    return RedirectToAction("Index", "Import");
                }
                else if (roleType.RolesType.Name.ToLower() == "local")
                {
                    return RedirectToAction("Index", "Local");
                }
                else if (roleType.RolesType.Name.ToLower() == "subcont")
                {
                    return RedirectToAction("Index", "Subcont");
                }
                else
                {
                    return View(userData);
                }
            }
            else if (myRole.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower())
            {
                return RedirectToAction("Index", "Import");
            }
            else if (myRole.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower())
            {
                return RedirectToAction("Index", "Subcont");
            }
            else if (myRole.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower())
            {
                return RedirectToAction("Index", "MasterVendor");
            }
            else
            {
                return View(userData);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult UserManagement()
        {
            ViewBag.Message = "Your user management page";

            return View();
        }

        public ActionResult AddUser()
        {
            ViewBag.Message = "Your add user page";

            return View();
        }

        public ActionResult UserDetails()
        {
            ViewBag.Message = "Your user details";

            return View();
        }

        public ActionResult EditUser()
        {
            ViewBag.Message = "Your edit user page";

            return View();
        }

        public ActionResult UserManagementLogin()
        {
            ViewBag.Message = "Your login page";

            return View();
        }

        [HttpPost]
        public ActionResult DeleteNotification(int notificationID)
        {
            DateTime now = DateTime.Now;
            try
            {
                Notification existedNotification = db.Notifications.Where(x => x.ID == notificationID).FirstOrDefault();
                if (existedNotification != null)
                {
                    existedNotification.isActive = false;
                    existedNotification.Modified = now;
                    existedNotification.ModifiedBy = User.Identity.Name;
                }
                db.SaveChanges();
                return Json(new { success = true, responseCode = "200", responseText = "item deleted" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = true, responseCode = "200", responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        //public JsonResult GetNotificationByRole(string role)
        public JsonResult GetNotificationByRole()
        {
            try
            {
                UserManagementEntities DBUser = new UserManagementEntities();
                //int roleSearchDB = Convert.ToInt32(role);
                //var roleDB = db.Roles.Where(y => y.ID == roleSearchDB).SingleOrDefault().Name.ToLower();
                CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
                //var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();
                var role = myUser.Roles.ToLower();
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
                var notifications = db.Notifications.Where(x => x.Role == role && x.isActive == true);
                string userName = User.Identity.Name.ToLower();
                List<string> vendorCode = new List<string>();
                List<string> myUserNRPs = new List<string>();
                List<string> userInternalList = DBUser.Users.Select(x => x.Username).ToList();
                bool isHeadProcurement = false;
                bool isHeadSubcont = false;

                if (myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower() || role == LoginConstants.RoleSubcontDev.ToLower())
                {
                    var userInternal = DBUser.Users.Where(x => x.Username == userName).FirstOrDefault();
                    //if (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower())
                    //{
                    //    SubcontDevUserRole subcontDevUserRole = db.SubcontDevUserRoles.Where(x => x.Username == userName).FirstOrDefault();
                    //    if(subcontDevUserRole != null)
                    //    {
                    //        if(subcontDevUserRole.IsHead == null || subcontDevUserRole.IsHead == false)
                    //        {
                    //            vendorCode = db.SubcontDevVendors.Where(x => x.Username == userName).Select(x => x.VendorCode).ToList();
                    //            notifications = notifications.Where(x => vendorCode.Contains(x.PurchasingDocumentItem.PO.VendorCode));
                    //        }

                    //        if (subcontDevUserRole.RoleName.ToLower() == "subcont technical")
                    //        {
                    //            notifications = notifications.Where(x => vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode) && x.PurchasingDocumentItem.ActiveStageToNumber < 2);
                    //        }
                    //        else if(subcontDevUserRole.RoleName.ToLower() == "subcont management")
                    //        {
                    //            notifications = notifications.Where(x => vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode) && x.PurchasingDocumentItem.ActiveStageToNumber > 1);
                    //        }
                    //    }                        
                    //}
                    if (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower())
                    {
                        SubcontDevUserRole subcontDevUserRole = db.SubcontDevUserRoles.Where(x => x.Username == userName).FirstOrDefault();
                        if (subcontDevUserRole != null)
                        {
                            if (subcontDevUserRole.IsHead == false)
                            {
                                if (subcontDevUserRole.RoleID == 1)
                                {
                                    vendorCode = db.SubcontDevVendors.Where(x => x.Username == userName).Select(x => x.VendorCode).ToList();
                                    notifications = notifications.Where(x => vendorCode.Contains(x.PurchasingDocumentItem.PO.VendorCode));
                                }
                            }
                            else
                            {
                                isHeadSubcont = true;
                            }

                            if (subcontDevUserRole.RoleName.ToLower() == "subcont management")
                            {
                                notifications = notifications.Where(x => vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode) && x.Stage == null || x.Stage == "0" || x.Stage == "1");
                            }
                            else if (subcontDevUserRole.RoleName.ToLower() == "subcont technical")
                            {
                                notifications = notifications.Where(x => vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode) && (x.Stage != null && x.Stage != "0" && x.Stage != "1"));
                            }
                            else
                            {
                                notifications = null;
                            }
                        }
                        else
                        {
                            notifications = null;
                        }
                    }
                    else
                    {
                        myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
                        myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

                        var noShowNotifications = db.Notifications.Where(x => x.Role == role && x.isActive == true);

                        if (myUserNRPs.Count > 0)
                        {
                            foreach (var myUserNRP in myUserNRPs)
                            {
                                noShowNotifications = noShowNotifications.Where(x => x.PurchasingDocumentItem.PO.CreatedBy != myUserNRP);
                            }
                        }

                        notifications = notifications.Except(noShowNotifications);

                        // For Head or Superior
                        if (myUserNRPs.Count() > 2)
                        {
                            isHeadProcurement = true;
                        }
                    }
                }
                else if (role == LoginConstants.RoleVendor.ToLower())
                {
                    var userEksternal = db.UserVendors.Where(x => x.Username == userName).FirstOrDefault();

                    notifications = notifications.Where(x => x.PurchasingDocumentItem.PO.VendorCode == userEksternal.VendorCode);
                    //if (userEksternal != null)
                    //{
                    //    vendorCode.Add(userEksternal.VendorCode);
                    //}
                }

                var notificationsDTO = notifications.Select(x =>
                  new
                  {
                      ID = x.ID,
                      VendorCode = x.PurchasingDocumentItem.PO.VendorCode,
                      POImport = x.PurchasingDocumentItem.PO.Type.ToLower() == "zo04" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo07" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo08",
                      POLocal = (x.PurchasingDocumentItem.PO.Type.ToLower() == "zo05" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo09" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode),
                      POSubcont = (x.PurchasingDocumentItem.PO.Type.ToLower() == "zo05" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo09" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo10") && vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode),
                      PONumber = x.PurchasingDocumentItem.PO.Number,
                      POQty = x.PurchasingDocumentItem.Quantity,
                      POConfirmedQty = x.PurchasingDocumentItem.ConfirmedQuantity,
                      DeliveryDate = x.PurchasingDocumentItem.DeliveryDate,
                      PDIID = x.PurchasingDocumentItem.ID,
                      material = x.PurchasingDocumentItem.Material,
                      GRDate = x.GoodsReceiptDate,
                      GRQty = x.GoodsReceiptQuantity,
                      stage = x.Stage,
                      statusID = x.StatusID,
                      role = x.Role.ToString(),
                      assignedFromInternal = userInternalList.Contains(x.CreatedBy),
                      url = "#",
                      created = x.Created,
                      POConfirmedItem = x.PurchasingDocumentItem.ConfirmedItem,
                      CountEtaHistory = x.PurchasingDocumentItem.ETAHistories.Count,
                      ConfirmFirstETA = x.PurchasingDocumentItem.ETAHistories.OrderBy(y => y.Created).FirstOrDefault().AcceptedByProcurement,
                      AskProformaInvoice = x.PurchasingDocumentItem.ApproveProformaInvoiceDocument,
                      ProformaInvoice = x.PurchasingDocumentItem.ProformaInvoiceDocument,
                      ConfirmedPaymentReceive = x.PurchasingDocumentItem.ConfirmReceivedPaymentDate,
                      SecondETAHistory = x.PurchasingDocumentItem.ETAHistories.OrderByDescending(y => y.Created).FirstOrDefault().ETADate,
                      FirstETAHistory = x.PurchasingDocumentItem.ETAHistories.OrderBy(y => y.Created).FirstOrDefault().ETADate,
                      CountProgressPhotos = x.PurchasingDocumentItem.ProgressPhotoes.Count(),
                      ShipmentBookingDate = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().BookingDate,
                      ShipmentATD = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().ATDDate,
                      ShipmentCopyBLDate = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().CopyBLDate,
                      ShipmentATA = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().ATADate,
                      InvoiceDocument = x.PurchasingDocumentItem.InvoiceDocument,
                      IsHead = isHeadProcurement,
                      IsHeadSubcont = isHeadSubcont,
                      ConfirmedDate = x.PurchasingDocumentItem.ConfirmedDate,
                      ReleaseDate = x.PurchasingDocumentItem.PO.ReleaseDate,
                      POCreatedBy = x.PurchasingDocumentItem.PO.PurchaseOrderCreator
                  }).OrderByDescending(x => x.created);

                if (notificationsDTO != null)
                {
                    return Json(new { success = true, responseCode = "200", notifications = JsonConvert.SerializeObject(notificationsDTO) }, JsonRequestBehavior.AllowGet);
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

                if (!String.IsNullOrEmpty(userProcurementSuperior.NRP))
                {
                    userNRPs.Add(userProcurementSuperior.NRP);
                }

                if (userProcurementSuperior != null)
                {
                    List<UserProcurementSuperior> childUsers = db.UserProcurementSuperiors.Where(x => x.ParentID == userProcurementSuperior.ID).ToList();

                    foreach (var childUser in childUsers)
                    {
                        if (!string.IsNullOrEmpty(childUser.NRP))
                        {
                            userNRPs.Add(childUser.NRP);
                        }

                        List<UserProcurementSuperior> grandchildUsers = db.UserProcurementSuperiors.Where(x => x.ParentID == childUser.ID).ToList();

                        if (grandchildUsers.Count > 0)
                        {
                            foreach (var grandchildUser in grandchildUsers)
                            {
                                if (!string.IsNullOrEmpty(grandchildUser.NRP))
                                {
                                    userNRPs.Add(grandchildUser.NRP);
                                }
                            }
                        }
                    }
                }
            }

            return userNRPs;
        }

        public JsonResult GetPOItemCount()
        {
            try
            {
                CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
                string role = myUser.Roles.ToLower();
                string userName = User.Identity.Name;
                DateTime today = DateTime.Now;

                #region Import

                var pOesImport = db.POes.Where(x => (x.Type.ToLower() == "zo04" || x.Type.ToLower() == "zo07" || x.Type.ToLower() == "zo08") &&
                        (x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material))) &&
                        (x.ReleaseDate != null))
                        .AsQueryable();

                if (role == LoginConstants.RoleProcurement.ToLower())
                {
                    List<string> myUserNRPs = new List<string>();
                    myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
                    myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

                    var noShowPOes = pOesImport;

                    if (myUserNRPs.Count > 0)
                    {
                        foreach (var myUserNRP in myUserNRPs)
                        {
                            noShowPOes = noShowPOes.Where(x => x.CreatedBy != myUserNRP);
                        }
                    }

                    pOesImport = pOesImport.Except(noShowPOes);
                }
                else if (role == LoginConstants.RoleAdministrator.ToLower())
                {

                }
                else
                {
                    pOesImport = pOesImport.Where(x => x.VendorCode == db.UserVendors.Where(y => y.Username == myUser.UserName).FirstOrDefault().VendorCode);
                }


                string ImportPOItemsCountNew = pOesImport.Where(x => x.PurchasingDocumentItems.Any(y => (y.ActiveStage == null || y.ActiveStage == "0") && y.PurchasingDocumentItemHistories.All(z => z.POHistoryCategory != "Q") && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx")).Count().ToString();
                string ImportPOItemsCountOnGoing = pOesImport.Where(x => x.PurchasingDocumentItems.Any(y => y.ActiveStage != null && y.ActiveStage != "0" && y.PurchasingDocumentItemHistories.All(z => z.POHistoryCategory != "Q") && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx")).Count().ToString();
                string ImportPOItemsDone = pOesImport.Where(x => x.PurchasingDocumentItems.Any(y => y.PurchasingDocumentItemHistories.Any(z => z.POHistoryCategory == "Q") || y.IsClosed.ToLower() == "l" || y.IsClosed.ToLower() == "lx")).Count().ToString();

                #endregion

                #region Subcont
                int subcontNewPO = 0;
                int subcontOngoing = 0;
                int subcontDone = 0;
                var pOesSubcont = db.POes.Where(po => po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10").AsQueryable();
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
                if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
                {
                    string vendorCode = db.UserVendors.Where(x => x.Username == myUser.UserName).Select(x => x.VendorCode).FirstOrDefault();
                    //pOes = pOes.Where(po => po.VendorCode == myUser. (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode)).OrderBy(x => x.Number);
                    subcontNewPO = pOesSubcont.Where(po => vendorCode == po.VendorCode).SelectMany(x => x.PurchasingDocumentItems).Count(x => x.ConfirmedQuantity == null && x.Material != "" && x.Material != null && x.ParentID == null);
                    subcontOngoing = pOesSubcont.Where(po => vendorCode == po.VendorCode).SelectMany(x => x.PurchasingDocumentItems).Count(x => (x.ConfirmedQuantity != null || x.ConfirmedItem != null) && x.Material != "" && x.Material != null && x.ParentID == null);
                    subcontDone = pOesSubcont.Where(po => vendorCode == po.VendorCode).SelectMany(x => x.PurchasingDocumentItems).Count(y => (y.PO.Date.Year == today.Year || y.PO.Date.Year == today.Year - 1) && (y.IsClosed.ToLower() == "x" && y.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "t")) || y.IsClosed.ToLower() == "l" || (y.IsClosed.ToLower() == "lx" && y.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "t")));
                }
                else if (role.ToLower() == LoginConstants.RoleSubcontDev.ToLower())
                {
                    var listVendorSubconDev = db.SubcontDevVendors.Where(x => x.Username == myUser.UserName).Select(x => x.VendorCode).Distinct();

                    SubcontDevUserRole subcontDevUserRole = db.SubcontDevUserRoles.Where(x => x.Username == userName).FirstOrDefault();
                    List<string> listUsername;
                    //var listUsername = userName;
                    if (subcontDevUserRole != null)
                    {
                        if (subcontDevUserRole.IsHead == true)
                        {
                            listUsername = db.SubcontDevUserRoles.Where(x => x.RoleID == subcontDevUserRole.RoleID).Select(x => x.Username.ToLower()).ToList();
                            listVendorSubconDev = db.SubcontDevVendors.Where(x => listUsername.Contains(x.Username.ToLower())).Select(x => x.VendorCode).Distinct();
                        }
                    }

                    if (listVendorSubconDev != null && subcontDevUserRole.RoleID == 1)
                    {
                        pOesSubcont = pOesSubcont.Where(po => listVendorSubconDev.Contains(po.VendorCode));
                    }

                    subcontNewPO = pOesSubcont.Where(po => vendorSubcont.Contains(po.VendorCode)).SelectMany(x => x.PurchasingDocumentItems).Count(x => x.ConfirmedQuantity == null && !String.IsNullOrEmpty(x.Material) && x.ParentID == null && String.IsNullOrEmpty(x.IsClosed));
                    subcontOngoing = pOesSubcont.Where(po => vendorSubcont.Contains(po.VendorCode)).SelectMany(x => x.PurchasingDocumentItems).Count(x => x.ConfirmedQuantity > 0 && x.Material != "" && x.Material != null && x.ParentID == null);
                    subcontDone = pOesSubcont.Where(po => vendorSubcont.Contains(po.VendorCode)).SelectMany(x => x.PurchasingDocumentItems).Count(y => (y.PO.Date.Year == today.Year || y.PO.Date.Year == today.Year - 1) && y.IsClosed != null && ((y.IsClosed.ToLower() == "x" && y.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory!= null && pdih.POHistoryCategory == "t")) || y.IsClosed.ToLower() == "l" || (y.IsClosed.ToLower() == "lx" && y.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory != null && pdih.POHistoryCategory == "t"))));
                }
                else
                {
                    subcontNewPO = pOesSubcont.Where(po => vendorSubcont.Contains(po.VendorCode)).SelectMany(x => x.PurchasingDocumentItems).Count(x => x.ConfirmedQuantity == null && !String.IsNullOrEmpty(x.Material) && x.ParentID == null && String.IsNullOrEmpty(x.IsClosed));
                    subcontOngoing = pOesSubcont.Where(po => vendorSubcont.Contains(po.VendorCode)).SelectMany(x => x.PurchasingDocumentItems).Count(x => x.ConfirmedQuantity > 0 && !String.IsNullOrEmpty(x.Material) && x.ParentID == null);
                    subcontDone = pOesSubcont.Where(po => vendorSubcont.Contains(po.VendorCode)).SelectMany(x => x.PurchasingDocumentItems).Count(y => (y.PO.Date.Year == today.Year || y.PO.Date.Year == today.Year - 1) && y.IsClosed != null && ((y.IsClosed.ToLower() == "x" && y.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory != null && pdih.POHistoryCategory == "t")) || y.IsClosed.ToLower() == "l" || (y.IsClosed.ToLower() == "lx" && y.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory != null && pdih.POHistoryCategory == "t"))));
                }
                #endregion

                #region Local
                //var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
                var pOesLocal = db.POes.Where(x => (x.Type.ToLower() == "zo05" || x.Type.ToLower() == "zo09" || x.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.VendorCode) &&
                                     (x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material))) && (x.ReleaseDate != null))
                                     .AsQueryable();

                if (role == LoginConstants.RoleProcurement.ToLower())
                {
                    List<string> myUserNRPs = new List<string>();
                    myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
                    myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

                    var noShowPOes = pOesLocal;

                    if (myUserNRPs.Count > 0)
                    {
                        foreach (var myUserNRP in myUserNRPs)
                        {
                            noShowPOes = noShowPOes.Where(x => x.CreatedBy != myUserNRP);
                        }
                    }
                    pOesLocal = pOesLocal.Except(noShowPOes);
                }

                else if (role == LoginConstants.RoleAdministrator.ToLower())
                {
                    //pOesLocal = pOes.Include(x => x.PurchasingDocumentItems)
                    //                .Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null))
                    //                .AsQueryable();
                }
                else
                {
                    pOesLocal = pOesLocal.Where(x => x.VendorCode == db.UserVendors.Where(y => y.Username == myUser.UserName).FirstOrDefault().VendorCode);
                }
                //count by item
                //string LocalPOItemsCountNew = pOesLocal.SelectMany(x => x.PurchasingDocumentItems).Count(y => (y.ActiveStage == null || y.ActiveStage == "0") && (y.IsClosed.ToLower() != "x" && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx")).ToString();
                //string LocalPOItemsCountOnGoing = pOesLocal.SelectMany(x => x.PurchasingDocumentItems).Count(y => y.ActiveStage != null && y.ActiveStage != "0" && y.IsClosed.ToLower() != "x" && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx").ToString();
                //string LocalPOItemsDone = pOesLocal.SelectMany(x => x.PurchasingDocumentItems).Count(y => y.IsClosed.ToLower() == "x" || y.IsClosed.ToLower() == "l" || y.IsClosed.ToLower() == "lx").ToString();

                //count by po
                string LocalPOItemsCountNew = pOesLocal.Where(x => x.PurchasingDocumentItems.Any(y => (y.ActiveStage == null || y.ActiveStage == "0") && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx" && y.PurchasingDocumentItemHistories.All(z => z.POHistoryCategory.ToLower() != "t" && z.POHistoryCategory.ToLower() != "q"))).Count().ToString();
                string LocalPOItemsCountOnGoing = pOesLocal.Where(x => x.PurchasingDocumentItems.Any(y => y.ActiveStage != null && y.ActiveStage != "0" && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx" && y.PurchasingDocumentItemHistories.All(z => z.POHistoryCategory.ToLower() != "t" && z.POHistoryCategory.ToLower() != "q"))).Count().ToString();
                string LocalPOItemsDone = pOesLocal.Where(x => x.PurchasingDocumentItems.Any(y => y.IsClosed.ToLower() == "l" || y.IsClosed.ToLower() == "lx" || y.PurchasingDocumentItemHistories.Any(z => z.POHistoryCategory.ToLower() == "t" || z.POHistoryCategory.ToLower() == "q"))).Count().ToString();

                #endregion

                return Json(new { success = true, ImportPOItemsCountNew, ImportPOItemsCountOnGoing, ImportPOItemsDone, subcontNewPO, subcontOngoing, subcontDone, LocalPOItemsCountNew, LocalPOItemsCountOnGoing, LocalPOItemsDone }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseCode = "500", responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}