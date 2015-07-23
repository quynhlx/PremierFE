using debt_fe.DataAccessHelper;
using debt_fe.Models;
using debt_fe.Utilities;
using log4net;
using RightSignatures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
            var query = "select * from Vw_DebtExt_Document where MemberISN=@LoginISN  order by docAddedDate desc";

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
            if (row == null || row.Table.Columns.Count == 0)
            {
                return null;
            }

            var doc = new DocumentModel();

            

            doc.ID = int.Parse(row["DocumentISN"].ToString());

            var memberISN = 0;
            int.TryParse(row["MemberISN"].ToString(), out memberISN);


            doc.MemberISN = memberISN;


            doc.FileName = row["docFileName"].ToString();
            doc.DocName = row["docName"].ToString();
            doc.Desc = row["docDesc"].ToString();
            doc.CreditorName = row["cdtName"].ToString();
            doc.AddedDate = DateTime.Parse(row["docAddedDate"].ToString());

            doc.Public = false;
            var isPublic = row["docPublic"].ToString();
            if (!string.IsNullOrEmpty(isPublic) && !string.IsNullOrWhiteSpace(isPublic))
            {
                if (isPublic.Trim().Equals("1"))
                {
                    doc.Public = true;
                }
            }
                        
            doc.CanSign = false;
            var canSign = row["docSignatureStatus"].ToString();
            if (!string.IsNullOrEmpty(canSign))
            {
                if (canSign.Trim().Equals("1"))
                {
                    doc.CanSign = true;
                }
            }

            var creditorISN = row["CreditorISN"].ToString();
            if (!string.IsNullOrEmpty(creditorISN))
            {
                doc.CreditorISN = int.Parse(creditorISN);
            }


            return doc;
        }


        public string GetDocumentPath(int documentISN, DateTime? addedDate)
        {
            var path = string.Empty;

            DocumentModel document = null;

            if (addedDate == null)
            {
                //
                // TODO: get document's added date
                var query = "select * from Vw_DebtExt_Document";
                var ds = _data.ExecuteQuery(query);

                var documents = GetDocumentsFromTable(ds);

                try
                {
                    document = documents.First(d => d.ID == documentISN);
                    addedDate = document.AddedDate;
                }
                catch (Exception ex)
                {
                    _logger.Info("cannot get document path", ex);

                    return string.Empty;
                }
            }


            path = Path.Combine(
                    addedDate.Value.ToString("yyyyMM"),
                    "Documents",
                    addedDate.Value.ToString("dd"),
                    documentISN.ToString());
            

            return path;
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
            parameters.Add("docName", document.DocName);
            parameters.Add("docFileName", document.FileName);
            parameters.Add("docSize", document.FileSize);
            parameters.Add("docPublic", document.Public);
            parameters.Add("docStatus", document.Status);
            parameters.Add("docDesc", document.Desc);
            parameters.Add("docLastAction", document.LastAction);
            parameters.Add("docSignatureStatus", document.SignatureStatus);
            parameters.Add("CreditorISN", document.CreditorISN);
            parameters.Add("updatedBy", document.UpdatedBy);
            // parameters.Add("docAddedBy", document.AddedBy);

            // _logger.InfoFormat("");
            _logger.InfoFormat("[upload_document] store {0} - parameters {1}", "xp_debtext_document_insupd", Utility.HashtableToString(parameters));
            // parameters.

            var documentISN = (int)_data.ExecuteStoreProcedure("xp_debtext_document_insupd", parameters);

            _logger.InfoFormat("[upload_document] document isn = {0}", documentISN);

            return documentISN;
        }

        public bool EditDocument(DocumentModel document)
        {
            _logger.InfoFormat("[edit_document] prepare data");

            //if (!CanEditDocument(document.DocName, document.ID, document.MemberISN))
            //{
            //    _logger.Info("Cannot edit document cuz duplicated name");
            //    return false;
            //}



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

                _logger.InfoFormat("[edit_document] edit success {0}", document.ID);

                return true;
            }
            catch (Exception ex)
            {
                _logger.InfoFormat("[edit_document] error {0}-{1}", ex.Message, ex);

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
        public bool CanEditDocument(string documentName, int documentISN, int memberISN, bool ignoreCase = true)
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

        public int GetSignatureId(int documentISN, int memberISN, string signName)
        {
            int signId = -1;

            var parameters = new Hashtable();
            parameters.Add("sign_document_name", signName);
            parameters.Add("entityISN", memberISN);
            parameters.Add("documentISN", documentISN);

            signId = (int)_data.ExecuteStoreProcedure("xp_rightsignature_document_insupd", parameters);

            return signId;
        }

        public TemplateModel GetTemplateByDocumentId(int documentId, int memberISN, int templateId, int? signId)
        {
            /*
            var templateId = GetTemplateId(documentId);
            if (templateId <= 0)
            {
                return null;
            }

            templateId = 1;
             */ 

            var query = "select * from Vw_DebtTemplate where TemplateISN = @templateId";
            var parameters = new Hashtable();
            parameters.Add("templateId", templateId);

            DataTable table = null;

            try
            {
                table = _data.ExecuteQuery(query: query, parameters: parameters);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);

                return null;
            }

            var template = GetTemplateFromRow(table.Rows[0], memberISN);

            

            return template;
        }

        private TemplateModel GetTemplateFromRow(DataRow row, int memberISN)
        {
            if (row.Table.Rows.Count==0||row.Table.Columns.Count==0)
            {
                return null;
            }

            var model = new TemplateModel();

            model.TemplateId = int.Parse(row["TemplateISN"].ToString());
            model.TemplateName = row["tplName"].ToString();
            model.TemplateDesc = row["tplDesc"].ToString();
            
            var status = row["tplStatus"].ToString();
            if (!string.IsNullOrEmpty(status) && status.Equals("1"))
            {
                model.TemplateStatus = true;
            }

            model.TemplateFile = row["tplFile"].ToString();
            model.AddedDate = DateTime.Parse(row["addedDate"].ToString());
            model.AddedBy = row["addedBy"].ToString();
            model.UpdatedDate = DateTime.Parse(row["updatedDate"].ToString());
            model.UpdatedBy = row["updatedBy"].ToString();

            var canSign = row["tplRightSign"].ToString();
            if (!string.IsNullOrEmpty(canSign) && canSign.Equals("1"))
            {
                model.CanSign = true;
            }

            model.SignGuid = row["tplRightSignGUID"].ToString();
            model.SignerRole = row["tplSignerRole"].ToString();
            model.AddedName = row["addedName"].ToString();
            model.UpdatedName = row["updatedName"].ToString();

            var mergeField = row["tplMergeFields"].ToString();
            if (!string.IsNullOrEmpty(mergeField))
            {
                model.MergeFields = new List<RightSignatures.Structs.MergeField>();
                DataSet ds = null;

                try
                {
                    ds = Utility.ConvertXMLToDataSet(mergeField);

                    /*
                       <root>
                          <item attID="fistname" attValue="memFirstName" />
                          <item attID="lastname" attValue="memLastName" />
                          <item attID="address" attValue="memAddress" />
                          <item attID="phone" attValue="memPhone" />
                          <item attID="test" attValue="memZip" />
                        </root>
                     */

                }
                catch(Exception ex)
                {
                    _logger.Error("Cannot convert xml to dataset. Error message = "+ex.Message, ex);
                }
                
                //
                // LOL, it's very complicated

                if (ds != null && ds.Tables.Count>0 && ds.Tables[0].Rows.Count>0)
                {
                    var tableMapField = ds.Tables[0];

                    var parameters = new Hashtable();
                    parameters.Add("MemberISN", memberISN);

                    var userInfo = _data.ExecuteQuery("select * from Member where MemberISN=@MemberISN", parameters: parameters);

                    if (userInfo != null && userInfo.Rows.Count>0)
                    {
                        var rowUser = userInfo.Rows[0];

                        foreach (DataRow rowMapField in tableMapField.Rows)
                        {
                            var field = new Structs.MergeField();

                            try
                            {
                                field.name = rowMapField["attID"].ToString();
                                field.value = rowUser[rowMapField["attValue"].ToString()].ToString();

                                model.MergeFields.Add(field);
                            }
                            catch(Exception ex)
                            {
                                _logger.ErrorFormat("Cannot get column value. Error message: {0}",ex.Message);
                            }
                        }
                    }
                }

            }
            

            return model;
        }

        public int GetTemplateId(int documentId)
        {
            var templateId = -1;

            var query = "select * from Vw_DebtExt_Document where DocumentISN=@docISN";

            var parameters = new Hashtable();
            parameters.Add("docISN", documentId);

            try
            {
                var ds = _data.ExecuteQuery(query, parameters);
                
                if (int.TryParse(Utility.GetColumnValue(ds.Rows[0], "TemplateISN"), out templateId))
                {
                    return templateId;
                }
                else
                {
                    return -1;
                }
                
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);

                return -1;
            }
        }
    }
}