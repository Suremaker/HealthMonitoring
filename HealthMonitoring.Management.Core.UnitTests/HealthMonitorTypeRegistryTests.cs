using System.Linq;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Management.Core.Repositories;
using Moq;
using Xunit;

namespace HealthMonitoring.Management.Core.UnitTests
{
    public class HealthMonitorTypeRegistryTests
    {
        private readonly Mock<IHealthMonitorTypeRepository> _repository;

        public HealthMonitorTypeRegistryTests()
        {
            _repository = new Mock<IHealthMonitorTypeRepository>();
        }

        [Fact]
        public void Registry_should_register_push_monitor_type_during_construction_and_return_it()
        {
            var registry = new HealthMonitorTypeRegistry(_repository.Object);
            _repository.Verify(r => r.SaveMonitorType(HealthMonitorTypeRegistry.PushMonitorType), Times.Once);
            Assert.Equal(new[] { "push" }, registry.GetMonitorTypes().ToArray());
        }

        [Fact]
        public void GetMonitorTypes_should_return_all_currently_registered_monitors()
        {
            _repository.Setup(r => r.LoadMonitorTypes()).Returns(new[] { "abc", "def" });
            var registry = new HealthMonitorTypeRegistry(_repository.Object);
            registry.RegisterMonitorType("ghi");
            Assert.Equal(new[] { "abc", "def", "ghi", HealthMonitorTypeRegistry.PushMonitorType }, registry.GetMonitorTypes().OrderBy(t => t));
        }

        [Fact]
        public void RegisterMonitorType_should_register_new_types_and_ignore_currently_existing_ones()
        {
            _repository.Setup(r => r.LoadMonitorTypes()).Returns(new[] { "abc" });

            var registry = new HealthMonitorTypeRegistry(_repository.Object);

            Assert.Equal(new[] { "abc", HealthMonitorTypeRegistry.PushMonitorType }, registry.GetMonitorTypes().OrderBy(t => t));

            registry.RegisterMonitorType("def");
            registry.RegisterMonitorType("abc");
            registry.RegisterMonitorType("ghi");
            Assert.Equal(new[] { "abc", "def", "ghi", HealthMonitorTypeRegistry.PushMonitorType }, registry.GetMonitorTypes().OrderBy(t => t));
        }
    }
}