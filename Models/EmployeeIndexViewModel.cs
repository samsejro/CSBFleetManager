using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSBFleetManager.Models
{
    public class EmployeeIndexViewModel
    {
        public string LASRRAID { get; set; }
        public string EmployeeNo { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string ImageUrl { get; set; }
        public string MDA { get; set; }
        public string MDAId { get; set; }
        public string Designation { get; set; }
        public DateTime DateofFirstAppointment { get; set; }
        public string LGA { get; set; }
        public string EmploymentTypeName { get; set; }
        public string Ministry { get; set; }

    }
}
