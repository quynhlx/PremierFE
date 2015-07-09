using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace debt_fe.Models
{
	public class DocumentModel
	{
        public int ID { get; set; }
        public int MemberISN { get; set; }
        public string FileName { get; set; } // docFileName
        public string DocName { get; set; } // docName
        public string CreditorName { get; set; }
        public DateTime? AddedDate { get; set; }
        public double FileSize { get; set; }
        public bool Public { get; set; }
        public bool Status { get; set; }
        public string Desc { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public int CampaignISN { get; set; }
        public string AddedName { get; set; }
        public string UpdatedName { get; set; }
	}
}