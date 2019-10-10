using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;
        private IHostingEnvironment _env;

        public VehicleController(IHostingEnvironment env, ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
            _env = env;

        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Vehicles.ToListAsync());
        }
        [AllowAnonymous]
        public async Task<IActionResult> Vehicles()
        {
            return View(await _context.Vehicles.ToListAsync());
        }

        //Using External API to Get the Vehicle Details
        [HttpGet("/Vehicle/GetDataFromDvlaSearchAPI/{id}")]
        public async Task<IActionResult> GetDataFromDvlaSearchAPI(string id)
        {
            if (_context.Vehicles.Any(m => m.Registration == id))
            {
                return Json(false);
            }
            WebSiteSetting _WebSiteSetting =await _context.WebSiteSettings.FirstOrDefaultAsync();
            string DVLAKey = _WebSiteSetting.DVLAAPI;
            //DvlaSearchDemoAccount
            HttpClient _client = new HttpClient();
            string _Url = "https://dvlasearch.appspot.com/DvlaSearch?apikey="+ DVLAKey + "&licencePlate=" + id;
            Vehicle _Vehicle = new Vehicle();
            HttpResponseMessage  response = await _client.GetAsync(_Url);
            if (response.IsSuccessStatusCode)
            {
                _Vehicle = await response.Content.ReadAsAsync<Vehicle>();
                _Vehicle.Registration = id;
                return Json(_Vehicle);
            }
            return Json(false);
        }

        [AllowAnonymous]
        // GET: Vehicles/Details/5
        //get the vehicle with reviews join
        public async Task<IActionResult> VehicleDetails(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            VehicleModelView _VehicleModelView = new VehicleModelView();
            Vehicle _Vehicle= await _context.Vehicles.FindAsync(id);
            _VehicleModelView.Id = _Vehicle.Id;
            _VehicleModelView.VIN = _Vehicle.VIN;
            _VehicleModelView.Registration = _Vehicle.Registration;
            _VehicleModelView.Model = _Vehicle.Model;
            _VehicleModelView.CylinderCapacity = _Vehicle.CylinderCapacity;
            _VehicleModelView.NumberOfDoors = _Vehicle.NumberOfDoors;
            _VehicleModelView.SixMonthRate = _Vehicle.SixMonthRate;
            _VehicleModelView.TwelveMonthRate = _Vehicle.TwelveMonthRate;
            _VehicleModelView.DateOfFirstRegistration = _Vehicle.DateOfFirstRegistration;
            _VehicleModelView.YearOfManufacture = _Vehicle.YearOfManufacture;
            _VehicleModelView.Co2Emissions = _Vehicle.Co2Emissions;
            _VehicleModelView.FuelType = _Vehicle.FuelType;
            _VehicleModelView.TaxStatus = _Vehicle.TaxStatus;
            _VehicleModelView.Transmission = _Vehicle.Transmission;
            _VehicleModelView.Colour = _Vehicle.Colour;
            _VehicleModelView.TypeApproval = _Vehicle.TypeApproval;
            _VehicleModelView.WheelPlan = _Vehicle.WheelPlan;
            _VehicleModelView.RevenueWeight = _Vehicle.RevenueWeight;
            _VehicleModelView.TaxDetails = _Vehicle.TaxDetails;
            _VehicleModelView.MotDetails = _Vehicle.MotDetails;
            _VehicleModelView.Taxed = _Vehicle.Taxed;
            _VehicleModelView.Mot = _Vehicle.Mot;          
            _VehicleModelView.UserId = _Vehicle.UserId;
            _VehicleModelView.PricePerHour = _Vehicle.PricePerHour;
            _VehicleModelView.Image = _Vehicle.Image;
            _VehicleModelView.Rating = _Vehicle.Rating;
            _VehicleModelView.VehicleReviews = _context.Reviews.Where(vid=>vid.VehicleId==id);         

             

            return View(_VehicleModelView);
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VIN,Registration,Model,CylinderCapacity,NumberOfDoors,SixMonthRate,TwelveMonthRate,DateOfFirstRegistration,YearOfManufacture,Co2Emissions,FuelType,TaxStatus,Transmission,Colour,TypeApproval,WheelPlan,RevenueWeight,TaxDetails,MotDetails,Taxed,Mot,CreateTime,UpdateTime,UserId,Image,PricePerHour")] Vehicle vehicle)
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            vehicle.UserId = currentUserId;
            vehicle.UpdateTime = DateTime.Now;
            vehicle.CreateTime = DateTime.Now;
            vehicle.Image = "/Image/CarShare/Empty_Car_Logo.jpg";//default Image

            if (ModelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VIN,Registration,Model,CylinderCapacity,NumberOfDoors,SixMonthRate,TwelveMonthRate,DateOfFirstRegistration,YearOfManufacture,Co2Emissions,FuelType,TaxStatus,Transmission,Colour,TypeApproval,WheelPlan,RevenueWeight,TaxDetails,MotDetails,Taxed,Mot,CreateTime,UpdateTime,UserId,Image,PricePerHour")] Vehicle vehicle)
        {

            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId  

            if (id != vehicle.Id)
            {
                return RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {

                    Vehicle _Vehicle = await _context.Vehicles.FindAsync(id);
                    _Vehicle.VIN = vehicle.VIN;
                    _Vehicle.Registration = vehicle.Registration;
                    _Vehicle.Model = vehicle.Model;
                    _Vehicle.CylinderCapacity = vehicle.CylinderCapacity;
                    _Vehicle.NumberOfDoors = vehicle.NumberOfDoors;
                    _Vehicle.SixMonthRate = vehicle.SixMonthRate;
                    _Vehicle.TwelveMonthRate = vehicle.TwelveMonthRate;
                    _Vehicle.DateOfFirstRegistration = vehicle.DateOfFirstRegistration;
                    _Vehicle.YearOfManufacture = vehicle.YearOfManufacture;
                    _Vehicle.Co2Emissions = vehicle.Co2Emissions;
                    _Vehicle.FuelType = vehicle.FuelType;
                    _Vehicle.TaxStatus = vehicle.TaxStatus;
                    _Vehicle.Transmission = vehicle.Transmission;
                    _Vehicle.Colour = vehicle.Colour;
                    _Vehicle.TypeApproval = vehicle.TypeApproval;
                    _Vehicle.WheelPlan = vehicle.WheelPlan;
                    _Vehicle.RevenueWeight = vehicle.RevenueWeight;
                    _Vehicle.TaxDetails = vehicle.TaxDetails;
                    _Vehicle.MotDetails = vehicle.MotDetails;
                    _Vehicle.Taxed = vehicle.Taxed;
                    _Vehicle.Mot = vehicle.Mot;
                    _Vehicle.UpdateTime = DateTime.Now;
                    _Vehicle.UserId = currentUserId;
                    _Vehicle.PricePerHour = vehicle.PricePerHour;
                    _context.Update(_Vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
                    {
                        return RedirectToAction("Error", "Home");
                    }
                    
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }

        //Upload Image
        public IActionResult UploadVehicleImage(int Id)
        {
            if (Id.Equals(0))
            {
                return RedirectToAction("Error", "Home");
            }
            Vehicle model = new Vehicle
            {
                Id = Id
            };
            return View(model);
        }
        [HttpGet("/Vehicle/DeleteVehicle/{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            POJOMsgs model = new POJOMsgs();
           
            try
            {
                Vehicle _Vehicle =await _context.Vehicles.FindAsync(id);
                _context.Vehicles.Remove(_Vehicle);
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



        [HttpPost]
        public async Task<IActionResult> SaveVehicleImage(IFormCollection form, Vehicle _Vehicle)
        {
            POJOMsgs POJO = new POJOMsgs();
            try { 
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
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

            //Give  ImageThumbnail for Vehicle
            Vehicle model = await _context.Vehicles.FindAsync(_Vehicle.Id);
            model.UpdateTime = DateTime.Now;
            model.UserId = currentUserId;
            model.Image = "/Image/" + filename;
            await _context.SaveChangesAsync();

                POJO.Flag = true;
                POJO.Msg = "Has Been Added Successfully...";
            }
            catch(Exception e)
            {
                POJO.Flag = false;
                POJO.Msg = e.ToString();
            }
            return Json(POJO);

        }


        //Set the Vehicle Location (id= VehicleId
        public async Task<IActionResult> VehicleLocation(int id)
        {
            VehicleCurrentLocation _VehicleCurrentLocation =await _context.VehicleCurrentLocations.FirstOrDefaultAsync(m=>m.VehicleId ==id);
            if (_VehicleCurrentLocation == null)
            {
                VehicleCurrentLocation model = new VehicleCurrentLocation
                {
                    Id = 0,
                    VehicleId = id
                };
                return View(model);
            }
            return View(_VehicleCurrentLocation);
        }
        //Add or update Location
        public async Task<IActionResult> VehicleLocationAddUpdate(VehicleCurrentLocation data)
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId

            if (ModelState.IsValid){

                if (data.Id.Equals(0))//add new record
                {
                    data.UpdateTime = DateTime.Now;
                    data.CreateTime = DateTime.Now;
                    data.UserId = currentUserId;

                  await  _context.VehicleCurrentLocations.AddAsync(data);
                    ViewBag.Msg = "Your Vehicle Current Location has been Added....";

                }
                else//update 
                {
                    VehicleCurrentLocation _VehicleCurrentLocation =await _context.VehicleCurrentLocations.FindAsync(data.Id);
                    _VehicleCurrentLocation.Latitue = data.Latitue;
                    _VehicleCurrentLocation.Longitute = data.Longitute;
                    _VehicleCurrentLocation.UpdateTime = data.UpdateTime;
                    _VehicleCurrentLocation.UserId = currentUserId;

                    _context.VehicleCurrentLocations.Update(_VehicleCurrentLocation);

                    ViewBag.Msg = "Your Vehicle Current Location  has been Updated....";
                }
             await   _context.SaveChangesAsync();
                return View("VehicleLocation", data);
            }

            return View("VehicleLocation",data);
        }

        //Date Should be for coming date not past.....
        public IActionResult VehicleBookingTime(int Id)
        {
            IList<BookingTimeModelView> _LstBookingTimeModelViews = new List<BookingTimeModelView>();
            //Get the Availability tb for current vehicle(List) in the current date..
            IQueryable<VehicleAvailability> _VehicleAvailability = _context.VehicleAvailabilities.Where(e => e.VehicleId == Id && e.Date >= DateTime.Now.Date);
            foreach (var _vehicleAvailability in _VehicleAvailability)
            {
                //Get the booking row from db
                string _MyBookedTime = _vehicleAvailability.BookingTime;
                //Split to string
                string[] _MyBookedTimeArrayStr = _MyBookedTime.Split(",");
                //Remove the white or empty index
                _MyBookedTimeArrayStr = _MyBookedTimeArrayStr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                _MyBookedTimeArrayStr = _MyBookedTimeArrayStr.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                //sort it
                Array.Sort(_MyBookedTimeArrayStr);
                Queue<int> q = new Queue<int>();
                //convert to int list
                for (int i = 0; i < _MyBookedTimeArrayStr.Length; i++)
                {
                    q.Enqueue(Convert.ToInt32(_MyBookedTimeArrayStr[i]));
                }
                while (q.Count() > 0)
                {
                    BookingTimeModelView _bookingtime = new BookingTimeModelView()
                    {
                        //Get the first time slid
                        Date = _vehicleAvailability.Date,
                        PickUpTime = q.Dequeue(),
                        DropOffTime = q.Dequeue()
                    };
                    //Add to the main list
                    _LstBookingTimeModelViews.Add(_bookingtime);
                }
            }
            return View(_LstBookingTimeModelViews);
        }

    }
}
