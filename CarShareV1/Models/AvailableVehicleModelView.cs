namespace CarShareV1.Models
{
    public class AvailableVehicleModelView
    {        
        public int VehicleId { get; set; }
        public double PickLongitute { get; set; }
        public double PickLatitue { get; set; }
        public double DropoffLongitute { get; set; }
        public double DropoffLatitue { get; set; }
        public string PickUpDate { get; set; }
       // public string DropOffDate { get; set; }
        public string PickUpTime { get; set; }
        public string DropOffTime { get; set; }


    }
}
