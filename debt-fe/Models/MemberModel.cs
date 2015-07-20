using System;

namespace debt_fe.Models
{
    public class MemberModel
    {
        public int ISN { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string SSN { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public int Status { get; set; }
        public DateTime? SignUp { get; set; }
        public int? AgentISN { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime DOB { get; set; }
        public int Gender { get; set; }
        public string AccountName { get; set; }
        public string AccountComment { get; set; }
        public int? BankISN { get; set; }
    }
}