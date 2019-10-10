using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CPController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;
        private readonly RoleManager<IdentityRole> _RoleManager;

        public CPController(ApplicationDbContext context, UserManager<UserApplication> _UserManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = _UserManager;
            _RoleManager = roleManager;
        }
        public IActionResult Index()
        {
            //Last 24 Hours
            DateTime Last24Hours = DateTime.Now.Date.AddHours(-24);
            CPModelView model = new CPModelView()
            {
                NumberOfBranches = _context.Branches.Count(),
                NumberOfResevations = _context.Reservations.Where(p => p.CreateTime >= Last24Hours).Count(),//Last 24h reservations 
                NumberOfUsers = _context.Users.Count(),
                NumberOfVehicles = _context.Vehicles.Count(),
                NumberOfPendingDrivingLicence = _context.DrivingLicences.Where(dr => dr.IsValid == false).Count(),
                Top10Vehicles = (from p in _context.Vehicles orderby p.CreateTime descending select p).Take(10),
                Last10Orders = (from p in _context.Orders orderby p.CreateDate descending select p).Take(10).OrderBy(m => m.CreateDate)
            };

            return View(model);
        }
        public IActionResult AllUsers()
        {
            var _Users = from ur in _context.UserRoles
                         join u in _context.Users on ur.UserId equals u.Id
                         join r in _context.Roles on ur.RoleId equals r.Id
                         select new UserRoleModelView
                         {
                             UserId = u.Id,
                             UserName = u.FirstName + " " + u.LastName,
                             RoleName = r.Name,
                             RoleId = r.Id,
                             Email = u.Email
                         };
            return View(_Users);
        }
        public IActionResult AllRoles()
        {
            IQueryable<IdentityRole> _Roles = _context.Roles;
            return View(_Roles);
        }
        public IActionResult CreateRole()
        {
            IdentityRole data = new IdentityRole()
            {
                Id = "0"
            };
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(IdentityRole data)
        {
            if (ModelState.IsValid)
            {
                if (data.Id == "0")
                {
                    var _Role = new IdentityRole(data.Name);
                    await _RoleManager.CreateAsync(_Role);
                    ViewBag.Msg = "Your Rolebeen Added....";
                }
                else
                {
                    var _role = _context.Roles.Find(data.Id);
                    _role.Name = data.Name;
                    _context.Roles.Update(_role);
                    _context.SaveChanges();
                }
                return RedirectToAction("AllRoles", "CP");
            }
            return View(data);
        }
        public IActionResult EditRole(string id)
        {
            var model = _context.Roles.Find(id);
            return View("CreateRole", model);
        }

        public IActionResult UserDetails(string UserId)
        {
            var _User = from ur in _context.UserRoles.Where(u => u.UserId == UserId)
                        join u in _context.Users on ur.UserId equals u.Id
                        join r in _context.Roles on ur.RoleId equals r.Id
                        select new
                        {
                            UserId = u.Id,
                            UserName = u.FirstName,
                            RoleName = r.Name,
                            RoleId = r.Id
                        };


            return View(_User);
        }
        public IActionResult GetUserRole(string id)
        {

            var _User = from ur in _context.UserRoles.Where(u => u.UserId == id)
                        join u in _context.Users on ur.UserId equals u.Id
                        join r in _context.Roles on ur.RoleId equals r.Id
                        select new UserRoleModelView
                        {
                            UserId = u.Id,
                            UserName = u.FirstName,
                            RoleName = r.Name,
                            RoleId = r.Id
                        };
            var res = _User.FirstOrDefault();
            res.Roles = _context.Roles;
            return View(res);
        }
        [HttpPost]
        public IActionResult EditUserRole(UserRoleModelView data)
        {
            //get it  and remove it (coz of the primary key(userId,RoleId))
            var model = _context.UserRoles.FirstOrDefault(m => m.UserId == data.UserId);
            _context.UserRoles.Remove(model);
            _context.SaveChanges();

            //Add new role
            IdentityUserRole<string> addmodel = new IdentityUserRole<string>
            {
                UserId = data.UserId,
                RoleId = data.RoleId
            };
            _context.UserRoles.Add(addmodel);
            _context.SaveChanges();


            return RedirectToAction("AllUsers", "CP");
        }

        //for Email Settings
        public async Task<IActionResult> WebsiteEmailSettings()
        {
            EmailSetting _EmailSetting = await _context.EmailSettings.FirstOrDefaultAsync();
            return View(_EmailSetting);
        }

        [HttpPost]
        public async Task<IActionResult> WebsiteEmailSettings(EmailSetting _EmailSettingData)
        {
            if (_EmailSettingData == null)
            {
                return RedirectToAction("Error", "Home");
            }
            string currentUserId = _userManager.GetUserId(HttpContext.User); //Get UserId
            EmailSetting _EmailSettingCheck = await _context.EmailSettings.FirstOrDefaultAsync();

            if (_EmailSettingCheck == null)
            {
                EmailSetting _EmailSetting = new EmailSetting
                {
                    FromEmail = _EmailSettingData.FromEmail,
                    UserName = _EmailSettingData.UserName,
                    Password = _EmailSettingData.Password,
                    Smtp = _EmailSettingData.Smtp,
                    Port = _EmailSettingData.Port,
                    SSL = _EmailSettingData.SSL,
                    UserId = currentUserId,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                await _context.EmailSettings.AddAsync(_EmailSetting);
                await _context.SaveChangesAsync();
                return View(_EmailSetting);
            }
            else
            {
                _EmailSettingCheck.FromEmail = _EmailSettingData.FromEmail;
                _EmailSettingCheck.UserName = _EmailSettingData.UserName;
                _EmailSettingCheck.Password = _EmailSettingData.Password;
                _EmailSettingCheck.Smtp = _EmailSettingData.Smtp;
                _EmailSettingCheck.Port = _EmailSettingData.Port;
                _EmailSettingCheck.SSL = _EmailSettingData.SSL;
                _EmailSettingCheck.UpdateTime = DateTime.Now;
                _EmailSettingCheck.UserId = currentUserId;
                _context.EmailSettings.Update(_EmailSettingCheck);
                await _context.SaveChangesAsync();
                return View(_EmailSettingCheck);
            }
        }

        public async Task<IActionResult> AdminSettings()
        {
            WebSiteSetting _Setting = await _context.WebSiteSettings.FirstOrDefaultAsync();
            return View(_Setting);
        }

        //for settings
        [HttpPost]
        public async Task<IActionResult> AdminSettings(WebSiteSetting _settingdata)
        {
            if (_settingdata == null)
            {
                return RedirectToAction("Error", "Home");
            }

            string currentUserId = _userManager.GetUserId(HttpContext.User); //Get UserId
            WebSiteSetting _SettingCheck = await _context.WebSiteSettings.FirstOrDefaultAsync();

            if (_SettingCheck == null)
            {
                WebSiteSetting _settingAdd = new WebSiteSetting
                {
                    AddressAPI = _settingdata.AddressAPI,
                    DVLAAPI = _settingdata.DVLAAPI,
                    PaymentAPI = _settingdata.PaymentAPI,
                    MobileApiKey = _settingdata.MobileApiKey,
                    MobileApiSecret = _settingdata.MobileApiSecret,
                    MobileWebsiteName = _settingdata.MobileWebsiteName,
                    UserId = currentUserId,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                await _context.WebSiteSettings.AddAsync(_settingAdd);
                await _context.SaveChangesAsync();
                return View(_settingAdd);
            }
            else
            {
                _SettingCheck.Id = _settingdata.Id;
                _SettingCheck.AddressAPI = _settingdata.AddressAPI;
                _SettingCheck.DVLAAPI = _settingdata.DVLAAPI;
                _SettingCheck.PaymentAPI = _settingdata.PaymentAPI;
                _SettingCheck.MobileApiKey = _settingdata.MobileApiKey;
                _SettingCheck.MobileApiSecret = _settingdata.MobileApiSecret;
                _SettingCheck.MobileWebsiteName = _settingdata.MobileWebsiteName;
                _SettingCheck.UserId = currentUserId;
                _context.WebSiteSettings.Update(_SettingCheck);
                await _context.SaveChangesAsync();
                return View(_SettingCheck);
            }
        }

        public async Task<IActionResult> BankTransactions()
        {
            return View(await _context.BankTransactions.OrderBy(m => m.Date).ToListAsync());
        }

        //Show the paid order in cp
        public IActionResult OrdersPaid()
        {
            IQueryable<Order> _Orders = _context.Orders.Where(ord => ord.IsPaid == true).OrderBy(m => m.CreateDate);
            return View(_Orders);

        }
        //show the unpiad order in the cp
        public IActionResult OrdersUnPaid()
        {
            IQueryable<Order> _Orders = _context.Orders.Where(ord => ord.IsPaid == false).OrderBy(m => m.CreateDate);
            return View(_Orders);
        }

        public async Task<IActionResult> UserAddress(string Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Error", "Home");
            }
            Address model = await _context.Addresses.FirstOrDefaultAsync(n => n.UserId == Id);
            return View("MyAddress", model);
        }


        public async Task<IActionResult> UserDrivingLicence(string Id)
        {

            DrivingLicence _DrivingLicence = new DrivingLicence();
            _DrivingLicence = await _context.DrivingLicences.FirstOrDefaultAsync(m => m.UserId == Id);
            if (_DrivingLicence == null)
            {
                DrivingLicence _drivingLicence = new DrivingLicence();
                _drivingLicence.Image = "0";
                return View(_drivingLicence);
            }
            return View("MyDrivingLicence", _DrivingLicence);
        }
        public IActionResult ReportChart()
        {
            return View();
        }
        [HttpPost("/CP/GetReportChart")]
        public IActionResult GetReportChart([FromBody]ReportChart aYear)
        {
            int Year = Convert.ToInt32(aYear.aYear);
            var _Reservation = _context.Reservations.Where(re => re.CreateTime.Year == Year);
            var ResG = _Reservation.GroupBy(l => l.CreateTime.Month)
                .Select(c1 => new Reservation { CreateTime = c1.First().CreateTime, Id = c1.Count() }).ToList();
            var chartData = new object[ResG.Count() + 1];

            chartData[0] = new object[]{
                    "Monthly",
                    "Booking"
                };

            int j = 0;
            foreach (var i in ResG)
            {
                j++;
                chartData[j] = new object[] { CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i.CreateTime.Month), i.Id };
            }



            return Ok(chartData);
        }

        public IActionResult GetReportChartTimeLine()
        {

            return View();
        }
        [HttpPost("/CP/GetReportChartTimeLine")]
        public IActionResult GetReportChartTimeLine([FromBody]ReportChart aYear)
        {

            IList<BookingTimeChartModelView> _LstBookingTimeModelViews = new List<BookingTimeChartModelView>();
            //Get the Availability tb for current vehicle(List) in the current date..
            IQueryable<VehicleAvailability> _VehicleAvailability = _context.VehicleAvailabilities.Where(e => e.Date == aYear.Date);
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
                // Array.Sort(_MyBookedTimeArrayStr);
                Queue<int> q = new Queue<int>();
                //convert to int list
                for (int i = 0; i < _MyBookedTimeArrayStr.Length; i++)
                {
                    q.Enqueue(Convert.ToInt32(_MyBookedTimeArrayStr[i]));
                }
                while (q.Count() > 0)
                {
                    BookingTimeChartModelView _bookingtime = new BookingTimeChartModelView()
                    {
                        //Get the first time slid
                        VehicleId = _vehicleAvailability.VehicleId,
                        Date = _vehicleAvailability.Date,
                        PickUpTime = q.Dequeue(),
                        DropOffTime = q.Dequeue()
                    };
                    //Add to the main list
                    _LstBookingTimeModelViews.Add(_bookingtime);
                }
            }

            //joing with vehicle to get the model of the car...
            var result = from lst in _LstBookingTimeModelViews
                         join ve in _context.Vehicles on lst.VehicleId equals ve.Id
                         select new
                         {
                             registration = ve.Registration,
                             date = lst.Date,
                             pickuptime = lst.PickUpTime,
                             dropofftime = lst.DropOffTime
                         };


            var chartData = new object[result.Count()];
            int j = 0;
            foreach (var i in result)
            {

                chartData[j] = new object[] { i.registration.ToString(), i.date, i.pickuptime, i.dropofftime };
                j++;
            }
            return Ok(chartData);
        }

        //Show the vehicles location on google map
        public IActionResult GetVehcielsLocations()
        {
            FindVehicleModelView model = new FindVehicleModelView();
            //after get the available cars, need to find there currrent location from Vehicle cuurert TB.             
            IList<AvailVehicleWithLocationAndBookData> AvailableVehicleWithLocation = (from se in _context.VehicleCurrentLocations
                                                                                       join ba in _context.Vehicles on se.VehicleId equals ba.Id
                                                                                       select new AvailVehicleWithLocationAndBookData
                                                                                       {
                                                                                           VehicleId = ba.Id,
                                                                                           VehicleModel = ba.Model,
                                                                                           VehiclePricePH = ba.PricePerHour,
                                                                                           VehicleFuel = ba.FuelType,
                                                                                           VehicleTransmission = ba.Transmission,
                                                                                           VehicleYear = ba.YearOfManufacture,
                                                                                           VehicleImage = ba.Image,
                                                                                           VehicleRating = ba.Rating,
                                                                                           VehicleCurrentLatitue = se.Latitue,
                                                                                           VehicleCurrentLongitute = se.Longitute

                                                                                       }).ToList();


            //Building google map....  Need solo for it
            string markers = "[";
            foreach (var item in AvailableVehicleWithLocation)
            {

                //Need to Pass <Form>
                StringBuilder sb = new StringBuilder();
                sb.Append("<div id=content><div id=siteNotice><h3>");
                sb.Append(item.VehicleModel);
                sb.Append("</h3><p>Price £ ");
                sb.Append(item.VehiclePricePH);
                sb.Append(" P/h</p><p>Rating:<img src=/Image/CarShare/ratingstar.jpg width=20/> ");
                sb.Append(item.VehicleRating);           

                sb.Append("</p></div><img class=img-thumbnail style=width:150px; src=");
                sb.Append(item.VehicleImage);
                sb.Append(" /><p> Year: ");
                sb.Append(item.VehicleYear);
                sb.Append("</p><div id=bodyContent><p>");
                sb.Append("<a type=button class=btn-primary href=/Vehicle/Details/");
                sb.Append(item.VehicleId);
                sb.Append("> Details</a></p> </div></div>");


                markers += "{";
                markers += string.Format("'lat': '{0}',", item.VehicleCurrentLatitue);
                markers += string.Format("'lng': '{0}',", item.VehicleCurrentLongitute);
                markers += string.Format("'text': '{0}',", sb);
                markers += "},";

            }
            markers += "];";
            //Ends Google maps
            model.Marker = markers;

            return View(model);
        }
    }
}