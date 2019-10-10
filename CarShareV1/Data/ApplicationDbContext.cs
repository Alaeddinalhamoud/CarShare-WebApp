using CarShare.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarShareV1.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserApplication>
    {
        

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<EmailSetting> EmailSettings { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public  DbSet<Branch> Branches { get; set; }
        public  DbSet<DrivingLicence> DrivingLicences { get; set; }
        public  DbSet<Insurance> Insurances { get; set; }
        public DbSet<Agreement> Agreements { get; set; }
        public DbSet<BranchAddress> BranchAddresses { get; set; }
        public DbSet<WebSiteSetting> WebSiteSettings { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<VehicleAvailability> VehicleAvailabilities { get; set; }
        public DbSet<VehicleCurrentLocation> VehicleCurrentLocations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
    }
}
