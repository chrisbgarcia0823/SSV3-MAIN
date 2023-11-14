using Microsoft.AspNetCore.Mvc;

namespace ShopSuppliesV3.Controllers
{
    public class Misc : Controller
    {
        public IActionResult ContactUs()
        {
            return View();
        }
    }
}
