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
    
    public partial class PurchasingDocumentItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PurchasingDocumentItem()
        {
            this.ETAHistories = new HashSet<ETAHistory>();
            this.Notifications = new HashSet<Notification>();
            this.ProductionSequenceDatas = new HashSet<ProductionSequenceData>();
            this.ProgressPhotoes = new HashSet<ProgressPhoto>();
            this.PurchasingDocumentItemHistories = new HashSet<PurchasingDocumentItemHistory>();
            this.QualityControlProgresses = new HashSet<QualityControlProgress>();
            this.RawMaterialOrders = new HashSet<RawMaterialOrder>();
            this.Shipments = new HashSet<Shipment>();
        }
    
        public int ID { get; set; }
        public int POID { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public Nullable<int> ParentID { get; set; }
        public int ItemNumber { get; set; }
        public string Material { get; set; }
        public string Description { get; set; }
        public int NetPrice { get; set; }
        public string Currency { get; set; }
        public int Quantity { get; set; }
        public Nullable<System.DateTime> ConfirmedDate { get; set; }
        public Nullable<int> ConfirmedQuantity { get; set; }
        public Nullable<bool> ConfirmedItem { get; set; }
        public Nullable<System.DateTime> ConfirmReceivedPaymentDate { get; set; }
        public string InvoiceDocument { get; set; }
        public string InvoiceMethod { get; set; }
        public string ProformaInvoiceDocument { get; set; }
        public Nullable<bool> ApproveProformaInvoiceDocument { get; set; }
        public int NetValue { get; set; }
        public Nullable<int> WorkTime { get; set; }
        public Nullable<decimal> LeadTimeItem { get; set; }
        public Nullable<int> OpenQuantity { get; set; }
        public Nullable<System.DateTime> ATA { get; set; }
        public string ActiveStage { get; set; }
        public string IsClosed { get; set; }
        public Nullable<int> PB { get; set; }
        public Nullable<int> Setting { get; set; }
        public Nullable<int> Fullweld { get; set; }
        public Nullable<int> Primer { get; set; }
        public Nullable<System.DateTime> PBActualDate { get; set; }
        public Nullable<int> PBALateReasonID { get; set; }
        public Nullable<System.DateTime> SettingActualDate { get; set; }
        public Nullable<int> SettingLateReasonID { get; set; }
        public Nullable<System.DateTime> FullweldActualDate { get; set; }
        public Nullable<int> FullweldLateReasonID { get; set; }
        public Nullable<System.DateTime> PrimerActualDate { get; set; }
        public Nullable<int> PremierLateReasonID { get; set; }
        public System.DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public Nullable<bool> Shipment_HasInbound { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ETAHistory> ETAHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual PO PO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductionSequenceData> ProductionSequenceDatas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProgressPhoto> ProgressPhotoes { get; set; }
        public virtual SequencesProgressReason SequencesProgressReason { get; set; }
        public virtual SequencesProgressReason SequencesProgressReason1 { get; set; }
        public virtual SequencesProgressReason SequencesProgressReason2 { get; set; }
        public virtual SequencesProgressReason SequencesProgressReason3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchasingDocumentItemHistory> PurchasingDocumentItemHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QualityControlProgress> QualityControlProgresses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RawMaterialOrder> RawMaterialOrders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Shipment> Shipments { get; set; }
    }
}
