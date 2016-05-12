using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using debt_fe.Models;
using Net.Code.ADONet;
using System.IO;

namespace debt_fe.Businesses
{
    public class PremierBusiness : IPremierBusiness
    {
        private Db _db = Db.FromConfig("Premier");

        public int EditDocument(DocumentModel doc)
        {
            var query = _db.Sql("update Document set MemberISN=@memberISN, docName=@name, docDesc=@desc, CreditorISN=@creditorISN , docFileName=@fileName, docSize=@fileSize where DocumentISN=@ISN");
            query.WithParameters(new
            {
                fileName = doc.FileName,
                fileSize = doc.FileSize > 0 ? doc.FileSize : 0,
                memberISN = doc.MemberISN,
                name = doc.DocName,
                desc = doc.Desc,
                creditorISN = doc.CreditorISN,
                ISN = doc.ID
            });
           return query.AsNonQuery();
        }

        public int EditSignatureDocument(DocumentModel doc)
        {
            var query = _db.Sql("update Vw_DebtExt_Document set docFileName=@Filename, docSignatureIP=@SendIP where DocumentISN=@DocId").
                WithParameters(new
                {
                    Filename = doc.FileName,
                    DocId = doc.ID,
                    SendId = doc.SendIP
                });
            return query.AsScalar<int>();

        }

        public string GetDocumentPath(int DocId, DateTime? AddedDate)
        {
            var path = string.Empty;

            var query = _db.Sql("select top 1 * from Vw_DebtExt_Document where DocumentISN = @DocId").WithParameter("DocId", DocId);
            var rs = query.AsEnumerable().Select(row => new { row.DocumentISN, row.docAddedDate }).FirstOrDefault();

            if (rs != null)
            {
                path = Path.Combine(
                   ((DateTime)rs.docAddedDate).ToString("yyyyMM"),
                   "Documents",
                    ((DateTime)rs.docAddedDate).ToString("dd"),
                  DocId.ToString());
            }
            else
            {
                var now = DateTime.Now;
                path = Path.Combine(
                   now.ToString("yyyyMM"),
                   "Documents",
                  now.ToString("dd"),
                  DocId.ToString());
            }
            return path;
        }

        public List<DocumentModel> GetDocuments(int memberISN)
        {
            var query = "select * from Vw_DebtExt_Document where MemberISN=@MemberISN  order by docAddedDate asc";
            var result = _db.Sql(query).WithParameters(new { MemberISN = memberISN });
            var list = result.AsEnumerable().Select(row => new DocumentModel()
            {
                ID = row.DocumentISN,
                MemberISN = row.MemberISN,
                DocName = row.docName,
                FileName = row.docFileName,
                FileSize = row.docSize == null ? 0 : row.docSize,
                Public = Convert.ToBoolean(row.docPublic),
                Status = Convert.ToBoolean(row.docStatus),
                Desc = row.docDesc,
                LastAction = row.docLastAction,
                SignatureStatus = row.docSignatureStatus,
                CreditorISN = row.CreditorISN,
                UpdatedBy = row.updatedBy,
                AddedDate = row.docAddedDate,
                docSignatureDate = row.docSignatureDate,
                AddedBy = row.docAddedBy,
                docNoOfSign = row.docNoOfSign

            }).ToList();
            return list;
        }

        public int UpdateDocSignature(DocumentModel doc, string appendNotes = "")
        {
            var sproc = _db.StoredProcedure("xp_debtext_document_signature_upd").WithParameters(
                new
                {
                    DocumentISN = doc.ID,
                    docFileName = doc.FileName,
                    docSignatureIP = doc.SignatureIP,
                    docGUID = doc.DocGUID,
                    AppendNotes = appendNotes,
                    updatedBy = doc.MemberISN
                }
                );
            int result = sproc.AsScalar<int>();
            return result;
        }

        public int UpdateHistory(int DocId, string Note)
        {
            var query = _db.Sql("Update Document set docHistory=isnull(docHistory,'') + @note where DocumentISN = @docISN")
                .WithParameters(new { docISN = DocId, note = Note });
            return query.AsNonQuery();
        }

        public int UpdateLeadStatus(int MemberISN, string Status, int UpdatedBy)
        {
            var sproc = _db.StoredProcedure("xp_debt_lead_status_upd").WithParameters(
                new
                {
                    MemberISN = MemberISN,
                    Status = Status,
                    updatedBy = UpdatedBy,
                    Action = "Contract Received"
                }
                );
            return sproc.AsScalar<int>();
        }
    }
}