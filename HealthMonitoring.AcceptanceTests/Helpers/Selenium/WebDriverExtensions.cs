using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace HealthMonitoring.AcceptanceTests.Helpers.Selenium
{
    public static class WebDriverExtensions
    {
        public static IWebElement WaitElementIsRendered(this IWebDriver driver, By selector)
        {
            return Wait.Until(Timeouts.Default,
                () => FindElement(driver, selector),
                element => element != null,
                $"Element with selector:{selector} could not be rendered");
        }

        public static string WaitTextIsRendered(this IWebDriver driver, By elementSelector)
        {
            return Wait.Until(Timeouts.Default,
                () => FindElement(driver, elementSelector)?.Text,
                text => !string.IsNullOrEmpty(text),
                $"Element with selector:{elementSelector} could not be rendered");
        }

        public static IEnumerable<IWebElement> WaitElementsAreRendered(this IWebDriver driver, By selector, Func<IWebElement, bool> condition = null)
        {
            condition = condition ?? (el => true);
             
            return Wait.Until(Timeouts.Default,
                () => FindElements(driver, selector)?.Where(condition),
                elements => elements != null && elements.Any(),
                $"Elements with selector:{selector} could not be rendered");
        }

        public static void LoadUrl(this IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public static string WaitUntilPageIsChanged(this IWebDriver driver, string initialUrl)
        {
            return Wait.Until(
                Timeouts.Default,
                () => driver.Url,
                url => !string.Equals(url, initialUrl),
                $"Page url: {initialUrl} did not change!");
        }

        public static void RetryTimeout(this IWebDriver driver, TimeSpan period)
        {
            driver.Manage().Timeouts().ImplicitlyWait(period);
        }

        private static IWebElement FindElement(IWebDriver driver, By selector)
        {
            try
            {
                return driver.FindElement(selector);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        private static IEnumerable<IWebElement> FindElements(IWebDriver driver, By selector)
        {
            try
            {
                return driver.FindElements(selector);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }
    }
}