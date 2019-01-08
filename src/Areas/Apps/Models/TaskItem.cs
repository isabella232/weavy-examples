using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Weavy.Core.Models;
using Weavy.Core.Services;

namespace Wvy.Areas.Apps.Models {
    /// <summary>
    /// A custom content type representing a task item in a task app
    /// </summary>
    [Serializable]
    [Guid("F16EFF39-3BD7-4FB6-8DBF-F8FE88BBF3EB")]
    [Content(Icon = "checkbox-marked-outline", Name = "Task item", Description = "A task item.", Parents = new Type[] { typeof(TasksApp) })]
    public class TaskItem : Content, ICommentable, IStarrable {

        [NonSerialized]
        private Lazy<User> _assignedTo = null;

        /// <summary>
        /// A description of the task
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The due date of the task
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// The id of the user that the task is assigned to
        /// </summary>        
        public int? AssignedTo { get; set; }
                
        /// <summary>
        /// If the task is completed or not
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// The priority of the task
        /// </summary>
        public TaskPriority Priority { get; set; }

        /// <summary>
        /// The sort order of the task
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// If the task i starred or not
        /// </summary>
        public bool IsStarred => this.IsStarred();

        /// <summary>
        /// Gets the ids of all comments.
        /// </summary>
        [ScaffoldColumn(false)]
        public IEnumerable<int> CommentIds { get; set; }

        /// <summary>
        /// Gets the ids of all that starred this task
        /// </summary>
        public IEnumerable<int> StarredByIds { get; set; }

        /// <summary>
        /// A lazy property returning the user object of the assigned user
        /// </summary>        
        public User AssignedToUser {
            get {
                if (_assignedTo == null) {
                    _assignedTo = new Lazy<User>(() => {
                        if (!AssignedTo.HasValue) {
                            return null;
                        }
                        return UserService.Get(AssignedTo.Value);
                    });
                }
                return _assignedTo.Value;
            }
        }
    }

   
}