using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Integration.PushClient.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Integration.PushClient.UnitTests
{
    public class BackOffStrategyTests
    {


        [Theory]
        [InlineData(null, 1D)]
        [InlineData(1D, 2D)]
        [InlineData(2D, 4D)]
        [InlineData(16D, 32D)]
        [InlineData(120D, 120D)]
        [InlineData(121D, 120D)]
        public async Task RecommendedBackOffStrategy_Returns_Expectected_NextTimeInterval(double? currentInterval, double? nextInterval)
        {
            TimeSpan? currentTimeInterval = currentInterval.HasValue
                ? TimeSpan.FromSeconds(currentInterval.Value)
                : (TimeSpan?) null;

            var result = await new RecommendedBackOffStrategy().NextInterval(currentTimeInterval, It.IsAny<CancellationToken>());
            
            Assert.Equal(nextInterval, result.Value.TotalSeconds);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(1D, true)]
        [InlineData(2D, true)]
        [InlineData(16D, true)]
        [InlineData(120D, true)]
        [InlineData(121D, true)]
        public async Task RecommendedBackOffStrategy_Returns_Expectected_ShouldLogStatus(double? currentInterval, bool shouldLog)
        {
            TimeSpan? currentTimeInterval = currentInterval.HasValue
                ? TimeSpan.FromSeconds(currentInterval.Value)
                : (TimeSpan?)null;

            var result = await new RecommendedBackOffStrategy().RecordLog(currentTimeInterval, It.IsAny<CancellationToken>());

            Assert.Equal(result, shouldLog);
        }

        [Theory]
        [InlineData(null, 1D, true)]
        [InlineData(1D, 2D, true)]
        [InlineData(2D, 4D, true)]
        [InlineData(16D, 32D, true)]
        [InlineData(120D, 120D, true)]
        [InlineData(121D, 120D, true)]
        public async Task RecommendedBackOffStrategy_Returns_Expectected_BackOffPlan(double? currentInterval, double nextInterval, bool shouldLog)
        {
            TimeSpan? currentTimeInterval = currentInterval.HasValue
                ? TimeSpan.FromSeconds(currentInterval.Value)
                : (TimeSpan?)null;

            var result = await new RecommendedBackOffStrategy().GetCurrent(currentTimeInterval, It.IsAny<CancellationToken>());

            Assert.NotNull(result);
            Assert.Equal(result.ShouldLog, shouldLog);
            Assert.Equal(result.RetryInterval, TimeSpan.FromSeconds(nextInterval));
        }
    }
}
