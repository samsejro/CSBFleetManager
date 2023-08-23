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
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using System.Drawing;
//using Microsoft.AspNetCore.Mvc;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Http;

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
        private readonly IGetDetailsOnLASRRAIdService _getDetailsOnLASRRAIdService;
        private readonly IGetDetailsOnOracleNumberService _getDetailsOnOracleNumberService;
        private readonly IRegistrationStatistics _getRegistrationStatistics;


        //Image SignatureImage = null;
        Image ResidentfPicture = null;
        byte[] pictureByteArray = null;
        byte[] signatureByteArray = null;
        public EmployeeController(IEmployeeService employeeService, IEmployeeTypeService employeeTypeService, IMDAService mDAService, IValidationService validationService, IGetDetailsOnLASRRAIdService getDetailsOnLASRRAIdService,IGetDetailsOnOracleNumberService getDetailsOnOracleNumberService,IRegistrationStatistics getRegistrationStatistics,   IWebHostEnvironment hostingEnvironment)
        {
            _employeeService = employeeService;
            _mdaService = mDAService;
            _employeeTypeService = employeeTypeService;
            _hostingEnvironment = hostingEnvironment;
            _ivalidationService = validationService;
            _getDetailsOnLASRRAIdService = getDetailsOnLASRRAIdService;
            _getDetailsOnOracleNumberService = getDetailsOnOracleNumberService;
            _getRegistrationStatistics = getRegistrationStatistics;
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
        public IActionResult Index(int? pageNumber)
        {
            //int TotalReg = 0;
            //int TotalRegMale = 0;
            //int TotalRegeFemale = 0;
            //int TotalRegToday = 0;

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
                EmploymentTypeName =employee.EmployeeTypeId,
                //EmploymentTypeName = empType.,
                //LAGID=employee.LAGID,
                Gender = employee.Gender,
               // Designation = employee.Designation,
                Ministry=employee.MDAId,
                // Ministry =_mdaService.GetMDANameById(employee.MDAId),
                ImageUrl = employee.ImageUrl,
                LGA = employee.LGA

            }).ToList();

            int pageSize = 4;
            return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employees, pageNumber ?? 1, pageSize));
            //return View();
        }

        //[Authorize(Roles = "Basic,Manager")]
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
                    DOB = model.DOB,
                    Gender = model.Gender,
                    Phone = model.Phone,
                    Address = model.Address,
                    LGA=model.LGA,
                    NextofKinFirstName = (string.IsNullOrEmpty(model.NextofKinFirstName) ? " " : char.ToUpper(model.NextofKinFirstName[0]) + model.NextofKinFirstName.Substring(1)),
                    NextofKinLastName = model.NextofKinLastName.ToUpper(),
                    NextOfkinrelationship = model.NextOfkinrelationship,
                    NextofKinPhone = model.NextofKinPhone,
                    DateofFirstAppointment = model.DateofFirstAppointment,
                    Email = model.Email,
                    DateUploaded=DateTime.Now.Date

                };
                if (!string.IsNullOrEmpty(model.EmployeeNo))
                {
                    employee.EmployeeNo = model.EmployeeNo;
                }
                else
                {
                    employee.EmployeeNo = model.LASRRAID;
                }
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
                await _employeeService.CreateAsync(employee);
                return RedirectToAction(nameof(Index));

            }
            ViewBag.MDA = _mdaService.GetAllMDAforEmployee();
            ViewBag.EmployeeType = _employeeTypeService.GetAllEmploymentTypeforEmployee();

            string last = "I got here";

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
        
        public JsonResult GetOracleStaffRecord(string oracleNumber)
        {
            OracleStaffData oracleStaffObject = new OracleStaffData();

            // IEnumerable<OracleStaffData> listOfStaff = _getDetailsOnOracleNumberService.GetAllOracleStaffList();
            //var list1 = _employeeTypeService.GetAll();

            //var list = _getDetailsOnOracleNumberService.GetAll();

            var ExistingOracleStaff = _getDetailsOnOracleNumberService.GetDetailonEmployeeDetailView(oracleNumber);

            if (ExistingOracleStaff ==null)
            {
                //return NotFound();
            }
          
            string resultString = JsonConvert.SerializeObject(oracleStaffObject);

            if (ModelState.IsValid==true)
            {
                //oracleStaffObject = _getDetailsOnOracleNumberService.GetDetailsOnEmployeeNo(oracleNumber);

                oracleStaffObject = ExistingOracleStaff;
            }
            return Json(new { OracleNumber = oracleStaffObject.EmployeeNo, surname = oracleStaffObject.SurName, firstname = oracleStaffObject.FirstName, middlename = oracleStaffObject.MiddleName, gender = oracleStaffObject.Gender, name11 = oracleStaffObject.JobTitle, ministry = oracleStaffObject.Ministry, name12 = oracleStaffObject.Email, employeecategory = oracleStaffObject.EmployeeCategory, dateofFirstappointment = oracleStaffObject.DateOfFirstAppointment, dueDate = oracleStaffObject.DueDate });

        }
        
        [HttpGet]
        public IActionResult Detail(string LasrraID)
        {
            string text = LasrraID;

            var employee = _employeeService.GetByLASRRAIDDetailView(LasrraID);
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
                Surname = employee.LastName,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                Gender = employee.Gender,
                DOB = employee.DOB,
                ContactPhone = employee.Phone,
                Address = employee.Address,
                LGA = employee.LGA,
                ImageUrl = employee.ImageUrl,
                EmploymentTypeName = employee.EmployeeTypeId,
                Designation = employee.Designation,
                Ministry = employee.MDAId,
                EmployeeNo = employee.EmployeeNo,
                DateofFirstAppointment = employee.DateofFirstAppointment,
                NextofkinFullName = employee.NextofKinLastName.ToUpper() + " " + (string.IsNullOrEmpty(employee.NextofKinFirstName) ? " " : char.ToUpper(employee.NextofKinFirstName[0]) + employee.NextofKinFirstName.Substring(1)),
                NextOfkinrelationship = employee.NextOfkinrelationship,
                NextofKinPhone = employee.NextofKinPhone,
                Email=employee.Email


            };
            //EmployeeDetailViewModel model = new EmployeeDetailViewModel()
            //{
            //    LASRRAID = emp.LASRRAID,
            //    Surname = emp.LastName,
            //    FirstName = emp.FirstName,
            //    MiddleName = emp.MiddleName,
            //    Gender = emp.Gender,
            //    DOB = emp.DOB,
            //    ContactPhone = emp.Phone,
            //    Address = emp.Address,
            //    LGA = emp.LGA,
            //    ImageUrl = emp.ImageUrl,
            //    EmploymentTypeName = emp.EmployeeTypeId,
            //    Designation = emp.Designation,
            //    Ministry = emp.MDAId,
            //    EmployeeNo = emp.EmployeeNo,
            //    DateofFirstAppointment = emp.DateofFirstAppointment,
            //    NextofkinFullName = emp.NextofKinLastName.ToUpper() + " " + (string.IsNullOrEmpty(emp.NextofKinFirstName) ? " " : char.ToUpper(emp.NextofKinFirstName[0]) + emp.NextofKinFirstName.Substring(1)),
            //    NextOfkinrelationship = emp.NextOfkinrelationship,
            //    NextofKinPhone = emp.NextofKinPhone,


            //};

            return View(model);
        }
      
        public IActionResult Edit(string LASRRAID)
        {
            var employee = _employeeService.GetByLASRRAIDDetailView(LASRRAID);
            // var employee = _employeeService.GetByLASRRAId(LASRRAID);

            ViewBag.MDA = _mdaService.GetAllMDAforEmployee();
            ViewBag.EmployeeType = _employeeTypeService.GetAllEmploymentTypeforEmployee();

            if (employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeEditViewModel()
            {
                LASRRAID = employee.LASRRAID,
                FirstName = employee.FirstName,
                MiddleName = (string.IsNullOrEmpty(employee.MiddleName) ? "" : employee.MiddleName),
                LastName = employee.LastName,
                DOB = employee.DOB,
                Gender=employee.Gender,
                Address = employee.Address,
                LGA = employee.LGA,
                ImageUrl = employee.ImageUrl,
                EmployeeNo = employee.EmployeeNo,
                // MDAId = employee.MDAId,
                EmploymentType = employee.EmployeeTypeId,
                Designation = employee.Designation,
                Ministry = employee.MDAId,
               // EmployeeTypeId = employee.EmployeeTypeId,
                NextofKinLastName = employee.NextofKinLastName,
                NextofKinFirstName = employee.NextofKinFirstName,
                NextofKinPhone = employee.NextofKinPhone,
                NextOfkinrelationship = employee.NextOfkinrelationship,
                Email=employee.Email,
                Phone=employee.Phone
                


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
                employee.Designation = model.Designation;
                employee.NextofKinLastName = model.NextofKinLastName;
                employee.NextofKinFirstName = model.NextofKinFirstName;
                employee.NextofKinPhone = model.NextofKinPhone;
                employee.NextOfkinrelationship = model.NextOfkinrelationship;

                await _employeeService.UpdateAsync(employee);
                return RedirectToAction(nameof(Index));
            }
            return View();

        }

    }
}
