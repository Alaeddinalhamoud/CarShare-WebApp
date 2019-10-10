using System;

namespace CarShareV1.Models
{
    public class AvailVehicleWithLocationAndBookData
    {
        public int VehicleId { get; set; }
        public string VehicleModel { get; set; }
        public double VehiclePricePH { get; set; }
        public string VehicleFuel { get; set; }
        public string VehicleTransmission { get; set; }
        public string VehicleYear { get; set; }
        public string VehicleImage { get; set; }

        public double VehicleRating { get; set; }
        //Current to use it with the map
        public double VehicleCurrentLatitue { get; set; }
        public double VehicleCurrentLongitute { get; set; }
        //to use it for future plan booking
        public double DropOffLatitue { get; set; }
        public double DropOffLongitute { get; set; }
        public DateTime PickUpDate { get; set; }
        public DateTime DropOffDate { get; set; }
        public string PickUpTime { get; set; }
        public string DropOffTime { get; set; }
       
       





                          
                          
                         
    }
}
