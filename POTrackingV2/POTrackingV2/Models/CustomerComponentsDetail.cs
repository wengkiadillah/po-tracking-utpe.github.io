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
    
    public partial class CustomerComponentsDetail
    {
        public int ID { get; set; }
        public int IssueHeaderID { get; set; }
        public string SalesOrderNumber { get; set; }
        public string SerialNumber { get; set; }
        public Nullable<System.DateTime> ComponentArrivalDate { get; set; }
        public System.DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public Nullable<System.DateTime> ConfirmationDate { get; set; }
        public string ProductName { get; set; }
        public string PRONumber { get; set; }
        public Nullable<System.DateTime> AssemblyDate { get; set; }
    
        public virtual IssueHeader IssueHeader { get; set; }
    }
}
