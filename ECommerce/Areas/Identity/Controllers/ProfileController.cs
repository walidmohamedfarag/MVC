using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Areas.Identity.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public ProfileController(UserManager<ApplicationUser> _userManager , SignInManager<ApplicationUser> _signInManager)
        {
            userManager = _userManager;
            signInManager = _signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var user =await userManager.GetUserAsync(User);
            var userVM = user.Adapt<UpdateProfileVM>();
            return View(userVM);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileVM updateProfileVM)
        {
            var user = await userManager.GetUserAsync(User);
            user!.FirstName = updateProfileVM.FullName.Split(' ')[0];
            user.LastName = updateProfileVM.FullName.Split(' ')[1];
            user.PhoneNumber = updateProfileVM.PhoneNumber;
            user.Address = updateProfileVM.Address;
            await userManager.UpdateAsync(user);
            TempData["success-notification"] = "Profile Updated Successfully";
            return RedirectToAction(nameof(UpdateProfile));
        }
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdateProfileVM updateProfileVM)
        {
            var user = await userManager.GetUserAsync(User);
            if(updateProfileVM.CurrentPassword is null || updateProfileVM.NewPassword is null)
            {
                TempData["error-notification"] = "You Must Enter The Current and New Passwords";
                return RedirectToAction(nameof(UpdateProfile),updateProfileVM);
            }
            var isCorrect = await userManager.CheckPasswordAsync(user!, updateProfileVM.CurrentPassword!);
            if(!isCorrect)
            {
                TempData["error-notification"] = "Current Password is Incorrect";
                return RedirectToAction(nameof(UpdateProfile), updateProfileVM);
            }
            var result = await userManager.ChangePasswordAsync(user!, updateProfileVM.CurrentPassword!, updateProfileVM.NewPassword!);
            if(!result.Succeeded)
            {
                StringBuilder builder = new();
                foreach (var error in result.Errors)
                   builder.AppendLine(error.Description);
                TempData["error-notification"] = builder.ToString();
                return RedirectToAction(nameof(UpdateProfile), updateProfileVM);
            }
            TempData["success-notification"] = "Password Updated Successfully";
            return RedirectToAction("Login" , "Register", new {area = "Identity"});
        }
        public async Task<IActionResult> Logout()
        {
            var user = await userManager.GetUserAsync(User);
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Register", new { area = "Identity" });
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
