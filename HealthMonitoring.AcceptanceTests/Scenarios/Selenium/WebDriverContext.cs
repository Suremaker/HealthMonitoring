using System;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Selenium;
using OpenQA.Selenium;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    public class WebDriverContext : IDisposable
    {
        public IWebDriver Driver { get; }

        public WebDriverContext()
        {
            Driver = SeleniumConfiguration.GetWebDriver();
            Driver.RetryTimeout(Timeouts.Default);
            Driver.Manage().Window.Maximize();
        }

        public void Dispose()
        {
            Driver.Dispose();
        }
    }
}