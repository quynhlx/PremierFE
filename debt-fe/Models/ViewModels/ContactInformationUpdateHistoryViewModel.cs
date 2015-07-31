using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace debt_fe.Models.ViewModels
{
	public class ContactInformationUpdateHistoryViewModel
	{
		public List<ContactInformationUpdateHistoryModel> GetContactInformationUpdateHistories(DataTable table)
		{
			if (table == null || table.Rows.Count==0)
			{
				return null;
			}

			var list = new List<ContactInformationUpdateHistoryModel>();

			foreach (DataRow row in table.Rows)
			{
				var history = GetContactInformationUpdateHistory(row);
				list.Add(history);
			}

			return list;
		}

		public ContactInformationUpdateHistoryModel GetContactInformationUpdateHistory(DataRow row)
		{
			var history = new ContactInformationUpdateHistoryModel();

			history.ContactInformationId = int.Parse(row[""].ToString());
			history.UpdatedBy = row["UpdatedBy"].ToString();
			history.UpdatedDate = DateTime.Parse(row["UpdatedDate"].ToString());

			return history;
		}			

		
	}
}