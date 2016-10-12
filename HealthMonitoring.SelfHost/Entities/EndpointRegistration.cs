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
        [CustomValidation(typeof(TagsValidator), "CheckForUnallowedSymbols")]
        public string[] Tags { get; set; }
        [MinLength(64)]
        public string PrivateToken { get; set; }

        public string GetNaturalKey()
        {
            return $"{MonitorType}|{Address.ToLowerInvariant()}";
        }
    }
}