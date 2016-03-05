using debt_fe.DataAccessHelper;
using debt_fe.Models;
using debt_fe.Utilities;
using log4net;
using RightSignatures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            var query = "select * from Vw_DebtExt_Document where MemberISN=@LoginISN  order by docAddedDate asc";

            var parameters = new Hashtable();
            parameters.Add("LoginISN", memberISN);

            var table = _data.ExecuteQuery(query, parameters);

            var documents = GetDocumentsFromTable(table);

            return documents;
        }

        public void UpdateLeadStatus(int memberISN, string status, int by)
        {
            var store = "xp_debt_lead_status_upd";
            var parameters = new Hashtable();
            parameters.Add("MemberISN", memberISN);
            parameters.Add("Status", status);
            parameters.Add("updatedBy", by);
            parameters.Add("Action", "Contract Received");
            var table = _data.ExecuteStoreProcedure(store, parameters);
        }
        public void UpdateDocSignature(int docID, string FileName, string UserIP, string BrowserInfo, int MemberISN)
        {
            var store = "xp_debtext_document_signature_upd";
            var parameters = new Hashtable();
            parameters.Add("DocumentISN", docID);
            parameters.Add("docFileName", FileName);
            parameters.Add("docSignatureIP", UserIP);
            parameters.Add("ClientInfo", BrowserInfo);
            parameters.Add("updatedBy", MemberISN);
            _data.ExecuteStoreProcedure(store, parameters);
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

            doc.IsSignatureDocument = false;

            var canSign = row["docSignatureStatus"].ToString();

            if (!string.IsNullOrEmpty(canSign))
            {
                if (canSign.Trim().Equals("1"))
                {
                    doc.IsSignatureDocument = true;
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

                    addedDate = DateTime.Now;

                    // return string.Empty;
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
            parameters.Add("docSignatureStatus", document.IsSignatureDocument);
            parameters.Add("CreditorISN", document.CreditorISN);
            parameters.Add("updatedBy", document.UpdatedBy);
            //parameters.Add("docSendIP", document.SendIP);
            //parameters.Add("docSignatureIP", document.SignatureIP);
            // parameters.Add("docAddedBy", document.AddedBy);

            // _logger.InfoFormat("");
            _logger.InfoFormat("[upload_document] store {0} - parameters {1}", "xp_debtext_documenttask_insupd", Utility.HashtableToString(parameters));
            // parameters.

            var documentISN = (int)_data.ExecuteStoreProcedure("xp_debtext_documenttask_insupd", parameters);

            _logger.InfoFormat("[upload_document] document isn = {0}", documentISN);

            return documentISN;
        }

        /// <summary>
        /// update document which type of signature document after sign
        /// </summary>
        /// <param name="filename">a string of download file</param>
        /// <param name="documentId">a number of document isn</param>
        /// <returns></returns>
        public bool EditSignatureDocument(string filename, int documentId, string IP)
        {
            var query = "update Vw_DebtExt_Document set docFileName=@filename, docSignatureIP=@SendIP where DocumentISN=@docId";

            var parameters = new Hashtable();
            parameters.Add("filename", filename);
            parameters.Add("docId", documentId);
            parameters.Add("SendIP", IP);
            try
            {
                _data.ExecuteNonQuery(query, parameters);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);

                return false;
            }
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
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);

                return null;
            }

            var template = GetTemplateFromRow(table.Rows[0], memberISN);
            string OutFirstPaymentDate = string.Empty;
            string OutTerm = string.Empty;
            var datas = GetScheduleOfPayment(memberISN, out OutFirstPaymentDate, out OutTerm );
            int i = 1;
            decimal TotalPaymentAmount = 0;
            decimal TotalBankFee = 0;
            decimal TotalSavingsAmount = 0;
            foreach ( var item in datas)
            {
                TotalPaymentAmount += Convert.ToDecimal(item.PaymentAmount.Replace("$", ""));
                TotalBankFee += Convert.ToDecimal(item.BankFee.Replace("$",""));
                TotalSavingsAmount += Convert.ToDecimal(item.SavingsAmount.Replace("$", ""));
                template.MergeFields.Add(new Structs.MergeField() {
                    name = string.Format("ProjectedDate{0}", i),
                    value = item.PaymentDate
                });
                template.MergeFields.Add(new Structs.MergeField()
                {
                    name = string.Format("PaymentAmount{0}", i),
                    value = item.PaymentAmount
                });
                template.MergeFields.Add(new Structs.MergeField()
                {
                    name = string.Format("BankFee{0}", i),
                    value = item.BankFee
                });
                template.MergeFields.Add(new Structs.MergeField()
                {
                    name = string.Format("SavingsAmount{0}", i),
                    value = item.SavingsAmount
                });
                if (i == 1 )
                {
                    template.MergeFields.Add(new Structs.MergeField()
                    {
                        name = "SavingsAmount",
                        value = item.SavingsAmount
                    });
                    template.MergeFields.Add(new Structs.MergeField()
                    {
                        name = "Term",
                        value = OutTerm
                    });
                    template.MergeFields.Add(new Structs.MergeField()
                    {
                        name = "ADSSSavingsAmount",
                        value = "$" + (Convert.ToDecimal(item.SavingsAmount.Replace("$","")) - Convert.ToDecimal(59 + 99 + 13.95)).ToString("#,##0.00")
                    });
                }
               
                
                i++;
            }
            template.MergeFields.Add(new Structs.MergeField()
            {
                name = "TotalPaymentAmount",
                value = "$" + TotalPaymentAmount.ToString("#,##0.00")
            });
            template.MergeFields.Add(new Structs.MergeField()
            {
                name = "TotalBankFee",
                value = "$" + TotalBankFee.ToString("#,##0.00")
            });
            template.MergeFields.Add(new Structs.MergeField()
            {
                name = "TotalSavingsAmount",
                value = "$" + TotalSavingsAmount.ToString("#,##0.00")
            });

            decimal PushTotal = 0;
            var data2 = GetDebtCreditorContract(memberISN);
            i = 1;
            foreach (var item in data2)
            {
                PushTotal += Convert.ToDecimal(item.AccountBalance.Replace("$", ""));
                template.MergeFields.Add(new Structs.MergeField()
                {
                    name = string.Format("Creditor{0}", i),
                    value = item.Creditor
                });
                template.MergeFields.Add(new Structs.MergeField()
                {
                    name = string.Format("AccountNumber{0}", i),
                    value = item.CollectorAccount
                });
                template.MergeFields.Add(new Structs.MergeField()
                {
                    name = string.Format("BalanceOriginal{0}", i),
                    value = item.AccountBalance
                });
                i++;
            }
            template.MergeFields.Add(new Structs.MergeField()
            {
                name = "PushTotal",
                value = "$" + PushTotal.ToString("#,##0.00")
            });
            template.MergeFields.Add(new Structs.MergeField()
            {
                name = "1stPaymentDate",
                value = OutFirstPaymentDate
            });
            template.MergeFields.Add(new Structs.MergeField()
            {
                name = "InvoiceAmount",
                value = "$" + TotalSavingsAmount.ToString("#,##0.00")
            });
            return template;
        }
        private TemplateModel GetTemplateFromRow(DataRow row, int memberISN)
        {
            if (row.Table.Rows.Count == 0 || row.Table.Columns.Count == 0)
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
                catch (Exception ex)
                {
                    _logger.Error("Cannot convert xml to dataset. Error message = " + ex.Message, ex);
                }

                //
                // LOL, it's very complicated

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var tableMapField = ds.Tables[0];

                    var parameters = new Hashtable();
                    parameters.Add("MemberISN", memberISN);
                    int returnValue = 0;
                    var dsUserInfo = _data.ExecuteStoreProcedure("xp_debtuser_getinfo", parameters, out returnValue);
                    var userInfo = dsUserInfo.Tables[0];
                    var userAttr = dsUserInfo.Tables[1];
                    var BankAcctOwner = "0";
                    var IsCoClient = "0";
                    try
                    {
                        BankAcctOwner = userAttr.Select("attID ='BankAcctOwner'")[0]["attValue"].ToString();
                        IsCoClient = userAttr.Select("attID ='IsCoClient'")[0]["attValue"].ToString();
                    }
                    catch
                    {
                        BankAcctOwner = "0";
                        IsCoClient = "0";
                    }

                    if (userInfo != null && userInfo.Rows.Count > 0)
                    {
                        var rowUser = userInfo.Rows[0];
                        foreach (DataRow rowMapField in tableMapField.Rows)
                        {
                            var field = new Structs.MergeField();
                            string mapFieldName = rowMapField["attID"].ToString();
                            string mapFieldValue = rowMapField["attValue"].ToString();
                            if(string.IsNullOrEmpty(mapFieldValue))
                            {
                                continue;
                            }
                            //
                            if (mapFieldName == "CurrentDate")
                            {

                                field.name = rowMapField["attID"].ToString();
                                field.value = DateTime.Now.ToString("MM/dd/yyyy");
                                model.MergeFields.Add(field);
                                continue;
                            }
                            if (mapFieldName == "11daysafterCurrentDate")
                            {

                                field.name = rowMapField["attID"].ToString();
                                field.value = DateTime.Now.AddDays(11).ToString("MM/dd/yyyy");
                                model.MergeFields.Add(field);
                                continue;
                            }
                            if (mapFieldName == "7businessdaysafterCurrentDate")
                            {

                                field.name = rowMapField["attID"].ToString();
                                field.value = DateTime.Now.AddDays(9).ToString("MM/dd/yyyy");
                                model.MergeFields.Add(field);
                                continue;
                            }
                            // Neu la attr
                            if (rowMapField["attValue"].ToString().Contains("_"))
                            {
                                string mapField = rowMapField["attValue"].ToString().Replace("_", "");
                                var attrRow = userAttr.Select("attID =" + "'" + mapField + "'");
                                try
                                {
                                    if (mapFieldName == "CoClientFullName")
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = userAttr.Select("attID ='CoFirstName'")[0]["attValue"].ToString() + " " + userAttr.Select("attID ='CoLastName'")[0]["attValue"].ToString();
                                        model.MergeFields.Add(field);
                                        continue;
                                    }
                                    if (mapFieldName.Contains("DOB") || mapFieldName.Contains("Date"))
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = Convert.ToDateTime(attrRow[0]["attValue"]).ToString("MM/dd/yyyy");
                                        model.MergeFields.Add(field);
                                        continue;
                                    }
                                    if (mapFieldName.Contains("SSN"))
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = attrRow[0]["attValue"].ToString();
                                        field.value = field.value.Substring(field.value.Length - 4);
                                        model.MergeFields.Add(field);
                                        continue;
                                    }
                                    if (mapFieldName.Contains("Phone"))
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = attrRow[0]["attValue"].ToString();
                                        field.value = Regex.Replace(field.value.Trim(new Char[] { ' ', '(', ')', '-' }), @"(\d{3})(\d{3})(\d{4})", "$1-$2-$3");
                                        model.MergeFields.Add(field);
                                        continue;
                                    }
                                    if (mapField == "BankAcctType")
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = attrRow[0]["attValue"].ToString() == "0" ? "Checking" : "Savings";
                                    }
                                    else
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = attrRow[0]["attValue"].ToString();
                                    }
                                    model.MergeFields.Add(field);
                                }
                                catch (Exception ex)
                                {
                                    field.value = string.Empty;
                                    model.MergeFields.Add(field);
                                    _logger.ErrorFormat("Cannot get column value. Error message: {0}", ex.Message);
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (mapFieldName.Contains("DOB") || mapFieldName.Contains("Date"))
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = Convert.ToDateTime(rowUser[rowMapField["attValue"].ToString()]).ToString("MM/dd/yyyy");
                                        model.MergeFields.Add(field);
                                        continue;
                                    }
                                    if (mapFieldName.Contains("SSN"))
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = rowUser[rowMapField["attValue"].ToString()].ToString();
                                        field.value = field.value.Substring(field.value.Length - 4);
                                        model.MergeFields.Add(field);
                                        continue;
                                    }
                                    if (mapFieldName.Contains("Phone"))
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = rowUser[rowMapField["attValue"].ToString()].ToString();
                                        field.value = Regex.Replace(field.value.Trim(new Char[] { ' ', '(', ')', '-' }), @"(\d{3})(\d{3})(\d{4})", "$1-$2-$3");
                                        model.MergeFields.Add(field);
                                        continue;
                                    }
                                    if (mapFieldValue == "memFullName")
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = rowUser[rowMapField["attValue"].ToString()].ToString().ToUpper();
                                    }
                                    else
                                    {
                                        field.name = rowMapField["attID"].ToString();
                                        field.value = rowUser[rowMapField["attValue"].ToString()].ToString();
                                    }


                                    model.MergeFields.Add(field);
                                }
                                catch (Exception ex)
                                {
                                    field.value = string.Empty;
                                    model.MergeFields.Add(field);
                                    _logger.ErrorFormat("Cannot get column value. Error message: {0}", ex.Message);
                                }
                            }
                            //



                        }


                        if (BankAcctOwner == null || BankAcctOwner == string.Empty || BankAcctOwner == "0")
                        {
                            ReplaceField(model.MergeFields, "AcctOwnerName", "ClientFullName");
                            ReplaceField(model.MergeFields, "AcctOwnerDOB", "ClientDOB");
                            ReplaceField(model.MergeFields, "AcctOwnerSSN", "ClientLast4SSN");
                            ReplaceField(model.MergeFields, "AcctOwnerAddress", "ClientAddress");
                            ReplaceField(model.MergeFields, "AcctOwnerCity", "ClientCity");
                            ReplaceField(model.MergeFields, "AcctOwnerState", "ClientState");
                            ReplaceField(model.MergeFields, "AcctOwnerZip", "ClientZip");
                            ReplaceField(model.MergeFields, "AcctOwnerMobile", "ClientMobilePhone");
                        }
                        else if (BankAcctOwner == "1")
                        {
                            if (IsCoClient == "1")
                            {
                                ReplaceField(model.MergeFields, "AcctOwnerName", "CoClientFullName");
                                ReplaceField(model.MergeFields, "AcctOwnerDOB", "CoClienDOB");
                                ReplaceField(model.MergeFields, "AcctOwnerSSN", "CoClientLast4SSN");
                                ReplaceField(model.MergeFields, "AcctOwnerAddress", "CoClientAddress");
                                ReplaceField(model.MergeFields, "AcctOwnerCity", "CoClientCity");
                                ReplaceField(model.MergeFields, "AcctOwnerState", "CoClientState");
                                ReplaceField(model.MergeFields, "AcctOwnerZip", "CoClientZip");
                                ReplaceField(model.MergeFields, "AcctOwnerMobile", "CoClientMobilePhone");

                            }
                            else
                            {
                                ReplaceFieldEmpty(model.MergeFields, "AcctOwnerName");
                                ReplaceFieldEmpty(model.MergeFields, "AcctOwnerDOB");
                                ReplaceFieldEmpty(model.MergeFields, "AcctOwnerSSN");
                                ReplaceFieldEmpty(model.MergeFields, "AcctOwnerAddress");
                                ReplaceFieldEmpty(model.MergeFields, "AcctOwnerCity");
                                ReplaceFieldEmpty(model.MergeFields, "AcctOwnerState");
                                ReplaceFieldEmpty(model.MergeFields, "AcctOwnerZip");
                                ReplaceFieldEmpty(model.MergeFields, "AcctOwnerMobile");

                               

                            }

                        }

                        if(IsCoClient != "1")
                        {
                            ReplaceFieldEmpty(model.MergeFields, "CoClientFullName");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientAddress");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientCity");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientState");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientZip");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientHomePhone");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientWorkPhone");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientMobilePhone");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientEmail");
                            ReplaceFieldEmpty(model.MergeFields, "CoClienDOB");
                            ReplaceFieldEmpty(model.MergeFields, "CoClientLast4SSN");
                        }
                    }
                }

            }


            return model;
        }
        private void ReplaceField(List<Structs.MergeField> model, string DesField, string SourField)
        {

            var item = model.Find(f => f.name == DesField);
            model.Remove(item);
            item.value = model.Find(m => m.name == SourField).value;
            model.Add(item);
        }
        private void ReplaceFieldEmpty(List<Structs.MergeField> model, string DesField)
        {
            var items = model.Where(f => f.name == DesField).ToArray();
            for(int i=0; i < items.Count(); i++)
            {
                var temp = items[i];
                model.Remove(items[i]);
                temp.value = string.Empty;
                model.Add(temp);
            }
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
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);

                return -1;
            }
        }
        public List<ScheduleOfPayment> GetScheduleOfPayment(int LeadISN, out string OutFirstPaymentDate, out string OutTerm)
        {
            OutFirstPaymentDate = string.Empty;
            OutTerm = string.Empty;
            var result = new List<ScheduleOfPayment>();
            var parameters = new Hashtable();
            parameters.Add("MemberISN", LeadISN);
            int rs = 0;
            var dsUserInfo = _data.ExecuteStoreProcedure("xp_debtuser_getinfo", parameters, out rs );
            if (!IsEmptyDataSet(dsUserInfo))
            {
                DataRow row = dsUserInfo.Tables[0].Rows[0];
                DataTable tb = dsUserInfo.Tables[1];
                var PremierCCDebtFree = ConvertObjectToDecimal(getValueByID("PLANDEBTAMOUNT", tb), 0) * ConvertObjectToDecimal(0.35, 0);
                var PremierPSLDebtFee = ConvertObjectToDecimal(getValueByID("PrivateStudentLoanAmount", tb), 0) * ConvertObjectToDecimal(0.25, 0);
                var TotalFee = PremierCCDebtFree + PremierPSLDebtFee;
                if (TotalFee < 3000) TotalFee = 3000;
                if (TotalFee > 19995) TotalFee = 19995;

                var monthlyServiceFee = 0;
                var programLength = string.IsNullOrEmpty(getValueByID("PureProgramLength", tb)) ? "18" : getValueByID("PureProgramLength", tb);
                OutTerm = programLength;
                if (Convert.ToInt32(programLength) < 12)
                    monthlyServiceFee = 49 * 11;
                else {
                    monthlyServiceFee = 49 * (Convert.ToInt32(programLength) - 1);
                }
                var GrandTotalFee = TotalFee + monthlyServiceFee + 99;
                var PaymentAmount = Math.Round(GrandTotalFee / Convert.ToInt32(programLength), 2);
                var DateCurr = DateTime.Now;
                var numMonth = 0;
                var FirstPaymentDate =ConvertToDate(getValueByID("PureFirstPaymentDate", tb));
                if (FirstPaymentDate == null) FirstPaymentDate = DateCurr.AddDays(1);
                var SecenPaymentDate = ConvertToDate(getValueByID("Pure2ndPaymentDate", tb));
                if (SecenPaymentDate == null) SecenPaymentDate = FirstPaymentDate.Value.AddMonths(1);

                for (var i = 0; i < Convert.ToInt32(programLength); i++)
                {

                    if (i + 1 == Convert.ToInt32(programLength))
                    {
                        PaymentAmount = GrandTotalFee;
                    }
                    else
                    {
                        GrandTotalFee = GrandTotalFee - PaymentAmount;
                    }
                    if (i == 0)
                    {

                        var payment = new ScheduleOfPayment()
                        {
                            LeadISN = LeadISN.ToString(),
                            BankFee = "$13.95",
                            PaymentDate = ConvertToDateString(FirstPaymentDate, "MM/dd/yyyy"),
                            PaymentAmount = "$" + PaymentAmount.ToString("#,##0.00"),
                            SavingsAmount = "$" + (PaymentAmount + ConvertObjectToDecimal(13.95,0)).ToString("#,##0.00")
                        };
                        result.Add(payment);
                    }
                    else if (i == 1)
                    {
                        var payment = new ScheduleOfPayment()
                        {
                            LeadISN = LeadISN.ToString(),
                            BankFee = "$13.95",
                            PaymentDate = ConvertToDateString(SecenPaymentDate, "MM/dd/yyyy"),
                            PaymentAmount = "$" + PaymentAmount.ToString("#,##0.00"),
                            SavingsAmount = "$" + (PaymentAmount + ConvertObjectToDecimal(13.95,0)).ToString("#,##0.00")
                        };
                        result.Add(payment);
                    }
                    else
                    {
                        numMonth++;
                        var payDate = SecenPaymentDate.Value.AddMonths(numMonth);
                        var payment = new ScheduleOfPayment()
                        {
                            LeadISN = LeadISN.ToString(),
                            BankFee = "$13.95",
                            PaymentDate = payDate.ToString("MM/dd/yyyy"),
                            PaymentAmount = "$" + PaymentAmount.ToString("#,##0.00"),
                            SavingsAmount = "$" + (PaymentAmount + ConvertObjectToDecimal(13.95,0)).ToString("#,##0.00")
                        };
                        result.Add(payment);
                    }
                    OutFirstPaymentDate = FirstPaymentDate.Value.ToString("MM/dd/yyyy");
                }
               
            }
          
            return result;
        }
        public class ScheduleOfPayment
        {
            public string LeadISN { get; set; }
            public string PaymentDate { get; set; }
            public string PaymentAmount { get; set; }
            public string BankFee { get; set; }
            public string SavingsAmount { get; set; }
        }
        public static string getValueByID(string id, DataTable tb)
        {
            string s = "";
            DataRow[] arrRow2 = tb.Select("attID = '" + id + "'");
            if (arrRow2.Length > 0)
            {
                s = arrRow2[0]["attValue"].ToString();
            }
            return s;
        }
        public static bool IsEmptyDataSet(DataSet ds)
        {
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) return false;
            return true;
        }
        public  DateTime? ConvertToDate(object date)
        {
            try
            {
                if (date == null) return null;
                return Convert.ToDateTime(date);
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public  string ConvertToDateString(object date, string format)
        {
            var dDate = ConvertToDate(date);
            if (dDate == null)
            {
                return string.Empty;
            }
            else
            {
                return dDate.Value.ToString(format);
            }
        }
        public decimal ConvertObjectToDecimal(object obj, decimal defaultValue)
        {
            if (obj == null || obj is DBNull)
                return 0;
            try
            {
                return decimal.Parse(obj.ToString());
            }
            catch { }
            return defaultValue;
        }
        public List<DebtCreditorContract> GetDebtCreditorContract(int LeadISN)
        {
            var result = new List<DebtCreditorContract>();
            var parameters = new Hashtable();
            parameters.Add("MemberISN", LeadISN);
            int rs = 0;
            var dsUserInfo = _data.ExecuteStoreProcedure("xp_debtuser_getinfo", parameters, out rs);
            var dsCreditor = _data.ExecuteQuery("select * from Vw_DebtExt_Creditor where MemberISN=@MemberISN and cdtIsPush='1'", parameters);


            if (dsCreditor.Rows.Count > 0)
            {
                foreach (DataRow row in dsCreditor.Rows)
                {
                    result.Add(new DebtCreditorContract()
                    {
                        Creditor = row["Creditor"].ToString(),
                        CollectorAccount = row["cdtAcctNo"].ToString(),
                        AccountBalance = "$" + ConvertObjectToDecimal(row["cdtBalance"].ToString(), 0).ToString("#,##0.00")
                    });
                }
            }
            return result;
        }
        public class DebtCreditorContract
        {
            public string Creditor { get; set; }
            public string CollectorAccount { get; set; }
            public string AccountBalance { get; set; }
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

            try
            {
                signId = (int)_data.ExecuteStoreProcedure("xp_rightsignature_document_insupd", parameters);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            return signId;
        }
    }
}