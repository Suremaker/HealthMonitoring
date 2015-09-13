using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Protocols;

namespace HealthMonitoring.Model
{
    public class Endpoint : IDisposable
    {
        private readonly IHealthCheckProtocol _protocol;

        public Endpoint(Guid id, IHealthCheckProtocol protocol, string address, string name, string group)
        {
            Id = id;
            _protocol = protocol;
            Address = address;
            Name = name;
            Group = group;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Protocol { get { return _protocol.Name; } }
        public string Group { get; private set; }
        public bool IsDisposed { get; private set; }

        public Endpoint Update(string group, string name)
        {
            Group = group;
            Name = name;
            return this;
        }

        public async Task CheckHealth(CancellationToken cancellationToken)
        {
            var health = await _protocol.CheckHealthAsync(Address, cancellationToken);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}