using System;
using System.Linq;

namespace POTrackingV2.Models
{
    public partial class Shipment
    {
        public string BookingDateView
        {
            get
            {
                DateTime bookingDate = this.BookingDate.GetValueOrDefault();

                if (bookingDate != null)
                {
                    if (bookingDate.ToString("dd/MM/yyyy") != "01/01/0001")
                    {
                        return bookingDate.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        public string ATADateView
        {
            get
            {
                DateTime ataDate = this.ATADate.GetValueOrDefault();

                if (ataDate != null)
                {
                    if (ataDate.ToString("dd/MM/yyyy") != "01/01/0001")
                    {
                        return ataDate.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        public string ETADateView
        {
            get
            {
                DateTime etaDate = this.ETADate.GetValueOrDefault();

                if (etaDate != null)
                {
                    if (etaDate.ToString("dd/MM/yyyy") != "01/01/0001")
                    {
                        return etaDate.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        public string ATDDateView
        {
            get
            {
                   DateTime atdDate = this.ATDDate.GetValueOrDefault();

                if (atdDate != null)
                {
                    if (atdDate.ToString("dd/MM/yyyy") != "01/01/0001")
                    {
                        return atdDate.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        public string CopyBLDateView
        {
            get
            {
                   DateTime copyBLDate = this.CopyBLDate.GetValueOrDefault();

                if (copyBLDate != null)
                {
                    if (copyBLDate.ToString("dd/MM/yyyy") != "01/01/0001")
                    {
                        return copyBLDate.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
        }
    }
}