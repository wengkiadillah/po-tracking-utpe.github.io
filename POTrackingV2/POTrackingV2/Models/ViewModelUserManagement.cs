using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POTrackingV2.Models
{
    public class ViewModelUserManagement
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string OrganizaionGroup { get; set; }
        public int OrganizaionGroupID { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public int RoleID { get; set; }
        public string Email { get; set; }
        public string ProductRefernceName { get; set; }
    }
}