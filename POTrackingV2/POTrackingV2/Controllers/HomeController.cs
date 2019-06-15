﻿using Newtonsoft.Json;
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

        [HttpGet]
        public JsonResult GetNotificationByRole(string role)
        {
            try
            {
                UserManagementEntities DBUser = new UserManagementEntities();
                //int roleSearchDB = Convert.ToInt32(role);
                //var roleDB = db.Roles.Where(y => y.ID == roleSearchDB).SingleOrDefault().Name.ToLower();
                CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
                var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
                var notifications = db.Notifications.Where(x => x.Role == role && x.isActive == true);
                string userName = User.Identity.Name;
                List<string> vendorCode = new List<string>();
                List<string> myUserNRPs = new List<string>();

                if (myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower())
                {
                    var userInternal = DBUser.Users.Where(x => x.Username == userName).FirstOrDefault();
                    if (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower())
                    {
                        vendorCode = db.SubcontDevVendors.Where(x => x.Username == userName).Select(x => x.VendorCode).ToList();

                        notifications = notifications.Where(x => vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode) && vendorCode.Contains(x.PurchasingDocumentItem.PO.VendorCode));
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
                    }
                }
                else
                {
                    var userEksternal = db.UserVendors.Where(x => x.Username == userName).FirstOrDefault();

                    notifications = notifications.Where(x => x.PurchasingDocumentItem.PO.VendorCode == userEksternal.VendorCode);
                    //if (userEksternal != null)
                    //{
                    //    vendorCode.Add(userEksternal.VendorCode);
                    //}
                }

                //if (roleType.RolesTypeID == 1) // Notif buat orang Subcont
                //{
                //    List<string> vendorCode = new List<string>();

                //    var userInternal = DBUser.Users.Where(x => x.Username == userName).FirstOrDefault();
                //    if (userInternal != null)
                //    {
                //        vendorCode = db.SubcontDevVendors.Where(x => x.Username == userName).Select(x => x.VendorCode).ToList();
                //    }
                //    else
                //    {
                //        var userEksternal = db.UserVendors.Where(x => x.Username == userName).FirstOrDefault();
                //        if (userEksternal != null)
                //        {
                //            vendorCode.Add(userEksternal.VendorCode);
                //        }
                //    }

                //    notifications = notifications.Where(x => (x.PurchasingDocumentItem.PO.Type.ToLower() == "zo05" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo09" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo10") && vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode) && vendorCode.Contains(x.PurchasingDocumentItem.PO.VendorCode));
                //}
                //else if (roleType.RolesTypeID == 2) // Notif buat orang Local
                //{
                //    notifications = notifications.Where(x => (x.PurchasingDocumentItem.PO.Type.ToLower() == "zo05" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo09" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode));
                //}
                //else if (roleType.RolesTypeID == 3) // Notif buat orang Import
                //{
                //    notifications = notifications.Where(x => x.PurchasingDocumentItem.PO.Type.ToLower() == "zo04" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo07" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo08");

                //    if (role == LoginConstants.RoleProcurement.ToLower())
                //    {
                //        List<string> myUserNRPs = new List<string>();
                //        myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
                //        myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

                //        var noShowNotifications = db.Notifications.Where(x => x.PurchasingDocumentItem.PO.Type.ToLower() == "zo04" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo07" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo08");

                //        if (myUserNRPs.Count > 0)
                //        {
                //            foreach (var myUserNRP in myUserNRPs)
                //            {
                //                noShowNotifications = noShowNotifications.Where(x => x.PurchasingDocumentItem.PO.CreatedBy != myUserNRP);
                //            }
                //        }

                //        notifications = notifications.Except(noShowNotifications);
                //    }
                //    else if ( role == LoginConstants.RoleVendor.ToLower())
                //    {
                //        notifications = notifications.Where(x => x.PurchasingDocumentItem.PO.VendorCode == db.UserVendors.Where(y => y.Username == myUser.UserName).FirstOrDefault().VendorCode);
                //    }
                //}

                var notificationsDTO = notifications.Select(x =>
                  new
                  {
                      id = x.ID,
                      VendorCode = x.PurchasingDocumentItem.PO.VendorCode,
                      POImport = x.PurchasingDocumentItem.PO.Type.ToLower() == "zo04" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo07" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo08",
                      POLocal = (x.PurchasingDocumentItem.PO.Type.ToLower() == "zo05" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo09" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode),
                      POSubcont = (x.PurchasingDocumentItem.PO.Type.ToLower() == "zo05" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo09" || x.PurchasingDocumentItem.PO.Type.ToLower() == "zo10") && vendorSubcont.Contains(x.PurchasingDocumentItem.PO.VendorCode),
                      PONumber = x.PurchasingDocumentItem.PO.Number,
                      POQty = x.PurchasingDocumentItem.ConfirmedQuantity,
                      material = x.PurchasingDocumentItem.Material,
                      GRDate = x.GoodsReceiptDate,
                      GRQty = x.GoodsReceiptQuantity,
                      stage = x.Stage,
                      statusID = x.StatusID,
                      role = x.Role,
                      url = "#",
                      created = x.Created,
                      POConfirmedItem = x.PurchasingDocumentItem.ConfirmedItem,
                      CountEtaHistory = x.PurchasingDocumentItem.ETAHistories.Count,
                      ConfirmFirstETA = x.PurchasingDocumentItem.ETAHistories.OrderBy(y => y.Created).FirstOrDefault().AcceptedByProcurement,
                      ProformaInvoice = x.PurchasingDocumentItem.ProformaInvoiceDocument,
                      ConfirmedPaymentReceive = x.PurchasingDocumentItem.ConfirmReceivedPaymentDate,
                      SecondETAHistory = x.PurchasingDocumentItem.ETAHistories.OrderByDescending(y => y.Created).FirstOrDefault().ETADate,
                      CountProgressPhotos = x.PurchasingDocumentItem.ProgressPhotoes.Count(),
                      ShipmentBookingDate = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().BookingDate,
                      ShipmentATD = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().ATDDate,
                      ShipmentCopyBLDate = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().CopyBLDate,
                      ShipmentATA = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().ATADate,
                      InvoiceDocument = x.PurchasingDocumentItem.InvoiceDocument
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
    }
}