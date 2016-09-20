using System;
using System.Linq;
using OpenQA.Selenium;

namespace HealthMonitoring.AcceptanceTests.Helpers.Selenium
{
    public static class WebDriverExtensions
    {
        private static int _waitSeconds = 5;

        public static void WaitElementIsRendered(this IWebDriver driver, By selector)
        {
            Wait.Until(Timeouts.Default,
                () => driver.FindElement(selector),
                element => element.Displayed,
                $"Element with selector:{selector} could not be rendered");
        }

        public static void WaitElementsAreRendered(this IWebDriver driver, By selector)
        {
            Wait.Until(Timeouts.Default,
                () => driver.FindElements(selector),
                elements => elements.Any(),
                $"Elements with selector:{selector} could not be rendered");
        }

        public static void LoadUrl(this IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
            driver.Sleep(_waitSeconds);
        }

        public static void Sleep(this IWebDriver driver, int seconds)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(seconds));
        }
    }
}