using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using UserCalendarAPI.DTOs;
using UserCalendarAPI.IServices;
using UserCalendarAPI.Services;

namespace UserCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IGoogleCalendarService _googleCalendarService;

        public EventsController(IGoogleCalendarService googleCalendarService)
        {
            _googleCalendarService = googleCalendarService;
        }

        //[HttpGet]
        //[Route("index")]
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet("googleAuth")]
        public async Task<IActionResult> GoogleAuth()
        {
            return Redirect(_googleCalendarService.GetAuthCode());
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> Callback()
        {
            string code = HttpContext.Request.Query["code"];
            string scope = HttpContext.Request.Query["scope"];

            //get token method
            var token = await _googleCalendarService.GetTokens(code);
            return Ok(token);
        }

        [HttpPost("CreateCalendar")]
        public IActionResult CreateCalendar([FromBody][Bind("Summary", "refreshToken")] GoogleCalendarReqDTO calendarEventReqDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            }

            try
            {
                // Call AddGoogleCalendar method or service to create the calender
                string CalendarId = _googleCalendarService.AddGoogleCalendar(calendarEventReqDTO);

                if (!string.IsNullOrEmpty(CalendarId))
                {
                    // Successfully created the event
                    // You can return the event details and a 201 Created status code
                    // Construct an eventDetails object with the created event details
                    var calendarDetails = new {
                        CalendarId,
                        calendarEventReqDTO.Summary,
                    };
                    return Ok(calendarDetails);
                }
                else
                {
                    return StatusCode(500, "Failed to create the event.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "one or more errors occurred. " + " " + e);
            }
        }


        [HttpGet("GetCalendars")]
        public IActionResult GetCalendars(string refreshtoken)
        {
            try
            {
                // You can accept query parameters to filter events, such as start date, end date, and a search query.
                // These parameters can be provided in the URL when making a GET request to this endpoint.

                // Call your method or service to fetch events from Google Calendar, and use the query parameters to filter the results.

                var Calendars = _googleCalendarService.GetGoogleCalendars(refreshtoken);

                if (Calendars != null && Calendars.Any())
                {
                    // If there are events, return them
                    return Ok(Calendars);
                }
                else
                {
                    return NotFound("No events found.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "An error occurred.");
            }
        }


        [HttpPost]
        public IActionResult PostEvent([FromBody] GoogleCalendarReqDTO calendarEventReqDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            }

            // Additional validation for not allowing events in the past or on Fridays/Saturdays
            if (calendarEventReqDTO.StartTime < DateTime.Now ||
                calendarEventReqDTO.EndTime < DateTime.Now ||
                calendarEventReqDTO.StartTime.DayOfWeek == DayOfWeek.Friday ||
                calendarEventReqDTO.StartTime.DayOfWeek == DayOfWeek.Saturday)
            {
                return BadRequest("Invalid event date/time");
            }

            try
            {
                // Call AddToGoogleCalendar method or service to create the event
                string eventId = _googleCalendarService.AddToGoogleCalendar(calendarEventReqDTO);

                if (!string.IsNullOrEmpty(eventId))
                {
                    // Successfully created the event
                    // You can return the event details and a 201 Created status code
                    // Construct an eventDetails object with the created event details
                    var eventDetails = new {
                        eventId,
                        calendarEventReqDTO.Summary,
                        calendarEventReqDTO.Description,
                        calendarEventReqDTO.StartTime,
                        calendarEventReqDTO.EndTime
                    };
                    
                    return Created($"https://localhost:44387/api/events/geteventbyid?refreshtoken={calendarEventReqDTO.refreshToken}&calendarId={calendarEventReqDTO.CalendarId}&eventId={eventId}", eventDetails);
                }
                else
                {
                    return StatusCode(500, "Failed to create the event.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "one or more errors occurred. " + " " + e);
            }
        }

        [HttpGet]
        public IActionResult GetEvents(string refreshtoken,string calendarId,[FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? searchQuery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            }
            try
            {

                var events = _googleCalendarService.GetGoogleCalendarEvents(startDate, endDate, searchQuery, refreshtoken,calendarId);

                if (events != null && events.Any())
                {
                    // If there are events, return them
                    return Ok(events);
                }
                else
                {
                    return NotFound("No events found.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpDelete]
        public IActionResult DeleteEvents(string refreshToken,string CalendarId,string EventId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            }
            try
            {

                var result = _googleCalendarService.DeleteEventFromGoogleCalendar(refreshToken,CalendarId,EventId);

                if (result)
                {
                    return NoContent(); //"Event Deleted Successfully"
                }
                else
                {
                    return NotFound("Error, check refresh token,calendar Id Or/And Event Id");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpGet("geteventbyid")]
        public IActionResult GetEventById(string refreshtoken, string calendarId, string eventId)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            }
            try
            {
                var token = new TokenResponse {
                    RefreshToken = refreshtoken
                };
                var credentials = new UserCredential(
                    new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer {
                        ClientSecrets = new ClientSecrets {
                            ClientId = "691007424503-f58024vnino9bon4i0tb0s53klrvblvf.apps.googleusercontent.com",
                            ClientSecret = "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"
                        }

                    }), "user", token);
                var service = new CalendarService(new BaseClientService.Initializer() {
                    HttpClientInitializer = credentials,
                });

                var Event =service.Events.Get(calendarId, eventId).Execute();
                if(Event == null)
                {
                    return NotFound();
                }
                return Ok(Event);
            }
            catch (Google.GoogleApiException gae)
            {
                if (gae.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(); // Event not found
                }
                else
                {

                    return StatusCode(500, "Failed to Get the event.");

                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "an Error occurred "+" " + e);
            }
        }
    }
}
