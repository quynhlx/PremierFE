using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace debt_fe.Models
{
	public class ContactInformationModel
	{
		public int Id { get; set; }
		public string HomePhone { get; set; }
		public string WorkPhone { get; set; }
		public string MobilePhone { get; set; }
		public int Preferred { get; set; }
		/*
		 * preferred code
		 * 
		 * 1: home phone
		 * 2: work phone
		 * 3: mobile phone
		 * 
		 * */
		public string Email { get; set; }
		public string BestTimeToContact { get; set; }
		public string StreetAddress { get; set; }
		public string City { get; set; }
		public string Zip { get; set; }
		public string State { get; set; }
	}
}