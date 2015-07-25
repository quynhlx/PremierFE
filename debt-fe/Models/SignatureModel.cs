using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace debt_fe.Models
{
    public class SignatureModel
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public string DocumentGuid { get; set; }
        public int? EntityId { get; set; }
        public string SignStatus { get; set; }
        public bool IsSigned { get; set; }
        public int? DocumentId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}