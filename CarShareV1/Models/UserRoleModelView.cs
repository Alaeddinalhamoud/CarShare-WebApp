using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CarShareV1.Models
{
    public class UserRoleModelView
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string Email { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }
    }
}
