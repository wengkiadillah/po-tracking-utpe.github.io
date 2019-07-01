using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POTrackingV2.Models
{
    public partial class PO
    {
        public bool IsTwentyFivePercent
        {
            get
            {
                if (this.ProgressDay.HasValue && this.ReleaseDate.HasValue)
                {
                    int daysAdded = this.ProgressDay.GetValueOrDefault() / 4;
                    DateTime today = DateTime.Now;
                    DateTime twentyFivePercentDate = this.ReleaseDate.GetValueOrDefault().AddDays(daysAdded);

                    if (today >= twentyFivePercentDate)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsSeventyFivePercent
        {
            get
            {
                if (this.ProgressDay.HasValue && this.ReleaseDate.HasValue)
                {
                    int daysAdded = (this.ProgressDay.GetValueOrDefault() * 3) / 4 ;
                    DateTime today = DateTime.Now;
                    DateTime seventyFivePercentDate = this.ReleaseDate.GetValueOrDefault().AddDays(daysAdded);

                    if (today >= seventyFivePercentDate)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}