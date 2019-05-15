using System;
using System.Linq;

namespace POTrackingV2.Models
{
    public partial class ETAHistory
    {
        public string ETADateView
        {
            get
            {
                if (this.ETADate.GetValueOrDefault().ToString("dd/MM/yyyy") != "01/01/0001")
                {
                    return this.ETADate.GetValueOrDefault().ToString("dd/MM/yyyy");
                }
                else
                {
                    return "";
                }
            }
        }
    }
}