using System;
using HealthMonitoring.Model;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.Management.Core
{
    public class Endpoint : IDisposable
    {
        private readonly ITimeCoordinator _timeCoordinator;
        public EndpointIdentity Identity { get; }
        public EndpointMetadata Metadata { get; private set; }
        public bool IsDisposed { get; private set; }
        public EndpointHealth Health { get; private set; }
        public DateTimeOffset LastModifiedTimeUtc { get; private set; }

        public Endpoint(ITimeCoordinator timeCoordinator, EndpointIdentity identity, EndpointMetadata metadata)
        {
            _timeCoordinator = timeCoordinator;
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
            LastModifiedTimeUtc = _timeCoordinator.UtcNow;
        }

        private void UpdatePrivateToken(string token)
        {
            Identity.PrivateToken = token;
        }

        public Endpoint UpdateEndpoint(string group, string name, string[] tags, string token = null)
        {
            Metadata = new EndpointMetadata(name, group, tags ?? Metadata.Tags);
            UpdatePrivateToken(token);
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