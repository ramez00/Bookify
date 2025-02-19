using Microsoft.AspNetCore.DataProtection;

namespace Bookify.Web.Controllers
{
    public class RentalController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;

        public RentalController(ApplicationDbContext context, IMapper mapper, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtectionProvider.CreateProtector("ProtectValue");
        }

        [HttpGet]
        public IActionResult Create(string sKey)
        {
            var subscriber = _context.Subscripers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .FirstOrDefault(s => s.Id == int.Parse(_dataProtector.Unprotect(sKey)));

            if (subscriber is null)
                return NotFound();

            var (errorMsg, maxAllowed) = SubscribtionValidation(subscriber);

            if (!string.IsNullOrEmpty(errorMsg))
                return View("NotAllowedRental", errorMsg);

            var viewModel = new RentalFormViewModel()
            {
                SubscriberKey = sKey,
                MaxAllowed = maxAllowed
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RentalFormViewModel model)
        {

            if (!ModelState.IsValid)
                return View(model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = _context.Subscripers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .FirstOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (copies, errorMsg) = CheckRental(model);

            if (copies is null)
                return View("NotAllowedRental", errorMsg);


            Rental rental = new()
            {
                RentalCopies = copies,
                CreatedById = User.GetUserId()
            };

            subscriber.Rentals.Add(rental);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetCopyDetials(SearchViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var copy = _context.BookCopies.Include(c => c.Book)
                .SingleOrDefault(c => c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted);

            if (copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRent)
                return NotFound(Errors.NotAvliableForRental);

            //var Rentaled = _context.RentalCopies.Where(c => c.BookCopyId == copy.BookId && !c.ReturnDate.HasValue);

            var copyIsRentaled = _context.RentalCopies.Any(c => c.BookCopyId == copy.Id && !c.ReturnDate.HasValue);

            if (copyIsRentaled)
                return BadRequest(Errors.CopyIsRentaled);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_CopyDetials", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelRental(int id)
        {
            var rental = _context.Rentals.Find(id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdateById = User.GetUserId();

            _context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public IActionResult Details(int Id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(r => r.BookCopy)
                .ThenInclude(b => b!.Book)
                .FirstOrDefault(r => r.Id == Id);

            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);

            return View("Details", viewModel);

        }


        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var rental = _context.Rentals
                .Include(r => r.Subscriper)
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .SingleOrDefault(r => r.Id == Id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscripers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .FirstOrDefault(r => r.Id == rental.Subscriper!.Id);


            var selectedBookIds = rental.RentalCopies.Select(s => s.BookCopyId).ToList();

            var selectedBooks = _context.BookCopies
                .Where(b => selectedBookIds.Contains(b.Id))
                .Include(b => b.Book)
                .ToList();

            var (errorMsg, maxAllowed) = SubscribtionValidation(subscriber!, rental.Id);

            var viewModel = new RentalFormViewModel()
            {
                SubscriberKey = _dataProtector.Protect(subscriber!.Id.ToString()),
                MaxAllowed = maxAllowed,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(selectedBooks)
            };

            return View(nameof(Create), viewModel);
        }


        [HttpGet]
        public IActionResult Return(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.Subscriper)
                .ThenInclude(s => s!.Subscriptions)
                .Include(r => r.RentalCopies.Where(c => !c.ReturnDate.HasValue))
                .ThenInclude(c => c.BookCopy)
                .ThenInclude(c => c!.Book)
                .SingleOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var viewModel = new RentalReturnFormViewModel
            {
                Id = id,

                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies),
                SelectedCopies = rental.RentalCopies.Select(c => new ReturnCopyOption { Id = c.BookCopyId, IsReturned = c.ExtendOn.HasValue ? false : null }).ToList(),
                AllowExtend = !rental.Subscriper!.IsBlackListed
                && rental.Subscriper.Subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)ApplicationEnums.MaxRentalDuration)
                && rental.StartDate.AddDays((int)ApplicationEnums.RentalDuration) >= DateTime.Today
            };

            //viewModel.TotalDelayInDays = viewModel.Copies.Sum(c => c.DelayInDays);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Return(RentalReturnFormViewModel model)
        {
            var rental = _context.Rentals
                .Include(r => r.Subscriper)
                .ThenInclude(s => s!.Subscriptions)
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .ThenInclude(c => c!.Book)
                .SingleOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies);
                return View(model);
            }

            var subscriber = rental.Subscriper;

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            {
                model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies);

                if (subscriber!.IsBlackListed)
                    ModelState.AddModelError("", Errors.BlackListedSubscriber);

                if (subscriber!.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)ApplicationEnums.MaxRentalDuration))
                    ModelState.AddModelError("", Errors.SubsucriberNotAllowedToExtend);

                if (rental.StartDate.AddDays((int)ApplicationEnums.MaxRentalDuration) < DateTime.Today)
                    ModelState.AddModelError("", Errors.SubsucriberNotAllowedToExtend);

                if (ModelState.ErrorCount != 0)
                    return View(model);
            }

            var isUpdated = false;

            foreach (var copy in model.SelectedCopies)
            {
                if (!copy.IsReturned.HasValue) continue;

                var currentCopy = rental.RentalCopies.SingleOrDefault(r => r.BookCopyId == copy.Id);

                if (currentCopy is null) continue;

                if (copy.IsReturned.HasValue && copy.IsReturned.Value)
                {
                    if (currentCopy.ReturnDate.HasValue) continue;

                    isUpdated = true;
                    currentCopy.ReturnDate = DateTime.Now;
                }

                if (copy.IsReturned.HasValue && !copy.IsReturned.Value)
                {
                    if (currentCopy.ExtendOn.HasValue) continue;

                    isUpdated = true;
                    currentCopy.ExtendOn = DateTime.Now;
                    currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)ApplicationEnums.MaxRentalDuration);
                }
            }

            if (isUpdated)
            {
                rental.LastUpdatedOn = DateTime.Now;
                rental.LastUpdateById = User.GetUserId();
                rental.PentalyPaid = model.IsPenalitesPaied;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Create), model);

            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .SingleOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var (copies, errorMsg) = CheckRental(model, isEditMode: true);

            if (copies is null)
                return View("NotAllowedRental", errorMsg);

            rental.CreatedById = User.GetUserId();
            rental.LastUpdatedOn = DateTime.Now;

            rental.RentalCopies = copies;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        private (List<RentalCopy>? copies, string? errorMsg) CheckRental(RentalFormViewModel model, bool isEditMode = false)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = _context.Subscripers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .FirstOrDefault(s => s.Id == subscriberId);

            var rentalId = isEditMode ? model.Id : null;

            var (errorMsg, maxAllowed) = SubscribtionValidation(subscriber!, rentalId);

            if (!string.IsNullOrEmpty(errorMsg))
                return (null, errorMsg);

            var selectedCopies = _context.BookCopies
                .Include(c => c.Book)
                .Include(c => c.Rentals)
                .Where(c => model.SelectedCopies.Contains(c.SerialNumber))
                .ToList();


            var CurrentSubscriberRental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .Where(r => r.Subscriper!.Id == subscriberId && r.Id != rentalId)
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.BookCopy!.BookId)
                .ToList();

            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRent)
                    return (null, Errors.NotAvliableForRental);


                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue && c.RentalId != rentalId))
                    return (null, Errors.CopyIsRentaled);

                if (CurrentSubscriberRental.Any(bookId => bookId == copy.BookId))
                    return (null, "Subscriber Rented copy of the Book");

                copies.Add(new RentalCopy { BookCopyId = copy.Id });
            }

            return (copies, null);
        }

        private (string errorMsg, int? maxAllowed) SubscribtionValidation(Subscriper subscriber, int? rentalId = null)
        {
            if (subscriber.IsBlackListed)
                return (errorMsg: Errors.BlackListedSubscriber, maxAllowed: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)ApplicationEnums.MaxRentalCopies))
                return (errorMsg: Errors.InactiveSubscriber, maxAllowed: null);

            var currentRentals = subscriber.Rentals
                .Where(r => rentalId == null || r.Id != rentalId)
                .SelectMany(r => r.RentalCopies)
                .Count(c => !c.ReturnDate.HasValue);

            var maximumAllowed = (int)ApplicationEnums.MaxRentalCopies - currentRentals;

            if (maximumAllowed.Equals(0))
                return (errorMsg: Errors.MaximumReachedSubscriber, maxAllowed: null);

            return (errorMsg: string.Empty, maximumAllowed);

        }
    }
}
