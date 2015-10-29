using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debt_fe.Models.ViewModels
{
    public class HeaderInfoViewModel
    {
        public DateTime DraftDate { set; get; }
        public Decimal DraftAmount { set; get; }
        public string DraftDateStr
        {
            get
            {
                if(DraftDate != DateTime.MinValue)
                {
                    return DraftDate.ToString("MM/dd/yyyy");
                }
                return "N/A";
            }
        }
        public string DraftAmountStr
        {
            get
            {
                if(DraftAmount != 0)
                {
                    return DraftAmount.ToString("C");
                }
                return "N/A";
            }
        }
    }
}
