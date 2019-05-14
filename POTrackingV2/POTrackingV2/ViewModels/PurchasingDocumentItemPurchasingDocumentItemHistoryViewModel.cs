using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POTracking.ViewModels
{
    public class PurchasingDocumentItemPurchasingDocumentItemHistoryViewModel
    {
        public PurchasingDocumentItem PurchasingDocumentItem { get; set; }
        public ICollection<PurchasingDocumentItemHistory> PurchasingDocumentItemHistories { get; set; }
        public DateTime LatestDeliveryDate
        {
            get
            {
                return this.PurchasingDocumentItemHistories.OrderByDescending(x => x.Created).Select(x=>x.DeliveryDate.GetValueOrDefault()).FirstOrDefault();
            }
        }
    }
}