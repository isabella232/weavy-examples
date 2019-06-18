using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Weavy.Core.Helpers;
using Weavy.Core.Models;
using JWT.Algorithms;
using JWT.Builder;

namespace Weavy.Areas.Apps.Models {
    /// <summary>
    /// View model for the ZoomVideo app
    /// </summary>
    [Serializable]
    [Guid("CE48D826-048C-44AB-B5D2-03F1FF20633F")]
    [App(Icon = "video", Color = "blue", Name = "Zoom", Description = "Zoom Video Conferencing. Requires Pro or Corp Zoom account.", AllowMultiple = false)]
    public class ZoomVideo : App {

        /// <summary>
        /// The API key found in the app configuration in Zoom App Marketplace
        /// </summary>
        [Required]
        [Display(Name = "Zoom API Key", Description = "The API key found in your app configuration in Zoom App Marketplace")]
        public string ApiKey { get; set; }

        /// <summary>
        /// The API secret found in the app configuration in Zoom App Marketplace
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Zoom API Secret", Description = "The API secret found in your app configuration in Zoom App Marketplace")]
        public string ApiSecret { get; set; }

        /// <summary>
        /// The token for event subscription in the app configuration in Zoom App Marketplace
        /// </summary>
        [Display(Name = "Zoom Event Notification Token", Description = "The verification token for event subscription in your app configuration in Zoom App Marketplace. Make sure the Zoom event subscription points to https://[yourinstallation]/api/zoom/events")]
        public string EventToken { get; set; }

        /// <summary>
        /// Get the encoded API JSON web token.
        /// </summary>
        public string ApiToken {
            get {
                IDictionary<string, string> claims = new Dictionary<string, string>() {
                    { "iss", ApiKey },
                    { "exp", DateTime.UtcNow.AddMinutes(10).ToUnixTime().ToString() }
                };

                JwtBuilder builder = new JwtBuilder().WithAlgorithm(new HMACSHA256Algorithm()).WithSecret(ApiSecret);

                foreach (var claim in claims) {
                    builder.AddClaim(claim.Key, claim.Value);
                }
                return builder.Build();
            }
        }

        /// <summary>
        /// Fallback user for scheduling when users don't have a connected Zoom user account
        /// </summary>
        [Display(Name = "Default Zoom User", Description = "Used for scheduling when users don't have a connected Zoom user account or as default co-host for meetings.")]
        public string OptionDefaultUser { get; set; }

        /// <summary>
        /// Hide the modal when starting a meeting. Not recommended
        /// </summary>
        [Display(Name = "Schedule for Default Zoom User", Description = "Schedule all created meetings for the Default Zoom User as well")]
        public bool OptionScheduleForDefaultUser { get; set; } = false;

        /// <summary>
        /// Hide the modal when starting a meeting. Not recommended
        /// </summary>
        [Display(Name = "Hide Launch Modal", Description = "Hide the modal when starting a meeting except when user doesn't have a Zoom Account.")]
        public bool OptionHideProgress { get; set; } = false;

        /// <summary>
        /// Remove the weavy tagging from titles
        /// </summary>
        [Display(Name = "Hide space tagging", Description = "Removes the space tags from topic in listing.")]
        public bool OptionRemoveTags { get; set; } = true;

        /// <summary>
        /// Hides all error alerts.
        /// </summary>
        [Display(Name = "Disable error messages", Description = "Hides all error alerts.")]
        public bool OptionDisableErrors { get; set; } = false;

        /// <summary>
        /// Skips caching. Might be more accurate but slower.
        /// </summary>
        [Display(Name = "Disable cache", Description = "Skips caching. Might be more accurate but slower.")]
        public bool OptionDisableCache { get; set; } = false;

        /// <summary>
        /// Current Zoom User
        /// </summary>
        public ZoomUser User { get; internal set; }

        /// <summary>
        /// Zoom users
        /// </summary>
        public ZoomUsers Users { get; internal set; }

        /// <summary>
        /// Members that are Zoom users
        /// </summary>
        public List<ZoomUserMember> UserMembers { get; internal set; }

        /// <summary>
        /// Default zoom user for scheduling
        /// </summary>
        public ZoomUser DefaultUser { get; internal set; }

        /// <summary>
        /// Assistants for the Default User
        /// </summary>
        public ZoomAssistant[] DefaultUserAssistants { get; internal set; }

