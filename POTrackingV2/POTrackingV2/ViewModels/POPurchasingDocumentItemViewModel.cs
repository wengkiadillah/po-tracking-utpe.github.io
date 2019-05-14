using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using POTrackingV2.Models;

namespace POTracking.ViewModels
{
    public class POPurchasingDocumentItemViewModel
    {
        public PO PO { get; set; }
        public ICollection<PurchasingDocumentItemPurchasingDocumentItemHistoryViewModel> PurchasingDocumentItemPurchasingDocumentItemHistoryViewModel { get; set; }
    }
}