using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using ProiectMDS.Data;
using ProiectMDS.Models;
using System.Net.NetworkInformation;
using System.IO;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Data.SqlClient;

namespace ProiectMDS.Controllers
{
    public class ProductsController : Controller
    {
        // useri si roluri
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public ProductsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        //index 
        public IActionResult Index(string sortOrder, string search)
        {
            // Setari pentru sortare
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParm = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewBag.RatingSortParm = sortOrder == "rating_asc" ? "rating_desc" : "rating_asc";

            // Setarea cautarii
            ViewBag.CurrentFilter = search;

            // Preluare produse aprobate
            var products = db.Products.Include("Category")
                                      .Include("User")
                                      .Where(p => p.IsApproved == true);

            if (!String.IsNullOrEmpty(search))
            {
                List<int> productIds = db.Products.Where(
                                         at => at.Title.Contains(search)
                                         || at.Description.Contains(search)
                                     ).Select(a => a.Id).ToList();

                List<int> productIdsOfCommentsWithSearchString = db.Comments
                                         .Where(c => c.Content.Contains(search))
                                         .Select(c => (int)c.ProductId).ToList();

                List<int> mergedIds = productIds.Union(productIdsOfCommentsWithSearchString).ToList();

                products = products.Where(product => mergedIds.Contains(product.Id));
            }

            // Calculare scoruri produse
            var productRatings = db.Comments
                .Where(c => c.Rating.HasValue && c.ProductId.HasValue)
                .GroupBy(c => c.ProductId.Value)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AverageRating = g.Average(c => c.Rating)
                })
                .ToDictionary(g => g.ProductId, g => g.AverageRating);

            // Creare lista de ViewModel
            var productCommentViewModels = products.ToList().Select(p => new ProductCommentViewModel
            {
                Title = p.Title,
                Product = p,
                ProductId = p.Id,
                Rating = productRatings.ContainsKey(p.Id) ? productRatings[p.Id] : 0,
                ListOfComments = db.Comments.Where(c => c.ProductId == p.Id).ToList()
            });

