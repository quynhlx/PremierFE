using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Net.Code.ADONet;

namespace debt_fe.SignInManager
{
    public class PremierUserStore : IUserStore<PremierUser>
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
            var sproc = _db.StoredProcedure("xp_debtuser_getinfo").WithParameter("MemberISN", userId);
            var result = await sproc.AsEnumerableAsync();
            var users = result.Select(row =>
            new PremierUser
            {
                Id = row.MemberISN,
                Email = row.memEmail,
                FirstName = row.memFirstName,
                LastName = row.memLastName,
                Phone = row.memPhone,
                UserName = row.memUserName,
            });
            var user = users.FirstOrDefault();
            return user;
        }

        public async Task<PremierUser> FindByNameAsync(string userName)
        {
            var query = _db.Sql("select * from Member where memUserName = @userName").WithParameter("userName", userName);
            var result = await query.AsEnumerableAsync();
            var users = result.Select(row =>
            new PremierUser
            {
                Id = row.MemberISN,
                Email = row.memEmail,
                FirstName = row.memFirstName,
                LastName = row.memLastName,
                Phone = row.memPhone,
                UserName = row.memUserName,
            });
            var user = users.FirstOrDefault();
            return user;
        }

        public Task UpdateAsync(PremierUser user)
        {
            throw new NotImplementedException();
        }
    }
}