        /// <summary>
        /// Indicates if the user has zoom
        /// </summary>
        private bool _userHasZoom;
        public bool UserHasZoom {
            get => _userHasZoom || User != null;
            // May be explicitly set
            internal set => _userHasZoom = value;
        }

        /// <summary>
        /// Indicates if the user has zoom outside the pro/corp account
        /// </summary>
        public bool UserHasExternalZoom { get; internal set; }

        /// <summary>
        /// Indicates if there is a default zoom user
        /// </summary>
        public bool HasDefaultUser {
            get => !OptionDefaultUser.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Lists all live meetings
        /// </summary>
        public ZoomDashboardMeeting[] LiveMeetings { get; internal set; }

        /// <summary>
        /// Lists all upcoming meetings
        /// </summary>
        public ZoomExtendedMeeting[] UpcomingMeetings { get; internal set; }


        /// <summary>
        /// Lists all recorded meetings
        /// </summary>
        public ZoomMeetingRecordings[] RecordedMeetings { get; internal set; }

        /// <summary>
        /// Template for new meetings
        /// </summary>
        public ZoomMeetingTemplate MeetingTemplate { get; internal set; }

        /// <summary>
        /// Details for a meeting
        /// </summary>
        public ZoomMeetingDetails MeetingDetails { get; internal set; }

        /// <summary>
        /// Details for a meeting
        /// </summary>
        public ZoomMeetingRegistrants MeetingRegistrants { get; internal set; }

        /// <summary>
        /// Recordings for a meeting
        /// </summary>
        public ZoomRecordingFile[] MeetingRecordings { get; internal set; }
    }
}

// Disable documentation warning for Zoom API
// See https://zoom.github.io/api/
// See https://marketplace.zoom.us/docs/api-reference/

#pragma warning disable CS1591

/// <summary>
/// Zoom API Users
/// </summary>
public class ZoomUsers {
    public long page_count { get; set; }
    public long page_number { get; set; }
    public long page_size { get; set; }
    public long total_records { get; set; }
    public ZoomUser[] users { get; set; }
}

/// <summary>
/// Zoom API User
/// </summary>
public class ZoomUser {
    public string id { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string email { get; set; }
    public int type { get; set; }
    public long pmi { get; set; }
    public bool use_pmi { get; set; }
    public string timezone { get; set; }
    public int verified { get; set; }
    public string dept { get; set; }
    public DateTime created_at { get; set; }
    public DateTime last_login_time { get; set; }
    public string last_client_version { get; set; }
    public string pic_url { get; set; }
}

/// <summary>
/// Combined Zoom Api User and Space Member
/// </summary>
public class ZoomUserMember {
    public ZoomUser User { get; set; }
    public Member Member { get; set; }
}

/// <summary>
/// Zoom Api Meeting Types
/// </summary>
public enum ZoomMeetingType {
    Instant = 1,
    Scheduled = 2,
    Recurring = 3,
    RecurringFixed = 8
}

/// <summary>
/// Zoom Api Dashboard Meetings
/// </summary>
public class ZoomDashboardMeetings {
    public DateTime from { get; set; }
    public DateTime to { get; set; }
    public long page_count { get; set; }
    public long page_size { get; set; }
    public long total_records { get; set; }
    public string next_page_token { get; set; }
    public ZoomDashboardMeeting[] meetings { get; set; }
}

public class ZoomExtendedMeeting {
    public long id { get; set; }
    public ZoomMeeting meeting { get; set; }
    public ZoomMeetingDetails details { get; set; }
    public ZoomUserMember host { get; set; }
}

/// <summary>
/// Zoom Api Meeting
/// </summary>
public class ZoomMeeting {
    public string uuid { get; set; }
    public long id { get; set; }
    public string host_id { get; set; }
    public string topic { get; set; }
    public ZoomMeetingType type { get; set; }
    public DateTime start_time { get; set; }
    public string duration { get; set; }
    public string timezone { get; set; }
    public DateTime created_at { get; set; }
    public string join_url { get; set; }
}

public class ZoomDashboardMeeting : ZoomMeeting {
    public string host { get; set; }
    public string email { get; set; }
    public string user_type { get; set; }
    public DateTime? end_time { get; set; }
    public long participants { get; set; }
    public bool has_pstn { get; set; }
    public bool has_voip { get; set; }
    public bool has_3rd_party_audio { get; set; }
    public bool has_video { get; set; }
    public bool has_screen_share { get; set; }
    public bool has_recording { get; set; }
    public bool has_sip { get; set; }
}


public class ZoomMeetingTemplate {
    public ZoomMeetingTemplate() {
        topic = "";
        type = ZoomMeetingType.Scheduled;
    }

