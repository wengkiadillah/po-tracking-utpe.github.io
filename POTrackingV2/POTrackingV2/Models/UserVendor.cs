//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace POTrackingV2.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserVendor
    {
        public System.Guid ID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Salt { get; set; }
        public string Hash { get; set; }
        public int RoleID { get; set; }
        public Nullable<int> RolesTypeID { get; set; }
        public string VendorCode { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
    
        public virtual RolesType RolesType { get; set; }
    }
}
