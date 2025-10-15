using ECommerce.Models;
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
            //ViewBag.brandid = brandID;
            context.Categroys.Add(categroy);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = context.Categroys.FirstOrDefault(c=>c.Id == id);
            if (category is null)
                return View("NotFoundPage", "Home");
            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Categroy categroy)
        {
            context.Categroys.Update(categroy);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var categroy = context.Categroys.FirstOrDefault(c => c.Id == id);
            if (categroy is null)
                return View("NotFoundPage", "Home");
            context.Categroys.Remove(categroy);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
