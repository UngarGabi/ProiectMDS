using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using ProiectMDS.Data;
using ProiectMDS.Models;

namespace ProiectMDS.Controllers
{
    [Authorize(Roles = "User")]
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private List<FavoriteItem> _favItems;

        public FavoriteController(ApplicationDbContext context)
        {
            _context = context;
            _favItems = new List<FavoriteItem>();
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult AddToFavorites(int id)
        {
            var productToAdd = _context.Products.Find(id);
            var favorites = HttpContext.Session.Get<List<FavoriteItem>>("Favorites") ?? new List<FavoriteItem>();

            if (productToAdd == null || productToAdd.Stock <= 0)
            {
                TempData["FavError"] = $"Produsul {productToAdd?.Title} nu mai este disponibil!";
                return RedirectToAction("ViewFavorite");
            }

            var favItems = HttpContext.Session.Get<List<FavoriteItem>>("Favorites") ?? new List<FavoriteItem>();

            if (favItems.Any(f => f.Product.Id == id))
            {
                TempData["FavWarning"] = "Produsul este deja adaugat in favorite.";

            }
            else
            {
                favorites.Add(new FavoriteItem
                {
                    Product = productToAdd
                });

                TempData["FavMessage"] = $"Produsul '{productToAdd.Title}' a fost adaugat la favorite. ";
            }
            HttpContext.Session.Set("Favorites", favorites);
            return RedirectToAction("ViewFavorite");

        }

        public IActionResult ViewFavorite()
        {
            var favItems = HttpContext.Session.Get<List<FavoriteItem>>("Favorites") ?? new List<FavoriteItem>();

            var favViewModel = new FavoriteViewModel
            {
                FavItems = favItems
            };

            ViewBag.FavMessage = TempData["FavMessage"];
            ViewBag.FavError = TempData["FavError"];
            ViewBag.FavWarning = TempData["FavWarning"];

            return View(favViewModel);
        }

        public IActionResult RemoveFromFavorites(int id)
        {
            var favItems = HttpContext.Session.Get<List<FavoriteItem>>("Favorites") ?? new List<FavoriteItem>();

            var itemToRemove = favItems.FirstOrDefault(f => f.Product.Id == id);

            if (itemToRemove != null)
            {
                favItems.Remove(itemToRemove);
                TempData["FavMessage"] = $"Produsul '{itemToRemove.Product.Title}' a fost eliminat din favorite. ";

            }
            else
            {
                TempData["FavWarning"] = "Produsul nu a fost gasit";
            }

            HttpContext.Session.Set("Favorites", favItems);
            return RedirectToAction("ViewFavorite");

        }
    }


}