            // Sortare dupa sortOrder
            switch (sortOrder)
            {
                case "rating_desc":
                    productCommentViewModels = productCommentViewModels.OrderByDescending(p => p.Rating);
                    break;
                case "rating_asc":
                    productCommentViewModels = productCommentViewModels.OrderBy(p => p.Rating);
                    break;
                case "price_desc":
                    productCommentViewModels = productCommentViewModels.OrderByDescending(p => p.Product.Price);
                    break;
                case "price_asc":
                    productCommentViewModels = productCommentViewModels.OrderBy(p => p.Product.Price);
                    break;
                default:
                    productCommentViewModels = productCommentViewModels.OrderBy(p => p.Title); // Ordine implicita
                    break;
            }
            // Paginare
            int _perPage = 3;
            int totalItems = productCommentViewModels.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = (currentPage > 0) ? (currentPage - 1) * _perPage : 0;
            var paginatedProducts = productCommentViewModels.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Products = paginatedProducts.Select(p => p.Product).ToList();
            ViewBag.Ratings = paginatedProducts.ToDictionary(p => p.Product.Id, p => p.Rating);

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = $"/Products/Index/?search={search}&sortOrder={sortOrder}&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = $"/Products/Index/?sortOrder={sortOrder}&page";
            }
            //return View(productCommentViewModels.ToList());
            return View(paginatedProducts.ToList());
        }





        // Se afiseaza un singur produs in functie de id-ul sau impreuna cu categoria din care face parte
        // In plus, sunt preluate si toate comentariile asociate unui produs
        // Se afiseaza si userul care a postat produsul respectiv
        //[Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show(int id)
        {
            Product product = db.Products.Include("Category").Include("Comments").Include("User")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == id)
                                         .First();
            ProductCommentViewModel vm = new ProductCommentViewModel();

            if (product == null)
            {
                // cazul in care produsul nu e gasit
                return NotFound();
            }
            vm.ProductId = product.Id;
            vm.Product = product;
            vm.Title = product.Title;

            var comments = db.Comments.Include("User")
                                      .Where(c => c.ProductId == id)
                                      .ToList();
            vm.ListOfComments = comments;

            var ratings = db.Comments
                .Where(c => c.ProductId == id)
                .Select(c => c.Rating)
                .ToList();

            if (ratings.Count > 0)
            {
                var ratingSum = ratings.Sum();
                ViewBag.RatingSum = ratingSum;
                var ratingCount = ratings.Count();
                ViewBag.RatingCount = ratingCount;
                ViewBag.AverageRating = ratingSum / ratingCount;
            }
            else
            {
                ViewBag.RatingSum = 0;
                ViewBag.RatingCount = 0;
            }

            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(vm);
        }

        // Adaugarea unui comentariu asociat unui produs in baza de date
        // Toate rolurile pot adauga comentarii in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            // preluam Id-ul utilizatorului care posteaza comentariul
            comment.UserId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Products/Show/" + comment.ProductId);
            }
            else
            {
                Product art = db.Products.Include("Category").Include("User").Include("Comments")
                               .Include("Comments.User")
                               .Where(art => art.Id == comment.ProductId)
                               .First();

                SetAccessRights();
                return View(art);
            }
        }


        // Se afiseaza formularul in care se vor completa datele unui produs impreuna cu selectarea categoriei din care face parte
        // Doar utilizatorii cu rolul de colaborator si admin pot adauga produse in platforma
        // [HttpGet] implicit
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New()
        {
            Product product = new Product();

            product.Categ = GetAllCategories();

            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New(Product product)
        {
            var sanitizer = new HtmlSanitizer();
            product.Categ = GetAllCategories();
            // preluam Id-ul utilizatorului care posteaza produsul
            product.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                product.Description = sanitizer.Sanitize(product.Description);
                if (product.UserId == "8e445865-a24d-4543-a6c6-9443d048cdb0") // este admin
                {
                    product.IsApproved = true;
                    db.Products.Add(product);
                    db.SaveChanges();

                    var ProductId = product.Id;
                    string wwwRootPath = _env.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var productFromDb = db.Products.Find(ProductId);
                    if (files.Count != 0)
                    {
                        /* var ImagePath = @"images\products";
                         var Extension = Path.GetExtension(files[0].FileName);
                         var RelativeImagePath = ImagePath + ProductId + Extension;
                         var AbsImagePath = Path.Combine(wwwRootPath, RelativeImagePath);
                         using (var fileStream = new FileStream(AbsImagePath, FileMode.Create))
                         {
                             files[0].CopyTo(fileStream);
                         }
                         productFromDb.ImageUrl = RelativeImagePath;
                         db.SaveChanges();*/
                        var ImagePath = @"images/products/";
                        var Extension = Path.GetExtension(files[0].FileName);
                        var RelativeImagePath = Path.Combine(ImagePath, $"{ProductId}{Extension}");
                        var AbsImagePath = Path.Combine(wwwRootPath, RelativeImagePath);

                        // Creeaza directorul daca nu exista
                        if (!Directory.Exists(Path.Combine(wwwRootPath, ImagePath)))
                        {
                            Directory.CreateDirectory(Path.Combine(wwwRootPath, ImagePath));
                        }

                        using (var fileStream = new FileStream(AbsImagePath, FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        productFromDb.ImageUrl = "/" + RelativeImagePath.Replace("\\", "/"); // Normalizează pentru URL
                        db.SaveChanges();

                    }
                    TempData["message"] = "Produsul a fost adaugat";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else // este colaborator
                {
                    product.IsApproved = false;
                    db.Products.Add(product);
                    db.SaveChanges();
                    /*
                    var ProductId = product.Id;
                    string wwwRootPath = _env.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var productFromDb = db.Products.Find(ProductId);
                    if (files.Count != 0)
                    {
                        var ImagePath = @"images\";
                        var Extension = Path.GetExtension(files[0].FileName);
                        var RelativeImagePath = ImagePath + ProductId + Extension;
                        var AbsImagePath = Path.Combine(wwwRootPath, RelativeImagePath);
                        using (var fileStream = new FileStream(AbsImagePath, FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productFromDb.ImageUrl = RelativeImagePath;
                        db.SaveChanges();
                    }*/
                    var ProductId = product.Id;
                    string wwwRootPath = _env.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var productFromDb = db.Products.Find(ProductId);
                    if (files.Count != 0)
                    {
                        /*var ImagePath = @"images\products";
                        var Extension = Path.GetExtension(files[0].FileName);
                        var RelativeImagePath = ImagePath + ProductId + Extension;
                        var AbsImagePath = Path.Combine(wwwRootPath, RelativeImagePath);
                        using (var fileStream = new FileStream(AbsImagePath, FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productFromDb.ImageUrl = RelativeImagePath;
                        db.SaveChanges();*/
                        var ImagePath = @"images/products/";
                        var Extension = Path.GetExtension(files[0].FileName);
                        var RelativeImagePath = Path.Combine(ImagePath, $"{ProductId}{Extension}");
                        var AbsImagePath = Path.Combine(wwwRootPath, RelativeImagePath);

                        // Creează directorul dacă nu există
                        if (!Directory.Exists(Path.Combine(wwwRootPath, ImagePath)))
                        {
                            Directory.CreateDirectory(Path.Combine(wwwRootPath, ImagePath));
                        }

                        using (var fileStream = new FileStream(AbsImagePath, FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        productFromDb.ImageUrl = "/" + RelativeImagePath.Replace("\\", "/"); // Normalizează pentru URL
                        db.SaveChanges();

                    }
                    var request = new Request
                    {
                        ProductId = product.Id,
                        RequestType = "ADD",
                        RequestedByUserId = _userManager.GetUserId(User)
                    };

                    db.Requests.Add(request);
                    db.SaveChanges();

                    TempData["message"] = "Produsul a fost trimis pentru aprobare.";
                    TempData["messageType"] = "alert-success";

                    return RedirectToAction("Index");
                }
            }
            else
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
        }













        // Se editeaza un produs existent in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // Se afiseaza formularul impreuna cu datele aferente produsului din baza de date
        // Doar utilizatorii cu rolul de colaborator si admin pot edita produsele
        // Adminii pot edita orice produs din baza de date
        // Colaboratorii pot edita doar produsele proprii
        // HttpGet implicit

        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                         .Where(art => art.Id == id)
                                         .First();

            product.Categ = GetAllCategories();

            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {
                product.Categ = GetAllCategories();
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // Se adauga produsul modificat in baza de date
        // Se verifica rolul utilizatorilor care au dreptul sa editeze: colaborator si admin
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id, Product requestProduct)
        {
            var sanitizer = new HtmlSanitizer();
            Product product = db.Products.Find(id);
            product.Categ = GetAllCategories();

            if (ModelState.IsValid)
            {
                if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    product.Title = requestProduct.Title;
                    requestProduct.Description = sanitizer.Sanitize(requestProduct.Description);
                    product.Description = requestProduct.Description;
                    product.Price = requestProduct.Price;
                    product.Stock = requestProduct.Stock;
                    product.CategoryId = requestProduct.CategoryId;
                    if (User.IsInRole("Admin"))
                    {
                        product.IsApproved = true;
                        db.SaveChanges();
                        TempData["message"] = "Produsul a fost modificat";
                        TempData["messageType"] = "alert-success";
                        return RedirectToAction("Index");
                    }
                    else
                        product.IsApproved = false;
                    db.SaveChanges();
                    var request = new Request
                    {
                        ProductId = product.Id,
                        RequestType = "EDIT",
                        RequestedByUserId = _userManager.GetUserId(User) ?? string.Empty
                    };

                    db.Requests.Add(request);
                    db.SaveChanges();
                    TempData["message"] = "Produsul a fost trimis pentru aprobare.";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    product.Categ = GetAllCategories();
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }

        // Se sterge un produs din baza de date
        //Doar utilizatorii cu rolul de colaborator si admin pot sterge produse
        // Adminii pot sterge orice produs din baza de date
        // Colaboratorii pot sterge doar produsele propri
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public ActionResult Delete(int id)
        {
            //Product product = db.Products.Find(id);

            Product product = db.Products.Include("Comments")
                                         .Where(art => art.Id == id)
                                         .First();

            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                var comments = db.Comments.Where(c => c.ProductId == id);
                db.Comments.RemoveRange(comments);//stergem comentariile si ratingul mai intai
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        //Conditiile de afisare pentru butoanele de editare si stergere
        //butoanele aflate in view-uri
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Colaborator"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName;

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        // Metoda utilizata pentru exemplificarea Layout-ului
        // Am adaugat un nou Layout in Views -> Shared -> numit _LayoutNou.cshtml
        // Aceasta metoda are un View asociat care utilizeaza noul layout creat
        // in locul celui default generat de framework numit _Layout.cshtml
        public IActionResult IndexNou()
        {
            return View();
        }
    }
}
