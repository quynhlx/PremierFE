//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace debt_fe.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Vw_TroubleTicket
    {
        public int TroubleTicketISN { get; set; }
        public Nullable<int> MemberISN { get; set; }
        public Nullable<int> CallDetailISN { get; set; }
        public string CallID { get; set; }
        public string PinNumber { get; set; }
        public string Destination { get; set; }
        public Nullable<System.DateTime> tblAddedDate { get; set; }
        public Nullable<int> tblAddedBy { get; set; }
        public string tblSubject { get; set; }
        public string tblDesc { get; set; }
        public Nullable<short> tblStatus { get; set; }
        public string tblResult { get; set; }
        public string tblAttachFile { get; set; }
        public Nullable<System.DateTime> updatedDate { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public Nullable<System.DateTime> tblDate { get; set; }
        public string tblCode { get; set; }
        public Nullable<int> LeadISN { get; set; }
        public string tblLeadPhone { get; set; }
        public string tblAgentPhone { get; set; }
        public string tblAgentDTMF { get; set; }
        public string memUserName { get; set; }
        public string memFullName { get; set; }
        public string memFullName2 { get; set; }
        public string updatedName { get; set; }
        public string tblAddedName { get; set; }
        public string memFax { get; set; }
        public string saleman { get; set; }
        public Nullable<int> CampaignISN { get; set; }
        public string cpnName { get; set; }
        public string cpnDID { get; set; }
        public Nullable<int> DealerISN { get; set; }
        public string Dealer { get; set; }
        public string tblDescExt { get; set; }
    }
}
