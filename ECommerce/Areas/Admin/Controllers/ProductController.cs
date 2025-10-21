using ECommerce.Models;
using ECommerce.ModelVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        ApplicationDBContext context = new();
        public IActionResult Index(string name, decimal? minPrice, decimal? maxPrice, int? categoryId, int? brandId, bool? lessQuantity, int page = 1)
        {
            var product = context.Products.Include(p => p.Categroy).Include(p => p.Brand).AsQueryable();
            if (name is not null)
                product = product.Where(p => p.Name.Contains(name));
            if (minPrice is not null)
                product = product.Where(p => p.Price - (p.Price * (p.Discount / 100)) >= minPrice);
            if (maxPrice is not null)
                product = product.Where(p => p.Price - (p.Price * (p.Discount / 100)) <= maxPrice);
            if (categoryId is not null)
                product = product.Where(p => p.CategroyId == categoryId);
            if (brandId is not null)
                product = product.Where(p => p.BrandId == brandId);
            if (lessQuantity is not null)
                product = product.Where(p => p.Quantity <= 100);
            ViewBag.name = name;
            ViewBag.minPrice = minPrice;
            ViewBag.maxPrice = maxPrice;
            ViewBag.categoryId = categoryId;
            ViewBag.brandId = brandId;
            ViewBag.lessQuantity = lessQuantity;
            ViewBag.categories = context.Categroys;
            ViewData["brands"] = context.Brands;
            ViewBag.totalPages = Math.Ceiling(context.Products.Count() / 8.0);
            ViewBag.currentPage = page;
            product = product.Skip((page - 1) * 8).Take(8);
            return View(product);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var brands = context.Brands;
            var categories = context.Categroys;
            return View(new ProductVM
            {
                brands = brands,
                Categroys = categories
            });
        }
        [HttpPost]
        public IActionResult Create(Product product, IFormFile img, List<IFormFile> subImgs, string[] colors)
        {
            #region add product with main image
            if (img is not null && img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    img.CopyTo(stream);
                }
                product.MainImage = fileName;
            }
            context.Products.Add(product);
            context.SaveChanges();
            #endregion

            #region add sub imges
            if (subImgs is not null && subImgs.Count > 0)
            {
                foreach (var simg in subImgs)
                {
                    if (simg.Length > 0)
                    {
                        var imgName = Guid.NewGuid().ToString() + Path.GetExtension(simg.FileName);
                        var imgPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg\ProductSubImg", imgName);
                        using (var stream = System.IO.File.Create(imgPath))
                        {
                            simg.CopyTo(stream);
                        }
                        context.ProductSubImages.Add(new ProductSubImage
                        {
                            Imge = imgName,
                            ProductId = product.Id
                        });
                    }
                    context.SaveChanges();
                }
            }
            #endregion

            #region add colors
            if (colors.Length > 0)
            {
                foreach (var color in colors)
                {
                    context.ProductColors.Add(new ProductColor
                    {
                        Color = color,
                        ProductId = product.Id
                    });
                }
                context.SaveChanges();
            }
            #endregion

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = context.Products.FirstOrDefault(b => b.Id == id);
            if (product is null)
                return View("NotFoundPage", "Home");

            var brands = context.Brands;
            var categories = context.Categroys;
            var prductSubImgs = context.ProductSubImages.Where(p => p.ProductId == id);
            var productColors = context.ProductColors.Where(pc => pc.ProductId == id);

            return View(new ProductVM
            {
                brands = brands,
                Categroys = categories,
                product = product,
                ProductSubImages = prductSubImgs,
                productColors = productColors
            });
        }
        [HttpPost]
        public IActionResult Edit(Product product, string[] colors, IFormFile img, List<IFormFile> imgs)
        {
            #region edit main imge
            var oldProduct = context.Products.AsNoTracking().FirstOrDefault(p => p.Id == product.Id);
            if (img is not null && img.Length > 0)
            {
                var imgName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg", imgName);
                using (var stream = System.IO.File.Create(imgPath))
                {
                    img.CopyTo(stream);
                }
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg", oldProduct.MainImage);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
                product.MainImage = imgName;
            }
            else
                product.MainImage = oldProduct.MainImage;
            context.Update(product);
            context.SaveChanges();
            #endregion

            #region edit sub imges
            if (imgs is not null && imgs.Count > 0)
            {
                var editSubImgs = context.ProductSubImages.Where(p => p.ProductId == product.Id);
                foreach (var oldSubImg in editSubImgs)
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg\ProductSubImg", oldSubImg.Imge);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                    context.Remove(oldSubImg);
                }
                context.SaveChanges();
                foreach (var simg in imgs)
                {
                    if (simg.Length > 0)
                    {
                        var imgName = Guid.NewGuid().ToString() + Path.GetExtension(simg.FileName);
                        var imgPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg\ProductSubImg", imgName);
                        using (var stream = System.IO.File.Create(imgPath))
                        {
                            simg.CopyTo(stream);
                        }
                        context.ProductSubImages.Add(new ProductSubImage
                        {
                            Imge = imgName,
                            ProductId = product.Id
                        });
                    }
                }
                context.SaveChanges();
            }
            #endregion

            #region edit colors
            if (colors.Length > 0 && colors is not null)
            {
                var oldColors = context.ProductColors.Where(pc => pc.ProductId == product.Id);
                foreach (var oldColor in oldColors)
                    context.Remove(oldColor);
                context.SaveChanges();
                foreach (var color in colors)
                {
                    context.ProductColors.Add(new ProductColor
                    {
                        Color = color,
                        ProductId = product.Id
                    });
                }
                context.SaveChanges();
            }
            #endregion

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var product = context.Products.FirstOrDefault(b => b.Id == id);
            var subimgs = context.ProductSubImages.Where(p => p.ProductId == id);
            var productColors = context.ProductColors.Where(p => p.ProductId == id);
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg", product.MainImage);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
            if (subimgs is not null)
            {
                foreach (var subimg in subimgs)
                {
                    var oldSubPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg\ProductSubImg", subimg.Imge);
                    if (System.IO.File.Exists(oldSubPath))
                        System.IO.File.Delete(oldSubPath);
                    context.ProductSubImages.Remove(subimg);
                }
                context.SaveChanges();
            }
            if (productColors is not null)
            {
                foreach (var color in productColors)
                    context.ProductColors.Remove(color);
                context.SaveChanges();
            }
            context.Remove(product);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteSubImg(int productId, string img)
        {
            //get subimg from db
            var subimg = context.ProductSubImages.FirstOrDefault(p => p.ProductId == productId && p.Imge == img);
            //get old path of subimg
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg\ProductSubImg", img);
            if (subimg is null)
                return View("NotFoundPage", "Home");
            //delete subimg from wwwroot
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
            //delete subimg from db
            context.ProductSubImages.Remove(subimg);
            context.SaveChanges();
            return RedirectToAction(nameof(Edit) , new {id = productId});
        }
    }
}
