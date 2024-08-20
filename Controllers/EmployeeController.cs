using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using CSBFleetManager.Entity;
using CSBFleetManager.Models;
using CSBFleetManager.Services;
using CSBFleetManager.Services.Implementation;
using System.IO;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using System.Drawing;
//using Microsoft.AspNetCore.Mvc;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Http;
using CSBFleetManager.Persistence;
using Microsoft.AspNetCore.Identity;
using CSBFleetManager.Entity.ViewModels;

namespace CSBFleetManager.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMDAService _mdaService;
        private readonly IEmployeeTypeService _employeeTypeService;
        private readonly IValidationService _ivalidationService;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IGetDetailsOnLASRRAIdService _getDetailsOnLASRRAIdService;
        private readonly IGetDetailsOnOracleNumberService _getDetailsOnOracleNumberService;
        private readonly IRegistrationStatistics _getRegistrationStatistics;
        private readonly IPrintReasonService _printReasonServices;
        private readonly ICardRequestService _cardRequestService;


        //Image SignatureImage = null;
        Image ResidentfPicture = null;
        byte[] pictureByteArray = null;
        byte[] signatureByteArray = null;
        private readonly ApplicationDbContext _context;
        public EmployeeController(IEmployeeService employeeService, IEmployeeTypeService employeeTypeService,
            IMDAService mDAService, IValidationService validationService, UserManager<ApplicationUser> userManager,
            IGetDetailsOnLASRRAIdService getDetailsOnLASRRAIdService,IGetDetailsOnOracleNumberService getDetailsOnOracleNumberService,
            IRegistrationStatistics getRegistrationStatistics,IPrintReasonService printReasonServices, ICardRequestService cardRequestService, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context)
        {
            _employeeService = employeeService;
            _mdaService = mDAService;
            _employeeTypeService = employeeTypeService;
            _hostingEnvironment = hostingEnvironment;
            _ivalidationService = validationService;
            _userManager = userManager;
			_getDetailsOnLASRRAIdService = getDetailsOnLASRRAIdService;
            _getDetailsOnOracleNumberService = getDetailsOnOracleNumberService;
            _getRegistrationStatistics = getRegistrationStatistics;
            _printReasonServices = printReasonServices;
            _cardRequestService = cardRequestService;
            context = _context;
            
        }
        public static Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }
        private byte[] ConvertImageToByteArray(System.Drawing.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                return ms.ToArray();
            }
        }
        public Image ConvertByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
        }

        //[Authorize(Roles = "SuperAdmin")]
        //[Authorize(Roles = "Admin")]
        //[Authorize]
        public ActionResult Index(int pageNumber=0)
        {
            ////int TotalReg = 0;
            ////int TotalRegMale = 0;
            ////int TotalRegeFemale = 0;
            ////int TotalRegToday = 0;

            ViewBag.MDAList = _mdaService.GetDistinctMDA();

            ViewBag.TotalReg = _getRegistrationStatistics.GetTotalRegistration();

            ViewBag.TotalRegMale = _getRegistrationStatistics.GetTotalRegistrationByGender("Male");

            ViewBag.TotalRegeFemale = _getRegistrationStatistics.GetTotalRegistrationByGender("Female");

            ViewBag.TotalRegToday = _getRegistrationStatistics.GetTotaRegistrationToday();


            ViewBag.pageNumber = pageNumber;
            //ViewBag.mdaID = mdaID;


            //var employees = _employeeService.GetAllForIndexView().Select(employee => new EmployeeIndexViewModel
            //{
            //    LASRRAID = employee.LASRRAID,
            //    FullName = employee.FullName,
            //    EmployeeNo = employee.EmployeeNo,
            //    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
            //    EmploymentTypeName = employee.EmployeeTypeId,
            //    //EmploymentTypeName = empType.,
            //    //LAGID=employee.LAGID,
            //    Gender = employee.Gender,
            //    // Designation = employee.Designation,
            //    Ministry = employee.MDAId,
            //    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
            //    ImageUrl = employee.ImageUrl,
            //    LGA = employee.LGA

            //}).ToList();

            //int pageSize = 4;
            //return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            return View();
        }
        public IActionResult LoadIndexEmployeeList(int pageNumber)
        {
            ViewBag.MDAList = _mdaService.GetDistinctMDA();

            ViewBag.TotalReg = _getRegistrationStatistics.GetTotalRegistration();

            ViewBag.TotalRegMale = _getRegistrationStatistics.GetTotalRegistrationByGender("Male");

            ViewBag.TotalRegeFemale = _getRegistrationStatistics.GetTotalRegistrationByGender("Female");

            ViewBag.TotalRegToday = _getRegistrationStatistics.GetTotaRegistrationToday();


            var employees = _employeeService.GetAllForIndexView().Select(employee => new EmployeeIndexViewModel
            {
                LASRRAID = employee.LASRRAID,
                FullName = employee.FullName,
                EmployeeNo = employee.EmployeeNo,
                //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                EmploymentTypeName = employee.EmployeeTypeId,
                //EmploymentTypeName = empType.,
                //LAGID=employee.LAGID,
                Gender = employee.Gender,
                // Designation = employee.Designation,
                Ministry = employee.MDAId,
                // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                ImageUrl = employee.ImageUrl,
                LGA = employee.LGA

            }).ToList();

            int pageSize = 4;
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            return PartialView("_EmployeeListByMDA.cshtml", EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber , pageSize));
            //return View();

        }

        //[Authorize]
        public IActionResult MDAIndex(int? pageNumber, string mdaID)
        {
            //int TotalReg = 0;
            //int TotalRegMale = 0;
            //int TotalRegeFemale = 0;
            //int TotalRegToday = 0;

            string id = mdaID;

            ViewBag.TotalReg = _getRegistrationStatistics.GetTotalRegistrationByMDA(mdaID);

            ViewBag.TotalRegMale = _getRegistrationStatistics.GetTotaRegistrationByGenderMDA("Male", mdaID);

            ViewBag.TotalRegeFemale = _getRegistrationStatistics.GetTotaRegistrationByGenderMDA("Female",mdaID);

            ViewBag.TotalRegToday = _getRegistrationStatistics.GetTotaRegistrationTodayByMDA(mdaID);


            var employees = _employeeService.GetAllForMDAIndexView(mdaID).Select(employee => new EmployeeIndexViewModel
            {
                LASRRAID = employee.LASRRAID,
                FullName = employee.FullName,
                EmployeeNo = employee.EmployeeNo,
                //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                EmploymentTypeName = employee.EmployeeTypeId,
                //EmploymentTypeName = empType.,
                //LAGID=employee.LAGID,
                Gender = employee.Gender,
                // Designation = employee.Designation,
                Ministry = employee.MDAId,
                // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                ImageUrl = employee.ImageUrl,
                LGA = employee.LGA

            }).ToList();

            int pageSize = 4;
            return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            //return View();
        }

        public ActionResult MDAIndexJQueryDataTable(string mdaID)
        {
            //int TotalReg = 0;
            //int TotalRegMale = 0;
            //int TotalRegeFemale = 0;
            //int TotalRegToday = 0;

            string id = mdaID;

            ViewBag.TotalReg = _getRegistrationStatistics.GetTotalRegistrationByMDA(mdaID);

            ViewBag.TotalRegMale = _getRegistrationStatistics.GetTotaRegistrationByGenderMDA("Male", mdaID);

            ViewBag.TotalRegeFemale = _getRegistrationStatistics.GetTotaRegistrationByGenderMDA("Female", mdaID);

            ViewBag.TotalRegToday = _getRegistrationStatistics.GetTotaRegistrationTodayByMDA(mdaID);


            var employees = _employeeService.GetAllForMDAIndexView(mdaID).Select(employee => new EmployeeIndexViewModel
            {
                LASRRAID = employee.LASRRAID,
                FullName = employee.FullName,
                EmployeeNo = employee.EmployeeNo,
                //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                EmploymentTypeName = employee.EmployeeTypeId,
                //EmploymentTypeName = empType.,
                //LAGID=employee.LAGID,
                Gender = employee.Gender,
                // Designation = employee.Designation,
                Ministry = employee.MDAId,
                // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                ImageUrl = employee.ImageUrl,
                LGA = employee.LGA

            }).ToList();

            int pageSize = 8;
            //return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            //pageNumber = pageNumber == 0 ? 1 : pageNumber;
            //ViewBag.pageNumber = pageNumber;
            ViewBag.mdaID = mdaID;
            //return PartialView("_MDAEmployeeList", EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber, pageSize));
            //return PartialView("_MDAEmployeeListJqueryDataTable", employees);
            return View(employees);
        }
        public ActionResult MDAIndexJQueryCardRequestDataTable(string mdaID)
        {
            //int TotalReg = 0;
            //int TotalRegMale = 0;
            //int TotalRegeFemale = 0;
            //int TotalRegToday = 0;

            string id = mdaID;

            ViewBag.TotalReg = _getRegistrationStatistics.GetTotalRegistrationByMDA(mdaID);

            ViewBag.TotalRegMale = _getRegistrationStatistics.GetTotaRegistrationByGenderMDA("Male", mdaID);

            ViewBag.TotalRegeFemale = _getRegistrationStatistics.GetTotaRegistrationByGenderMDA("Female", mdaID);

            ViewBag.TotalRegToday = _getRegistrationStatistics.GetTotaRegistrationTodayByMDA(mdaID);

            ViewBag.PrintReasons = _printReasonServices.GetAllforEmployee();

            var employees = _employeeService.GetAllForMDAIndexView(mdaID).Select(employee => new EmployeeCardRequestIndexViewModel
            {
                LASRRAID = employee.LASRRAID,
                FullName = employee.FullName,
                EmployeeNo = employee.EmployeeNo,
                //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                EmploymentTypeName = employee.EmployeeTypeId,
                //EmploymentTypeName = empType.,
                //LAGID=employee.LAGID,
                Gender = employee.Gender,
                // Designation = employee.Designation,
                Ministry = employee.MDAId,
                // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                ImageUrl = employee.ImageUrl,
                LGA = employee.LGA

            }).ToList();

            int pageSize = 8;
            //return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            //pageNumber = pageNumber == 0 ? 1 : pageNumber;
            //ViewBag.pageNumber = pageNumber;
            ViewBag.mdaID = mdaID;
            //return PartialView("_MDAEmployeeList", EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber, pageSize));
            //return PartialView("_MDAEmployeeListJqueryDataTable", employees);
            return View(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //Prevents cross-site Request Forgery Attacks
        public async Task<ActionResult> MDAIndexJQueryCardRequestDataTable([FromBody] List<EmployeeCardRequestSubmissionVM> validSelections)
        {
            var user = await _userManager.GetUserAsync(User);
            if (validSelections != null && validSelections.Count > 0)
            {
                //return Json(new { success = false, message = "No employees selected." });
                foreach (var cardRequest in validSelections)
                {
                    var request = new CardRequest()
                    {
                        LASRRAID = cardRequest.LASRRAID,
                        LAGID = cardRequest.LAGID,
                        EmployeeNo = cardRequest.EmployeeNo,
                        PrintReasonId = cardRequest.CardRequestType,
                        RequestDate = DateTime.Now.Date,
                        UserName = user.UserName,
                        MDAId = cardRequest.MDAId,
                        CurrentStatus = "Request"



                    };
                    await _cardRequestService.CreateAsync(request);
                    return Ok(new { success = true, message = "Data inserted successfully." });
                }
            }
            return BadRequest(new { success = false, message = "No valid selection found." });
          
        }

            [HttpGet]
        public IActionResult FilterByMDA(int pageNumber, string mdaID)
        {
            var employees =new List<EmployeeIndexViewModel>();

            if (!string.IsNullOrEmpty(mdaID))
            {
                 employees = _employeeService.GetAllForMDAIndexView(mdaID).Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();
            }
            else
            {
                 employees = _employeeService.GetAllForIndexView().Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();

            }

           // _testpartial.cshtml

            int pageSize = 8;
            //return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            return PartialView("_EmployeeListByMDA", EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber , pageSize));
            //return View();
            
            //return PartialView("@~/Views/Employee/_testpartial.cshtml");
        }
        
        
        public PartialViewResult MDAEmployeeList(int pageNumber, string mdaID)
        {
            var employees = new List<EmployeeIndexViewModel>();
            if (!string.IsNullOrEmpty(mdaID))
            {
                employees = _employeeService.GetAllForMDAIndexView(mdaID).Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();
            }
            else
            {
                employees = _employeeService.GetAllForIndexView().Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();

            }

            // _testpartial.cshtml

            int pageSize = 8;
            //return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            ViewBag.pageNumber = pageNumber;
            ViewBag.mdaID = mdaID;
            return PartialView("_MDAEmployeeList", EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber, pageSize));

        }
        public PartialViewResult MDAEmployeeListDataTable(string mdaID)
        {
            var employees = new List<EmployeeIndexViewModel>();
            if (!string.IsNullOrEmpty(mdaID))
            {
                employees = _employeeService.GetAllForMDAIndexView(mdaID).Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();
            }
            else
            {
                employees = _employeeService.GetAllForIndexView().Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();

            }

            // _testpartial.cshtml

            int pageSize = 8;
            //return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            //pageNumber = pageNumber == 0 ? 1 : pageNumber;
            //ViewBag.pageNumber = pageNumber;
            ViewBag.mdaID = mdaID;
            //return PartialView("_MDAEmployeeList", EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber, pageSize));
            return PartialView("_MDAEmployeeListJqueryDataTable",employees);



        }
        public PartialViewResult MDAEmployeeListDataTableUser(string mdaID)
        {
            var employees = new List<EmployeeIndexViewModel>();
            if (!string.IsNullOrEmpty(mdaID))
            {
                employees = _employeeService.GetAllForMDAIndexView(mdaID).Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();
            }
            else
            {
                employees = _employeeService.GetAllForIndexView().Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();

            }

            // _testpartial.cshtml

            int pageSize = 8;
            //return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            //pageNumber = pageNumber == 0 ? 1 : pageNumber;
            //ViewBag.pageNumber = pageNumber;
            ViewBag.mdaID = mdaID;
            //return PartialView("_MDAEmployeeList", EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber, pageSize));
            return PartialView("_MDAEmployeeListJqueryDataTableUser", employees);



        }

        public PartialViewResult MDAIndexJQueryDataTableCardRequest(string mdaID)
        {
            var employees = new List<EmployeeIndexViewModel>();
            if (!string.IsNullOrEmpty(mdaID))
            {
                employees = _employeeService.GetAllForMDAIndexView(mdaID).Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();
            }
            else
            {
                employees = _employeeService.GetAllForIndexView().Select(employee => new EmployeeIndexViewModel
                {
                    LASRRAID = employee.LASRRAID,
                    FullName = employee.FullName,
                    EmployeeNo = employee.EmployeeNo,
                    //EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
                    EmploymentTypeName = employee.EmployeeTypeId,
                    //EmploymentTypeName = empType.,
                    //LAGID=employee.LAGID,
                    Gender = employee.Gender,
                    // Designation = employee.Designation,
                    Ministry = employee.MDAId,
                    // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                    ImageUrl = employee.ImageUrl,
                    LGA = employee.LGA

                }).ToList();

            }

            // _testpartial.cshtml

            int pageSize = 8;
            //return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            //pageNumber = pageNumber == 0 ? 1 : pageNumber;
            //ViewBag.pageNumber = pageNumber;
            ViewBag.mdaID = mdaID;
            //return PartialView("_MDAEmployeeList", EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber, pageSize));
            return PartialView("_CardRequest", employees);



        }

        [HttpGet]
        public IActionResult Create()
        {
            //List<MDA> mdaList = _mdaService.GetAll().ToList();
            //List<EmployeeType> EmpTypeList = _employeeTypeService.GetAll().ToList();

            //ViewBag.MDA = new SelectList(mdaList, "MDAId", "MDAname");
            //ViewBag.EmployeeType = new SelectList(EmpTypeList, "EmployeeTypeId", "EmployeeTypeName");

            ViewBag.MDA = _mdaService.GetAllMDAforEmployee();
            ViewBag.EmployeeType = _employeeTypeService.GetAllEmploymentTypeforEmployee();

            var model = new EmployeeCreateViewModel();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] //Prevents cross-site Request Forgery Attacks
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {
            //string val = "I am here";
            var user = await _userManager.GetUserAsync(User);

            Employee employeeExist = _employeeService.GetByLASRRAId(model.LASRRAID);

            if (employeeExist != null)
            {
                ModelState.AddModelError("", "Duplicate entry for an employee not allowed. Emplyee already exist");
                ViewBag.AlreadyExistingEmployee = "Duplicate entry for an employee not allowed. Emplyee already exist";
                return View();
            }

            if (ModelState.IsValid)
            {
                var employee = new Employee()
                {
                    LASRRAID = model.LASRRAID,
                    LAGID = model.LAGID,
                    EmployeeTypeId = model.EmployeeTypeId,
                    //EmployeeNo = model.EmployeeNo,
                    //EmploymentType = model.EmploymentTypeName,
                    MDAId = model.MDAId,
                    Designation = model.Designation,
                    LastName = model.LastName.ToUpper(),
                    //FirstName = model.FirstName.ToUpper(),
                    FirstName = (string.IsNullOrEmpty(model.FirstName) ? " " : char.ToUpper(model.FirstName[0]) + model.FirstName.Substring(1)),
                    // MiddleName = model.MiddleName,
                    MiddleName = (string.IsNullOrEmpty(model.MiddleName) ? " " : char.ToUpper(model.MiddleName[0]) + model.MiddleName.Substring(1)),
                    FullName = model.FullName,
                    EmpSurname=model.EmpSurname,
                    EmpFirstname=model.EmpFirstname,
                    EmpMiddleName=model.EmpMiddleName,
                    DOB = model.DOB,
                    Gender = model.Gender,
                    Phone = model.Phone,
                    Address = model.Address,
                    LGA = model.LGA,
                    NextofKinFirstName = (string.IsNullOrEmpty(model.NextofKinFirstName) ? " " : char.ToUpper(model.NextofKinFirstName[0]) + model.NextofKinFirstName.Substring(1)),
                    NextofKinLastName = model.NextofKinLastName.ToUpper(),
                    NextOfkinrelationship = model.NextOfkinrelationship,
                    NextofKinPhone = model.NextofKinPhone,
                    DateofFirstAppointment = model.DateofFirstAppointment,
                    Email = model.Email,
                    DateUploaded = DateTime.Now.Date,
                    UploadedBy = user.UserName


                };
                if (!string.IsNullOrEmpty(model.EmployeeNo))
                {
                    employee.EmployeeNo = model.EmployeeNo;
                }
                else
                {
                    employee.EmployeeNo = model.LASRRAID;
                }

                //get picture details from model
                if (model.Photo != null && model.Photo.Length > 0)
                //if (!string.IsNullOrEmpty(form["imgCapture"].ToString()))
                {
                    var uploadDir = @"images/CSBUsers";
                    //var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    string fileName = model.LASRRAID;
                    //var extension = Path.GetExtension(model.ImageUrl.FileName);
                    string extension = ".jpg";
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);

                    //string data = form["imgCapture"].ToString();
                    string data = model.Photo.ToString();

                    //pictureByteArray = Convert.FromBase64String(data.Split(',')[1]);
                    pictureByteArray = Convert.FromBase64String(data);
                    // ResidentfPicture = ConvertByteArrayToImage(pictureByteArray);

                    int offset = 0;
                    const int Buffer_Size = 4096;

                    using var fileStream = new FileStream(path,
                                                            FileMode.Append, FileAccess.Write,
                                                            FileShare.None, bufferSize: Buffer_Size, useAsync: true);
                    await fileStream.WriteAsync(pictureByteArray, offset, pictureByteArray.Length);

                    // await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.ImageUrl = "/" + uploadDir + "/" + fileName;
                }

                //get signature details from model
                if (model.Signature != null && model.Signature.Length > 0)
                //if (!string.IsNullOrEmpty(form["imgCapture"].ToString()))
                {
                    var uploadDir = @"images/Signatures";
                    //var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    string fileName = model.LASRRAID;
                    //var extension = Path.GetExtension(model.ImageUrl.FileName);
                    string extension = "_26.jpg";
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);

                    //string data = form["imgCapture"].ToString();
                    string Sigdata = model.Signature.ToString();

                    //pictureByteArray = Convert.FromBase64String(data.Split(',')[1]);
                    signatureByteArray = Convert.FromBase64String(Sigdata);
                    // ResidentfPicture = ConvertByteArrayToImage(pictureByteArray);

                    int offset = 0;
                    const int Buffer_Size = 4096;

                    using var fileStream = new FileStream(path,
                                                            FileMode.Append, FileAccess.Write,
                                                            FileShare.None, bufferSize: Buffer_Size, useAsync: true);
                    await fileStream.WriteAsync(signatureByteArray, offset, signatureByteArray.Length);

                    // await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.SignatureImageUrl = "/" + uploadDir + "/" + fileName;
                }
                else
                {
                    ModelState.AddModelError("", "You have provided Invalid Data");
                }

                await _employeeService.CreateAsync(employee);
                return RedirectToAction(nameof(Index));

            }
            ViewBag.MDA = _mdaService.GetAllMDAforEmployee();
            ViewBag.EmployeeType = _employeeTypeService.GetAllEmploymentTypeforEmployee();


            //string last = "I got here";

            return View();
        }

        [HttpGet]
        public IActionResult CreatMDAEmployee(string MDAId)
        {
            ViewBag.MDA = _mdaService.GetSpecificMDAforEmployee(MDAId);
            ViewBag.EmployeeType = _employeeTypeService.GetAllEmploymentTypeforEmployee();

            var model = new EmployeeCreateViewModel();
            return View(model);
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //Prevents cross-site Request Forgery Attacks
        public async Task<IActionResult> CreatMDAEmployee(EmployeeCreateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            string val = "I am here";

            Employee employeeExist = _employeeService.GetByLASRRAId(model.LASRRAID);

			if (employeeExist != null)
			{
                ModelState.AddModelError("", "Duplicate entry for an employee not allowed. Emplyee already exist");
                return View();
			}

            if (ModelState.IsValid)
            {

                var employee = new Employee()
                {
                    LASRRAID = model.LASRRAID,
                    LAGID = model.LAGID,
                    EmployeeTypeId = model.EmployeeTypeId,
                    //EmployeeNo = model.EmployeeNo,
                    //EmploymentType = model.EmploymentTypeName,
                    MDAId = model.MDAId,
                    Designation = model.Designation,
                    LastName = model.LastName.ToUpper(),
                    //FirstName = model.FirstName.ToUpper(),
                    FirstName = (string.IsNullOrEmpty(model.FirstName) ? " " : char.ToUpper(model.FirstName[0]) + model.FirstName.Substring(1)),
                    // MiddleName = model.MiddleName,
                    MiddleName = (string.IsNullOrEmpty(model.MiddleName) ? " " : char.ToUpper(model.MiddleName[0]) + model.MiddleName.Substring(1)),
                    FullName = model.FullName,
                    EmpSurname = model.EmpSurname,
                    EmpFirstname = model.EmpFirstname,
                    EmpMiddleName = model.EmpMiddleName,
                    DOB = model.DOB,
                    Gender = model.Gender,
                    Phone = model.Phone,
                    Address = model.Address,
                    LGA = model.LGA,
                    NextofKinFirstName = (string.IsNullOrEmpty(model.NextofKinFirstName) ? " " : char.ToUpper(model.NextofKinFirstName[0]) + model.NextofKinFirstName.Substring(1)),
                    NextofKinLastName = model.NextofKinLastName.ToUpper(),
                    NextOfkinrelationship = model.NextOfkinrelationship,
                    NextofKinPhone = model.NextofKinPhone,
                    DateofFirstAppointment = model.DateofFirstAppointment,
                    DateofPresentAppointment = model.DateofPresentAppointment,
                    DueDate = model.DueDate,
                    Email = model.Email,
                    DateUploaded = DateTime.Now.Date,
                    UploadedBy = user.UserName
                    

                };

                if (!string.IsNullOrEmpty(model.EmployeeNo))
                {
                    employee.EmployeeNo = model.EmployeeNo;
                }
                else
                {
                    employee.EmployeeNo = model.LASRRAID;
                }
                //Picture details
                if (model.Photo != null && model.Photo.Length > 0)
                //if (!string.IsNullOrEmpty(form["imgCapture"].ToString()))
                {
                    var uploadDir = @"images/CSBUsers";
                    //var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    string fileName = model.LASRRAID;
                    //var extension = Path.GetExtension(model.ImageUrl.FileName);
                    string extension = ".jpg";
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);

                    //string data = form["imgCapture"].ToString();
                    string data = model.Photo.ToString();

                    //pictureByteArray = Convert.FromBase64String(data.Split(',')[1]);
                    pictureByteArray = Convert.FromBase64String(data);
                    // ResidentfPicture = ConvertByteArrayToImage(pictureByteArray);

                    int offset = 0;
                    const int Buffer_Size = 4096;

                    using var fileStream = new FileStream(path,
                                                            FileMode.Append, FileAccess.Write,
                                                            FileShare.None, bufferSize: Buffer_Size, useAsync: true);
                    await fileStream.WriteAsync(pictureByteArray, offset, pictureByteArray.Length);

                    // await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.ImageUrl = "/" + uploadDir + "/" + fileName;
                }
                //get signature details from model
                if (model.Signature != null && model.Signature.Length > 0)
                //if (!string.IsNullOrEmpty(form["imgCapture"].ToString()))
                {
                    var uploadDir = @"images/Signatures";
                    //var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    string fileName = model.LASRRAID;
                    //var extension = Path.GetExtension(model.ImageUrl.FileName);
                    string extension = "_26.jpg";
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);

                    //string data = form["imgCapture"].ToString();
                    string Sigdata = model.Signature.ToString();

                    //pictureByteArray = Convert.FromBase64String(data.Split(',')[1]);
                    signatureByteArray = Convert.FromBase64String(Sigdata);
                    // ResidentfPicture = ConvertByteArrayToImage(pictureByteArray);

                    int offset = 0;
                    const int Buffer_Size = 4096;

                    using var fileStream = new FileStream(path,
                                                            FileMode.Append, FileAccess.Write,
                                                            FileShare.None, bufferSize: Buffer_Size, useAsync: true);
                    await fileStream.WriteAsync(signatureByteArray, offset, signatureByteArray.Length);

                    // await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.SignatureImageUrl = "/" + uploadDir + "/" + fileName;
                }
                await _employeeService.CreateAsync(employee);
                //return RedirectToAction(nameof(MDAIndex));
                //return RedirectToAction(nameof(MDAIndexJQueryDataTable));
                return RedirectToAction("MDAIndexJQueryDataTable", "Employee", new { mdaID = model.MDAId });

            }
            else
            {
                ModelState.AddModelError("", "You have provided Invalid Data");
                return View(model);
            }
            ViewBag.MDA = _mdaService.GetSpecificMDAforEmployee(model.MDAId);
            ViewBag.EmployeeType = _employeeTypeService.GetAllEmploymentTypeforEmployee();

            //string last = "I got here";

            return View();

        }
       
        public async Task<JsonResult> VerifyLASRRAId(string LA)
        {
            //string ResponseMessage = "";

            IDValidation validationModel = new IDValidation();

            if (ModelState.IsValid == true)
            {
                var message = await _ivalidationService.ValidateID(LA);

                validationModel.ResponseMessage = message.ToString();

                ViewBag.ValidationMessage = validationModel.ResponseMessage;

            }
            //return (ViewBag.ValidationMessage);
            return Json(new { name6 = ViewBag.ValidationMessage });

        }
        public async Task<JsonResult> GetDetailByLASRRAID(string LA)
        {
            GetInfoPictureModel gpi = new GetInfoPictureModel();

            string resultString = JsonConvert.SerializeObject(gpi);

            if (ModelState.IsValid == true)
            {
                var messaage = await _getDetailsOnLASRRAIdService.GetDetailsOnLASRRAId(LA);

                resultString = messaage;
                //var result = new JsonResult
                //{
                //    Data = JsonConvert.DeserializeObject(messaage)

                //};

                gpi = JsonConvert.DeserializeObject<GetInfoPictureModel>(resultString);
            }
            return Json(new { Id = gpi.Id, surname = gpi.surname, firstname = gpi.firstname, middlename = gpi.middlename, gender = gpi.gender, phone = gpi.phone, birtDate = gpi.birtDate, address = gpi.Address, regDate = gpi.regDate, name6 = gpi.name6 });

        }
        public async Task<JsonResult>GetImagesByLASRRAID()
        {
            GetInfoPictureModel gpi = new GetInfoPictureModel();

            string resultString = JsonConvert.SerializeObject(gpi);
            List<string> ids = new List<string>();
            string textFilePath = "D:/ProjectPicture/LAWMA.txt";
            try
            {
                using (StreamReader reader = new StreamReader(textFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ids.Add(line.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
            foreach (string item in ids)
            {
                var messaage = await _getDetailsOnLASRRAIdService.GetDetailsOnLASRRAId(item);

                resultString = messaage;
                //var result = new JsonResult
                //{
                //    Data = JsonConvert.DeserializeObject(messaage)

                //};

                gpi = JsonConvert.DeserializeObject<GetInfoPictureModel>(resultString);
            }

            return Json(new { Id = gpi.Id, surname = gpi.surname, firstname = gpi.firstname, middlename = gpi.middlename, gender = gpi.gender, phone = gpi.phone, birtDate = gpi.birtDate, address = gpi.Address, regDate = gpi.regDate, name6 = gpi.name6 });
        }
        
        
        public JsonResult GetOracleStaffRecord(string oracleNumber)
        {
            OracleStaffData oracleStaffObject = new OracleStaffData();

            // IEnumerable<OracleStaffData> listOfStaff = _getDetailsOnOracleNumberService.GetAllOracleStaffList();
            //var list1 = _employeeTypeService.GetAll();

            //var list = _getDetailsOnOracleNumberService.GetAll();

            var ExistingOracleStaff = _getDetailsOnOracleNumberService.GetDetailonEmployeeDetailView(oracleNumber);

            if (ExistingOracleStaff == null)
            {
                //return NotFound();
            }

            string resultString = JsonConvert.SerializeObject(oracleStaffObject);

            if (ModelState.IsValid == true)
            {
                //oracleStaffObject = _getDetailsOnOracleNumberService.GetDetailsOnEmployeeNo(oracleNumber);

                oracleStaffObject = ExistingOracleStaff;
            }
            return Json(new { OracleNumber = oracleStaffObject?.EmployeeNo, surname = oracleStaffObject?.SurName, firstname = oracleStaffObject?.FirstName, middlename = oracleStaffObject?.MiddleName, gender = oracleStaffObject?.Gender, name6 = oracleStaffObject?.JobTitle, ministry = oracleStaffObject?.Ministry, name7 = oracleStaffObject?.Email, employeecategory = oracleStaffObject?.EmployeeCategory, name8 = oracleStaffObject?.DateOfFirstAppointment.ToString("yyyy-MM-dd"), dueDate = oracleStaffObject?.DueDate });
        }

        [HttpGet]
        public IActionResult Detail(string LasrraID, string mdaID)
        {
            string text = LasrraID;

            var employee = _employeeService.GetByLASRRAIDetailslView(LasrraID);
            //var employee2 = _employeeService.GetByLASRRAId(LasrraID);
            // IQueryable<Employee> empList = _employeeService.GetEmployeeByLASRRAIDDetailView();
            //if (empList==null)
            //{
            //    return NotFound();
            //}
            //var emp = empList.First(x => x.LASRRAID == LasrraID);

            //var employee = _employeeService.GetByLASRRAId(LasrraID);

            if (employee == null)
            {
                return NotFound();
            }
            EmployeeDetailViewModel model = new EmployeeDetailViewModel()
            {
                LASRRAID = employee.LASRRAID,
                Surname = employee.Surname,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                Gender = employee.Gender,
                DOB = employee.DOB,
                ContactPhone = employee.ContactPhone,
                Address = employee.Address,
                LGA = employee.LGA,
                ImageUrl = employee.ImageUrl,
                SignatureImageUrl = employee.SignatureImageUrl,
                //EmploymentTypeName = employee.EmployeeTypeName,
                EmployeeTypeName = employee.EmployeeTypeName,
                Designation = employee.Designation,
                //Ministry = employee.Ministryname,
                MDAId = employee.MDAID,
                EmployeeNo = employee.EmployeeNo,
                DateofFirstAppointment = employee.DateofFirstAppointment,
                NextofkinFullName = employee.NextofkinFullName,
                NextOfkinrelationship = employee.NextOfkinrelationship,
                NextofKinPhone = employee.NextofKinPhone,
                Email = employee.Email
            };

            return View(model);
        }
        [HttpGet]
        public IActionResult DetailCardRequest(string LasrraID)
        {
            string text = LasrraID;

            var employee = _employeeService.GetByLASRRAIDetailslView(LasrraID);
            //var employee2 = _employeeService.GetByLASRRAId(LasrraID);
            // IQueryable<Employee> empList = _employeeService.GetEmployeeByLASRRAIDDetailView();
            //if (empList==null)
            //{
            //    return NotFound();
            //}
            //var emp = empList.First(x => x.LASRRAID == LasrraID);

            //var employee = _employeeService.GetByLASRRAId(LasrraID);

            if (employee == null)
            {
                return NotFound();
            }
            EmployeeDetailViewModel model = new EmployeeDetailViewModel()
            {
                LASRRAID = employee.LASRRAID,
                Surname = employee.Surname,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                Gender = employee.Gender,
                DOB = employee.DOB,
                ContactPhone = employee.ContactPhone,
                Address = employee.Address,
                LGA = employee.LGA,
                ImageUrl = employee.ImageUrl,
                SignatureImageUrl = employee.SignatureImageUrl,
                //EmploymentTypeName = employee.EmployeeTypeId,
                EmployeeTypeName=employee.EmployeeTypeName,
                Designation = employee.Designation,
                Ministry = employee.Ministry,
                EmployeeNo = employee.EmployeeNo,
                DateofFirstAppointment = employee.DateofFirstAppointment,
                NextofkinFullName = employee.NextofkinFullName,
                NextOfkinrelationship = employee.NextOfkinrelationship,
                NextofKinPhone = employee.NextofKinPhone,
                Email = employee.Email
            };


            return View(model);
        }
        [HttpGet]
        public IActionResult AdminDetail(string LasrraID)
        {
            string text = LasrraID;

            var employee = _employeeService.GetByLASRRAIDetailslView(LasrraID);
            //var employee2 = _employeeService.GetByLASRRAId(LasrraID);
            // IQueryable<Employee> empList = _employeeService.GetEmployeeByLASRRAIDDetailView();
            //if (empList==null)
            //{
            //    return NotFound();
            //}
            //var emp = empList.First(x => x.LASRRAID == LasrraID);

            //var employee = _employeeService.GetByLASRRAId(LasrraID);

            if (employee == null)
            {
                return NotFound();
            }
            EmployeeDetailViewModel model = new EmployeeDetailViewModel()
            {
                LASRRAID = employee.LASRRAID,
                Surname = employee.Surname,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                Gender = employee.Gender,
                DOB = employee.DOB,
                ContactPhone = employee.ContactPhone,
                Address = employee.Address,
                LGA = employee.LGA,
                ImageUrl = employee.ImageUrl,
                SignatureImageUrl = employee.SignatureImageUrl,
                //EmploymentTypeName = employee.EmployeeTypeName,
                EmployeeTypeName=employee.EmployeeTypeName,
                Designation = employee.Designation,
                //Ministry = employee.Ministryname,
                MDAId=employee.MDAID,
                EmployeeNo = employee.EmployeeNo,
                DateofFirstAppointment = employee.DateofFirstAppointment,
                NextofkinFullName = employee.NextofkinFullName,
                NextOfkinrelationship = employee.NextOfkinrelationship,
                NextofKinPhone = employee.NextofKinPhone,
                Email = employee.Email
            };


            return View(model);
        }

        public IActionResult Edit(string LASRRAID, string mdaID)
        {
            //ViewBag.mdaID = mdaID;

            if (LASRRAID==null || LASRRAID==string.Empty)
			{
                return NotFound();
			}
            Employee employee = _employeeService.GetByLASRRAIDEditlView(LASRRAID);

            if (employee == null )
            {
                return NotFound();
            }
            ViewBag.MDA = _mdaService.GetAllMDAforEmployee();
            ViewBag.EmployeeType = _employeeTypeService.GetAllEmploymentTypeforEmployee();
            string MDASelected = employee.MDAId;
            string emptypeId = employee.EmployeeTypeId;
            //ViewBag.MDA = _mdaService.GetByMDAId(MDASelected);
            //ViewBag.EmployeeType = _employeeTypeService.GetByEmployeeTypeById(emptypeId);

            var model = new EmployeeEditViewModel()
            {
                LASRRAID = employee.LASRRAID,
                FirstName = employee.FirstName,
                MiddleName = (string.IsNullOrEmpty(employee.MiddleName) ? "" : employee.MiddleName),
                LastName = employee.LastName,
                EmpSurname=employee.EmpSurname,
                EmpFirstname=employee.EmpFirstname,
                EmpMiddleName=employee.EmpMiddleName,
                DOB = employee.DOB,
                Gender=employee.Gender,
                Address = employee.Address,
                LGA = employee.LGA,
                ImageUrl = employee.ImageUrl,
                EmployeeNo = employee.EmployeeNo,
                MDAId = employee.MDAId,
                //EmploymentType = employee.EmployeeTypeId,
                Designation = employee.Designation,
                //Ministry = employee.MDAId,
               EmployeeTypeId = employee.EmployeeTypeId,
                NextofKinLastName = employee.NextofKinLastName,
                NextofKinFirstName = employee.NextofKinFirstName,
                NextofKinPhone = employee.NextofKinPhone,
                NextOfkinrelationship = employee.NextOfkinrelationship,
                Email=employee.Email,
                Phone=employee.Phone,
                DateofFirstAppointment=employee.DateofFirstAppointment.Date,
                DateofPresentAppointment=employee.DateofPresentAppointment.Date,
                SignatureImageUrl = employee.SignatureImageUrl

            };
            return View(model);
           
        }
        public IActionResult AdminEdit(string LASRRAID)
        {


            if (LASRRAID == null || LASRRAID == string.Empty)
            {
                return NotFound();
            }
            Employee employee = _employeeService.GetByLASRRAIDEditlView(LASRRAID);

            if (employee == null)
            {
                return NotFound();
            }
            ViewBag.MDA = _mdaService.GetAllMDAforEmployee();
            ViewBag.EmployeeType = _employeeTypeService.GetAllEmploymentTypeforEmployee();
            string MDASelected = employee.MDAId;
            string emptypeId = employee.EmployeeTypeId;
            //ViewBag.MDA = _mdaService.GetByMDAId(MDASelected);
            //ViewBag.EmployeeType = _employeeTypeService.GetByEmployeeTypeById(emptypeId);

            var model = new EmployeeEditViewModel()
            {
                LASRRAID = employee.LASRRAID,
                FirstName = employee.FirstName,
                MiddleName = (string.IsNullOrEmpty(employee.MiddleName) ? "" : employee.MiddleName),
                LastName = employee.LastName,
                EmpSurname = employee.EmpSurname,
                EmpFirstname = employee.EmpFirstname,
                EmpMiddleName = employee.EmpMiddleName,
                DOB = employee.DOB,
                Gender = employee.Gender,
                Address = employee.Address,
                LGA = employee.LGA,
                ImageUrl = employee.ImageUrl,
                EmployeeNo = employee.EmployeeNo,
                MDAId = employee.MDAId,
                EmploymentType = employee.EmployeeTypeId,
                Designation = employee.Designation,
                //Ministry = employee.MDAId,
                EmployeeTypeId = employee.EmployeeTypeId,
                NextofKinLastName = employee.NextofKinLastName,
                NextofKinFirstName = employee.NextofKinFirstName,
                NextofKinPhone = employee.NextofKinPhone,
                NextOfkinrelationship = employee.NextOfkinrelationship,
                Email = employee.Email,
                Phone = employee.Phone,
                DateofFirstAppointment = employee.DateofFirstAppointment.Date,
                DateofPresentAppointment = employee.DateofPresentAppointment.Date,
                SignatureImageUrl=employee.SignatureImageUrl

            };
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditViewModel model)
        {
            var employee = _employeeService.GetByLASRRAId(model.LASRRAID);
            

            if (ModelState.IsValid)
            {
                if (employee == null)
                {
                    return NotFound();
                }
                employee.Phone = model.Phone;
                employee.Address = model.Address;
                employee.LGA = model.LGA;
                employee.MDAId = model.MDAId;
                employee.EmployeeTypeId = model.EmployeeTypeId;
                employee.Designation = model.Designation;
                employee.NextofKinLastName = model.NextofKinLastName;
                employee.NextofKinFirstName = model.NextofKinFirstName;
                employee.NextofKinPhone = model.NextofKinPhone;
                employee.NextOfkinrelationship = model.NextOfkinrelationship;

                //get signature details from model
                if (model.Signature != null && model.Signature.Length > 0)
                //if (!string.IsNullOrEmpty(form["imgCapture"].ToString()))
                {
                    var uploadDir = @"images/Signatures";
                    //var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    string fileName = model.LASRRAID;
                    //var extension = Path.GetExtension(model.ImageUrl.FileName);
                    string extension = "_26.jpg";
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);

                    //string data = form["imgCapture"].ToString();
                    string Sigdata = model.Signature.ToString();

                    //pictureByteArray = Convert.FromBase64String(data.Split(',')[1]);
                    signatureByteArray = Convert.FromBase64String(Sigdata);
                    // ResidentfPicture = ConvertByteArrayToImage(pictureByteArray);

                    int offset = 0;
                    const int Buffer_Size = 4096;

                    using var fileStream = new FileStream(path,
                                                            FileMode.Append, FileAccess.Write,
                                                            FileShare.None, bufferSize: Buffer_Size, useAsync: true);
                    await fileStream.WriteAsync(signatureByteArray, offset, signatureByteArray.Length);

                    // await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.SignatureImageUrl = "/" + uploadDir + "/" + fileName;
                }
                await _employeeService.UpdateAsync(employee);
                // return RedirectToAction(nameof(Index));
                return RedirectToAction("MDAIndexJQueryDataTable", "Employee", new { mdaID = model.MDAId });
            }
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEdit(EmployeeEditViewModel model)
        {
            var employee = _employeeService.GetByLASRRAId(model.LASRRAID);


            if (ModelState.IsValid)
            {
                if (employee == null)
                {
                    return NotFound();
                }
                employee.Phone = model.Phone;
                employee.Address = model.Address;
                employee.LGA = model.LGA;
                employee.MDAId = model.MDAId;
                employee.EmployeeTypeId = model.EmployeeTypeId;
                employee.Designation = model.Designation;
                employee.NextofKinLastName = model.NextofKinLastName;
                employee.NextofKinFirstName = model.NextofKinFirstName;
                employee.NextofKinPhone = model.NextofKinPhone;
                employee.NextOfkinrelationship = model.NextOfkinrelationship;
                employee.DateofFirstAppointment = model.DateofFirstAppointment;
                employee.DateofPresentAppointment = model.DateofPresentAppointment;
                employee.EmployeeTypeId = model.EmployeeTypeId;


                //get signature details from model
                if (model.Signature != null && model.Signature.Length > 0)
                //if (!string.IsNullOrEmpty(form["imgCapture"].ToString()))
                {
                    var uploadDir = @"images/Signatures";
                    //var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    string fileName = model.LASRRAID + "_26";
                    //var extension = Path.GetExtension(model.ImageUrl.FileName);
                    string extension = ".jpg";
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);

                    //string data = form["imgCapture"].ToString();
                    string Sigdata = model.Signature.ToString();

                    //pictureByteArray = Convert.FromBase64String(data.Split(',')[1]);
                    signatureByteArray = Convert.FromBase64String(Sigdata);
                    // ResidentfPicture = ConvertByteArrayToImage(pictureByteArray);

                    int offset = 0;
                    const int Buffer_Size = 4096;

                    using var fileStream = new FileStream(path,
                                                            FileMode.Append, FileAccess.Write,
                                                            FileShare.None, bufferSize: Buffer_Size, useAsync: true);
                    await fileStream.WriteAsync(signatureByteArray, offset, signatureByteArray.Length);

                    // await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.SignatureImageUrl = "/" + uploadDir + "/" + fileName;
                }
                await _employeeService.UpdateAsync(employee);
                return RedirectToAction(nameof(Index));
                //return RedirectToAction("MDAIndexJQueryDataTable", "Employee", new { mdaID = model.MDAId });
            }
            return View(model);

        }



    }
}
