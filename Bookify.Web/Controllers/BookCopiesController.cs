namespace Bookify.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create(int BookId)
        {
            var book = _context.Books.Find(BookId);

            if (book is null)
                return NotFound();

            var viewModel = new BookCopiesFormViewModel
            {
                BookId = BookId,
                IsBookAvaliableForRental = book.IsAvailableForRent
            };
            return PartialView("Form", viewModel);
        }

        [HttpGet]
        public IActionResult RentalHistory(int id)
        {

            var rentals = _context.RentalCopies
                .Include(c => c.Rental)
                .ThenInclude(c => c!.Subscriper)
                .Where(c => c.BookCopyId == id)
                .ToList();


            // var viewModel = _mapper.Map<IEnumerable<RentalViewModel>>(rentals);


            IList<RentalHistoryViewModel> viewModels = new List<RentalHistoryViewModel>();

            foreach (var rental in rentals)
            {
                var history = new RentalHistoryViewModel
                {
                    SubscriberEmail = rental.Rental!.Subscriper!.Email,
                    SubscriberFullName = rental.Rental!.Subscriper!.FirstName + rental.Rental!.Subscriper!.LastName,
                    CreatedOn = rental.Rental!.CreatedOn,
                    RentalDate = rental.RentalDate,
                    ExtendOn = rental.ExtendOn,
                    EndDate = rental.EndDate,
                    ReturnDate = rental.ReturnDate,

                };
                viewModels.Add(history);
            }


            return PartialView("_RentalHistory", viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopiesFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _context.Books.Find(model.BookId);

            if (book is null)
                return NotFound();

            var copy = new BookCopy
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRent && model.IsAvailableForRental
            };

            book.Copies.Add(copy);
            book.CreatedById = User.GetUserId();

            _context.SaveChanges();

            var ViewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", ViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int Id)
        {
            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(b => b.Id == Id);

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopiesFormViewModel>(copy);
            viewModel.IsBookAvaliableForRental = copy.Book!.IsAvailableForRent;

            return PartialView("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookCopiesFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(b => b.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = model.IsAvailableForRental ? model.IsAvailableForRental : false;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdateById = User.GetUserId();

            _context.SaveChanges();

            var ViewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int Id)
        {
            var copy = _context.BookCopies.Find(Id);

            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdateById = User.GetUserId();

            _context.SaveChanges();

            return Ok(copy.LastUpdatedOn.ToString());
        }
    }
}
