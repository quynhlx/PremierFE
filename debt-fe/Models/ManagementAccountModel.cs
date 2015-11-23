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
        public string NameType = "Account Manager";

        public ManagementAccountModel(int MemberISN)
        {
            _dataProvider = new debt_fe.DataAccessHelper.DataProvider("tbone", "tbone");
            FullName = "";
            Email = "";
            Phone = "";
            GetDataFromDataBase(MemberISN);
        }
        public void GetDataFromDataBase(int MemberISN)
        {
            try
            {

            
            var paramNames2 = new List<string>
			{
				"MemberISN"
			};

            var paramValues2 = new System.Collections.ArrayList{MemberISN};
            int rs;
            var admin = _dataProvider.ExecuteStoreProcedure("xp_lead_accountmanager_getinfo", paramNames2, paramValues2, out rs);
            if (admin.Tables[0].Rows.Count >0 )
            {
                var Table = admin.Tables[0];
                var ISN = (int)Table.Rows[0]["MemberISN"];
                var paramNames3 = new List<string> { "MemberISN" };

                var paramValues3 = new System.Collections.ArrayList{ISN};
                var TableMember = _dataProvider.ExecuteQuery("select * from Vw_Debt_dMember where MemberISN = @MemberISN", paramNames3, paramValues3);
                if (TableMember.Rows.Count  != 0)
                {
                    this.FullName = TableMember.Rows[0]["memFullName"].ToString();
                    this.Email = TableMember.Rows[0]["memEmail"].ToString();
                    this.Phone = TableMember.Rows[0]["memPhone"].ToString();
                }
            }
            else
            {
                var saleman = _dataProvider.ExecuteStoreProcedure("xp_debtuser_getinfo", paramNames2, paramValues2, out rs);
                if(saleman.Tables[0].Rows.Count > 0 && saleman.Tables[0].Rows[0]["AgentISN"] != null)
                {
                    var salemanISN = Convert.ToInt32(saleman.Tables[0].Rows[0]["AgentISN"]);
                    var paramNames4 = new List<string> { "MemberISN" };
                    var paramValues4 = new System.Collections.ArrayList { salemanISN };
                    var salemanTable = _dataProvider.ExecuteQuery("select * from Vw_DebtStaff where MemberISN= @MemberISN", paramNames4, paramValues4);
                    if (salemanTable.Rows.Count > 0)
                    {
                        this.FullName = salemanTable.Rows[0]["memUserName"].ToString();
                        this.Email = salemanTable.Rows[0]["memEmail"].ToString();
                        this.Phone = salemanTable.Rows[0]["memPhone"].ToString();
                        NameType = "Account Salesman";
                    }
                }
            }
            }
            catch
            {
                NameType = "Account Salesman";
                this.FullName = string.Empty;
                this.Email = string.Empty;
                this.Phone = string.Empty;
            }
        }
    }
}
