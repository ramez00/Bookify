using Bookify.Web.Cloudinary;
using Bookify.Web.Services;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using System.Linq.Dynamic.Core;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly IImageService _imageService;

        private List<string> _allowedImageExtentions = new List<string> { ".jpg", ".jpeg", ".png" };
        private int _allowedImageSize = 2097152;

        public BooksController(ApplicationDbContext context, IMapper mapper,
            IWebHostEnvironment webHostEnvironment, IOptions<CloundinarySettings> cloudinary, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;

            Account acc = new()
            {
                Cloud = cloudinary.Value.Name,
                ApiKey = cloudinary.Value.APIKey,
                ApiSecret = cloudinary.Value.APISecret,
            };

            _cloudinary = new CloudinaryDotNet.Cloudinary(acc);
            _imageService = imageService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetBooks()
        {
            var skip = int.Parse(Request.Form["start"]);
            var pageSize = int.Parse(Request.Form["length"]);

            var searchParm = Request.Form["search[value]"];

            var orderIndex = Request.Form["order[0][column]"];

            var orderColumnName = Request.Form[$"columns[{orderIndex}][name]"];

            var sortDirection = Request.Form["order[0][dir]"];

            IQueryable<Book> books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if (!string.IsNullOrEmpty(searchParm))
                books = books.Where(b => b.Title.Contains(searchParm) || b.Author!.Name.Contains(searchParm));

            var recordsTotal = books.Count();

            books = books.OrderBy($"{orderColumnName} {sortDirection}");

            var data = books.Skip(skip).Take(pageSize).ToList();

            var dataMapped = _mapper.Map<IEnumerable<BookViewModel>>(data);

            return Ok(new { recoredsFiltered = recordsTotal, recordsTotal, data = dataMapped });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int Id)
        {
            var book = _context.Books.Find(Id);

            if (book is null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdateById = User.GetUserId();

            _context.SaveChanges();

            return Ok();
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", PopulateBookModal());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateBookModal(model));

            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var folderPath = $"/Images/Books/";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, folderPath, hasThumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), errorMessage!);
                    return View("Form", PopulateBookModal(model));
                }

                book.ImageUrl = $"{folderPath}{imageName}";
                book.ImageThumbUrl = $"{folderPath}thumb/{imageName}";



                //var imageExtension = Path.GetExtension(model.Image.FileName);
                //if(_allowedImageExtentions.Contains(imageExtension)
                //    && _allowedImageSize >= model.Image.Length)
                //{
                //    var imageName = $"{Guid.NewGuid()}{imageExtension}";

                //    var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", imageName);
                //    var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books/thumb", imageName);

                //    using var stream = System.IO.File.Create(path);
                //    await model.Image.CopyToAsync(stream);
                //    stream.Dispose();

                //    book.ImageUrl = $"/Images/Books/{imageName}";
                //    book.ImageThumbUrl = $"/Images/Books/thumb/{imageName}";

                //    using var image = Image.Load(model.Image.OpenReadStream());
                //    var ratio = (float)image.Width / 200;
                //    var custHeight = image.Height / ratio;
                //    image.Mutate(i => i.Resize(width: 200, height: (int)custHeight));
                //    image.Save(thumbPath);
                //    image.Dispose();

                //using var stream = model.Image.OpenReadStream() ;
                //var imgParm = new ImageUploadParams
                //{
                //    File = new FileDescription(imageName,stream),
                //    UseFilename = true
                //};

                //var res = await _cloudinary.UploadAsync(imgParm);

                //book.ImageUrl = res.SecureUrl.ToString();
                //book.ImageThumbUrl = getThumbnailUrl(res.SecureUrl.ToString());
                //book.ImagePublicId = res.PublicId.ToString();
                // }
                //else
                //{
                //    ModelState.AddModelError(nameof(model.Image),
                //        "The image should be .jpg , .png And Max. length 2 MB");
                //    return View("Form",PopulateBookModal(model));
                //}
            }

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            book.CreatedById = User.GetUserId();

            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Detials), new { Id = book.Id });
        }

        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(c => c.Id == Id);

            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateBookModal(model);

            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateBookModal(model));

            var book = _context.Books
                .Include(b => b.Categories)
                .Include(b => b.Copies)
                .SingleOrDefault(c => c.Id == model.Id);

            if (book is null)
                return NotFound();

            string ImagePublicId = null;

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                    _imageService.Delete(book.ImageUrl, book.ImageThumbUrl);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var folderPath = $"/Images/Books/";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, folderPath, hasThumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), errorMessage!);
                    return View("Form", PopulateBookModal(model));
                }

                model.ImageUrl = $"{folderPath}{imageName}";
                model.ImageThumbUrl = $"{folderPath}thumb/{imageName}";

                var imageExtension = Path.GetExtension(model.Image.FileName);

                //if (_allowedImageExtentions.Contains(imageExtension)
                //    && _allowedImageSize >= model.Image.Length)
                //{
                //              if (book.ImageUrl is not null)
                //              {
                //_imageService.Delete(book.ImageUrl, book.ImageThumbUrl);

                //var oldPath = $"{_webHostEnvironment.WebRootPath}{book.ImageUrl}";
                //                  var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{book.ImageThumbUrl}";


                //                  if (System.IO.File.Exists(oldPath))
                //                      System.IO.File.Delete(oldPath);

                //                  if (System.IO.File.Exists(oldThumbPath))
                //                      System.IO.File.Delete(oldThumbPath);


                //await _cloudinary.DeleteResourcesAsync(book.ImagePublicId!);
                //}

                //var imageName = $"{Guid.NewGuid()}{imageExtension}";

                //using var stream = model.Image.OpenReadStream();
                //var imgParm = new ImageUploadParams
                //{
                //    File = new FileDescription(imageName, stream),
                //    UseFilename = true
                //};

                //var res = await _cloudinary.UploadAsync(imgParm);

                //model.ImageUrl = res.SecureUrl.ToString();
                //model.ImageThumbUrl = getThumbnailUrl(res.SecureUrl.ToString());
                //ImagePublicId = res.PublicId.ToString();



                //var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", imageName);
                //var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books/thumb", imageName);

                //using var stream = System.IO.File.Create(path);
                //await model.Image.CopyToAsync(stream);
                //stream.Dispose();

                //model.ImageUrl = $"/Images/Books/{imageName}";
                //model.ImageThumbUrl = $"/Images/Books/thumb/{imageName}";

                //using var image = Image.Load(model.Image.OpenReadStream());
                //var ratio = (float)image.Width / 200;
                //var custHeight = image.Height / ratio;
                //image.Mutate(i => i.Resize(width: 200, height: (int)custHeight));
                //image.Save(thumbPath);
                //image.Dispose();

                //}
                //else
                //{
                //    ModelState.AddModelError(nameof(model.Image),
                //        "The image should be .jpg , .png And Max. length 2 MB");
                //    return View("Form", PopulateBookModal(model));
                //}

            }
            else if (model.Image is null && book.ImageUrl is not null)
            {
                model.ImageUrl = book.ImageUrl;
                model.ImageThumbUrl = book.ImageThumbUrl;
            }

            book = _mapper.Map(model, book);

            if (!book.IsAvailableForRent)
                foreach (var copy in book.Copies)
                    copy.IsAvailableForRental = false;

            book.ImagePublicId = ImagePublicId;
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdateById = User.GetUserId();

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _context.SaveChanges();

            return RedirectToAction(nameof(Detials), new { Id = book.Id });
        }

        [HttpGet]
        public IActionResult Detials(int Id)
        {
            var book = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .SingleOrDefault(b => b.Id == Id);

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);

            return View(viewModel);
        }

        public IActionResult IsExist(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);

            var isValid = book is null || book.Id.Equals(model.Id);

            return Json(isValid);
        }
        private BookFormViewModel PopulateBookModal(BookFormViewModel? modal = null)
        {
            var viewModal = modal is null ? new BookFormViewModel() : modal;

            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();

            viewModal.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModal.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

            return viewModal;
        }


        private string getThumbnailUrl(string imgUrl)
        {

            var thumb = "/c_thumb,w_200,g_face";
            var seprator = "/image/upload";

            var url = imgUrl.Split(seprator);
            return $"{url[0]}{seprator}{thumb}{url[1]}";
        }
    }
}
