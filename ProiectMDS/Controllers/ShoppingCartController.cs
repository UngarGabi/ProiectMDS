using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using ProiectMDS.Data;
using ProiectMDS.Models;

namespace ProiectMDS.Controllers
{
    [Authorize(Roles = "User")]
    public class ShoppingCartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private List<ShoppingCartItem> _cartItems;

        public ShoppingCartsController(ApplicationDbContext context)
        {
            _context = context;
            _cartItems = new List<ShoppingCartItem>();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddToCart(int id)
        {
            var productToAdd = _context.Products.Find(id);
            if (productToAdd == null || productToAdd.Stock <= 0)
            {
                TempData["CartError"] = $"Produsul {productToAdd?.Title} nu mai este disponibil!";
                return RedirectToAction("ViewCart");
            }
            var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();

            var existingCartItem = cartItems.FirstOrDefault(item => item.Product.Id == id);
            if (existingCartItem != null)
            {
                if (existingCartItem.Quantity + 1 > productToAdd.Stock)
                {
                    TempData["CartWarning"] = $"Nu exista suficient stoc pentru {productToAdd.Title}. Disponibil: {productToAdd.Stock}.";
                }
                else
                {
                    existingCartItem.Quantity++;
                    //TempData["CartMessage"] = $"{productToAdd.Title} a fost adaugat in cos!";
                }
            }
            else
            {
                if (productToAdd.Stock < 1)
                {
                    TempData["CartWarning"] = $"Nu exista suficient stoc pentru {productToAdd.Title}. Disponibil: {productToAdd.Stock}.";
                }
                else
                {
                    cartItems.Add(new ShoppingCartItem
                    {
                        Product = productToAdd,
                        Quantity = 1
                    });
                    //TempData["CartMessage"] = $"{productToAdd.Title} a fost adaugat in cos!";
                }
            }
            HttpContext.Session.Set("Cart", cartItems);
            return RedirectToAction("ViewCart");
        }

        public IActionResult ViewCart()
        {
            var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();
            var cartViewModel = new ShoppingCartViewModel
            {
                CartItems = cartItems,
                TotalPrice = cartItems.Sum(item => item.Product.Price * item.Quantity)
            };
            ViewBag.CartMessage = TempData["CartMessage"];
            ViewBag.CartError = TempData["CartError"];
            ViewBag.CartWarning = TempData["CartWarning"];
            return View(cartViewModel);
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();
            var itemToRemove = cartItems.FirstOrDefault(item => item.Product.Id == id);
            TempData["CartMessage"] = $"{itemToRemove.Product.Title} a fost sters din cos!";
            if (itemToRemove != null)
            {
                if (itemToRemove.Quantity > 1)
                {
                    itemToRemove.Quantity--;
                }
                else
                {
                    cartItems.Remove(itemToRemove);
                }
            }
            HttpContext.Session.Set("Cart", cartItems);
            return RedirectToAction("ViewCart");
        }
        public IActionResult AddFromCart(int id)
        {
            var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();
            var itemToAdd = cartItems.FirstOrDefault(item => item.Product.Id == id);
            if (itemToAdd != null)
            {
                var product = _context.Products.Find(id);
                if (itemToAdd.Quantity + 1 > product.Stock)
                {
                    TempData["CartWarning"] = $"Nu exista suficient stoc pentru {product.Title}. Disponibil: {product.Stock}.";
                }
                else
                {
                    itemToAdd.Quantity++;
                    TempData["CartMessage"] = $"{product.Title} cantitatea a fost actualizata in cos!";
                }
            }
            HttpContext.Session.Set("Cart", cartItems);
            return RedirectToAction("ViewCart");
        }

        public IActionResult PurchaseItems()
        {
            var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();
            foreach (var item in cartItems)
            {
                _context.Purchases.Add(new Purchase
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    PurchaseDate = DateTime.Now,
                    Total = item.Product.Price * item.Quantity
                });
                var product = _context.Products.Find(item.Product.Id);
                product.Stock -= item.Quantity;
            }
            _context.SaveChanges();
            HttpContext.Session.Set("Cart", new List<ShoppingCartItem>());
            TempData["message"] = "Produsele au fost achizitionate cu succes!";
            return RedirectToAction("Index", "Home");

        }
    }
}
