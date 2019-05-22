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
    
    public partial class UserGroup
    {
        public int ID { get; set; }
        public bool IsSender { get; set; }
        public bool IsReceiver { get; set; }
        public bool IsCloser { get; set; }
        public int MasterIssueID { get; set; }
        public int OrganizationGroupID { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime Created { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public int RoleID { get; set; }
    
        public virtual MasterIssue MasterIssue { get; set; }
        public virtual OrganizationGroup OrganizationGroup { get; set; }
    }
}
