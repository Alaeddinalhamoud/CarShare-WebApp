using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CarShare.Data
{
   public class Order
    {
        [Key]
        public int Id { get; set; }        
        public int VehicleId { get; set; }       
        public string UserId { get; set; }
        public DateTime PickUpDate { get; set; }       
        public string PickUpTime { get; set; }
      //  public DateTime DropOffDate { get; set; }
        public string DropOffTime { get; set; }         
        public double TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        //Need it to show in the Invoice
        public int ReservationId { get; set; }


    }
}
