using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Areas.Admin.Controllers
{
    public class BrandController : Controller
    {
        ApplicationDBContext context = new();
        public IActionResult Index()
        {
            var brands = context.Brands.ToList();
            return View(brands);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Brand brand, IFormFile img)
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
            context.Brands.Add(brand);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var brand = context.Brands.FirstOrDefault(b => b.Id == id);
            if (brand is null)
                return View("NotFoundPage", "Home");
            return View(brand);
        }
        [HttpPost]
        public IActionResult Edit(Brand brand,IFormFile? img)
        {
            var brandInDb = context.Brands.AsNoTracking().FirstOrDefault(b => b.Id == brand.Id);
            if(brandInDb is null)
                return View("NotFoundPage", "Home");
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
            context.Update(brand);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var brand = context.Brands.FirstOrDefault(b => b.Id == id);
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\Admin\BrandImg", brand.Image);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            context.Remove(brand);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
