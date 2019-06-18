using System.Collections.Generic;
using System.Web.Mvc;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Areas.Apps.Models;
using Weavy.Web.Controllers;

namespace Weavy.Areas.Apps.Controllers {
    /// <summary>
    /// Controller for the <see cref="TasksApp"/>.
    /// </summary>    
    [RoutePrefix("{id:int}/F1C835B0-E2A7-4CF3-8900-FE95B6504145")]
    public class TasksController : AppController<TasksApp> {

        /// <summary>
        /// Get action for the app
        /// </summary>
        /// <param name="app">The app to display.</param>
        /// <param name="query">An object with query parameters for search, paging etc.</param>        
        public override ActionResult Get(TasksApp app, Query query) {
            app.Tasks = ContentService.Search<TaskItem>(new ContentQuery<TaskItem>(query) { AppId = app.Id, Depth = 1, OrderBy = "CreatedAt ASC", Count = true });
            return View(app);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The id of the app</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("tasks")]
        public JsonResult InsertTask(int id, TaskIn model) {
            var app = AppService.Get<TasksApp>(id);

            var task = new TaskItem {
                Name = model.Name,
                Order = model.Order
            };
            var inserted = ContentService.Insert<TaskItem>(task, app);

            return Json(inserted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The id of the app</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("tasks/sort")]
        public JsonResult UpdateSortOrder(int id, IEnumerable<TaskIn> model) {
            var app = AppService.Get<TasksApp>(id);

            foreach (var input in model) {
                var task = ContentService.Get<TaskItem>(input.Id);
                if (task != null) {
                    task.Order = input.Order;
                    ContentService.Update<TaskItem>(task);
                }

            }

            return Json(model);
        }

    }
}
