using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace MDSTests
{
    public class FavoriteTests
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
        public void Test_Favorite(string email, string password, string role)
        {
            // Login
            driver.Navigate().GoToUrl($"{appUrl}/Identity/Account/Login");
            wait.Until(d => d.FindElement(By.Id("Input_Email"))).SendKeys(email);
            driver.FindElement(By.Id("Input_Password")).SendKeys(password);
            driver.FindElement(By.Id("login-submit")).Click();

            if (role == "User")
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.LinkText("Afisare produse")));

                // Mergem la pagina de produse
                driver.Navigate().GoToUrl($"{appUrl}/Products");

                // Adauga produs la favorite (produs cu ID 1)
                driver.Navigate().GoToUrl($"{appUrl}/Favorite/AddToFavorites/1");

                // Verificam redirect la ViewFavorite si mesajul
                wait.Until(d => d.Url.Contains("/Favorite/ViewFavorite"));
                Assert.That(driver.PageSource, Does.Contain("a fost adaugat la favorite"));

                // Verificam ca produsul apare în favorite
                Assert.That(driver.PageSource, Does.Contain("Adaugă în coș"));
                Assert.That(driver.PageSource, Does.Contain("Șterge"));

                // Stergem produsul din favorite
                driver.Navigate().GoToUrl($"{appUrl}/Favorite/RemoveFromFavorites/1");

                // Verificam mesajul de stergere
                Assert.That(driver.PageSource, Does.Contain("a fost eliminat din favorite"));
            }
            else
            {
                // Incearca acces la /Favorite/ViewFavorite
                driver.Navigate().GoToUrl($"{appUrl}/Favorite/ViewFavorite");
                wait.Until(d => d.Url.Contains("/Account/AccessDenied"));
                // Redirectionat la AccessDenied
                Assert.That(driver.Url.Contains("/Account/AccessDenied"));
            }
        }
    }
}