using PagedList;
using POTrackingV2.Models;
using System.Collections.Generic;
using System.Web;

namespace POTracking.ViewModels
{
    public class IndexSubcontViewModel
    {
        public IPagedList<PO> POes { get; set; }
        public ICollection<PurchasingDocumentItem> InputPurchasingDocumentItem { get; set; }
        public ETAHistory InputETAHistory { get; set; }
        public HttpPostedFileBase FileProformaInvoice { get; set; }
        public string CurrentPONumber { get; set; }
        public string CurrentStartPODate { get; set; }
        public string CurrentEndPODate { get; set; }
    }
}