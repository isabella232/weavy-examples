using System.Threading.Tasks;
using System.Web.Mvc;
using Weavy.Core.Helpers;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Areas.Apps.Models;
using Weavy.Web.Controllers;

namespace Weavy.Areas.Apps.Controllers {
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("{id:int}/F16EFF39-3BD7-4FB6-8DBF-F8FE88BBF3EB")]
    public class TaskItemController : ContentController<TaskItem> {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public override ActionResult Get(TaskItem content, Query query) {
            return Redirect(content.App().Url() + $"#task-{content.Id}");

        }

        /// <summary>
        /// Get task details
        /// </summary>
        /// <param name="id">The id of the app</param>        
        /// <returns></returns>
        [HttpGet]
        [Route("comments")]
        public ActionResult Comments(int id) {

            var task = ContentService.Get<TaskItem>(id);

            return PartialView("_TaskComments", task);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The id of the app</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("toggle")]
        public async Task<JsonResult> ToggleCompleted(int id) {

            var task = ContentService.Get<TaskItem>(id);

            task.Completed = !task.Completed;
            var updated = ContentService.Update<TaskItem>(task);

            // push realtime event           
            await PushService.Push("task_toggle_completed", task);

            return Json(updated);
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The id of the app</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("tasks")]
        public async Task<JsonResult> UpdateTask(int id, TaskIn model) {

            var notify = false;
            var task = ContentService.Get<TaskItem>(model.Id);

            if (model.AssignedTo.HasValue && model.AssignedTo != task.AssignedTo && model.AssignedTo != User.Id) {
                notify = true;
            }

            task.Name = model.Name;
            task.Description = model.Description;
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


            // push realtime event           
            await PushService.Push("task_updated", task);

            return Json(updated);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The id of the app</param>        
        /// <returns></returns>
        [HttpDelete]
        [Route("tasks")]
        public JsonResult DeleteTask(int id) {

            var task = ContentService.Get<TaskItem>(id);

            if (task != null) {
                ContentService.Delete(task.Id);
            }

            return Json("Deleted");
        }
    }
}
