using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wvy.Models {
    /// <summary>
    /// Representing a task from a xhr request
    /// </summary>
    public class TaskIn {

        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? AssignedTo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TaskPriority Priority { get; set; }
    }
}