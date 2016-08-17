using System;
using System.Linq;

namespace HealthMonitoring.Persistence
{
    public static class TagsHelper
    {
        private static string separator = ",";
        public static string ToDbString(this string[] tags)
        {
            if (tags == null)
                return null;

            if (!tags.Any())
                return string.Empty;

            return string.Join(separator, tags);
        }

        public static string[] FromDbString(this string tags)
        {
            return tags?.Split(new [] {separator}, StringSplitOptions.None).ToArray();
        }
    }
}
