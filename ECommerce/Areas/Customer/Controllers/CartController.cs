using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Areas.Customer.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRepositroy<Cart> repoCart;

        public CartController(UserManager<ApplicationUser> _userManager , IRepositroy<Cart> _repoCart)
        {
            userManager = _userManager;
            repoCart = _repoCart;
        }

        public IActionResult Cart()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Cart(int productId, int count , CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            var productInDB = await repoCart.GetOneAsync(p=>p.ProductId == productId , cancellationToken:cancellationToken);
            if(productInDB is not null)
            {
                productInDB.Count += count;
                repoCart.Update(productInDB);
                await repoCart.CommitAsync(cancellationToken);
                TempData["success-notification"] = "Count Of Product Is Updated Successfully";
                return RedirectToAction("Index", "Home");
            }
            await repoCart.AddAsync(new()
            {
                ProductId = productId,
                Count = count,
                UserId = user!.Id
            } , cancellationToken:cancellationToken);
            await repoCart.CommitAsync(cancellationToken);  
            return View();
        }
    }
}
