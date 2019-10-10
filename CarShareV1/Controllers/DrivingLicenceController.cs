using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class DrivingLicenceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;
        private readonly IHostingEnvironment _env;
        private readonly IEmailSender _emailSender;

        public DrivingLicenceController( IEmailSender emailSender,IHostingEnvironment env, ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
            _env = env;
            _emailSender = emailSender;

        }
 
        // GET: DrivingLicence vaild
        public async Task<IActionResult> Index()
        {
            return View(await _context.DrivingLicences.Where(dr => dr.IsValid == true).ToListAsync());
        }
        //need validate
        public async Task<IActionResult> PendingDrivingLicence()
        {
       
            return View("Index", await _context.DrivingLicences.Where(dr=> dr.IsValid==false).ToListAsync());
        }


        public async Task<IActionResult> MyDrivingLicence()
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            DrivingLicence _DrivingLicence = new DrivingLicence();
            _DrivingLicence= await _context.DrivingLicences.FirstOrDefaultAsync(m => m.UserId == currentUserId);
            if (_DrivingLicence == null)
            {
                DrivingLicence _drivingLicence = new DrivingLicence();
                _drivingLicence.Image = "0";
                ViewBag.Msg = "Your Driving Licence has been updated";
                return View(_drivingLicence);
            }
            return View(_DrivingLicence);
        }
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCreate([Bind("Id,LicenseNumber,IssueDate,ExpireDate,Image,UserId,CreateTime,UpdateTime")] DrivingLicence drivingLicence)
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            if (currentUserId == null)
            {
                return RedirectToAction("Error", "Home");
            }
            if (ModelState.IsValid)
            {
                
                int Id;
                if (drivingLicence.Id.Equals(0))//Add New Record
                {    
                     
                drivingLicence.UpdateTime = DateTime.Now;
                drivingLicence.CreateTime = DateTime.Now;
                drivingLicence.Image ="/Image/CarShare/DL.png";
                drivingLicence.UserId = currentUserId;
                    drivingLicence.IsValid = false;
                _context.Add(drivingLicence);
                 await _context.SaveChangesAsync();
                Id = drivingLicence.Id;
               
                }
                else
                {
                    //Update
                    DrivingLicence _drivingLicence = await _context.DrivingLicences.FindAsync(drivingLicence.Id);
                    _drivingLicence.LicenseNumber = drivingLicence.LicenseNumber;
                    _drivingLicence.IssueDate = drivingLicence.IssueDate;
                    _drivingLicence.ExpireDate = drivingLicence.ExpireDate;
                    _drivingLicence.UpdateTime = DateTime.Now;
                    _drivingLicence.IsValid = false;
                    _context.DrivingLicences.Update(_drivingLicence);
                    await _context.SaveChangesAsync();
                    Id = _drivingLicence.Id;
                }
              
               
                return RedirectToAction("UploadDrivingLicenceImage", "DrivingLicence", new { id = Id });
            }
            else
            {
                ViewBag.Msg ="Please fill the required field..";
                return View("MyDrivingLicence", drivingLicence);
            }
          
        }

         

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var model = await _context.DrivingLicences.FindAsync(id);
            if (model == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(model);
        }

        // POST: DrivingLicence/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LicenseNumber,IssueDate,ExpireDate,Image,UserId,IsValid,CreateTime,UpdateTime")] DrivingLicence drivingLicence)
        {
            if (id != drivingLicence.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                   DrivingLicence _DL = new DrivingLicence();
                _DL.Id = drivingLicence.Id;
                _DL.UserId = drivingLicence.UserId;
                _DL.LicenseNumber = drivingLicence.LicenseNumber;
                _DL.ExpireDate = drivingLicence.ExpireDate;
                _DL.IssueDate = drivingLicence.IssueDate;
                _DL.IsValid = drivingLicence.IsValid;
                _DL.Image = drivingLicence.Image;
                _DL.CreateTime = drivingLicence.CreateTime;
                _DL.UpdateTime = DateTime.Now;
                _context.Update(_DL);
                    await _context.SaveChangesAsync();
                //After Update Send Email to the User...
                var _User = await _context.Users.FindAsync(drivingLicence.UserId);
                if(_User != null) { 
                await _emailSender.SendEmailAsync(_User.Email, "Car Share Service", $"Dear {_User.FirstName} Your Driving Licence has been validated successfully.");
                }
                ViewBag.Msg = "has been updated";
            }
            return View(drivingLicence);
        }

        private bool DrivingLicenceExists(int id)
        {
            return _context.DrivingLicences.Any(e => e.Id == id);
        }

        //Upload Image
        public IActionResult UploadDrivingLicenceImage(int Id)
        {
            if (Id.Equals(0))
            {
                return RedirectToAction("Error", "Home");
            }
            DrivingLicence model = new DrivingLicence
            {
                Id = Id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveDrivingLicenceImage(IFormCollection form, DrivingLicence _DrivingLicence)
        {
            POJOMsgs POJO = new POJOMsgs();
            try
            {
               // string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
                string _imgname = Guid.NewGuid().ToString();//Random String To chang the Image Name
                var webRoot = _env.WebRootPath;
                string storePath = webRoot + "/Image/";//The root to save the image
                if (form.Files == null || form.Files[0].Length == 0)
                    return RedirectToAction("Index");

                var filename = _imgname + form.Files[0].FileName;
                var name = form.Files[0].Name;
                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), storePath,
                            filename);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await form.Files[0].CopyToAsync(stream);
                }
                //Give  ImageThumbnail for DrivingLicence
                DrivingLicence model = await _context.DrivingLicences.FindAsync(_DrivingLicence.Id);
                model.UpdateTime = DateTime.Now;
             //   model.UserId = currentUserId;
                model.Image = "/Image/" + filename;
                _context.DrivingLicences.Update(model);
                await _context.SaveChangesAsync();

                POJO.Flag = true;
                POJO.Msg = "Has Been Added Successfully...";
                ViewBag.Msg = "Your Driving Licence has been updated";
            }
            catch (Exception e)
            {
                POJO.Flag = false;
                POJO.Msg = e.ToString();
                ViewBag.Msg = e.ToString();
            }
            return Json(POJO);
        }


        public async Task<IActionResult> Delete(int Id)
        {
            DrivingLicence model =await _context.DrivingLicences.FindAsync(Id);

            if(model != null)
            {
                        _context.DrivingLicences.Remove(model);
              await   _context.SaveChangesAsync();
             ViewBag.Mssg="Has Been Deleted...";
            }
            return RedirectToAction("Index");
        }





    }
}
