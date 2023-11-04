using System.ComponentModel.DataAnnotations;

namespace UserCalendarAPI.DTOs
{
    public class ApplicationUserLOGINDto
    {
        [Required]
        public string UserNameOrEmail { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

    }
}
