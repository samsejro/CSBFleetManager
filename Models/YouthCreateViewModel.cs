using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSBFleetManager.Entity;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace CSBFleetManager.Models
{
    public class YouthCreateViewModel
    {
        [Required(ErrorMessage = "LASRRAID is required")]
        public string LASRRAID { get; set; }
        public string LAGID { get; set; }
        public string EmployeeNo { get; set; }
       
        public EmployeeType EmploymentTypeName { get; set; }

        [Required(ErrorMessage = "Employment Type is required"), Display(Name = "Employment Type")]
       
        public string EmployeeTypeId { get; set; }

        public MDA MDA { get; set; }

        [Required(ErrorMessage = "MDA is required"), Display(Name = "MDA")]
        public string MDAId { get; set; }

        [Required(ErrorMessage = "First Name is required"), StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[A-Z][a-zA-Z""'\s-]*$"), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(50), Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "SurName is required"), StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[A-Z][a-zA-Z""'\s-]*$"), Display(Name = "Surname")]
        public string LastName { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + (string.IsNullOrEmpty(MiddleName) ? " " : (" " + (char?)MiddleName[0] + ". ").ToUpper()) + LastName;
            }
        }
        public Image EmployeePhoto { get; set; }
        public string Photo { get; set; }
        public string Gender { get; set; }
        [Display(Name = "Photo")]
        //public IFormFile ImageUrl { get; set; }
        public string ImageUrl { get; set; }
        [DataType(DataType.Date), Display(Name = "Date Of Birth")]
        public DateTime DOB { get; set; }

        [DataType(DataType.Date), Display(Name = "Date of First Appointment")]
        public DateTime DateofFirstAppointment { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date), Display(Name = "Date of Present Appointment")]
        public DateTime DateofPresentAppointment { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date), Display(Name = "Date of Exit from Service")]
        public DateTime DueDate { get; set; } = DateTime.UtcNow;
        public string Phone { get; set; }
        [Required(ErrorMessage = "Designation is required")]
        public string Designation { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "Next of Kin First is required"), Display(Name = "First Name")]
        public string NextofKinFirstName { get; set; }

        [Required(ErrorMessage = "Next of Kin Surname is required"), Display(Name = "Surname")]
        public string NextofKinLastName { get; set; }

        [Required(ErrorMessage = "Next of Kin Contact Phone is required"), Display(Name = "Phone")]
        public string NextofKinPhone { get; set; }

        [Required(ErrorMessage = "Next of Kin relationship type is required"), Display(Name = "Relationship")]
        public Relationship NextOfkinrelationship { get; set; }

        public string LGA { get; set; }

        public string UploadedBy { get; set; }


    }
}
