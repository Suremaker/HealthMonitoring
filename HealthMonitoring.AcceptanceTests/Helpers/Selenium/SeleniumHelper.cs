using RestSharp;

namespace HealthMonitoring.AcceptanceTests.Helpers.Selenium
{
    public static class SeleniumHelper
    {
        public static string[] TestTags =  { "app", "web", "api" };
        public static string[] TestGroups =  { "first-group", "second-group", "third-group" };

        public static void RegisterTestEndpoints(this RestClient client)
        {
            // unhealthy endpoints
            client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8090", TestGroups[0], "1", TestTags);
            client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8091", TestGroups[0], "2");
            client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8092", TestGroups[2], "3", TestTags);
            client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8093", TestGroups[0], "4");
            client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8094", TestGroups[2], "5", TestTags);
            client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8095", TestGroups[2], "6");
            // healthy endpoints
            client.RegisterEndpoint(MonitorTypes.Http, SeleniumConfiguration.ProjectUrl, TestGroups[0], "7");
            client.RegisterEndpoint(MonitorTypes.Http, SeleniumConfiguration.BaseUrl, TestGroups[1], "8-valid", TestTags);
        }
    }
}
