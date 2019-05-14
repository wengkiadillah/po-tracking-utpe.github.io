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
    
    public partial class RawMaterialOrder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RawMaterialOrder()
        {
            this.RawMaterialOrderDetails = new HashSet<RawMaterialOrderDetail>();
            this.RawMaterialOrderPoFiles = new HashSet<RawMaterialOrderPoFile>();
            this.RawMaterialOrderProgressFiles = new HashSet<RawMaterialOrderProgressFile>();
        }
    
        public int ID { get; set; }
        public int PurchasingDocumentsItemID { get; set; }
        public System.DateTime PlanFinishDate { get; set; }
        public System.DateTime ActualFinishDate { get; set; }
        public System.DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
    
        public virtual PurchasingDocumentItem PurchasingDocumentItem { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RawMaterialOrderDetail> RawMaterialOrderDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RawMaterialOrderPoFile> RawMaterialOrderPoFiles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RawMaterialOrderProgressFile> RawMaterialOrderProgressFiles { get; set; }
    }
}
