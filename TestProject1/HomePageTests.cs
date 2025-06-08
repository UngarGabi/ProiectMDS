using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace MDSTests
{
    public class HomePageTests
    {
        private IWebDriver? driver;
        private string baseUrl = "https://localhost:7221"; 

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("start-maximized");
            options.AddArgument("--remote-allow-origins=*");

            driver = new ChromeDriver(options);
        }

        [Test]
        public void HomePage_Tests()
        {
            driver!.Navigate().GoToUrl(baseUrl + "/");

            var header = driver.FindElement(By.TagName("h1"));
            Assert.That(header.Text, Does.Contain("Glow Boutique"));

            var button = driver.FindElement(By.CssSelector("a.btn.btn-primary"));
            Assert.That(button.Text, Does.Contain("Vezi Produsele"));

            var testimonials = driver.FindElements(By.TagName("blockquote"));
            Assert.That(testimonials.Count, Is.EqualTo(3));
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}