namespace HealthMonitoring.Integration.PushClient
{
    public enum HealthStatus
    {
        /// <summary>
        /// The target endpoint is fully operational
        /// </summary>
        Healthy,
        /// <summary>
        /// The target endpoint is faulty and it is not able to function properly
        /// </summary>
        Faulty,
        /// <summary>
        /// The target endpoint is operational but has performance or other minor issues
        /// </summary>
        Unhealthy,
        /// <summary>
        /// The target endpoint exists, but is not actively serving requests (it is offline / put into maintenance etc)
        /// </summary>
        Offline,
    }
}