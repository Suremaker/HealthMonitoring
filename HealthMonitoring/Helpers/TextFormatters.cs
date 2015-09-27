using System.Linq;
using HealthMonitoring.Model;

namespace HealthMonitoring.Helpers
{
    static class TextFormatters
    {
        public static string PrettyFormatDetails(this EndpointHealth endpointHealth)
        {
            return string.Join(", ", endpointHealth.Details.Select(kv => string.Format("{0}={1}", kv.Key, PrettyFormatLongText(kv.Value, 128))));
        }

        private static string PrettyFormatLongText(this string text, int maxLength)
        {
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength) + "...";
        }
    }
}