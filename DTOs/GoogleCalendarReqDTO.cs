using System.ComponentModel.DataAnnotations;

namespace UserCalendarAPI.DTOs
{
    public class GoogleCalendarReqDTO
    {
        [Required]
        public string Summary {
            get;
            set;
        }
        [Required]
        public string Description {
            get;
            set;
        }
        [DataType(DataType.DateTime)]
        public DateTime StartTime {
            get;
            set;
        }
        [DataType(DataType.DateTime)]
        public DateTime EndTime {
            get;
            set;
        }
        public string? CalendarId {
            get;
            set;
        }
        [Required]
        public string refreshToken {
            get;
            set;
        } = null!;
    }

    public class GoogleTokenResponse
    {
        public string access_type {
            get;
            set;
        }

        public long expires_in {
            get;
            set;
        }

        public string refresh_token {
            get;
            set;
        }

        public string scope {
            get;
            set;
        }

        public string token_type {
            get;
            set;
        }
    }
}
