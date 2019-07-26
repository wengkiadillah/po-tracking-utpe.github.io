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
    
    public partial class PurchasingDocumentItemHistory
    {
        public int ID { get; set; }
        public int PurchasingDocumentItemID { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public string PayTerms { get; set; }
        public Nullable<System.DateTime> GoodsReceiptDate { get; set; }
        public Nullable<int> GoodsReceiptQuantity { get; set; }
        public Nullable<int> MovementType { get; set; }
        public string POHistoryCategory { get; set; }
        public string DocumentNumber { get; set; }
        public string DeliveryOrder { get; set; }
        public string Shipment_InboundNumber { get; set; }
        public System.DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
    
        public virtual PurchasingDocumentItem PurchasingDocumentItem { get; set; }
    }
}
