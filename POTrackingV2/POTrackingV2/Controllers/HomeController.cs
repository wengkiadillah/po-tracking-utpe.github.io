using Newtonsoft.Json;
using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize]
    public class HomeController : Controller
    {
        POTrackingEntities db = new POTrackingEntities();
        UserManagementEntities DBUser = new UserManagementEntities();

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
                //int roleSearchDB = Convert.ToInt32(role);

                //var roleDB = db.Roles.Where(y => y.ID == roleSearchDB).SingleOrDefault().Name.ToLower();

                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
                var notifications = db.Notifications.Where(x => x.Role == role && x.isActive == true).Select(x =>
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
                      ApproveProformaInvoice = x.PurchasingDocumentItem.ApproveProformaInvoiceDocument,
                      ConfirmedPaymentReceive = x.PurchasingDocumentItem.ConfirmReceivedPaymentDate,
                      SecondETAHistory = x.PurchasingDocumentItem.ETAHistories.OrderByDescending(y => y.Created).FirstOrDefault().ETADate,
                      CountProgressPhotos = x.PurchasingDocumentItem.ProgressPhotoes.Count(),
                      ShipmentBookingDate = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().BookingDate,
                      ShipmentATD = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().ATDDate,
                      ShipmentCopyBLDate = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().CopyBLDate,
                      ShipmentATA = x.PurchasingDocumentItem.Shipments.OrderBy(y => y.Created).FirstOrDefault().ATADate
                  }).OrderByDescending(x => x.created);

                if (notifications != null)
                {
                    return Json(new { success = true, responseCode = "200", notifications = JsonConvert.SerializeObject(notifications) }, JsonRequestBehavior.AllowGet);
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
    }
}