using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace POTrackingV2.CustomAuthentication
{
    public class CustomMembershipUser : MembershipUser
    {
        #region User Properties

        /// <summary>
        /// Propertis yang digunakan dalam identitas login User
        /// </summary>
        public string UserName { get; set; }
        public string Name { get; set; }
        //public string Roles { get; set; }

        public string Roles { get; set; }
        //public int RolesType { get; set; }
        //public string VendorCode { get; set; }

        #endregion

        #region UnUsed

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public CustomMembershipUser(UserRole user) : base("CustomMembership", user.Username, user.ID, user.User.Email, string.Empty, string.Empty, true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now)
        {
            UserName = user.Username;
            Name = user.User.Name;
            Roles = user.Role.Name.ToLower();
            //Roles = user.RoleID;
            //RolesType = user.RolesTypeID != null ? (int)user.RolesTypeID : 0;
            //VendorCode = user.VendorCode;
        }

        #endregion

    }
}