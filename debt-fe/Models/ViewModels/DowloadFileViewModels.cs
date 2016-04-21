using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debt_fe.Models.ViewModels
{
    public class DocumentDownloadViewModel
    {
        public int Id { set; get; }
        public string FileName { set; get; }
        public DateTime UploadedDate { set; get; }
        public DateTime AddedDate { set; get; }
        public string Path { set; get; }
        public string UpdatedBy { set; get; }
    }
}
