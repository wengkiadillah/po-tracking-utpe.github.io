using System;
using System.Collections.Generic;
using POTrackingV2.Models;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace POTrackingV2.ViewModels
{
    public class MasterVendorViewModel
    {
        [NotMapped]
        public string SelectedName { get; set; }

        public SelectList ListName { get; set; }

        public SubcontComponentCapability subCont { get; set; }

    }
}