using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Jobs;
using CarShareV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;
         IScheduler _scheduler;

       
        public ReservationController( ApplicationDbContext context, UserManager<UserApplication> _UserManager,
            IScheduler _Scheduler)
        {
            _context = context;
            _userManager = _UserManager;
            _scheduler = _Scheduler;
        }
        
        //to add booking to db 
        [HttpPost("/Reservation/BookIt")]
        public async Task<IActionResult> BookIt([FromBody] AvailableVehicleModelView data)
        {
            POJOMsgs POJOmodel = new POJOMsgs();
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            if (currentUserId != null)
            {
                //get the current location of the car
                //  VehicleCurrentLocation _VehicleCurrentLocation = await _context.VehicleCurrentLocations.FirstOrDefaultAsync(car =>car.VehicleId==data.VehicleId);
                try
                {
                    //GEt the user email
                    var _User =await _context.Users.FindAsync(currentUserId);
                    //GEt the vehicle details
                    Vehicle _Vehicle = await _context.Vehicles.FindAsync(data.VehicleId);
                    //Get the total hours and price
                    double TotalPrice = _Vehicle.PricePerHour * (Convert.ToInt32(data.DropOffTime) - Convert.ToInt32(data.PickUpTime));
                    // string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
                    Reservation model = new Reservation
                    {
                        VehicleId = data.VehicleId,
                        PickUpLocationLatitue = data.PickLatitue,// car current location
                        PickUpLocationLongitute = data.PickLongitute,// car current location
                        ReturnLocationLatitue = data.DropoffLatitue,
                        ReturnLocationLongitute = data.DropoffLongitute,
                        PickUpDate = Convert.ToDateTime(data.PickUpDate),
                        //   DropOffDate = Convert.ToDateTime(data.DropOffDate),
                        PickUpTime = data.PickUpTime,
                        DropOffTime = data.DropOffTime,
                        UserId = currentUserId,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                        Amount = TotalPrice
                    };
                    //Adding New Reservation
                    await _context.Reservations.AddAsync(model);
                    await _context.SaveChangesAsync();
                    int _ReservationId = model.Id;
                    //to add/or update VehicleAvailabilities tb (5,2)
                    VehicleAvailability _AvailableVehicle = await _context.VehicleAvailabilities.FirstOrDefaultAsync(a => a.VehicleId == model.VehicleId && a.Date == model.PickUpDate);
                    //if avail just update it 
                    if (_AvailableVehicle != null)
                    {
                        string TempBookingTime = _AvailableVehicle.BookingTime + "," + model.PickUpTime + "," + model.DropOffTime;
                        _AvailableVehicle.BookingTime = TempBookingTime;
                        _AvailableVehicle.UpdateTime = DateTime.Now;
                        _AvailableVehicle.UserId = currentUserId;
                        //update VehicleAvailabilities
                        _context.VehicleAvailabilities.Update(_AvailableVehicle);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        VehicleAvailability _availableVehicle = new VehicleAvailability
                        {
                            VehicleId = data.VehicleId,
                            Date = model.PickUpDate,
                            BookingTime = model.PickUpTime + "," + model.DropOffTime,
                            UserId = currentUserId,
                            CreateTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        };
                        //Add new VehicleAvailabilities
                        await _context.VehicleAvailabilities.AddAsync(_availableVehicle);
                        await _context.SaveChangesAsync();
                    }



                    //after create order after booking                
                    Order _Order = new Order
                    {
                        VehicleId = data.VehicleId,
                        UserId = currentUserId,
                        PickUpDate = Convert.ToDateTime(data.PickUpDate),
                        //   DropOffDate = Convert.ToDateTime(data.DropOffDate),
                        PickUpTime = data.PickUpTime,
                        DropOffTime = data.DropOffTime,
                        ReservationId = _ReservationId
                    };

                    _Order.TotalAmount = TotalPrice;//Need the total hours
                    _Order.IsPaid = false;//before pay
                    _Order.CreateDate = DateTime.Now;
                    _Order.UpdateDate = DateTime.Now;
                    await _context.Orders.AddAsync(_Order);
                    await _context.SaveChangesAsync();

                    POJOmodel.Flag = true;
                    POJOmodel.Msg = _Order.Id.ToString();


                    //Do Job
                    IJobDetail _JobDeleteUnpaid = JobBuilder.Create<JobDeleteUnpaid>()
                        .UsingJobData("Id", _Order.Id.ToString())
                        .UsingJobData("Email", _User.Email)
                   .WithIdentity("simplejob" + _Order.Id.ToString(), "quartzexapmle" + _Order.Id.ToString())
                   .Build();

                    ITrigger _JobDeleteUnpaid_triger = TriggerBuilder.Create()
                        .WithIdentity("testtrigger" + _Order.Id.ToString(), "quartzexapmle" + _Order.Id.ToString())
                        .StartNow()
                        .WithSimpleSchedule(x => x.WithIntervalInMinutes(3).WithRepeatCount(2))
                        .Build();
                    await _scheduler.ScheduleJob(_JobDeleteUnpaid, _JobDeleteUnpaid_triger);

                    //

                }
                catch (Exception e)
                {

                    POJOmodel.Flag = false;
                    POJOmodel.Msg = e.ToString();
                }
            }
            else
            {
                POJOmodel.Flag = false;
                POJOmodel.Msg = "Plase, Login or Regiester....";
            }


            return Json(POJOmodel);
        }

      

        // GET: Reservation
        public async Task<IActionResult> Index()
        {
            return View(await _context.Reservations.ToListAsync());
        }

        // GET: Reservation Futer
        public IActionResult FutuerIndex()
        {
            IQueryable<Reservation> _MyReservation = _context.Reservations.Where(m => m.PickUpDate >= DateTime.Now.Date).OrderBy(m =>m.PickUpDate);
            return View("Index", _MyReservation);
        }

        // GET: Reservation
        public IActionResult PastIndex()
        {
            //adddays(-10 to show all before today)
            IQueryable<Reservation> _MyReservation = _context.Reservations.Where(m=>m.PickUpDate < DateTime.Now.AddDays(-1)).OrderBy(m => m.PickUpDate);
            return View("Index",_MyReservation);
        }

        // GET: Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservation/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reservation/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VehicleId,UserId,PickUpLocationLongitute,PickUpLocationLatitue,ReturnLocationLongitute,ReturnLocationLatitue,PickUpDate,PickUpTime,DropOffDate,DropOffTime,Amount,CreateTime,UpdateTime")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(FutuerIndex));
            }
            return View(reservation);
        }

        // GET: Reservation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            return View(reservation);
        }

        // POST: Reservation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,UserId,PickUpLocationLongitute,PickUpLocationLatitue,ReturnLocationLongitute,ReturnLocationLatitue,PickUpDate,PickUpTime,DropOffTime,Amount,CreateTime,UpdateTime")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(FutuerIndex));
            }
            return View(reservation);
        }

        // GET: Reservation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        //Should Give only the opened one
        public IActionResult MyOldReservations()
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
           IQueryable<Reservation> _MyReservation = _context.Reservations.Where(m => m.UserId == currentUserId &&
                                                                                m.PickUpDate.Date <= DateTime.Now.Date &&
                                                                                Convert.ToInt32(m.DropOffTime) <= DateTime.Now.Hour &&
                                                                                m.IsConfirmed == true
                                                                                ).OrderBy(m => m.PickUpDate);
            return View(_MyReservation);
        }

        //Should Give only the opened one
        public IActionResult MyFutureReservations()
        {
            //need to charck date and hour
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            IQueryable<Reservation> _MyReservation = _context.Reservations.Where(m => m.UserId == currentUserId && 
                                                                                 m.PickUpDate.Date >= DateTime.Now.Date &&
                                                                                 Convert.ToInt32(m.PickUpTime) >= DateTime.Now.Hour &&
                                                                                 m.IsConfirmed==true).OrderBy(m => m.PickUpDate);
            return View(_MyReservation);
        }


        public IActionResult ReservationDetails(int Id)
        {           
            var model = from res in _context.Reservations.Where(i => i.Id == Id)
                        join ve in _context.Vehicles on res.VehicleId equals ve.Id
                        join us in _context.Users on res.UserId equals us.Id
                        select new ReservationModelView
                        {
                            UserName=us.FirstName +" "+us.LastName,
                            VehicleModel=ve.Model,
                            PickUpDate=res.PickUpDate,
                          //  DropOffDate=res.DropOffDate,
                            PickUpTime=res.PickUpTime,
                            DropOffTime=res.DropOffTime,
                            PickUpLocationLongitute=res.PickUpLocationLongitute,
                            PickUpLocationLatitue=res.PickUpLocationLatitue,
                            ReturnLocationLongitute=res.ReturnLocationLongitute,
                            ReturnLocationLatitue=res.ReturnLocationLatitue,
                            Total=res.Amount,
                            Date=res.CreateTime,
                            IsConfirmed=res.IsConfirmed
                        };
            
            ReservationModelView result = model.FirstOrDefault();
            return View(result);
        }



       

    }
}
