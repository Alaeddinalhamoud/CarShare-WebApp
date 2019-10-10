using Microsoft.AspNetCore.Mvc;
using System;

namespace CarShareV1.Models
{
    public class FindVehicleModelView
    {
        public DateTime PickUpDate { get; set; }
      //  public DateTime DropOffDate { get; set; }
        public string PickUpTime { get; set; }
        public string DropOffTime { get; set; }
        public string PickLatitude { get; set; }
        public string PickLongitude { get; set; }
        public string DropLatitude { get; set; }
        public string DropLongitude { get; set; }
        public string Marker { get; set; }
        [TempData]
        public string StatusMessage { get; set; }

    }
}
