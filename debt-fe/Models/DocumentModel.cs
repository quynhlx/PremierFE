using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using debt_fe.Models.ViewModels;

namespace debt_fe.Models
{
    public class DocumentModel : BaseViewModel
    {

        public int ID { get; set; }
        public int MemberISN { get; set; }
        public string FileName { get; set; } // docFileName
        public string DocName { get; set; } // docName
        public string CreditorName { get; set; }
        public int? CreditorISN { get; set; }
        public DateTime? AddedDate { get; set; }
        public double FileSize { get; set; }
        public bool Public { get; set; }
        public bool Status { get; set; }
        public string Desc { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public int CampaignISN { get; set; }
        public string AddedName { get; set; }
        public int? AddedBy { get; set; }
        public string UpdatedName { get; set; }
        public string LastAction { get; set; }
        public int SignatureStatus { get; set; }
        public bool IsSignatureDocument
        {
            get
            {
                if (SignatureStatus == 1)
                    return true;
                else
                    return false;
            }
           
        }
        public string SendIP { set; get; }
        public string SignatureIP { set; get; }
        public DateTime? docSignatureDate { set; get; }
        public int? docNoOfSign { set; get; }
        public string docHistory { set; get; }
        public string DocGUID { set; get; }
        public bool IsSubSingature { set; get; }
        public int? GroupId { set; get; }
        public bool OneSigntureCompleted { set; get; }
        public bool SigntureCompleted { set; get; }
        //
        private string _customFileName;
        public string CustomFileName
        {
            get { return _customFileName; }
            set { _customFileName = value; }
        }
    }

}