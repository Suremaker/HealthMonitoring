namespace HealthMonitoring.Protocols.Rest
{
    public class RestProtocol : IHealthCheckProtocol
    {
        public string Name { get { return "rest"; } }
    }
}
