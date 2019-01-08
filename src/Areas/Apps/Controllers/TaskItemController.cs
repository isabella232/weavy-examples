using System.Collections.Generic;
using System.Web.Mvc;
using Weavy.Core.Helpers;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Web.Controllers;
using Wvy.Areas.Apps.Models;

namespace Wvy.Areas.Apps.Controllers
{    
    public class TaskItemController : ContentController<TaskItem>
    {
        // GET: Apps/TaskItem
        public override ActionResult Get(TaskItem content, Query query)
        {
            return Redirect(content.App().Url() + $"#task-{content.Id}");
            
        }

        /// <summary>
        /// Get task details
        /// </summary>
        /// <param name="id">The id of the app</param>
        /// <param name="tid">The id of the task</param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/content/{id:int}/F16EFF39-3BD7-4FB6-8DBF-F8FE88BBF3EB/comments")]
        public ActionResult Comments(int id)
        {

            var task = ContentService.Get<TaskItem>(id);

            return PartialView("_TaskComments", task);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The id of the app</param>
        /// <param name="tid">The id of the task</param>
        /// <returns></returns>
        [HttpPut]
        [Route("~/content/{id:int}/F16EFF39-3BD7-4FB6-8DBF-F8FE88BBF3EB/toggle")]
        public JsonResult ToggleCompleted(int id)
        {

            var task = ContentService.Get<TaskItem>(id);

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
        [HttpPut]
        [Route("~/content/{id:int}/F16EFF39-3BD7-4FB6-8DBF-F8FE88BBF3EB/tasks")]
        public JsonResult UpdateTask(int id, TaskIn model)
        {

            var notify = false;
            var task = ContentService.Get<TaskItem>(model.Id);

            if (model.AssignedTo.HasValue && model.AssignedTo != task.AssignedTo && model.AssignedTo != User.Id)
            {
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
            if (notify)
            {
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
        [Route("~/content/{id:int}/F16EFF39-3BD7-4FB6-8DBF-F8FE88BBF3EB/tasks")]
        public JsonResult DeleteTask(int id)
        {

            var task = ContentService.Get<TaskItem>(id);

            if (task != null)
            {
                ContentService.Delete(task.Id);
            }

            return Json("Deleted");
        }
    }
}