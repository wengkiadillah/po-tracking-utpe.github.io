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
    
    public partial class PO
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PO()
        {
            this.PurchasingDocumentItems = new HashSet<PurchasingDocumentItem>();
        }
    
        public int ID { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public Nullable<int> ProgressDay { get; set; }
        public string VendorCode { get; set; }
        public Nullable<int> KompoCategoryID { get; set; }
        public Nullable<System.DateTime> EstimateDeliveryFromSubcont1Date { get; set; }
        public Nullable<int> EstimateDeliveryFromSubcont1Quantity { get; set; }
        public Nullable<System.DateTime> EstimateDeliveryFromSubcont2Date { get; set; }
        public Nullable<int> EstimateDeliveryFromSubcont2Quantity { get; set; }
        public Nullable<System.DateTime> EstimateDeliveryFromSubcont3Date { get; set; }
        public Nullable<int> EstimateDeliveryFromSubcont3Quantity { get; set; }
        public string Information { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public string ProductGroup { get; set; }
        public Nullable<bool> CanHavePI { get; set; }
        public byte[] DocumentPI { get; set; }
        public Nullable<bool> IsApprovedPI { get; set; }
        public Nullable<bool> IsApprovedByVendor { get; set; }
        public Nullable<bool> IsApprovedByUser { get; set; }
        public Nullable<bool> DatePaymentReceived { get; set; }
        public byte[] DocumentInvoice { get; set; }
        public byte[] DocumentPostedInvoice { get; set; }
        public Nullable<System.DateTime> DatePostedInvoice { get; set; }
        public string NumberPostedInvoice { get; set; }
        public string PurchaseOrderCreator { get; set; }
        public string Status { get; set; }
        public string Reference { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime Created { get; set; }
        public string LastModifiedBy { get; set; }
        public System.DateTime LastModified { get; set; }
    
        public virtual KompoCategory KompoCategory { get; set; }
        public virtual Vendor Vendor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchasingDocumentItem> PurchasingDocumentItems { get; set; }
    }
}
