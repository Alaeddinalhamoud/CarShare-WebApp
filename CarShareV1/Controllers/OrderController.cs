using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
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
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;
         

        public OrderController( ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;    
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("/Order/CheckOut/{id}")]
        public async Task<IActionResult> CheckOut(int? id)
        {
            //If the order delted go to error ...
            ErrorViewModel errorViewModel = new ErrorViewModel() {
                ErrorMsg = "Something went wrong while confirming your order.Contact Support"
                  };
            if (id.Equals(0) || string.IsNullOrEmpty(id.ToString()))
            {               
                return RedirectToAction("Error", "Home", errorViewModel);//error message 
            }
            //data to prepare the checkout and invioce , payment
            Order _Order = await _context.Orders.FindAsync(id);
            if (_Order==null)
            {                
                return RedirectToAction("Error", "Home", errorViewModel);//error message 
            }

            Address _Address = await _context.Addresses.FirstOrDefaultAsync(d => d.UserId==_Order.UserId);
            var _User = await _context.Users.FirstOrDefaultAsync(f =>f.Id==_Order.UserId);

            CheckOutModelView model = new CheckOutModelView
            {
                FirstName = _User.FirstName,
                LastName = _User.LastName,
                Email = _User.Email,
                Country = _Address.Country,
                County = _Address.County,
                Street = _Address.Street,
                City = _Address.City,
                PostCode = _Address.PostCode,
                Mobile = _User.PhoneNumber,
                TotalOrder = _Order.TotalAmount,
                OrderId = _Order.Id,
                VehicleId=_Order.VehicleId
                
            };

            return View(model);
        }


        public IActionResult OrderConfirmation(int id)
        {
            ErrorViewModel errorViewModel = new ErrorViewModel();
            if (id.Equals(0))
            {
                errorViewModel.ErrorMsg = "Something went wrong while confirming your order.Contact Support";
                return RedirectToAction("Error", "Home", errorViewModel);//error message 
            }
            Order _Order = new Order();
            _Order = _context.Orders.Find(id);
            return View(_Order);
        }


        //is there is any order not paid
        public IActionResult MyPendingOrders()
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);
            IQueryable<Order> _Orders = _context.Orders.Where(user =>user.UserId==currentUserId &&user.IsPaid==false).OrderBy(m => m.PickUpDate); ;
            return View(_Orders);
        }

        //show the paid orders
        public IActionResult MyOrderHistory()
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);
            IQueryable<Order> _Orders = _context.Orders.Where(user => user.UserId == currentUserId && user.IsPaid == true).OrderBy(m => m.PickUpDate); ;
            return View(_Orders);
        }
        //Need join with reservation and user 
        public IActionResult OrderDetails(int Id)
        {
           
            Order _Order = _context.Orders.Find(Id);
            Reservation _Reservation = _context.Reservations.Find(_Order.ReservationId);
            var Customer = _context.Users.Find(_Order.UserId);
            Vehicle _Vehicle = _context.Vehicles.Find(_Order.VehicleId);
            //Preper the link for Location image
            string GoogleImagePath = $"https://maps.googleapis.com/maps/api/staticmap?size=600x300&maptype=roadmap&markers=color:blue%7Clabel:S%7C{_Reservation.PickUpLocationLatitue},{_Reservation.PickUpLocationLongitute}&markers=color:green%7Clabel:E%7C{_Reservation.ReturnLocationLatitue},{_Reservation.ReturnLocationLongitute}&key=AIzaSyAJu3YwpJf6K5u1EVMtncPm4CD3209OWnw";
            //Number of Hours
            int Hours = Convert.ToInt32(_Order.DropOffTime) - Convert.ToInt32(_Order.PickUpTime);

            EmailReceiptsBodyModelView data = new EmailReceiptsBodyModelView() {
                OrderNum = _Order.Id.ToString(),
                CustomerName = Customer.FirstName + " " + Customer.LastName,
                CustomerEmail = Customer.Email,
                ServiceName = _Vehicle.Model,
                Hours = Hours.ToString(),
                Price = _Vehicle.PricePerHour.ToString(),
                Total = _Order.TotalAmount.ToString(),
                PickUpDate = _Reservation.PickUpDate.ToShortDateString(),
              //  DropOFFDate = _Reservation.DropOffDate.ToShortDateString(),
                PickUpTime = _Reservation.PickUpTime + ":00",
                DropOFFTime = _Reservation.DropOffTime + ":00",
                Date=_Order.CreateDate.ToShortDateString(),
                MapImage = GoogleImagePath
            };

            return View(data);
        }

    }
}