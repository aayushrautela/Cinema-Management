using Microsoft.AspNetCore.Mvc;

namespace CinemaTicketSystemCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