    [Display(Name = "Topic", Description = "The topic for the meeting")]
    public string topic { get; set; }

    [Display(Name = "Type", Description = "The type of meeting")]
    public ZoomMeetingType type { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(Name = "Agenda", Description = "Meeting description")]
    public string agenda { get; set; }

    public ZoomTrackingField[] tracking_fields { get; internal set; }

    [Display(Name = "Start time", Description = "Meeting start time. Only used for scheduled and recurring meetings.")]
    public DateTime? start_time { get; set; }

    [Display(Name = "Duration", Description = "Duration of the meeting in minutes. Used for scheduled meetings only.")]
    public long? duration { get; set; }

    public string timezone { get; internal set; }

    [DataType(DataType.Password)]
    [Display(Name = "Password", Description = "Optional password for the meeting")]
    public string password { get; set; }

    public string schedule_for { get; internal set; }

    public ZoomMeetingTemplateRecurrence recurrence { get; set; }
    public ZoomMeetingTemplateSettings settings { get; set; }
}

public class ZoomMeetingTemplateRecurrence {
    public int type { get; set; }
    public int repeat_interval { get; set; }
    public int weekly_days { get; set; }
    public int monthly_day { get; set; }
    public int monthly_week { get; set; }
    public int monthly_week_day { get; set; }
    public int end_times { get; set; }
    public DateTime end_date_time { get; set; }
}

public class ZoomMeetingTemplateSettings {
    public bool host_video { get; set; }
    public bool participant_video { get; set; }
    public bool cn_meeting { get; set; }
    public bool in_meeting { get; set; }
    public bool join_before_host { get; set; }
    public bool mute_upon_entry { get; set; }
    public bool watermark { get; set; }
    public bool use_pmi { get; set; }
    public ZoomMeetingApproval approval_type { get; set; }
    public ZoomMeetingRegistration registration_type { get; set; }
    public string audio { get; set; }
    public string auto_recording { get; set; }
    public bool enforce_login { get; set; }
    public string enforce_login_domains { get; set; }
    public string alternative_hosts { get; set; }
}


/// <summary>
/// Zoom Api Meeting Details for started and retrieved meetings
/// </summary>
public class ZoomMeetingDetails {
    [Display(Name = "Topic", Description = "The topic for the meeting")]
    public string topic { get; set; }

    [Display(Name = "Type", Description = "The type of meeting")]
    public ZoomMeetingType type { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(Name = "Agenda", Description = "Meeting description")]
    public string agenda { get; set; }

    [Display(Name = "Start time", Description = "Meeting start time. Only used for scheduled and recurring meetings.")]
    public DateTime? start_time { get; set; }

    [Display(Name = "Duration", Description = "Duration of the meeting in minutes. Used for scheduled meetings only.")]
    public int? duration { get; set; }

