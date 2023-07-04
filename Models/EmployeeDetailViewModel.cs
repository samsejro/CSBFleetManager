using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSBFleetManager.Entity;
using System.Drawing;

namespace CSBFleetManager.Models
{
    public class EmployeeDetailViewModel
    {
        public string LASRRAID { get; set; }
        public string EmployeeNo { get; set; }
        public string ContactPhone { get; set; }
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public string ImageUrl { get; set; }
        public DateTime DOB { get; set; }
        public string Designation { get; set; }
        public DateTime DateofFirstAppointment { get; set; }
        public string LGA { get; set; }
        public string EmploymentTypeName { get; set; }
        public string Ministry { get; set; }
        public string Address { get; set; }

        public string NextofkinFullName { get; set; }
        public string NextofKinPhone { get; set; }

        public Relationship NextOfkinrelationship { get; set; }

        public string Email { get; set; }


    }
}
