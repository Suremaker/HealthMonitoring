using System;
using System.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;

namespace HealthMonitoring.AcceptanceTests.Helpers.Selenium
{
    public static class SeleniumConfiguration
    {
        private const string _driverKey = "WebDriver";
        private const string _urlKey = "BaseUrl";
        private const string _projectUrlKey = "ProjectUrl";

        public static string BaseUrl => Get<string>(_urlKey);
        public static string ProjectUrl => Get<string>(_projectUrlKey);

        public static IWebDriver GetWebDriver()
        {
            DriverToTest driver = Get<DriverToTest>(_driverKey);

            switch (driver)
            {
                case DriverToTest.Chrome:
                    return new ChromeDriver();
                case DriverToTest.InternetExplorer:
                    return new InternetExplorerDriver();
                default: return new ChromeDriver();
            }
        }

        public static T Get<T>(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                throw new ArgumentException($"Cannot find value by key={key}");
            }

            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), value);
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}