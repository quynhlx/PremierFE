using debt_fe.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace debt_fe.Businesses
{
    public class DocumentBusiness
    {
        public DocumentBusiness()
        {

        }

        public List<DocumentModel> GetList(DataTable table)
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
            doc.CreditorName = "";
            doc.AddedDate = DateTime.Parse(row["docAddedDate"].ToString());

            return doc;
        }
    }
}