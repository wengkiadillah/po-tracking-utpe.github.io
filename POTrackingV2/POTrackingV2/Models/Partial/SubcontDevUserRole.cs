using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using POTrackingV2.Constants;
using POTrackingV2.Models;

namespace POTrackingV2.Models
{
    [MetadataType(typeof(SubcontDevUserRoleAnnotaiton))]
    public partial class SubcontDevUserRole
    {
        //public Nullable<int> RolesTypeID { get; set; }
        public int RolesTypeID { get; set; }
        public string RoleName
        {
            get
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    return db.SubcontDevRoles.FirstOrDefault(x => x.ID == this.RoleID).Name;
                }
            }
        }
    }

    public class SubcontDevUserRoleAnnotaiton
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public int RolesTypeID { get; set; }
    }
}