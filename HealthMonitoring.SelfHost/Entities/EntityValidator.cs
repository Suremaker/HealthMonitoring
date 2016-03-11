using System;
using System.ComponentModel.DataAnnotations;

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
}