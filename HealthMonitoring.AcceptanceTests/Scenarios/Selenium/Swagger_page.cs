using System.Net;
using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    [FeatureDescription(
@"In order to use the api
As User 
I want to open swagger page")]
    public partial class Swagger_page
    {
        [Scenario]
        public void Verification_of_basic_authentication_with_valid_credentials()
        {
            Runner.RunScenario(
                _ => Given_swagger_page(),
                _ => Given_endpoint_with_password_is_registered("registered", "http://localhost:2595/status", "group", MonitorTypes.HttpJson, _credentials.PersonalCredentials.Password),
                _ => Given_endpoint_id_is_received(),
                _ => When_user_expand_DELETE_endpoint_section(),
                _ => When_user_fills_auth_form(_credentials.PersonalCredentials),
                _ => When_user_enters_endpoint_id_parameter(),
                _ => When_user_sends_request(),
                _ => Then_response_status_should_be(HttpStatusCode.OK)
                );
        }

        [Scenario]
        public void Verification_of_basic_authentication_with_wrong_credentials()
        {
            Runner.RunScenario(
                _ => Given_swagger_page(),
                _ => Given_endpoint_with_password_is_registered("registered", "http://localhost:2595/status", "group", MonitorTypes.HttpJson, _credentials.MonitorCredentials.Password),
                _ => Given_endpoint_id_is_received(),
                _ => When_user_expand_DELETE_endpoint_section(),
                _ => When_user_fills_auth_form(_credentials.MonitorCredentials),
                _ => When_user_enters_endpoint_id_parameter(),
                _ => When_user_sends_request(),
                _ => Then_response_status_should_be(HttpStatusCode.Forbidden)
                );
        }
    }
}
