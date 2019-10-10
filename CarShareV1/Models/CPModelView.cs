using CarShare.Data;
using System.Linq;

namespace CarShareV1.Models
{
    public class CPModelView
    {
        public int NumberOfUsers { get; set; }
        public int NumberOfVehicles { get; set; }
        public int NumberOfResevations { get; set; }
        public int NumberOfBranches { get; set; }
        public int NumberOfPendingDrivingLicence { get; set; }

        public IQueryable<Vehicle> Top10Vehicles { get; set; }
        public IQueryable<Order> Last10Orders { get; set; }


    }


}
