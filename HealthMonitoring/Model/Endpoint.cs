using System;

namespace HealthMonitoring.Model
{
    public class Endpoint
    {
        public Endpoint(Guid id, string protocol, string address, string name, string @group)
        {
            Id = id;
            Protocol = protocol;
            Address = address;
            Name = name;
            Group = group;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Protocol { get; private set; }
        public string Group { get; private set; }

        public Endpoint Update(string group, string name)
        {
            Group = group;
            Name = name;
            return this;
        }
    }
}