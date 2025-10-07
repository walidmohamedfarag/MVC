using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        ApplicationDBContext context = new();
        public IActionResult Index()
        {
            var categories = context.Categroys;
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Categroy categroy)
        {
            context.Categroys.Add(categroy);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = context.Categroys.FirstOrDefault(c=>c.Id == id);
            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Categroy categroy)
        {
            context.Categroys.Update(categroy);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
