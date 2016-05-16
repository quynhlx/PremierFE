using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Net.Code.ADONet;

namespace debt_fe.SignInManager
{
    public class PremierUserStore : IUserStore<PremierUser>, IUserSecurityStampStore<PremierUser>
    {
        private Db _db = Db.FromConfig("Premier");
        public Task CreateAsync(PremierUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(PremierUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
        public async Task<PremierUser> FindByIdAsync(string userId)
        {
            int id = Convert.ToInt32(userId);
            var sproc = _db.Sql("select * from Member where MemberISN = @MemberISN").WithParameter("MemberISN", id);
            var result = await sproc.AsEnumerableAsync();
            var users = result.Select(row =>
            new PremierUser
            {
                Id =  Convert.ToString(row.MemberISN),
                Email = row.memEmail,
                FirstName = row.memFirstName,
                LastName = row.memLastName,
                Phone = row.memPhone,
                UserName = row.memUserName,
                PasswordHash = row.memPassword
            });
            var user = users.FirstOrDefault();
            if (user != null)
            {
                var query2 = _db.Sql("select val_number as TwoFactorEnabled  from MemberExt3 where AttributeISN = 581 and MemberISN  = 67956").
                    WithParameter("MemberISN", user.ISN).AsEnumerable().Select(row => new { TwoFactorEnabled = row.TwoFactorEnabled });
                int? mfa = Convert.ToInt32(query2.FirstOrDefault().TwoFactorEnabled);
                if (mfa.HasValue && mfa.Value == 1)
                {
                    user.TwoFactorEnabled = true;
                }
            }
            return user;
        }
        public async Task<PremierUser> FindByNameAsync(string userName)
        {
            var query = _db.Sql("select * from Member where memUserName = @userName").WithParameter("userName", userName);
            var result = await query.AsEnumerableAsync();
            var users = result.Select(row =>
            new PremierUser
            {
                Id = Convert.ToString(row.MemberISN),
                Email = row.memEmail,
                FirstName = row.memFirstName,
                LastName = row.memLastName,
                Phone = row.memPhone,
                UserName = row.memUserName,
                PasswordHash = row.memPassword
            });
            var user = users.FirstOrDefault();
            if(user != null)
            {
                var query2 = _db.Sql("select val_number as TwoFactorEnabled  from MemberExt3 where AttributeISN = 581 and MemberISN  = 67956").
                    WithParameter("MemberISN", user.ISN).AsEnumerable().Select(row => new { TwoFactorEnabled = row.TwoFactorEnabled });
                int? mfa = Convert.ToInt32(query2.FirstOrDefault().TwoFactorEnabled);
                if (mfa.HasValue && mfa.Value == 1)
                {
                    user.TwoFactorEnabled = true;
                }
            }
            return user;
        }

        public Task<string> GetSecurityStampAsync(PremierUser user)
        {
            return Task.Run(() =>
            {
                return string.Empty;
            });
        }

        public Task SetSecurityStampAsync(PremierUser user, string stamp)
        {
            return Task.Run(() =>
            {
                // HttpContext.Current.Session.Add("SecurityStamp", stamp);
                return;
            });
        }

        public Task UpdateAsync(PremierUser user)
        {
            throw new NotImplementedException();
        }
    }
}