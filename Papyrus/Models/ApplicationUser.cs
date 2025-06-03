using Microsoft.AspNetCore.Identity;

namespace Papyrus.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Position { get; set; }
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }
    }
}