    public string uuid { get; set; }
    public long id { get; set; }
    public string host_id { get; set; }
    public string timezone { get; set; }
    public DateTime created_at { get; set; }
    public string status { get; set; }
    public string start_url { get; set; }
    public string join_url { get; set; }
    public string registration_url { get; set; }
    public string password { get; set; }
    public string h323_password { get; set; }
    public long? pmi { get; set; }
    public ZoomTrackingField[] tracking_fields { get; set; }
    public ZoomMeetingOccurrence[] occurrences { get; set; }
    public ZoomMeetingSettings settings { get; set; }
}

public class ZoomTrackingField {
    public string field { get; set; }
    public string value { get; set; }
}

/// <summary>
/// Zoom Api Meeting Detail Settings
/// </summary>
public class ZoomMeetingSettings {
    public bool host_video { get; set; }
    public bool participant_video { get; set; }
    public bool cn_meeting { get; set; }
    public bool in_meeting { get; set; }
    public bool join_before_host { get; set; }
    public bool mute_upon_entry { get; set; }
    public bool watermark { get; set; }
    public bool use_pmi { get; set; }
    public ZoomMeetingApproval approval_type { get; set; }
    public ZoomMeetingRegistration registration_type { get; set; }
    public string audio { get; set; }
    public string auto_recording { get; set; }
    public bool enforce_login { get; set; }
    public string enforce_login_domains { get; set; }
    public string alternative_hosts { get; set; }
    public bool close_registration { get; set; }
    public bool waiting_room { get; set; }
    public string[] global_dial_in_countries { get; set; }
    public string contact_name { get; set; }
    public string contact_email { get; set; }
    public bool registrants_confirmation_email { get; set; }
}

public enum ZoomMeetingApproval {
    Automatically = 0,
    Manual = 1,
    None = 2
}

public enum ZoomMeetingRegistration {
    Once = 1,
    Each = 2,
    Multiplle = 3
}


/// <summary>
/// Zoom Api Meeting Details Occurence
/// </summary>
public class ZoomMeetingOccurrence {
    public long occurrence_id { get; set; }
    public DateTime start_time { get; set; }
    public long duration { get; set; }
    public string status { get; set; }
}

public class ZoomMeetingRecordingsResult {
    public DateTime from { get; set; }
    public DateTime to { get; set; }
    public int page_count { get; set; }
    public int page_size { get; set; }
    public int total_records { get; set; }
    public string next_page_token { get; set; }
    public ZoomMeetingRecordings[] meetings { get; set; }
}


/// <summary>
/// Zoom Api Meeting Recordings
/// </summary>
public class ZoomMeetingRecordings : ZoomMeeting {
    public string account_id { get; set; }
    public long total_size { get; set; }
    public int recording_count { get; set; }
    public string share_url { get; set; }
    public ZoomRecordingFile[] recording_files { get; set; }
}

public class ZoomRecordingFile {
    public string id { get; set; }
    public string meeting_id { get; set; }
    public DateTime recording_start { get; set; }
    public DateTime? recording_end { get; set; }
    public string file_type { get; set; }
    public long file_size { get; set; }
    public string play_url { get; set; }
    public string download_url { get; set; }
    public string status { get; set; }
    public string deleted_time { get; set; }
    public string recording_type { get; set; }
}


public class ZoomMeetingRegistrants {
    public int page_count { get; set; }
    public int page_size { get; set; }
    public int total_records { get; set; }
    public string next_page_token { get; set; }
    public ZoomRegistrant[] registrants { get; set; }
}

public class ZoomRegistrant {
    public string id { get; set; }
    public string email { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string address { get; set; }
    public string city { get; set; }
    public string country { get; set; }
    public string zip { get; set; }
    public string state { get; set; }
    public string phone { get; set; }
    public string industry { get; set; }
    public string org { get; set; }
    public string job_title { get; set; }
    public string purchasing_time_frame { get; set; }
    public string role_in_purchase_process { get; set; }
    public string no_of_employees { get; set; }
    public string comments { get; set; }
    public ZoomRegistrantCustomQuestions[] custom_questions { get; set; }
    public string status { get; set; }
    public DateTime create_time { get; set; }
    public string join_url { get; set; }
}

public class ZoomRegistrantCustomQuestions {
    public string title { get; set; }
    public string value { get; set; }
}

public class ZoomRegistrationResponse {
    public string registrant_id { get; set; }
    public long id { get; set; }
    public string topic { get; set; }
    public DateTime start_time { get; set; }
    public string join_url { get; set; }
}

public class ZoomAssistants {
    public ZoomAssistant[] assistants { get; set; }
}

public class ZoomAssistant {
    public string id { get; set; }
    public string email { get; set; }
}

public class ZoomErrorResponse {
    public ZoomErrorCode code { get; set; }
    public string message { get; set; }
    public ZoomError[] errors { get; set; }
}

public class ZoomError {
    public string field { get; set; }
    public string message { get; set; }
}

public enum ZoomErrorCode {
    InvalidRequest = 300,
    BadRequest = 400,
    Unautorized = 401,
    Forbidden = 403,
    NotFound = 404,
    InternalServerError = 500,
    UserNotExist = 1001,
    UserNotBelongToAccount = 1010,
    CannotAccessWebinar = 3000,
    MeetingNotFoundOrExpired = 3001
}

public class ZoomEmailResponse {
    public bool existed_email { get; set; }
}

#pragma warning restore CS1591
