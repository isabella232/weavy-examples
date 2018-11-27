using System;
using System.Runtime.InteropServices;
using Weavy.Core.Models;

namespace Wvy.Models {
    /// <summary>
    /// A task management app
    /// </summary>
    [Serializable]
    [Guid("F1C835B0-E2A7-4CF3-8900-FE95B6504145")]
    [App(Icon = "checkbox-marked-outline", Name = "Tasks", Description = "A task management app.", AllowMultiple = true, Content = new Type[] { typeof(TaskItem) })]
    public class TasksApp : App {

        /// <summary>
        /// All the tasks in the task list
        /// </summary>
        public SearchResult<Content, ContentQuery> Tasks { get; set; }
    }
}