using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CarShare.Data
{
   public class VehicleCurrentLocation
    {
        [Key]
        public int Id { get; set; }
        public int VehicleId { get; set; }
       
        //Current Location
        public double Longitute { get; set; }
        public double Latitue { get; set; }

        //Just for details
        public string UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
