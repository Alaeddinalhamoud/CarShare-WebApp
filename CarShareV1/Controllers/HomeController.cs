using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarShareV1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
        }
        //List of car ,, need more enhance to show the address with the car details
        public IActionResult FindCarInList( FindVehicleModelView model)
        {
            //Get All the Cars.
            IQueryable<Vehicle> _Vehicles = _context.Vehicles;
            //List to store the Availabe cars
            List<Vehicle> AvailableVehicle = new List<Vehicle>();
            //Current location of the customer(PickUp)
            double CustomerLatitude=Convert.ToDouble(model.PickLatitude);
            double CustomerLongitude = Convert.ToDouble(model.PickLongitude);
            //DropOff Location
            double DropOffLatitude = Convert.ToDouble(model.DropLatitude);
            double DropOffLongitude = Convert.ToDouble(model.DropLongitude);

            //loop for cars
            foreach (var item in _Vehicles)
            {
                //temp list for current car
                Vehicle _Vehicle = new Vehicle();
                //Get the Availability tb for current vehicle(List) in the current date..
                IQueryable<VehicleAvailability> _VehicleAvailability = _context.VehicleAvailabilities.Where(e => e.VehicleId == item.Id && e.Date==model.PickUpDate);
                
              //  bool IsExist = _VehicleAvailability.Any(a => a.Id == item.Id);
                //if the vehicle not exist in the reservation tb...
                if (!_VehicleAvailability.Any())
                {
                    AvailableVehicle.Add(item);
                }
                else//if its exist in the db in this  date loop it
                {              

                foreach (var _vehicleAvailability in _VehicleAvailability)
                {                    
                        //Get the booking row from db
                        string _MyBookedTime = _vehicleAvailability.BookingTime;

                        //Split to string
                        string [] _MyBookedTimeArrayStr =_MyBookedTime.Split(",");
                        //sort it
                        Array.Sort(_MyBookedTimeArrayStr);
                      
                        Queue<int> q = new Queue<int>();
                        //convert to int list
                        for (int i = 0; i < _MyBookedTimeArrayStr.Length; i++) {
                            q.Enqueue(Convert.ToInt32(_MyBookedTimeArrayStr[i]));
                        }
                        //Flag to mark the time is exist
                        bool _Flag = true;
                        //loop the booked time
                        while (q.Count() > 0)
                        {
                            //Get the first time slid
                            int PickUpTime = q.Dequeue();
                            int DropOffTime = q.Dequeue();
                            if (Convert.ToInt32( model.DropOffTime)<= PickUpTime || Convert.ToInt32(model.PickUpTime) >= DropOffTime)
                            {
                                //do nothng
                            }
                            else
                            {
                                _Flag = false;
                                break;
                            } 
                        }
                        if (_Flag)
                        {
                            AvailableVehicle.Add(item);
                        }     
                }//End of _VehicleAvailability
                }//End of (if not esxit in the db date and id
            }
            //after get the available cars, need to find there currrent location from Vehicle cuurert TB.
            IEnumerable< AvailVehicleWithLocationAndBookData> AvailableVehicleWithLocation = from ba in AvailableVehicle
                      join se in _context.VehicleCurrentLocations on ba.Id equals se.VehicleId
                      select new AvailVehicleWithLocationAndBookData
                      {
                          VehicleId=ba.Id,
                          VehicleModel = ba.Model,
                          VehiclePricePH=ba.PricePerHour,
                          VehicleFuel=ba.FuelType,
                          VehicleTransmission=ba.Transmission,
                          VehicleYear = ba.YearOfManufacture,
                          VehicleImage = ba.Image,
                          VehicleRating=ba.Rating,
                          VehicleCurrentLatitue = se.Latitue,
                          VehicleCurrentLongitute = se.Longitute,
                          DropOffLongitute=DropOffLongitude,
                          DropOffLatitue=DropOffLatitude,
                          PickUpDate=model.PickUpDate,
                        //  DropOffDate=model.DropOffDate,
                          PickUpTime=model.PickUpTime,
                          DropOffTime=model.DropOffTime
                      };
     
           
            return View( model); //send only the dropoff location
           // return View("AvailableVehicles", AvailableVehicleWithLocation);
        }

        //Show the result In Map
        public IActionResult FindCarInMap(FindVehicleModelView model)
        {
            //Get All the Cars.
            IQueryable<Vehicle> _Vehicles = _context.Vehicles;
            //List to store the Availabe cars
            List<Vehicle> AvailableVehicle = new List<Vehicle>();
            //Current location of the customer(PickUp)
            double CustomerLatitude = Convert.ToDouble(model.PickLatitude);
            double CustomerLongitude = Convert.ToDouble(model.PickLongitude);
            //DropOff Location
            double DropOffLatitude = Convert.ToDouble(model.DropLatitude);
            double DropOffLongitude = Convert.ToDouble(model.DropLongitude);

            //loop for cars
            foreach (var item in _Vehicles)
            {
                //temp list for current car
                Vehicle _Vehicle = new Vehicle();
                //Get the Availability tb for current vehicle(List) in the current date..
                IQueryable<VehicleAvailability> _VehicleAvailability = _context.VehicleAvailabilities.Where(e => e.VehicleId == item.Id && e.Date == model.PickUpDate);

                //  bool IsExist = _VehicleAvailability.Any(a => a.Id == item.Id);
                //if the vehicle not exist in the reservation tb...
                if (!_VehicleAvailability.Any())
                {
                    AvailableVehicle.Add(item);
                }
                else//if its exist in the db in this  date loop it
                {

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
                        //Flag to mark the time is exist
                        bool _Flag = true;
                        //loop the booked time
                        while (q.Count() > 0)
                        {
                            //Get the first time slid
                            int PickUpTime = q.Dequeue();
                            int DropOffTime = q.Dequeue();
                            if (Convert.ToInt32(model.DropOffTime) <= PickUpTime || Convert.ToInt32(model.PickUpTime) >= DropOffTime)
                            {
                                //do nothng
                            }
                            else
                            {
                                _Flag = false;
                                break;
                            }
                        }
                        if (_Flag)
                        {
                            AvailableVehicle.Add(item);
                        }
                    }//End of _VehicleAvailability
                }//End of (if not esxit in the db date and id
            }

            //Need more work ,, if there is no session
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            //after get the available cars, need to find there currrent location from Vehicle cuurert TB.             
            IList<AvailVehicleWithLocationAndBookData> AvailableVehicleWithLocation = ( from ba in AvailableVehicle
                                                                                            join se in _context.VehicleCurrentLocations on ba.Id equals se.VehicleId                                                                                          
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
                                                                                                VehicleCurrentLongitute = se.Longitute,
                                                                                                DropOffLongitute = DropOffLongitude,
                                                                                                DropOffLatitue = DropOffLatitude,
                                                                                                PickUpDate = model.PickUpDate,
                                                                                              //  DropOffDate = model.DropOffDate,
                                                                                                PickUpTime = model.PickUpTime,
                                                                                                DropOffTime = model.DropOffTime
                                                                                            }).ToList();

            //To Get Car will be droppedoff in the same area in the time wanted..
            IQueryable<Reservation> _Reservations = _context.Reservations.Where(pre=>pre.PickUpDate==model.PickUpDate && pre.DropOffTime==model.PickUpTime);
            foreach (var item_Res in _Reservations)
            {
                Vehicle _ResVehicle = _context.Vehicles.Find(item_Res.VehicleId);
                AvailVehicleWithLocationAndBookData _ReservationsModel = new AvailVehicleWithLocationAndBookData() {
                    VehicleId = _ResVehicle.Id,
                    VehicleModel = _ResVehicle.Model,
                    VehiclePricePH = _ResVehicle.PricePerHour,
                    VehicleFuel = _ResVehicle.FuelType,
                    VehicleTransmission = _ResVehicle.Transmission,
                    VehicleYear = _ResVehicle.YearOfManufacture,
                    VehicleImage = _ResVehicle.Image,
                    VehicleRating = _ResVehicle.Rating,
                    VehicleCurrentLatitue = item_Res.ReturnLocationLatitue,
                    VehicleCurrentLongitute = item_Res.ReturnLocationLongitute       
                };
                AvailableVehicleWithLocation.Add(_ReservationsModel);
            }
           
            //Building google map....  Need solo for it
            string markers = "[";
            foreach (var item in AvailableVehicleWithLocation)
            {
                bool _IsMyFavorite = _context.Favorites.Any(fa=>fa.UserId==currentUserId && fa.VehicleId==item.VehicleId);
                string _MyHeart = "";
                if (_IsMyFavorite)
                {
                    _MyHeart = " This Vehicle is in your favorite list <img src=/Image/CarShare/MyHeart.png width=20/> ";
                }  
                //Need to Pass <Form>
                StringBuilder sb = new StringBuilder();
                sb.Append("<div id=content><div id=siteNotice><h3>");
                sb.Append(item.VehicleModel);
                sb.Append("</h3><p>Price £ ");
                sb.Append(item.VehiclePricePH);
                sb.Append(" P/h</p><p>Rating:<img src=/Image/CarShare/ratingstar.jpg width=20/> ");
                sb.Append(item.VehicleRating);
                sb.Append(" </p><p>");
                sb.Append(_MyHeart);
                sb.Append("</p></div><img class=img-thumbnail style=width:150px; src=");
                sb.Append(item.VehicleImage);
                sb.Append(" /><p> Year: ");
                sb.Append(item.VehicleYear);
                sb.Append("</p><div id=bodyContent><p>");
                sb.Append("<a type=button class=btn-primary onclick=BookIt(");
                sb.Append(item.VehicleId);
                sb.Append(")>Book It</a></p> </div></div>");


                double rs = Distance(CustomerLatitude, CustomerLongitude, item.VehicleCurrentLatitue, item.VehicleCurrentLongitute);
                if (rs < 10000)
                {
                    markers += "{";
                    markers += string.Format("'lat': '{0}',", item.VehicleCurrentLatitue);
                    markers += string.Format("'lng': '{0}',", item.VehicleCurrentLongitute);
                    markers += string.Format("'text': '{0}',", sb);
                    markers += "},";
                }
            }
            markers += "];";
            //Ends Google maps
            model.Marker = markers;
             return View("FindCar",model); //send only the dropoff location
       // return View("AvailableVehicles", AvailableVehicleWithLocation);
        }

       
        public IActionResult Index()
        {
            FindVehicleModelView model = new FindVehicleModelView();
            //to show the today data in HTML
            model.PickUpDate = DateTime.Now;
            //Get the user from session
            string currentUserId = _userManager.GetUserId(HttpContext.User);
            if(currentUserId != null) {               
              
                var _DrivingLicense =  _context.DrivingLicences.FirstOrDefault(m => m.UserId == currentUserId);
                if (_DrivingLicense == null)
                {
                    model.StatusMessage = "Dear User, Please Complete Your Driving License data to be able to book a car.";
                    return View(model);
                }
                if (_DrivingLicense.IsValid == false)
                {
                    model.StatusMessage = "Dear User, Your Drinving License is on validation process.....";
                    return View(model);
                }
                var _Address =  _context.Addresses.FirstOrDefault(ma => ma.UserId == currentUserId);
                if (_Address == null) {
                    model.StatusMessage = "Dear User, Please Complete Your Address data to be able to book a car.";                     
                    return View(model);
                }
                var _UserInfo =  _context.Users.Find(currentUserId);
                if (_UserInfo == null)
                {
                model.StatusMessage = "Dear User, Please Complete Your Profile data to be able to book a car.";
                    return View(model);
                }
                
                if (_UserInfo.PhoneNumber== null || _UserInfo.FirstName==null || _UserInfo.LastName==null ||
                    _DrivingLicense.LicenseNumber==null || _DrivingLicense.IssueDate==null || _DrivingLicense.ExpireDate==null ||
                    _Address.PostCode==null)
                {  
                    model.StatusMessage = "Dear User, Please Complete Your Profile (Driving License, Profile) to be able to book a car.";
                        
                    return View(model);
                 }
            }
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(ErrorViewModel error)
        {
            return View(error);
        }


        //map code 

        private double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(Deg2rad(lat1)) * Math.Sin(Deg2rad(lat2)) + Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) * Math.Cos(Deg2rad(theta));
                dist = Math.Acos(dist);
                dist = Rad2deg(dist);
                dist = dist * 60 * 1.1515;
                dist = dist * 1.609344;
                dist = dist * 1000;
                return (dist);
            }
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public double Deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public double Rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }


    }
}
