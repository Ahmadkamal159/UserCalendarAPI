using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System.Text;
using UserCalendarAPI.Common;
using UserCalendarAPI.DTOs;
using UserCalendarAPI.IServices;

namespace UserCalendarAPI.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly HttpClient _httpClient;

        public GoogleCalendarService()
        {
            _httpClient = new HttpClient();
        }

        public string GetAuthCode()
        {
            try
            {

                string scopeURL1 = "https://accounts.google.com/o/oauth2/auth?redirect_uri={0}&prompt={1}&response_type={2}&client_id={3}&scope={4}&access_type={5}";
                var redirectURL = "https://localhost:44387/api/events/google-callback";
                string prompt = "consent";
                string response_type = "code";
                string clientID = "691007424503-f58024vnino9bon4i0tb0s53klrvblvf.apps.googleusercontent.com";
                string scope = "https://www.googleapis.com/auth/calendar";
                string access_type = "offline";
                string redirect_uri_encode = Method.urlEncodeForGoogle(redirectURL);
                var mainURL = string.Format(scopeURL1, redirect_uri_encode, prompt, response_type, clientID, scope, access_type);

                return mainURL;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public async Task<GoogleTokenResponse> GetTokens(string code)
        {

            var clientId = "691007424503-f58024vnino9bon4i0tb0s53klrvblvf.apps.googleusercontent.com";
            string clientSecret = "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";
            var redirectURL = "https://localhost:44387/api/events/google-callback";
            var tokenEndpoint = "https://accounts.google.com/o/oauth2/token";
            var content = new StringContent($"code={code}&redirect_uri={Uri.EscapeDataString(redirectURL)}&client_id={clientId}&client_secret={clientSecret}&grant_type=authorization_code", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);
                return tokenResponse;
            }
            else
            {
                // Handle the error case when authentication fails
                throw new Exception($"Failed to authenticate: {responseContent}");
            }
        }
        public IEnumerable<CalendarListEntry> GetGoogleCalendars(string refreshToken)
        {
            try
            {
                // Create the CalendarService as you did in the previous methods
                var token = new TokenResponse {
                    RefreshToken = refreshToken
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

                CalendarList calendarList = service.CalendarList.List().Execute();
                IEnumerable<CalendarListEntry> Calendaritems = calendarList.Items;


                if (Calendaritems != null && Calendaritems.Any())
                {
                    return Calendaritems;
                }
                else
                {
                    return new List<CalendarListEntry>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string AddGoogleCalendar(GoogleCalendarReqDTO googleCalendarReqDTO)
        {
            try
            {
                var token = new TokenResponse {
                    RefreshToken = googleCalendarReqDTO.refreshToken
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

               Calendar NewCalendar= new Calendar() {
               Summary= googleCalendarReqDTO.Summary
               };
                CalendarsResource.InsertRequest insertRequest=service.Calendars.Insert(NewCalendar);
                Calendar createdcalendar = insertRequest.Execute();
               
                return createdcalendar.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return e.Message;
            }
        }

        public string AddToGoogleCalendar(GoogleCalendarReqDTO googleCalendarReqDTO)
        {
            try
            {
                var token = new TokenResponse {
                    RefreshToken = googleCalendarReqDTO.refreshToken
                };
                var credentials = new UserCredential(
                    new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                      ClientSecrets = new ClientSecrets {
                          ClientId = "691007424503-f58024vnino9bon4i0tb0s53klrvblvf.apps.googleusercontent.com",
                          ClientSecret = "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"
                      }

                    }), "user", token);

                var service = new CalendarService(new BaseClientService.Initializer() {
                    HttpClientInitializer = credentials,
                });

                Event newEvent = new Event() {
                    Summary = googleCalendarReqDTO.Summary,
                    Description = googleCalendarReqDTO.Description,
                    Start = new EventDateTime() {
                        DateTime = googleCalendarReqDTO.StartTime,
                        //TimeZone = Method.WindowsToIana()    //user's time zone
                    },
                    End = new EventDateTime() {
                        DateTime = googleCalendarReqDTO.EndTime,
                        //TimeZone = Method.WindowsToIana();    //user's time zone
                    },
                    Reminders = new Event.RemindersData() {
                        UseDefault = false,
                        Overrides = new EventReminder[] 
                        {

                         new EventReminder() 
                         {
                             Method = "email", Minutes = 30
                         },
                         
                         new EventReminder()
                         {
                             Method = "popup", Minutes = 15
                         },
                         
                         new EventReminder() 
                         {
                           Method = "popup", Minutes = 1
                         },
                        }
                    }

                };

                EventsResource.InsertRequest insertRequest = service.Events.Insert(newEvent, googleCalendarReqDTO.CalendarId);
                Event createdEvent = insertRequest.Execute();
                return createdEvent.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }
        }

        public IEnumerable<Event> GetGoogleCalendarEvents(DateTime? startDate, DateTime? endDate, string? searchQuery,string refreshToken,string calendarId)
        {
            try
            {
                // Create the CalendarService as you did in the previous methods
                var token = new TokenResponse {
                    RefreshToken = refreshToken
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

                // Create a list request for the user's primary calendar
                EventsResource.ListRequest listRequest = service.Events.List(calendarId);

                // Set optional query parameters
                if (startDate != null)
                {
                    listRequest.TimeMin = startDate;
                }
                if (endDate != null)
                {
                    listRequest.TimeMax = endDate;
                }
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    listRequest.Q = searchQuery;
                }

                // Execute the list request and get the events
                Events events = listRequest.Execute();

                if (events.Items != null && events.Items.Any())
                {
                    return events.Items;
                }
                else
                {
                    return new List<Event>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public bool DeleteEventFromGoogleCalendar(string refreshToken,string CalendarId,string eventId)
        {
            try
            {
                var token = new TokenResponse {
                    RefreshToken = refreshToken
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



                service.Events.Delete(CalendarId, eventId).Execute();

                return true;
            }
            catch (Google.GoogleApiException gae)
            {
                if (gae.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false; // Event not found
                }
                else
                {
                    Console.WriteLine("Google Calendar API Error: " + gae.Message);
                    // Log the error or take appropriate action
                    throw;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }


}
