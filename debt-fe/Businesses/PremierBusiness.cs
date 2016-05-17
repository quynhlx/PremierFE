using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using debt_fe.Models;
using Net.Code.ADONet;
using System.IO;
using RightSignatures;
using System.Configuration;
using NLog;
using System.Threading.Tasks;
using System.Net;

namespace debt_fe.Businesses
{
    public class PremierBusiness : IPremierBusiness
    {
        private Db _db = Db.FromConfig("Premier");
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public int DownloadSigntureDocumentFromServer(DocumentModel doc, int userId)
        {
            var uploadFolder = ConfigurationManager.AppSettings["UploadFolder"];
            var docPath = this.GetDocumentPath(doc.ID, null);
            string matchId = doc.DocGUID;
            this.UpdateLeadStatus(userId, "Contract Received", userId);
            var urlSigned = RightSignature.GetURLPDFSigned(matchId);

            try
            {
                uploadFolder = Path.Combine(uploadFolder, docPath);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message, "Error");
                return -1; 
            }

            if (!Directory.Exists(uploadFolder))
            {
                try
                {
                    Directory.CreateDirectory(uploadFolder);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, ex.Message, "Error");
                    return -2; //  create UploadFile Failed
                }
            }

            var savePath = Path.Combine(uploadFolder, doc.FileName);
            string status = string.Empty;
            string statusFile = string.Empty;
            bool canDownload = false;
            for (int i = 0; i < 10; i++)
            {
                RightSignature.GetStatusDocumentDetail(matchId, out status, out statusFile);
                {

                }
                if (status.Trim() == "signed" && statusFile.Trim() == "done-processing")
                {
                    canDownload = true;
                    break;
                }
                    
                else
                {
                    System.Threading.Thread.Sleep(3000);
                }
            }
            if(canDownload)
            {
                DownloadSignatureAsync(urlSigned, savePath);
            }
            else
            {
                return -3; // Can't dowload file
            }
            return 0;
        }

        private void DownloadSignatureAsync(UrlAfterSigned url, string fullPath)
        {
            _logger.Info("---Start DownloadSignatureAsync---");
            _logger.Debug("url = {0}, fullPath = {1}", url.UrlFileSigned, fullPath);

            Task.Factory.StartNew(new Action(() =>
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        client.DownloadFileTaskAsync(url.UrlFileSigned, fullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, ex.Message);
                    }
                }
               
            }));
            _logger.Info("---End DownloadSignatureAsync---");
        }

        public int EditDocument(DocumentModel doc)
        {
            string sqlQuery = "update Document set MemberISN=@memberISN, docName=@name, docDesc=@desc, CreditorISN=@creditorISN , docFileName=@fileName, docSize=@fileSize where DocumentISN=@ISN";
            if (string.IsNullOrWhiteSpace(doc.FileName))
            {
                sqlQuery = "update Document set MemberISN=@memberISN, docName=@name, docDesc=@desc, CreditorISN=@creditorISN , docSize=@fileSize where DocumentISN=@ISN";
            }
            var query = _db.Sql(sqlQuery);
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
            var list = result.AsEnumerable().Select(row =>
            new DocumentModel()
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
                UpdatedBy = row.updatedBy != null? row.updatedBy : 0,
                AddedDate = row.docAddedDate,
                docSignatureDate = row.docSignatureDate,
                AddedBy =  row.docAddedBy != null ? row.updatedBy : 0,
                docNoOfSign = row.docNoOfSign,
                GroupId = row.GroupID,
                docHistory = row.docHistory,
                DocGUID = row.docGuid,
                AddedName = row.docAddedName
                

            }).ToList();

            //var results = new List<DocumentModel>();
            int docGroupId = 0;
            foreach (var doc in list.OrderBy(d => d.ID).ToList())
            {
                if (doc.IsSignatureDocument && doc.GroupId.HasValue)
                {
                    if (doc.GroupId.Value == 0 || doc.GroupId.Value != docGroupId)
                    {
                        docGroupId = doc.GroupId.Value;
                        if (list.Count(d => d.docSignatureDate.HasValue && d.GroupId == docGroupId) == list.Count(d => d.GroupId == docGroupId))
                        {
                            doc.OneSigntureCompleted = true;
                        }
                        if (list.Count(d => !string.IsNullOrEmpty(d.FileName) && d.GroupId == docGroupId) == list.Count(d => d.GroupId == docGroupId))
                        {
                            doc.SigntureCompleted = true;
                        }
                    }
                    else
                    {
                        doc.IsSubSingature = true;
                    }
                }
            }


            return list;
        }
    
        public DocumentModel GetSigntureDocument(int Id)
        {
            var query = _db.Sql("select * from Vw_DebtExt_Document where GroupId = @GroupId").WithParameter("GroupId", Id);
            var list = query.AsEnumerable().Select(row =>
           new DocumentModel()
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
               docNoOfSign = row.docNoOfSign,
               GroupId = row.GroupID,
               docHistory = row.docHistory,
               DocGUID = row.docGuid

           }).ToList();
            int docGroupId = 0;
            foreach (var doc in list.OrderBy(d => d.ID).ToList())
            {
                if (doc.IsSignatureDocument && doc.GroupId.HasValue)
                {
                    if (doc.GroupId.Value == 0 || doc.GroupId.Value != docGroupId)
                    {
                        docGroupId = doc.GroupId.Value;
                        if (list.Count(d => d.docSignatureDate.HasValue && d.GroupId == docGroupId) == list.Count(d => d.GroupId == docGroupId))
                        {
                            doc.OneSigntureCompleted = true;
                        }
                        if (list.Count(d => !string.IsNullOrEmpty(d.FileName)) == list.Count(d => d.GroupId == docGroupId))
                        {
                            doc.SigntureCompleted = true;
                        }
                    }
                    else
                    {
                        doc.IsSubSingature = true;
                    }
                }
            }
            return list.FirstOrDefault(d => !d.IsSubSingature);
        }

        public SigntureStatus GetStatusSigntureDocument(int Id)
        {
            var query = _db.Sql("select * from Vw_DebtExt_Document where GroupId = @GroupId").WithParameter("GroupId", Id);
            var list = query.AsEnumerable().Select(row =>
           new DocumentModel()
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
               docNoOfSign = row.docNoOfSign,
               GroupId = row.GroupID

           }).ToList();
            var doc = list.FirstOrDefault();
            if (doc.docNoOfSign == 1)
            {
                if (list.Count(d => d.docSignatureDate.HasValue && d.GroupId == Id) < list.Count(d => d.GroupId == Id))
                {
                    return SigntureStatus.Wait;
                }
                if (list.Count(d => !string.IsNullOrEmpty(d.FileName) && d.GroupId == Id) == list.Count(d => d.GroupId == Id) ||
                    list.Count(d => d.docSignatureDate.HasValue && d.GroupId == Id) == list.Count(d => d.GroupId == Id))
                {
                    return SigntureStatus.Signed;
                }
            }
            if (doc.docNoOfSign == 2)
            {
                if (list.Count(d => d.docSignatureDate.HasValue && d.GroupId == Id) < list.Count(d => d.GroupId == Id))
                {
                    return SigntureStatus.Wait;
                }
                if ( list.Count(d => d.docSignatureDate.HasValue && d.GroupId == Id) == list.Count(d => d.GroupId == Id))
                {
                    return SigntureStatus.Signed;
                }
                if (list.Count(d => !string.IsNullOrEmpty(d.FileName) && d.GroupId == Id) == list.Count(d => d.GroupId == Id))
                {
                    return SigntureStatus.Signed;
                }
            }
            return SigntureStatus.None;
        }

        public DocumentModel GetSubSigntureDocument(int Id, bool IdIsMain)
        {
            string queryStr = "select * from Vw_DebtExt_Document where DocumentISN = @GroupId";
            if (IdIsMain)
            {
                queryStr = "select * from Vw_DebtExt_Document where GroupId = @GroupId";
            }
            var query = _db.Sql(queryStr).WithParameter("GroupId", Id);
            var list = query.AsEnumerable().Select(row =>
           new DocumentModel()
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
               docNoOfSign = row.docNoOfSign,
               GroupId = row.GroupID,
               docHistory = row.docHistory,
               DocGUID = row.docGuid

           }).ToList();
            if (!IdIsMain)
            {
                return list.FirstOrDefault();
            }
            int docGroupId = 0;
            foreach (var doc in list.OrderBy(d => d.ID).ToList())
            {
                if (doc.IsSignatureDocument && doc.GroupId.HasValue)
                {
                    if (doc.GroupId.Value == 0 || doc.GroupId.Value != docGroupId)
                    {
                        docGroupId = doc.GroupId.Value;
                        if (list.Count(d => d.docSignatureDate.HasValue && d.GroupId == docGroupId) == list.Count(d => d.GroupId == docGroupId))
                        {
                            doc.OneSigntureCompleted = true;
                        }
                        if (list.Count(d => !string.IsNullOrEmpty(d.FileName)) == list.Count(d => d.GroupId == docGroupId))
                        {
                            doc.SigntureCompleted = true;
                        }
                    }
                    else
                    {
                        doc.IsSubSingature = true;
                    }
                }
            }
            return list.FirstOrDefault(d => d.IsSubSingature);
        }

        public int UpdateDocGuid(int docId, string docGuid)
        {
            var query = _db.Sql("update Document set docGuid = @docGuid where DocumentISN = @docId").WithParameters(new { docGuid = docGuid, docId = docId });
            query.AsNonQuery();
            return 0;
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
            return sproc.AsNonQuery();
        }
        private int DocumentEdit(int DocumentISN, int MemberISN, string docFileName, string docSize, string docPublic, string docDesc, string CreditorISN, string docName, int docLast)
        {
            _logger.Info("---Start DocumentEdit---");
            if (string.IsNullOrEmpty(CreditorISN))
            {
                var sproc = _db.StoredProcedure("xp_debtext_documenttask_insupd").WithParameters(new {
                    DocumentISN = DocumentISN,
                    MemberISN = MemberISN,
                    docFileName = docFileName,
                    docSize = docSize,
                    docPublic = docPublic,
                    docDesc = docDesc,
                    docName = docName,
                    docLastAction = docLast
                });
                return sproc.AsNonQuery();

            }
            else
            {
                var sproc = _db.StoredProcedure("xp_debtext_documenttask_insupd").WithParameters(new
                {
                    DocumentISN = DocumentISN,
                    MemberISN = MemberISN,
                    docFileName = docFileName,
                    docSize = docSize,
                    docPublic = docPublic,
                    docDesc = docDesc,
                    docName = docName,
                    docLastAction = docLast,
                    CreditorISN = CreditorISN
                });
                return sproc.AsNonQuery();
            }
        }
        public void AddTemplateDefaut(int userId)
        {
            _logger.Info("---Start AddTemplateDefaut---");
            try
            {
                var docs = this.GetDocuments(userId);
                var check = docs.Count(m => m.MemberISN == userId && m.LastAction == "2");
                if (check >= 2) return;
                var templateId = ConfigurationManager.AppSettings["TeamplateISN_CreateLead"];
                //var dsTemplateName = new DataProvider().ExecuteQuery("Select TemplateISN, tplName, tplFile From DebtTemplate Where TemplateISN in (" + templateId + ")");

                var query = _db.Sql("Select TemplateISN, tplName, tplFile From DebtTemplate Where TemplateISN in (" + templateId + ")");
                var templates = query.AsEnumerable().Select(row => new { row.TemplateISN, row.tplName, row.tplFile }).ToList();

                foreach (var item in templates)
                {
                    var docISN = DocumentEdit(0, userId, item.tplFile , null, "1", null, null, item.tplName, 2);
                    if(docISN > 0) {
                        var folder = GetTemplatePath(Convert.ToInt32(item.TemplateISN), null);
                        folder = Path.Combine(ConfigurationManager.AppSettings["UploadFolder"], folder);
                        var pathFull = Path.Combine(folder, item.tplFile);
                        if (System.IO.File.Exists(pathFull))
                        {
                            var pathSave = Path.Combine(ConfigurationManager.AppSettings["UploadFolder"], GetDocumentsPath(docISN, null));
                            if (!Directory.Exists(pathSave)) Directory.CreateDirectory(pathSave);
                            System.IO.File.Copy(pathFull, Path.Combine(pathSave, item.tplFile));
                        }
                    }
                }
               
                _logger.Info("---End AddTemplateDefaut---");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
        }
        public string GetDocumentsPath(int DocumentISN, object AddedDate)
        {
            _logger.Info("---Start GetDocumentsPath---");
            DateTime date = DateTime.Now;
            if (AddedDate == null)
            {
                var query = _db.Sql("Select docAddedDate From Document Where DocumentISN = @DocumentISN").WithParameter("DocumentISN", DocumentISN).AsEnumerable().Select(row => new { row.docAddedDate });
                AddedDate = query.FirstOrDefault().docAddedDate;
            
            }
            if (AddedDate != null) date = Convert.ToDateTime(AddedDate);
            string strPath = date.ToString("yyyyMM") + "\\" + "Documents\\" + date.ToString("dd") + "\\" + DocumentISN.ToString();
            _logger.Info("---End GetDocumentsPath---");
            return strPath;
        }
        private string GetTemplatePath(int TemplateISN, object AddedDate)
        {
            _logger.Info("---Start GetTemplatePath---");
            DateTime date = DateTime.Now;
            if (AddedDate == null)
            {
                var query = _db.Sql("Select addedDate From DebtTemplate Where TemplateISN= @TempISN").WithParameter("TempISN", TemplateISN).AsEnumerable().Select(row=> new { row.addedDate });
                AddedDate = query.FirstOrDefault().addedDate;
            }
            if (AddedDate != null) date = Convert.ToDateTime(AddedDate);
            string strPath = date.ToString("yyyyMM") + "\\" + "DebtExtTemplate\\" + date.ToString("dd") + "\\" + TemplateISN.ToString();
            _logger.Info("---End GetTemplatePath---");
            return strPath;
        }

        public void RollBackSigntureDocument(int docId)
        {
            var query = _db.Sql("update Document set docSignatureDate = null, docGuid = null, docFileName = null where DocumentISN = @docId")
                .WithParameter("docId", docId).AsNonQuery();
        }
    }
}