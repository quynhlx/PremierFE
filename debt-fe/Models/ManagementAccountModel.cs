using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debt_fe.Models
{
    public class ManagementAccountModel
    {
        private debt_fe.DataAccessHelper.DataProvider _dataProvider;
        public string FullName { set; get; }
        public string Email { set; get; }
        public string Phone { set; get; }

        public ManagementAccountModel()
        {
            _dataProvider = new debt_fe.DataAccessHelper.DataProvider("tbone", "tbone");
            FullName = "No Data Found";
            Email = "No Data Found";
            Phone = "No Data Found";
        }
        public void GetDataFromDataBase(int MemberISN)
        {
            var paramNames2 = new List<string>
			{
				"MemberISN"
			};

            var paramValues2 = new System.Collections.ArrayList
			{
				MemberISN
			};
            int rs;
            var admin = _dataProvider.ExecuteStoreProcedure("xp_lead_accountmanager_getinfo", paramNames2, paramValues2, out rs);
            if (admin.Tables != null)
            {
                var Table = admin.Tables[0];
                var ISN = (int)Table.Rows[0]["MemberISN"];
                 var paramNames3 = new List<string>
			{
				"MemberISN"
			};

            var paramValues3 = new System.Collections.ArrayList
			{
				ISN
			};
            var TableMember = _dataProvider.ExecuteQuery("select * from Vw_Debt_dMember where MemberISN = @MemberISN", paramNames3, paramValues3);
                if (Table != null)
                {
                    this.FullName = TableMember.Rows[0]["memFullName"].ToString();
                    this.Email = TableMember.Rows[0]["memEmail"].ToString();
                    this.Phone = TableMember.Rows[0]["memPhone"].ToString();
                }
            }
        }
    }
}
