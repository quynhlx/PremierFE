using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debt_fe.Models.ViewModels
{
    public class DebtViewModel
    {
        public int CreditorISN { set; get; }
        public int Id;
        public string DebtName { set; get; }
        public string Creditor { set; get; }
        public string AccountNumber { set; get; }
        public Decimal DebtAmount { set; get; }
        public string DebtAmountStr 
        { 
            get 
            {
                if (DebtAmount != 0) return DebtAmount.ToString("C").Trim(new char[] { '$' });
                return string.Empty;
            } 
        }
        public string Collector { set; get; }
        private string _status;
        public string Status { 
            set { this._status = value;}
            get {
                if (string.IsNullOrEmpty(_status)) return "Waiting more info expected";
                else return _status;
            }
        }
    }
}
