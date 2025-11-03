
namespace ECommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{StaticRole.SUPER_ADMIN},{StaticRole.ADMIN}")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
      
    }
}
