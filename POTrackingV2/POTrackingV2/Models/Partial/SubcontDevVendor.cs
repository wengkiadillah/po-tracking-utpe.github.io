using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using POTrackingV2.Constants;
using POTrackingV2.Models;

namespace POTrackingV2.Models
{
    [MetadataType(typeof(SubcontDevVendorAnnotaiton))]
    public class SubcontDevVendorAnnotaiton
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vendor is required")]
        public string VendorCode { get; set; }
    }
}