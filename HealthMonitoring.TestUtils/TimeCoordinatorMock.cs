using System;
using HealthMonitoring.TimeManagement;
using Moq;

namespace HealthMonitoring.TestUtils
{
    public static class TimeCoordinatorMock
    {
        public static Mock<ITimeCoordinator> Get()
        {
            var coordinator = new Mock<ITimeCoordinator>();

            coordinator.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

            return coordinator;
        }
    }
}
