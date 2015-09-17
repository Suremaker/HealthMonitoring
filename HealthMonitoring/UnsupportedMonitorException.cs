using System;

namespace HealthMonitoring
{
    public class UnsupportedMonitorException : InvalidOperationException
    {
        public UnsupportedMonitorException(string monitorType):base(string.Format("Unsupported monitor: {0}",monitorType))
        {
        }
    }
}