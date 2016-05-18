using debt_fe.DataAccessHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using debt_fe.Businesses;

namespace debt_fe.Models.ViewModels
{
	public class DocumentViewModel
	{
        //private DocumentBusiness _docBusiness;
        private IPremierBusiness _premierBusiness; 

        public string DocName { get; set; }
		public HttpPostedFileBase UploadedFile { get; set; }
		public string Creditor { get; set; }
		public string Notes { get; set; }
        public int DocumentISN { get; set; }
        public string OldFileName { get; set; }
        public double OldFileSize { get; set; }
        public DateTime? AddedDate { get; set; }

		private List<CreditorModel> _creditors;

        public IEnumerable<CreditorModel> Creditors
        {
           get
            {
                return _creditors;
            }
        }
		public int? SelectedCreditorID { get; set; }

		public IEnumerable<SelectListItem> CreditorItems
		{
			get
			{
				return new SelectList(_creditors, "ID", "DebtName");
			}
		}

		public DocumentViewModel()
		{
			this._creditors = new List<CreditorModel>();
            _premierBusiness = new PremierBusiness();
		}

		public DocumentViewModel(int memberISN) : this()
		{
			_creditors = GetCreditors(memberISN);
		}

		public DocumentViewModel(int memberISN, int documentISN):this(memberISN)
		{
            this.DocumentISN = documentISN;

            var documents = _premierBusiness.GetDocuments(memberISN);

            if (documents != null && documents.Count>0)
            {
                var document = documents.Find(d => d.ID == documentISN);

                if (document != null)
                {
                    this.DocName = document.DocName;
                    this.Notes = document.Desc;
                    this.SelectedCreditorID = document.CreditorISN;
                    this.OldFileName = document.FileName;
                    this.OldFileSize = document.FileSize;
                    this.AddedDate = document.AddedDate;
                }
            }
			
		}

		private List<CreditorModel> GetCreditors(int memberISN)
		{
            Net.Code.ADONet.Db db = Net.Code.ADONet.Db.FromConfig("premier");
            var query = db.Sql("select Creditor, CreditorISN, cdtName, cdtAcctNo  from Vw_DebtExt_Creditor where MemberISN=@MemberISN and cdtIsPush =1").WithParameter("MemberISN", memberISN);
            var creditors = query.AsEnumerable().Select(row => new CreditorModel() {
                ID = row.CreditorISN,
                AccountNumber = row.cdtAcctNo,
                DebtName = row.cdtName,
                Name = row.Creditor
            }).ToList();
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