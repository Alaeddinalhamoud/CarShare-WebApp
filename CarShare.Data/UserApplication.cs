using Microsoft.AspNetCore.Identity;
using System;

namespace CarShare.Data
{
    public class UserApplication: IdentityUser
    {
       
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
       
        public DateTime DOB { get; set; }        
        public Genders Gender { get; set; }
    }
    
    public enum Genders
    {
        Male, Female, NA
    }
}
