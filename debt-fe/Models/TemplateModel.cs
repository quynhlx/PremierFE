using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RightSignatures;

namespace debt_fe.Models
{
    public class TemplateModel
    {
        public TemplateModel()
        {
            List<Structs.MergeField> MergeFields = new List<Structs.MergeField>();
        }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDesc { get; set; }
        public bool TemplateStatus { get; set; }
        public string TemplateFile { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public bool CanSign { get; set; }
        public string SignGuid { get; set; }
        public string TemplateSubject { get; set; }
        // public string MergeFields { get; set; }
        public List<Structs.MergeField> MergeFields { get; set; }
        public string SignerRole { get; set; }
        public string AddedName { get; set; }
        public string UpdatedName { get; set; }
    }
}