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
    public class BaseViewModel
    {
        //int TotalReg = 0;
        //int TotalRegMale = 0;
        //int TotalRegeFemale = 0;
        //int TotalRegToday = 0;
        public int TotalReg { get; set; }

        public int TotalRegMale { get; set; }
        public int TotalRegeFemale { get; set; }
        public int TotalRegToday { get; set; }
    }
}
