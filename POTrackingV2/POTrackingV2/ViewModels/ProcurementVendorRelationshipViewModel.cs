using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POTrackingV2.ViewModels
{
    public class ProcurementVendorRelationshipCreateViewModel
    {
        public ProcurementVendorRelationship procurementVendorRelationship { get; set; }
        public SelectList selectListProcurementUsernames { get; set; }
        public SelectList selectListVendors { get; set; }
    }
}