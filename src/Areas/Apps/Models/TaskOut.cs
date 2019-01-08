using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wvy.Areas.Apps.Models {
    internal class TaskOut {
        public TaskOut() {
        }

        public int TaskId { get; set; }
        public bool Completed { get; set; }
    }

}