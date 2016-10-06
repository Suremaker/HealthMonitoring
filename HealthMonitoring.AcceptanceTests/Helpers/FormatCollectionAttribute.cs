using System.Collections;
using System.Linq;
using LightBDD.Formatting.Parameters;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    class FormatCollectionAttribute : ParameterFormatterAttribute 
    {
        public override string Format(object parameter)
        {
            var collection=(IEnumerable) parameter;
            return string.Join(", ", collection.Cast<object>());
        }
    }
}