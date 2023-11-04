using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UserCalendarAPI.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;
        
        [Range(18,90,ErrorMessage ="Age Must be between 18-90 ")]
        public int? Age { get; set; }

        [RegularExpression("^01[0125][0-9]{8}$",ErrorMessage ="Please enter a valid Egyptian number 01xxxxxxxxx")]
        public override string? PhoneNumber { get; set; }

    }
}
