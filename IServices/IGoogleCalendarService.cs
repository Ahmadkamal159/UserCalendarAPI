using Google.Apis.Calendar.v3.Data;
using UserCalendarAPI.DTOs;

namespace UserCalendarAPI.IServices
{
    public interface IGoogleCalendarService
    {
        string GetAuthCode();
        Task<GoogleTokenResponse> GetTokens(string code);
        string AddGoogleCalendar(GoogleCalendarReqDTO googleCalendarReqDTO);
        IEnumerable<CalendarListEntry> GetGoogleCalendars(string refreshToken);
        string AddToGoogleCalendar(GoogleCalendarReqDTO googleCalendarReqDTO);
        IEnumerable<Event> GetGoogleCalendarEvents(DateTime? startDate, DateTime? endDate, string? searchQuery, string refreshToken, string calendarId);
        bool DeleteEventFromGoogleCalendar(string refreshToken, string CalendarId, string eventId);
    }
}
