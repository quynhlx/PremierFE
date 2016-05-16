using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace debt_fe.SignInManager
{
    public class PremierUser : IUser<string>
    {
        public int ISN
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Id)) return 0;
                return Convert.ToInt32(Id);
            }
        }
        public string Id { set; get; }
        public string UserName { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Phone { set; get; }
        public string Email { set; get; }
        public string PasswordHash { set; get; }
        public bool TwoFactorEnabled { set; get; }
    }
   



}