using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace POTrackingV2.Models
{
    public partial class Notification
    {
        POTrackingEntities db = new POTrackingEntities();
        
        public int StageToNumber
        {
            get
            {
                int stage;

                if (!String.IsNullOrEmpty(this.Stage))
                {
                    stage = Convert.ToInt32(Regex.Replace(this.Stage, "[^.0-9]", ""));
                }
                else
                {
                    stage = 0;
                }

                return stage;
            }
        }
    }
}