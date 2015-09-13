using System;
using System.Collections.Generic;

namespace HealthMonitoring.AcceptanceTests.Helpers.Entities
{
    internal class EndpointEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Protocol { get; set; }
        public string Group { get; set; }
        public EndpointStatus? Status { get; set; }
        public DateTime? LastCheckUtc { get; set; }
        public TimeSpan? LastResponseTime { get; set; }
        public IDictionary<string,string> Details { get; set; }

        public override string ToString()
        {
            return string.Format(
                    "Name: {0}, Address: {1}, Protocol: {2}, Group: {3}, Status: {4}, LastCheckUtc: {5}, LastResponseTime: {6}, Details: {7}",
                    Name, Address, Protocol, Group, Status, LastCheckUtc, LastResponseTime, Details);
        }
    }
}