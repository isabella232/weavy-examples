using System.Web.Mvc;
using Weavy.Core.Models;
using Weavy.Areas.Apps.Models;
using Weavy.Web.Controllers;

namespace Weavy.Areas.Apps.Controllers {
    /// <summary>
    /// Controller for the <see cref="IFrameApp"/>.
    /// </summary>
    [RoutePrefix("{id:int}/336F0C5F-019E-459B-B79E-3DE8F56E8D56")]
    public class IFrameAppController : AppController<IFrameApp> {

        /// <summary>
        /// Display the specified url in an iframe.
        /// </summary>
        /// <param name="app">The app to display.</param>
        /// <param name="query">An object with query parameters for search, paging etc.</param>
        public override ActionResult Get(IFrameApp app, Query query) {
            // add you custom logic here...
            return View(app);
        }

        /// <summary>
        /// A test action to show that you can add custom actions to App controllers.
        /// </summary>
        /// <param name="id">Id of app</param>
        [HttpGet]
        [Route("test")]
        public ActionResult Test(int id) {
            return Content("Test result");
        }
    }
}
