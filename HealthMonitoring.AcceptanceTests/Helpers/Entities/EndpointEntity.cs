using System;
using System.Collections.Generic;

namespace HealthMonitoring.AcceptanceTests.Helpers.Entities
{
    internal class EndpointEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string MonitorType { get; set; }
        public string Group { get; set; }
        public EndpointStatus? Status { get; set; }
        public DateTime? LastCheckUtc { get; set; }
        public TimeSpan? LastResponseTime { get; set; }
        public IDictionary<string,string> Details { get; set; }
        public string[] Tags { get; set; }

        public override string ToString()
        {
            return
                $"Name: {Name}, Address: {Address}, MonitorType: {MonitorType}, Group: {Group}, Status: {Status}, LastCheckUtc: {LastCheckUtc}, LastResponseTime: {LastResponseTime}, Details: {Details}";
        }
    }
}