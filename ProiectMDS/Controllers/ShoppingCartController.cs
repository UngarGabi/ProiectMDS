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
        private readonly Dictionary<string, decimal> _validPromoCodes = new()
        {
            { "REDUCERE10", 0.10m }, // 10% reducere
            { "STUDENT25", 0.25m }   // 25% reducere
        };


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
            var total = cartItems.Sum(item => item.Product.Price * item.Quantity);
            var promoCode = HttpContext.Session.Get<string>("PromoCode");
            decimal discount = 0;

            if (!string.IsNullOrEmpty(promoCode))
            {
                var validCodes = new Dictionary<string, decimal>
        {
            { "REDUCERE10", 0.10m },
            { "STUDENT25", 0.25m }
        };

                if (validCodes.TryGetValue(promoCode.ToUpper(), out var rate))
                {
                    discount = total * rate;
                }
            }

            var cartViewModel = new ShoppingCartViewModel
            {
                CartItems = cartItems,
                TotalPrice = total, 
                DiscountValue = discount,
                PromoCode = promoCode
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
        [HttpPost]
        [HttpPost]
        public IActionResult ApplyPromoCode(string promoCode)
        {
            if (string.IsNullOrEmpty(promoCode))
            {
                TempData["CartWarning"] = "Te rugam sa introduci un cod promotional.";
                return RedirectToAction("ViewCart");
            }

            // Nu permite aplicarea unui nou cod daca deja exista unul activ
            if (HttpContext.Session.Get<string>("PromoCode") != null)
            {
                TempData["CartWarning"] = "Ai deja un cod promotional activ.";
                return RedirectToAction("ViewCart");
            }

            // Validare cod
            var validCodes = new Dictionary<string, decimal>
            {
                { "REDUCERE10", 0.10m },
                { "STUDENT25", 0.25m }
            };

            if (!validCodes.TryGetValue(promoCode.ToUpper(), out var discountRate))
            {
                TempData["CartWarning"] = "Cod promotional invalid.";
                return RedirectToAction("ViewCart");
            }
            // Salveaza în sesiune
            HttpContext.Session.Set("PromoCode", promoCode.ToUpper());

            TempData["CartMessage"] = $"Codul '{promoCode}' a fost aplicat.";
            return RedirectToAction("ViewCart");
        }
        public IActionResult RemovePromoCode()
        {
            HttpContext.Session.Remove("PromoCode");
            HttpContext.Session.Remove("DiscountValue");
            TempData["CartMessage"] = "Codul promotional a fost eliminat.";
            return RedirectToAction("ViewCart");
        }

    }
}
