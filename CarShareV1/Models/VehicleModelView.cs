using CarShare.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarShareV1.Models
{
    public class VehicleModelView
    {
        public int Id { get; set; }
        public string VIN { get; set; }
        public string Registration { get; set; }
        public string Model { get; set; }
        public string CylinderCapacity { get; set; }
        public int NumberOfDoors { get; set; }
        public string SixMonthRate { get; set; }
        public string TwelveMonthRate { get; set; }
        public string DateOfFirstRegistration { get; set; }
        public string YearOfManufacture { get; set; }
        public string Co2Emissions { get; set; }
        public string FuelType { get; set; }
        public string TaxStatus { get; set; }
        public string Transmission { get; set; }
        public string Colour { get; set; }
        public string TypeApproval { get; set; }
        public string WheelPlan { get; set; }
        public string RevenueWeight { get; set; }
        public string TaxDetails { get; set; }
        public string MotDetails { get; set; }
        public bool Taxed { get; set; }
        public bool Mot { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UserId { get; set; }
        public string Image { get; set; }
        public double PricePerHour { get; set; }
        public double Rating { get; set; }


        public IEnumerable<Review> VehicleReviews { get; set; }
    }
}
