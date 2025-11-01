using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Areas.Identity.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailSender emailSender;

        public RegisterController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, IEmailSender _emailSender)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            emailSender = _emailSender;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM register)
        {
            if (!ModelState.IsValid)
                return View(register);
            Random random = new Random();
            var user = new ApplicationUser
            {
                UserName = $"{register.FirstName.ToLower().TrimEnd()}{register.LastName.ToLower()}{random.Next(0,11)}",
                Email = register.EmailAddress,
                FirstName = register.FirstName,
                LastName = register.LastName
            };
            var result = await userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
            {
                StringBuilder error = new StringBuilder();
                foreach (var item in result.Errors)
                {
                    error.AppendLine(item.Description);
                }
                TempData["error-notification"] = error.ToString();
                return View(register);
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(EmailConfirmation), "Register", new { area = "Identity", token, userId = user.Id }, Request.Scheme);
            await emailSender.SendEmailAsync(register.EmailAddress, "ECommerce Confirm Email", $"<h1> To Confirm Your Email Click <a href='{link}'>Here</a></h1>");
            return View(register);
        }
        public async Task<IActionResult> EmailConfirmation(string token, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            //هنا نبق نعمل اتشيك لو اليوزر مش موجود ولا لا ونطبع توستر
            if (user is null)
            {
                //طباعة توستر
                TempData["error-notification"] = "Invalid User";
                return RedirectToAction(nameof(Register), new { user!.FirstName, user.LastName, user.Email });
            }
            var result = await userManager.ConfirmEmailAsync(user, token);
            //هنا نبق نعمل اتشيك لو الايميل كونفرمد ولا لا ونطبع توستر
            if (!result.Succeeded)
            {
                //طباعة توستر
                TempData["error-notification"] = "Email Confirmation Failed";
                return RedirectToAction(nameof(Register), new { user.FirstName, user.LastName, user.Email });
            }
            TempData["success-notification"] = "Email Confirmed Successfully";
            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM login)
        {
            var user = await userManager.FindByEmailAsync(login.Email);
            if (user is null)
            {
                TempData["error-notification"] = "Invalid Your Email Or Password";
                return View(login);
            }
            var checkPass = await signInManager.PasswordSignInAsync(user, login.Password, true, true);
            if (!checkPass.Succeeded)
            {
                if (checkPass.IsLockedOut)
                    TempData["error-notification"] = "Invalid Your Email Or Password";
                else
                    TempData["error-notification"] = "Invalid Your Email Or Password";
            }
            TempData["success-notification"] = "Login Successfully";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }
}
