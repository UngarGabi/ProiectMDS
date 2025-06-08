using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MDSTests
{
    public class ShoppingCartTests
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
        public void Test_ShoppingCart(string email, string password, string role)
        {
            // Login
            driver.Navigate().GoToUrl($"{appUrl}/Identity/Account/Login");
            wait.Until(d => d.FindElement(By.Id("Input_Email"))).SendKeys(email);
            driver.FindElement(By.Id("Input_Password")).SendKeys(password);
            driver.FindElement(By.Id("login-submit")).Click();

            Thread.Sleep(1000);

            // Incearca accesul la /ShoppingCarts/ViewCart
            driver.Navigate().GoToUrl($"{appUrl}/ShoppingCarts/ViewCart");

            if (role == "User")
            {
                Assert.That(driver.PageSource, Does.Contain("Cosul tau de cumparaturi"));

                // Incearca sa adaugi produsul 1 
                driver.Navigate().GoToUrl($"{appUrl}/ShoppingCarts/AddToCart/1");
                driver.Navigate().GoToUrl($"{appUrl}/ShoppingCarts/ViewCart");
                Assert.That(driver.PageSource, Does.Contain("Sterge"));

                // Aplica un cod promotional valid
                var promoInput = driver.FindElement(By.Id("promoCode"));
                promoInput.SendKeys("REDUCERE10");
                promoInput.Submit();
                Thread.Sleep(1000);
                Assert.That(driver.PageSource, Does.Contain("REDUCERE10"));

                // Elimina codul promotional
                driver.FindElement(By.ClassName("btn-outline-danger")).Click();
                Thread.Sleep(500);
                Assert.That(driver.PageSource, Does.Not.Contain("REDUCERE10"));

                // Finalizeaza comanda
                driver.FindElement(By.LinkText("Checkout")).Click();
                Assert.That(driver.PageSource, Does.Contain("Produsele au fost achizitionate cu succes!"));
            }
            else
            {
                Assert.That(driver.Url.Contains("/Account/AccessDenied"));
            }
        }
    }
}