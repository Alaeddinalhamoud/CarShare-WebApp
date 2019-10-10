using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CarShare.Data
{
   public class Favorite
    {
        [Key]
        public int Id { get; set; }
         public int VehicleId { get; set; }
        public string UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
