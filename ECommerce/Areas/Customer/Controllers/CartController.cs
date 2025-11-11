
using ECommerce.Models;
using System.Threading.Tasks;

namespace ECommerce.Areas.Customer.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRepositroy<Cart> repoCart;
        private readonly IRepositroy<Promotion> repoPromotion;
        private readonly IRepositroy<Product> repoProduct;

        public CartController(UserManager<ApplicationUser> _userManager, IRepositroy<Cart> _repoCart, IRepositroy<Promotion> _repoPromotion, IRepositroy<Product> _repoProduct)
        {
            userManager = _userManager;
            repoCart = _repoCart;
            repoPromotion = _repoPromotion;
            repoProduct = _repoProduct;
        }

        public async Task<IActionResult> Cart(string code, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            var cartProducts = await repoCart.GetAsync(c => c.UserId == user!.Id, includes: [c => c.Product!, c => c.Product.Brand, c => c.Product.Categroy, c => c.User], tracked: false, cancellationToken: cancellationToken);
            var promotion = await repoPromotion.GetOneAsync(p => p.Code == code && p.IsValid, cancellationToken: cancellationToken);
            if (promotion is not null)
            {
                var productInCart = cartProducts.FirstOrDefault(c => c.UserId == user!.Id && c.ProductId == promotion!.ProductId);

                if (productInCart is not null)
                {
                    productInCart.Price -= productInCart.Price * (promotion!.Discount / 100);
                    TempData["success-notification"] = "Promotion Applied Successfully";
                }
                await repoCart.CommitAsync(cancellationToken);
            }
            return View(cartProducts);
        }
        [HttpPost]
        public async Task<IActionResult> Cart(int productId, int count, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            var productInDB = await repoProduct.GetOneAsync(p => p.Id == productId, cancellationToken: cancellationToken);
            var productInCart = await repoCart.GetOneAsync(p => p.ProductId == productId, cancellationToken: cancellationToken);
            if (productInCart is not null)
            {
                productInCart.Count += count;
                productInCart.Price += (productInDB!.Price - (productInDB.Price * (productInDB.Discount / 100))) * count;
                repoCart.Update(productInCart);
                await repoCart.CommitAsync(cancellationToken);
                TempData["success-notification"] = "Count Of Product Is Updated Successfully";
                return RedirectToAction(nameof(Cart));
            }
            await repoCart.AddAsync(new()
            {
                ProductId = productId,
                Count = count,
                UserId = user!.Id,
                Price = await repoProduct.GetOneAsync(p => p.Id == productId, cancellationToken: cancellationToken) is Product product ? (product.Price - (product.Price * (product.Discount / 100))) * count : 0
            }, cancellationToken: cancellationToken);
            await repoCart.CommitAsync(cancellationToken);
            TempData["success-notification"] = "The product Add To Cart";
            return RedirectToAction(nameof(Cart));
        }
        public async Task<IActionResult> Increment(int productId, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            var productInDB = await repoProduct.GetOneAsync(p => p.Id == productId, cancellationToken: cancellationToken);
            var productInCart = await repoCart.GetOneAsync(p => p.ProductId == productId && p.UserId == user!.Id);
            productInCart!.Count += 1;
            productInCart.Price += productInDB!.Price - (productInDB.Price * (productInDB.Discount / 100));
            await repoCart.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Cart));
        }
        public async Task<IActionResult> Decrement(int productId, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            var productInDB = await repoProduct.GetOneAsync(p => p.Id == productId, cancellationToken: cancellationToken);
            var productInCart = await repoCart.GetOneAsync(p => p.ProductId == productId && p.UserId == user!.Id);
            if (productInCart!.Count <= 1)
                return RedirectToAction(nameof(Delete), new { productInCart.ProductId });
            productInCart!.Count -= 1;
            productInCart.Price -= productInDB!.Price - (productInDB.Price * (productInDB.Discount / 100));
            await repoCart.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Cart));
        }
        public async Task<IActionResult> Delete(int productId, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            var productInDB = await repoCart.GetOneAsync(p => p.ProductId == productId && p.UserId == user!.Id);
            repoCart.Delete(productInDB!);
            await repoCart.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Cart));
        }
    }
}
