using debt_fe.DataAccessHelper;
using debt_fe.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace debt_fe.Businesses
{
    public class DocumentBusiness
    {
		private DataProvider _data;

        public DocumentBusiness()
        {
			_data = new DataProvider("tbone", "tbone");
        }

		public List<DocumentModel> GetDocuments(int memberISN)
		{
			var query = "select * from Vw_DebtExt_Document where MemberISN=@LoginISN";
			
			var parameters = new Hashtable();
			parameters.Add("LoginISN", memberISN);

			var table = _data.ExecuteQuery(query, parameters);

			var documents = GetDocumentsFromTable(table);

			return documents;
		}

        private List<DocumentModel> GetDocumentsFromTable(DataTable table)
        {
            var documents = new List<DocumentModel>();

            if (table == null || table.Rows.Count == 0)
            {
                return documents;
            }

            foreach (DataRow row in table.Rows)
            {
                var doc = GetDocumentFromRow(row);

                if (doc != null)
                {
                    documents.Add(doc);
                }
            }

            return documents;
        }

        private DocumentModel GetDocumentFromRow(DataRow row)
        {
            if (row == null || row.Table.Columns.Count==0)
            {
                return null;
            }

            var doc = new DocumentModel();

            doc.ID = int.Parse(row["DocumentISN"].ToString());
            doc.MemberISN = int.Parse(row["MemberISN"].ToString());
            doc.FileName = row["docFileName"].ToString();
            doc.DocName = row["docName"].ToString();
            doc.Desc = row["docDesc"].ToString();
			doc.CreditorName = row["cdtName"].ToString();
            doc.AddedDate = DateTime.Parse(row["docAddedDate"].ToString());

            return doc;
        }

		/// <summary>
		/// add a document with store xp_debtext_document_insupd 
		/// </summary>
		/// <param name="document">a document to add</param>
		public int UploadDocument(DocumentModel document)
		{
			//
			// Insert Into Document(
			//
			
			var parameters = new Hashtable();
			parameters.Add("MemberISN", document.MemberISN);
			parameters.Add("docName",document.DocName);
			parameters.Add("docFileName", document.FileName);
			parameters.Add("docSize", document.FileSize);
			parameters.Add("docPublic", document.Public);
			parameters.Add("docStatus", document.Status);
			parameters.Add("docDesc", document.Desc);
			parameters.Add("docLastAction", document.LastAction);
			parameters.Add("docSignatureStatus", document.SignatureStatus);
			parameters.Add("CreditorISN",document.CreditorISN);
			parameters.Add("updatedBy",document.UpdatedBy);
			// parameters.Add("docAddedBy", document.AddedBy);

			var documentISN = (int)_data.ExecuteStoreProcedure("xp_debtext_document_insupd", parameters);

			return documentISN;
		}
    }
}