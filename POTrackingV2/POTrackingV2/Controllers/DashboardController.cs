using POTrackingV2.Constants;
using POTrackingV2.CustomAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POTrackingV2.Controllers
{
    [CustomAuthorize(Roles = LoginConstants.RoleAdministrator + "," + LoginConstants.RoleVendor + "," + LoginConstants.RoleProcurement + "," + LoginConstants.RoleSubcontDev)]
    public class DashboardController : Controller
    {
        //WebConfigurationManager.AppSettings["ActiveDirectoryUrl"];
        // GET: Dashboard
        public ActionResult Index()
        {
            
            ViewBag.Message = "Your application dashboard page.";
            return View();
        }

        public ActionResult DashboardCommandCenterPOTracking()
        {
            ViewBag.Message = "Your application dashboard page.";

            return View();
        }

        public ActionResult DashboardCommandCenterAlert()
        {
            ViewBag.Message = "Your application dashboard page.";

            return View();
        }

        public ActionResult DashboardSubcontdevProgress()
        {
            ViewBag.Message = "Your application dashboard page.";

            return View();
        }

        public ActionResult DashboardProcurementProgress()
        {
            ViewBag.Message = "Your application dashboard page.";

            return View();
        }

        public ActionResult DashboardReminderComponentInspection()
        {
            ViewBag.Message = "Your application dashboard page.";

            return View();
        }

        public ActionResult DashboardItemArrivalStatus()
        {
            ViewBag.Message = "Your application dashboard page.";

            return View();
        }

        public ActionResult DashboardLeadTimeFromPR_PORelease()
        {
            ViewBag.Message = "Your application dashboard page.";

            return View();
        }
    }
}