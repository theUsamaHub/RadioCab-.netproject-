using Microsoft.AspNetCore.Mvc;

namespace RadioCab.Controllers
{
    public class AdvertisementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
