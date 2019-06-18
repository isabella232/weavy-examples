using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Weavy.Core.Helpers;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Areas.Apps.Models;
using Weavy.Web.Controllers;

namespace Weavy.Areas.Apps.Controllers {
    /// <summary>
    /// Controller for the <see cref="ZoomVideo"/>.
    /// </summary>
    [RoutePrefix("{id:int}/CE48D826-048C-44AB-B5D2-03F1FF20633F")]
    public class ZoomVideoController : AppController<ZoomVideo> {

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly string AppGuid = "CE48D826-048C-44AB-B5D2-03F1FF20633F";

        private object Cache(string key, object value = null) {
            string cacheKey = $"{AppGuid}/{key}";
            if (value != null) {
                HttpContext.Cache.Insert(cacheKey, value, null, DateTime.Now.AddMinutes(2), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return HttpContext.Cache.Get(cacheKey);
        }

        private void InvalidateCache(string key) {
            string cacheKey = $"{AppGuid}/{key}";
            HttpContext.Cache.Remove(cacheKey);
        }

        /// <summary>
        /// Display a list of video meetings.
        /// </summary>
        /// <param name="app">The app to display.</param>
        /// <param name="query">An object with query parameters for search, paging etc.</param>
        public override ActionResult Get(ZoomVideo app, Query query = null) {

            FetchUser(app, User.Email);
            FetchUsers(app);
            FetchDefaultUser(app);
            CheckExternalUser(app);
            FetchLiveMeetings(app);
            FetchUpcomingMeetings(app);

            if (Request.IsAjaxRequest()) {
                return PartialView("_Meetings", app);
            }

            return View(app);
        }

        /// <summary>
        /// Display a list of zoom users in the space.
        /// </summary>
        /// <param name="id">The current app id</param>
        [HttpGet]
        [Route("users")]
        public ActionResult Users(int id) {
            var app = (ZoomVideo)GetApp(id);

            FetchUser(app, User.Email);
            FetchUsers(app);

            return View(app);
        }

        /// <summary>
        /// Display a list of video meeting recordings.
        /// </summary>
        /// <param name="id">The current app id</param>
        [HttpGet]
        [Route("recordings")]
        public ActionResult RecordedMeetings(int id) {
            var app = (ZoomVideo)GetApp(id);

            FetchDefaultUser(app);
            FetchUsers(app);
            FetchRecordedMeetings(app);

            if (Request.IsAjaxRequest()) {
                return PartialView("_RecordedMeetings", app);
            }

            return View(app);
        }

        /// <summary>
        /// Display a list of video meeting recordings.
        /// </summary>
        /// <param name="id">The current app id</param>
        /// <param name="meetingId">The meeting id</param>
        [HttpGet]
        [Route("recordings/{meetingId:long}")]
        public ActionResult MeetingRecordings(int id, long meetingId) {
            var app = (ZoomVideo)GetApp(id);

            FetchMeetingRecordings(app, meetingId);

            if (Request.IsAjaxRequest()) {
                return PartialView("_MeetingRecordings", app);
            }

            return View(app);
        }

        /// <summary>
        /// Display meeting details.
        /// </summary>
        /// <param name="id">The current app id</param>
        /// <param name="meetingId">The meeting id</param>
        [HttpGet]
        [Route("meetings/{meetingId:long}")]
        public ActionResult MeetingDetails(int id, long meetingId) {
            var app = (ZoomVideo)GetApp(id);

            FetchUser(app, User.Email);
            FetchUsers(app);

            try {
                FetchMeetingDetails(app, meetingId);
            } catch (WebException ex) {
                var meetingError = GetZoomError<ZoomErrorResponse>(ex);
                Alert(AlertType.Warning, meetingError.message);
                return Redirect(app.Url());
            }

            if (app.MeetingDetails.type != ZoomMeetingType.Instant) {
                FetchMeetingRegistrants(app, meetingId);
            }

            if (Request.IsAjaxRequest()) {
                return PartialView("_MeetingDetails", app);
            }

            return View(app);
        }

        /// <summary>
        /// Register to a meeting.
        /// </summary>
        /// <param name="id">The current app id</param>
        /// <param name="meetingId">The meeting id</param>
        [HttpPost]
        [Route("meetings/{meetingId:long}/register")]
        public ActionResult MeetingRegister(int id, long meetingId) {
            var app = (ZoomVideo)GetApp(id);

            var firstName = User.Profile.Name.Split(' ')[0] ?? User.Username;
            var lastName = User.Profile.Name.Substring(User.Profile.Name.IndexOf(' ')) ?? User.Username;

            var registrant = new ZoomRegistrant() {
                email = User.Email,
                first_name = firstName,
                last_name = lastName
            };

            var meetingRegistration = GetZoomApi<ZoomRegistrationResponse>(app, $"/meetings/{meetingId}/registrants", "POST", registrant);

            if (!meetingRegistration.registrant_id.IsNullOrEmpty()) {
                InvalidateCache($"/meetings/{meetingId}/registrants");
                InvalidateMeetingCache(app);

                Alert("Successfully registered to " + meetingRegistration.topic);
            }


            return RedirectToAction(nameof(MeetingDetails), new { id, meetingId });
        }

        /// <summary>
        /// Make a new meeting.
        /// </summary>
        [HttpGet]
        [Route("meeting/schedule")]
        public ActionResult ScheduleMeeting(int id) {
            var app = (ZoomVideo)GetApp(id);

            app.MeetingTemplate = new ZoomMeetingTemplate() {
                topic = app.Space().Name + " meeting",
                type = ZoomMeetingType.Scheduled,
                agenda = $"Read more at {app.Url(true)}",
                start_time = DateTime.Now.AddHours(1),
                duration = 30
            };

            if (Request.IsAjaxRequest()) {
                return PartialView("_ScheduleMeeting", app);
            }

            return View(app);
        }

        /// <summary>
        /// Create and start a meeting.
        /// </summary>
        [HttpGet]
        [Route("meeting/instant")]
        public ActionResult InstantMeeting(int id) {
            var app = (ZoomVideo)GetApp(id);
            var space = app.Space();

            FetchUser(app, User.Email);

            if (app.UserHasZoom || app.HasDefaultUser) {
                _log.Info("Instant meeting");

                bool initialUsePmi = GetZoomApi<ZoomUser>(app, $"users/{(app.UserHasZoom ? User.Email : app.OptionDefaultUser)}").use_pmi;

                if (initialUsePmi) {
                    // Disable use_pmi
                    GetZoomApi<ZoomUser>(app, $"users/{(app.UserHasZoom ? User.Email : app.OptionDefaultUser)}", "PATCH", new { use_pmi = false });
                }

                ZoomMeetingTemplate meetingTemplate = new ZoomMeetingTemplate {
                    topic = space.Name + " instant meeting",
                    type = ZoomMeetingType.Instant,
                    agenda = $"Read more at {app.Url(true)}",
                    settings = new ZoomMeetingTemplateSettings {
                        alternative_hosts = app.UserHasZoom ? app.OptionDefaultUser : string.Empty,
                        auto_recording = "cloud",
                        host_video = true,
                        participant_video = true,
                        use_pmi = false
                    }
                };

                AddTracking(app, meetingTemplate);

                ZoomMeetingDetails meeting = GetZoomApi<ZoomMeetingDetails>(app, $"users/{(app.UserHasZoom ? User.Email : app.OptionDefaultUser)}/meetings", "POST", meetingTemplate);

                if (initialUsePmi) {
                    // Restore use_pmi
                    GetZoomApi<ZoomUser>(app, $"users/{(app.UserHasZoom ? User.Email : app.OptionDefaultUser)}", "PATCH", new { use_pmi = true });
                }
                //GetZoomApi<ZoomErrorResponse>(app, $"meetings/{meeting.id}?occurrence_id={meeting.uuid}", "PATCH", new ZoomMeetingTemplate { type = ZoomMeetingType.Instant });
                InvalidateMeetingCache(app);

                StartMeeting(app, meeting);
            } else {
                Alert(AlertType.Warning, "Unable to start meeting. No Zoom user account configured.");
            }

            return Redirect(app.Url());
        }

        /// <summary>
        /// Create and start a meeting.
        /// </summary>
        [HttpPost]
        [Route("meeting/create")]
        public ActionResult CreateMeeting(int id, ZoomMeetingTemplate meetingTemplate) {
            ZoomVideo app = (ZoomVideo)GetApp(id);
            Space space = app.Space();

            FetchUser(app, User.Email);

            if (app.UserHasZoom || app.HasDefaultUser) {
                _log.Info("Creating meeting");

                FetchUsers(app);

                if (meetingTemplate.settings == null) {
                    meetingTemplate.settings = new ZoomMeetingTemplateSettings {
                        alternative_hosts = app.UserHasZoom ? app.OptionDefaultUser : string.Empty,
                        auto_recording = "cloud",
                        host_video = true,
                        participant_video = true
                    };
                }

                if (app.OptionScheduleForDefaultUser && app.HasDefaultUser && app.UserHasZoom) {
                    AddDefaultUserAssistant(app, User.Email);
                    meetingTemplate.schedule_for = app.OptionDefaultUser;
                }

                AddTracking(app, meetingTemplate);

                GetZoomApi<ZoomMeetingDetails>(app, $"users/{(app.UserHasZoom ? User.Email : app.OptionDefaultUser)}/meetings", "POST", meetingTemplate);

                InvalidateMeetingCache(app);
            } else {
                Alert(AlertType.Warning, "Unable to create meeting. No Zoom user account configured.");
            }

            return Redirect(app.Url());
        }

        /// <summary>
        /// Endpoint for received zoom events configured for the Zoom app at the Zoom Marketplace
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Route("~/api/zoom/events")]
        public HttpStatusCodeResult ZoomEvent() {
            if (Request.Headers["Authorization"] != null && Request.Headers["Authorization"]?.Length == 22) {
                JObject jsonData;

                try {
                    var streamReader = new StreamReader(Request.GetBufferedInputStream());
                    jsonData = JObject.Parse(streamReader.ReadToEnd());
                } catch {
                    _log.Error("Event JSON data parsing failed");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                string eventName = "";
                string eventType = "";
                string[] eventNames = null;

                try {
                    eventName = jsonData.GetValue("event").ToString();

                    // reverse the order for weavy event naming conformance
                    eventNames = eventName.Split('.');
                    eventType = eventNames[0];
                    Array.Reverse(eventNames);
                    eventName = eventNames.Join(".", s => s);

                    eventName += ".zoom";
                } catch {
                    _log.Error("Event name parsing failed: " + eventName);
                    return new HttpStatusCodeResult(HttpStatusCode.NoContent);
                }

                string teamname = "";
                int spaceId = -1;

                try {
                    // Decode space tracking
                    if (new Regex("/meeting|recording|webinar/").IsMatch(eventType)) {
                        string topic = jsonData.SelectToken("payload.object.topic").ToObject<string>();

                        Match teamnameMatch = new Regex(@"@([a-zA-Z0-9_]+)(?:\s|$)").Match(topic);
                        if (teamnameMatch.Success) {
                            teamname = teamnameMatch.Groups[1].ToString();
                        }

                        Match spaceIdMatch = new Regex(@"#weavy(\d+)(?:\s|$)").Match(topic);
                        if (spaceIdMatch.Success) {
                            spaceId = Convert.ToInt32(spaceIdMatch.Groups[1].Value);
                        }
                    }
                } catch (Exception) {
                    _log.Warn("Could not decode event space tracking. \n" + jsonData.ToString());
                }

                List<ZoomVideo> zoomApps = null;
                try {
                    // Get all zoom video apps with the correct validation configured
                    // Match space tracking if available
                    List<ZoomVideo> zoomAppSearch = AppService.Search(new AppQuery() { Guids = new List<Guid>() { new Guid("CE48D826-048C-44AB-B5D2-03F1FF20633F") } }).ConvertAll(app => (ZoomVideo)app);
                    zoomApps = zoomAppSearch.FindAll(zoomApp => {
                        return !zoomApp.EventToken.IsNullOrDefault()
                        && zoomApp.EventToken == Request.Headers["Authorization"]?.Right(22)
                        && (teamname.IsNullOrEmpty() || zoomApp.Space().Teamname == teamname)
                        && (spaceId <= 0 || zoomApp.SpaceId == spaceId);
                    });
                } catch (Exception) {
                    _log.Warn("Could not match space to event token");
                }

                HashSet<int> pushUsers = new HashSet<int>();
                if (!zoomApps.IsNullOrEmpty()) {
                    foreach (ZoomVideo za in zoomApps) {
                        _log.Info("Pushing " + eventType + " event to space " + za.SpaceId + ": " + eventName);
                        // Invalidate cache
                        try {
                            InvalidateCacheByEvent(za, eventName, eventType);
                        } catch (Exception) {
                            _log.Warn("could not invalidate cache by event: " + eventName);
                        }

                        // Get all the users in the spaces with zoom apps
                        foreach (int mi in za.Space().MemberIds) {
                            pushUsers.Add(mi);
                        }
                    }
                }

                // Push the event 
                _log.Info("Event received: " + eventName);

#pragma warning disable CS4014
                PushService.PushToUsers(eventType + ".zoom", jsonData.ToObject<object>(), pushUsers);
                PushService.PushToUsers(eventName, jsonData.ToObject<object>(), pushUsers);
#pragma warning restore CS4014

                return new HttpStatusCodeResult(HttpStatusCode.Accepted);
            } else {
                _log.Warn("Event authorization failed");
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }

        private void FetchUser(ZoomVideo app, string user) {
            if (!user.IsNullOrEmpty()) {
                try {
                    app.User = GetZoomApi<ZoomUser>(app, $"users/{user}", throwExceptions: true);
                } catch (WebException) {
                    _log.Info("Zoom user not found: " + user);
                }
            }
        }

        private void FetchUsers(ZoomVideo app) {
            app.Users = GetZoomApi<ZoomUsers>(app, "users");

            if (app.Users != null && app.Users.users != null) {
                app.UserMembers = SpaceService.GetMembers(app.SpaceId.GetValueOrDefault()).ConvertAll(m => new ZoomUserMember { Member = m });
                app.UserMembers.ForEach(um => um.User = Array.Find(app.Users.users, user => user.email != null && user.email == um.Member.Email));
                app.UserMembers.RemoveAll(um => um.User == null);
            }
        }

        private void FetchDefaultUser(ZoomVideo app) {
            if (!app.OptionDefaultUser.IsNullOrWhiteSpace()) {
                try {
                    app.DefaultUser = GetZoomApi<ZoomUser>(app, $"users/{app.OptionDefaultUser}", throwExceptions: true);
                } catch (WebException) {
                    Alert(AlertType.Warning, $"Could not get Default Zoom User ({app.OptionDefaultUser}). Check your tab settings or try again.");
                }
            }
        }

        private void FetchDefaultUserAssistants(ZoomVideo app) {
            if (!app.OptionDefaultUser.IsNullOrWhiteSpace()) {
                app.DefaultUserAssistants = GetZoomApi<ZoomAssistants>(app, $"users/{app.OptionDefaultUser}/assistants")?.assistants;
            }
        }

        private void AddDefaultUserAssistant(ZoomVideo app, string useremail) {
            if (!app.OptionDefaultUser.IsNullOrWhiteSpace()) {
                FetchDefaultUserAssistants(app);

                if (!app.DefaultUserAssistants.Any(ua => ua.email == useremail)) {
                    var assistants = new ZoomAssistants() {
                        assistants = new ZoomAssistant[] {
                            new ZoomAssistant() { email = useremail }
                        }
                    };
                    GetZoomApi<ZoomAssistants>(app, $"users/{app.OptionDefaultUser}/assistants", "POST", assistants);
                }
            }
        }

        private void AddDefaultUserAssistants(ZoomVideo app) {
            app.UserMembers.ForEach(um => AddDefaultUserAssistant(app, um.Member.Email));
        }

        private void CheckExternalUser(ZoomVideo app) {
            if (!User.Email.IsNullOrEmpty()) {
                try {
                    app.UserHasExternalZoom = GetZoomApi<ZoomEmailResponse>(app, $"users/email?email={User.Email}", throwExceptions: true).existed_email;
                } catch (WebException) {
                    Alert(AlertType.Warning, $"Could not verify your zoom user email ({User.Email}). Try again in a few seconds or contact your administrator.");
                }
            }
        }


        private void FetchRecordedMeetings(ZoomVideo app) {
            /*string from = DateTime.UtcNow.AddDays(-182).ToString("yyyy-MM-dd");
            string to = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");

            ZoomDashboardMeetings pastMeetings = GetZoomApi<ZoomDashboardMeetings>(app, $"metrics/meetings?type=past&page_size=300&from={from}&to={to}");
            ZoomDashboardMeetings pastOneMeetings = GetZoomApi<ZoomDashboardMeetings>(app, $"metrics/meetings?type=pastOne&page_size=300&from={from}&to={to}");

            if (pastMeetings != null && pastMeetings.meetings != null) {
                app.RecordedMeetings = CurrentAppZoomMeetings(app, pastMeetings.meetings).Where(pm => pm.has_recording).ToArray();
            }
            if (pastOneMeetings != null && pastOneMeetings.meetings != null) {
                if(app.RecordedMeetings == null) {
                    app.RecordedMeetings = CurrentAppZoomMeetings(app, pastOneMeetings.meetings).Where(pm => pm.has_recording).ToArray();
                } else {
                    app.RecordedMeetings = app.RecordedMeetings.Concat(CurrentAppZoomMeetings(app, pastOneMeetings.meetings).Where(pm => pm.has_recording).ToArray()).ToArray();
                    app.RecordedMeetings.OrderByDescending(pm => pm.start_time);
                }
            }
            */
            string from = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-01");
            string to = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-01");

            ZoomMeetingRecordingsResult recordedMeetings = null;
            ZoomMeetingRecordings[] meetings = new ZoomMeetingRecordings[0];

            if (!app.OptionDefaultUser.IsNullOrWhiteSpace() && app.OptionDefaultUser != User.Email) {
                recordedMeetings = GetZoomApi<ZoomMeetingRecordingsResult>(app, $"users/{app.OptionDefaultUser}/recordings?page_size=300&from={from}&to={to}");
            }
            if (recordedMeetings != null && recordedMeetings.meetings != null) {
                meetings = meetings.Concat(recordedMeetings.meetings).ToArray();
            }

            if (!app.UserMembers.IsNullOrEmpty()) {
                foreach (ZoomUserMember um in app.UserMembers) {
                    ZoomMeetingRecordingsResult userMeetings = GetZoomApi<ZoomMeetingRecordingsResult>(app, $"users/{um.Member.Email}/recordings?page_size=300&from={from}&to={to}");
                    if (userMeetings != null && userMeetings.meetings != null) {
                        meetings = meetings.Concat(userMeetings.meetings).ToArray();
                    }
                }
            }

            meetings = meetings.DistinctBy(m => m.uuid).ToArray();
            meetings.OrderByDescending(m => m.start_time);

            app.RecordedMeetings = CurrentAppZoomMeetings(app, meetings);
            /*app.RecordedMeetings = CurrentAppZoomMeetings(app, meetings).Select(m => new ZoomExtendedMeeting {
                meeting = m,
                details = GetZoomApi<ZoomMeetingDetails>(app, $"meetings/{m.id}"),
                host = app.UserMembers.Find(um => um.User.id == m.host_id)
            }).ToArray();*/
        }

        private void FetchLiveMeetings(ZoomVideo app) {

            ZoomDashboardMeetings liveMeetings = GetZoomApi<ZoomDashboardMeetings>(app, "metrics/meetings");

            if (liveMeetings != null && liveMeetings.meetings != null) {
                app.LiveMeetings = CurrentAppZoomMeetings(app, liveMeetings.meetings);
            }

            /*ZoomDashboardMeetings liveMeetings = null;
            ZoomMeeting[] meetings = new ZoomMeeting[0];

            if (!app.OptionDefaultUser.IsNullOrWhiteSpace() && app.OptionDefaultUser != User.Email) {
                liveMeetings = GetZoomApi<ZoomDashboardMeetings>(app, $"users/{app.OptionDefaultUser}/meetings?page_size=300");
            }
            if (liveMeetings != null && liveMeetings.meetings != null) {
                meetings = meetings.Concat(liveMeetings.meetings).ToArray();
            }

            if (app.UserMembers.IsNullOrEmpty()) {
                if (!User.Email.IsNullOrEmpty()) {
                    ZoomDashboardMeetings liveUserMeetings = GetZoomApi<ZoomDashboardMeetings>(app, $"users/{User.Email}/meetings?page_size=300");
                    if (liveUserMeetings != null && liveUserMeetings.meetings != null) {
                        meetings = meetings.Concat(liveUserMeetings.meetings).ToArray();
                    }
                }
            } else {
                foreach (ZoomUserMember um in app.UserMembers) {
                    ZoomDashboardMeetings liveUserMeetings = GetZoomApi<ZoomDashboardMeetings>(app, $"users/{um.Member.Email}/meetings?page_size=300");
                    if (liveUserMeetings != null && liveUserMeetings.meetings != null) {
                        meetings = meetings.Concat(liveUserMeetings.meetings).ToArray();
                    }
                }
            }

            meetings = meetings.DistinctBy(m => m.uuid).ToArray();
            meetings.OrderByDescending(m => m.start_time);

            app.LiveMeetings = CurrentAppZoomMeetings(app, meetings).Select(m => new ZoomExtendedMeeting {
                meeting = m,
                details = GetZoomApi<ZoomMeetingDetails>(app, $"meetings/{m.id}"),
                host = app.UserMembers.Find(um => um.User.id == m.host_id)
            }).ToArray();

            app.LiveMeetings = app.LiveMeetings.Where(lm => lm.details.status == "started" || (lm.details.type == ZoomMeetingType.Instant && lm.details.status != "finished")).ToArray();*/
        }

        private void FetchUpcomingMeetings(ZoomVideo app) {
            ZoomDashboardMeetings upcomingMeetings = null;
            ZoomMeeting[] meetings = new ZoomMeeting[0];

            if (!app.OptionDefaultUser.IsNullOrWhiteSpace() && app.OptionDefaultUser != User.Email) {
                upcomingMeetings = GetZoomApi<ZoomDashboardMeetings>(app, $"users/{app.OptionDefaultUser}/meetings?type=upcoming");
            }
            if (upcomingMeetings != null && upcomingMeetings.meetings != null) {
                meetings = meetings.Concat(upcomingMeetings.meetings).ToArray();
            }

            if (app.UserMembers.IsNullOrEmpty()) {
                if (!User.Email.IsNullOrEmpty()) {
                    ZoomDashboardMeetings upcomingUserMeetings = GetZoomApi<ZoomDashboardMeetings>(app, $"users/{User.Email}/meetings?type=upcoming");
                    if (upcomingUserMeetings != null && upcomingUserMeetings.meetings != null) {
                        meetings = meetings.Concat(upcomingUserMeetings.meetings).ToArray();
                    }
                }
            } else {
                foreach (ZoomUserMember um in app.UserMembers) {
                    ZoomDashboardMeetings upcomingUserMeetings = GetZoomApi<ZoomDashboardMeetings>(app, $"users/{um.Member.Email}/meetings?type=upcoming");
                    if (upcomingUserMeetings != null && upcomingUserMeetings.meetings != null) {
                        meetings = meetings.Concat(upcomingUserMeetings.meetings).ToArray();
                    }
                }
            }

            meetings = meetings.DistinctBy(m => m.uuid).ToArray();
            meetings.OrderByDescending(m => m.start_time);

            app.UpcomingMeetings = CurrentAppZoomMeetings(app, meetings).Select(m => new ZoomExtendedMeeting {
                meeting = m,
                details = GetZoomApi<ZoomMeetingDetails>(app, $"meetings/{m.id}"),
                host = app.UserMembers.Find(um => um.User.id == m.host_id)
            }).ToArray();
        }

        private void FetchMeetingDetails(ZoomVideo app, long meetingId) {
            app.MeetingDetails = GetZoomApi<ZoomMeetingDetails>(app, $"meetings/{meetingId}", throwExceptions: true);
        }

        private void FetchMeetingRegistrants(ZoomVideo app, long meetingId) {
            app.MeetingRegistrants = GetZoomApi<ZoomMeetingRegistrants>(app, $"meetings/{meetingId}/registrants");
        }

        private void FetchMeetingRecordings(ZoomVideo app, long meetingId) {
            ZoomMeetingRecordings meetingRecordings = GetZoomApi<ZoomMeetingRecordings>(app, $"meetings/{meetingId}/recordings");

            app.MeetingRecordings = meetingRecordings.recording_files.DistinctBy(rf => rf.recording_start.Ticks).OrderBy(rf => rf.recording_start).ToArray();
        }

        private void StartMeeting(ZoomVideo app, ZoomMeetingDetails meeting) {
            if (meeting != null && meeting.start_url != null) {
                var space = app.Space();

                foreach (int memberId in space.MemberIds) {
                    string notificationMessage = space.Teamname.IsNullOrEmpty() ?
                        $@"<strong>@{User.Username}</strong> started a Zoom meeting in <strong>{space.Name}</strong>" :
                        $@"<strong>@{User.Username}</strong> started a Zoom meeting with <strong>@{space.Teamname}</strong>";
                    NotificationService.Insert(
                       new Notification(memberId, notificationMessage) { LinkUrl = $"{app.Url()}/{AppGuid}/meetings/{meeting.id}" }
                    );
                }

                Script($"wvy.zoom.start(\"{meeting.start_url}\", {meeting.id.ToString()}, \"{meeting.topic.AttributeSafe()}\");");
            } else {
                Alert(AlertType.Warning, "Error starting meeting");
            }
        }


        private void InvalidateMeetingUserCache(string user) {
            if (!user.IsNullOrEmpty()) {
                InvalidateCache($"users/{user}/meetings");
                InvalidateCache($"users/{user}/meetings?page_size=300");
                InvalidateCache($"users/{user}/meetings?type=upcoming");
            }
        }

        private void InvalidateMeetingCache(ZoomVideo app) {
            InvalidateCache("metrics/meetings");
            InvalidateMeetingUserCache(User.Email);
            InvalidateMeetingUserCache(app.OptionDefaultUser);

            FetchUsers(app);

            foreach (var um in app.UserMembers) {
                InvalidateMeetingUserCache(um.User.email);
                InvalidateMeetingUserCache(um.User.id);
            };
        }

        private void InvalidateRecordingCache(ZoomVideo app) {
            string from = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-01");
            string to = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-01");

            FetchUsers(app);

            InvalidateCache($"users/{app.OptionDefaultUser}/recordings?page_size=300&from={from}&to={to}");

            foreach (var um in app.UserMembers) {
                InvalidateCache($"users/{um.User.email}/recordings?page_size=300&from={from}&to={to}");
            };

        }

        private void InvalidateUserCache(ZoomVideo app) {
            FetchUsers(app);

            InvalidateCache("users");
            InvalidateCache($"users/email?email={User.Email}");

            foreach (var um in app.UserMembers) {
                InvalidateCache($"users/{um.User.id}");
                InvalidateCache($"users/{um.User.email}");
            }

            if (!app.OptionDefaultUser.IsNullOrEmpty()) {
                InvalidateCache($"users/{app.OptionDefaultUser}");
                InvalidateCache($"users/{app.OptionDefaultUser}/assistants");
            }
        }

        private void InvalidateCacheByEvent(ZoomVideo app, string eventName, string eventType) {
            if (eventType == "meeting") {
                InvalidateMeetingCache(app);
            }
            if (eventType == "recording") {
                InvalidateRecordingCache(app);
            }
            if (eventType == "user") {
                InvalidateUserCache(app);
            }

        }


        private void AddTracking(ZoomVideo app, ZoomMeetingTemplate meetingTemplate) {
            Space space = app.Space();

            string trackingTag = space.Teamname.IsNullOrEmpty() ? "#weavy" + app.SpaceId.ToString() : "@" + space.Teamname;
            if (meetingTemplate.topic.IsNullOrEmpty()) {
                meetingTemplate.topic = trackingTag;
            } else {
                meetingTemplate.topic += " " + trackingTag;
            }

            if (meetingTemplate.tracking_fields == null) {
                meetingTemplate.tracking_fields = new ZoomTrackingField[] { };
            }
            meetingTemplate.tracking_fields.Append(new ZoomTrackingField { field = "weavy_space", value = app.SpaceId.ToString() });
            meetingTemplate.tracking_fields.Append(new ZoomTrackingField { field = "weavy_app", value = app.Id.ToString() });
        }

        private T[] CurrentAppZoomMeetings<T>(ZoomVideo app, T[] meetings) where T : ZoomMeeting {
            var spaceMembers = SpaceService.GetMembers(app.SpaceId ?? -1);

            return meetings.Where(meeting => {
                //_log.Info("Zoom Matching teamname in \"" + meeting.topic + "\": " + new Regex(@"@([a-zA-Z0-9_]+)(?:\s|$)").IsMatch(meeting.topic).ToString());
                bool hasTeamname = matchTeamname(app, meeting);
                bool hasSpaceId = matchSpaceId(app, meeting);

                return hasTeamname || hasSpaceId;
            }).ToArray();
        }

        private bool matchTeamname(ZoomVideo app, ZoomMeeting meeting) {
            string teamname = "";
            bool hasTeamname = false;

            Match teamnameMatch = new Regex(@"@([a-zA-Z0-9_]+)(?:\s|$)").Match(meeting.topic);
            if (teamnameMatch.Success) {
                teamname = teamnameMatch.Groups[1].ToString();
                _log.Info("Zoom meeting has teamname: " + teamname);
                hasTeamname = app.Space().Teamname == teamname;
                if (app.OptionRemoveTags && hasTeamname) {
                    meeting.topic = meeting.topic.Remove(teamnameMatch.Index, teamnameMatch.Length);
                }
            }
            return hasTeamname;
        }

        private bool matchSpaceId(ZoomVideo app, ZoomMeeting meeting) {
            int spaceId = -1;
            bool hasSpaceId = false;

            Match spaceIdMatch = new Regex(@"#weavy(\d+)(?:\s|$)").Match(meeting.topic);
            if (spaceIdMatch.Success) {
                spaceId = Convert.ToInt32(spaceIdMatch.Groups[1].Value);
                _log.Info("Zoom meeting has spaceId: " + spaceId.ToString());
                hasSpaceId = app.SpaceId == spaceId;
                if (app.OptionRemoveTags && hasSpaceId) {
                    meeting.topic = meeting.topic.Remove(spaceIdMatch.Index, spaceIdMatch.Length);
                }
            }
            return hasSpaceId;
        }

        private T GetZoomError<T>(WebException ex) where T : ZoomErrorResponse {
            if (ex.Status == WebExceptionStatus.ProtocolError) {
                var response = ex.Response as HttpWebResponse;
                if (response != null) {
                    var streamReader = new StreamReader(response.GetResponseStream());
                    string responseString = streamReader.ReadToEnd();

                    if (responseString.IsNullOrWhiteSpace()) {
                        return (T)new ZoomErrorResponse { code = (ZoomErrorCode)response.StatusCode, message = response.StatusDescription };
                    }

                    return JObject.Parse(responseString).ToObject<T>();
                }
            }

            return (T)new ZoomErrorResponse { code = ZoomErrorCode.BadRequest, message = ex.Message };
        }

        private T GetZoomApi<T>(ZoomVideo app, string api, string method = null, object arguments = null, bool forceCache = false, bool throwExceptions = false) {
            bool cached = forceCache || (method == null && arguments == null && api.IndexOf("from=") == -1 && api.IndexOf("to=") == -1);

            if (cached) {
                string cachedItem = (string)Cache(api);
                if (!app.OptionDisableCache && cachedItem != null) {
                    _log.Info("Cache: " + api);
                    return JObject.Parse(cachedItem).ToObject<T>();
                }
            }

            var zoomApi = "https://api.zoom.us/v2/";
            var requestUri = new UriBuilder(zoomApi + api);

            var request = WebRequest.Create(requestUri.Uri);

            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + app.ApiToken);

            if (method != null) {
                request.Method = method;
            }

            if (arguments != null) {
                string jsonArguments = arguments.SerializeToJson(settings: new JsonSerializerSettings {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore
                });
                byte[] jsonData = Encoding.UTF8.GetBytes(jsonArguments);

                request.ContentType = "application/json";
                request.ContentLength = jsonData.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(jsonData, 0, jsonData.Length);
            }

            try {
                _log.Info("Fetching: " + api);
                var response = request.GetResponse();
                var streamReader = new StreamReader(response.GetResponseStream());
                string responseString = streamReader.ReadToEnd();

                if (responseString.IsNullOrWhiteSpace()) {
                    return new JObject().ToObject<T>();
                }

                if (cached) {
                    Cache(api, responseString);
                }
                return JObject.Parse(responseString).ToObject<T>();
            } catch (WebException ex) {
                _log.Warn("Could not fetch Zoom API: " + ex.Message);
                if (throwExceptions) {
                    throw ex;
                }
                if (ex.Status == WebExceptionStatus.ProtocolError) {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null && !app.OptionDisableErrors) {
                        if (response.StatusCode == HttpStatusCode.Unauthorized) {
                            Alert(AlertType.Warning, "Could not authorize Zoom, try again in a few seconds or check your configuration.");
                        } else if ((int)response.StatusCode == 429) {
                            Alert(AlertType.Warning, "Could not fetch Zoom data, try again in a few seconds.");
                        } else {
                            Alert(AlertType.Warning, "Could not fetch Zoom data, see logs for more info.");
                        }
                    }
                }
            }

            return new JObject().ToObject<T>();
        }

    }

}
