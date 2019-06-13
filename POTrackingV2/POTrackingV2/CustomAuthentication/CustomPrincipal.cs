using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace POTrackingV2.CustomAuthentication
{
    /// <summary>
    /// Objek-objek prinsip pada fungsional dasar keamanan login
    /// </summary>
    public class CustomPrincipal : IPrincipal
    {
        private POTrackingEntities db = new POTrackingEntities();
        #region Identity Properties
        /// <summary>
        /// Propertis yang digunakan dalam identitas login User
        /// </summary>
        //public int UserId { get; set; }
        //public string Name { get; set; }
        //public string Email { get; set; }
        //public string[] Roles { get; set; }

        public string UserName { get; set; }
        public string Name { get; set; }
        //public string Roles { get; set; }
        public string Roles { get; set; }
        //public int RolesType { get; set; }
        //public string VendorCode { get; set; }


        #endregion

        /// <summary>
        /// Method Identity, definisi dari fungsi dasar identitas objek User
        /// </summary>
        public IIdentity Identity
        {
            get; private set;
        }

        /// <summary>
        /// Method IsInRole, fungsi dasar pengecekan apakah User memiliki Role tertentu
        /// </summary>
        /// <param name="role"></param>
        /// <returns>Return 'True' jika sebuah Username termasuk ke dalam sebuah Role, 'False' jika tidak</returns>
        public bool IsInRole(string role)
        {
            bool val = false;
            string[] roles = role.Split(',');


            if (roles.Count() > 1)
            {
                foreach (var item in roles)
                {
                    if (item.ToLower().Trim().Equals(Roles.ToLower()) && !val)
                    //if (Roles.Any(x=>x.ToLower().Trim() == item.ToLower().Trim()) && !val)
                        val = true;
                }
            }
            else
            {
                if (Roles.ToLower().Equals(role.ToLower()))
                //if (Roles.Any(x => x.ToLower().Trim() == role.ToLower().Trim()))
                    val = true;
            }



            //if (Roles.Any(r => role.Contains(r)))
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

            return val;
        }

        /// <summary>
        /// Inisialisasi identitas baru yang merepresentasikan pengguna tertentu
        /// </summary>
        /// <param name="username"></param>
        public CustomPrincipal(string username)
        {
            Identity = new GenericIdentity(username);
        }
    }
}