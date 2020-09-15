using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using POTrackingV2.Models;
using POTrackingV2.ViewModels;
using POTrackingV2.Constants;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using POTrackingV2.CustomAuthentication;
using Newtonsoft.Json;
using System.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data.OleDb;
using System.Data;
using LinqToExcel;
using System.Data.Entity.Validation;
using System.Web.Security;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator)]
    public class MasterVendorController : Controller
    {
        List<RolesType> listRoleType = new List<RolesType>();
        List<Vendor> listVendor = new List<Vendor>();
        //SubcontDevVendorViewModelEdit subcontDevVendorViewModelEdit = new SubcontDevVendorViewModelEdit();

        // GET: MasterVendor
        public ActionResult Index(string searchBy, string search, int? page)
        {
            ViewBag.CurrentSearchFilterBy = searchBy;
            ViewBag.CurrentSearchString = search;
            using (POTrackingEntities db = new POTrackingEntities())
            {
                if (searchBy == "description")
                {
                    return View(db.SubcontComponentCapabilities.Where(x => x.Description.Contains(search) || search == null).OrderBy(x => x.VendorCode.Length).ThenBy(x => x.VendorCode).ToList().ToPagedList(page ?? 1, 10));
                }
                else if (searchBy == "material")
                {
                    return View(db.SubcontComponentCapabilities.Where(x => x.Material.Contains(search) || search == null).OrderBy(x => x.VendorCode.Length).ThenBy(x => x.VendorCode).ToList().ToPagedList(page ?? 1, 10));
                }
                else
                {
                    return View(db.SubcontComponentCapabilities.Where(x => x.VendorCode.Contains(search) || search == null).OrderBy(x => x.VendorCode.Length).ThenBy(x => x.VendorCode).ToList().ToPagedList(page ?? 1, 10));
                }
            }
        }
        // GET: CustomMasterVendor //Fahmi
        public ActionResult IndexCustom(string searchBy, string search, int? page)
        {
            POTrackingEntities db = new POTrackingEntities();
            string userName = User.Identity.Name;
            DateTime today = DateTime.Now;
            CustomMembershipUser myUser = (CustomMembershipUser)Membership.GetUser(User.Identity.Name, false);
            string role = myUser.Roles;
            string roleVendor = LoginConstants.RoleVendor.ToLower();
            try
            {
                var vendorSubcont = db.SubcontComponentCapabilities.Select(x => x.VendorCode).Distinct();
                SubcontDevUserRole subcontDevUserRole = db.SubcontDevUserRoles.Where(x => x.Username == userName).FirstOrDefault();

                var pOes = (from pdi in db.PurchasingDocumentItems
                            join p in db.POes on pdi.POID equals p.ID
                            where !db.SubcontComponentCapabilities.Where(scc => scc.VendorCode == pdi.PO.VendorCode && scc.Material == pdi.Material).Any()
                            && (pdi.IsClosed == "X" || pdi.IsClosed == "L" || pdi.IsClosed == "LX")
                            //group pdi by new { pdi.PO.VendorCode, pdi.Material, pdi.Description, pdi.PB, pdi.Fullweld, pdi.Setting, pdi.Primer } into pdi
                            select pdi).GroupBy(pdi => new { pdi.PO.VendorCode, pdi.Material, pdi.Description, pdi.PB, pdi.Fullweld, pdi.Setting, pdi.Primer })
                            .Select(g => g.FirstOrDefault()); //produces flat sequence;
                                                              //view index Subcont

                //end
                ViewBag.CurrentSearchFilterBy = searchBy;
                ViewBag.CurrentSearchString = search;


                if (searchBy == "description")
                {
                    //return View(db.PurchasingDocumentItems.Where(x => x.Description.Contains(search) && !db.SubcontComponentCapabilities.Where(scc => scc.VendorCode == x.PO.VendorCode && scc.Material == x.Material).Any() || search == null )
                    //    .OrderBy(x => x.PO.VendorCode.Length).ThenBy(x => x.PO.VendorCode).Distinct().ToList().ToPagedList(page ?? 1, 10));
                    return View(pOes.Where(x => x.Description.Contains(search) || search == null)//.GroupBy(x =>new { x.PO.VendorCode, x.Material, x.Description, x.PB, x.Fullweld, x.Setting, x.Primer })
                                                                                                 //.Select(x => new { x.PO.VendorCode, x.Material, x.Description, x.PB, x.Fullweld, x.Setting, x.Primer })
                        .OrderBy(x => x.PO.VendorCode).ThenByDescending(x => x.Material).Select(x => x).Distinct()
                        .ToList().ToPagedList(page ?? 1, 10));
                }
                else if (searchBy == "material")
                {
                    //return View(db.PurchasingDocumentItems.Where(x => x.Material.Contains(search) && !db.SubcontComponentCapabilities.Where(scc => scc.VendorCode == x.PO.VendorCode && scc.Material == x.Material).Any() || search == null)
                    //    .OrderBy(x => x.PO.VendorCode.Length).ThenBy(x => x.PO.VendorCode).Distinct().ToList().ToPagedList(page ?? 1, 10));
                    return View(pOes.Where(x => x.Material.Contains(search) || search == null)//.GroupBy(x => new { x.PO.VendorCode, x.Material, x.Description, x.PB, x.Fullweld, x.Setting, x.Primer })
                        .OrderBy(x => x.PO.VendorCode).ThenByDescending(x => x.Material).Select(x => x).Distinct()
                        .ToList().ToPagedList(page ?? 1, 10));
                }
                else
                {
                    //return View(db.PurchasingDocumentItems.Where(x => x.PO.VendorCode.Contains(search) && !db.SubcontComponentCapabilities.Where(scc => scc.VendorCode == x.PO.VendorCode && scc.Material == x.Material).Any() || search == null)
                    //    .OrderBy(x => x.PO.VendorCode.Length).ThenBy(x => x.PO.VendorCode).Distinct().ToList().ToPagedList(page ?? 1, 10));
                    return View(pOes.Where(x => x.PO.VendorCode.Contains(search) || search == null)//.GroupBy(x => new { x.PO.VendorCode, x.Material, x.Description, x.PB, x.Fullweld, x.Setting, x.Primer })
                        .OrderBy(x => x.PO.VendorCode).ThenByDescending(x => x.Material).Select(x => x).Distinct()
                        .ToList().ToPagedList(page ?? 1, 10));
                }
            }
            catch (Exception ex)
            {
                return View(ex.Message + "-----" + ex.StackTrace);
            }
        }

        // GET: MasterVendor/Details/5
        public ActionResult Details(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.SubcontComponentCapabilities.Where(x => x.ID == id).FirstOrDefault());
            }

        }
        //Fahmi
        public ActionResult DetailsCustom(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.PurchasingDocumentItems.Include(x => x.PO).SingleOrDefault(x => x.ID == id));
            }

        }
        //Fahmi 
        public void DownloadExcel(string searchBy, string search)
        {
            POTrackingEntities db = new POTrackingEntities();
            var listPDI = new List<PurchasingDocumentItem>();
            if (searchBy == "description")
            {
                listPDI = db.PurchasingDocumentItems.Where(x => x.Description.Contains(search) || search == null).OrderBy(x => x.PO.VendorCode.Length).ThenBy(x => x.PO.VendorCode).ToList();
            }
            else if (searchBy == "material")
            {
                listPDI = db.PurchasingDocumentItems.Where(x => x.Material.Contains(search) || search == null).OrderBy(x => x.PO.VendorCode.Length).ThenBy(x => x.PO.VendorCode).ToList();
            }
            else
            {
                listPDI = db.PurchasingDocumentItems.Where(x => x.PO.VendorCode.Contains(search) || search == null).OrderBy(x => x.PO.VendorCode.Length).ThenBy(x => x.PO.VendorCode).ToList();
            }

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "ID";
            Sheet.Cells["B1"].Value = "PO Number";
            Sheet.Cells["C1"].Value = "Tanggal PO";
            Sheet.Cells["D1"].Value = "Vendor Code";
            Sheet.Cells["E1"].Value = "Material";
            Sheet.Cells["F1"].Value = "Description";
            Sheet.Cells["G1"].Value = "PB";
            Sheet.Cells["H1"].Value = "Setting";
            Sheet.Cells["I1"].Value = "Fullweld";
            Sheet.Cells["J1"].Value = "Primer";
            #region settingCell

            //The numbers represent a range: (FromRow, FromCol, ToRow, ToCol)
            using (var range = Sheet.Cells[1, 1, 1, 6])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
            }

            using (var range = Sheet.Cells[1, 7, 1, 10])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }
            Sheet.Cells["A1"].Style.Font.Bold = true;
            Sheet.Cells["B1"].Style.Font.Bold = true;
            Sheet.Cells["C1"].Style.Font.Bold = true;
            Sheet.Cells["D1"].Style.Font.Bold = true;
            Sheet.Cells["E1"].Style.Font.Bold = true;
            Sheet.Cells["F1"].Style.Font.Bold = true;
            Sheet.Cells["G1"].Style.Font.Bold = true;
            Sheet.Cells["H1"].Style.Font.Bold = true;
            Sheet.Cells["I1"].Style.Font.Bold = true;
            Sheet.Cells["J1"].Style.Font.Bold = true;

            #endregion
            int row = 2;
            foreach (var item in listPDI)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.ID;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.PO.Number;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.PO.Date.ToString("dd/MM/yyyy");
                Sheet.Cells[string.Format("D{0}", row)].Value = item.PO.VendorCode;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.Material;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Description;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.PB;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.Setting;
                Sheet.Cells[string.Format("I{0}", row)].Value = item.Fullweld;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.Primer;
                row++;
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        //Fahmi 
        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase FileUpload)
        {
            POTrackingEntities db = new POTrackingEntities();
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {

                    string filename = FileUpload.FileName;
                    string targetpath = Server.MapPath("~/Files/Import/");
                    FileUpload.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }

                    var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                    var ds = new DataSet();

                    adapter.Fill(ds, "ExcelTable");

                    DataTable dtable = ds.Tables["ExcelTable"];

                    string sheetName = "Report";

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var excelPDI = from a in excelFile.Worksheet<PurchasingDocumentItem>(sheetName) select a;

                    foreach (var a in excelPDI)
                    {
                        try
                        {
                            if (a.PB.HasValue && a.Setting.HasValue && a.Primer.HasValue && a.Fullweld.HasValue)
                            {
                                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == a.ID).FirstOrDefault();
                                SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material && x.VendorCode == purchasingDocumentItem.PO.VendorCode).FirstOrDefault();
                                purchasingDocumentItem.PB = a.PB;
                                purchasingDocumentItem.Setting = a.Setting;
                                purchasingDocumentItem.Fullweld = a.Fullweld;
                                purchasingDocumentItem.Primer = a.Primer;
                                db.SaveChanges();



                            }
                            else
                            {
                                data.Add("<ul>");
                                if (!a.PB.HasValue) data.Add("<li> PB is required</li>");
                                if (!a.Setting.HasValue) data.Add("<li> Setting is required</li>");
                                if (!a.Primer.HasValue) data.Add("<li>Primer is required</li>");
                                if (!a.Fullweld.HasValue) data.Add("<li>Fullweld is required</li>");

                                data.Add("</ul>");
                                data.ToArray();
                                ViewBag.Message = Json(data, JsonRequestBehavior.AllowGet);

                            }
                        }

                        catch (DbEntityValidationException ex)
                        {
                            foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            {

                                foreach (var validationError in entityValidationErrors.ValidationErrors)
                                {

                                    Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);

                                }

                            }
                        }
                    }
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    ViewBag.Message = Json("success", JsonRequestBehavior.AllowGet);
                    return RedirectToAction("IndexCustom");
                }
                else
                {
                    //alert message for invalid file format  
                    data.Add("<ul>");
                    data.Add("<li>Only Excel file format is allowed</li>");
                    data.Add("</ul>");
                    data.ToArray();
                    ViewBag.Message = Json(data, JsonRequestBehavior.AllowGet);
                    return RedirectToAction("IndexCustom");
                }
            }
            else
            {
                data.Add("<ul>");
                if (FileUpload == null) data.Add("<li>Please choose Excel file</li>");
                data.Add("</ul>");
                data.ToArray();
                ViewBag.Message = Json(data, JsonRequestBehavior.AllowGet);
                return RedirectToAction("IndexCustom");
            }
        }
        //Fahmi 
        [HttpPost]
        public ActionResult InsertViaExcel(HttpPostedFileBase FileUpload)
        {
            POTrackingEntities db = new POTrackingEntities();
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {

                    string filename = FileUpload.FileName;
                    string targetpath = Server.MapPath("~/Files/Import/");
                    FileUpload.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }

                    var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                    var ds = new DataSet();

                    adapter.Fill(ds, "ExcelTable");

                    DataTable dtable = ds.Tables["ExcelTable"];

                    string sheetName = "Sheet1";

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var excelPDI = from a in excelFile.Worksheet<SubcontComponentCapability>(sheetName) select a;
                    string userName = User.Identity.Name;
                    DateTime now = DateTime.Now;
                    foreach (var a in excelPDI)
                    {
                        try
                        {
                            if (a.VendorCode != null && a.Material != null)
                            {
                                SubcontComponentCapability cekRedundan = db.SubcontComponentCapabilities.Where(x => x.Material == a.Material && x.VendorCode == a.VendorCode).FirstOrDefault();
                                if (cekRedundan == null)
                                {
                                    Vendor cekVendor = db.Vendors.Where(x => x.Code == a.VendorCode).FirstOrDefault();
                                    if (cekVendor != null)
                                    {
                                        SubcontComponentCapability subcontComponentCapability = new SubcontComponentCapability
                                        {
                                            VendorCode = a.VendorCode,
                                            Material = a.Material,
                                            Description = a.Description,
                                            DailyLeadTime = a.DailyLeadTime,
                                            MonthlyLeadTime = a.MonthlyLeadTime,
                                            PB = a.PB,
                                            Setting = a.Setting,
                                            Fullweld = a.Fullweld,
                                            Primer = a.Primer,
                                            MonthlyCapacity = a.MonthlyCapacity,
                                            DailyCapacity = a.DailyCapacity,
                                            CreatedBy = userName,
                                            Created = now,
                                            LastModified = now,
                                            LastModifiedBy = userName

                                        };
                                        ViewBag.Message = "Data Berhasil di Tambahkan";
                                        db.SubcontComponentCapabilities.Add(subcontComponentCapability);
                                        db.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                data.Add("<ul>");
                                if (a.VendorCode == null) data.Add("<li> VendorCode is required</li>");
                                if (a.Material == null) data.Add("<li> Material is required</li>");

                                data.Add("</ul>");
                                data.ToArray();
                                ViewBag.Message = Json(data, JsonRequestBehavior.AllowGet);

                            }
                        }

                        catch (DbEntityValidationException ex)
                        {
                            foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            {

                                foreach (var validationError in entityValidationErrors.ValidationErrors)
                                {

                                    Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);

                                }

                            }
                        }
                    }
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    //alert message for invalid file format  
                    data.Add("<ul>");
                    data.Add("<li>Only Excel file format is allowed</li>");
                    data.Add("</ul>");
                    data.ToArray();
                    ViewBag.Message = Json(data, JsonRequestBehavior.AllowGet);
                    return RedirectToAction("Index");
                }
            }
            else
            {
                data.Add("<ul>");
                if (FileUpload == null) data.Add("<li>Please choose Excel file</li>");
                data.Add("</ul>");
                data.ToArray();
                ViewBag.Message = Json(data, JsonRequestBehavior.AllowGet);
                return RedirectToAction("Index");
            }
        }
        // GET: MasterVendor/Create
        [HttpGet]
        public ActionResult Create()
        {
            POTrackingEntities db = new POTrackingEntities();
            //List<Vendor> Vendors = db.Vendors.Where(x => x.Code.Length == 5).OrderBy(x => x.Name).ToList();
            //IEnumerable<object> en = Vendors;
            var ViewModel = new MasterVendorViewModel
            {
                ListName = new SelectList(db.Vendors.Where(x => x.Code.Length == 5).OrderBy(x => x.Name), "Code", "Name")
            };

            return View(ViewModel);
            //return View();
        }

        // POST: MasterVendor/Create
        [HttpPost]
        public ActionResult Create(MasterVendorViewModel masterVendorViewModel)
        {
            try
            {
                //POTrackingEntities db1 = new POTrackingEntities();
                POTrackingEntities db = new POTrackingEntities();
                //{
                //masterVendorViewModel.Vendors = db.Vendors.Where(x => x.Code.Length == 5).OrderBy(x => x.Name);
                var ViewModel = new MasterVendorViewModel
                {
                    ListName = new SelectList(db.Vendors.Where(x => x.Code.Length == 5).OrderBy(x => x.Name), "Code", "Name")
                };

                if (!ModelState.IsValid)
                {
                    return View(ViewModel);
                }
                else
                {
                    string userName = User.Identity.Name;
                    DateTime now = DateTime.Now;
                    string VendorCode = masterVendorViewModel.SelectedName;
                    decimal dailyCapacity = masterVendorViewModel.subCont.MonthlyCapacity / 22;
                    // TODO: Add insert logic here
                    //using (POTrackingEntities db = new POTrackingEntities())
                    //{
                    int subcontComponentCapabilityCount = db.SubcontComponentCapabilities.Where(x => x.VendorCode.ToLower() == VendorCode && x.Material.Trim().ToLower() == masterVendorViewModel.subCont.Material.Trim().ToLower()).Count();
                    if (subcontComponentCapabilityCount < 1)
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

                        ViewBag.Message = "Data Berhasil di Tambahkan";
                        db.SubcontComponentCapabilities.Add(subcontComponentCapability);
                        db.SaveChanges();
                        //return View();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Message = "Data Material Sudah Ada";
                        return View(ViewModel);
                        //return View();
                    }
                    //}
                }
                //}
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View();
                //return RedirectToAction("Index");
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
        //Fahmi
        public ActionResult EditCustom(int id)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.Where(x => x.ID == id).FirstOrDefault();
                //SubcontComponentCapability subcontComponentCapability = db.SubcontComponentCapabilities.Where(x => x.Material == purchasingDocumentItem.Material && x.VendorCode == purchasingDocumentItem.PO.VendorCode).FirstOrDefault();
                //var totalSubCount = subcontComponentCapability.PB + subcontComponentCapability.Setting + subcontComponentCapability.Fullweld + subcontComponentCapability.Primer;
                @ViewBag.Daily = 0;
                @ViewBag.Monthly = 0;
                @ViewBag.Capacity = 0;
                return View(db.PurchasingDocumentItems.Include(x => x.PO).SingleOrDefault(x => x.ID == id));
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
                    int subcontComponentCapabilityCount = db.SubcontComponentCapabilities.Where(x => x.ID != id && x.VendorCode.ToLower() == selectedSubContComponent.VendorCode && x.Material.Trim().ToLower() == subcontComponent.Material.Trim().ToLower()).Count();
                    if (subcontComponentCapabilityCount > 0)
                    {
                        var ViewModel = new MasterVendorViewModel
                        {
                            ListName = new SelectList(db.Vendors.Where(x => x.Code.Length == 5).OrderBy(x => x.Name), "Code", "Name")
                        };

                        ViewBag.Message = "Data Material Sudah Ada";
                        return View(db.SubcontComponentCapabilities.Where(x => x.ID == id).FirstOrDefault());
                    }
                    else
                    {
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
                        ViewBag.Message = "Data Berhasil di Update";
                        db.SaveChanges();
                        return RedirectToAction("Index");
                        //return View(db.SubcontComponentCapabilities.Where(x => x.ID == id).FirstOrDefault());
                    }
                }
            }
            catch
            {
                return View();
            }
        }
        //Fahmi
        [HttpPost]
        public ActionResult EditCustom(int id, PurchasingDocumentItem pdi)
        {
            DateTime now = DateTime.Now;
            var userName = User.Identity.Name;
            try
            {
                // TODO: Add update logic here
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    PurchasingDocumentItem purchasingDocumentItem = db.PurchasingDocumentItems.SingleOrDefault(x => x.ID == id);
                    string sVendorCode = purchasingDocumentItem.PO.VendorCode;
                    string sMaterial = purchasingDocumentItem.Material;
                    string sDescription = purchasingDocumentItem.Description;
                    int iPB = pdi.PB.Value;
                    int iSetting = pdi.Setting.Value;
                    int iFullweld = pdi.Fullweld.Value;
                    int iPrimer = pdi.Primer.Value;

                    SubcontComponentCapability subCon = new SubcontComponentCapability
                    {
                        VendorCode = sVendorCode,
                        Material = sMaterial,
                        Description = sDescription,
                        DailyLeadTime = 0,
                        MonthlyLeadTime = 0,
                        PB = iPB,
                        Setting = iSetting,
                        Fullweld = iFullweld,
                        Primer = iPrimer,
                        MonthlyCapacity = 0,
                        DailyCapacity = 0,
                        CreatedBy = userName,
                        Created = now,
                        LastModified = now,
                        LastModifiedBy = userName
                    };
                    db.SubcontComponentCapabilities.Add(subCon);
                    db.SaveChanges();

                    ViewBag.Message = "Data Berhasil di Update";
                    //db.SaveChanges();
                }
                return RedirectToAction("IndexCustom");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return RedirectToAction("Index");
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

        [HttpGet]
        public ActionResult getById(string code)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                var test = (from x in db.Vendors
                            where x.Code == code
                            select x).FirstOrDefault();

                Vendor vendor = db.Vendors.Where(x => x.Code == code).FirstOrDefault();


                return Json(new { vendorCode = vendor.Code }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CreateUserVendor()
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                listRoleType = db.RolesTypes.ToList();
                listVendor = db.Vendors.Where(x => x.Code.Length == 5).ToList();
                ViewBag.RolesTypeID = new SelectList(listRoleType, "ID", "Name");
                ViewBag.VendorCode = new SelectList(listVendor, "Code", "CodeName");

                return View();
            }
        }

        [HttpPost]
        public ActionResult CreateUserVendor(UserVendor objNewUser)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                listRoleType = db.RolesTypes.ToList();
                listVendor = db.Vendors.Where(x => x.Code.Length == 5).ToList();
                ViewBag.RolesTypeID = new SelectList(listRoleType, "ID", "Name");
                ViewBag.VendorCode = new SelectList(listVendor, "Code", "CodeName");
                var chkUser = (db.UserVendors.FirstOrDefault(x => x.Username == objNewUser.Username));
                if (chkUser == null)
                {
                    var chkUserRole = (db.UserRoleTypes.FirstOrDefault(x => x.Username == objNewUser.Username));
                    if (chkUserRole == null)
                    {
                        var keyNew = Helper.GeneratePassword(10);
                        var password = Helper.EncodePassword(LoginConstants.defaultPassword, keyNew);
                        objNewUser.ID = Guid.NewGuid();
                        objNewUser.Name = objNewUser.Name;
                        objNewUser.Username = objNewUser.Username;
                        objNewUser.Email = objNewUser.Email;
                        objNewUser.RoleID = 3;
                        objNewUser.VendorCode = objNewUser.VendorCode;
                        objNewUser.IsActive = objNewUser.IsActive;
                        objNewUser.Salt = keyNew;
                        objNewUser.Hash = password;
                        objNewUser.Created = DateTime.Now;
                        objNewUser.CreatedBy = User.Identity.Name;
                        objNewUser.LastModified = DateTime.Now;
                        objNewUser.LastModifiedBy = User.Identity.Name;
                        db.UserVendors.Add(objNewUser);

                        UserRoleType objNewUserRole = new UserRoleType();
                        objNewUserRole.Username = objNewUser.Username;
                        objNewUserRole.RolesTypeID = objNewUser.RolesTypeID;
                        objNewUserRole.Created = DateTime.Now;
                        objNewUserRole.CreatedBy = User.Identity.Name;
                        objNewUserRole.LastModified = DateTime.Now;
                        objNewUserRole.LastModifiedBy = User.Identity.Name;
                        db.UserRoleTypes.Add(objNewUserRole);

                        db.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("ViewUserVendor", "MasterVendor");
                    }
                    ViewBag.ErrorMessage = "User Already Exist!";
                    return View();
                }
                ViewBag.ErrorMessage = "User Already Exist!";
                return View();
            }

        }

        [HttpGet]
        public ActionResult CreateVendorSubcontDev()
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                listVendor = db.Vendors.Where(x => x.Code.Length == 5).ToList();
                ViewBag.VendorCode = new SelectList(listVendor, "Code", "CodeName");
                return View();
            }
        }

        [HttpPost]
        public ActionResult CreateVendorSubcontDev(string username, List<string> vendorList)
        {
            try
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    listRoleType = db.RolesTypes.ToList();
                    listVendor = db.Vendors.Where(x => x.Code.Length == 5).ToList();
                    ViewBag.RolesTypeID = new SelectList(listRoleType, "ID", "Name");
                    ViewBag.VendorCode = new SelectList(listVendor, "Code", "CodeName");
                    List<SubcontDevVendor> subcontDevExist = db.SubcontDevVendors.Where(x => x.Username.ToLower() == username.ToLower()).ToList();

                    if (subcontDevExist.Count == 0)
                    {
                        foreach (var vendorCode in vendorList)
                        {
                            SubcontDevVendor subcontDev = new SubcontDevVendor();
                            subcontDev.Username = username;
                            subcontDev.VendorCode = vendorCode;
                            subcontDev.Created = DateTime.Now;
                            subcontDev.CreatedBy = User.Identity.Name;
                            subcontDev.LastModified = DateTime.Now;
                            subcontDev.LastModifiedBy = User.Identity.Name;
                            db.SubcontDevVendors.Add(subcontDev);
                            //ModelState.Clear();
                        }
                        db.SaveChanges();
                        return Json(new { success = true, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Username Already Exist!" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult EditVendorSubcontDev(int ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                var subcontDevVendor = db.SubcontDevVendors.Where(x => x.ID == ID).FirstOrDefault();
                var listVendor = db.Vendors.Where(x => x.Code.Length == 5).ToList();
                ViewBag.VendorCode = new SelectList(listVendor, "Code", "CodeName");
                return View(subcontDevVendor);
            }
        }

        [HttpGet]
        public ActionResult getSelectedVendorList(string username)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                try
                {
                    var subcontDevVendor = db.SubcontDevVendors.Where(x => x.Username == username).Select(x => x.VendorCode).ToList();

                    return Json(new { success = true, responseText = "OK", arrayDataVendors = subcontDevVendor }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult EditVendorSubcontDev(string username, List<string> vendorList)
        {
            try
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    listVendor = db.Vendors.Where(x => x.Code.Length == 5).ToList();
                    ViewBag.VendorCode = new SelectList(listVendor, "Code", "CodeName");
                    List<SubcontDevVendor> subcontDevExist = db.SubcontDevVendors.Where(x => x.Username == username).ToList();

                    if (subcontDevExist != null)
                    {
                        foreach (var subcontDev in subcontDevExist)
                        {
                            db.SubcontDevVendors.Remove(subcontDev);
                        }
                    }

                    foreach (var vendorCode in vendorList)
                    {
                        SubcontDevVendor subcontDev = new SubcontDevVendor();
                        subcontDev.Username = username;
                        subcontDev.VendorCode = vendorCode;
                        subcontDev.Created = DateTime.Now;
                        subcontDev.CreatedBy = User.Identity.Name;
                        subcontDev.LastModified = DateTime.Now;
                        subcontDev.LastModifiedBy = User.Identity.Name;
                        db.SubcontDevVendors.Add(subcontDev);
                        //ModelState.Clear();
                    }
                    db.SaveChanges();
                    return Json(new { success = true, responseText = "data updated" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewUserVendor(string search, int? page)
        {
            ViewBag.CurrentSearchString = search;
            using (POTrackingEntities db = new POTrackingEntities())
            {

                return View(db.UserVendors.Where(x => x.Username.Contains(search) || x.Name.Contains(search) || search == null).OrderBy(x => x.Name).ToList().ToPagedList(page ?? 1, 10));

            }
        }
        public ActionResult ViewVendorSubcontDev(string search, int? page)
        {
            ViewBag.CurrentSearchString = search;
            using (POTrackingEntities db = new POTrackingEntities())
            {
                return View(db.SubcontDevVendors.Where(x => x.Username.Contains(search) || x.VendorCode.Contains(search) || search == null).GroupBy(x => x.Username).Select(x => x.FirstOrDefault()).OrderBy(x => x.Username).ToList().ToPagedList(page ?? 1, 10));
            }
        }

        [HttpGet]
        public JsonResult GetUserFromValue(string value)
        {
            UserManagementEntities dbUserManagement = new UserManagementEntities();
            try
            {
                object data = null;
                value = value.ToLower();

                data = dbUserManagement.Users.Where(x => x.Username.Contains(value)).Select(x =>
                    new
                    {
                        Data = x.Username,
                        MatchEvaluation = x.Username.ToLower().IndexOf(value)
                    }).Distinct().OrderBy(x => x.MatchEvaluation).Take(10);

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

        [HttpGet]
        public ActionResult EditUserVendor(Guid ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                listRoleType = db.RolesTypes.ToList();
                listVendor = db.Vendors.Where(x => x.Code.Length == 5).ToList();
                var selectedUserVendor = db.UserVendors.Find(ID);
                var selectedUserRole = db.UserRoleTypes.Where(x => x.Username == selectedUserVendor.Username).FirstOrDefault();

                ViewBag.RolesTypeID = new SelectList(listRoleType, "ID", "Name", selectedUserRole.RolesTypeID);
                ViewBag.VendorCode = new SelectList(listVendor, "Code", "CodeName", selectedUserVendor.VendorCode);
                return View(selectedUserVendor);
            }
        }

        [HttpPost]
        public ActionResult EditUserVendor(Guid ID, UserVendor objEditUser)
        {
            try
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    listRoleType = db.RolesTypes.ToList();
                    listVendor = db.Vendors.Where(x => x.Code.Length == 5).ToList();
                    var selectedUserVendor = db.UserVendors.Find(ID);
                    var selectedUserRole = db.UserRoleTypes.Where(x => x.Username == selectedUserVendor.Username).FirstOrDefault();

                    ViewBag.RolesTypeID = new SelectList(listRoleType, "ID", "Name", selectedUserRole.RolesTypeID);
                    ViewBag.VendorCode = new SelectList(listVendor, "Code", "CodeName", selectedUserVendor.VendorCode);

                    var chkUser = (db.UserVendors.FirstOrDefault(x => x.Username == objEditUser.Username && x.ID != ID));
                    if (chkUser == null)
                    {
                        var userRole = db.UserRoleTypes.FirstOrDefault(x => x.Username == selectedUserVendor.Username);
                        userRole.Username = objEditUser.Username;
                        userRole.RolesTypeID = objEditUser.RolesTypeID;
                        userRole.LastModified = DateTime.Now;
                        userRole.LastModifiedBy = User.Identity.Name;

                        selectedUserVendor.Name = objEditUser.Name;
                        selectedUserVendor.Username = objEditUser.Username;
                        selectedUserVendor.Email = objEditUser.Email;
                        selectedUserVendor.RolesTypeID = objEditUser.RolesTypeID;
                        selectedUserVendor.VendorCode = objEditUser.VendorCode;
                        selectedUserVendor.IsActive = objEditUser.IsActive;
                        selectedUserVendor.LastModified = DateTime.Now;
                        selectedUserVendor.LastModifiedBy = User.Identity.Name;

                        db.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("ViewUserVendor", "MasterVendor");
                    }
                    ViewBag.ErrorMessage = "User Already Exist!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }


        public ActionResult DeleteUserVendor(Guid ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                var userVendor = db.UserVendors.Find(ID);
                var userRole = db.UserRoleTypes.Where(x => x.Username == userVendor.Username).FirstOrDefault();
                db.UserRoleTypes.Remove(userRole);
                db.UserVendors.Remove(userVendor);
                db.SaveChanges();

                return RedirectToAction("ViewUserVendor");
            }
        }
        public ActionResult DeleteVendorSubcontDev(int ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                var subcontDev = db.SubcontDevVendors.Find(ID);
                List<SubcontDevVendor> subcontDevExist = db.SubcontDevVendors.Where(x => x.Username == subcontDev.Username).ToList();

                if (subcontDevExist != null)
                {
                    foreach (var item in subcontDevExist)
                    {
                        db.SubcontDevVendors.Remove(item);
                    }
                }
                db.SaveChanges();

                return RedirectToAction("ViewVendorSubcontDev");
            }
        }

        public ActionResult ResetPasswordUserVendor(Guid ID)
        {
            using (POTrackingEntities db = new POTrackingEntities())
            {
                var selectedUserVendor = db.UserVendors.Find(ID);
                var keyNew = Helper.GeneratePassword(10);
                var password = Helper.EncodePassword(LoginConstants.defaultPassword, keyNew);
                selectedUserVendor.Salt = keyNew;
                selectedUserVendor.Hash = password;
                db.SaveChanges();
                ModelState.Clear();
                return RedirectToAction("ViewUserVendor");
            }
        }


        [HttpPost]
        public ActionResult ResetPasswordUserVendor(PasswordView objPassword)
        {
            return View();

        }


        [HttpGet]
        public JsonResult GetDataFromMaterialAndDescription(string value)
        {
            POTrackingEntities db = new POTrackingEntities();
            try
            {
                object data = null;
                value = value.ToLower();

                data = db.PurchasingDocumentItems.Where(x => x.Material.Contains(value) || x.Description.Contains(value)).Select(x =>
                new
                {
                    Data = x.Material.ToLower().StartsWith(value) ? x.Material : x.Description.ToLower().StartsWith(value) ? x.Description : x.Material.ToLower().Contains(value) ? x.Material : x.Description,
                    MatchEvaluation = (x.Material.ToLower().StartsWith(value) ? 1 : 0) + (x.Description.ToLower().StartsWith(value) ? 1 : 0)
                }).Distinct().OrderByDescending(x => x.MatchEvaluation).Take(10);

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
    }

    public static class Helper
    {
        public static string GeneratePassword(int length) //length of salt    
        {
            const string allowedChars = LoginConstants.allowChars;
            var randNum = new Random();
            var chars = new char[length];
            var allowedCharCount = allowedChars.Length;
            for (var i = 0; i <= length - 1; i++)
            {
                chars[i] = allowedChars[Convert.ToInt32((allowedChars.Length - 1) * randNum.NextDouble())];
            }
            return new string(chars);
        }
        public static string EncodePassword(string pass, string salt) //encrypt password    
        {
            byte[] bytes = Encoding.Unicode.GetBytes(pass);
            byte[] src = Encoding.Unicode.GetBytes(salt);
            byte[] dst = new byte[src.Length + bytes.Length];
            System.Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            System.Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
            HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
            byte[] inArray = algorithm.ComputeHash(dst);
            return EncodePasswordMd5(Convert.ToBase64String(inArray));
        }
        public static string EncodePasswordMd5(string pass) //Encrypt using MD5    
        {
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;
            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)    
            md5 = new MD5CryptoServiceProvider();
            originalBytes = ASCIIEncoding.Default.GetBytes(pass);
            encodedBytes = md5.ComputeHash(originalBytes);
            //Convert encoded bytes back to a 'readable' string    
            return BitConverter.ToString(encodedBytes);
        }
        public static string base64Encode(string sData) // Encode    
        {
            try
            {
                byte[] encData_byte = new byte[sData.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(sData);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }
        public static string base64Decode(string sData) //Decode    
        {
            try
            {
                var encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();
                byte[] todecodeByte = Convert.FromBase64String(sData);
                int charCount = utf8Decode.GetCharCount(todecodeByte, 0, todecodeByte.Length);
                char[] decodedChar = new char[charCount];
                utf8Decode.GetChars(todecodeByte, 0, todecodeByte.Length, decodedChar, 0);
                string result = new String(decodedChar);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Decode" + ex.Message);
            }
        }
    }
}
