using System;

namespace HealthMonitoring.Monitors.Core.Exchange
{
    public class DataExchangeConfig
    {
        public int OutgoingQueueMaxCapacity { get; }
        public int ExchangeOutBucketSize { get; }
        public TimeSpan UploadRetryInterval { get; }
        public TimeSpan EndpointChangeQueryInterval { get; }
        public string MonitorTag { get; }

        public DataExchangeConfig(int outgoingQueueMaxCapacity, int exchangeOutBucketSize, TimeSpan uploadRetryInterval, TimeSpan endpointChangeQueryInterval, string monitorTag)
        {
            OutgoingQueueMaxCapacity = outgoingQueueMaxCapacity;
            ExchangeOutBucketSize = exchangeOutBucketSize;
            UploadRetryInterval = uploadRetryInterval;
            EndpointChangeQueryInterval = endpointChangeQueryInterval;
            MonitorTag = monitorTag;
        }
    }
}