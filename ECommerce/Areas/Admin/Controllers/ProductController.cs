using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDBContext context;
        IRepositroy<Product> productRepo;
        IRepositroy<Categroy> categoryRepo;
        IRepositroy<Brand> brandRepo;
        IRepositroy<ProductColor> productColorRepo;
        IRepositroy<ProductSubImage> subImageRepo;
        public ProductController(ApplicationDBContext _context,IRepositroy<Product> _productRepo , IRepositroy<ProductSubImage> _subImageRepo , IRepositroy<Brand> _brandRepo , IRepositroy<ProductColor> _productColorRepo , IRepositroy<Categroy> _categoryRepo)
        {
            context = _context;
            productRepo = _productRepo;
            subImageRepo = _subImageRepo;
            brandRepo = _brandRepo;
            productColorRepo = _productColorRepo;
            categoryRepo = _categoryRepo;
        }

        public async Task<IActionResult> Index(string name, decimal? minPrice, decimal? maxPrice, int? categoryId, int? brandId, bool? lessQuantity, CancellationToken cancellationToken, int page = 1)
        {
            var product =await productRepo.GetAsync(includes: [p=>p.Categroy , p=>p.Brand ] , cancellationToken: cancellationToken);
            if (name is not null)
                product =await productRepo.GetAsync(p => p.Name.Contains(name) , cancellationToken: cancellationToken);
            if (minPrice is not null)
                product = await productRepo.GetAsync(p => p.Price - (p.Price * (p.Discount / 100)) >= minPrice, cancellationToken: cancellationToken);
            if (maxPrice is not null)
                product = await productRepo.GetAsync(p => p.Price - (p.Price * (p.Discount / 100)) <= maxPrice, cancellationToken: cancellationToken);
            if (categoryId is not null)
                product = await productRepo.GetAsync(p => p.CategroyId == categoryId, cancellationToken: cancellationToken);
            if (brandId is not null)
                product = await productRepo.GetAsync(p => p.BrandId == brandId, cancellationToken: cancellationToken);
            if (lessQuantity is not null)
                product = await productRepo.GetAsync(p => p.Quantity <= 100, cancellationToken: cancellationToken);
            ViewBag.name = name;
            ViewBag.minPrice = minPrice;
            ViewBag.maxPrice = maxPrice;
            ViewBag.categoryId = categoryId;
            ViewBag.brandId = brandId;
            ViewBag.lessQuantity = lessQuantity;
            ViewBag.categories = await categoryRepo.GetAsync();
            ViewData["brands"] = await brandRepo.GetAsync();
            ViewBag.totalPages = Math.Ceiling(product.Count() / 8.0);
            ViewBag.currentPage = page;
            product = product.Skip((page - 1) * 8).Take(8);
            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var brands =await brandRepo.GetAsync(cancellationToken: cancellationToken);
            var categories = await categoryRepo.GetAsync(cancellationToken: cancellationToken);
            return View(new ProductVM
            {
                brands = brands,
                Categroys = categories
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile img, List<IFormFile> subImgs, string[] colors , CancellationToken cancellationToken)
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
            await productRepo.AddAsync(product , cancellationToken : cancellationToken);
            await productRepo.CommitAsync();
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
                        await subImageRepo.AddAsync(new ProductSubImage
                        {
                            Imge = imgName,
                            ProductId = product.Id
                        },cancellationToken : cancellationToken);
                    }
                    await subImageRepo.CommitAsync();
                }
            }
            #endregion

            #region add colors
            if (colors.Length > 0)
            {
                foreach (var color in colors)
                {
                    await productColorRepo.AddAsync(new ProductColor
                    {
                        Color = color,
                        ProductId = product.Id
                    },cancellationToken);
                }
                await productColorRepo.CommitAsync();
            }
            #endregion

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id , CancellationToken cancellationToken)
        {
            var product = await productRepo.GetOneAsync(b => b.Id == id , cancellationToken: cancellationToken);
            if (product is null)
                return View("NotFoundPage", "Home");

            var brands = await brandRepo.GetAsync(cancellationToken: cancellationToken);
            var categories = await categoryRepo.GetAsync(cancellationToken: cancellationToken);
            var prductSubImgs = await subImageRepo.GetAsync(p => p.ProductId == id , cancellationToken : cancellationToken);
            var productColors = await productColorRepo.GetAsync(pc => pc.ProductId == id, cancellationToken: cancellationToken);

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
        public async Task<IActionResult> Edit(Product product, string[] colors, IFormFile img, List<IFormFile> imgs, CancellationToken cancellationToken)
        {
            #region edit main imge
            var oldProduct = await productRepo.GetOneAsync(p => p.Id == product.Id,tracked:false , cancellationToken: cancellationToken);
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
            productRepo.Update(product);
            await productRepo.CommitAsync();
            #endregion

            #region edit sub imges
            if (imgs is not null && imgs.Count > 0)
            {
                var editSubImgs =await subImageRepo.GetAsync(p => p.ProductId == product.Id , cancellationToken: cancellationToken);
                foreach (var oldSubImg in editSubImgs)
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg\ProductSubImg", oldSubImg.Imge);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                    subImageRepo.Delete(oldSubImg);
                }
                await subImageRepo.CommitAsync();
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
                        await subImageRepo.AddAsync(new ProductSubImage
                        {
                            Imge = imgName,
                            ProductId = product.Id
                        },cancellationToken);
                    }
                }
                await subImageRepo.CommitAsync();
            }
            #endregion

            #region edit colors
            if (colors.Length > 0 && colors is not null)
            {
                var oldColors = await productColorRepo.GetAsync(pc => pc.ProductId == product.Id , cancellationToken: cancellationToken);
                foreach (var oldColor in oldColors)
                    productColorRepo.Delete(oldColor);
                await productColorRepo.CommitAsync(cancellationToken);
                foreach (var color in colors)
                {
                    await productColorRepo.AddAsync(new ProductColor
                    {
                        Color = color,
                        ProductId = product.Id
                    },cancellationToken);
                }
                await productColorRepo.CommitAsync(cancellationToken);
            }
            #endregion

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id , CancellationToken cancellationToken)
        {
            var product = await productRepo.GetOneAsync(b => b.Id == id , cancellationToken: cancellationToken);
            var subimgs = await subImageRepo.GetAsync(p => p.ProductId == id,cancellationToken: cancellationToken);
            var productColors = await productColorRepo.GetAsync(p => p.ProductId == id, cancellationToken: cancellationToken);
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
                    subImageRepo.Delete(subimg);
                }
                await subImageRepo.CommitAsync(cancellationToken);
            }
            if (productColors is not null)
            {
                foreach (var color in productColors)
                    productColorRepo.Delete(color);
                await productColorRepo.CommitAsync(cancellationToken);
            }
            productRepo.Delete(product);
            await productRepo.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteSubImg(int productId, string img , CancellationToken cancellationToken)
        {
            //get subimg from db
            var subimg = await subImageRepo.GetOneAsync(p => p.ProductId == productId && p.Imge == img,cancellationToken: cancellationToken);
            //get old path of subimg
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\Admin\ProductImg\ProductSubImg", img);
            if (subimg is null)
                return View("NotFoundPage", "Home");
            //delete subimg from wwwroot
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
            //delete subimg from db
            subImageRepo.Delete(subimg);
            await subImageRepo.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Edit) , new {id = productId});
        }
    }
}
