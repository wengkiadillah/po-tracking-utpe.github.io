using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using POTrackingV2.Models;

namespace POTrackingV2.Models
{
    [MetadataType(typeof(UserVendorAnnotaiton))]
    public partial class UserVendor
    {

        public string RoleName
        {
            get
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    return db.Roles.SingleOrDefault(x => x.ID == this.RoleID).Name;
                }
            }
        }
        public string RoleTypeName
        {
            get
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    return db.RolesTypes.SingleOrDefault(x => x.ID == this.RolesTypeID).Name;
                }
            }
        }
        public string VendorName
        {
            get
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    var vendor = db.Vendors.SingleOrDefault(x => x.Code == this.VendorCode);
                    return vendor.Code + " - " + vendor.Name;
                }
            }
        }
    }

    public partial class Vendor
    {
        public string CodeName
        {
            get { return this.Code + " - " + this.Name; }
        }
    }

    public class UserVendorAnnotaiton
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        public Nullable<int> RolesTypeID { get; set; }
        [Required(ErrorMessage = "Vendor is required")]
        public string VendorCode { get; set; }
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }
    }
}