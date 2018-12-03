using System;
using System.Web.Mvc;
using Weavy.Core.Events;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Web.Controllers;
using Wvy.Models;

namespace Wvy.Controllers {
    /// <summary>
    /// Controller for the <see cref="AsanaApp"/>.
    /// </summary>
    [RoutePrefix("apps/{id:int}/B309F192-9F22-4E39-AAFC-DD59589227C7")]
    public class AsanaAppController : AppController<AsanaApp> {

        /// <summary>
        /// Get action for the app
        /// </summary>
        /// <param name="app">The app to display.</param>
        /// <param name="query">An object with query parameters for search, paging etc.</param>        
        public override ActionResult Get(AsanaApp app, Query query) {                        
            return View(app);
        }


        /// <summary>
        /// Handle OAuth2 response url
        /// </summary>
        /// <returns></returns>
        [Route("~/apps/B309F192-9F22-4E39-AAFC-DD59589227C7/auth")]
        public ActionResult Auth() {
            return View();
        }
    }
    
}