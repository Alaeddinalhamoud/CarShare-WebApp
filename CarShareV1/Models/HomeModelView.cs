using CarShare.Data;
using System.Collections.Generic;

namespace CarShareV1.Models
{
    public class HomeModelView
    {
        public IEnumerable<Branch> FromBranch { get; set; }
        public IEnumerable<Branch> ToBranch { get; set; }
        public IEnumerable<Vehicle> Vehicles { get; set; }
        public int BranchId { get; set; }
        public int VehicleId { get; set; }

    }
}
