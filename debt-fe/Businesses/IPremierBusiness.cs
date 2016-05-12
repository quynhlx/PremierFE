using debt_fe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace debt_fe.Businesses
{
    public interface IPremierBusiness
    {
        /// <summary>
        /// Lấy danh sách những documents theo memberisn
        /// </summary>
        /// <param name="memberISN">Người dùng đang login</param>
        /// <returns>List<DocumentModel></returns>
        List<DocumentModel> GetDocuments(int memberISN);

        //Actions
        /// <summary>
        /// Cập nhập lại document Signature, và thêm ghi chú
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="appendNote"></param>
        /// <returns></returns>
        int UpdateDocSignature(DocumentModel doc, string appendNote = "");

        /// <summary>
        /// Sửa thông tin document loại Signature
        /// </summary>
        /// <param name="doc">DocumentModel</param>
        /// <returns></returns>
        int EditSignatureDocument(DocumentModel doc);
        int EditDocument(DocumentModel doc);


        int UpdateLeadStatus(int MemberISN, string Status, int UpdatedBy);
        int UpdateHistory(int DocId, string Note);
        string GetDocumentPath(int DocId, DateTime? AddedDate);
    }
}