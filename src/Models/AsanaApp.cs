using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using Weavy.Core.Models;

namespace Wvy.Models {

    [Serializable]
    [Guid("B309F192-9F22-4E39-AAFC-DD59589227C7")]
    [App(Icon = "application", Name = "Asana tasks", Description = "A Asana integration demo", AllowMultiple = true)]

    public class AsanaApp: App {

        [Required]
        [Display(Name = "Client Id", Description = "The client Id of the asana developer app you have created")]
        public string ClientId { get; set; }
    }
}