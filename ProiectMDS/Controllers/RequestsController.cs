using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectMDS.Models;
using ProiectMDS.Data;

namespace ProiectMDS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext db;

        public RequestsController(ApplicationDbContext context)
        {
            db = context;
        }

        // Vizualizarea cererilor de aprobare
        public IActionResult Index()
        {
            var requests = db.Requests.Include(r => r.Product)
                                       .ThenInclude(p => p.Category)
                                    .Include(r => r.RequestedByUser)
                                    .ToList();

            return View(requests);
        }

        // Aproba cererea
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var request = db.Requests.Find(id);
            if (request == null)
            {
                TempData["message"] = "Cerere negasita.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var product = db.Products.Find(request.ProductId);
            if (product == null)
            {
                TempData["message"] = "Produs negasit.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            product.IsApproved = true;
            request.Status = "Approved";
            RemoveRequest(id);
            db.SaveChanges();

            TempData["message"] = "Cerere aprobata.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index");
        }

        // Respinge cererea
        [HttpPost]
        public IActionResult Reject(int id)
        {
            var request = db.Requests.Find(id);
            if (request == null)
            {
                TempData["message"] = "Cerere negasita.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            request.Status = "Rejected";
            RemoveRequest(id);
            db.SaveChanges();

            TempData["message"] = "Cerere respinsa.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index");
        }

        private void RemoveRequest(int id)
        {
            var request = db.Requests.Find(id);
            db.Requests.Remove(request);
            db.SaveChanges();
        }
    }
}
