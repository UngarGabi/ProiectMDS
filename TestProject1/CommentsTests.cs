using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace MDSTests
{
    public class CommentsTests
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private string appUrl = "https://localhost:7221";

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }

        [TestCase("user@test.com", "User1!", "User")]
        [TestCase("colaborator@test.com", "Colaborator1!", "Colaborator")]
        [TestCase("admin@test.com", "Admin1!", "Admin")]
        public void Test_Comments(string email, string password, string role)
        {
            // LOGIN
            driver.Navigate().GoToUrl($"{appUrl}/Identity/Account/Login");
            wait.Until(d => d.FindElement(By.Id("Input_Email"))).SendKeys(email);
            driver.FindElement(By.Id("Input_Password")).SendKeys(password);
            driver.FindElement(By.Id("login-submit")).Click();

            // Navigheaza la produsul 1
            driver.Navigate().GoToUrl($"{appUrl}/Products/Show/1");
            Assert.That(driver.Url, Does.Contain("/Products/Show/1"));
            // Asteapta sa fie clickabil si foloseste Actions pentru a da click
            var starElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("Rate4")));
            new OpenQA.Selenium.Interactions.Actions(driver).MoveToElement(starElement).Click().Perform();

            // Scrie recenzie
            var textarea = wait.Until(d => d.FindElement(By.Name("CommentContent")));
            textarea.Clear();
            textarea.SendKeys("Test comm adaugat");

            // Trimite formularul
            var addelement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("btnRegister")));
            new OpenQA.Selenium.Interactions.Actions(driver).MoveToElement(addelement).Click().Perform();

            wait.Until(d => d.Url.Contains("/Products/Show/1"));
            wait.Until(d => d.PageSource.Contains("Comentariu adaugat."));
            Assert.That(driver.PageSource, Does.Contain("Comentariu adaugat."));

            // Gaseste linkul de editare si editeaza comentariul
            var editLinks = wait.Until(d =>
            {
                var els = d.FindElements(By.PartialLinkText("Editeaza"));
                return els.Any() ? els : null;
            });

            var editLink = editLinks.Last();
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", editLink);
            Thread.Sleep(300);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", editLink);

            var contentBox = wait.Until(d => d.FindElement(By.Name("Content")));
            contentBox.Clear();
            contentBox.SendKeys("Comentariu modificat");
            driver.FindElement(By.CssSelector("form[action^='/Comments/Edit'] button[type='submit']")).Click();
            wait.Until(d => d.Url.Contains("/Products/Show/1"));
            Assert.That(driver.PageSource, Does.Contain("Comentariu editat."));

            // Sterge comentariul
            var deleteForm = driver.FindElements(By.CssSelector("form[action^='/Comments/Delete']")).FirstOrDefault();
            Assert.That(deleteForm, Is.Not.Null);
            deleteForm.Submit();
            wait.Until(d => d.Url.Contains("/Products/Show/1"));
            wait.Until(d => d.PageSource.Contains("Comentariu sters."));
            Assert.That(driver.PageSource, Does.Contain("Comentariu sters."));
        }
    }
}