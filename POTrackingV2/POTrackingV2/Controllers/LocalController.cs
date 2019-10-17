using POTrackingV2.Models;
using System;
using System.Data;
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
using Newtonsoft.Json;
using System.DirectoryServices;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleVendor + "," + LoginConstants.RoleProcurement)]
    public class LocalController : Controller
    {
        public DateTime now = DateTime.Now;
        private POTrackingEntities db = new POTrackingEntities();
        private AlertToolsEntities alertDB = new AlertToolsEntities();
        private string iisAppName = WebConfigurationManager.AppSettings["IISAppName"];

        #region PAGELIST
        // GET: Local
        public ActionResult Index(string searchPOStatus, string searchPONumber, string searchVendorName, string searchMaterial, string searchStartPODate, string searchEndPODate, string searchUserProcurement, int? page)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles.ToLower();
            var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();

            ViewBag.IsHeadProcurement = false;
            ViewBag.CurrentSearchPONumber = searchPONumber;
            ViewBag.CurrentSearchVendorName = searchVendorName;
            ViewBag.CurrentSearchMaterial = searchMaterial;
            ViewBag.CurrentStartPODate = searchStartPODate;
            ViewBag.CurrentEndPODate = searchEndPODate;
            ViewBag.CurrentRoleID = role.ToLower();
            ViewBag.CurrentSearchPOStatus = searchPOStatus;
            ViewBag.CurrentSearchUserProcurement = searchUserProcurement;
            ViewBag.IISAppName = iisAppName;

            List<DelayReason> delayReasons = db.DelayReasons.ToList();

            ViewBag.DelayReasons = delayReasons;

            var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
            var pOes = db.POes.Where(x => ((x.Type.ToLower() == "zo05" || x.Type.ToLower() == "zo09" || x.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.VendorCode)) &&
                                (x.PurchasingDocumentItems.Any(y => y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx" && !String.IsNullOrEmpty(y.Material) && y.PurchasingDocumentItemHistories.All(z => z.POHistoryCategory.ToLower() != "t"))) && (x.ReleaseDate != null))                               
                                .AsQueryable();
                        
            var noShowPOes = pOes;

            if (role == LoginConstants.RoleProcurement.ToLower())
            {
                List<string> myUserNRPs = new List<string>();

                myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
                myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

                if (myUserNRPs.Count > 2)
                {
                    ViewBag.IsHeadProcurement = true;
                }

                if (!string.IsNullOrEmpty(searchUserProcurement))
                {
                    myUserNRPs = GetChildNRPsByUsernameWithFilter(myUser.UserName, searchUserProcurement);
                }

                //var noShowPOes = db.POes.Where(x => (x.Type.ToLower() == "zo05" || x.Type.ToLower() == "zo09" || x.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.VendorCode))
                //                        .Where(x => x.PurchasingDocumentItems.Any(y => y.IsClosed.ToLower() != "x" && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx"))
                //                        .Where(x => x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material)));
                ////.Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null));

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
                //pOes = pOes.Include(x => x.PurchasingDocumentItems)
                //                .Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null))
                //               .AsQueryable();
            }
            else
            {
                pOes = pOes.Where(x => x.VendorCode == db.UserVendors.Where(y => y.Username == myUser.UserName).FirstOrDefault().VendorCode);
            }

                       


            if (!String.IsNullOrEmpty(searchPONumber))
            {
                pOes = pOes.Where(x => x.Number.Contains(searchPONumber));
            }

            if (!String.IsNullOrEmpty(searchVendorName))
            {
                pOes = pOes.Where(x => x.Vendor.Name.Contains(searchVendorName));
            }

            if (!String.IsNullOrEmpty(searchMaterial))
            {
                pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.Material.Contains(searchMaterial) || y.Description.Contains(searchMaterial) || y.MaterialVendor.Contains(searchMaterial)));
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

            if (!String.IsNullOrEmpty(searchPOStatus))
            {
                if (searchPOStatus.ToLower() == "ongoing")
                {
                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.ActiveStage != null && y.ActiveStage != "0") && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx"));
                }
                else if (searchPOStatus.ToLower() == "newpo")
                {
                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.ActiveStage == null || y.ActiveStage == "0") && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx"));
                }
                else if (role == LoginConstants.RoleProcurement.ToLower() || role == LoginConstants.RoleAdministrator.ToLower())
                {
                    if (searchPOStatus.ToLower() == "negotiated")
                    {
                        pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.ActiveStage == "1" && (y.ConfirmedQuantity != y.Quantity || y.ConfirmedDate != y.DeliveryDate) && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx"));
                    }
                    else
                    {
                        pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => (y.ActiveStage != null && y.ActiveStage != "0") && y.IsClosed.ToLower() != "l" && y.IsClosed.ToLower() != "lx"));
                    }
                }
            }

            return View(pOes.OrderBy(x => x.Number).ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
        }

        public ActionResult Report(string searchPONumber, string searchVendorName, string searchMaterial, int? page)
        {
            POTrackingEntities db = new POTrackingEntities();
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles.ToLower();
            string userName = User.Identity.Name;

            try
            {
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();

                var pOes = db.POes.AsQueryable();

                pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.IsClosed.ToLower() != "l" && x.IsClosed.ToLower() != "lx" && x.ActiveStage != null && x.ActiveStage != "0" && x.Material != "" && x.Material != null && x.PurchasingDocumentItemHistories.All(y => y.POHistoryCategory.ToLower() != "t")) && !vendorSubcont.Contains(po.VendorCode) && (po.ReleaseDate != null));

                var noShowPOes = pOes;

                if (role == LoginConstants.RoleProcurement.ToLower())
                {
                    List<string> myUserNRPs = new List<string>();
                    myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
                    myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

                   
                    if (myUserNRPs.Count > 0)
                    {
                        foreach (var myUserNRP in myUserNRPs)
                        {
                            noShowPOes = noShowPOes.Where(x => x.CreatedBy != myUserNRP);
                        }
                    }

                    pOes = pOes.Except(noShowPOes);
                }

                ViewBag.CurrentSearchPONumber = searchPONumber;
                ViewBag.CurrentSearchVendorName = searchVendorName;
                ViewBag.CurrentSearchMaterial = searchMaterial;                                
                ViewBag.CurrentRoleID = role.ToLower();                            
                ViewBag.IISAppName = iisAppName;

                if (!String.IsNullOrEmpty(searchPONumber))
                {
                    pOes = pOes.Where(x => x.Number.Contains(searchPONumber));
                }

                if (!String.IsNullOrEmpty(searchVendorName))
                {
                    pOes = pOes.Where(x => x.Vendor.Name.ToLower().Contains(searchVendorName.ToLower()));
                }

                if (!String.IsNullOrEmpty(searchMaterial))
                {
                    pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.Material.ToLower().Contains(searchMaterial.ToLower()) || y.Description.ToLower().Contains(searchMaterial.ToLower())));
                }
                pOes = pOes.OrderBy(x => x.Number);

                return View(pOes.ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
            }
            catch (Exception ex)
            {
                return View(ex.Message + "-----" + ex.StackTrace);
            }
        }

        public void DownloadReport(string searchPONumber, string searchVendorName, string searchMaterial)
        {
            POTrackingEntities db = new POTrackingEntities();
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles.ToLower();
            var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();

            if (!((myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "local") ||
                (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "subcont") ||
                (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "import") ||
                (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower())))
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", "ReportPOLocal.xls"));
                Response.ContentType = "application/ms-excel";
                DataTable dt = BindDataTable(searchPONumber, searchVendorName, searchMaterial);
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


        public DataTable BindDataTable(string searchPONumber, string searchVendorName, string searchMaterial)
        {
            POTrackingEntities db = new POTrackingEntities();
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles.ToLower();
            var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();

            if ((myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "local") ||
                (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "subcont") ||
                (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "import") ||
                (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower()))
            {
                return null;
            }
            var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();

            var pOes = db.POes.AsQueryable();
            pOes = pOes.Where(po => (po.Type.ToLower() == "zo05" || po.Type.ToLower() == "zo09" || po.Type.ToLower() == "zo10") && po.PurchasingDocumentItems.Any(x => x.Material != "" && x.Material != null && (x.ActiveStage != null && x.ActiveStage != "0") && (x.IsClosed == null || (x.IsClosed != null && x.IsClosed.ToLower() != "x" && x.IsClosed.ToLower() != "l" && x.IsClosed.ToLower() != "lx"))) && !vendorSubcont.Contains(po.VendorCode) && (po.ReleaseDate != null));


            var noShowPOes = pOes;
            List<string> myUserNRPs = new List<string>();

            myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
            myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

            if (myUserNRPs.Count > 0)
            {
                foreach (var myUserNRP in myUserNRPs)
                {
                    noShowPOes = noShowPOes.Where(x => x.CreatedBy != myUserNRP);
                }
            }

            pOes = pOes.Except(noShowPOes);

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


            #endregion

            DataTable dt = new DataTable();
            dt.Columns.Add("Number", typeof(Int32));
            dt.Columns.Add("PO Number", typeof(string));
            dt.Columns.Add("Item Number", typeof(string));
            dt.Columns.Add("Material", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Quantity", typeof(string));
            dt.Columns.Add("Delivery Date", typeof(string));
            dt.Columns.Add("Vendor", typeof(string));
            dt.Columns.Add("Estimated Time Arrival", typeof(string));

            int rowNumber = 1;

            foreach (var po in pOes)
            {
                var purchasingDocumentItems = po.PurchasingDocumentItems.Where(x => !String.IsNullOrEmpty(x.Material) && x.ActiveStage != null && x.ActiveStage != "0" && (x.IsClosed == null || (x.IsClosed != null && x.IsClosed.ToLower() != "x" && x.IsClosed.ToLower() != "l" && x.IsClosed.ToLower() != "lx")))
                                                                        .OrderBy(x => x.ItemNumber);

                foreach (var purchasingDocumentItem in purchasingDocumentItems)
                {
                    string deliveryDate = "-";
                    //string estimatedInspectionDate = "-";
                    string estimatedTimeArrival = "-";

                    if (purchasingDocumentItem.DeliveryDate != null)
                    {
                        deliveryDate = purchasingDocumentItem.DeliveryDate.GetValueOrDefault().ToString("dd/MM/yyyy");
                    }


                    if (purchasingDocumentItem.HasETAHistory)
                    {
                        if (purchasingDocumentItem.ETAHistories.Count < 2)
                        {
                            if (purchasingDocumentItem.FirstETAHistory.ETADate != null)
                            {
                                estimatedTimeArrival = purchasingDocumentItem.FirstETAHistory.ETADate.GetValueOrDefault().AddDays(2).ToString("dd/MM/yyyy");
                            }
                        }
                        else
                        {
                            if (purchasingDocumentItem.LastETAHistory.ETADate != null)
                            {
                                estimatedTimeArrival = purchasingDocumentItem.LastETAHistory.ETADate.GetValueOrDefault().AddDays(2).ToString("dd/MM/yyyy");
                            }
                        }
                    }

                    dt.Rows.Add(rowNumber, po.Number, purchasingDocumentItem.ItemNumber, purchasingDocumentItem.Material, purchasingDocumentItem.Description, purchasingDocumentItem.Quantity, deliveryDate, po.Vendor.Name, estimatedTimeArrival);

                    rowNumber++;
                }
            }

            return dt;
        }


        public ActionResult History(string searchPONumber, string searchVendorName, string searchMaterial, string searchStartPODate, string searchEndPODate, string searchUserProcurement, int? page)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles.ToLower();
            var roleType = db.UserRoleTypes.Where(x => x.Username == myUser.UserName).FirstOrDefault();
            var today = DateTime.Now;

            ViewBag.IsHeadProcurement = false;

            //ViewBag.MyUser = myUser;
            //ViewBag.Role = role;
            //ViewBag.RoleType = roleType.RolesType.Name.ToLower();

            //if (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "local")
            //{
            //    return RedirectToAction("Index", "Local");
            //}
            //if (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower() && roleType.RolesType.Name.ToLower() == "subcont")
            //{
            //    return RedirectToAction("Index", "SubCont");
            //}
            //if (myUser.Roles.ToLower() == LoginConstants.RoleSubcontDev.ToLower())
            //{
            //    return RedirectToAction("Index", "SubCont");
            //}

            var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
            var pOes = db.POes.Where(x => (x.Date.Year == today.Year || x.Date.Year == today.Year - 1) && (x.Type.ToLower() == "zo05" || x.Type.ToLower() == "zo09" || x.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.VendorCode) &&
                               (x.PurchasingDocumentItems.Any(y => y.IsClosed.ToLower() == "l" || y.IsClosed.ToLower() == "lx" || y.PurchasingDocumentItemHistories.Any(z => z.POHistoryCategory.ToLower() == "t") && !String.IsNullOrEmpty(y.Material))) && x.ReleaseDate != null )
                               .AsQueryable();
            //var pOes = db.POes.Include(x => x.PurchasingDocumentItems)
            //                    //.Where(x => x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material)))
            //                   .Where(x => (x.Type.ToLower() == "zo05" || x.Type.ToLower() == "zo09" || x.Type.ToLower() == "zo10") && (x.Date.Year == today.Year || x.Date.Year == today.Year - 1) && !vendorSubcont.Contains(x.VendorCode) && x.PurchasingDocumentItems.Any(y => !String.IsNullOrEmpty(y.Material)))
            //                   .Include(x => x.Vendor)
            //                   .AsQueryable();

            var noShowPOes = pOes;



            if (role == LoginConstants.RoleProcurement.ToLower())
            {
                List<string> myUserNRPs = new List<string>();
                myUserNRPs = GetChildNRPsByUsername(myUser.UserName);
                myUserNRPs.Add(GetNRPByUsername(myUser.UserName));

                if (myUserNRPs.Count > 2)
                {
                    ViewBag.IsHeadProcurement = true;
                }

                if (!string.IsNullOrEmpty(searchUserProcurement))
                {
                    myUserNRPs = GetChildNRPsByUsernameWithFilter(myUser.UserName, searchUserProcurement);
                }

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
                //pOes = pOes.Include(x => x.PurchasingDocumentItems)
                //                .Where(x => x.PurchasingDocumentItems.Any(y => y.ConfirmedQuantity != null || y.ConfirmedDate != null))
                //               .AsQueryable();
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
            ViewBag.CurrentSearchUserProcurement = searchUserProcurement;
            ViewBag.POCount = pOes.Count(); // DEBUG 
            ViewBag.IISAppName = iisAppName;

            List<DelayReason> delayReasons = db.DelayReasons.ToList();

            ViewBag.DelayReasons = delayReasons;


            if (!String.IsNullOrEmpty(searchPONumber))
            {
                pOes = pOes.Where(x => x.Number.Contains(searchPONumber));
            }

            if (!String.IsNullOrEmpty(searchVendorName))
            {
                pOes = pOes.Where(x => x.Vendor.Name.Contains(searchVendorName));
            }

            if (!String.IsNullOrEmpty(searchMaterial))
            {
                pOes = pOes.Where(x => x.PurchasingDocumentItems.Any(y => y.Material.Contains(searchMaterial) || y.Description.Contains(searchMaterial) || y.MaterialVendor.Contains(searchMaterial)));
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



            //return View(pOes.ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
            return View(pOes.OrderBy(x => x.Number).ToPagedList(page ?? 1, Constants.LoginConstants.PageSize));
        }

        [HttpGet]
        public JsonResult GetDataForSearch(string searchFilterBy, string value)
        {
            try
            {
                object data = null;
                value = value.ToLower();

                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();

                IEnumerable<Vendor> vendors = db.Vendors.Where(x => x.POes.Any(y => (y.Type.ToLower() == "zo05" || y.Type.ToLower() == "zo09" || y.Type.ToLower() == "zo10") && !vendorSubcont.Contains(y.VendorCode)));
                IEnumerable<PurchasingDocumentItem> purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => (x.PO.Type.ToLower() == "zo05" || x.PO.Type.ToLower() == "zo09" || x.PO.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.PO.VendorCode));


                if (searchFilterBy == "poNumber")
                {
                    data = db.POes.Where(x => x.Number.Contains(value) && (x.Type.ToLower() == "zo05" || x.Type.ToLower() == "zo09" || x.Type.ToLower() == "zo10")).Select(x =>
                     new
                     {
                         Data = x.Number,
                         MatchEvaluation = x.Number.ToLower().IndexOf(value)
                     }).Distinct().OrderBy(x => x.MatchEvaluation).Take(10);
                }
                else if (searchFilterBy == "vendor")
                {
                    //IEnumerable<Vendor> vendors = db.Vendors.Where(x => x.POes.Any(y => (y.Type.ToLower() == "zo05" || y.Type.ToLower() == "zo09" || y.Type.ToLower() == "zo10") && !vendorSubcont.Contains(y.VendorCode)));
                    data = vendors.Where(x => x.Name.ToLower().Contains(value.ToLower())).Select(x =>
                    new
                    {
                        Data = x.Name,
                        MatchEvaluation = x.Name.ToLower().IndexOf(value)
                    }).Distinct().OrderBy(x => x.MatchEvaluation).Take(10);
                }
                else if (searchFilterBy == "material")
                {
                    //IEnumerable<PurchasingDocumentItem> purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => (x.PO.Type.ToLower() == "zo05" || x.PO.Type.ToLower() == "zo09" || x.PO.Type.ToLower() == "zo10") && !vendorSubcont.Contains(x.PO.VendorCode));
                    data = purchasingDocumentItems.Where(x => x.Material.ToLower().Contains(value) || x.Description.ToLower().Contains(value) || x.MaterialVendor.ToLower().Contains(value)).Select(x =>
                    new
                    {
                        Data = x.Material.ToLower().StartsWith(value) ? x.Material : x.Description.ToLower().StartsWith(value) ? x.Description : (x.MaterialVendor??"").ToLower().StartsWith(value) ? x.MaterialVendor : x.Material.ToLower().Contains(value) ? x.Material : (x.MaterialVendor ?? "").ToLower().Contains(value) ? x.MaterialVendor : x.Description,
                        MatchEvaluation = (x.Material.ToLower().StartsWith(value) ? 1 : 0) + ((x.MaterialVendor ?? "").ToLower().StartsWith(value) ? 1 : 0) + (x.Description.ToLower().StartsWith(value) ? 1 : 0)
                    }).Distinct().OrderByDescending(x => x.MatchEvaluation).Take(10);
                }
                else if (searchFilterBy == "userProcurement")
                {
                    CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);

                    //Get Child User
                    List<string> userProcurementInferiors = new List<string>();

                    if (!string.IsNullOrEmpty(myUser.UserName))
                    {
                        UserProcurementSuperior userProcurementSuperior = db.UserProcurementSuperiors.Where(x => x.Username.ToLower() == myUser.UserName.ToLower()).SingleOrDefault();

                        if (userProcurementSuperior != null)
                        {
                            List<UserProcurementSuperior> childUsers = db.UserProcurementSuperiors.Where(x => x.ParentID == userProcurementSuperior.ID).ToList();

                            foreach (var childUser in childUsers)
                            {
                                if (!string.IsNullOrEmpty(childUser.Username))
                                {
                                    userProcurementInferiors.Add(childUser.Username);
                                }

                                List<UserProcurementSuperior> grandchildUsers = db.UserProcurementSuperiors.Where(x => x.ParentID == childUser.ID).ToList();

                                if (grandchildUsers.Count > 0)
                                {
                                    foreach (var grandchildUser in grandchildUsers)
                                    {
                                        if (!string.IsNullOrEmpty(grandchildUser.Username))
                                        {
                                            userProcurementInferiors.Add(grandchildUser.Username);
                                        }
                                    }
                                }
                            }
                        }

                        data = userProcurementInferiors.Select(x =>
                    new
                    {
                        Data = x,
                        MatchEvaluation = x.ToLower().IndexOf(value)
                    }).Distinct().OrderByDescending(x => x.MatchEvaluation).Take(10);
                    }
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

        public JsonResult GetUserDataFromAD(string username)
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

                return Json(new { sResultSet }, JsonRequestBehavior.AllowGet);
            }
            return null;
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

        public List<string> GetChildNRPsByUsernameWithFilter(string username, string searchUserProcurement)
        {
            List<UserProcurementSuperior> userProcurements = new List<UserProcurementSuperior>();

            if (!string.IsNullOrEmpty(username))
            {
                UserProcurementSuperior userProcurementSuperior = db.UserProcurementSuperiors.Where(x => x.Username.ToLower() == username.ToLower()).SingleOrDefault();

                if (userProcurementSuperior != null)
                {
                    userProcurements.Add(userProcurementSuperior);
                }

                if (userProcurementSuperior != null)
                {
                    List<UserProcurementSuperior> childUsers = db.UserProcurementSuperiors.Where(x => x.ParentID == userProcurementSuperior.ID).ToList();

                    foreach (var childUser in childUsers)
                    {
                        if (childUser != null)
                        {
                            userProcurements.Add(childUser);
                        }

                        List<UserProcurementSuperior> grandchildUsers = db.UserProcurementSuperiors.Where(x => x.ParentID == childUser.ID).ToList();

                        if (grandchildUsers.Count > 0)
                        {
                            foreach (var grandchildUser in grandchildUsers)
                            {
                                if (grandchildUser != null)
                                {
                                    userProcurements.Add(grandchildUser);
                                }
                            }
                        }
                    }
                }
            }

            userProcurements = userProcurements.Where(x => x.Username.ToLower().Contains(searchUserProcurement.ToLower())).ToList();

            return userProcurements.Select(x => x.NRP).ToList();
        }


        #endregion

        #region stage1
        // GET: Local/VendorConfirmItem
        [HttpPost]
        public ActionResult VendorConfirmQuantity(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
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
            List<bool> isTwentyFivePercents = new List<bool>();

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

                        if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0" || (databasePurchasingDocumentItem.ActiveStage == "2" && !databasePurchasingDocumentItem.HasETAHistory))
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
                                isTwentyFivePercents.Add(databasePurchasingDocumentItem.IsTwentyFivePercent);

                                Notification notificationVendor = new Notification();
                                notificationVendor.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                                notificationVendor.StatusID = 3;
                                notificationVendor.Stage = "1";
                                notificationVendor.Role = "vendor";
                                notificationVendor.isActive = true;
                                notificationVendor.Created = now;
                                notificationVendor.CreatedBy = User.Identity.Name;
                                notificationVendor.Modified = now;
                                notificationVendor.ModifiedBy = User.Identity.Name;

                                db.Notifications.Add(notificationVendor);

                                Notification notificationProcurement = new Notification();
                                notificationProcurement.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                                notificationProcurement.StatusID = 1;
                                notificationProcurement.Stage = "1";
                                notificationProcurement.Role = "procurement";
                                notificationProcurement.isActive = true;
                                notificationProcurement.Created = now;
                                notificationProcurement.CreatedBy = User.Identity.Name;
                                notificationProcurement.Modified = now;
                                notificationProcurement.ModifiedBy = User.Identity.Name;

                                db.Notifications.Add(notificationProcurement);
                            }
                            else
                            {
                                databasePurchasingDocumentItem.ConfirmedItem = null;
                                databasePurchasingDocumentItem.ActiveStage = "1";
                                isSameAsProcs.Add(false);
                                isTwentyFivePercents.Add(databasePurchasingDocumentItem.IsTwentyFivePercent);

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

                        if (databasePurchasingDocumentItem.ActiveStage == null || databasePurchasingDocumentItem.ActiveStage == "1" || databasePurchasingDocumentItem.ActiveStage == "0" || (databasePurchasingDocumentItem.ActiveStage == "2" && !databasePurchasingDocumentItem.HasETAHistory))
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
                            inputPurchasingDocumentItem.LeadTimeItem = databasePurchasingDocumentItem.LeadTimeItem;
                            inputPurchasingDocumentItem.PRNumber = databasePurchasingDocumentItem.PRNumber;
                            inputPurchasingDocumentItem.PRCreateDate = databasePurchasingDocumentItem.PRCreateDate;
                            inputPurchasingDocumentItem.PRReleaseDate = databasePurchasingDocumentItem.PRReleaseDate;
                            //inputPurchasingDocumentItem.DeliveryDate = databasePurchasingDocumentItem.DeliveryDate;

                            inputPurchasingDocumentItem.ActiveStage = "1";
                            inputPurchasingDocumentItem.Created = now;
                            inputPurchasingDocumentItem.CreatedBy = User.Identity.Name;
                            inputPurchasingDocumentItem.LastModified = now;
                            inputPurchasingDocumentItem.LastModifiedBy = User.Identity.Name;
                            isSameAsProcs.Add(false);
                            isTwentyFivePercents.Add(databasePurchasingDocumentItem.IsTwentyFivePercent);

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

                            db.SaveChanges();
                            counter++;
                        }
                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{counter} Item succesfully affected", isSameAsProcs, isTwentyFivePercents }, JsonRequestBehavior.AllowGet);
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
        //                    return Json(new { responseText = $"{counter} Item succesfully affected" }, JsonRequestBehavior.AllowGet);
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
        //                    //databasePurchasingDocumentItem.ConfirmedItem = null;
        //                    databasePurchasingDocumentItem.ConfirmedQuantity = inputPurchasingDocumentItem.ConfirmedQuantity;
        //                    databasePurchasingDocumentItem.ConfirmedDate = inputPurchasingDocumentItem.ConfirmedDate;
        //                    //databasePurchasingDocumentItem.ActiveStage = "1";
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
            if (!(myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower()|| myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower()))
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

                        Notification notificationProc = new Notification();
                        notificationProc.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notificationProc.StatusID = 1;
                        notificationProc.Stage = "1";
                        notificationProc.Role = "procurement";
                        notificationProc.isActive = true;
                        notificationProc.Created = now;
                        notificationProc.CreatedBy = User.Identity.Name;
                        notificationProc.Modified = now;
                        notificationProc.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notificationProc);

                        if (databasePurchasingDocumentItem.Quantity != databasePurchasingDocumentItem.ConfirmedQuantity)
                        {
                            Notification notificationSAP = new Notification();
                            notificationSAP.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                            notificationSAP.StatusID = 4;
                            notificationSAP.Stage = "1";
                            notificationSAP.Role = "procurement";
                            notificationSAP.isActive = true;
                            notificationSAP.Created = now;
                            notificationSAP.CreatedBy = User.Identity.Name;
                            notificationSAP.Modified = now;
                            notificationSAP.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notificationSAP);                            
                        }
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
                    CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);

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

                    if (myUser.Roles.ToLower() == LoginConstants.RoleVendor.ToLower())
                    {

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
                    }
                    else
                    {
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
         // POST: Local/VendorConfirmAllFirstETA
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
                List<bool> isSameAsProcs = new List<bool>();

                foreach (var inputETAHistory in inputETAHistories)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputETAHistory.PurchasingDocumentItemID);

                    if ((databasePurchasingDocumentItem.ActiveStage == "2" || (databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ApproveProformaInvoiceDocument == null)) && databasePurchasingDocumentItem.IsTwentyFivePercent)
                    {
                        List<ETAHistory> databaseEtaHistories = db.ETAHistories.Where(x => x.PurchasingDocumentItemID == inputETAHistory.PurchasingDocumentItemID).ToList();

                        if (databaseEtaHistories.Count > 0)
                        {
                            foreach (var databaseEtaHistory in databaseEtaHistories)
                            {
                                db.ETAHistories.Remove(databaseEtaHistory);
                            }
                        }

                        inputETAHistory.Created = now;
                        inputETAHistory.CreatedBy = user;
                        inputETAHistory.LastModified = now;
                        inputETAHistory.LastModifiedBy = user;

                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = user;

                        if (!databasePurchasingDocumentItem.HasETAHistory)
                        {
                            if (inputETAHistory.ETADate.GetValueOrDefault() == databasePurchasingDocumentItem.ConfirmedDate)
                            {
                                inputETAHistory.AcceptedByProcurement = true;
                                databasePurchasingDocumentItem.ActiveStage = "2a";

                                db.ETAHistories.Add(inputETAHistory);

                                List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
                                foreach (var previousNotification in previousNotifications)
                                {
                                    previousNotification.isActive = false;
                                }

                                Notification notificationProcurement = new Notification();
                                notificationProcurement.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                                notificationProcurement.StatusID = 3;
                                notificationProcurement.Stage = "2";
                                notificationProcurement.Role = "procurement";
                                notificationProcurement.isActive = true;
                                notificationProcurement.Created = now;
                                notificationProcurement.CreatedBy = User.Identity.Name;
                                notificationProcurement.Modified = now;
                                notificationProcurement.ModifiedBy = User.Identity.Name;

                                db.Notifications.Add(notificationProcurement);

                                Notification notificationVendor = new Notification();
                                notificationVendor.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                                notificationVendor.StatusID = 1;
                                notificationVendor.Stage = "2";
                                notificationVendor.Role = "vendor";
                                notificationVendor.isActive = true;
                                notificationVendor.Created = now;
                                notificationVendor.CreatedBy = User.Identity.Name;
                                notificationVendor.Modified = now;
                                notificationVendor.ModifiedBy = User.Identity.Name;

                                db.Notifications.Add(notificationVendor);

                                isSameAsProcs.Add(true);
                            }
                            else
                            {
                                inputETAHistory.AcceptedByProcurement = null;
                                databasePurchasingDocumentItem.ActiveStage = "2";

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

                                isSameAsProcs.Add(false);
                            }
                        }

                        count++;
                    }
                    //else if (databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ProformaInvoiceDocument == null) // EDIT
                    //{
                    //    List<ETAHistory> databaseEtaHistories = db.ETAHistories.Where(x => x.PurchasingDocumentItemID == inputETAHistory.PurchasingDocumentItemID).ToList();

                    //    foreach (var databaseEtaHistory in databaseEtaHistories)
                    //    {
                    //        db.ETAHistories.Remove(databaseEtaHistory);
                    //    }

                    //    inputETAHistory.Created = now;
                    //    inputETAHistory.CreatedBy = user;
                    //    inputETAHistory.LastModified = now;
                    //    inputETAHistory.LastModifiedBy = user;

                    //    //databasePurchasingDocumentItem.ActiveStage = "2a";
                    //    databasePurchasingDocumentItem.LastModified = now;
                    //    databasePurchasingDocumentItem.LastModifiedBy = user;

                    //    if (!databasePurchasingDocumentItem.HasETAHistory)
                    //    {
                    //        db.ETAHistories.Add(inputETAHistory);
                    //    }

                    //    count++;
                    //}
                }

                db.SaveChanges();
                return Json(new { responseText = $"{count} item affected", isSameAsProcs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return Json(new { responseText = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Local/ProcurementAcceptFirstEta
        [HttpPost]
        public ActionResult ProcurementAcceptFirstEta(List<int> inputPurchasingDocumentItemIDs)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (!(myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower()))
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

                    if ((databasePurchasingDocumentItem.ActiveStage == "2" || (databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ProformaInvoiceDocument == null)) && databasePurchasingDocumentItem.IsTwentyFivePercent)
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

                        // Set OpenQuantity
                        if (databasePurchasingDocumentItem.ParentID == null || databasePurchasingDocumentItem.ID == databasePurchasingDocumentItem.ParentID)
                        {
                            int quantity = databasePurchasingDocumentItem.Quantity;
                            quantity = quantity - databasePurchasingDocumentItem.ConfirmedQuantity.GetValueOrDefault();

                            // look for child
                            List<PurchasingDocumentItem> childDatabasePurchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ParentID == databasePurchasingDocumentItem.ID && x.ID != x.ParentID).ToList();

                            if (childDatabasePurchasingDocumentItems.Count > 0)
                            {
                                foreach (var childDatabasePurchasingDocumentItem in childDatabasePurchasingDocumentItems)
                                {
                                    quantity = quantity - childDatabasePurchasingDocumentItem.ConfirmedQuantity.GetValueOrDefault();
                                }
                            }

                        }
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

        // POST: Local/ProcurementDeclineFirstEta
        [HttpPost]
        public ActionResult ProcurementDeclineFirstEta(List<int> inputPurchasingDocumentItemIDs)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (!(myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower()))
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

                    if ((databasePurchasingDocumentItem.ActiveStage == "2" || (databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ProformaInvoiceDocument == null)) && databasePurchasingDocumentItem.IsTwentyFivePercent)
                    {
                        ETAHistory firstETAHistory = databasePurchasingDocumentItem.FirstETAHistory;

                        firstETAHistory.AcceptedByProcurement = false;
                        firstETAHistory.LastModified = now;
                        firstETAHistory.LastModifiedBy = user;

                        databasePurchasingDocumentItem.ActiveStage = "2";
                        databasePurchasingDocumentItem.ConfirmedItem = false;
                        databasePurchasingDocumentItem.LastModified = now;
                        databasePurchasingDocumentItem.LastModifiedBy = user;

                        List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID && x.StatusID == 3).ToList();
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

        #region stage 2a (uploadproformainvoice)
        [HttpPost]
        public ActionResult VendorUploadInvoice (int inputPurchasingDocumentItemID,HttpPostedFileBase fileProformaInvoice)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (fileProformaInvoice.ContentLength > 0 && databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ApproveProformaInvoiceDocument == true)
                {
                    string user = User.Identity.Name;

                    string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{Path.GetFileName(fileProformaInvoice.FileName)}";
                    string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/ProformaInvoice"), fileName);

                    using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                    {
                        fileProformaInvoice.InputStream.CopyTo(fileStream);
                    }

                    databasePurchasingDocumentItem.ProformaInvoiceDocument = fileName;
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

                    string downloadUrl = Path.Combine("/", iisAppName, "Files/Local/ProformaInvoice", fileName);

                    return Json(new { responseText = $"Proforma Invoice successfully uploaded", proformaInvoiceUrl = downloadUrl }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { responseText = $"Proforma Invoice not uploaded" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult VendorUploadAllProformaInvoice(List<int> inputPurchasingDocumentItemIDs, HttpPostedFileBase[] fileProformaInvoices)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                int count = 0;
                List<string> downloadURLs = new List<string>();

                foreach (var inputPurchasingDocumentItemID in inputPurchasingDocumentItemIDs)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

                    if (fileProformaInvoices[count].ContentLength > 0 && databasePurchasingDocumentItem.ActiveStage == "2a" && databasePurchasingDocumentItem.ApproveProformaInvoiceDocument == true)
                    {
                        string user = User.Identity.Name;

                        string fileName = $"{inputPurchasingDocumentItemID.ToString()}_{Path.GetFileName(fileProformaInvoices[count].FileName)}";
                        string uploadPathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/ProformaInvoice"), fileName);

                        using (FileStream fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                        {
                            fileProformaInvoices[count].InputStream.CopyTo(fileStream);
                        }

                        databasePurchasingDocumentItem.ProformaInvoiceDocument = fileName;
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
                        notification.StatusID = 1;
                        notification.Stage = "2a";
                        notification.Role = "procurement";
                        notification.isActive = true;
                        notification.Created = now;
                        notification.CreatedBy = User.Identity.Name;
                        notification.Modified = now;
                        notification.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notification);

                        downloadURLs.Add(Path.Combine("/", iisAppName, "Files/Local/ProformaInvoice", fileName));
                        count++;
                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{count} Proforma Invoice successfully uploaded", proformaInvoiceUrls = downloadURLs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        [HttpPost]
        public ActionResult VendorRemoveProformaInvoice(int inputPurchasingDocumentItemID)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage == "3" & databasePurchasingDocumentItem.ConfirmReceivedPaymentDate == null)
                {
                    if (databasePurchasingDocumentItem.ProformaInvoiceDocument != null)
                    {
                        string user = User.Identity.Name;

                        string pathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/ProformaInvoice"), databasePurchasingDocumentItem.ProformaInvoiceDocument);

                        System.IO.File.Delete(pathWithfileName);

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
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        [HttpPost]
        public ActionResult ProcurementAskProformaInvoice(List<int> inputPurchasingDocumentItemIDs)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (!(myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower()))
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                int count = 0;

                foreach (var inputPurchasingDocumentItemID in inputPurchasingDocumentItemIDs)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

                    if (databasePurchasingDocumentItem.ActiveStage == "2a" || databasePurchasingDocumentItem.ActiveStage == "3")
                    {
                        string user = User.Identity.Name;

                        databasePurchasingDocumentItem.ActiveStage = "2a";
                        databasePurchasingDocumentItem.ProformaInvoiceDocument = null;
                        databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = true;
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

                        count++;
                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{count} Stage(s) defined as need for Proforma Invoice" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        [HttpPost]
        public ActionResult ProcurementSkipProformaInvoice(List<int> inputPurchasingDocumentItemIDs)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (!(myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower()))
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }


            try
            {
                int count = 0;

                foreach (var inputPurchasingDocumentItemID in inputPurchasingDocumentItemIDs)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

                    if (databasePurchasingDocumentItem.ActiveStage == "2a" || databasePurchasingDocumentItem.ActiveStage == "3")
                    {
                        string user = User.Identity.Name;

                        databasePurchasingDocumentItem.ActiveStage = "3";
                        databasePurchasingDocumentItem.ProformaInvoiceDocument = null;
                        databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = false;
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

                return Json(new { responseText = $"{count} Stage(s) successfully Skipped" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }

        //[HttpPost]
        //public ActionResult VendorSkipPI(int inputPurchasingDocumentItemID)
        //{
        //    CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
        //    if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
        //    {
        //        return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
        //    }

        //    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

        //    try
        //    {
        //        //PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItem.ID);

        //        if (databasePurchasingDocumentItem.ActiveStage == "2a" || databasePurchasingDocumentItem.ActiveStage == "3")
        //        {
        //            string user = User.Identity.Name;

        //            //databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = false;
        //            databasePurchasingDocumentItem.ActiveStage = "3";
        //            databasePurchasingDocumentItem.ProformaInvoiceDocument = null;
        //            databasePurchasingDocumentItem.ApproveProformaInvoiceDocument = null;
        //            databasePurchasingDocumentItem.LastModified = now;
        //            databasePurchasingDocumentItem.LastModifiedBy = user;

        //            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
        //            foreach (var previousNotification in previousNotifications)
        //            {
        //                previousNotification.isActive = false;
        //            }

        //            Notification notification = new Notification();
        //            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
        //            notification.StatusID = 1;
        //            notification.Stage = "2a";
        //            notification.Role = "procurement";
        //            notification.isActive = true;
        //            notification.Created = now;
        //            notification.CreatedBy = User.Identity.Name;
        //            notification.Modified = now;
        //            notification.ModifiedBy = User.Identity.Name;

        //            db.Notifications.Add(notification);

        //            db.SaveChanges();

        //            return Json(new { responseText = $"Proforma Invoice successfully skipped" }, JsonRequestBehavior.AllowGet);
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

        #region stage 3 (procurementconfirmpaymentreceived)
        [HttpPost]
        public ActionResult ProcurementConfirmPaymentReceived(List<PurchasingDocumentItem> inputPurchasingDocumentItems)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (!(myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower()))
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

                        Notification notificationVendor = new Notification();
                        notificationVendor.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notificationVendor.StatusID = 3;
                        notificationVendor.Stage = "3";
                        notificationVendor.Role = "vendor";
                        notificationVendor.isActive = true;
                        notificationVendor.Created = now;
                        notificationVendor.CreatedBy = User.Identity.Name;
                        notificationVendor.Modified = now;
                        notificationVendor.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notificationVendor);

                        Notification notificationProc = new Notification();
                        notificationProc.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notificationProc.StatusID = 1;
                        notificationProc.Stage = "3";
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

                return Json(new { responseText = $"{count} data affected" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errorMessage = (ex.Message + ex.StackTrace);
                return View(errorMessage);
            }
        }

        [HttpPost]
        public ActionResult ProcurementSkipConfirmPayment(List<int> inputPurchasingDocumentItemIDs)
        {
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            if (!(myUser.Roles.ToLower() == LoginConstants.RoleProcurement.ToLower() || myUser.Roles.ToLower() == LoginConstants.RoleAdministrator.ToLower()))
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }
            
            try
            {
                int counter = 0;

                foreach (var inputPurchasingDocumentItemID in inputPurchasingDocumentItemIDs)
                {
                    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

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

                        Notification notificationVendor = new Notification();
                        notificationVendor.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notificationVendor.StatusID = 3;
                        notificationVendor.Stage = "3";
                        notificationVendor.Role = "vendor";
                        notificationVendor.isActive = true;
                        notificationVendor.Created = now;
                        notificationVendor.CreatedBy = User.Identity.Name;
                        notificationVendor.Modified = now;
                        notificationVendor.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notificationVendor);

                        Notification notificationProc = new Notification();
                        notificationProc.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                        notificationProc.StatusID = 1;
                        notificationProc.Stage = "3";
                        notificationProc.Role = "procurement";
                        notificationProc.isActive = true;
                        notificationProc.Created = now;
                        notificationProc.CreatedBy = User.Identity.Name;
                        notificationProc.Modified = now;
                        notificationProc.ModifiedBy = User.Identity.Name;

                        db.Notifications.Add(notificationProc);

                        counter++;                        
                    }
                }

                db.SaveChanges();

                return Json(new { responseText = $"{counter} data successfully Skipped" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + " --- " + ex.StackTrace;

                return View(errorMessage);
            }
        }
        //[HttpPost]
        //public ActionResult VendorSkipConfirmPayment(int inputPurchasingDocumentItemID)
        //{
        //    CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
        //    if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
        //    {
        //        return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
        //    }

        //    PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

        //    try
        //    {
        //        if (databasePurchasingDocumentItem.ActiveStage == "3" || databasePurchasingDocumentItem.ActiveStage == "4")
        //        {
        //            string user = User.Identity.Name;

        //            databasePurchasingDocumentItem.ConfirmReceivedPaymentDate = null;
        //            databasePurchasingDocumentItem.ActiveStage = "4";
        //            databasePurchasingDocumentItem.LastModified = now;
        //            databasePurchasingDocumentItem.LastModifiedBy = user;

        //            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID).ToList();
        //            foreach (var previousNotification in previousNotifications)
        //            {
        //                previousNotification.isActive = false;
        //            }

        //            Notification notification = new Notification();
        //            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
        //            notification.StatusID = 1;
        //            notification.Stage = "3";
        //            notification.Role = "procurement";
        //            notification.isActive = true;
        //            notification.Created = now;
        //            notification.CreatedBy = User.Identity.Name;
        //            notification.Modified = now;
        //            notification.ModifiedBy = User.Identity.Name;

        //            db.Notifications.Add(notification);

        //            db.SaveChanges();

        //            return Json(new { responseText = $"Stage successfully Skipped" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { responseText = $"Stage failed to skip" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMessage = ex.Message + " --- " + ex.StackTrace;

        //        return View(errorMessage);
        //    }
        //}
        #endregion

        #region stage 4 (ETAOntimeDelay)
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
                if (purchasingDocumentItem.ActiveStage == "4" && purchasingDocumentItem.IsSeventyFivePercent)
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

                        #region insert data 25% to 75% procurement to Alert Tools
                        if (inputETAHistory.ETADate > purchasingDocumentItem.FirstETAHistory.ETADate.Value)
                        {
                            int masterIssueID = alertDB.MasterIssues.Where(x => x.Name.ToLower().Contains("material procurement")).Select(x => x.ID).FirstOrDefault();

                            if (masterIssueID > 0)
                            {
                                IssueHeader issueHeader = new IssueHeader();
                                issueHeader.MasterIssueID = masterIssueID;
                                issueHeader.RaisedBy = User.Identity.Name;
                                issueHeader.DateOfIssue = now;
                                issueHeader.IssueDescription = "Material Procurement Local";
                                issueHeader.Created = now;
                                issueHeader.CreatedBy = User.Identity.Name;
                                issueHeader.LastModified = now;
                                issueHeader.LastModifiedBy = User.Identity.Name;
                                alertDB.IssueHeaders.Add(issueHeader);
                                alertDB.SaveChanges();

                                MaterialProcurementPOTracking materialProcurementPOTracking = new MaterialProcurementPOTracking();
                                materialProcurementPOTracking.IssueHeaderID = issueHeader.ID;
                                materialProcurementPOTracking.PONumber = purchasingDocumentItem.PO.Number;
                                materialProcurementPOTracking.ETADate = purchasingDocumentItem.FirstETAHistory.ETADate.Value;
                                materialProcurementPOTracking.ConfirmedETADate = inputETAHistory.ETADate.Value;
                                materialProcurementPOTracking.MaterialNumber = purchasingDocumentItem.Material;
                                materialProcurementPOTracking.MaterialName = purchasingDocumentItem.Description;
                                materialProcurementPOTracking.Quantity = purchasingDocumentItem.ConfirmedQuantity.Value;
                                materialProcurementPOTracking.Created = now;
                                materialProcurementPOTracking.CreatedBy = User.Identity.Name;
                                materialProcurementPOTracking.LastModified = now;
                                materialProcurementPOTracking.LastModifiedBy = User.Identity.Name;
                                alertDB.MaterialProcurementPOTrackings.Add(materialProcurementPOTracking);
                                alertDB.SaveChanges();
                            }
                        }
                        #endregion

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

            if (databasePurchasingDocumentItem.ProgressPhotoes.Count < 1 && databasePurchasingDocumentItem.IsSeventyFivePercent)
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
                //string path = $"../Files/Local/ProgressProforma/{progressPhoto.FileName}";
                string downloadurl = Path.Combine("/", iisAppName, "Files/Local/ProgressProforma", progressPhoto.FileName);
                imageSources.Add(downloadurl);
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
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage != "2a")
                {
                    if (fileInvoice.ContentLength > 0 || Convert.ToInt32(databasePurchasingDocumentItem.ActiveStage) > 3)
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

                            databasePurchasingDocumentItem.ActiveStage = "6";
                            databasePurchasingDocumentItem.InvoiceDocument = fileName;
                            databasePurchasingDocumentItem.LastModified = now;
                            databasePurchasingDocumentItem.LastModifiedBy = user;

                            List<Notification> previousNotifications = db.Notifications.Where(x => x.PurchasingDocumentItemID == databasePurchasingDocumentItem.ID && x.StatusID == 3).ToList();
                            foreach (var previousNotification in previousNotifications)
                            {
                                previousNotification.isActive = false;
                            }

                            Notification notification = new Notification();
                            notification.PurchasingDocumentItemID = databasePurchasingDocumentItem.ID;
                            notification.StatusID = 1;
                            notification.Stage = "6";
                            notification.Role = "procurement";
                            notification.isActive = true;
                            notification.Created = now;
                            notification.CreatedBy = User.Identity.Name;
                            notification.Modified = now;
                            notification.ModifiedBy = User.Identity.Name;

                            db.Notifications.Add(notification);

                            db.SaveChanges();

                            string downloadUrl = Path.Combine("/", iisAppName, "Files/Local/Invoice", fileName);

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
            if (myUser.Roles.ToLower() != LoginConstants.RoleVendor.ToLower())
            {
                return Json(new { responseText = $"You are not Authorized" }, JsonRequestBehavior.AllowGet);
            }

            PurchasingDocumentItem databasePurchasingDocumentItem = db.PurchasingDocumentItems.Find(inputPurchasingDocumentItemID);

            try
            {
                if (databasePurchasingDocumentItem.ActiveStage != "2a")
                {
                    if (Convert.ToInt32(databasePurchasingDocumentItem.ActiveStage) > 3)
                    {
                        if (databasePurchasingDocumentItem.InvoiceDocument != null)
                        {
                            string user = User.Identity.Name;

                            string pathWithfileName = Path.Combine(Server.MapPath("~/Files/Local/Invoice"), databasePurchasingDocumentItem.InvoiceDocument);

                            System.IO.File.Delete(pathWithfileName);

                            databasePurchasingDocumentItem.ActiveStage = "5";
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
