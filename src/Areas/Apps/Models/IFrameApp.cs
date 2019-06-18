using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Weavy.Core.Attributes;
using Weavy.Core.Models;

namespace Weavy.Areas.Apps.Models {
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Guid("336F0C5F-019E-459B-B79E-3DE8F56E8D56")]
    [App(Icon = "application", Name = "IFrame", Description = "Generic app for embedding a web page in a space.", AllowMultiple = true)]
    public class IFrameApp : App {

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [DataType(DataType.Url)]
        [Display(Name = "Frame source", Description = "Full url of the resource to embed including http(s)://")]
        [Uri(ErrorMessage = "Must be a valid and fully-qualified url.")]
        public string Src { get; set; }
    }
}
