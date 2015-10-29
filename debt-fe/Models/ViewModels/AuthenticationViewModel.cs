using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debt_fe.Models.ViewModels
{
    public class AuthenticationViewModel
    {
        [Required]
        [Display(Name = "SMS Code")]
        public string SMSCode { set; get; }

        public string NumberPhone { set; get; }
        public string HideNumber
        {
            get
            {
                if (string.IsNullOrEmpty(NumberPhone)) return "N/A";
                else
                {
                    NumberPhone.Trim(new char[] {'(', ')'});
                    if (NumberPhone.Length > 4) return NumberPhone.Substring(NumberPhone.Length - 4, 4);
                    else return "N/A";
                }
            }
        }
    }
}
