using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POTrackingV2.Models
{
    public partial class PO
    {
        //public bool IsTwentyFivePercent
        //{
        //    get
        //    {
        //        PurchasingDocumentItem purchasingDocumentItem = new PurchasingDocumentItem();

        //        if (purchasingDocumentItem.ConfirmedDate.HasValue && this.ReleaseDate.HasValue)
        //        {
        //            DateTime date1 = purchasingDocumentItem.ConfirmedDate.GetValueOrDefault();
        //            DateTime date2 = this.ReleaseDate.GetValueOrDefault();
        //            TimeSpan t = date1.Subtract(date2);//date1 - date2;
        //            int daysAdded = t.Days / 4;
        //            DateTime today = DateTime.Now;
        //            DateTime twentyFivePercentDate = this.ReleaseDate.GetValueOrDefault().AddDays(daysAdded);

        //            if (today >= twentyFivePercentDate)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //}

        //public string GetTwentyFivePercentdate
        //{
        //    get
        //    {
        //        PurchasingDocumentItem purchasingDocumentItem = new PurchasingDocumentItem();

        //        if (purchasingDocumentItem.ConfirmedDate.HasValue && this.ReleaseDate.HasValue)
        //        {
        //            DateTime date1 = purchasingDocumentItem.ConfirmedDate.GetValueOrDefault();
        //            DateTime date2 = this.ReleaseDate.GetValueOrDefault();
        //            TimeSpan t = date1.Subtract(date2);//date1 - date2;
        //            int daysAdded = t.Days / 4;
        //            //DateTime today = DateTime.Now;
        //            DateTime twentyFivePercentDate = this.ReleaseDate.GetValueOrDefault().AddDays(daysAdded);

        //            return twentyFivePercentDate.ToString("dd/MM/yyyy");
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }

        //}

        //public bool IsSeventyFivePercent
        //{
        //    get
        //    {
        //        PurchasingDocumentItem purchasingDocumentItem = new PurchasingDocumentItem();

        //        if (purchasingDocumentItem.HasETAHistory && this.ReleaseDate.HasValue)
        //        {
        //            //int daysAdded = (this.ProgressDay.GetValueOrDefault() * 3) / 4 ;
        //            //DateTime today = DateTime.Now;
        //            //DateTime seventyFivePercentDate = this.ReleaseDate.GetValueOrDefault().AddDays(daysAdded);
        //            DateTime date1 = purchasingDocumentItem.FirstETAHistory.ETADate.GetValueOrDefault();
        //            DateTime date2 = this.ReleaseDate.GetValueOrDefault();
        //            TimeSpan t = date1.Subtract(date2);//date1 - date2;
        //            int daysAdded = (t.Days *3) / 4;
        //            DateTime today = DateTime.Now;
        //            DateTime seventyFivePercentDate = this.ReleaseDate.GetValueOrDefault().AddDays(daysAdded);

        //            if (today >= seventyFivePercentDate)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //}

        //public string GetSeventyFivePercentDate
        //{
        //    get
        //    {
        //        PurchasingDocumentItem purchasingDocumentItem = new PurchasingDocumentItem();

        //        if (purchasingDocumentItem.HasETAHistory && this.ReleaseDate.HasValue)
        //        {
        //            //int daysAdded = (this.ProgressDay.GetValueOrDefault() * 3) / 4 ;
        //            //DateTime today = DateTime.Now;
        //            //DateTime seventyFivePercentDate = this.ReleaseDate.GetValueOrDefault().AddDays(daysAdded);
        //            DateTime date1 = purchasingDocumentItem.FirstETAHistory.ETADate.GetValueOrDefault();
        //            DateTime date2 = this.ReleaseDate.GetValueOrDefault();
        //            TimeSpan t = date1.Subtract(date2);//date1 - date2;
        //            int daysAdded = (t.Days * 3) / 4;
        //            DateTime today = DateTime.Now;
        //            DateTime seventyFivePercentDate = this.ReleaseDate.GetValueOrDefault().AddDays(daysAdded);

        //            return seventyFivePercentDate.ToString("dd/MM/yyyy");
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }

        //}
    }
}