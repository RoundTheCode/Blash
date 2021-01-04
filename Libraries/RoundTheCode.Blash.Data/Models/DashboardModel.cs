using System.ComponentModel.DataAnnotations;

namespace RoundTheCode.Blash.Data.Models
{
    /// <summary>
    /// Used when a dashboard is created in RoundTheCode.Blash.BlazorWasm and RoundTheCode.Blash.Api.
    /// </summary>
    public class DashboardModel
    {
        /// <summary>
        /// The dashboard's title (required).
        /// </summary>
        [Required]
        public string Title { get; set; }
    }
}
