using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debt_fe.Models.ViewModels
{
    public class BaseViewModel
    {
        public HeaderInfoViewModel HeaderViewModel { set; get; }
        public ManagementAccountModel ManagerViewModel { set; get; }
    }
}
