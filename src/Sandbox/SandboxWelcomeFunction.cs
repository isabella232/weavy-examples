using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading;
using Dapper;
using Weavy.Core;
using Weavy.Core.Helpers;
using Weavy.Core.Models;
using Weavy.Core.Repos;
using Weavy.Core.Services;

namespace Weavy.Sandbox {

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Guid("300A6913-9772-495A-9AC4-96F8D9F5FC56")]
    [Plugin(Icon = "account-multiple", Name = "Welcome to Sandbox", Description = "Sends welcome emails with username/password to the users in the installation. Run when the sandbox is ready.")]
    public class SandboxWelcomeFunction : Function, ICommand, ITool {

        // email template
        public const string EmailTemplate = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <meta name=""viewport"" content=""width=device-width"" />
    <title>Welcome to Weavy</title>
    <style type=""text/css"">
        
        body, #bodyTable, #bodyCell{{height:100%!important;margin:0;padding:0;width:100%!important;}}
        table{{border-collapse:collapse;}}
        img,a img{{border:0;outline:none;text-decoration:none;}}
        h1,h2,h3,h4,h5,h6{{margin:0;padding:0;}}
        p{{margin:1em 0;}}
        
        .ReadMsgBody{{width:100%;}} .ExternalClass{{width:100%;}} 
        .ExternalClass,.ExternalClass p,.ExternalClass span,.ExternalClass font,.ExternalClass td,.ExternalClass div{{line-height:100%;}} 
        table,td{{mso-table-lspace:0pt;mso-table-rspace:0pt;}} 
        #outlook a{{padding:0;}} 
        img{{-ms-interpolation-mode:bicubic;}} 
        body,table,td,p,a,li,blockquote{{-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%;}} 
        
        .flexibleContainerCell{{padding-top:20px;padding-right:20px;padding-left:20px;}}
        .flexibleImage{{height:auto;}}
        .bottomShim{{padding-bottom:20px;}}
        .imageContent,.imageContentLast{{padding-bottom:20px;}}
        .nestedContainerCell{{padding-top:20px; padding-right:20px; padding-left:20px;}}
        
        body,#bodyTable{{background-color:#FAFAFA;}}
        #bodyCell{{padding-top:40px;padding-bottom:40px;}}
        #emailBody{{background-color:#FFFFFF;border:1px solid #E0E0E0;border-collapse:separate;border-radius:4px;}}
        #headerImage{{height:auto;max-width:600px!important;}}
        h1,h2,h3,h4,h5,h6{{color:#383838;font-family:Arial,sans-serif;font-size:20px;line-height:125%;text-align:left;}}
        small {{color:#8C8C8C;font-family:Arial,sans-serif;font-size:14px;line-height:150%;}}
        .headerContent{{text-align:center;vertical-align:middle;}}
        .textContent,.textContentLast{{color:#383838;font-family:Arial,sans-serif;font-size:16px;line-height:150%;text-align:left;padding-bottom:20px;}}
        .textContent a,.textContentLast a{{color:#1C8FC4;text-decoration:underline;}}
        .nestedContainer{{background-color:#F5F5F5;border:1px solid #E0E0E0;border-collapse:separate;border-radius:4px;}}
        .emailButton{{background-color:#36ACE2;border-collapse:separate;border-radius:4px;}}
        .buttonContent{{color:#FFFFFF;font-family:Arial,sans-serif;font-size:18px;font-weight:bold;line-height:100%;text-align:center;padding:15px;}}
        .buttonContent a{{color:#FFFFFF;display:block;text-decoration:none;}}
        #emailFooter{{background-color:#FAFAFA;border:0;}}
        .footerContent{{color:#8C8C8C;font-family:Arial,sans-serif;font-size:14px;line-height:150%;text-align:center;}}
        .footerContent a{{color:#1C8FC4;text-decoration:underline;}}
        
        @media only screen and (max-width: 480px){{            
            body{{width:100%!important;min-width:100%!important;}} 
            
            table[id=""emailBody""],table[id=""emailFooter""],table[class=""flexibleContainer""]{{width:100%!important;}} 
            img[class=""flexibleImage""]{{height:auto!important;width:100%!important;}} 
            
            body,#bodyTable,#emailFooter{{background-color:#FFFFFF;}}
            #emailBody{{border:0;}}
            td[id=""bodyCell""]{{padding-top:10px!important;padding-right:10px!important;padding-left:10px!important;}}
            table[class=""emailButton""]{{width:100% !important;}} 
            td[class=""buttonContent""]{{padding:0!important;}}
            td[class=""buttonContent""] a{{padding:15px!important;}}
            td[class=""textContentLast""],td[class=""imageContentLast""]{{padding-top:20px!important;}}
        }}
    </style>
</head>
<body>
    <center>
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" height=""100%"" width=""100%"" id=""bodyTable"">
            <tr>
                <td align=""center"" valign=""top"" id=""bodyCell"">                                                           
                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" id=""emailBody"">                        
                        <tr>
                            <td align=""center"" valign=""top"">                                                               
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                    <tr>
                                        <td align=""center"" valign=""top"">                                                                                        
                                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" class=""flexibleContainer"">
                                                <tr>
                                                    <td align=""center"" valign=""top"" width=""600"" class=""flexibleContainerCell"">                                                                                                               
                                                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                            <tr>
                                                                <td valign=""top"" class=""headerContent"">
                                                                    <a href=""/"" target=""_blank"">
                                                                        <img src=""{0}img/icon.svg"" width=""48"" height=""48"" alt="""" id=""headerImage"" style=""max-width:600px;"" />
                                                                    </a>
                                                                </td>
                                                            </tr>
                                                        </table>                                                        
                                                    </td>
                                                </tr>
                                            </table>                                            
                                        </td>
                                    </tr>
                                </table>                                
                            </td>
                        </tr>
                        <tr>
                            <td align=""center"" valign=""top"">        
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                    <tr>
                                        <td align=""center"" valign=""top"">                    
                                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" class=""flexibleContainer"">
                                                <tr>
                                                    <td align=""center"" valign=""top"" width=""600"" class=""flexibleContainerCell"">                                
                                                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                            <tr>
                                                                <td valign=""top"" class=""textContent"">
                                                                    <h3>{1}</h3>
                                                                    <p>We've created a personal sandbox of &lt;weavy/&gt; for you.</p>
                                                                    <p>You’ll be adding &lt;weavy/&gt; through what we call the Drop-in UI. It's branded with your colors, logos, etc and embedded into your UI as a floating panel.</p>
                                                                    <p>There are two ways to embed &lt;weavy/&gt; into your app:</p>
                                                                    <p><strong>1) Add it to your development branch / testing environment with the script below</strong></p>
                                                                    <code>
                                                                        &lt;script src=""{0}javascript/weavy.min.js""&gt;&lt;/script&gt;<br />
                                                                        &lt;script&gt;var weavy = new Weavy();&lt;/script&gt;
                                                                    </code>
                                                                    <p><strong>2) Or, add it through our <a href=""https://chrome.google.com/webstore/detail/weavy/lncoljhmhbonffenaodpanfnmbnmhmhj"">Chrome Extension</a>.</strong></p>
                                                                    <p>Follow the instructions in the setup wizard, and when asked for URL use {0}</p>
                                                                    <p>To log in to &lt;weavy/&gt; use your username <strong>{2}</strong> with the password <strong>{3}</strong></p>
                                                                    <p>The separate login is needed when in sandbox mode, when you have it properly embedded into your app you'll have SSO fully integrated with your app.</p>
                                                                    <h4>Mobile</h4>
                                                                    <p>As a part of our framework, you’ll also get mobile apps for you to brand and publish under your own name.</p>
                                                                    <p>If you want to play around with them just search for Weavy in App Store and/or Google Play and install. When asked for a domain use <strong>{4}</strong> and use the same login details as above.</p>
                                                                    <br>
                                                                    <p>
                                                                        Have fun,<br>
                                                                        The &lt;weavy/&gt; team!
                                                                    </p>
                                                                </td>
                                                            </tr>
                                                        </table>                                
                                                    </td>
                                                </tr>
                                            </table>                    
                                        </td>
                                    </tr>
                                </table>        
                            </td>
                        </tr>
                        <tr>
                            <td align=""center"" valign=""top"" class=""textContent"">
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </center>
</body>
</html>
";

        /// <summary>
        ///  
        /// </summary>
        [Display(Name = "Send Welcome Emails", Description = "Sends an email with username/password to all active users.")]
        public bool SendWelcomeMail { get; set; }

        /// <summary>
        ///  
        /// </summary>
        [Display(Name = "Email Notifications", Description = "Check to enable email notifications for all active users.")]
        public bool EnableEmailNotifications { get; set; }

        public SandboxWelcomeFunction() {
            SendWelcomeMail = true;
            EnableEmailNotifications = true;
        }

        /// <summary>
        /// Sends an invitiation email to all active users.
        /// </summary>
        /// <returns></returns>
        public override bool Run(CancellationToken token, params string[] args) {

            var emailNotificationsKey = "_notify_email";

            try {
                // get all active users with email except self
                var users = UserService.Search(new UserQuery() { BuiltIn = false, Suspended = false }).Where(x => x.Id != WeavyContext.Current.User.Id && !x.Email.IsNullOrEmpty());
                var password = "weavydemo";

                // mark ALL existing notifications as sent (to prevent multiple emails notifications)
                using (var cnn = SqlHelper.GetConnection()) {
                    cnn.Execute(@"UPDATE dbo.Notifications SET NotifiedAt = GETUTCDATE() WHERE NotifiedAt IS NULL");
                }

                foreach (var user in users) {
                    if (SendWelcomeMail) {
                        
                        // send login details                    
                        // https://domain.weavycloud.com -> "domain"
                        var domain = WeavyContext.Current.ApplicationUrl?.RightAfter("https://").LeftBefore(".");

                        var message = new MailMessage();                                                   
                        message.To.Add(new MailAddress(user.Email));
                        message.Subject = "Welcome to Weavy!";
                        message.Body = string.Format(EmailTemplate, WeavyContext.Current.ApplicationUrl, user.GetTitle(), user.Username, password, domain);
                        message.IsBodyHtml = true;
                        message.From = !WeavyContext.Current.User.Email.IsNullOrEmpty() ? new MailAddress(WeavyContext.Current.User.Email, WeavyContext.Current.User.GetTitle()) :  new MailAddress(ConfigurationService.SmtpFrom, WeavyContext.Current.System.Name);

                        MailService.Send(message, null, true);
                        Output.WriteLine($"Welcome email sent to '{user.Email}'");
                    }

                    if (EnableEmailNotifications) {

                        // turn on email notifications 
                        var notify = user.Profile.Value<bool?>(emailNotificationsKey);

                        if (notify != null && !notify.Value) {
                            user.Profile[emailNotificationsKey] = true;
                            UserService.Update(user);
                        }
                        Output.WriteLine($"Email notifications enabled for '{user.Email}'");
                    }
                }

                if (SendWelcomeMail) {
                    Output.WriteLine();
                    Output.WriteLine($"All emails sent. Do NOT send emails through this tool again!");
                }
                return true;
            } catch (Exception ex) {
                Output.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
