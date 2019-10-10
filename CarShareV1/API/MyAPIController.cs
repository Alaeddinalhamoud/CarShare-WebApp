using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarShareV1.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyAPIController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;

        public MyAPIController(ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
        }

        [HttpGet("GetServiceFavoriteCheck")]
        public IActionResult GetServiceFavoriteCheck([FromQuery]int id,string userid)
        {
            POJOMsgs model = new POJOMsgs();
            string currentUserId = userid;                     
            bool _IsMyFavorite =  _context.Favorites.Any(m => m.VehicleId == id && m.UserId == currentUserId);
            if (_IsMyFavorite)
            {
                model.Flag = true;
                model.Msg = "available in db";
                return Ok(model);
            }
            else
            {
                model.Flag = false;
                model.Msg = "not available in db";
                return Ok(model);
            }
        }

        [HttpGet("AddToServiceWishList")]
        public async Task<IActionResult> AddToServiceWishList([FromQuery]int id, string userid)
        {
            POJOMsgs model = new POJOMsgs();
            string currentUserId = userid;         
            Favorite _Favorite = await _context.Favorites.FirstOrDefaultAsync(m => m.VehicleId == id && m.UserId == currentUserId);
            if (_Favorite != null)
            {
                model.Flag = true;
                model.Msg = " available in db and we deleted it.";
                _context.Favorites.Remove(_Favorite);
            }
            else
            {

                Favorite _FavoriteAdd = new Favorite
                {
                    VehicleId = id,
                    UserId = currentUserId,
                    UpdateTime = DateTime.Now,
                    CreateTime = DateTime.Now
                };
                await _context.Favorites.AddAsync(_FavoriteAdd);
                model.Flag = false;
                model.Msg = "not available in db and we added it.";
            }
            await _context.SaveChangesAsync();
            return Ok(model);
        }



        //Check if he cutomer if he has reviewed the car or no .. if yes dont show the rew box
        //and check if the customer had booking for this car,,, only reviwe he rent...
        [HttpGet("CheckCustomerReview")]
        public IActionResult CheckCustomerReview([FromQuery]int id, string userid)
        {
            string currentUserId = userid;//Get UserId
            POJOMsgs model = new POJOMsgs();
            //Check if he booked the car.
            bool _IsMyReservation = _context.Reservations.Any(us => us.UserId == currentUserId && us.VehicleId == id);
            if (_IsMyReservation)
            {
                Review _Reviews = _context.Reviews.FirstOrDefault(m => m.VehicleId == id && m.UserId == currentUserId);
                if (_Reviews != null)
                {
                    model.Flag = true;
                    model.Msg = "You have already reviewed this Car.";
                    return Ok(model);
                }
                else
                {
                    model.Flag = false;
                    model.Msg = "You dont have reviewe for this Car.";
                    return Ok(model);
                }
            }
            else
            {
                //true to disable the review box
                model.Flag = true;
                model.Msg = "You dont have right to reviewe  this Car, you need to book it first.";
                return Ok(model);
            }
        }


        //Posting Review
        [HttpPost("SubmitReview")]
        public async Task<IActionResult> SubmitReview([FromBody] Review _review)
        {
            POJOMsgs model = new POJOMsgs();
            string currentUserId = _review.UserId;//Get UserId            
            Review _Review = new Review();
            try
            {
                _Review.Rating = _review.Rating;
                _Review.UserId = currentUserId;
                _Review.Description = _review.Description;
                _Review.VehicleId = _review.VehicleId;
                _Review.CreateTime = DateTime.Now;
                _Review.UpdateTime = DateTime.Now;
                await _context.Reviews.AddAsync(_Review);
                await _context.SaveChangesAsync();
                model.Flag = true;
                model.Msg = "has Been Added";

                //Calc it with the last rate value avg.(Future Method)
                IQueryable<Review> _Reviews = _context.Reviews.Where(r => r.VehicleId == _review.VehicleId);
                double AverageRating = await _Reviews.AverageAsync(m => m.Rating);
                Vehicle _Vehicle = await _context.Vehicles.FindAsync(_review.VehicleId);
                _Vehicle.Rating = AverageRating;
                _Vehicle.UpdateTime = DateTime.Now;
                _context.Vehicles.Update(_Vehicle);
                await _context.SaveChangesAsync();
                //Calc it with the last rate value avg.
            }
            catch (Exception e)
            {
                model.Flag = false;
                model.Msg = e.ToString();
            }
            return Ok(model);
        }


        [HttpGet("DeleteFavorite")]
        public async Task<IActionResult> DeleteFavorite([FromQuery]int id, string userid)
        {
            POJOMsgs model = new POJOMsgs();
            string currentUserId = userid;
            Favorite favorite = new Favorite();
            try
            {                
                Favorite fav = await _context.Favorites.FirstOrDefaultAsync(m => m.Id == id);
                if (currentUserId == fav.UserId)
                {
                    _context.Favorites.Remove(fav);
                    await _context.SaveChangesAsync();
                    model.Flag = true;
                    model.Msg = "Has Been Deleted it";
                }
                else
                {
                    model.Flag = true;
                    model.Msg = "Has Been Deleted it";
                    return Ok(model);
                }
            }
            catch (Exception e)
            {
                model.Flag = false;
                model.Msg = e.ToString();
            }
            return Ok(model);
        }


        ////Delete DL
        //[HttpGet("DeleteDrivingLicence")]
        //public JsonResult DeleteDrivingLicence([FromQuery] int id)
        //{
        //    POJOMsgs model = new POJOMsgs();
        //         DrivingLicence _DrivingLicence =   _context.DrivingLicences.Find(id);
        //    try
        //    {
        //      //  _context.DrivingLicences.Remove(_DrivingLicence);
        //        //  _context.SaveChanges();
        //        model.Flag = true;
        //        model.Msg = "Has Been Deleted it";
        //    }
        //    catch (Exception e)
        //    {
        //        model.Flag = false;
        //        model.Msg = e.ToString();
        //    }
        //    return new JsonResult (model);
        //}


    }
}