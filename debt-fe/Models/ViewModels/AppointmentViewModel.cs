using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debt_fe.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public int ISN { set; get; }
        public int No { set; get; }
        public string Datetime { set; get; }
        public AppointmentStatus Status { set; get; }
        public AppointmentAction Action { set; get; }
        public string TypeStr
        {
            get
            {
                return Type.Trim(new char[] { '@' });
            }
        }
        public string Type { set; get; }
        public string ActionStr
        {
            get
            {
                switch (Action)
                {
                    case AppointmentAction.All :
                        return "All";
                    case AppointmentAction.CallCompleted:
                        return "Call Completed";
                    case AppointmentAction.CallNotCompleted:
                        return "Call Not Completed";
                    case AppointmentAction.RescheduleMissed:
                        return "Reschedule/Missed";
                    default:
                        return ""; 
                }
            }
        }
        public bool HasAttachFile { set; get; }
        public string CssActtachFile { get {  if ( !HasAttachFile) return "hidden"; else return string.Empty; }}
        public int DownloadFile { get; set; }
        public string With { set; get; }
        public string Description { set; get; }
    }
    public enum AppointmentStatus
    {
        All,
        Pending,
        Completed,
        Reschedule,
        Incompleted
    }
    public enum AppointmentAction
    {
        All,
        NA,
        CallCompleted,
        RescheduleMissed,
        CallNotCompleted
    }
}
