using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace POTrackingV2.Models
{
    public class PasswordView
    {
        [Required(ErrorMessage = "Old Password is required")]
        [Display(Name = "Old Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Re-Enter New Password is required")]
        [Display(Name = "Re-Enter New Password")]
        public string ReNewPassword { get; set; }
    }
}