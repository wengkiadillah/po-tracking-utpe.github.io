using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using System.Globalization;
using System.IO;
using POTrackingV2.CustomAuthentication;
using System.Web.Security;
using Newtonsoft.Json;
using POTrackingV2.Constants;
using System.Web.Configuration;
using System.Transactions;
using System.Data;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleVendor + "," + LoginConstants.RoleSubcontDev)]
    public class SubcontController : Controller
    {
        DateTime now = DateTime.Now;
        private string iisAppName = WebConfigurationManager.AppSettings["IISAppName"];

        [HttpGet]
        public JsonResult GetDataFromValue(string filterBy, string value)
        {
            POTrackingEntities db = new POTrackingEntities();
            try
            {
                object data = null;
                value = value.ToLower();

                if (filterBy == "poNumber")
                {
                    data = db.POes.Where(x => x.Number.Contains(value)).Select(x =>
                     new
                     {
                         Data = x.Number,
                         MatchEvaluation = x.Number.ToLower().IndexOf(value)
                     }).Distinct().OrderBy(x => x.MatchEvaluation).Take(10);
                }
                else if (filterBy == "vendor")
                {
                    data = db.Vendors.Where(x => x.Name.Contains(value)).Select(x =>
                    new
                    {
                        Data = x.Name,
                        MatchEvaluation = x.Name.ToLower().IndexOf(value)
                    }).Distinct().OrderBy(x => x.MatchEvaluation).Take(10);
                }
                else if (filterBy == "material")
                {
                    data = db.PurchasingDocumentItems.Where(x => x.Material.Contains(value) || x.Description.Contains(value)).Select(x =>
                    new
                    {
                        Data = x.Material.ToLower().StartsWith(value) ? x.Material : x.Description.ToLower().StartsWith(value) ? x.Description : x.Material.ToLower().Contains(value) ? x.Material : x.Description,
                        MatchEvaluation = (x.Material.ToLower().StartsWith(value) ? 1 : 0) + (x.Description.ToLower().StartsWith(value) ? 1 : 0)
                    }).Distinct().OrderByDescending(x => x.MatchEvaluation).Take(10);
                }
                else if (filterBy == "subcontdev")
                {
                    data = db.POes.Where(x => x.PurchaseOrderCreator.Contains(value)).Select(x =>
                    new
                    {
                        Data = x.PurchaseOrderCreator,
                        MatchEvaluation = x.PurchaseOrderCreator.ToLower().IndexOf(value)
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

        // GET: POSubcont
        public ActionResult Index(string searchPOStatus, string searchPONumber, string searchVendorName, string searchMaterial, string searchSubcontDev, string searchStartPODate, string searchEndPODate, int? page)
        {
            int totalPDI = 0;
            POTrackingEntities db = new POTrackingEntities();
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles;
            string userName = User.Identity.Name;
            //role = "administrator";
            //userName = "altrovis";
            //role = "subcontdev";
            //userName = "pangeran";
            //role = "vendor";
            //userName = "50993";
            DateTime today = DateTime.Now;
            try
            {
                //var pOes = db.POes.OrderBy(x => x.Number).AsQueryable();
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
                SubcontDevUserRole subcontDevUserRole = db.SubcontDevUserRoles.Where(x => x.Username == userName).FirstOrDefault();

                var pOes = db.POes.AsQueryable();

                if (!string.IsNullOrEmpty(searchPOStatus))
                {
                    if (searchPOStatus.ToLower() != "done")
                    {
                        if (searchPOStatus.ToLower() == "newpo")
                        {
                            pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.IsClosed != "X" && y.IsClosed != "L" && y.IsClosed != "LX" && y.ConfirmedQuantity == null));
                        }
                        else
                        {
                            //pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.IsClosed != "X" && y.IsClosed != "L" && y.IsClosed != "LX") || (y.ParentID==null && y.IsClosed != null && y.IsClosed.Contains("X") && !y.PurchasingDocumentItemHistories.Any(z => z.POHistoryCategory != null && z.POHistoryCategory.ToLower() == "q")) ));
                            //pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.ParentID == null && y.IsClosed != null && y.IsClosed.Contains("X") && ( (y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity) == null) || (y.ConfirmedQuantity >= 0 && y.ConfirmedQuantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)) || (y.ConfirmedQuantity == null && y.Quantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)) ))));
                            //pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.ParentID == null && y.IsClosed != null && y.IsClosed.Contains("X") && ((y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity) == null) || (y.ConfirmedQuantity >= 0 && y.ConfirmedQuantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)) || (y.ConfirmedQuantity == null && y.Quantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0))))));
                            pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.ParentID == null && ((y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity) == null) || (y.ConfirmedQuantity >= 0 && y.ConfirmedQuantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)) || (y.ConfirmedQuantity == null && y.Quantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0))))));
                        }
                    }
                    else
                    {
                        //pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.IsClosed == "X" || y.IsClosed == "L" || y.IsClosed == "LX") ));
                        pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.IsClosed == "L") || (y.ParentID == null && y.IsClosed != null && y.IsClosed.Contains("X") && ((y.ConfirmedQuantity >= 0 && y.ConfirmedQuantity <= y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)) || (y.ConfirmedQuantity == null && y.Quantity <= y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0))))));
                    }
                }
                else
                {
                    //pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.IsClosed != "X" && y.IsClosed != "L" && y.IsClosed != "LX"));
                    //pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.IsClosed != "X" && y.IsClosed != "L" && y.IsClosed != "LX") || (y.ParentID == null && y.IsClosed != null && y.IsClosed.Contains("X") && !y.PurchasingDocumentItemHistories.Any(z => z.POHistoryCategory != null && z.POHistoryCategory.ToLower() == "q"))));
                    //pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.IsClosed != "L" || (y.ParentID == null && y.IsClosed != null && y.IsClosed.Contains("X") && (y.ConfirmedQuantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)))));
                    //pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.IsClosed != "X" && y.IsClosed != "L" && y.IsClosed != "LX" && y.ConfirmedQuantity == null) || 
                    //(y.ParentID == null && y.IsClosed != null && y.IsClosed.Contains("X") && ((y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity) == null) || (y.ConfirmedQuantity >= 0 && y.ConfirmedQuantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)) || (y.ConfirmedQuantity == null && y.Quantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)))) 
                    //));
                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.IsClosed != "X" && y.IsClosed != "L" && y.IsClosed != "LX" && y.ConfirmedQuantity == null) ||
                    (y.ParentID == null && ((y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity) == null) || (y.ConfirmedQuantity >= 0 && y.ConfirmedQuantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0)) || (y.ConfirmedQuantity == null && y.Quantity > y.PurchasingDocumentItemHistories.Where(zx => zx.POHistoryCategory != null && zx.POHistoryCategory.ToLower() == "q").Sum(z => z.GoodsReceiptQuantity ?? 0))))
                    ));
                }

                if (role != null && (role.ToLower() == LoginConstants.RoleSubcontDev.ToLower() || role.ToLower() == LoginConstants.RoleAdministrator.ToLower()))
                {
                    var listVendorSubconDev = db.SubcontDevVendors.Where(x => x.Username == userName).Select(x => x.VendorCode).Distinct();

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

                    if (listVendorSubconDev != null && role.ToLower() == LoginConstants.RoleSubcontDev.ToLower() && subcontDevUserRole.RoleID == 1)
                    {
                        pOes = pOes.Where(po => listVendorSubconDev.Contains(po.VendorCode));
                    }

                    if (searchPOStatus == "newpo")
                    {
                        pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.ConfirmedQuantity == null && !String.IsNullOrEmpty(x.Material) && x.ParentID == null && String.IsNullOrEmpty(x.IsClosed)) && vendorSubcont.Contains(po.VendorCode));
                    }
                    else if (searchPOStatus == "ongoing")
                    {
                        //pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => (x.ConfirmedQuantity != null || x.ConfirmedItem != null) && x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode));
                        pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode));
                    }
                    else if (searchPOStatus == "done")
                    {
                        //pOes = pOes.Where(po => (po.Date.Year == today.Year || po.Date.Year == today.Year - 1) && (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => (x.IsClosed == "X" && x.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "t")) || x.IsClosed == "L" || (x.IsClosed == "LX" && x.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "t"))) && vendorSubcont.Contains(po.VendorCode));
                        pOes = pOes.Where(po => (po.Date.Year == today.Year || po.Date.Year == today.Year - 1) && (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => (x.IsClosed == "X" && x.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "q")) || x.IsClosed == "L" || (x.IsClosed == "LX" && x.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "q"))) && vendorSubcont.Contains(po.VendorCode));
                    }
                    else if (searchPOStatus == "negotiated")
                    {
                        pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.ActiveStage == "1" && (y.ConfirmedQuantity != y.Quantity || y.ConfirmedDate != y.DeliveryDate) && y.ConfirmedItem != false));
                    }
                    else
                    {
                        //pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.ConfirmedQuantity > 0 && x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode));
                        pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode));
                    }
                }
                else if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
                {
                    string vendorCode = db.UserVendors.Where(x => x.Username == userName).Select(x => x.VendorCode).FirstOrDefault();
                    //pOes = pOes.Where(po => po.VendorCode == myUser. (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode)).OrderBy(x => x.Number);
                    if (searchPOStatus == "newpo")
                    {
                        pOes = pOes.Where(po => po.VendorCode == vendorCode && (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.ConfirmedQuantity == null && !String.IsNullOrEmpty(x.Material) && x.ParentID == null && String.IsNullOrEmpty(x.IsClosed)));
                    }
                    else if (searchPOStatus == "ongoing")
                    {
                        //pOes = pOes.Where(po => po.VendorCode == vendorCode && (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => (x.ConfirmedQuantity != null || x.ConfirmedItem != null) && x.Material != "" && x.Material != null && x.ParentID == null));
                        pOes = pOes.Where(po => po.VendorCode == vendorCode && (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && x.ParentID == null));
                    }
                    else if (searchPOStatus == "done")
                    {
                        //pOes = pOes.Where(po => (po.Date.Year == today.Year || po.Date.Year == today.Year - 1) && po.VendorCode == vendorCode && (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => ((x.IsClosed == "X" && x.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "t")) || x.IsClosed == "L" || (x.IsClosed == "LX" && x.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "t")))));
                        pOes = pOes.Where(po => (po.Date.Year == today.Year || po.Date.Year == today.Year - 1) && po.VendorCode == vendorCode && (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => ((x.IsClosed == "X" && x.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "q")) || x.IsClosed == "L" || (x.IsClosed == "LX" && x.PurchasingDocumentItemHistories.Any(pdih => pdih.POHistoryCategory.ToLower() == "q")))));
                    }
                    else
                    {
                        pOes = pOes.Where(po => po.VendorCode == vendorCode && (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && x.ParentID == null));
                    }
                }

                string subcontDevUserRoleName = subcontDevUserRole != null ? subcontDevUserRole.RoleName.ToLower() : "";
                ViewBag.CurrentRoleID = role.ToLower();
                ViewBag.CurrentSubcontDevRoleName = subcontDevUserRoleName;
                ViewBag.CurrentSearchPOStatus = searchPOStatus;
                ViewBag.RoleSubcontDev = LoginConstants.RoleSubcontDev.ToLower();
                ViewBag.RoleAdministrator = LoginConstants.RoleAdministrator.ToLower();
                ViewBag.RoleVendor = LoginConstants.RoleVendor.ToLower();
                ViewBag.CurrentDataPONumber = searchPONumber;
                ViewBag.CurrentDataVendorName = searchVendorName;
                ViewBag.CurrentDataMaterial = searchMaterial;
                ViewBag.CurrentDataSubcontDev = searchSubcontDev;
                ViewBag.CurrentStartPODate = searchStartPODate;
                ViewBag.CurrentEndPODate = searchEndPODate;
                ViewBag.IISAppName = iisAppName;

                totalPDI = pOes.SelectMany(x => x.PurchasingDocumentItems).Count();
                ViewBag.NewPO = totalPDI.ToString();
                ViewBag.Ongoing = totalPDI.ToString();
                ViewBag.Done = totalPDI.ToString();

                #region Filter
                if (!String.IsNullOrEmpty(searchPONumber))
                {
                    pOes = pOes.Where(po => po.Number.Contains(searchPONumber));
                }
                if (!String.IsNullOrEmpty(searchVendorName))
                {
                    pOes = pOes.Where(po => po.Vendor.Name.ToLower().Contains(searchVendorName.ToLower()));
                }
                if (!String.IsNullOrEmpty(searchMaterial))
                {
                    pOes = pOes.Where(po => po.PurchasingDocumentItems.Any(x => x.Material.ToLower().Contains(searchMaterial.ToLower()) || x.Description.ToLower().Contains(searchMaterial.ToLower())));
                }
                if (!String.IsNullOrEmpty(searchSubcontDev))
                {
                    pOes = pOes.Where(po => po.PurchaseOrderCreator.ToLower().Contains(searchSubcontDev.ToLower()));
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
                return View(pOes.OrderBy(x => x.Number).ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
            }
            catch (Exception ex)
            {
                return View(ex.Message + "-----" + ex.StackTrace);
            }
        }

        #region Report
        public ActionResult Report(string searchPONumber, string searchVendorName, string searchMaterial, string searchStartInspectionDate, string searchEndInspectionDate, int? page)
        {
            POTrackingEntities db = new POTrackingEntities();
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles;
            string userName = User.Identity.Name;
            try
            {
                //var pOes = db.POes.OrderBy(x => x.Number).AsQueryable();
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();

                var pOes = db.POes.AsQueryable();

                pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.PrimerActualDate != null && x.Material != "" && x.Material != null && x.ParentID == null) && vendorSubcont.Contains(po.VendorCode));

                ViewBag.CurrentRoleID = role.ToLower();
                ViewBag.RoleSubcont = LoginConstants.RoleSubcontDev.ToLower();
                ViewBag.CurrentDataPONumber = searchPONumber;
                ViewBag.CurrentDataVendorName = searchVendorName;
                ViewBag.CurrentDataMaterial = searchMaterial;
                //ViewBag.CurrentFilter = filterBy;
                ViewBag.CurrentStartInspectionDate = searchStartInspectionDate;
                ViewBag.CurrentEndInspectionDate = searchEndInspectionDate;
                ViewBag.IISAppName = iisAppName;

                #region Filter
                if (!String.IsNullOrEmpty(searchPONumber))
                {
                    pOes = pOes.Where(po => po.Number.Contains(searchPONumber));
                }
                if (!String.IsNullOrEmpty(searchVendorName))
                {
                    pOes = pOes.Where(po => po.Vendor.Name.ToLower().Contains(searchVendorName.ToLower()));
                }
                if (!String.IsNullOrEmpty(searchMaterial))
                {
                    pOes = pOes.Where(po => po.PurchasingDocumentItems.Any(x => x.Material.ToLower().Contains(searchMaterial.ToLower()) || x.Description.ToLower().Contains(searchMaterial.ToLower())));
                }

                if (!String.IsNullOrEmpty(searchStartInspectionDate))
                {
                    DateTime startDate = DateTime.ParseExact(searchStartInspectionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(pdi => pdi.PrimerActualDate >= startDate));
                }

                if (!String.IsNullOrEmpty(searchEndInspectionDate))
                {
                    DateTime endDate = DateTime.ParseExact(searchEndInspectionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(pdi => pdi.PrimerActualDate <= endDate));
                }
                #endregion
                //return (RedirectToAction("Index", "Error", new { ErrorList = "hi there!" }));
                return View(pOes.OrderBy(x => x.Number).ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
            }
            catch (Exception ex)
            {
                return View(ex.Message + "-----" + ex.StackTrace);
            }
        }

        public void DownloadReport(string searchPONumber, string searchVendorName, string searchMaterial, string searchStartInspectionDate, string searchEndInspectionDate)
        {
            POTrackingEntities db = new POTrackingEntities();
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles.ToLower();
            var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();

            if (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower())
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", "ReportPOSubcont.xls"));
                Response.ContentType = "application/ms-excel";
                DataTable dt = BindDataTable(searchPONumber, searchVendorName, searchMaterial, searchStartInspectionDate, searchEndInspectionDate);
                string str = string.Empty;
                if (dt != null)
                {
                    foreach (DataColumn dtcol in dt.Columns)
                    {
                        Response.Write(str + dtcol.ColumnName);
                        str = "\t";
                    }


                    Response.Write("\n");
                    foreach (DataRow dr in dt.Rows)
                    {
                        str = "";
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            Response.Write(str + Convert.ToString(dr[j]));
                            str = "\t";
                        }
                        Response.Write("\n");
                    }
                }
                Response.End();
            }
        }

        public DataTable BindDataTable(string searchPONumber, string searchVendorName, string searchMaterial, string searchStartInspectionDate, string searchEndInspectionDate)
        {
            POTrackingEntities db = new POTrackingEntities();
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles.ToLower();
            var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();

            if (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower())
            {
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();

                var pOes = db.POes.AsQueryable();
                pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.PrimerActualDate != null && x.Material != "" && x.Material != null && x.ParentID == null && (x.IsClosed == null || (x.IsClosed != null && x.IsClosed.ToLower() != "x" && x.IsClosed.ToLower() != "l" && x.IsClosed.ToLower() != "lx"))) && vendorSubcont.Contains(po.VendorCode));


                var noShowPOes = pOes;
                List<string> myUserNRPs = new List<string>();

                #region Filter
                if (!String.IsNullOrEmpty(searchPONumber))
                {
                    pOes = pOes.Where(po => po.Number.Contains(searchPONumber));
                }
                if (!String.IsNullOrEmpty(searchVendorName))
                {
                    pOes = pOes.Where(po => po.Vendor.Name.ToLower().Contains(searchVendorName.ToLower()));
                }
                if (!String.IsNullOrEmpty(searchMaterial))
                {
                    pOes = pOes.Where(po => po.PurchasingDocumentItems.Any(x => x.Material.ToLower().Contains(searchMaterial.ToLower()) || x.Description.ToLower().Contains(searchMaterial.ToLower())));
                }

                if (!String.IsNullOrEmpty(searchStartInspectionDate))
                {
                    DateTime startDate = DateTime.ParseExact(searchStartInspectionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(pdi => pdi.PrimerActualDate >= startDate));
                }

                if (!String.IsNullOrEmpty(searchEndInspectionDate))
                {
                    DateTime endDate = DateTime.ParseExact(searchEndInspectionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(pdi => pdi.PrimerActualDate <= endDate));
                }
                #endregion

                DataTable dt = new DataTable();
                dt.Columns.Add("Number", typeof(Int32));
                dt.Columns.Add("PO Number", typeof(string));
                dt.Columns.Add("Item Number", typeof(string));
                dt.Columns.Add("Material", typeof(string));
                dt.Columns.Add("Description", typeof(string));
                dt.Columns.Add("Quantity", typeof(string));
                dt.Columns.Add("Delivery Date", typeof(string));
                dt.Columns.Add("Subcont", typeof(string));
                dt.Columns.Add("Estimated Inspection Date", typeof(string));
                dt.Columns.Add("Estimated Time Arrival", typeof(string));

                int rowNumber = 1;

                foreach (var po in pOes)
                {
                    var purchasingDocumentItems = po.PurchasingDocumentItems.Where(x => x.PrimerActualDate != null && !String.IsNullOrEmpty(x.Material) && x.ActiveStage != null && x.ActiveStage != "0" && (x.IsClosed == null || (x.IsClosed != null && x.IsClosed.ToLower() != "x" && x.IsClosed.ToLower() != "l" && x.IsClosed.ToLower() != "lx")))
                                                                            .OrderBy(x => x.ItemNumber);

                    foreach (var purchasingDocumentItem in purchasingDocumentItems)
                    {
                        string deliveryDate = "-";
                        string estimatedInspectionDate = "-";
                        string estimatedTimeArrival = "-";

                        if (purchasingDocumentItem.DeliveryDate != null)
                        {
                            deliveryDate = purchasingDocumentItem.DeliveryDate.GetValueOrDefault().ToString("dd/MM/yyyy");
                        }

                        if (purchasingDocumentItem.PrimerActualDate.HasValue)
                        {
                            estimatedInspectionDate = purchasingDocumentItem.PrimerActualDate.Value.ToString("dd/MM/yyyy");
                        }



                        if (purchasingDocumentItem.PrimerActualDate.HasValue)
                        {
                            var days = 2;
                            DateTime? QCDate = purchasingDocumentItem.PrimerActualDate.HasValue ? purchasingDocumentItem.PrimerActualDate.GetValueOrDefault() : (DateTime?)null;
                            DateTime? ETADate = (DateTime?)null;
                            if (QCDate != (DateTime?)null)
                            {
                                ETADate = QCDate.Value.AddDays(days);
                                estimatedTimeArrival = ETADate.Value.ToString("dd/MM/yyyy");
                            }
                        }

                        dt.Rows.Add(rowNumber, po.Number, purchasingDocumentItem.ItemNumber, purchasingDocumentItem.Material, purchasingDocumentItem.Description, purchasingDocumentItem.Quantity, deliveryDate, po.Vendor.Name, estimatedInspectionDate, estimatedTimeArrival);

                        rowNumber++;
                    }
                }
                return dt;
            }
            else
            {
                return null;
            }

        }

        #endregion Report

        #region Template
        public ActionResult Template(string searchPoNumber, string searchStartPODate, string searchEndPODate, int? page)
        {
            POTrackingEntities db = new POTrackingEntities();
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
            return View(viewModels.ToPagedList(pageNumber, Constants.LoginConstants.PageSize));
        }
        #endregion

        #region Stage 1

        [HttpPost]
        public ActionResult SaveAllPOItem(List<PurchasingDocumentItem> purchasingDocumentItems, List<PurchasingDocumentItem> purchasingDocumentItemChilds)
        {
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                POTrackingEntities db = new POTrackingEntities();
                CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
                string role = myUser.Roles;
                //using (TransactionScope transaction = new TransactionScope())
                //{
                if (purchasingDocumentItems != null)
                {
                    foreach (PurchasingDocumentItem item in purchasingDocumentItems)
                    {
                        PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == item.ID).FirstOrDefault();

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = Existed_PDI.ID;
                        notification.StatusID = 3;
                        //if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
                        if (role.ToLower() == LoginConstants.RoleVendor.ToLower() && (Existed_PDI.Quantity != item.ConfirmedQuantity || Existed_PDI.DeliveryDate != item.ConfirmedDate))
                        {
                            Existed_PDI.ConfirmedItem = null;
                            Existed_PDI.ActiveStage = "1";
                            Existed_PDI.ConfirmedDate = item.ConfirmedDate;
                            Existed_PDI.ConfirmedQuantity = item.ConfirmedQuantity;

                            notification.Stage = "1";
                            notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                        }
                        else
                        {
                            //Notification Existed_notification = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID && x.StatusID == 3).FirstOrDefault();
                            Notification Existed_notification = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID).FirstOrDefault();
                            if (Existed_notification != null)
                            {
                                Existed_notification.isActive = false;
                                Existed_notification.Modified = now;
                                Existed_notification.ModifiedBy = User.Identity.Name;
                            }
                            List<Notification> Existed_notificationCancels = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID).ToList();
                            foreach (var Existed_notificationCancel in Existed_notificationCancels)
                            {
                                Existed_notificationCancel.isActive = false;
                                Existed_notificationCancel.Modified = now;
                                Existed_notificationCancel.ModifiedBy = User.Identity.Name;
                            }

                            string vendorCode = db.POes.Where(x => x.ID == item.POID).Select(x => x.VendorCode).FirstOrDefault();
                            SubcontComponentCapability scc = db.SubcontComponentCapabilities.Where(x => x.VendorCode == vendorCode && x.Material == item.Material).FirstOrDefault();
                            int totalItemGR = Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;

                            if (Existed_PDI.Quantity == item.ConfirmedQuantity && Existed_PDI.DeliveryDate == item.ConfirmedDate)
                            {
                                Notification notificationInfo = new Notification();
                                notificationInfo.PurchasingDocumentItemID = Existed_PDI.ID;
                                notificationInfo.StatusID = 1;
                                notificationInfo.Stage = "1";
                                notificationInfo.Role = LoginConstants.RoleSubcontDev.ToLower();
                                notificationInfo.isActive = true;
                                notificationInfo.Created = now;
                                notificationInfo.CreatedBy = User.Identity.Name;
                                notificationInfo.Modified = now;
                                notificationInfo.ModifiedBy = User.Identity.Name;
                                db.Notifications.Add(notificationInfo);

                                Existed_PDI.ConfirmedDate = item.ConfirmedDate;
                                Existed_PDI.ConfirmedQuantity = item.ConfirmedQuantity;
                            }

                            Existed_PDI.ConfirmedItem = true;
                            if (scc != null)
                            {
                                if (scc.isNeedSequence == true)
                                {
                                    Existed_PDI.ActiveStage = "2";
                                    notification.Stage = "2";
                                    notification.Role = "vendor";
                                }
                                else
                                {
                                    if (item.ConfirmedQuantity > 0 && item.ConfirmedQuantity <= totalItemGR)
                                    {
                                        Existed_PDI.ActiveStage = "5";
                                        notification.Stage = "5";
                                        notification.Role = "vendor";
                                    }
                                    else
                                    {
                                        Existed_PDI.ActiveStage = "4";
                                        notification.Stage = "4";
                                        notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                                    }
                                }
                            }
                            else
                            {
                                Existed_PDI.ActiveStage = "2";
                                notification.Stage = "2";
                                notification.Role = LoginConstants.RoleVendor.ToLower();
                            }
                        }

                        Existed_PDI.LastModified = now;
                        Existed_PDI.LastModifiedBy = User.Identity.Name;

                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
                        {
                            // Child clean-up
                            List<Notification> notifications = db.Notifications.Where(x => x.PurchasingDocumentItem.ParentID == item.ID || x.PurchasingDocumentItemID == item.ID).ToList();
                            foreach (var notificationDatabasePurchasingDocumentItem in notifications)
                            {
                                db.Notifications.Remove(notificationDatabasePurchasingDocumentItem);
                            }
                            List<ProgressPhoto> progressPhotos = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItem.ParentID == item.ID || x.PurchasingDocumentItemID == item.ID).ToList();
                            foreach (var progressDatabasePurchasingDocumentItem in progressPhotos)
                            {
                                db.ProgressPhotoes.Remove(progressDatabasePurchasingDocumentItem);
                            }
                            List<PurchasingDocumentItem> childDatabasePurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == item.ID).ToList();
                            foreach (var childDatabasePurchasingDocumentItem in childDatabasePurchasingDocumentItems)
                            {
                                if (childDatabasePurchasingDocumentItem.ID != item.ID)
                                {
                                    db.PurchasingDocumentItems.Remove(childDatabasePurchasingDocumentItem);
                                }
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
                        PurchasingDocumentItem parent = db.PurchasingDocumentItems.Where(x => x.ID == item.ParentID).FirstOrDefault();

                        Notification notificationChild = new Notification();
                        notificationChild.StatusID = 3;

                        if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
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
                            db.SaveChanges();

                            notificationChild.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                            notificationChild.Stage = "1";
                            notificationChild.Role = LoginConstants.RoleSubcontDev.ToLower();
                        }
                        else
                        {
                            Notification Existed_notificationChild = db.Notifications.Where(x => x.PurchasingDocumentItem.ParentID == item.ParentID && x.StatusID == 3).FirstOrDefault();
                            if (Existed_notificationChild != null)
                            {
                                Existed_notificationChild.isActive = false;
                                Existed_notificationChild.Modified = now;
                                Existed_notificationChild.ModifiedBy = User.Identity.Name;
                            }
                            List<Notification> Existed_notificationCancels = db.Notifications.Where(x => x.PurchasingDocumentItem.ParentID == item.ParentID).ToList();
                            foreach (var Existed_notificationCancel in Existed_notificationCancels)
                            {
                                Existed_notificationCancel.isActive = false;
                                Existed_notificationCancel.Modified = now;
                                Existed_notificationCancel.ModifiedBy = User.Identity.Name;
                            }

                            PurchasingDocumentItem Existed_child = db.PurchasingDocumentItems.Where(x => x.ID == item.ID).FirstOrDefault();

                            string vendorCode = db.POes.Where(x => x.ID == item.POID).Select(x => x.VendorCode).FirstOrDefault();
                            SubcontComponentCapability scc = db.SubcontComponentCapabilities.Where(x => x.VendorCode == vendorCode && x.Material == item.Material).FirstOrDefault();
                            int totalItemGR = Existed_child.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_child.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;

                            if (Existed_child != null)
                            {
                                Existed_child.ConfirmedItem = true;
                                notificationChild.PurchasingDocumentItemID = Existed_child.ID;

                                if (scc != null)
                                {
                                    if (scc.isNeedSequence == true)
                                    {
                                        Existed_child.ActiveStage = "2";
                                        notificationChild.Stage = "2";
                                        notificationChild.Role = LoginConstants.RoleVendor.ToLower();
                                    }
                                    else
                                    {
                                        if (item.ConfirmedQuantity > 0 && item.ConfirmedQuantity <= totalItemGR)
                                        {
                                            Existed_child.ActiveStage = "6";
                                            notificationChild.Stage = "6";
                                            notificationChild.Role = LoginConstants.RoleVendor.ToLower();
                                        }
                                        else
                                        {
                                            Existed_child.ActiveStage = "4";
                                            notificationChild.Stage = "4";
                                            notificationChild.Role = LoginConstants.RoleSubcontDev.ToLower();
                                        }
                                    }
                                }
                                else
                                {
                                    Existed_child.ActiveStage = "2";
                                    notificationChild.Stage = "2";
                                    notificationChild.Role = LoginConstants.RoleVendor.ToLower();
                                }
                            }
                        }

                        notificationChild.isActive = true;
                        notificationChild.Created = now;
                        notificationChild.CreatedBy = User.Identity.Name;
                        notificationChild.Modified = now;
                        notificationChild.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notificationChild);
                    }
                }
                try
                {
                    db.SaveChanges();
                    //transaction.Complete();
                    return Json(new { success = true, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
                }
                //}
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
            //return Json(new { success = false, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SavePartialPurchasingDocumentItems(List<PurchasingDocumentItem> purchasingDocumentItems)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                POTrackingEntities db = new POTrackingEntities();
                CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
                string role = myUser.Roles;
                //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
                try
                {
                    foreach (PurchasingDocumentItem item in purchasingDocumentItems)
                    {
                        if (purchasingDocumentItems.First() == item)
                        {
                            PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == item.ID).FirstOrDefault();

                            Notification notification = new Notification();
                            notification.PurchasingDocumentItemID = Existed_PDI.ID;
                            notification.StatusID = 3;
                            if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
                            {
                                Existed_PDI.ConfirmedItem = null;
                                Existed_PDI.ActiveStage = "1";
                                Existed_PDI.ConfirmedQuantity = item.ConfirmedQuantity;
                                Existed_PDI.ConfirmedDate = item.ConfirmedDate;

                                notification.Stage = "1";
                                notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                            }
                            else
                            {
                                Notification Existed_notification = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID && x.StatusID == 3).FirstOrDefault();
                                if (Existed_notification != null)
                                {
                                    Existed_notification.isActive = false;
                                    Existed_notification.Modified = now;
                                    Existed_notification.ModifiedBy = User.Identity.Name;
                                }

                                string vendorCode = db.POes.Where(x => x.ID == item.POID).Select(x => x.VendorCode).FirstOrDefault();
                                SubcontComponentCapability scc = db.SubcontComponentCapabilities.Where(x => x.VendorCode == vendorCode && x.Material == item.Material).FirstOrDefault();
                                int totalItemGR = Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;

                                Existed_PDI.ConfirmedItem = true;

                                if (scc != null)
                                {
                                    if (scc.isNeedSequence == true)
                                    {
                                        Existed_PDI.ActiveStage = "2";
                                        notification.Stage = "2";
                                        notification.Role = LoginConstants.RoleVendor.ToLower();
                                    }
                                    else
                                    {
                                        if (item.ConfirmedQuantity > 0 && item.ConfirmedQuantity <= totalItemGR)
                                        {
                                            Existed_PDI.ActiveStage = "6";
                                            notification.Stage = "6";
                                            notification.Role = LoginConstants.RoleVendor.ToLower();
                                        }
                                        else
                                        {
                                            Existed_PDI.ActiveStage = "4";
                                            notification.Stage = "4";
                                            notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                                        }
                                    }
                                }
                                else
                                {
                                    Existed_PDI.ActiveStage = "2";
                                    notification.Stage = "2";
                                    notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                                }
                            }
                            Existed_PDI.LastModified = now;
                            Existed_PDI.LastModifiedBy = User.Identity.Name;

                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
                            {
                                // Child clean-up
                                List<PurchasingDocumentItem> childDatabasePurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == item.ParentID).ToList();
                                List<int> listIDPurchasingDocumentItems = childDatabasePurchasingDocumentItems.Select(x => x.ID).ToList();
                                if (listIDPurchasingDocumentItems.Count > 0)
                                {
                                    List<Notification> notifications = db.Notifications.Where(x => listIDPurchasingDocumentItems.Contains(x.PurchasingDocumentItemID)).ToList();
                                    foreach (var curentNotification in notifications)
                                    {
                                        db.Notifications.Remove(curentNotification);
                                    }
                                }

                                List<ProgressPhoto> progressPhotos = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItem.ParentID == item.ID || x.PurchasingDocumentItemID == item.ID).ToList();
                                foreach (var progressDatabasePurchasingDocumentItem in progressPhotos)
                                {
                                    db.ProgressPhotoes.Remove(progressDatabasePurchasingDocumentItem);
                                }

                                foreach (var childDatabasePurchasingDocumentItem in childDatabasePurchasingDocumentItems)
                                {
                                    db.PurchasingDocumentItems.Remove(childDatabasePurchasingDocumentItem);
                                }
                                // finish
                            }
                            db.Notifications.Add(notification);
                        }
                        else
                        {
                            PurchasingDocumentItem Parent_PDI = db.PurchasingDocumentItems.Where(x => x.ID == item.ParentID).FirstOrDefault();

                            if (Parent_PDI != null && role.ToLower() == LoginConstants.RoleVendor.ToLower())
                            {
                                Notification notificationChild = new Notification();

                                PurchasingDocumentItem purchasingDocumentItem = new PurchasingDocumentItem();
                                purchasingDocumentItem.POID = Parent_PDI.POID;
                                purchasingDocumentItem.ItemNumber = Parent_PDI.ItemNumber;
                                purchasingDocumentItem.Currency = Parent_PDI.Currency;
                                purchasingDocumentItem.Quantity = Parent_PDI.Quantity;
                                purchasingDocumentItem.NetPrice = Parent_PDI.NetPrice;
                                purchasingDocumentItem.NetValue = Parent_PDI.NetValue;
                                purchasingDocumentItem.Material = Parent_PDI.Material;
                                purchasingDocumentItem.Description = Parent_PDI.Description;
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
                                db.SaveChanges();

                                notificationChild.PurchasingDocumentItemID = purchasingDocumentItem.ID;
                                notificationChild.StatusID = 3;
                                notificationChild.Stage = "1";
                                notificationChild.Role = LoginConstants.RoleSubcontDev.ToLower();
                                notificationChild.isActive = true;
                                notificationChild.Created = now;
                                notificationChild.CreatedBy = User.Identity.Name;
                                notificationChild.Modified = now;
                                notificationChild.ModifiedBy = User.Identity.Name;

                                db.Notifications.Add(notificationChild);
                            }
                        }
                    }
                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return Json(new { success = true, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Exception(ex.Message + ex.StackTrace);
                    //return View("Error");
                    return Json(new { success = false, responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult SavePOItem(int pdItemID, int confirmedItemQty, DateTime confirmedDate, bool isCanceledPOItem)
        {
            POTrackingEntities db = new POTrackingEntities();
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles;
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).FirstOrDefault();
                if ((Existed_PDI.ActiveStageToNumber < 3 && role.ToLower() == LoginConstants.RoleVendor.ToLower()) || (Existed_PDI.ActiveStageToNumber < 3 && role.ToLower() == LoginConstants.RoleSubcontDev.ToLower()) || role.ToLower() == LoginConstants.RoleAdministrator.ToLower())
                {
                    //Notification Existed_notification = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID && x.StatusID == 3).FirstOrDefault();
                    Notification Existed_notification = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID).FirstOrDefault();
                    if (Existed_notification != null)
                    {
                        Existed_notification.isActive = false;
                        Existed_notification.Modified = now;
                        Existed_notification.ModifiedBy = User.Identity.Name;
                    }

                    List<Notification> Existed_notificationCancels = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID && x.Stage == "1").ToList();
                    foreach (var Existed_notificationCancel in Existed_notificationCancels)
                    {
                        Existed_notificationCancel.isActive = false;
                        Existed_notificationCancel.Modified = now;
                        Existed_notificationCancel.ModifiedBy = User.Identity.Name;
                    }

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = Existed_PDI.ID;

                    if (isCanceledPOItem)
                    {
                        Existed_PDI.ConfirmedItem = false;
                        //notification.StatusID = 2;
                        notification.Stage = "1";
                        if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
                        {
                            notification.StatusID = 2;
                            notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                        }
                        else
                        {
                            notification.StatusID = 2;
                            notification.Role = LoginConstants.RoleVendor.ToLower();

                            Notification notificationForDeleteItemSAP = new Notification();
                            notificationForDeleteItemSAP.PurchasingDocumentItemID = Existed_PDI.ID;
                            notificationForDeleteItemSAP.StatusID = 2;
                            notificationForDeleteItemSAP.Stage = "1";
                            notificationForDeleteItemSAP.Role = LoginConstants.RoleSubcontDev.ToLower();
                            notificationForDeleteItemSAP.isActive = true;
                            notificationForDeleteItemSAP.Created = now;
                            notificationForDeleteItemSAP.CreatedBy = User.Identity.Name;
                            notificationForDeleteItemSAP.Modified = now;
                            notificationForDeleteItemSAP.ModifiedBy = User.Identity.Name;
                            db.Notifications.Add(notificationForDeleteItemSAP);

                        }
                    }
                    else
                    {
                        notification.StatusID = 3;
                        //if (role.ToLower() == LoginConstants.RoleVendor.ToLower())
                        if (role.ToLower() == LoginConstants.RoleVendor.ToLower() && (Existed_PDI.Quantity != confirmedItemQty || Existed_PDI.DeliveryDate != confirmedDate))
                        {
                            Existed_PDI.ConfirmedItem = null;
                            Existed_PDI.ActiveStage = "1";
                            Existed_PDI.ConfirmedDate = confirmedDate;
                            Existed_PDI.ConfirmedQuantity = confirmedItemQty;

                            notification.Stage = "1";
                            notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                        }
                        else
                        {
                            string vendorCode = db.POes.Where(x => x.ID == Existed_PDI.POID).Select(x => x.VendorCode).FirstOrDefault();
                            SubcontComponentCapability scc = db.SubcontComponentCapabilities.Where(x => x.VendorCode == vendorCode && x.Material == Existed_PDI.Material).FirstOrDefault();
                            int totalItemGR = Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;

                            Existed_PDI.ConfirmedItem = true;

                            //kalo qty & date sama
                            if (Existed_PDI.Quantity == confirmedItemQty && Existed_PDI.DeliveryDate == confirmedDate)
                            {
                                Notification notificationInfo = new Notification();
                                notificationInfo.PurchasingDocumentItemID = Existed_PDI.ID;
                                notificationInfo.StatusID = 1;
                                notificationInfo.Stage = "1";
                                notificationInfo.Role = LoginConstants.RoleSubcontDev.ToLower();
                                notificationInfo.isActive = true;
                                notificationInfo.Created = now;
                                notificationInfo.CreatedBy = User.Identity.Name;
                                notificationInfo.Modified = now;
                                notificationInfo.ModifiedBy = User.Identity.Name;
                                db.Notifications.Add(notificationInfo);

                                Existed_PDI.ConfirmedDate = confirmedDate;
                                Existed_PDI.ConfirmedQuantity = confirmedItemQty;
                            }

                            if (scc != null)
                            {
                                if (scc.isNeedSequence == true)
                                {
                                    Existed_PDI.ActiveStage = "2";
                                    notification.Stage = "2";
                                    notification.Role = LoginConstants.RoleVendor.ToLower();
                                }
                                else
                                {
                                    if (Existed_PDI.ConfirmedQuantity > 0 && Existed_PDI.ConfirmedQuantity <= totalItemGR)
                                    {
                                        Existed_PDI.ActiveStage = "5";
                                        notification.Stage = "5";
                                        notification.Role = LoginConstants.RoleVendor.ToLower();
                                    }
                                    else
                                    {
                                        Existed_PDI.ActiveStage = "4";
                                        notification.Stage = "4";
                                        notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                                    }
                                }
                            }
                            else
                            {
                                Existed_PDI.ActiveStage = "2";
                                notification.Stage = "2";
                                notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                            }
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
                    return Json(new { success = true, responseCode = "200", responseText = "data updated" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseCode = "401", responseText = "Not Authorized" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //ViewBag.Exception(ex.Message + ex.StackTrace);
                //return View("Error");
                return Json(new { success = false, responseCode = "500", responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Stage 2

        [HttpGet]
        public JsonResult GetDataReasons()
        {
            POTrackingEntities db = new POTrackingEntities();
            try
            {
                object data = null;
                data = db.SequencesProgressReasons.Select(x =>
                new
                {
                    ID = x.ID,
                    Name = x.Name
                }).OrderBy(x => x.Name);

                if (data != null)
                {
                    return Json(new { success = true, responseCode = "200", responseText = "Bind Data Reason Success", data = JsonConvert.SerializeObject(data) }, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public JsonResult EditSequencesProgress(int pdItemID)
        {
            POTrackingEntities db = new POTrackingEntities();
            try
            {
                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).FirstOrDefault();

                if (purchasingDocumentItem != null)
                {
                    string progressName;
                    if (purchasingDocumentItem.PrimerActualDate != null)
                    {
                        PurchasingDocumentItemHistory purchasingDocumentItemHistory = db.PurchasingDocumentItemHistories.Where(x => x.PurchasingDocumentItemID == pdItemID || x.PurchasingDocumentItemID == purchasingDocumentItem.ParentID).FirstOrDefault();
                        if (purchasingDocumentItemHistory != null && purchasingDocumentItem.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity != null)
                        {
                            progressName = "Done";
                        }
                        else
                        {
                            progressName = "Primer";
                        }
                    }
                    else if (purchasingDocumentItem.FullweldActualDate != null)
                    {
                        progressName = "Fullweld";
                    }
                    else if (purchasingDocumentItem.SettingActualDate != null)
                    {
                        progressName = "Setting";
                    }
                    else
                    {
                        progressName = "PB";
                    }

                    return Json(new { success = true, responseCode = "200", responseText = "Bind Data Reason Success", data = progressName }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult GetSequenceData(int pdItemID)
        {
            POTrackingEntities db = new POTrackingEntities();
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
                    return Json(new { success = true, responseCode = "200", responseText = "OK", arrayDataTime = new { LeadTime = leadTime, PB = pb, Setting = setting, Fullweld = fullweld, Primer = primer }, isDisabled = isDisabled, isEditable = isEditable }, JsonRequestBehavior.AllowGet);
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
            POTrackingEntities db = new POTrackingEntities();
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).FirstOrDefault();
                //SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material).FirstOrDefault();

                List<Notification> Existed_notifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID && x.StatusID == 3 && x.Role.ToLower() == LoginConstants.RoleVendor.ToLower()).ToList();
                foreach (var Existed_notification in Existed_notifications)
                {
                    Existed_notification.isActive = false;
                    Existed_notification.Modified = now;
                    Existed_notification.ModifiedBy = User.Identity.Name;
                }

                Existed_PDI.ActiveStage = "3";
                Existed_PDI.PB = pb;
                Existed_PDI.Setting = setting;
                Existed_PDI.Fullweld = fullweld;
                Existed_PDI.Primer = primer;
                Existed_PDI.LastModified = now;
                Existed_PDI.LastModifiedBy = User.Identity.Name;

                Notification notificationTaskSubcontDev = new Notification();
                notificationTaskSubcontDev.PurchasingDocumentItemID = Existed_PDI.ID;
                notificationTaskSubcontDev.StatusID = 3;
                notificationTaskSubcontDev.Stage = "3";
                notificationTaskSubcontDev.Role = LoginConstants.RoleVendor.ToLower();
                notificationTaskSubcontDev.isActive = true;
                notificationTaskSubcontDev.Created = now;
                notificationTaskSubcontDev.CreatedBy = User.Identity.Name;
                notificationTaskSubcontDev.Modified = now;
                notificationTaskSubcontDev.ModifiedBy = User.Identity.Name;
                db.Notifications.Add(notificationTaskSubcontDev);

                Notification notificationInfoProcurement = new Notification();
                notificationInfoProcurement.PurchasingDocumentItemID = Existed_PDI.ID;
                notificationInfoProcurement.StatusID = 1;
                notificationInfoProcurement.Stage = "2";
                notificationInfoProcurement.Role = LoginConstants.RoleSubcontDev.ToLower();
                notificationInfoProcurement.isActive = true;
                notificationInfoProcurement.Created = now;
                notificationInfoProcurement.CreatedBy = User.Identity.Name;
                notificationInfoProcurement.Modified = now;
                notificationInfoProcurement.ModifiedBy = User.Identity.Name;
                db.Notifications.Add(notificationInfoProcurement);

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
            POTrackingEntities db = new POTrackingEntities();
            //return RedirectToAction("Index", new { searchPoNumber, searchStartPODate, searchEndPODate, page });
            try
            {
                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).FirstOrDefault();
                //double primerDays = Convert.ToDouble(purchasingDocumentItem.PB) * -1;

                SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material && x.VendorCode == purchasingDocumentItem.PO.VendorCode).FirstOrDefault();
                bool isDisabled = false;
                bool isEditable = false;
                int pb = purchasingDocumentItem.PB.HasValue ? purchasingDocumentItem.PB.Value : 0;
                int setting = purchasingDocumentItem.Setting.HasValue ? purchasingDocumentItem.Setting.Value : 0;
                int fullweld = purchasingDocumentItem.Fullweld.HasValue ? purchasingDocumentItem.Fullweld.Value : 0;
                int primer = purchasingDocumentItem.Primer.HasValue ? purchasingDocumentItem.Primer.Value : 0;

                int leadTime = subcontComponentCapability != null ? (subcontComponentCapability.PB + subcontComponentCapability.Setting + subcontComponentCapability.Fullweld + subcontComponentCapability.Primer) : 0;

                //int fullweldDays = primer;
                //int settingDays = fullweld + fullweldDays;
                //int pbDays = setting + settingDays;
                int fullweldDays = primer;
                int settingDays = fullweld + primer;
                int pbDays = setting + fullweld + primer;

                string primerDate = purchasingDocumentItem.ConfirmedDate.HasValue ? purchasingDocumentItem.ConfirmedDate.Value.ToString("dd/MM/yyyy") : "";
                string fullweldDate = purchasingDocumentItem.ConfirmedDate.HasValue ? purchasingDocumentItem.ConfirmedDate.Value.AddDays(fullweldDays * -1).ToString("dd/MM/yyyy") : "";
                string settingDate = purchasingDocumentItem.ConfirmedDate.HasValue ? purchasingDocumentItem.ConfirmedDate.Value.AddDays(settingDays * -1).ToString("dd/MM/yyyy") : "";
                string pbDate = purchasingDocumentItem.ConfirmedDate.HasValue ? purchasingDocumentItem.ConfirmedDate.Value.AddDays(pbDays * -1).ToString("dd/MM/yyyy") : "";

                string ATAPB = purchasingDocumentItem.PBActualDate.HasValue ? purchasingDocumentItem.PBActualDate.Value.ToString("dd/MM/yyyy") : "";
                string ATASetting = purchasingDocumentItem.SettingActualDate.HasValue ? purchasingDocumentItem.SettingActualDate.Value.ToString("dd/MM/yyyy") : "";
                string ATAFullweld = purchasingDocumentItem.FullweldActualDate.HasValue ? purchasingDocumentItem.FullweldActualDate.Value.ToString("dd/MM/yyyy") : "";
                string ATAPrimer = purchasingDocumentItem.PrimerActualDate.HasValue ? purchasingDocumentItem.PrimerActualDate.Value.ToString("dd/MM/yyyy") : "";

                int ATAPBReasonID = purchasingDocumentItem.PBLateReasonID.HasValue ? purchasingDocumentItem.PBLateReasonID.Value : 0;
                int ATASettingReasonID = purchasingDocumentItem.SettingLateReasonID.HasValue ? purchasingDocumentItem.SettingLateReasonID.Value : 0;
                int ATAFullweldReasonID = purchasingDocumentItem.FullweldLateReasonID.HasValue ? purchasingDocumentItem.FullweldLateReasonID.Value : 0;
                int ATAPrimerReasonID = purchasingDocumentItem.PremierLateReasonID.HasValue ? purchasingDocumentItem.PremierLateReasonID.Value : 0;

                var filePB = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == pdItemID && x.ProcessName == "PB").Select(x =>
                new
                {
                    id = x.ID,
                    fileName = x.FileName,
                    //url = Path.Combine("/", iisAppName, "Files/Subcont/SequencesProgress", x.FileName)
                    //url = "..\\" + iisAppName + "\\Files\\Subcont\\SequencesProgress\\" + x.FileName
                    url = string.IsNullOrEmpty(iisAppName) ? "\\Files\\Subcont\\SequencesProgress\\" + x.FileName : "\\" + iisAppName + "\\Files\\Subcont\\SequencesProgress\\" + x.FileName
                });

                var fileSetting = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == pdItemID && x.ProcessName == "Setting").Select(x =>
                new
                {
                    id = x.ID,
                    fileName = x.FileName,
                    url = string.IsNullOrEmpty(iisAppName) ? "\\Files\\Subcont\\SequencesProgress\\" + x.FileName : "\\" + iisAppName + "\\Files\\Subcont\\SequencesProgress\\" + x.FileName
                });

                var fileFullweld = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == pdItemID && x.ProcessName == "Fullweld").Select(x =>
                new
                {
                    id = x.ID,
                    fileName = x.FileName,
                    url = string.IsNullOrEmpty(iisAppName) ? "\\Files\\Subcont\\SequencesProgress\\" + x.FileName : "\\" + iisAppName + "\\Files\\Subcont\\SequencesProgress\\" + x.FileName
                });

                var filePrimer = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == pdItemID && x.ProcessName == "Primer").Select(x =>
                new
                {
                    id = x.ID,
                    fileName = x.FileName,
                    url = string.IsNullOrEmpty(iisAppName) ? "\\Files\\Subcont\\SequencesProgress\\" + x.FileName : "\\" + iisAppName + "\\Files\\Subcont\\SequencesProgress\\" + x.FileName
                });


                if (purchasingDocumentItem != null)
                {
                    return Json(new
                    {
                        success = true,
                        responseCode = "200",
                        responseText = "OK",
                        arrayDataFilePB = JsonConvert.SerializeObject(filePB),
                        arrayDataFileSetting = JsonConvert.SerializeObject(fileSetting),
                        arrayDataFileFullweld = JsonConvert.SerializeObject(fileFullweld),
                        arrayDataFilePrimer = JsonConvert.SerializeObject(filePrimer),
                        arrayDataTime = new { LeadTime = leadTime, PBDays = pb, SettingDays = setting, FullweldDays = fullweld, PrimerDays = primer, PB = pbDate, Setting = settingDate, Fullweld = fullweldDate, Primer = primerDate, ATAPB = ATAPB, ATASetting = ATASetting, ATAFullweld = ATAFullweld, ATAPrimer = ATAPrimer, ATAPBReasonID = ATAPBReasonID, ATASettingReasonID = ATASettingReasonID, ATAFullweldReasonID = ATAFullweldReasonID, ATAPrimerReasonID = ATAPrimerReasonID }
                    }, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveSequencesProgress(int pdItemID, DateTime? PBActualDate, DateTime? settingActualDate, DateTime? fullweldActualDate, DateTime? primerActualDate, int? PBActualReason, int? settingActualReason, int? fullweldActualReason, int? primerActualReason, HttpPostedFileBase[] invoices, string editableProgressName)
        {
            POTrackingEntities db = new POTrackingEntities();
            AlertToolsEntities alertDB = new AlertToolsEntities();
            //HttpPostedFileBase file = Request.Files["FileUpload"];
            try
            {
                PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).SingleOrDefault();
                int totalItemGR = Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.HasValue ? Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity.Value : 0;
                //List<ProgressPhoto> Existed_Attachments = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == pdItemID).ToList();
                //SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material).FirstOrDefault();

                List<Notification> Existed_notifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == Existed_PDI.ID && x.StatusID == 3 && x.Role.ToLower() == LoginConstants.RoleVendor.ToLower()).ToList();
                foreach (var Existed_notification in Existed_notifications)
                {
                    Existed_notification.isActive = false;
                    Existed_notification.Modified = now;
                    Existed_notification.ModifiedBy = User.Identity.Name;
                }

                if (invoices != null)
                {
                    var processName = "";

                    if (!string.IsNullOrEmpty(editableProgressName))
                    {
                        List<ProgressPhoto> progressPhotos = db.ProgressPhotoes.Where(x => x.PurchasingDocumentItemID == pdItemID && x.ProcessName.ToLower() == editableProgressName).ToList();
                        foreach (var progressDatabasePurchasingDocumentItem in progressPhotos)
                        {
                            db.ProgressPhotoes.Remove(progressDatabasePurchasingDocumentItem);
                        }
                        processName = editableProgressName;
                    }
                    else
                    {
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
                    }

                    foreach (var invoice in invoices)
                    {
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
                }

                Existed_PDI.PBActualDate = PBActualDate;
                Existed_PDI.SettingActualDate = settingActualDate;
                Existed_PDI.FullweldActualDate = fullweldActualDate;
                Existed_PDI.PrimerActualDate = primerActualDate;
                Existed_PDI.PBLateReasonID = PBActualReason;
                Existed_PDI.SettingLateReasonID = settingActualReason;
                Existed_PDI.FullweldLateReasonID = fullweldActualReason;
                Existed_PDI.PremierLateReasonID = primerActualReason;
                Existed_PDI.LastModified = now;
                Existed_PDI.LastModifiedBy = User.Identity.Name;

                //if (Existed_PDIHistory != null)
                //{
                if (Existed_PDI.ConfirmedQuantity == totalItemGR && (Existed_PDI.PBActualDate != null && Existed_PDI.SettingActualDate != null && Existed_PDI.FullweldActualDate != null && Existed_PDI.PrimerActualDate != null))
                {
                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = Existed_PDI.ID;
                    notification.StatusID = 3;
                    notification.Stage = "6";
                    notification.Role = LoginConstants.RoleVendor.ToLower();
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;
                    db.Notifications.Add(notification);
                    Existed_PDI.ActiveStage = "6";

                    //insert data QC to alert
                    int masterIssueID = alertDB.MasterIssues.Where(x => x.Name.ToLower().Contains("qc oh")).Select(x => x.ID).FirstOrDefault();
                    //try
                    //{
                    if (masterIssueID > 0)
                    {
                        IssueHeader issueHeader = new IssueHeader();
                        issueHeader.MasterIssueID = masterIssueID;
                        issueHeader.RaisedBy = User.Identity.Name;
                        issueHeader.DateOfIssue = now;
                        issueHeader.IssueDescription = "QC OH";
                        issueHeader.Created = now;
                        issueHeader.CreatedBy = User.Identity.Name;
                        issueHeader.LastModified = now;
                        issueHeader.LastModifiedBy = User.Identity.Name;
                        alertDB.IssueHeaders.Add(issueHeader);
                        alertDB.SaveChanges();

                        QualityControlPOTracking QCPotracking = new QualityControlPOTracking();
                        QCPotracking.IssueHeaderID = issueHeader.ID;
                        QCPotracking.PONumber = Existed_PDI.PO.Number;
                        QCPotracking.QADate = primerActualDate.Value;
                        QCPotracking.MaterialNumber = Existed_PDI.Material;
                        QCPotracking.MaterialName = Existed_PDI.Description;
                        QCPotracking.Quantity = Existed_PDI.ConfirmedQuantity.Value;
                        QCPotracking.Created = now;
                        QCPotracking.CreatedBy = User.Identity.Name;
                        QCPotracking.LastModified = now;
                        QCPotracking.LastModifiedBy = User.Identity.Name;
                        alertDB.QualityControlPOTrackings.Add(QCPotracking);
                        alertDB.SaveChanges();
                    }
                    //}
                    //catch (Exception)
                    //{
                    //throw;
                    //}

                }
                else if (Existed_PDI.PBActualDate != null && Existed_PDI.SettingActualDate != null && Existed_PDI.FullweldActualDate != null && Existed_PDI.PrimerActualDate != null)
                {
                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = Existed_PDI.ID;
                    notification.StatusID = 3;
                    notification.Stage = "4";
                    notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;
                    db.Notifications.Add(notification);
                    Existed_PDI.ActiveStage = "4";

                    //insert data QC to alert
                    int masterIssueID = alertDB.MasterIssues.Where(x => x.Name.ToLower().Contains("qc field")).Select(x => x.ID).FirstOrDefault();
                    //try
                    //{
                    if (masterIssueID > 0)
                    {
                        IssueHeader issueHeader = new IssueHeader();
                        issueHeader.MasterIssueID = masterIssueID;
                        issueHeader.RaisedBy = User.Identity.Name;
                        issueHeader.DateOfIssue = now;
                        issueHeader.IssueDescription = "QC Field";
                        issueHeader.Created = now;
                        issueHeader.CreatedBy = User.Identity.Name;
                        issueHeader.LastModified = now;
                        issueHeader.LastModifiedBy = User.Identity.Name;
                        alertDB.IssueHeaders.Add(issueHeader);
                        alertDB.SaveChanges();

                        QualityControlPOTracking QCPotracking = new QualityControlPOTracking();
                        QCPotracking.IssueHeaderID = issueHeader.ID;
                        QCPotracking.PONumber = Existed_PDI.PO.Number;
                        QCPotracking.QADate = primerActualDate.Value;
                        QCPotracking.MaterialNumber = Existed_PDI.Material;
                        QCPotracking.MaterialName = Existed_PDI.Description;
                        QCPotracking.Quantity = Existed_PDI.ConfirmedQuantity.Value;
                        QCPotracking.Created = now;
                        QCPotracking.CreatedBy = User.Identity.Name;
                        QCPotracking.LastModified = now;
                        QCPotracking.LastModifiedBy = User.Identity.Name;
                        alertDB.QualityControlPOTrackings.Add(QCPotracking);
                        alertDB.SaveChanges();
                    }
                    //}
                    //catch (Exception)
                    //{
                    //throw;
                    //}

                }
                else
                {
                    Notification notificationInfoProcurement = new Notification();
                    notificationInfoProcurement.PurchasingDocumentItemID = Existed_PDI.ID;
                    notificationInfoProcurement.StatusID = 1;
                    notificationInfoProcurement.Stage = "3";
                    notificationInfoProcurement.Role = LoginConstants.RoleSubcontDev.ToLower();
                    notificationInfoProcurement.isActive = true;
                    notificationInfoProcurement.Created = now;
                    notificationInfoProcurement.CreatedBy = User.Identity.Name;
                    notificationInfoProcurement.Modified = now;
                    notificationInfoProcurement.ModifiedBy = User.Identity.Name;
                    db.Notifications.Add(notificationInfoProcurement);

                    Notification notification = new Notification();
                    notification.PurchasingDocumentItemID = Existed_PDI.ID;
                    notification.StatusID = 3;
                    notification.Stage = "3";
                    notification.Role = LoginConstants.RoleVendor.ToLower();
                    notification.isActive = true;
                    notification.Created = now;
                    notification.CreatedBy = User.Identity.Name;
                    notification.Modified = now;
                    notification.ModifiedBy = User.Identity.Name;
                    db.Notifications.Add(notification);
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
                return Json(new { success = true, responseCode = "400", responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Stage 4

        [HttpPost]
        public ActionResult SaveInvoiceMethod(int pdItemID, string invoiceMethod)
        {
            POTrackingEntities db = new POTrackingEntities();
            try
            {
                PurchasingDocumentItem Existed_PDI = db.PurchasingDocumentItems.Where(x => x.ID == pdItemID).SingleOrDefault();
                List<PurchasingDocumentItem> Existed_PDIChilds = db.PurchasingDocumentItems.Where(x => x.ParentID == pdItemID).ToList();

                Existed_PDI.InvoiceMethod = invoiceMethod;
                if (Existed_PDI.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity >= Existed_PDI.ConfirmedQuantity && Existed_PDI.ConfirmedItem == true)
                {
                    Existed_PDI.ActiveStage = "6";
                }
                Existed_PDI.LastModified = now;
                Existed_PDI.LastModifiedBy = User.Identity.Name;

                foreach (var existed_PDIChild in Existed_PDIChilds)
                {
                    existed_PDIChild.InvoiceMethod = invoiceMethod;
                    if (existed_PDIChild.LatestPurchasingDocumentItemHistories.GoodsReceiptQuantity >= existed_PDIChild.ConfirmedQuantity && Existed_PDI.ConfirmedItem == true)
                    {
                        existed_PDIChild.ActiveStage = "6";
                    }
                    existed_PDIChild.LastModified = now;
                    existed_PDIChild.LastModifiedBy = User.Identity.Name;
                }

                Notification notification = new Notification();
                notification.PurchasingDocumentItemID = Existed_PDI.ID;
                notification.StatusID = 1;
                notification.Stage = "5";
                notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                notification.isActive = true;
                notification.Created = now;
                notification.CreatedBy = User.Identity.Name;
                notification.Modified = now;
                notification.ModifiedBy = User.Identity.Name;
                db.Notifications.Add(notification);

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
            POTrackingEntities db = new POTrackingEntities();
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
                            databasePurchasingDocumentItem.ActiveStage = "7";
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
                            notification.Role = LoginConstants.RoleSubcontDev.ToLower();
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);

                            db.SaveChanges();

                            //string downloadUrl = Path.Combine("..\\Files\\Subcont\\Invoice", fileName);
                            string downloadUrl = Path.Combine("/", iisAppName, "Files/Subcont/Invoice", fileName);

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
            POTrackingEntities db = new POTrackingEntities();
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

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID && x.StatusID == 3).ToList();
                        foreach (var previousNotification in previousNotifications)
                        {
                            previousNotification.isActive = false;
                        }

                        Notification notification = new Notification();
                        notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notification.StatusID = 2;
                        notification.Stage = "6";
                        notification.Role = LoginConstants.RoleSubcontDev.ToLower();
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
    }
}