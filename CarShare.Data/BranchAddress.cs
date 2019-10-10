using System;
using System.ComponentModel.DataAnnotations;

namespace CarShare.Data
{
    public class BranchAddress
    {
        [Key]
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string PostCode { get; set; }
        public string HouseNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string County { get; set; }
        public double Longitute { get; set; }
        public double Latitue { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UserId { get; set; }
    }
}
