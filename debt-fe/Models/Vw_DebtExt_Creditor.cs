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
    
    public partial class Vw_DebtExt_Creditor
    {
        public int CreditorISN { get; set; }
        public Nullable<int> MemberISN { get; set; }
        public string cdtName { get; set; }
        public string cdtAcctNo { get; set; }
        public Nullable<decimal> cdtBalance { get; set; }
        public Nullable<System.DateTime> updatedDate { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public string cdtType { get; set; }
        public string cdtWhose { get; set; }
        public Nullable<System.DateTime> addedDate { get; set; }
        public Nullable<int> addedBy { get; set; }
        public Nullable<byte> cdtIsPush { get; set; }
        public Nullable<byte> cdtSource { get; set; }
        public Nullable<int> PCreditorISN { get; set; }
        public string addedUserName { get; set; }
        public string updatedName { get; set; }
        public string Creditor { get; set; }
        public Nullable<int> CollectorISN { get; set; }
        public string cltAcctNo { get; set; }
        public string DebtStatus { get; set; }
        public string cltName { get; set; }
        public Nullable<System.DateTime> Graduation { get; set; }
        public int DebtRemoved { get; set; }
    }
}
