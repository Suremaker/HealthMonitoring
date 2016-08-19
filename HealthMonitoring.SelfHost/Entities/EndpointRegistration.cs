using System.ComponentModel.DataAnnotations;

namespace HealthMonitoring.SelfHost.Entities
{
    public class EndpointRegistration
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string MonitorType { get; set; }
        [Required]
        public string Group { get; set; }
        [CustomValidation(typeof(TagsAllowedSymbols), "ValidateTags")]
        public string[] Tags { get; set; }
    }
}