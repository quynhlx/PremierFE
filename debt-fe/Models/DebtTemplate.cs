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
    
    public partial class DebtTemplate
    {
        public int TemplateISN { get; set; }
        public Nullable<int> DealerISN { get; set; }
        public string tplName { get; set; }
        public string tplDesc { get; set; }
        public Nullable<byte> tplStatus { get; set; }
        public string tplFile { get; set; }
        public Nullable<byte> tplUsedEmail { get; set; }
        public Nullable<byte> tplUsedPrint { get; set; }
        public Nullable<byte> tplUsedFax { get; set; }
        public Nullable<System.DateTime> addedDate { get; set; }
        public Nullable<int> addedBy { get; set; }
        public Nullable<System.DateTime> updatedDate { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public Nullable<byte> tplRightSign { get; set; }
        public string tplRightSignGUID { get; set; }
        public string tplSubject { get; set; }
        public string tplMergeFields { get; set; }
        public string tplSignerRole { get; set; }
    
        public virtual MemberExt2 MemberExt2 { get; set; }
    }
}