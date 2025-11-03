
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IRepositroy<Categroy> repo;

        public CategoryController(IRepositroy<Categroy> _repo)
        {
            repo = _repo;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var categories = await repo.GetAsync(cancellationToken: cancellationToken);
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Categroy categroy,CancellationToken cancellationToken)
        {
            //ViewBag.brandid = brandID;
            await repo.AddAsync(categroy ,cancellationToken: cancellationToken);
            await repo.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    
        [HttpGet]
        public async Task<IActionResult> Edit(int id , CancellationToken cancellationToken)
        {
            var category = await repo.GetOneAsync(c=>c.Id == id , cancellationToken : cancellationToken , tracked:false);
            if (category is null)
                return View("NotFoundPage");
            return View(category);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Categroy categroy , CancellationToken cancellationToken)
        {
            repo.Update(categroy);
            await repo.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id , CancellationToken cancellationToken)
        {
            var categroy = await repo.GetOneAsync(c => c.Id == id,cancellationToken:cancellationToken);
            if (categroy is null)
                return View("NotFoundPage");
            repo.Delete(categroy);
            await repo.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
