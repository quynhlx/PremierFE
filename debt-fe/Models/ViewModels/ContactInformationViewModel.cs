﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace debt_fe.Models.ViewModels
{
	public class ContactInformationViewModel
	{
		public List<ContactInformationModel> GetContactInformations(DataTable table)
		{
			if (table == null || table.Rows.Count==0)
			{
				return null;
			}

			var list = new List<ContactInformationModel>();

			foreach (DataRow row in table.Rows)
			{
				var model = GetContactInformation(row);
				list.Add(model);
			}

			return list;
		}

		public ContactInformationModel GetContactInformation(DataRow row)
		{
			var info = new ContactInformationModel();

			info.Id = int.Parse(row["Id"].ToString());
			info.HomePhone = row["HomePhone"].ToString();
			info.WorkPhone = row["WorkPhone"].ToString();
			info.MobilePhone = row["MobilePhone"].ToString();
			info.Preferred = int.Parse(row["Preferred"].ToString());
			info.State = row["State"].ToString();
			info.StreetAddress = row["StreetAddress"].ToString();
			info.Zip = row["Zip"].ToString();
			info.BestTimeToContact = row["BestTimeToContact"].ToString();
			info.City = row["City"].ToString();
			info.Email = row["Email"].ToString();

			return info;
		}
	}
}