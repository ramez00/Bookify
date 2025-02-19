using RentalPerDayViewModel = Bookify.Web.Core.ViewModels.RentalPerDayViewModel;

namespace Bookify.Web.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashBoardController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var numberOfBooks = _context.BookCopies.Count();
            var numberOfSubscriber = _context.Subscripers.Count();

            numberOfBooks = numberOfBooks > 10 ? numberOfBooks / 10 * 10 : numberOfBooks;

            var copies = _context.Books
                .Include(x => x.Author)
                .Include(x => x.Copies.Where(c => !c.IsDeleted)).OrderByDescending(x => x.CreatedOn)
                .Take(8)
                .OrderByDescending(b => b.Id)
                .ToList();

            var topBooks = _context.RentalCopies
                .Include(r => r.BookCopy)
                .ThenInclude(c => c!.Book)
                .ThenInclude(b => b!.Author)
                .GroupBy(b => new
                {
                    b.BookCopy!.BookId,
                    b.BookCopy!.Book!.Title,
                    b.BookCopy.Book.ImageThumbUrl,
                    AuthorName = b.BookCopy.Book.Author!.Name
                })
                .Select(b => new
                {
                    b.Key.BookId,
                    b.Key.Title,
                    b.Key.ImageThumbUrl,
                    b.Key.AuthorName,
                    cont = b.Count()
                })
                .OrderByDescending(b => b.cont)
                .Take(6)
                .Select(b => new BookViewModel
                {
                    Title = b.Title,
                    Author = b.AuthorName,
                    Id = b.BookId,
                    ImageThumbUrl = b.ImageThumbUrl
                })
                .ToList();

            var viewModel = new DashBoardViewModel
            {
                NumberOfCopies = numberOfBooks,
                NumberOfSubscribers = numberOfSubscriber,
                LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(copies),
                TopBooks = topBooks
            };
            return View(viewModel);
        }

        [AjaxOnly]
        public IActionResult GetRentalPerDay(DateTime? startDate, DateTime? EndDate)
        {
            startDate ??= DateTime.Today.AddDays(-60);
            EndDate ??= DateTime.Today;

            var rentals = _context.RentalCopies
                .Where(r => r.RentalDate >= startDate && r.RentalDate <= EndDate)
                .GroupBy(r => r.RentalDate)
                .Select(r => new RentalPerDayViewModel
                {
                    Label = r.Key.ToString("d MMM"),
                    Value = r.Count().ToString()
                }).ToList();

            List<RentalPerDayViewModel> figure = new();

            for (var day = startDate; day <= EndDate; day = day.Value.AddDays(1))
            {
                var dayDate = rentals.SingleOrDefault(r => r.Label == day.Value.ToString("d MMM"));

                RentalPerDayViewModel record = new()
                {
                    Label = day.Value.ToString("d MMM"),
                    Value = dayDate is null ? "0" : dayDate.Value
                };
                figure.Add(record);
            }

            return Ok(rentals);
        }

        public IActionResult GetSubscriberPerCity()
        {
            var subscribers = _context.Subscripers
                .Include(s => s.Governorate)
                .Where(s => !s.IsDeleted)
                .GroupBy(s => new { GovernorateName = s.Governorate!.Name })
                .Select(s => new ChartItemsViewModel
                {
                    Label = s.Key.GovernorateName,
                    Value = s.Count().ToString()
                })
                .ToList();

            return Ok(subscribers);
        }
    }
}
