using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POTrackingV2.Models
{
    public partial class UserRoleType
    {


    }

    public partial class UserProxy
    {
        public string Username { get; set; }

        public string Name { get; set; }

        public string RoleName { get; set; }


        public string UserProp
        {
            get
            {
                return this.Name + " - " + RoleName;
            }
        }
        

        public UserProxy(string username, string name, string rolename)
        {
            this.Username = username;
            this.Name = name;
            this.RoleName = rolename;
        }

    }

    public partial class UserRoleTypeProxy
    {
        public string Username { get; set; }

        public string Name { get; set; }

        public string RoleName { get; set; }

        public int? RoleTypeID
        {
            get
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    var idRoleType = db.UserRoleTypes.FirstOrDefault(x => x.Username == this.Username).RolesTypeID;
                    return idRoleType;
                }
            }
        }

        public string RoleTypeName
        {
            get
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    return db.RolesTypes.FirstOrDefault(x => x.ID == this.RoleTypeID).Name;
                }
            }
        }

        public UserRoleTypeProxy(string username, string name, string rolename)
        {
            this.Username = username;
            this.Name = name;
            this.RoleName = rolename;
        }

    }


}