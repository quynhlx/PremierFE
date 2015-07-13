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
		// private string _viewName;
		public string ViewName
		{
			get
			{
				return string.Format("{0} {1}", this.Name, this.AccountNumber);
			}
		}

		public override string ToString()
		{
			return string.Format("{0} {1}",this.Name,this.AccountNumber);
		}
	}
}