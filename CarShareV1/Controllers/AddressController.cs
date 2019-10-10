using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using getAddress.Sdk;
using getAddress.Sdk.Api.Requests;
using getAddress.Sdk.Api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;

        public AddressController(ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> MyAddress()
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            if (currentUserId == null)
            {
                return RedirectToAction("Error", "Home");
            }
            CarShare.Data.Address model = await _context.Addresses.FirstOrDefaultAsync(n => n.UserId== currentUserId);
            return View(model);
        }
       
        public async Task<IActionResult> UpdateCreate(CarShare.Data.Address data)
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            
                if(data.Id.Equals(0))//Add NEw
                {
                    data.UpdateTime = DateTime.Now;
                    data.CreateTime = DateTime.Now;
                    data.UserId = currentUserId;
                    await _context.Addresses.AddAsync(data);

                }else //Update
                {
                    CarShare.Data.Address _address =await _context.Addresses.FindAsync(data.Id);
                    _address.PostCode = data.PostCode;
                    _address.HouseNumber = data.HouseNumber;
                    _address.Street = data.Street;
                    _address.City = data.City;
                    _address.Country = data.Country;
                    _address.County = data.County;
                    _address.Longitute = data.Longitute;
                    _address.Latitue = data.Latitue;
                    _address.UpdateTime = DateTime.Now;

                    _context.Addresses.Update(_address);
                   }
                await _context.SaveChangesAsync();
                ViewBag.Msg = "Your Address has been updated";
                return View("myaddress", data);
             
        }

        //Get Address From API
        [HttpPost("/Address/GetMyAddressFromAPI")]
        public async Task<IActionResult> GetMyAddressFromAPI([FromBody] CarShare.Data.Address value)
        {
            WebSiteSetting _WebsiteSetting = await _context.WebSiteSettings.FirstOrDefaultAsync();
            string APIMyKey = _WebsiteSetting.AddressAPI;
            //Rs_5XMJYb0aGFo6VJ6Xb4w17847
            BranchAddress Myaddress = new BranchAddress();
            // var WebsiteSetting = _context.Settings.FirstOrDefault();
            var apiKey = new ApiKey(APIMyKey);

            using (var api = new GetAddesssApi(apiKey))
            {
                var result = await api.Address.Get(new GetAddressRequest(value.PostCode, value.HouseNumber));

                if (result.IsSuccess)
                {
                    var successfulResult = (GetAddressResponse.Success)result;

                    var latitude = successfulResult.Latitude;

                    var Longitude = successfulResult.Longitude;

                    foreach (var address in successfulResult.Addresses)
                    {
                        Myaddress.Street = address.Line1 + " " + address.Line2 + " " + address.Line3 + " " + address.Line4;
                        Myaddress.City = address.TownOrCity;
                        Myaddress.County = address.County;
                        Myaddress.PostCode = value.PostCode;
                        Myaddress.HouseNumber = value.HouseNumber;
                        Myaddress.Country = "UK";
                        Myaddress.Longitute = Longitude;
                        Myaddress.Latitue = latitude;
                    }
                    return Json(Myaddress);
                }
                POJOMsgs model = new POJOMsgs
                {
                    Flag = false,
                    Msg = result.FailedResult.Raw.ToString()
                };
                return Json(model);
            }
        }


    }
}