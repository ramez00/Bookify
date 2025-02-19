using HashidsNet;

namespace Bookify.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IHashids _hashIds;
        private readonly IMapper _mapper;

        public SearchController(ApplicationDbContext context, IHashids hashids, IMapper mapper)
        {
            _context = context;
            _hashIds = hashids;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(string bKey)
        {
            var bookId = _hashIds.DecodeHex(bKey);
            var book = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .SingleOrDefault(b => b.Id == int.Parse(bookId));

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);

            return View(viewModel);
        }

        public IActionResult Find(string query)
        {
            var books = _context.Books
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted && (b.Title.Contains(query) || b.Author!.Name.Contains(query)))
                .Select(b => new { b.Title, author = b.Author!.Name, key = _hashIds.EncodeHex(b.Id.ToString()) })
                .ToList();

            return Ok(books);
        }
    }
}
