using System;
using System.Web.Mvc;
using Weavy.Core.Events;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Web.Controllers;
using Wvy.Models;

namespace Wvy.Controllers {
    /// <summary>
    /// Controller for the <see cref="TasksApp"/>.
    /// </summary>
    [RoutePrefix("apps/{id:int}/F1C835B0-E2A7-4CF3-8900-FE95B6504145")]
    public class TasksAppController : AppController<TasksApp> {

        /// <summary>
        /// Get action for the app
        /// </summary>
        /// <param name="app">The app to display.</param>
        /// <param name="query">An object with query parameters for search, paging etc.</param>        
        public override ActionResult Get(TasksApp app, Query query) {

            app.Tasks = ContentService.Search(new ContentQuery(query) { AppId = app.Id, Depth = 1, OrderBy = "CreatedAt ASC", Count = true, Guids = { typeof(TaskItem).GUID } });

            return View(app);
        }

        /// <summary>
        /// Get task details
        /// </summary>
        /// <param name="id">The id of the app</param>
        /// <param name="tid">The id of the task</param>
        /// <returns></returns>
        [HttpGet]
        [Route("tasks/{tid:int}/comments")]
        public ActionResult Comments(int id, int tid) {

            var task = ContentService.Get<TaskItem>(tid);

            return PartialView("_TaskComments", task);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The id of the app</param>
        /// <param name="tid">The id of the task</param>
        /// <returns></returns>
        [HttpPut]
        [Route("tasks/{tid:int}/toggle")]
        public JsonResult ToggleCompleted(int id, int tid) {

            var task = ContentService.Get<TaskItem>(tid);

            task.Completed = !task.Completed;
            var updated = ContentService.Update<TaskItem>(task);

            // call hooks            
            //Hooker.CallAsync(new RealTimeEvent() { EventName = "task_toggle_completed", Data = new TaskOut() { TaskId = tid, Completed = task.Completed } });

            return Json(updated);
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
                Name = model.Name
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
        [HttpPut]
        [Route("tasks")]
        public JsonResult UpdateTask(int id, TaskIn model) {

            var notify = false;
            var task = ContentService.Get<TaskItem>(model.Id);

            if (model.AssignedTo.HasValue && model.AssignedTo != task.AssignedTo && model.AssignedTo != User.Id) {
                notify = true;
            }

            task.Name = model.Name;
            task.Completed = model.Completed;
            task.DueDate = model.DueDate;
            task.AssignedTo = model.AssignedTo;
            task.Priority = model.Priority;

            var updated = ContentService.Update<TaskItem>(task);

            // notify
            if (notify) {
                NotificationService.Insert(
                    new Notification(model.AssignedTo.Value, $@"{User.GetTitle()} assigned you to the task <strong>{task.Name}</strong>") { Link = task }
                );
            }

            return Json(updated);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The id of the app</param>
        /// <param name="tid">The id of the task</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("tasks/{tid:int}")]
        public JsonResult DeleteTask(int id, int tid) {

            var task = ContentService.Get<TaskItem>(tid);

            if (task != null) {
                ContentService.Delete(task.Id);
            }

            return Json("Deleted");
        }
    }
    
}