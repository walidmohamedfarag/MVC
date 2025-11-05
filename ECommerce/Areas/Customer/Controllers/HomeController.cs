using System.Diagnostics;
using System.Threading.Tasks;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Areas.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDBContext context; //= new();
        private readonly IRepositroy<Product> productRepo;

        public HomeController(ILogger<HomeController> logger , ApplicationDBContext _context , IRepositroy<Product> _productRepo)
        {
            context = _context;
            productRepo = _productRepo;
            _logger = logger;
        }

        public IActionResult Index(string name, decimal? minPrice, decimal? maxPrice, int? categoryId, int? brandId, bool IsHot, int page = 1)
        {
            //IsHot = false;
            ViewBag.categories = context.Categroys;
            ViewData["brands"] = context.Brands;
            var products = context.Products.Include(p => p.Categroy).AsQueryable();
            if (name is not null)
                products = products.Where(p => p.Name.Contains(name));
            if(minPrice is not null)
                products = products.Where(p => p.Price - p.Price * p.Discount/100 >= minPrice);
            if(maxPrice is not null)
                products = products.Where(p => p.Price - p.Price * p.Discount/100 <= maxPrice);
            if(categoryId is not null)
                products = products.Where(p=>p.CategroyId == categoryId);
            if(brandId is not null)
                products = products.Where(p=>p.BrandId == brandId);
            if (IsHot)
            {
                var dicount = 20;
                products = products.Where(p => p.Discount >= dicount);
            }
            ViewBag.isHot = IsHot;
            ViewBag.brandId = brandId;
            ViewBag.categoryId = categoryId;
            ViewBag.minPrice = minPrice;
            ViewBag.maxPrice = maxPrice;
            ViewBag.name = name;
            ViewBag.currentPage = page;
            ViewBag.totalPages = Math.Ceiling(products.Count() / 8.0);
            products = products.Skip((page - 1) * 8).Take(8);
            return View(products);
        }
        [Authorize]
        public async Task<IActionResult> Item(int id , CancellationToken cancellationToken)
        {
            var product = await productRepo.GetOneAsync(p => p.Id == id, includes: [p => p.Categroy, p => p.Brand], tracked: false, cancellationToken: cancellationToken);
            if (product == null)
                return View("NotFoundPage");
            product.Traffic += 1;
            await productRepo.CommitAsync(cancellationToken);
            var relatedProducts = await productRepo.GetAsync(p => p.Name.Contains(product.Name) && p.Id != id, cancellationToken: cancellationToken);
            return View(new ProductVM 
            {
                product = product,
                RealatedProduct = relatedProducts.OrderBy(rp=>rp.Traffic).Skip(0).Take(4).ToList()
            });
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
