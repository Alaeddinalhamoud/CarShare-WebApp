using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using CarShareV1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;
        private IHostingEnvironment _env;
        private readonly IEmailSender _emailSender;
        private readonly ISMSProvider _SMSSender;

        public PaymentController(IHostingEnvironment env,ApplicationDbContext context,
            UserManager<UserApplication> _UserManager, IEmailSender emailSender, ISMSProvider _sMSSender)
        {
            _context = context;
            _userManager = _UserManager;
            _env = env;
            _emailSender = emailSender;
            _SMSSender = _sMSSender;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> PayByCard(string stripeEmail, string stripeToken, CheckOutModelView data)
        {
            POJOMsgs POJOModel = new POJOMsgs();
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            ErrorViewModel errorViewModel = new ErrorViewModel();     
            //Converting to duoble
            int? _amount = 0;
            if (!string.IsNullOrEmpty(data.TotalOrder.ToString()))
            {
                double n;
                bool isNumeric = double.TryParse(data.TotalOrder.ToString(), out n);
                _amount = isNumeric ? (int)(Convert.ToDecimal(data.TotalOrder.ToString()) * 100) : -1;
            }
            var customers = new CustomerService();
            var charges = new ChargeService();
            var customer = customers.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                SourceToken = stripeToken            
            });

            Charge charge = charges.Create(new ChargeCreateOptions
            {
                Amount = _amount,
                Description = data.Comment,
                Currency = "GBP",
                CustomerId = customer.Id,
                ReceiptEmail=data.Email,//the customer will recevive email invoice from stripe service                
                Metadata = new Dictionary<String, String>()
                          {
                          { "OrderId", data.OrderId.ToString()},
                           { "PostCode", data.PostCode},                            
                          }
              ,
            });

            if (charge.Status == "succeeded")
            {
                string BalanceTransactionId = charge.BalanceTransactionId;
                CarShare.Data.Order _Order = await _context.Orders.FindAsync(data.OrderId);
                //Get the Reservation for this order to send the details to the customer by email (Invoice).
                Reservation _Reservation = await _context.Reservations.FindAsync(_Order.ReservationId);
                try
                {
                    //change the order status to paid
                    _Order.IsPaid = true;
                    _Order.UpdateDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    //Change the REservation to IsConfirmed =ture mean paid too
                    _Reservation.IsConfirmed = true;
                    _Reservation.UpdateTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    POJOModel.Flag = false;
                    POJOModel.Msg = e.ToString();
                }
                POJOModel.Flag = true;
                POJOModel.Msg = _Order.Id.ToString();

                //Create Trans Record
                BankTransaction _BankTransaction = new BankTransaction
                {
                    TransactionId = BalanceTransactionId,
                    UserId = currentUserId,
                    OrderId = data.OrderId,
                    Amount = data.TotalOrder,
                    Status = "succeeded",
                    Date = DateTime.Now,
                    FullName = data.FirstName + " " + data.LastName
                };

                await _context.AddAsync(_BankTransaction);
                await _context.SaveChangesAsync();

                //Get te Vehicle Details.
                Vehicle _Vehicle = await _context.Vehicles.FindAsync(data.VehicleId);
                //Number of Hours
                int Hours = Convert.ToInt32(_Order.DropOffTime) - Convert.ToInt32(_Order.PickUpTime);
                //Send Email to the Cutomer Invoice...
                //Preper the link for Location image
                string GoogleImagePath = $"https://maps.googleapis.com/maps/api/staticmap?size=600x300&maptype=roadmap&markers=color:blue%7Clabel:S%7C{_Reservation.PickUpLocationLatitue},{_Reservation.PickUpLocationLongitute}&markers=color:green%7Clabel:E%7C{_Reservation.ReturnLocationLatitue},{_Reservation.ReturnLocationLongitute}&key=AIzaSyAJu3YwpJf6K5u1EVMtncPm4CD3209OWnw";
                EmailReceiptsBodyModelView _EmailReceipts = new EmailReceiptsBodyModelView()
                {
                   OrderNum= data.OrderId.ToString(),
                    CustomerName=  data.FirstName + " " + data.LastName,
                    CustomerEmail= data.Email,
                    ServiceName= _Vehicle.Model,
                    Hours=Hours.ToString(),
                    Price= _Vehicle.PricePerHour.ToString(),
                    Total=data.TotalOrder.ToString(),
                    PickUpDate=_Reservation.PickUpDate.ToShortDateString(),
                   // DropOFFDate=_Reservation.DropOffDate.ToShortDateString(),
                    PickUpTime=_Reservation.PickUpTime+":00",
                    DropOFFTime=_Reservation.DropOffTime+":00",
                    MapImage= GoogleImagePath
                };
                string EmailReciptsBody = EmailReceiptsBody(_EmailReceipts);
                 await _emailSender.SendEmailAsync(data.Email, "Car Share Service", EmailReciptsBody);
                    //Send SMS Confirmation
                _SMSSender.SendSMS(data.Mobile, "You Have Paid: £" + _Order.TotalAmount.ToString()+ " successfully, The Vehcile REG#"+_Vehicle.Registration);
                
                int id = data.OrderId;
                return RedirectToAction("OrderConfirmation", "Order", new { id = data.OrderId });
            }
            else
            {
                ErrorViewModel error = new ErrorViewModel()
                {
                    ErrorMsg = "Your payment has been Fail... you can pay it again from Pinding Order..."
                };
                //Error page if payment not sucssefuly done
                return RedirectToAction("Error", "Home",error);
            }

            ////Get te Vehicle Details.
            //Vehicle _Vehicle = await _context.Vehicles.FindAsync(data.VehicleId);
            ////Send Email to the Cutomer Invoice...
            //string EmailReciptsBody = EmailReceiptsBody(data.OrderId.ToString(), data.FirstName + " " + data.LastName, data.Email, _Vehicle.Model, _Vehicle.PricePerHour.ToString(), data.TotalOrder.ToString());
            //await _emailSender.SendEmailAsync(data.Email, "Car Share Service", EmailReciptsBody);
            //int id = data.OrderId;          
            //return RedirectToAction("OrderConfirmation", "Order", new { id = data.OrderId });
        }


        //Send Invoice to the customer html  template
        private string EmailReceiptsBody(EmailReceiptsBodyModelView _EmailReceipts)
        {
            var webRoot = _env.WebRootPath;
             
            string body = string.Empty;
            string TemplePath = webRoot + "/Pages/EmailReceipts.html";
            using (StreamReader reader = new StreamReader(TemplePath))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{InvoiceNumber}", _EmailReceipts.OrderNum);
            body = body.Replace("{Date}", DateTime.Now.ToShortDateString());
            body = body.Replace("{CustomerName}", _EmailReceipts.CustomerName);
            body = body.Replace("{CustomerEmail}", _EmailReceipts.CustomerEmail);
            body = body.Replace("{ServiceName}", _EmailReceipts.ServiceName);
            body = body.Replace("{Hours}", _EmailReceipts.Hours);
            body = body.Replace("{Price}", _EmailReceipts.Price);
            body = body.Replace("{Total}", _EmailReceipts.Total);
            //New Parms
            body = body.Replace("{PickUpDate}", _EmailReceipts.PickUpDate);
          //  body = body.Replace("{DropOFFDate}", _EmailReceipts.DropOFFDate);
            body = body.Replace("{PickUpTime}", _EmailReceipts.PickUpTime);
            body = body.Replace("{DropOFFTime}", _EmailReceipts.DropOFFTime);
            body = body.Replace("MapImage", _EmailReceipts.MapImage);

            return body;
        }



    }
}