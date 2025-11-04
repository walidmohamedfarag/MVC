using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    public class BrandController : Controller
    {
        private readonly IRepositroy<Brand> repo;

        public BrandController(IRepositroy<Brand> _repo)
        {
            repo = _repo;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var brands =await repo.GetAsync(tracked: false , cancellationToken: cancellationToken);
            return View(brands);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Brand brand, IFormFile img , CancellationToken cancellationToken)
        {
            if (img is not null && img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\Admin\BrandImg", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    img.CopyTo(stream);
                }
                brand.Image = fileName;
            }
            await repo.AddAsync(brand, cancellationToken: cancellationToken);
            await repo.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id , CancellationToken cancellationToken)
        {
            var brand =await repo.GetOneAsync(b => b.Id == id , cancellationToken:cancellationToken);
            if (brand is null)
                return View("NotFoundPage");
            return View(brand);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Brand brand,IFormFile? img, CancellationToken cancellationToken)
        {
            var brandInDb = await repo.GetOneAsync(b => b.Id == brand.Id, cancellationToken: cancellationToken , tracked:false);
            if(brandInDb is null)
                return View("NotFoundPage");
            if (img is not null && img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\Admin\BrandImg", fileName);
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\Admin\BrandImg", brandInDb.Image);
                using (var stream = System.IO.File.Create(filePath)) 
                {
                    img.CopyTo(stream); 
                }
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
                brand.Image = fileName;
            }
            else
                brand.Image = brandInDb.Image;
            repo.Update(brand);
            await repo.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id , CancellationToken cancellationToken)
        {
            var brand = await repo.GetOneAsync(b => b.Id == id);
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\Admin\BrandImg", brand.Image);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            repo.Delete(brand);
            await repo.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
