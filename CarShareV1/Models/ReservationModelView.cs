using System;

namespace CarShareV1.Models
{
    public class ReservationModelView
    {
        public string UserName { get; set; }
        public string VehicleModel { get; set; }
        public DateTime PickUpDate { get; set; }
       // public DateTime DropOffDate { get; set; }
        public string PickUpTime { get; set; }
        public string DropOffTime { get; set; }
        public double PickUpLocationLongitute { get; set; }
        public double PickUpLocationLatitue { get; set; }
        public double ReturnLocationLongitute { get; set; }
        public double ReturnLocationLatitue { get; set; }
        public double Total { get; set; }
        public DateTime Date { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
