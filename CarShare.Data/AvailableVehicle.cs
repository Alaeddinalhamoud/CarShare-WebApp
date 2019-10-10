using System;
using System.ComponentModel.DataAnnotations;

namespace CarShare.Data
{
    public class AvailableVehicle
    {
        [Key]
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        //Just for details
        public string UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
