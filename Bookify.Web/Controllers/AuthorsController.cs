
using Bookify.Application.Common.Interfaces.Repository;
using Bookify.Web.Services;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Archive)]
    public class AuthorsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AuthorsController(IMapper mapper,
             IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var authors = _unitOfWork.Authors.GetAll();

            var AuthorsViewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);

            return View(AuthorsViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_AuthorForm");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _mapper.Map<Author>(model);
            author.CreatedById = User.GetUserId();

            var AuthorViewModal = _mapper.Map<AuthorViewModel>(_unitOfWork.Authors.Add(author));

            return PartialView("_AuthorRow", AuthorViewModal);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int Id)
        {
            var author = _unitOfWork.Authors.GetById(Id);

            if (author is null)
                return NotFound();

            var authorViewModal = _mapper.Map<AuthorFormViewModel>(author);

            return PartialView("_AuthorForm", authorViewModal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _unitOfWork.Authors.GetById(model.Id);

            if (author is null)
                return NotFound();

            author.LastUpdatedOn = DateTime.Now;
            author.CreatedById = User.GetUserId();
            author.Name = model.Name;

            _unitOfWork.Authors.Update(author);
            _unitOfWork.SaveChanges();

            var AuthorViewModal = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", AuthorViewModal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int Id)
        {
            var author = _unitOfWork.Authors.GetById(Id); 

            if (author is null)
                return NotFound();

            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;
            author.CreatedById = User.GetUserId();

            _unitOfWork.Authors.Update(author);
            _unitOfWork.SaveChanges();

            return Ok(author.LastUpdatedOn.ToString());
        }

        public IActionResult IsExist(AuthorFormViewModel model)
        {
            var author = _unitOfWork.Authors.Find(au => au.Name == model.Name);

            var isValid = author is null || author.Id.Equals(model.Id);

            return Json(isValid);
        }
    }
}
