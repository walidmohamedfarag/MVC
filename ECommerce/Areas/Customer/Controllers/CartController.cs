
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

        public async Task<IActionResult> Cart()
        {
            var user = await userManager.GetUserAsync(User);
            var cartProducts = await repoCart.GetAsync(c => c.UserId == user!.Id, includes: [c => c.Product! , c=>c.Product.Brand , c => c.Product.Categroy], cancellationToken: CancellationToken.None);
            return View(cartProducts);
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
                return RedirectToAction(nameof(Cart));
            }
            await repoCart.AddAsync(new()
            {
                ProductId = productId,
                Count = count,
                UserId = user!.Id
            } , cancellationToken:cancellationToken);
            await repoCart.CommitAsync(cancellationToken);
            TempData["success-notification"] = "The product Add To Cart";
            return RedirectToAction(nameof(Cart));
        }
        public async Task<IActionResult> Increment(int productId , CancellationToken cancellationToken)
        {
            var user =await userManager.GetUserAsync(User);
            var productInDB = await repoCart.GetOneAsync(p => p.ProductId == productId && p.UserId == user!.Id);
            productInDB!.Count += 1;
            await repoCart.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Cart));
        }
        public async Task<IActionResult> Decrement(int productId , CancellationToken cancellationToken)
        {
            var user =await userManager.GetUserAsync(User);
            var productInDB = await repoCart.GetOneAsync(p => p.ProductId == productId && p.UserId == user!.Id);
            if(productInDB!.Count <= 1)
                return RedirectToAction(nameof(Delete) , new { productInDB.ProductId});
            productInDB!.Count -= 1;
            await repoCart.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Cart));
        }
        public async Task<IActionResult> Delete(int productId, CancellationToken cancellationToken)
        {
            var user =await userManager.GetUserAsync(User);
            var productInDB = await repoCart.GetOneAsync(p => p.ProductId == productId && p.UserId == user!.Id);
            repoCart.Delete(productInDB!);
            await repoCart.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Cart));
        }
    }
}
