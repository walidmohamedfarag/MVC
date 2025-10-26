using System.Diagnostics;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Areas.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDBContext context; //= new();
        public HomeController(ILogger<HomeController> logger , ApplicationDBContext _context)
        {
            context = _context;
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
