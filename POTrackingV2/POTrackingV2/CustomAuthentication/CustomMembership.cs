using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using POTrackingV2.Models;
using POTrackingV2.Controllers;
using POTrackingV2.Constants;

namespace POTrackingV2.CustomAuthentication
{
    public class CustomMembership : MembershipProvider
    {


        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            //using (UserManagementEntities db = new UserManagementEntities())
            //{
            //    var user = db.UserRoles.Where(a => a.Username.Equals(username) && a.Role.ApplicationID==3).SingleOrDefault();
            //    return (user != null) ? true : false;
            //}

            using (POTrackingEntities db = new POTrackingEntities())
            {
                var getUser = db.UserVendors.Where(x => x.Username == username).FirstOrDefault();
                if (getUser != null)
                {
                    var salt = getUser.Salt;
                    //Password Hasing Process Call Helper Class Method    
                    var encodingPasswordString = Helper.EncodePassword(password, salt);

                    var user = db.UserVendors.Where(a => a.Username.Equals(username) && a.Hash.Equals(encodingPasswordString)).SingleOrDefault();
                    return (user != null) ? true : false;
                }
                else
                {
                    return false;
                }
            }

        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            using (UserManagementEntities dbMaster = new UserManagementEntities())
            {
                using (POTrackingEntities db = new POTrackingEntities())
                {
                    var user = (from us in db.UserVendors//db.UserRoles
                                where (string.Compare(username, us.Username, StringComparison.OrdinalIgnoreCase) == 0) //&& us.Role.ApplicationID==3
                                select us).FirstOrDefault();

                    if (user == null)
                    {

                        var userInternal = (from us in dbMaster.UserRoles
                                            where (string.Compare(username, us.Username, StringComparison.OrdinalIgnoreCase) == 0) && us.Role.ApplicationID == 3
                                            select us).FirstOrDefault();
                        if (userInternal == null)
                        {
                            return null;
                        }
                        else
                        {
                            var userInternalCustom = new UserVendorProxy() {ID= new Guid(), Email = userInternal.User.Email, Username= userInternal.Username, Name= userInternal.User.Name, RoleName= userInternal.Role.Name};
                            var selectedUser = new CustomMembershipUser(userInternalCustom);

                            return selectedUser;
                        }
                    }
                    else
                    {
                        var userVendorCustom = new UserVendorProxy() { ID = user.ID, Email = user.Email, Username = user.Username, Name = user.Name, RoleName = LoginConstants.RoleVendor };
                        var selectedUser = new CustomMembershipUser(userVendorCustom);

                        return selectedUser;
                    }
                   
                }
            }

        }

        public override string GetUserNameByEmail(string email)
        {
            using (UserManagementEntities db = new UserManagementEntities())
            {
                string username = (from u in db.UserRoles
                                   where string.Compare(email, u.User.Email) == 0
                                   select u.Username).FirstOrDefault();

                return !string.IsNullOrEmpty(username) ? username : string.Empty;
            }
        }

        #region Overrides of Membership Provider

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool EnablePasswordReset
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool EnablePasswordRetrieval
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool RequiresUniqueEmail
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}