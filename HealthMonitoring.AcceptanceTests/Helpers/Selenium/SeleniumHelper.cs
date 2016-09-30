using System;
using System.Collections.Generic;
using RestSharp;

namespace HealthMonitoring.AcceptanceTests.Helpers.Selenium
{
    static class SeleniumHelper
    {
        public static string[] TestTags = { "app", "web", "api" };
        public static string[] TestGroups = { "first-group", "second-group", "third-group" };
        public static string[] UniqueTags = { "unique_for_home_page", "tags4568885" };

        public static void RegisterTestEndpoints(this RestClient client)
        {
            var guids = new List<Guid>
            {
                client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8090", TestGroups[0], "1", TestTags),
                client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8091", TestGroups[0], "2"),
                client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8092", TestGroups[2], "3", TestTags),
                client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8093", TestGroups[0], "4"),
                client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8094", TestGroups[2], "5", TestTags),
                client.RegisterEndpoint(MonitorTypes.Http, "http://localhost:8095", TestGroups[2], "6"),
                client.RegisterEndpoint(MonitorTypes.Http, SeleniumConfiguration.ProjectUrl, TestGroups[0],"project page"),
                client.RegisterEndpoint(MonitorTypes.Http, SeleniumConfiguration.BaseUrl, TestGroups[1], "self",TestTags),
                client.RegisterEndpoint(MonitorTypes.Http, "http://localhost", TestGroups[1], "localhost", UniqueTags)
            };

            foreach (var guid in guids)
                client.EnsureMonitoringStarted(guid);
        }
    }
}
