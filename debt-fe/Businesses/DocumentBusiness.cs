using debt_fe.DataAccessHelper;
using debt_fe.Models;
using debt_fe.Utilities;
using log4net;
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
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private DataProvider _data;

        public DocumentBusiness()
        {
			_data = new DataProvider("tbone", "tbone");
        }

        /// <summary>
        /// get all documents by memberisn
        /// </summary>
        /// <param name="memberISN">a number of member id</param>
        /// <returns>a list of document model</returns>
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
            

            var creditorISN = row["CreditorISN"].ToString();
            if (!string.IsNullOrEmpty(creditorISN))
            {
                doc.CreditorISN = int.Parse(creditorISN);
            }
            

            return doc;
        }

		/// <summary>
		/// add a document with store xp_debtext_document_insupd 
		/// </summary>
		/// <param name="document">a document to add</param>
		public int UploadDocument(DocumentModel document)
		{
            _logger.InfoFormat("[upload_document] prepare data");
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

            // _logger.InfoFormat("");
            _logger.InfoFormat("[upload_document] store {0} - parameters {1}", "xp_debtext_document_insupd", Utility.HashtableToString(parameters));
            // parameters.

			var documentISN = (int)_data.ExecuteStoreProcedure("xp_debtext_document_insupd", parameters);

            _logger.InfoFormat("[upload_document] document isn = {0}",documentISN);

			return documentISN;
		}

        public bool EditDocument(DocumentModel document)
        {
            _logger.InfoFormat("[edit_document] prepare data");

            if (!CanEditDocument(document.DocName, document.ID, document.MemberISN))
            {
                _logger.Info("Cannot edit document cuz duplicated name");
                return false;
            }



            var query = string.Format("update Document set MemberISN=@memberISN, docName=@name, docDesc=@desc, CreditorISN=@creditorISN ");
            var parameters = new Hashtable();

            if (document.FileSize > 0)
            {
                query += ", docFileName=@fileName, docSize=@fileSize";
                parameters.Add("fileName", document.FileName);
                parameters.Add("fileSize", document.FileSize > 0 ? document.FileSize : 0);
            }

            query += " where DocumentISN=@ISN";
            parameters.Add("memberISN", document.MemberISN);
            parameters.Add("name", document.DocName);            
            parameters.Add("desc", document.Desc);
            parameters.Add("creditorISN", document.CreditorISN);
            parameters.Add("ISN", document.ID);

            _logger.InfoFormat("[edit_document] parameters {0}", Utility.HashtableToString(parameters));

            try
            {
                _data.ExecuteNonQuery(query, parameters);

                _logger.InfoFormat("[edit_document] edit success {0}",document.ID);

                return true;
            }
            catch(Exception ex)
            {
                _logger.InfoFormat("[edit_document] error {0}-{1}",ex.Message,ex);

                return false;
            }
        }

        /// <summary>
        /// check if document name is duplicate
        /// </summary>
        /// <param name="documentName">a string of document name</param>
        /// <param name="documentISN">an integer of document id</param>
        /// <param name="memberISN">an integer of member id</param>
        /// <param name="ignoreCase">a bool value to check name without case sensitive</param>
        /// <returns>a bool value of document can edit or not</returns>
        public bool CanEditDocument(string documentName, int documentISN, int memberISN, bool ignoreCase=true)
        {
            if (string.IsNullOrEmpty(documentName) || string.IsNullOrWhiteSpace(documentName))
            {
                return false;
            }

            var documents = GetDocuments(memberISN);
            var cannot = ignoreCase;

            if (ignoreCase)
            {
                cannot = documents.Any(d => d.ID != documentISN && d.DocName.ToLower().Equals(documentName));
            }
            else
            {
                cannot = documents.Any(d => d.ID != documentISN && d.DocName.Equals(documentName));
            }

            return !cannot;
        }
    }
}