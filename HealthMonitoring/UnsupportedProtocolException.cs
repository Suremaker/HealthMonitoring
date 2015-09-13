using System;

namespace HealthMonitoring
{
    public class UnsupportedProtocolException : InvalidOperationException
    {
        public UnsupportedProtocolException(string protocol):base(string.Format("Unsupported protocol: {0}",protocol))
        {
        }
    }
}