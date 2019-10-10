using System;
using System.ComponentModel.DataAnnotations;

namespace CarShare.Data
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string UserId { get; set; }
        //PickUp Location
        public double PickUpLocationLongitute { get; set; }
        public double PickUpLocationLatitue { get; set; }
        //Drop Off location
        public double ReturnLocationLongitute { get; set; }
        public double ReturnLocationLatitue { get; set; }
        //Date of Pick up 01/01/2010
        public DateTime PickUpDate { get; set; }
        //Time of Pickup 12:30
        public string PickUpTime { get; set; }
      //  public DateTime DropOffDate { get; set; }
        public string DropOffTime { get; set; }
        //Will be calc after return the car.
        public double Amount { get; set; }
        //Just for details
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
