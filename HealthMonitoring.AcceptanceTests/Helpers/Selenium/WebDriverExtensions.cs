using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace HealthMonitoring.AcceptanceTests.Helpers.Selenium
{
    public static class WebDriverExtensions
    {
        public static void LoadUrl(this IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public static string ImplicitWaitPageTitle(this IWebDriver driver)
        {
            return driver.ImplicitWait(
                Timeouts.Default,
                _ => driver.Title,
                t => !string.IsNullOrEmpty(t),
                "Page does not contain title!");
        }

        public static IWebElement ImplicitWaitElementIsRendered(this IWebDriver driver, By selector)
        {
            return driver.ImplicitWait(
                TimeSpan.FromSeconds(60),
                _ => driver.FindElement(selector),
                element => element != null && element.Displayed,
                $"Element with selector:{selector} could not be rendered");
        }

        public static string ImplicitWaitTextIsRendered(this IWebDriver driver, By elementSelector)
        {
            return driver.ImplicitWait(
                Timeouts.Default,
                _ => driver.FindElement(elementSelector)?.Text,
                text => !string.IsNullOrEmpty(text),
                $"Element with selector:{elementSelector} could not be rendered");
        }

        public static IEnumerable<IWebElement> ImplicitWaitElementsAreRendered(this IWebDriver driver, By selector, Func<IWebElement, bool> condition = null)
        {
            condition = condition ?? (el => true);
            
            return driver.ImplicitWait(
                Timeouts.Default,
                _ => driver.FindElements(selector)?.Where(condition),
                elements => elements != null && elements.Any(),
                $"Elements with selector:{selector} could not be rendered");
        }
        
        public static string ImplicitWaitUntilPageIsChanged(this IWebDriver driver, string expectedUrl)
        {
            return driver.ImplicitWait(
                Timeouts.Default,
                _ => driver.Url,
                url => string.Equals(url, expectedUrl, StringComparison.CurrentCultureIgnoreCase),
                $"Page with url {expectedUrl} did not loaded!");
        }


        public static IWebElement ExplicitWaitElementIsRendered(this IWebDriver driver, Func<IWebDriver, IWebElement> selector)
        {
            return driver.ExplicitWait(
                TimeSpan.FromSeconds(60),
                selector,
                $"Element with selector:{selector} could not be rendered");
        }

        public static string ExplicitWaitTextIsRendered(this IWebDriver driver, Func<IWebDriver, IWebElement> selector)
        {
            return driver.ExplicitWait(
                Timeouts.Default,
                selector,
                $"Element with selector:{selector} could not be rendered").Text;
        }

        public static IEnumerable<IWebElement> ExplicitWaitElementsAreRendered(this IWebDriver driver, 
            Func<IWebDriver, IEnumerable<IWebElement>> selector)
        {
             return driver.ExplicitWait(
                 Timeouts.Default,
                 selector,
                 $"Elements with selector:{selector} could not be rendered");
        }

        public static bool ExplicitWaitUntilPageIsChanged(this IWebDriver driver, Func<IWebDriver, bool> selector, string expectedUrl)
        {
            return driver.ExplicitWait(
                Timeouts.Default,
                selector,
                $"Page with url {expectedUrl} did not loaded!");
        }

        public static T ImplicitWait<T, TWebDriver>(this TWebDriver webDriver,
            TimeSpan implicitWait,
            Func<TWebDriver, T> selector,
            Func<T, bool> predicate,
            string errorMessage)
            where TWebDriver : IWebDriver
        {
            T valueFound = default(T);

            try
            {
                webDriver.Manage().Timeouts().ImplicitWait = implicitWait;

                valueFound = selector(webDriver);

                if (predicate(valueFound))
                    return valueFound;

                throw new NoSuchElementException($"{valueFound} not found");
            }
            catch (NoSuchElementException e)
            {
                throw new TimeoutException($"Error message : {errorMessage} ; " +
                                           $"Value sought : [{valueFound}] ; " +
                                           $"NoSuchElementException Message : [{e.Message}]");
            }
        }

        public static T ExplicitWait<T, TWebDriver>(this TWebDriver webDriver,
            TimeSpan explicitWait,
            Func<IWebDriver, T> selector,
            string errorMessage)
            where TWebDriver : IWebDriver
        {
            T valueFound = default(T);

            try
            {
                valueFound = new WebDriverWait(webDriver, explicitWait).Until(selector);

                return valueFound;
            }
            catch (NoSuchElementException e)
            {
                throw new TimeoutException($"Error message : {errorMessage} ; " +
                                           $"Value sought : [{valueFound}] ; " +
                                           $"NoSuchElementException Message : [{e.Message}]");
            }
        }

    }
}