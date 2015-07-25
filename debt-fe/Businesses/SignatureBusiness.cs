using debt_fe.DataAccessHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using debt_fe.Models;


namespace debt_fe.Businesses
{
    public class SignatureBusiness
    {
        private DataProvider _data;

        public SignatureBusiness()
        {
            _data = new DataProvider();
        }

        public List<SignatureModel> GetSignatures()
        {
            var query = "select * from RightSignature_Document order by createddate desc";
            DataTable table = null;
            
            try
            {
                table = _data.ExecuteQuery(query);
            }
            catch(Exception)
            {
                return null;
            }

            var list = GetSignatureFromTable(table);

            return list;
        }

        public List<SignatureModel> GetSignatures(int entityId)
        {
            var list = GetSignatures();

            var listWithEntity = list.Where(s => s.EntityId != null && s.EntityId.Value == entityId).ToList();

            return listWithEntity;
        }

        private List<SignatureModel> GetSignatureFromTable(DataTable table)
        {
            if (table==null || table.Rows.Count==0)
            {
                return null;
            }

            var list = new List<SignatureModel>();

            foreach (DataRow row in table.Rows)
            {
                var model = GetSignatureModelFromRow(row);

                if (model != null)
                {
                    list.Add(model);
                }
            }

            return list;
        }

        private SignatureModel GetSignatureModelFromRow(DataRow row)
        {
            if (row == null || row.Table.Columns.Count==0)
            {
                return null;
            }

            var model = new SignatureModel();

            model.Id = int.Parse(row["RightSignature_Document_ISN"].ToString());
            model.DocumentName = row["sign_document_name"].ToString();
            model.DocumentGuid = row["sign_document_guid"].ToString();
            
            //
            // get entity id
            var entityIdStr = row["entityISN"].ToString();
            if (!string.IsNullOrEmpty(entityIdStr))
            {
                int entityId = 0;
                if (int.TryParse(entityIdStr,out entityId))
                {
                    model.EntityId = entityId;
                }
            }

            //
            // get sign status
            model.SignStatus = row["sign_status"].ToString();
            if (!string.IsNullOrEmpty(model.SignStatus))
            {
                if (model.SignStatus.ToLower().Trim().Equals("signed"))
                {
                    model.IsSigned = true;
                }
            }

            //
            // get document id
            var documentIdStr = row["DocumentISN"].ToString();
            if (!string.IsNullOrEmpty(documentIdStr))
            {
                int documentId = 0;
                if (int.TryParse(documentIdStr,out documentId))
                {
                    model.DocumentId = documentId;
                }
            }

            model.CreatedDate = DateTime.Parse(row["createddate"].ToString());

            return model;
        }
    }
}