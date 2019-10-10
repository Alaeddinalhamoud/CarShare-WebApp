using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarShareV1.Models
{
    public class BookingTimeChartModelView
    {
        public DateTime Date { get; set; }
        public int PickUpTime { get; set; }
        public int DropOffTime { get; set; }
        public int VehicleId { get; set; }
    }
}
