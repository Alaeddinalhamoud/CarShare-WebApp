using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;
        private readonly IEmailSender _emailSender;

        public ProfileController(ApplicationDbContext context, UserManager<UserApplication> _UserManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = _UserManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            

            return View();
        }
        //To guide the user to the car location
        public async Task<IActionResult> WalkMe(int Id)
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            Reservation _Reservation = await _context.Reservations.FindAsync(Id);
            VehicleCurrentLocation _VehicleCurrentLocation = await _context.VehicleCurrentLocations.FirstOrDefaultAsync(ca => ca.VehicleId == _Reservation.VehicleId);
            return View(_VehicleCurrentLocation);
        }

        //Drive set the car in new location ... check it there is delay
        public async Task<IActionResult> DriveMe(int Id)
        {
            Reservation _Reservation = await _context.Reservations.FindAsync(Id);
            //Need to check the booking date is same time and date today 

            var NowDate = DateTime.Now;
            var OneHoureExtra = NowDate;
            //To check the time is not 12 mid night  24 >= 23 & 23<= (24+1)=0 new day
            if (NowDate.Hour < 23)
            {
                OneHoureExtra = DateTime.Now.AddHours(1);
            }
           
            //just add hours to the pickup date.. concate
            var _ReservationeDate = _Reservation.PickUpDate.AddHours(Convert.ToDouble(_Reservation.PickUpTime));
            //if the now date == pickup date
            if(NowDate.Date == _ReservationeDate.Date)
            {
                //need to check the time is equal or within not greater....(need work)
                if (NowDate.Hour >= _ReservationeDate.Hour && _ReservationeDate.Hour<=OneHoureExtra.Hour)
                {
                    //go to driving page
                    return View(_Reservation);
                }
                //retrun to error not ur date 
                ErrorViewModel error = new ErrorViewModel()
                {
                    ErrorMsg = "Dear User, Your Booking Hour is not Now."
                };
                //Error page if payment not sucssefuly done
                return RedirectToAction("Error", "Home", error);
            }
            else
            {
                //retrun to error not ur date 
                ErrorViewModel error = new ErrorViewModel()
                {
                    ErrorMsg = "Dear User, Your Booking Date is not Today."
                };
                //Error page if payment not sucssefuly done
                return RedirectToAction("Error", "Home", error);
            }

             

        }
        [HttpPost]
        public async Task<IActionResult> DriveMe(Reservation data)
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            var WebSiteName = Request.Host.Value;
            //need to update vaicalecurrentlocaion 
            VehicleCurrentLocation _VehicleCurrentLocation = await _context.VehicleCurrentLocations
                .FirstOrDefaultAsync(u => u.VehicleId == data.VehicleId);

            _VehicleCurrentLocation.Latitue = data.ReturnLocationLatitue;
            _VehicleCurrentLocation.Longitute = data.ReturnLocationLongitute;
            _VehicleCurrentLocation.UpdateTime = DateTime.Now;
            _VehicleCurrentLocation.UserId = currentUserId;
            _context.VehicleCurrentLocations.Update(_VehicleCurrentLocation);
            await _context.SaveChangesAsync();

            //sending email to the user to give feedback about the journ
            var _User = await _context.Users.FindAsync(currentUserId);           
            string _Feedbacklink = HtmlEncoder.Default.Encode($"//{WebSiteName}/Vehicle/VehicleDetails/{data.VehicleId}");
            string _EmailReciptsBody = $"Dear {_User.FirstName} , thank you for using our service," +
                $" Please, give ur your feedback about the service on the follwing link " +
                $" <a href={_Feedbacklink}> Feedback </a>";
            await _emailSender.SendEmailAsync(_User.Email, "Car Share Service", _EmailReciptsBody);

            return RedirectToAction("Index","Home");
        }

        [AllowAnonymous]
        public IActionResult ActivationEmail()
        {

            return View();
        }
        [AllowAnonymous]
        public IActionResult IsNotAllowed()
        {

            return View();
        }

        public async Task<IActionResult> MyProfile()
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            UserApplication _User =await _context.Users.FindAsync(currentUserId);
            return View(_User);
        }


        public async Task<IActionResult> UpdateMyProfile(UserApplication data)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
                UserApplication model = await _context.Users.FindAsync(currentUserId);
                model.FirstName = data.FirstName;
                model.LastName = data.LastName;
                model.DOB = data.DOB;
                model.PhoneNumber = data.PhoneNumber;
                model.UserName = data.UserName;
                model.Gender = model.Gender;

                _context.Users.Update(model);
                await _context.SaveChangesAsync();
                ViewBag.Msg = "Your profile has been updated";
                return View("MyProfile",model);
            }
            else
            {
                ViewBag.Msg = "Please, fill the required data...";
                return View("MyProfile", data);
            }

           
        }
    }
}