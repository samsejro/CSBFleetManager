using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using CSBFleetManager.Entity;
using CSBFleetManager.Models;
using CSBFleetManager.Services;
using CSBFleetManager.Services.Implementation;
using System.IO;
using Newtonsoft.Json;

namespace CSBFleetManager.Controllers
{
    //[Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMDAService _mdaService;
        private readonly IEmployeeTypeService _employeeTypeService;
        private readonly IValidationService _ivalidationService;
        private readonly IGetDetailsOnLASRRAIdService _getDetailsOnLASRRAIdService;

        public EmployeeController(IEmployeeService employeeService,IEmployeeTypeService employeeTypeService, IMDAService mDAService, IValidationService validationService, IGetDetailsOnLASRRAIdService getDetailsOnLASRRAIdService , IWebHostEnvironment hostingEnvironment)
        {
            _employeeService = employeeService;
            _mdaService = mDAService;
            _employeeTypeService = employeeTypeService;
            _hostingEnvironment = hostingEnvironment;
            _ivalidationService = validationService;
            _getDetailsOnLASRRAIdService = getDetailsOnLASRRAIdService;
        }
        public IActionResult Index(int? pageNumber)
        {
            var employees=_employeeService.GetAll().Select(employee=> new EmployeeIndexViewModel
            {
              LASRRAID=employee.LASRRAID,
              EmployeeNo=employee.EmployeeNo,
              EmploymentTypeName = employee.EmploymentType.EmployeeTypeName,
              //LAGID=employee.LAGID,
              Gender = employee.Gender,
              Designation = employee.Designation,
              Ministry=employee.Ministry.MDAname,
              ImageUrl=employee.ImageUrl,
              LGA=employee.LGA

            }).ToList();

            int pageSize = 4;
            return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            //return View();
        }
        //[AllowAnonymous]
        [HttpGet]
        public IActionResult Create()
        {
            List<MDA> mdaList = _mdaService.GetAll().ToList();
            List<EmployeeType> EmpTypeList = _employeeTypeService.GetAll().ToList();
            
            ViewBag.MDA = new SelectList(mdaList, "MDAId", "MDAname");
            ViewBag.EmployeeType = new SelectList(EmpTypeList, "EmployeeTypeId", "EmployeeTypeName");

            var model = new EmployeeCreateViewModel();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] //Prevents cross-site Request Forgery Attacks
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {


            if (ModelState.IsValid)
            {
                var employee = new Employee
                {
                    LASRRAID = model.LASRRAID,
                    LAGID = model.LAGID,
                    EmployeeNo = model.EmployeeNo,
                    EmploymentType = model.EmploymentTypeName,
                    MDAId = model.MDA,
                    Designation = model.Designation,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    FullName = model.FullName,
                    DOB = model.DOB,
                    Gender = model.Gender,
                    Phone = model.Phone,
                    Address = model.Address,
                    NextofKinFirstName = model.NextofKinFirstName,
                    NextofKinLastName = model.NextofKinLastName,
                    NextOfkinrelationship = model.NextOfkinrelationship,
                    NextofKinPhone = model.NextofKinPhone,
                    DateofFirstAppointment = model.DateofFirstAppointment,
                    Email = model.Email,

                };
                if (model.ImageUrl != null && model.ImageUrl.Length > 0)
                {
                    var uploadDir = @"images/employee";
                    var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    var extension = Path.GetExtension(model.ImageUrl.FileName);
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);
                    await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.ImageUrl = "/" + uploadDir + "/" + fileName;
                }
                await _employeeService.CreateAsync(employee);
                return RedirectToAction(nameof(Index));

            }
            
            return View();
        }
        //public async Task<IActionResult> VerifyLASRRAId(string LA)
        //{
        //    //string ResponseMessage = "";

        //    IDValidation validationModel = new IDValidation();

        //    if (ModelState.IsValid==true)
        //    {
        //        var message = await   _ivalidationService.ValidateID(LA);

        //        validationModel.ResponseMessage = message.ToString();

        //        ViewBag.ValidationMessage = validationModel.ResponseMessage;

        //    }
        //    return (ViewBag.ValidationMessage);
        //}
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

            if (ModelState.IsValid==true)
            {
                var messaage = await _getDetailsOnLASRRAIdService.GetDetailsOnLASRRAId(LA);

                resultString = messaage;
                //var result = new JsonResult
                //{
                //    Data = JsonConvert.DeserializeObject(messaage)

                //};

                gpi= JsonConvert.DeserializeObject<GetInfoPictureModel>(resultString);
            }
            return Json(new { Id = gpi.Id, surname = gpi.surname, firstname = gpi.firstname, middlename = gpi.middlename, gender = gpi.gender, phone = gpi.phone, birtDate = gpi.birtDate, address = gpi.Address, regDate = gpi.regDate, name6 = gpi.name6 });

        }

    }
}
