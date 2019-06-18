using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Weavy.Core;
using Weavy.Core.Events;
using Weavy.Core.Helpers;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Web;

namespace Weavy.Sandbox {

    /// <summary>
    /// A hook that populates the installation with data after initial setup.
    /// </summary>
    [Serializable]
    [Guid("A101738D-A9E8-40F7-8A78-09DD8AE2D7EB")]
    [Plugin(Icon = "webhook", Name = "Sandbox Hook", Description = "Populates the installation with data after initial setup.")]
    public class SandboxHook : Hook,
        IAsyncHook<AfterSystemSetup> {

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        // email template
        private string _emailTemplate = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
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
                                                                    <h3>Sandbox {1} is up and running!</h3>
                                                                    <p>Login and make sure everything looks good, add some content and context if needed.</p>
                                                                    <p>When you’re satisfied with everything, click <code>Run</code> from <a href=""/manage/tools/300a6913-9772-495a-9ac4-96f8d9f5fc56"">here</a> to send login information to all the users in the sandbox.</p>
                                                                    <p>You can add additional users by entering email, username and name from <a href=""/manage/tools/ffeec5f6-785d-4700-a97f-3e3f6c372417"">here</a>.</p>
                                                                    <h3>Sandbox URL and your login information</h3>
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
                                                    <td align=""center"" valign=""top"" width=""600"" class=""flexibleContainerCell bottomShim"">
                                                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" class=""nestedContainer"">
                                                            <tr>
                                                                <td align=""center"" valign=""top"" class=""nestedContainerCell"">
                                                                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td valign=""top"" class=""textContent"">
                                                                                url: <a href=""/"">{0}</a><br />
                                                                                username: <strong>{2}</strong><br />
                                                                                password: <strong>{3}</strong>
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
                                                    <td align=""center"" valign=""top"" width=""600"" class=""flexibleContainerCell bottomShim"">
                                                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""260"" class=""emailButton"">
                                                            <tr>
                                                                <td align=""center"" valign=""middle"" class=""buttonContent"">
                                                                    <a href=""/sign-in"" target=""_blank"">Sign in</a>
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
                                                    <td align=""center"" valign=""top"" width=""600"" class=""flexibleContainerCell bottomShim"">
                                                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""260"" class=""emailButton"">
                                                            <tr>
                                                                <td align=""center"" valign=""middle"" class=""buttonContent"">
                                                                    <a href=""/manage/tools/300a6913-9772-495a-9ac4-96f8d9f5fc56"" target=""_blank"">Send Welcome Emails</a>
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
                                                    <td align=""center"" valign=""top"" width=""600"" class=""flexibleContainerCell bottomShim"">
                                                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""260"" class=""emailButton"">
                                                            <tr>
                                                                <td align=""center"" valign=""middle"" class=""buttonContent"">
                                                                    <a href=""/manage/tools/ffeec5f6-785d-4700-a97f-3e3f6c372417"" target=""_blank"">Invite additional users</a>
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
        /// Populate installation after system setup completes.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Task HandleAsync(AfterSystemSetup e) {
            var settingsString = ConfigurationService.AppSetting("weavy-custom.sandbox-settings");

            if (!settingsString.IsNullOrEmpty()) {
                try {
                    var settings = JsonConvert.DeserializeObject<SandboxSettings>(settingsString);
                    _log.Info($"Found Sandbox settings: " + settingsString);

                    if (settings.IsValid()) {
                        Populate(settings);
                    } else {
                        _log.Error($"Sandbox settings are not valid");
                    }
                } catch (Exception ex) {
                    _log.Error(ex, "Failed to deserialize sandbox settings");
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Populate Weavy with data from sandbox settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void Populate(SandboxSettings settings) {

            _log.Info("Populating installation...");

            // set system user as current user
            var system = UserService.Get(UserService.SystemId, filter: false);
            WeavyContext.Current.User = system;

            // delete admin
            var admin = UserService.Get("admin");

            if (admin != null) {
                UserService.Trash(admin.Id);
                _log.Info($"Trashed stale admin account");
            }

            User creator = null;

            if (settings.Creator != null) {
                _log.Info($"Adding creator '{settings.Creator.Email}'");
                var password = UserService.GeneratePassword();
                creator = AddUser(settings.Creator, password);

                var mm = new MailMessage {
                    From = new MailAddress(ConfigurationService.SmtpFrom, WeavyContext.Current.System.Name)
                };
                mm.To.Add(new MailAddress(creator.Email));
                mm.Subject = $"Sandbox {WeavyRequestContext.Current.Theme.Name} was deployed!";
                mm.Body = string.Format(_emailTemplate, WeavyContext.Current.ApplicationUrl, WeavyRequestContext.Current.Theme.Name, creator.Username, password);
                mm.IsBodyHtml = true;

                MailService.Send(mm, null, true);
            } else {
                creator = UserService.Search(new UserQuery()).FirstOrDefault(x => x.IsAdmin) ?? UserService.Get(UserService.SystemId);
            }

            // set current user = creator
            WeavyContext.Current.User = creator ?? throw new Exception("No creator to use. Exiting.");

            // process invites
            var invitees = new List<User>();

            if (!settings.Invites.IsNullOrEmpty()) {

                var password = "weavydemo";

                foreach (var invitee in settings.Invites) {

                    try {
                        _log.Info($"Adding invitee '{invitee.Email}'");

                        var user = AddUser(invitee, password);

                        // turn of email notifications 
                        user.Profile["_notify_email"] = false;
                        UserService.Update(user);
                        invitees.Add(user);

                    } catch (Exception ex) {
                        _log.Warn($"Failed to process '{invitee}': {ex.Message}");
                    }
                }
            }

            // download template
            if (!settings.TemplateUrl.IsNullOrEmpty()) {
                _log.Debug($"Downloading sandbox template from: {settings.TemplateUrl}");

                var tempDir = new DirectoryInfo(Path.Combine(WeavyContext.Current.ApplicationDirectory, "App_Data" + Path.DirectorySeparatorChar, Guid.NewGuid().ToString("N") + @"\"));
                tempDir.Create();
                tempDir.Refresh();

                var template = new FileInfo(Path.Combine(tempDir.FullName, FileHelper.SafeName(settings.TemplateUrl).ToLower()));

                template = DownloadFile(settings.TemplateUrl, template);

                if (template.Exists) {
                    _log.Info("Unzipping template");

                    // reset current user to creator 
                    // NOTE: current user could have been set to anonymous user, depending on thread execution etc.
                    WeavyContext.Current.User = creator;

                    // unzip and populate content
                    var destination = new DirectoryInfo(Path.Combine(template.DirectoryName, FileHelper.GetFileNameWithoutExtension(template.Name)));

                    ZipFile.ExtractToDirectory(template.FullName, destination.FullName, Encoding.Default);

                    destination.Refresh();

                    var globalTemplate = destination.GetFiles().FirstOrDefault(x => x.Name.Equals("global.json", StringComparison.OrdinalIgnoreCase));

                    if (globalTemplate != null) {
                        _log.Info($"Found global template");

                        try {
                            var globalSettings = JObject.Parse(System.IO.File.ReadAllText(globalTemplate.FullName));

                            var messengerSettings = globalSettings["messenger"];

                            if (invitees.Any() && messengerSettings["roomName"]?.Value<string>() != null) {

                                _log.Info($"Adding conversation");

                                var conversation = new Conversation();

                                // add roomname
                                if (invitees.Count > 1) {
                                    conversation.Name = messengerSettings["roomName"].Value<string>();
                                }

                                // insert and add others to conversation
                                conversation = ConversationService.Insert(conversation, invitees.Select(x => x.Id).ToList());

                                // add messages
                                foreach (var message in messengerSettings["messages"]?.Children().ToList()) {
                                    MessageService.Insert(new Message() { Text = message.Value<string>() }, conversation);
                                    _log.Info($"Adding message");
                                }
                            }
                        } catch (Exception ex) {
                            _log.Error(ex, "Failed to process global template");
                        }
                    }

                    foreach (var dir in destination.GetDirectories()) {
                        _log.Info($"Adding space '{dir.Name}'");

                        Blob avatarBlob = null;
                        var avatar = dir.GetFiles().FirstOrDefault(x => x.Name.StartsWith("avatar.", StringComparison.OrdinalIgnoreCase));

                        var space = SpaceService.Insert(new Space { Name = dir.Name, Privacy = Privacy.Open, Avatar = (avatar != null ? BlobService.Insert(avatar.FullName) : null) });

                        // add invitees to space
                        foreach (var user in invitees) {
                            SpaceService.AddMember(space.Id, user.Id, Access.Admin, false);
                        }

                        var spaceTemplate = dir.GetFiles().SingleOrDefault(x => x.Name.Equals("space.json", StringComparison.OrdinalIgnoreCase));

                        if (spaceTemplate != null) {

                            _log.Info($"Found space template");

                            try {
                                var spaceSettings = JObject.Parse(System.IO.File.ReadAllText(spaceTemplate.FullName));

                                space.Teamname = spaceSettings["teamName"]?.Value<string>();

                                if (spaceSettings["isHq"] != null && spaceSettings["isHq"].Value<bool>()) {
                                    SpaceService.SetHQ(space.Id);

                                    if (!settings.AppUrl.IsNullOrEmpty()) {
                                        var bubble = new Bubble() {
                                            SpaceId = space.Id,
                                            Url = settings.AppUrl.AddTrailingSlash() + "*"
                                        };
                                        BubbleService.Insert(bubble);
                                    }
                                    _log.Info($"Setting HQ: {space.Name}");
                                    avatarBlob = DownloadBlob(settings.HqAvatarUrl);
                                }
                                space.Avatar = avatarBlob ?? space.Avatar;
                                space = SpaceService.Update(space);

                            } catch (Exception ex) {
                                _log.Warn(ex, "Failed to parse space.json");
                            }
                        }

                        // add apps
                        var appsTemplate = dir.GetFiles().SingleOrDefault(x => x.Name.Equals("apps.json", StringComparison.OrdinalIgnoreCase));

                        if (appsTemplate != null) {
                            _log.Debug($"Found apps template");
                            try {
                                var appsJson = JObject.Parse(System.IO.File.ReadAllText(appsTemplate.FullName))["apps"].Children().ToList();

                                foreach (var appJson in appsJson) {
                                    try {
                                        var appGuid = Guid.Parse(appJson.Value<string>("type"));
                                        var appMeta = PluginService.GetApp(appGuid);

                                        if (appMeta != null) {
                                            var app = AppService.New(appGuid);
                                            JsonConvert.PopulateObject(appJson["properties"].ToString(), app);
                                            _log.Debug($"Adding '{app.Name}' to space '{space.Name}'");
                                            app = AppService.Insert(app, space);

                                            // files app
                                            if (appGuid == typeof(FilesApp).GUID) {

                                                // folder exists?
                                                var filesApp = dir.GetDirectories().FirstOrDefault();

                                                if (filesApp != null) {
                                                    _log.Debug($"Adding files app to space '{space.Name}'");
                                                    AddRecursive(app, null, filesApp);
                                                }
                                            } else {
                                                var children = appJson["children"]?.Children();

                                                if (children != null) {
                                                    foreach (var child in children) {
                                                        // posts
                                                        if (appGuid == typeof(PostsApp).GUID) {
                                                            var post = new Post();
                                                            JsonConvert.PopulateObject(child["properties"].ToString(), post);
                                                            PostService.Insert(post, app);
                                                        } else {
                                                            var contentGuid = Guid.Parse(child.Value<string>("type"));
                                                            var content = ContentService.New(contentGuid);
                                                            JsonConvert.PopulateObject(child["properties"].ToString(), content);
                                                            ContentService.Insert(content, app);
                                                        }
                                                    }
                                                }
                                            }
                                        } else {
                                            // did not find app
                                            _log.Warn($"Could not find app with guid {appGuid}");
                                        }
                                    } catch (Exception ex) {
                                        _log.Warn(ex, "Failed to add app");
                                    }
                                }

                            } catch (Exception ex) {
                                _log.Warn(ex, "Failed to parse apps.json");
                            }
                        }
                    }
                }

                // remove temp dir
                if (tempDir.Exists) {
                    tempDir.Delete(true);
                }
                _log.Info("Done populating installation");
            }
        }

        /// <summary>
        /// Adds the user.
        /// </summary>
        /// <param name="sandboxUser"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private User AddUser(SandboxUser sandboxUser, string password) {

            var user = new User() {
                Username = UserService.MakeUniqueUsername(sandboxUser.UserName),
                Email = sandboxUser.Email,
                Avatar = DownloadBlob(sandboxUser.AvatarUrl)
            };

            user.Profile.Name = sandboxUser.Name;

            if (user.Profile is Profile p) {
                p.Title = sandboxUser.Title;
                user.Profile.Name = sandboxUser.Name;
                user.Profile["_groupconversations"] = true;
                user.Profile["_timezone"] = sandboxUser.TimeZone;
            }
            user = UserService.Insert(user, password);

            // add to the administrators group
            return RoleService.AddMember(RoleService.AdministratorsId, user.Id);
        }

        /// <summary>
        /// Recurcively adds folders and files to the files app.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="folder"></param>
        /// <param name="directory"></param>
        private void AddRecursive(App app, Folder folder, DirectoryInfo directory) {

            // add folders recursively 
            foreach (var dir in directory.GetDirectories()) {
                var f = new Folder { Name = dir.Name };
                f = app != null ? ContentService.Insert(f, app) : ContentService.Insert(f, folder);
                AddRecursive(null, f, dir);
            }

            // add files
            foreach (var file in directory.GetFiles().Where(x => !x.Name.StartsWith("avatar.", StringComparison.OrdinalIgnoreCase))) {
                var f = new Core.Models.File() { Blob = BlobService.Insert(file.FullName) };
                f = app != null ? ContentService.Insert(f, app) : ContentService.Insert(f, folder);
            }
        }

        /// <summary>
        /// Downloads a file and saves in locally.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        private FileInfo DownloadFile(string url, FileInfo destination = null) {
            if (url.IsNullOrEmpty()) {
                return null;
            }

            _log.Info("Downloading file: " + url);

            destination = destination ?? new FileInfo(Path.Combine(FileHelper.GetTempDir(true).FullName, FileHelper.SafeName(url).ToLower()));

            try {
                using (WebClient client = new WebClient()) {
                    client.UseDefaultCredentials = true;
                    client.Headers.Add("user-agent", "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)");
                    client.DownloadFile(url, destination.FullName);

                    if (destination.Extension.IsNullOrEmpty()) {
                        var ext = FileHelper.GetImageExtension(client.ResponseHeaders["Content-Type"]);
                        destination.MoveTo(destination.FullName + ext);
                    }
                    destination.Refresh();
                    return destination;
                }
            } catch (Exception ex) {
                _log.Error(ex, $"Failed to download file from '{url}'");
                return null;
            }
        }

        /// <summary>
        /// Downloads file and saves as blob in sql.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Blob DownloadBlob(string url) {
            var file = DownloadFile(url);

            if (file != null) {

                _log.Info($"Saving blob: {file.FullName}");

                try {
                    // insert file from temp file
                    var blob = BlobService.Insert(new Blob { Name = file.Name }, file.OpenRead());

                    // remove local file after it's inserted into db
                    file.Delete();

                    return blob;
                } catch (Exception ex) {
                    _log.Error(ex, $"Failed to save blob from {url}");
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Sandbox settings object.
    /// </summary>
    public class SandboxSettings {

        /// <summary>
        /// 
        /// </summary>
        public SandboxSettings() {
            Invites = new List<SandboxUser>();
        }

        /// <summary>
        /// Gets or sets the url for the company.
        /// </summary>
        [Url(ErrorMessage = "That does not look like a valid url.")]
        public string AppUrl { get; set; }

        /// <summary>
        /// Gets or sets the email of the creator of resources.
        /// </summary>
        public SandboxUser Creator { get; set; }

        /// <summary>
        /// Gets or sets the invited users.
        /// </summary>
        public List<SandboxUser> Invites { get; set; }

        /// <summary>
        /// Gets or sets the url to the template to use when populating the installation.
        /// </summary>
        public string TemplateUrl { get; set; }

        /// <summary>
        /// Gets or sets the url for the avatar to use for the hq space.
        /// </summary>
        public string HqAvatarUrl { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SandboxUser {

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Invalid username. Valid characters are [a-zA-Z0-9_].")]
        [StringLength(32)]
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "That's not a valid email address")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the work title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the timezone.
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the avatar url.
        /// </summary>
        public string AvatarUrl { get; set; }

    }
}
