using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{StaticRole.SUPER_ADMIN} , {StaticRole.ADMIN}")]
    public class PromotionController : Controller
    {
        private readonly IRepositroy<Promotion> repoPromotion;

        private readonly IRepositroy<Product> repoProduct;

        public PromotionController(IRepositroy<Promotion> _repoPromotion , IRepositroy<Product> _repoProduct)
        {
            repoPromotion = _repoPromotion;
            repoProduct = _repoProduct;
        }

        public async Task<IActionResult> ShowPromotion(CancellationToken cancellationToken)
        {
            var promotions =await repoPromotion.GetAsync(tracked: false , includes: [p=>p.Product]);
            foreach (var promotion in promotions)
            {
                if (promotion.ValidTo < DateTime.Now)
                {
                    promotion.IsValid = false;
                    repoPromotion.Update(promotion);
                }
            }
            await repoPromotion.CommitAsync(cancellationToken);
            return View(promotions);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.products = await repoProduct.GetAsync(tracked: false);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Promotion promotion , CancellationToken cancellationToken)
        {
            await repoPromotion.AddAsync(promotion , cancellationToken);
            await repoPromotion.CommitAsync(cancellationToken);
            TempData["success-notification"] = "Add Promotion Successfully";
            return RedirectToAction(nameof(ShowPromotion));
        }
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await repoPromotion.GetOneAsync(p=>p.Id == id);
            ViewBag.products = await repoProduct.GetAsync(tracked: false);
            if (promotion is null)
            {
                TempData["error-notification"] = "Promotion Not Found";
                return RedirectToAction(nameof(ShowPromotion));
            }
            return View(promotion);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id , Promotion ePromotion , CancellationToken cancellationToken)
        {
            repoPromotion.Update(ePromotion);
            await repoPromotion.CommitAsync(cancellationToken);
            TempData["success-notification"] = "Edit Promotion Successfully";
            return RedirectToAction(nameof(ShowPromotion));
        }
        public async Task<IActionResult> Delete(int id , CancellationToken cancellationToken)
        {
            var promotion = await repoPromotion.GetOneAsync(p=>p.Id == id , cancellationToken:cancellationToken);
            if (promotion is null)
            {
                TempData["error-notification"] = "Promotion Not Found";
                return RedirectToAction(nameof(ShowPromotion));
            }
            repoPromotion.Delete(promotion);
            await repoPromotion.CommitAsync(cancellationToken);
            TempData["success-notification"] = "Delete Promotion Successfully";
            return RedirectToAction(nameof(ShowPromotion));
        }
    }
}
