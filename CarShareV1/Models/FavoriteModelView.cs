using System;

namespace CarShareV1.Models
{
    public class FavoriteModelView
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehicleModel { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
