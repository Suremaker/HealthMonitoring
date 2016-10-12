using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthMonitoring.SelfHost.Entities;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Validators
{
    public class EntityValidatorTest
    {
        [Fact]
        public void EntityValidator_should_raise_validation_exception_if_endpoint_entity_token_is_short()
        {
            var endpoint = new EndpointRegistration
            {
                Address = "address", Group = "group", MonitorType = "http", Name = "name", PrivateToken = "private_token"
            };

            Assert.Throws<ValidationException>(() => endpoint.ValidateModel());
        }
    }
}
