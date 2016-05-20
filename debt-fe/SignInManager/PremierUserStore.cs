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
            var sproc = _db.Sql("select * from Vw_Debt_dMember where MemberISN = @MemberISN").WithParameters(new { MemberISN = userId });
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
                DealerISN = row.DealerISN
            });
            var user = users.FirstOrDefault();
            if (user != null)
            {
                var query2 = _db.Sql("select val_number as TwoFactorEnabled from MemberExt3 where AttributeISN = 581 and MemberISN = @MemberISN").
                    WithParameter("MemberISN", user.ISN).AsEnumerable().Select(row => new { TwoFactorEnabled = row.TwoFactorEnabled });
                var userext = query2.FirstOrDefault();
                if (userext != null)
                {
                    int? mfa = Convert.ToInt32(userext.TwoFactorEnabled);
                    if (mfa.HasValue && mfa.Value == 0 || !mfa.HasValue)
                    {
                        user.TwoFactorEnabled = true;
                    }
                }
                var query3 = _db.Sql("select memPassword from Member where MemberISN = @UserISN ").WithParameter("UserISN", user.ISN);
                var Reader = query3.AsReader();
                if (Reader.Read())
                {
                    user.PasswordHash = Reader.GetString(0);
                }
            }
            return user;
        }
        public async Task<PremierUser> FindByNameAsync(string userName)
        {
            var query = _db.Sql("select * from Vw_Debt_dMember where memUserName = @userName").WithParameters(new { userName = userName });
            var rs = await query.AsEnumerableAsync();
            var users = rs.Select(row =>
            new PremierUser
            {
                Id = Convert.ToString(row.MemberISN),
                Email = row.memEmail,
                FirstName = row.memFirstName,
                LastName = row.memLastName,
                Phone = row.memPhone,
                UserName = row.memUserName,
                //PasswordHash = row.memPassword,
                DealerISN = row.DealerISN
            });
            var user = users.FirstOrDefault();
            if(user != null)
            {
                var query2 = _db.Sql("select val_number as TwoFactorEnabled from MemberExt3 where AttributeISN = 581 and MemberISN = @MemberISN").
                    WithParameter("MemberISN", user.ISN).AsEnumerable().Select(row => new { TwoFactorEnabled = row.TwoFactorEnabled });
                var userext = query2.FirstOrDefault();
                if(userext != null)
                {
                    int? mfa = Convert.ToInt32(userext.TwoFactorEnabled);
                    if (mfa.HasValue && mfa.Value == 0 || !mfa.HasValue)
                    {
                        user.TwoFactorEnabled = true;
                    }
                }
                var query3 = _db.Sql("select memPassword from Member where MemberISN = @UserISN ").WithParameter("UserISN", user.ISN);
                var Reader = query3.AsReader();
                if(Reader.Read())
                {
                    user.PasswordHash = Reader.GetString(0);
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