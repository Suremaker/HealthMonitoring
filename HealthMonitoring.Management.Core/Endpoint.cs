using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Management.Core
{
    public class Endpoint : IDisposable
    {
        public EndpointIdentity Identity { get; }
        public EndpointMetadata Metadata { get; private set; }
        public bool IsDisposed { get; private set; }
        public EndpointHealth Health { get; private set; }
        public DateTimeOffset LastModifiedTime { get; private set; }

        public Endpoint(EndpointIdentity identity, EndpointMetadata metadata)
        {
            Identity = identity;
            Metadata = metadata;
            UpdateLastModifiedTime();
        }

        public void Dispose()
        {
            IsDisposed = true;
            Health = null;
            UpdateLastModifiedTime();
        }

        private void UpdateLastModifiedTime()
        {
            LastModifiedTime = DateTimeOffset.UtcNow;
        }

        public Endpoint UpdateMetadata(string group, string name)
        {
            Metadata = new EndpointMetadata(name, group);
            UpdateLastModifiedTime();
            return this;
        }

        public Endpoint UpdateHealth(EndpointHealth health)
        {
            if (Health != null && Health.CheckTimeUtc > health.CheckTimeUtc)
                return this;

            Health = health;
            UpdateLastModifiedTime();
            return this;
        }

        public override string ToString()
        {
            return Metadata.ToString();
        }
    }
}