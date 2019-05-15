using System;
using System.Linq;

namespace POTrackingV2.Models
{
    public partial class PurchasingDocumentItemHistory
    {
        public string GRDateView
        {
            get
            {
                if (this.GoodsReceiptDate.GetValueOrDefault().ToString("dd/MM/yyyy") != "01/01/0001")
                {
                    return this.GoodsReceiptDate.GetValueOrDefault().ToString("dd/MM/yyyy");
                }
                else
                {
                    return "";
                }
            }
        }
    }
}