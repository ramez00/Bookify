using Bookify.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataProtector _dataProtector;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _iemailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public SubscribersController(ApplicationDbContext context, IDataProtectionProvider dataProtection, IWhatsAppService whatsAppService
            , IMapper mapper, IImageService imageService, IEmailBodyBuilder iemailBodyBuilder, IEmailSender emailSender)
        {
            _context = context;
            _dataProtector = dataProtection.CreateProtector("ProtectValue");
            _whatsAppService = whatsAppService;
            _mapper = mapper;
            _imageService = imageService;
            _iemailBodyBuilder = iemailBodyBuilder;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create", PopulateSubscribersModal());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            var subscriber = _context.Subscripers
                               .FirstOrDefault(s => s.Email == model.Value
                               || s.MobileNumber == model.Value
                               || s.NationalId == model.Value);

            var viewModel = _mapper.Map<SubscriberViewMOdel>(subscriber);

            if (subscriber is not null)
                viewModel.Id = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscribersFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Create), PopulateSubscribersModal());

            var subscriber = _mapper.Map<Subscriper>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
            var folderPath = $"/Images/Subscribers/";

            var (isUploaded, error) = await _imageService.UploadAsync(model.Image!, imageName, folderPath, hasThumbnail: true);

            if (!isUploaded)
            {
                ModelState.AddModelError(nameof(model.Image), error!);
                return View(nameof(Create), PopulateSubscribersModal(model));
            }

            subscriber.ImageUrl = $"{folderPath}/{imageName}";
            subscriber.ImageThumbnailUrl = $"{folderPath}/thumb/{imageName}";

            subscriber.CreatedById = User.GetUserId();

            Subscription subscription = new()
            {
                CreatedById = subscriber.CreatedById,
                CreatedOn = subscriber.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(15),
            };

            subscriber.Subscriptions.Add(subscription);

            _context.Subscripers.Add(subscriber);
            _context.SaveChanges();

            if (subscriber.HasWhatsApp)
                BackgroundJob.Enqueue(() => _whatsAppService.sendMessage("01000655438", WhatsAppTemplates.WelcomeMsg, subscriber.FirstName));

            var placeHolder = new Dictionary<string, string>()
            {
                {"imageUrl","https://res.cloudinary.com/masl7a/image/upload/v1711145383/Actve_hasv3x.png" },
                {"header",$"Welcome {model.FirstName} ," },
                {"body","Thanks for joining Clinify " },
            };

            var body = _iemailBodyBuilder.getBuilder(EmailTemplates.Notification, placeHolder);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                   "eng.ramez.mohamed@gmail.com",
                   "Welcome email", body));

            var subscirberId = _dataProtector.Protect(subscriber.Id.ToString());

            return RedirectToAction(nameof(Detials), new { id = subscirberId });
        }

        [HttpGet]
        public IActionResult Edit(string Id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(Id));

            var sub = _context.Subscripers.Find(subscriberId);

            if (sub is null)
                return NotFound();

            var model = _mapper.Map<SubscribersFormViewModel>(sub);
            model.Key = Id;

            return View("Create", PopulateSubscribersModal(model));
        }

        [HttpGet]
        public IActionResult Detials(string Id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(Id));

            var subscriber = _context.Subscripers
                .Include(s => s.Governorate)
                .Include(s => s.Area)
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            var model = _mapper.Map<SubscriberModel>(subscriber);
            model.Key = Id;

            return View(nameof(Detials), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RenewSubscription(string subKey)
        {
            var subId = int.Parse(_dataProtector.Unprotect(subKey));

            var Subscriper = _context.Subscripers
                                     .Include(s => s.Subscriptions)
                                     .SingleOrDefault(s => s.Id == subId);

            if (Subscriper is null)
                return NotFound();

            if (Subscriper.IsBlackListed)
                return BadRequest();

            var lastSubscription = Subscriper.Subscriptions.Count() > 0 ? Subscriper.Subscriptions.Last() : new Subscription();

            var startDate = lastSubscription.EndDate > DateTime.Today ? lastSubscription.EndDate.AddDays(1) : DateTime.Today;

            var newSubscription = new Subscription()
            {
                CreatedById = User.GetUserId(),
                CreatedOn = DateTime.Now,
                StartDate = startDate,
                EndDate = startDate.AddDays(10),
            };

            Subscriper.Subscriptions.Add(newSubscription);
            _context.SaveChanges();

            var placeHolder = new Dictionary<string, string>()
            {
                {"imageUrl","https://res.cloudinary.com/masl7a/image/upload/v1714036436/good-news-announcement_pu1w8j.jpg" },
                {"header",$"Hello {Subscriper.FirstName} ," },
                {"body",$"your subscribtion has been renewed through {newSubscription.EndDate.ToString("dd,MMM , yyyy")}." },
            };

            var body = _iemailBodyBuilder.getBuilder(EmailTemplates.RenewForm, placeHolder);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                   "eng.ramez.mohamed@gmail.com",
                   "Renew Subscribtion", body));

            var viewModel = _mapper.Map<SubscribtionViewModel>(newSubscription);

            return PartialView("_SubscriptionRow", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscribersFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Create), PopulateSubscribersModal());

            var subscriberID = 0;

            if (model.Key is not null)
                subscriberID = int.Parse(_dataProtector.Unprotect(model.Key.ToString()));

            var subscriber = _context.Subscripers.Find(subscriberID);

            if (subscriber is null)
                return NotFound();

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(subscriber.ImageUrl))
                    _imageService.Delete(subscriber.ImageUrl, subscriber.ImageThumbnailUrl);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
                var folderPath = $"/Images/Subscribers/";

                var (isUploaded, error) = await _imageService.UploadAsync(model.Image!, imageName, folderPath, hasThumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), error!);
                    return View(nameof(Create), PopulateSubscribersModal(model));
                }

                model.ImageUrl = $"{folderPath}/{imageName}";
                model.ImageThumbnailUrl = $"{folderPath}/thumb/{imageName}";
            }
            else if (!string.IsNullOrEmpty(subscriber.ImageUrl))
            {
                model.ImageUrl = subscriber.ImageUrl;
                model.ImageThumbnailUrl = subscriber.ImageThumbnailUrl;
            }

            subscriber = _mapper.Map(model, subscriber);

            subscriber.LastUpdateById = User.GetUserId();
            subscriber.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            var subscirberId = _dataProtector.Protect(subscriber.Id.ToString());

            return RedirectToAction(nameof(Detials), new { id = subscirberId });
        }

        private SubscribersFormViewModel PopulateSubscribersModal(SubscribersFormViewModel? modal = null)
        {
            var viewModal = modal is null ? new SubscribersFormViewModel() : modal;

            var Governators = _context.Governorates.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();

            viewModal.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(Governators);

            if (viewModal.GovernorateId > 0)
            {
                var area = _context.Areas
                    .Where(a => a.GovernorateId == viewModal.GovernorateId)
                    .OrderBy(area => area.Name)
                    .ToList();

                viewModal.Areas = _mapper.Map<IEnumerable<SelectListItem>>(area);
            }

            return viewModal;
        }

        [AjaxOnly]
        public IActionResult GetAreas(int GovId)
        {
            var Areas = _context.Areas
                .Where(a => a.GovernorateId == GovId)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .OrderBy(a => a.Text)
                .ToList();

            return Ok(Areas);
        }

        public IActionResult IsEmailExist(SubscribersFormViewModel model)
        {
            var id = 0;
            if (model.Key is not null)
                id = int.Parse(_dataProtector.Unprotect(model.Key));

            var subsciper = _context.Subscripers
                .SingleOrDefault(b => b.Email == model.Email);

            var isValid = subsciper is null || subsciper.Id.Equals(id);

            return Json(isValid);
        }

        public IActionResult IsNationalIdExist(SubscribersFormViewModel model)
        {
            var id = 0;
            if (model.Key is not null)
                id = int.Parse(_dataProtector.Unprotect(model.Key));

            var subsciper = _context.Subscripers
                .SingleOrDefault(b => b.NationalId == model.NationalId);

            var isValid = subsciper is null || subsciper.Id.Equals(id);

            return Json(isValid);
        }

        public IActionResult IsMobileExist(SubscribersFormViewModel model)
        {
            var id = 0;
            if (model.Key is not null)
                id = int.Parse(_dataProtector.Unprotect(model.Key));

            var subsciper = _context.Subscripers
                .SingleOrDefault(b => b.MobileNumber == model.MobileNumber);

            var isValid = subsciper is null || subsciper.Id.Equals(id);

            return Json(isValid);
        }
    }
}
