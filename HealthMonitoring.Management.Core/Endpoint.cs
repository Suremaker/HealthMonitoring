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
        public string PrivateToken { get; private set; }

        public Endpoint(ITimeCoordinator timeCoordinator, EndpointIdentity identity, EndpointMetadata metadata, string privateToken = null)
        {
            _timeCoordinator = timeCoordinator;
            Identity = identity;
            Metadata = metadata;
            PrivateToken = privateToken;
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

        public Endpoint UpdateMetadata(string group, string name, string[] tags)
        {
            Metadata = new EndpointMetadata(name, group, tags ?? Metadata.Tags);
            UpdateLastModifiedTime();
            return this;
        }

        public Endpoint UpdateEndpoint(string group, string name, string[] tags, string privateToken)
        {
            UpdateMetadata(group, name, tags ?? Metadata.Tags);
            PrivateToken = privateToken;
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