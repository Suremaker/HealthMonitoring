using System.ComponentModel.DataAnnotations;

namespace HealthMonitoring.SelfHost.Entities
{
    public class TagsModel
    {
        [Required]
        [CustomValidation(typeof(TagsAllowedSymbols), "ValidateTags")]
        public string[] Tags { get; set; }
    }
}
