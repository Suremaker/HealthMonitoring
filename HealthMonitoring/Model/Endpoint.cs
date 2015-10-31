using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors;

namespace HealthMonitoring.Model
{
    public class Endpoint : IDisposable
    {
        private readonly IHealthMonitor _monitor;

        public Endpoint(Guid id, IHealthMonitor monitor, string address, string name, string group)
        {
            Id = id;
            _monitor = monitor;
            Address = address;
            Name = name;
            Group = group;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string MonitorType { get { return _monitor.Name; } }
        public IHealthMonitor Monitor{ get { return _monitor; } }
        public string Group { get; private set; }
        public bool IsDisposed { get; private set; }
        public EndpointHealth Health { get; private set; }

        public Endpoint Update(string group, string name)
        {
            Group = group;
            Name = name;
            return this;
        }

        public async Task CheckHealth(IHealthSampler sampler, CancellationToken cancellationToken)
        {
            Health = await sampler.CheckHealth(this, cancellationToken);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} ({2}: {3})", Group, Name, MonitorType, Address);
        }
    }
}