using HashidsNet;
using Microsoft.AspNetCore.DataProtection;
using System.Diagnostics;

namespace Bookify.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;
        private readonly IHashids _hashids;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,
            IMapper mapper, IDataProtectionProvider dataProtection, IHashids hashids)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtection.CreateProtector("ProtectValue");
            _hashids = hashids;
        }

        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("index", "DashBoard");

            var lastAddedBooks = _context.Books
                                    .Include(b => b.Author)
                                    .Where(b => !b.IsDeleted)
                                    .OrderByDescending(b => b.Id)
                                    .Take(10)
                                    .ToList();

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks);

            foreach (var book in viewModel)
                book.key = _hashids.EncodeHex(book.Id.ToString());

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
