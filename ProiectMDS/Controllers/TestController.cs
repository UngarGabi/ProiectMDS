using Microsoft.AspNetCore.Mvc;

namespace ProiectMDS.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return Content("Pagina de test funcționează!");
        }
    }
}
