using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debt_fe.Models.ViewModels
{
    public class AppointmentViewModel
    {
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
