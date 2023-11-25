using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CSBFleetManager.Models
{
    public class ManageProfileModel
    {
        //[Phone]
        //[Display(Name = "Phone number")]
        //public string PhoneNumber { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Surname")]
        public string LastName { get; set; }
        [Display(Name = "Username")]
        public string Username { get; set; }
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Profile Picture")]
        public byte[] ProfilePicture { get; set; }

        [TempData]
        public string StatusMessage { get; set; }
        [TempData]
        public string UserNameChangeLimitMessage { get; set; }
    }
}
