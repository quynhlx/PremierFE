using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace debt_fe.Models
{
	public class CreditorModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string AccountNumber { get; set; }
        public string DebtName { set; get; }
		// private string _viewName;
		public string ViewName
		{
			get
			{				
                if (string.IsNullOrEmpty(this.AccountNumber))
                {
                    return string.Format("{0}", this.Name);
                }
                else
                {
                    return string.Format("{0} ({1})", this.Name, this.AccountNumber);
                }
			}
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})",this.Name,this.AccountNumber);
		}
	}
}