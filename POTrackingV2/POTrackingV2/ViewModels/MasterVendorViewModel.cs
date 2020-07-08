using System;
using System.Collections.Generic;
using POTrackingV2.Models;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace POTrackingV2.ViewModels
{
    public class MasterVendorViewModel
    {
        [Required]
        public string SelectedName { get; set; }

        public SelectList ListName { get; set; }
        //public IEnumerable<Vendor> Vendors { get; set; }

        public SubcontComponentCapability subCont { get; set; }

    }
}