using Bookify.Application.Common.Models;
using ClosedXML.Excel;
using OpenHtmlToPdf;
using ViewToHTML.Services;

namespace Bookify.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IViewRendererService _viewRendererService;

        public ReportsController(ApplicationDbContext context, IMapper mapper,
            IViewRendererService viewRendererService)
        {
            _context = context;
            _mapper = mapper;
            _viewRendererService = viewRendererService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Doctors(DateTime From, DateTime To, int? pageNumber)
        {
            var rentals = _context.RentalCopies
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriper)
                .Include(r => r.BookCopy)
                .ThenInclude(c => c!.Book)
                .ThenInclude(r => r!.Author)
                .Where(c => c.ReturnDate >= From && c.EndDate <= To);

            DoctorReportViewModel viewModel = new()
            {
                Rentals = PaginatedList<RentalCopy>.Create(rentals, 1, 10),
                PageMode = 1
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Clinics(IList<int> selectedAuthor, IList<int> selectedCategories
            , int? pageNumber)
        {
            var author = _context.Authors.OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.OrderBy(a => a.Name).ToList();

            IQueryable<Book> books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if (selectedAuthor.Any())
                books = books.Where(b => selectedAuthor.Contains(b.AuthorId));

            if (selectedCategories.Any())
                books = books.Where(b => b.Categories.Any(c => selectedCategories.Contains(c.CategoryId)));

            ClinicReportViewModel viewModel = new()
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(author),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories)
            };

            if (pageNumber is not null)
                viewModel.Books = PaginatedList<Book>.Create(books, pageNumber ?? 0, 10);

            return View(viewModel); ;
        }

        [HttpGet]
        public async Task<IActionResult> ExportDatatoExcel(string authors, string categories)
        {

            var selectedAuthor = authors?.Split(",");
            var selectedCategories = categories?.Split(",");

            IQueryable<Book> booksQuerable = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if (!string.IsNullOrEmpty(authors))
                booksQuerable = booksQuerable.Where(b => selectedAuthor!.Contains(b.AuthorId.ToString()));

            if (!string.IsNullOrEmpty(categories))
                booksQuerable = booksQuerable.Where(b => b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString())));

            var books = booksQuerable.ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Books");

            var headerCells = new string[] { "Title" , "Author" , "Categories" , "Publisher" ,
                               "Publishing Date", "Hall" , "Is avaliable for Rental" , "Status" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < books.Count; i++)
            {
                sheet.Cell(i + 2, 1).SetValue(books[i].Title);
                sheet.Cell(i + 2, 2).SetValue(books[i].Author!.Name);
                sheet.Cell(i + 2, 3).SetValue(string.Join(", ", books[i].Categories.Select(c => c.Category!.Name)));
                sheet.Cell(i + 2, 4).SetValue(books[i].Publisher);
                sheet.Cell(i + 2, 5).SetValue(books[i].PublishingDate.ToString("dd-MM-yyyy"));
                sheet.Cell(i + 2, 6).SetValue(books[i].Hall);
                sheet.Cell(i + 2, 7).SetValue(books[i].IsAvailableForRent ? "yes" : "no");
                sheet.Cell(i + 2, 8).SetValue(books[i].IsDeleted ? "deleted" : "available");
            }

            sheet.AddFormate();

            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), "application/octet-stream", $"books_{DateTime.Now}.xlsx");

        }

        [HttpGet]
        public async Task<IActionResult> ExportDatatoPDF(string authors, string categories)
        {

            var selectedAuthor = authors?.Split(",");
            var selectedCategories = categories?.Split(",");

            IQueryable<Book> booksQuerable = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if (!string.IsNullOrEmpty(authors))
                booksQuerable = booksQuerable.Where(b => selectedAuthor!.Contains(b.AuthorId.ToString()));

            if (!string.IsNullOrEmpty(categories))
                booksQuerable = booksQuerable.Where(b => b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString())));

            var books = booksQuerable.ToList();

            var tempPath = "~/Views/Reports/ReportPdf.cshtml";

            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, tempPath, books);

            var pdf = Pdf.From(html).Content();

            //.Encodwith("Utf-8") => if u have any problem to Arabic
            //.LandScape() => To show data at landscape

            return File(pdf.ToArray(), "application/octet-stream", $"books_{DateTime.Now}.pdf");

        }

        [HttpGet]
        public async Task<IActionResult> ExportDoctortoExcel(DateTime From, DateTime To, int? pageNumber)
        {

            var rentalsQuery = _context.RentalCopies
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriper)
                .Include(r => r.BookCopy)
                .ThenInclude(c => c!.Book)
                .ThenInclude(r => r!.Author)
                .Where(c => c.ReturnDate >= From && c.EndDate <= To);


            var rentals = rentalsQuery.ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Rentals");

            var headerCells = new string[] { "Subscriber ID" , "Subscriber Name" , "Subscriber Phone" ,"Author Name", "Book Title" ,
                               "Rental Date", "End Date" , "Return Date" , "Extended On" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < rentals.Count; i++)
            {
                sheet.Cell(i + 2, 1).SetValue(rentals[i].Rental!.Subscriper!.Id);
                sheet.Cell(i + 2, 2).SetValue(rentals[i].Rental!.Subscriper!.FirstName);
                sheet.Cell(i + 2, 3).SetValue(rentals[i].Rental!.Subscriper!.MobileNumber);
                sheet.Cell(i + 2, 4).SetValue(rentals[i].BookCopy!.Book!.Author!.Name);
                sheet.Cell(i + 2, 5).SetValue(rentals[i].BookCopy!.Book!.Title);
                sheet.Cell(i + 2, 6).SetValue(rentals[i].RentalDate.ToString("dd-MM-yyyy"));
                sheet.Cell(i + 2, 7).SetValue(rentals[i].EndDate.ToString("dd-MM-yyyy"));
                sheet.Cell(i + 2, 8).SetValue(rentals[i].ReturnDate?.ToString("dd-MM-yyyy"));
                sheet.Cell(i + 2, 9).SetValue(rentals[i].ExtendOn?.ToString("dd-MM-yyyy"));
            }

            sheet.AddFormate();

            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), "application/octet-stream", $"Rentals_{DateTime.Now}.xlsx");

        }

        [HttpGet]
        public async Task<IActionResult> ExportDoctortoPDF(DateTime From, DateTime To)
        {

            var rentalsQuery = _context.RentalCopies
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriper)
                .Include(r => r.BookCopy)
                .ThenInclude(c => c!.Book)
                .ThenInclude(r => r!.Author)
                .Where(c => c.ReturnDate >= From && c.EndDate <= To);


            var rentals = rentalsQuery.ToList();

            var tempPath = "~/Views/Reports/ReportDoctorPdf.cshtml";

            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, tempPath, rentals);

            var pdf = Pdf
                .From(html)
                .Landscape()
                .Content();

            return File(pdf.ToArray(), "application/octet-stream", $"Rentals_{DateTime.Now}.pdf");

        }

        [HttpGet]
        public IActionResult DelayedPatient()
        {
            var rentals = getDelayedRental();

            DoctorReportViewModel viewModel = new()
            {
                Rentals = PaginatedList<RentalCopy>.Create(rentals, 1, 10),
                PageMode = 2
            };

            return View(nameof(Doctors), viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportDelayedPatientToExcel(DateTime From, DateTime To, int? pageNumber)
        {
            var rentalsQuery = getDelayedRental();

            var rentals = rentalsQuery.ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Rentals");

            var headerCells = new string[] { "Subscriber ID" , "Subscriber Name" , "Subscriber Phone" ,"Author Name", "Book Title" ,
                               "Rental Date", "End Date" , "Return Date" , "Extended On" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < rentals.Count; i++)
            {
                sheet.Cell(i + 2, 1).SetValue(rentals[i].Rental!.Subscriper!.Id);
                sheet.Cell(i + 2, 2).SetValue(rentals[i].Rental!.Subscriper!.FirstName);
                sheet.Cell(i + 2, 3).SetValue(rentals[i].Rental!.Subscriper!.MobileNumber);
                sheet.Cell(i + 2, 4).SetValue(rentals[i].BookCopy!.Book!.Author!.Name);
                sheet.Cell(i + 2, 5).SetValue(rentals[i].BookCopy!.Book!.Title);
                sheet.Cell(i + 2, 6).SetValue(rentals[i].RentalDate.ToString("dd-MM-yyyy"));
                sheet.Cell(i + 2, 7).SetValue(rentals[i].EndDate.ToString("dd-MM-yyyy"));
                sheet.Cell(i + 2, 8).SetValue(rentals[i].ReturnDate?.ToString("dd-MM-yyyy"));
                sheet.Cell(i + 2, 9).SetValue(rentals[i].ExtendOn?.ToString("dd-MM-yyyy"));
            }

            sheet.AddFormate();

            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), "application/octet-stream", $"Rentals_{DateTime.Now}.xlsx");

        }

        [HttpGet]
        public async Task<IActionResult> ExportDelayedPatientToPDF(DateTime From, DateTime To)
        {

            var rentalsQuery = getDelayedRental();

            var rentals = rentalsQuery.ToList();

            var tempPath = "~/Views/Reports/ReportDoctorPdf.cshtml";

            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, tempPath, rentals);

            var pdf = Pdf
                .From(html)
                .Landscape()
                .Content();

            return File(pdf.ToArray(), "application/octet-stream", $"Rentals_{DateTime.Now}.pdf");

        }

        private IQueryable<RentalCopy> getDelayedRental()
        {
            return _context.RentalCopies
              .Include(r => r.Rental)
              .ThenInclude(r => r!.Subscriper)
              .Include(r => r.BookCopy)
              .ThenInclude(c => c!.Book)
              .ThenInclude(r => r!.Author)
              .Where(c => !c.ReturnDate.HasValue);
        }
    }
}
