using System;

namespace CarShare.Data
{
    public  class DrivingLicence
    {
        public int Id { get; set; }
        public string LicenseNumber{ get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public string Image { get; set; }
        public string UserId { get; set; }
        public bool IsValid { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
