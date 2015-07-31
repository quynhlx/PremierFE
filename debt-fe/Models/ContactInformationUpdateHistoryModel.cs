using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace debt_fe.Models
{
	public class ContactInformationUpdateHistoryModel
	{
		public string UpdatedBy { get; set; }
		public DateTime UpdatedDate { get; set; }
		public int? ContactInformationId { get; set; }
	}
}