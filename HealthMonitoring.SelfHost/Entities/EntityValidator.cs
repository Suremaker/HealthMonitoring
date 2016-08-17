using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HealthMonitoring.SelfHost.Entities
{
    public static class EntityValidator
    {
        public static void ValidateModel(this object model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            Validator.ValidateObject(model, new ValidationContext(model));
        }
    }

    public class TagsAllowedSymbols
    {
        public static ValidationResult ValidateTags(string[] tags)
        {
            const string allowedSymbols = "_";

            var symbols = string.Join(string.Empty, tags ?? new string[0]);

            bool isValid = symbols.All(symbol => char.IsLetterOrDigit(symbol) || allowedSymbols.Contains(symbol));

            if (isValid)
            {
                return ValidationResult.Success;
            }

            throw new ArgumentException("Tags contains unallowed symbols.");
        }
    }
}