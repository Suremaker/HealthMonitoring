using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HealthMonitoring.SelfHost.Entities;

namespace HealthMonitoring.SelfHost.Filters
{
    internal class EndpointFilter
    {
        private readonly List<Func<EndpointDetails, bool>> _filters = new List<Func<EndpointDetails, bool>> { e => true };
        public EndpointFilter WithStatus(string[] filterStatus)
        {
            if (filterStatus != null && filterStatus.Any())
            {
                _filters.Add(e =>
                {
                    var status = e.Status.ToString();
                    return filterStatus.Any(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));
                });
            }

            return this;
        }

        public EndpointFilter WithTags(string[] filterTags)
        {
            if (filterTags != null && filterTags.Any())
                _filters.Add(e => filterTags.All(tag => e.Tags.Any(etag => etag.Equals(tag, StringComparison.OrdinalIgnoreCase))));

            return this;
        }

        public EndpointFilter WithGroup(string filterGroup)
        {
            if (!string.IsNullOrWhiteSpace(filterGroup))
            {
                var search = PrepareSearch(filterGroup, true);
                _filters.Add(e => search(e.Group));
            }
            return this;
        }

        public EndpointFilter WithText(string filterText)
        {
            if (!string.IsNullOrWhiteSpace(filterText))
            {
                var search = PrepareSearch(filterText, false);
                _filters.Add(e => search(e.Group) || search(e.Name) || search(e.Status.ToString()) || search(e.MonitorType) || search(e.Address));
            }
            return this;
        }

        private static Func<string, bool> PrepareSearch(string filter, bool exactMatch)
        {
            var pattern = Regex.Escape(filter).Replace("\\*", ".*").Replace("\\?", ".");
            if (exactMatch)
                pattern = "^" + pattern + "$";

            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return regex.IsMatch;
        }

        public bool DoesMatch(EndpointDetails endpoint)
        {
            return _filters.All(f => f.Invoke(endpoint));
        }
    }
}