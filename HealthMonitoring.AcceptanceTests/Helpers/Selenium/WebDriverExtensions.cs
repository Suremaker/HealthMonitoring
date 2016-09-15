using System;
using OpenQA.Selenium;

namespace HealthMonitoring.AcceptanceTests.Helpers.Selenium
{
    public static class WebDriverExtensions
    {
        // time to wait until DOM is rendered
        private static int _waitTime = 5;

        public static void WaitDomIsRendered(this IWebDriver driver, int seconds)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(seconds));
        }

        public static void LoadUrl(this IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
            driver.WaitDomIsRendered(_waitTime);
        }
    }
}