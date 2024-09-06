using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Employee_Management_System.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string EmpId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Pan {  get; set; }
        [Required]
        public string Address {  get; set; }
    }
}
