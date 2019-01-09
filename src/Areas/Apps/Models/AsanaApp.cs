using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Weavy.Core.Models;

namespace Wvy.Areas.Apps.Models {

    [Serializable]
    [Guid("B309F192-9F22-4E39-AAFC-DD59589227C7")]
    [App(Icon = "asana-logo", Color ="native", Name = "Asana tasks", Description = "A Asana integration demo", AllowMultiple = true)]

    public class AsanaApp: App {

        [Required]
        [Display(Name = "Client Id", Description = "The client Id of the asana developer app you have created")]
        public string ClientId { get; set; }
    }
}