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
using System.Linq;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class BranchAddressController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;

        public BranchAddressController(ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
        }

        // GET: BranchAddress
        public async Task<IActionResult> Index()
        {
            return View(await _context.BranchAddresses.ToListAsync());
        }

        // GET: BranchAddress/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }
            //GEt the address by branchID
            var branchAddress = await _context.BranchAddresses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (branchAddress == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(branchAddress);
        }



        //By BranchId
        public async Task<IActionResult> DetailsByBranchId(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }
            //GEt the address by branchID
            var branchAddress = await _context.BranchAddresses
                .FirstOrDefaultAsync(m => m.BranchId == id);
            if (branchAddress == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View("Details", branchAddress);
        }

        // GET: BranchAddress/Create
        public IActionResult Create(int id)
        {
            if (id.Equals(0))
            {
                return RedirectToAction("Error", "Home");
            }
            BranchAddress model = new BranchAddress
            {
                BranchId = id
            };
            return View(model);
        }

        // POST: BranchAddress/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BranchId,PostCode,HouseNumber,Street,City,Country,County,Longitute,Latitue,CreateTime,UpdateTime,UserId")] BranchAddress branchAddress)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
                branchAddress.UserId = currentUserId;
                branchAddress.CreateTime = DateTime.Now;
                branchAddress.UpdateTime = DateTime.Now;
                _context.BranchAddresses.Add(branchAddress);
               await  _context.SaveChangesAsync();
                int Id = branchAddress.BranchId;//Return the Id after Add
                // return RedirectToAction(nameof(Index));
                //Redirect to Address page
                return RedirectToAction("Index", "Branch", new { id = Id });
            }
            return View(branchAddress);
        }

        // GET: BranchAddress/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var branchAddress = await _context.BranchAddresses.FindAsync(id);
            if (branchAddress == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(branchAddress);
        }

        // POST: BranchAddress/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BranchId,PostCode,HouseNumber,Street,City,Country,County,Longitute,Latitue,CreateTime,UpdateTime,UserId")] BranchAddress branchAddress)
        {
            if (id != branchAddress.Id)
            {
                return RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
                    BranchAddress model = await _context.BranchAddresses.FindAsync(id);
                    model.City = branchAddress.City;
                    model.Country = branchAddress.Country;
                    model.County = branchAddress.County;
                    model.HouseNumber = branchAddress.HouseNumber;
                    model.Latitue = branchAddress.Latitue;
                    model.Longitute = branchAddress.Longitute;
                    model.PostCode = branchAddress.PostCode;
                    model.Street = branchAddress.Street;
                    model.UpdateTime = DateTime.Now;
                    model.UserId = currentUserId;
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchAddressExists(branchAddress.Id))
                    {
                        return RedirectToAction("Error", "Home");
                    }
                     
                }
                return RedirectToAction(nameof(Index));
            }
            return View(branchAddress);
        }


        private bool BranchAddressExists(int id)
        {
            return _context.BranchAddresses.Any(e => e.Id == id);
        }

        //Get Address From API
        [HttpPost("/BranchAddress/GetAddressFromAPI")]
        public async Task<IActionResult> GetAddressFromAPI([FromBody] BranchAddressModelView value)
        {
            WebSiteSetting _WebsiteSetting =await _context.WebSiteSettings.FirstOrDefaultAsync();
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


        [HttpGet("/BranchAddress/DeleteBranchAddress/{id}")]
        public async Task<IActionResult> DeleteBranchAddress(int id)
        {
            POJOMsgs model = new POJOMsgs();

            try
            {
                BranchAddress _BranchAddress = await _context.BranchAddresses.FindAsync(id);
                _context.BranchAddresses.Remove(_BranchAddress);
                await _context.SaveChangesAsync();
                model.Flag = true;
                model.Msg = "Has Been Deleted it";
            }
            catch (Exception e)
            {
                model.Flag = false;
                model.Msg = e.ToString();
            }
            return Json(model);
        }


    }
}

