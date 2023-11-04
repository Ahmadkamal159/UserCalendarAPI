using System.ComponentModel.DataAnnotations;

namespace UserCalendarAPI.DTOs
{
    public class ApplicationUserREGESTERDto
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Compare("Email", ErrorMessage = "E-mail doesn't match")]
        public string ConfirmEmail { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Compare("Password",ErrorMessage ="password doesn't match")]
        [Required]
        public string ConfirmPassword { get; set; } = null!;

        [Range(18, 90, ErrorMessage = "Age Must be between 18-90 ")]
        public int? Age { get; set; }

        [RegularExpression("^01[0125][0-9]{8}$", ErrorMessage = "Please enter a valid Egyptian number 01xxxxxxxxx")]
        public string? PhoneNumber { get; set; }
    }
}
