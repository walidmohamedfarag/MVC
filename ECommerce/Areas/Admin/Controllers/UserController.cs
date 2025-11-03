using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{StaticRole.SUPER_ADMIN} , {StaticRole.ADMIN}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserController(UserManager<ApplicationUser> _userManager)
        {
            userManager = _userManager;
        }

        public IActionResult Index()
        {
            return View(userManager.Users);
        }
        public async Task<IActionResult> LockUnLock(string id) 
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["error-notification"] = "User Not Found";
                return RedirectToAction(nameof(Index));
            }
            if(await userManager.IsInRoleAsync(user,StaticRole.SUPER_ADMIN))
            {
                TempData["error-notification"] = "You Can Not Lock Super Admin User";
                return RedirectToAction(nameof(Index));
            }
            user.LockoutEnabled = !user.LockoutEnabled;
            if (!user.LockoutEnabled)
            {
                user.LockoutEnd = DateTime.Now.AddDays(15);
                TempData["success-notification"] = $"User: {user.FirstName} {user.LastName} Locked Successfully";
            }
            else
            {
                user.LockoutEnd = null;
                TempData["success-notification"] = $"User: {user.FirstName} {user.LastName} UnLocked Successfully";
            }
            await userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
