using PagedList;
using POTrackingV2.Models;
using System.Web;

namespace POTrackingV2.ViewModels
{
    public class IndexLocalViewModel
    {
        public IPagedList<PO> POes { get; set; }
        public PurchasingDocumentItem InputPurchasingDocumentItem { get; set; }
        public ETAHistory InputETAHistory { get; set; }
        public HttpPostedFileBase FileProformaInvoice { get; set; }
        public string CurrentPONumber { get; set; }
        public string CurrentStartPODate { get; set; }
        public string CurrentEndPODate { get; set; }
    }
}