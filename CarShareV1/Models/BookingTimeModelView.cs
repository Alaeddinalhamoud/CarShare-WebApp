using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarShareV1.Models
{
    public class BookingTimeModelView
    {
        public DateTime Date { get; set; }
        public int PickUpTime { get; set; }
        public int DropOffTime { get; set; }
       
    }
}
