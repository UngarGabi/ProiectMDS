using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace MDSTests
{
    public class CategoriesTests
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
        [TestCase("admin@test.com", "Admin1!", "Admin")]
        [TestCase("colaborator@test.com", "Colaborator1!", "Colaborator")]
        public void Test_Categories(string email, string password, string role)
        {
            // Login
            driver.Navigate().GoToUrl($"{appUrl}/Identity/Account/Login");
            wait.Until(d => d.FindElement(By.Id("Input_Email"))).SendKeys(email);
            driver.FindElement(By.Id("Input_Password")).SendKeys(password);
            driver.FindElement(By.Id("login-submit")).Click();

            if (role == "Admin")
            {
                // Index categorie
                driver.Navigate().GoToUrl($"{appUrl}/Categories");
                Assert.That(driver.Title, Does.Contain("Lista categorii"));

                // Creare categorie
                driver.Navigate().GoToUrl($"{appUrl}/Categories/New");
                wait.Until(d => d.Title.Contains("Adaugare Categorie"));
                Assert.That(driver.Url, Does.Contain("/Categories/New"));

                //Editare
                driver.Navigate().GoToUrl($"{appUrl}/Categories/Edit/1");
                wait.Until(d => d.Title.Contains("Editare categorie"));
                Assert.That(driver.Url, Does.Contain("/Categories/Edit/1"));

                //Afisare
                driver.Navigate().GoToUrl($"{appUrl}/Categories/Show/1");
                wait.Until(d => d.Title.Contains("Afisare categorie"));
                Assert.That(driver.Url, Does.Contain("/Categories/Show/1"));
            }
            else
            {
                // Verificare ca accesul e restrictionat
                driver.Navigate().GoToUrl($"{appUrl}/Categories");
                wait.Until(d => d.Url.Contains("/Account/AccessDenied"));
                Assert.That(driver.Url.Contains("/Account/AccessDenied"));
            }
        }
    }
}