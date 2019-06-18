using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading;
using Weavy.Core;
using Weavy.Core.Helpers;
using Weavy.Core.Models;
using Weavy.Core.Services;

namespace Weavy.Sandbox {

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Guid("FFEEC5F6-785D-4700-A97F-3E3F6C372417")]
    [Plugin(Icon = "account-plus", Name = "Add user to Sandbox", Description = "Creates and sends welcome emails to additional sandbox users.")]
    public class SandboxAddUserFunction : Function, ICommand, ITool {

        /// <summary>
        ///  
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "That's not a valid email address")]
        [Display(Name = "Email", Description = "Email of the user to add.")]
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Invalid username. Valid characters are [a-zA-Z0-9_].")]
        [StringLength(32)]
        [Display(Name = "Username", Description = "Username of the user to add.")]
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "Name", Description = "Name of the user to add.")]
        public string UserFullName { get; set; }

        public SandboxAddUserFunction() {
        }

        /// <summary>
        /// Sends an invitiation email to all active users.
        /// </summary>
        /// <returns></returns>
        public override bool Run(CancellationToken token, params string[] args) {

            var password = "weavydemo";

            if (ValidationHelper.IsValid(this)) {
                var exists = UserService.Get(UserName);

                if (exists != null) {
                    Output.WriteLine($"ERROR: Username '{UserName}' is already in use. Enter a different username.");
                    return false;
                }

                exists = UserService.GetByEmail(UserName);

                if (exists != null) {
                    Output.WriteLine($"ERROR: Email '{Email}' is already in use. Enter a different email.");
                    return false;
                }

                // create user
                var user = new User() { Email = Email, Username = UserName };
                user.Profile.Name = UserFullName;

                try {
                    user = UserService.Insert(user, password, false);

                    // send login details                    
                    // https://domain.weavycloud.com -> "domain"
                    var domain = WeavyContext.Current.ApplicationUrl?.RightAfter("https://").LeftBefore(".");

                    var message = new MailMessage();

                    message.To.Add(new MailAddress(user.Email));
                    message.Subject = "Welcome to Weavy!";
                    message.Body = string.Format(SandboxWelcomeFunction.EmailTemplate, WeavyContext.Current.ApplicationUrl, user.GetTitle(), user.Username, password, domain);
                    message.IsBodyHtml = true;
                    message.From = !WeavyContext.Current.User.Email.IsNullOrEmpty() ? new MailAddress(WeavyContext.Current.User.Email, WeavyContext.Current.User.GetTitle()) : new MailAddress(ConfigurationService.SmtpFrom, WeavyContext.Current.System.Name);

                    MailService.Send(message, null, true);
                    Output.WriteLine($"Welcome email sent to '{user.Email}'");
                    return true;
                } catch (Exception ex) {
                    Output.WriteLine(ex.Message);
                    return false;
                }
            } else {
                return false;
            }
        }
    }
}
