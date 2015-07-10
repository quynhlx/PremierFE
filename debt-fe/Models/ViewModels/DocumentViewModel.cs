using debt_fe.DataAccessHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

namespace debt_fe.Models.ViewModels
{
	public class DocumentViewModel
	{
		public string DocName { get; set; }
		public HttpPostedFileBase FileName { get; set; }
		public string Creditor { get; set; }
		public string Notes { get; set; }

		private List<CreditorModel> _creditors;
		public int SelectedCreditorID { get; set; }

		public IEnumerable<SelectListItem> CreditorItems
		{
			get
			{
				return new SelectList(_creditors, "ID", "ViewName");
			}
		}

		public DocumentViewModel()
		{
			this._creditors = new List<CreditorModel>();
		}

		public DocumentViewModel(int memberISN) : this()
		{
			_creditors = GetCreditors(memberISN);
		}

		public DocumentViewModel(int memberISN, int documentISN):this(memberISN)
		{
			var document = 1;
		}

		private List<CreditorModel> GetCreditors(int memberISN)
		{
			var query = "select * from Creditor where MemberISN=@MemberISN";
			var parameters = new Hashtable();
			parameters.Add("MemberISN", memberISN);


			var dataProvider = new DataProvider("tbone", "tbone");
			var tableCreditor = dataProvider.ExecuteQuery(query, parameters);

			var creditors = new List<CreditorModel>();

			if (tableCreditor != null && tableCreditor.Rows.Count > 0)
			{
				foreach (DataRow row in tableCreditor.Rows)
				{
					var creditor = new CreditorModel();

					creditor.ID = int.Parse(row["CreditorISN"].ToString());
					creditor.Name = row["cdtName"].ToString();
					creditor.AccountNumber = row["cdtAcct#"].ToString();

					creditors.Add(creditor);
				}
			}

			return creditors;
		}

		public string GetCreditorName(int creditorID, int memberISN)
		{
			var creditors = GetCreditors(memberISN);
			try
			{
				var creditor = creditors.First(c => c.ID == creditorID);

				return creditor.Name;
			}
			catch
			{
				return string.Empty;
			}
		}
	}
}