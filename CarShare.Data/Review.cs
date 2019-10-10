using System;
using System.ComponentModel.DataAnnotations;

namespace CarShare.Data
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }       
        public int Rating { get; set; }
        public int VehicleId { get; set; }
        public string Description { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
