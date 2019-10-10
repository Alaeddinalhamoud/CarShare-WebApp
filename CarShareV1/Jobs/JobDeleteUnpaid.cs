using CarShare.Data;
using CarShareV1.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CarShareV1.Jobs
{
    public class JobDeleteUnpaid: IJob
    {

        public IConfiguration Configuration { get; }
        private readonly IEmailSender _emailSender;

        public JobDeleteUnpaid(IConfiguration configuration, IEmailSender emailSender)
        {            
            Configuration = configuration;
            _emailSender = emailSender;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap _data = context.JobDetail.JobDataMap;
            string Id = _data.GetString("Id");
            string UserEmail = _data.GetString("Email"); ;
            Debug.WriteLine($"From Delte {Id}  his email {UserEmail}");
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            using (var _context = new ApplicationDbContext(optionsBuilder.Options))
            {
                int MyId = Convert.ToInt32(Id);
             
                //Check, if still not paid delete it 
                Reservation _DeleteReservation = await _context.Reservations.FirstOrDefaultAsync(r =>r.Id==MyId && r.IsConfirmed==false);
                if (_DeleteReservation != null)
                {
                    var StartTime = DateTime.Now;
                    var Sub = StartTime.Subtract(_DeleteReservation.CreateTime);
                    //time need to delete after 2 min as test...
                    var FromNow = TimeSpan.FromMinutes(2);
                    if (Sub >= FromNow)
                    {
                        //remove order, booking and reservation.
                        Order _order = await _context.Orders.FirstOrDefaultAsync(ord => ord.ReservationId == _DeleteReservation.Id);
                        string TimesToDelete = _DeleteReservation.PickUpTime + "," + _DeleteReservation.DropOffTime;
                        //Booking Table
                        VehicleAvailability _VehicleAvailability = await _context.VehicleAvailabilities
                                    .FirstOrDefaultAsync(va => va.Date == _DeleteReservation.PickUpDate && va.VehicleId == _DeleteReservation.VehicleId);
                        string Times = _VehicleAvailability.BookingTime;
                        string NewTimes = string.Empty;
                        if (Times.Contains(TimesToDelete))
                        {
                            NewTimes = Times.Remove(Times.IndexOf(TimesToDelete), TimesToDelete.Length);
                        }
                        //Send the New Time Table
                        _VehicleAvailability.BookingTime = NewTimes;
                        _context.VehicleAvailabilities.Update(_VehicleAvailability);
                        _context.Orders.Remove(_order);
                        //should remve to, if no will be show in predected result
                        _context.Reservations.Remove(_DeleteReservation);
                        await _context.SaveChangesAsync();
                        //Need Update the Availabiltiy tb too(after workin)..
                        Debug.WriteLine($"Reservation has been Deleted: {Id} ");
                        //Send Email to inform the user the booking has been cancceld
                        await _emailSender.SendEmailAsync(UserEmail, "Car Share Service", $" Dear User, Your Booking Num# {Id} has been canceled...");
                    }
                }//End of not null
                //IQueryable<Reservation> _Unpaid = _context.Reservations.Where(r => r.IsConfirmed == false);
                //foreach (var item in _Unpaid)
                //{
                //    var StartTime = DateTime.Now;
                //    var Sub = StartTime.Subtract(item.CreateTime);
                //    var FromNow = TimeSpan.FromMinutes(30);
                //    if (Sub >= FromNow)
                //    {
                //        Reservation _DeleteReservation = await _context.Reservations.FindAsync(item.Id);
                //        _context.Reservations.Remove(_DeleteReservation);
                //        await  _context.SaveChangesAsync();
                //        //Need Update the Availabiltiy tb too(after workin)..
                //        Debug.WriteLine($"Deleted {item.Id} ");
                //    }

                //}
                _context.Dispose();
            }



            
        }



    }
}
