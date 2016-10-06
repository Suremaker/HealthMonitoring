namespace HealthMonitoring.Monitors
{
    public enum HealthStatus
    {
        /// <summary>
        /// The target endpoint does not exist
        /// </summary>
        NotExists = 0,
        /// <summary>
        /// The target endpoint exists, but is not active, shutdown or offline
        /// </summary>
        Offline = 1,
        /// <summary>
        /// The target endpoint is fully operational
        /// </summary>
        Healthy = 2,
        /// <summary>
        /// The target endpoint is faulty, a communication error occurred or endpoint definition is invalid
        /// </summary>
        Faulty = 3,
        /// <summary>
        /// The target endpoint is operational but has long response time or other issues
        /// </summary>
        Unhealthy = 4,
        /// <summary>
        /// The target endpoint may be operational but it did not finished in specified time
        /// </summary>
        TimedOut=5
    }
}