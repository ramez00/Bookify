using Bookify.Application.Common.Interfaces.Repository;

namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(ApplicationDbContext context, IMapper mapper,
             IUnitOfWork unitOfWork)
        {
            _context = context;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var categories = _unitOfWork.Categories.GetAll();

            var catViewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

            return View(catViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var cat = _mapper.Map<Category>(model);
            cat.CreatedOn = DateTime.Now;
            cat.CreatedById = User.GetUserId();

            _context.Categories.Add(cat);
            _context.SaveChanges();

            var catViewModal = _mapper.Map<CategoryViewModel>(cat);

            return PartialView("_CategoryRow", catViewModal);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int Id)
        {
            var Category = _context.Categories.Find(Id);

            if (Category is null)
                return NotFound();

            var catViewModal = _mapper.Map<CategoryFormViewModel>(Category);

            return PartialView("_Form", catViewModal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var Category = _context.Categories.Find(model.Id);

            if (Category is null)
                return NotFound();

            Category.Name = model.Name;
            Category.LastUpdatedOn = DateTime.Now;
            Category.LastUpdateById = User.GetUserId();

            _context.SaveChanges();

            var catViewModal = _mapper.Map<CategoryViewModel>(Category);

            return PartialView("_CategoryRow", catViewModal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int Id)
        {
            var Category = _context.Categories.Find(Id);

            if (Category is null)
                return NotFound();

            Category.IsDeleted = !Category.IsDeleted;
            Category.LastUpdatedOn = DateTime.Now;
            Category.LastUpdateById = User.GetUserId();

            _context.SaveChanges();

            return Ok(Category.LastUpdatedOn.ToString());
        }

        public IActionResult IsExist(CategoryFormViewModel model)
        {
            var category = _context.Categories.SingleOrDefault(c => c.Name == model.Name);

            var isValid = category is null || category.Id.Equals(model.Id);

            return Json(isValid);
        }
    }
}
