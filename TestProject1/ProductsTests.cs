using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace MDSTests
{
    public class ProductsTests
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

        [TestCase("admin@test.com", "Admin1!", "Admin")]
        [TestCase("colaborator@test.com", "Colaborator1!", "Colaborator")]
        [TestCase("user@test.com", "User1!", "User")]
        public void Test_AllProducts(string email, string password, string role)
        {
            // LOGIN
            driver.Navigate().GoToUrl($"{appUrl}/Identity/Account/Login");
            wait.Until(d => d.FindElement(By.Id("Input_Email"))).SendKeys(email);
            driver.FindElement(By.Id("Input_Password")).SendKeys(password);
            driver.FindElement(By.Id("login-submit")).Click();

            // INDEX
            driver.Navigate().GoToUrl($"{appUrl}/Products/Index");
            Assert.That(driver.PageSource.Contains("Afisare produse"));

            // SHOW (Produs ID 1)
            driver.Navigate().GoToUrl($"{appUrl}/Products/Show/1");
            Assert.That(driver.PageSource.Contains("Adauga in cos"));

            // NEW
            if (role == "Admin" || role == "Colaborator")
            {
                driver.Navigate().GoToUrl($"{appUrl}/Products/New");
                Assert.That(driver.Url.Contains("/Products/New"));
            }
            else
            {
                driver.Navigate().GoToUrl($"{appUrl}/Products/New");
                Assert.That(driver.Url.Contains("/Account/AccessDenied"));
            }

            // EDIT
            driver.Navigate().GoToUrl($"{appUrl}/Products/Edit/1");

            if (role == "Admin")
            {
                // admin poate edita orice produs
                Assert.That(driver.PageSource.Contains("Editare produs"));
            }
            else if (role == "Colaborator")
            {
                // colaboratorul NU poate edita produsul 1
                Assert.That(driver.Url.Contains("/Products"));
            }
            else
            {
                // userul nu poate edita
                Assert.That(driver.Url.Contains("/Account/AccessDenied"));
            }

            // DELETE
            // Nu apelam DELETE direct, doar verificam dacă butonul exista (conform drepturilor)
            driver.Navigate().GoToUrl($"{appUrl}/Products/Show/1");

            if (role == "Admin")
            {
                Assert.That(driver.PageSource.Contains("Sterge produs"));
            }
            else if (role == "Colaborator")
            {
                Assert.That(driver.PageSource.Contains("Sterge produs") == false); // nu e al lui
            }
            else
            {
                Assert.That(driver.PageSource.Contains("Sterge produs") == false);
            }
        }
    }
}