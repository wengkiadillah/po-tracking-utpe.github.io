﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POTrackingV2.Constants
{
    public class LoginConstants
    {
        public const int UserTypeInternal = 1;
        public const string RoleAdministrator = "Administrator";
        public const string RoleProcurement = "Procurement";
        public const string RoleVendor = "Vendor";
        public const string RoleSubcontDev = "SubcontDev";
        public const string RoleOthers = "Others";
        public const string allowChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
        //public const string defaultPassword = "password0!";
        public const string defaultPassword = "popatria16";
        
        //page
        public const int PageSize = 10;
    }
}