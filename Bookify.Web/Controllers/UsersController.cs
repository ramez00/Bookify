using Bookify.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public UsersController(UserManager<ApplicationUser> userManager,
            IMapper mapper,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }
        // https://res.cloudinary.com/masl7a/image/upload/v1711145383/Actve_hasv3x.png

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var viewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);

            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            UserFormViewModel viewModel = new()
            {
                Roles = await _roleManager.Roles
                                .Select(r => new SelectListItem
                                {
                                    Text = r.Name,
                                    Value = r.Name
                                })
                                .ToListAsync(),
            };

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            ApplicationUser user = new()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.UserName,
                CreatedById = User.GetUserId()
            };
            var res = await _userManager.CreateAsync(user, model.Password);
            if (res.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code },
                    protocol: Request.Scheme);

                var placeHolders = new Dictionary<string, string>()
                {
                    { "imageUrl","https://res.cloudinary.com/masl7a/image/upload/v1711145383/Actve_hasv3x.png"},
                    { "header",$"Hey {user.FullName}, Thanks for joining us" },
                    { "body","Please Confirm your Email"},
                    { "url",$"{HtmlEncoder.Default.Encode(callbackUrl!)}"},
                    { "linkTitle","Active Account" }
                };

                var body = _emailBodyBuilder.getBuilder(EmailTemplates.Email, placeHolders);

                await _emailSender.SendEmailAsync("eng.ramez.mohamed@gmail.com", "Confirm your email", body);

                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }
            return BadRequest(string.Join(",", res.Errors.Select(e => e.Description)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedOn = DateTime.Now;
            user.LastUpdateById = User.GetUserId();

            await _userManager.UpdateAsync(user);

            if (user.IsDeleted)
                await _userManager.UpdateSecurityStampAsync(user);

            return Ok(user.LastUpdatedOn.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
                await _userManager.SetLockoutEndDateAsync(user, null);

            return Ok(DateTime.Now.ToString());
        }


        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user is null)
                return NotFound();

            var viewModel = new ResetPasswordViewModel() { Id = Id };

            return PartialView("_ResetPasswordForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            var CurrentPassword = user.PasswordHash;

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, model.Password);

            if (result.Succeeded)
            {
                user.LastUpdatedOn = DateTime.Now;
                user.LastUpdateById = User.GetUserId();
                user.IsDeleted = false;

                await _userManager.UpdateAsync(user);

                //var viewModel = _mapper.Map<UserViewModel>(user);
                //return PartialView("_UserRow", viewModel);
                return Ok(user.LastUpdatedOn.ToString());
            }

            user.PasswordHash = CurrentPassword;
            await _userManager.UpdateAsync(user);

            return BadRequest(string.Join(",", result.Errors.Select(e => e.Description)));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user is null)
                return NotFound();

            var viewModel = _mapper.Map<UserFormViewModel>(user);

            viewModel.SelectedRoles = await _userManager.GetRolesAsync(user);

            viewModel.Roles = await _roleManager.Roles
                                .Select(r => new SelectListItem
                                {
                                    Text = r.Name,
                                    Value = r.Name
                                })
                                .ToListAsync();

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            user = _mapper.Map(model, user);  // replace values From viewModel to application user to Update

            user.LastUpdateById = User.GetUserId();
            user.LastUpdatedOn = DateTime.Now;

            //var oldAssignedRole = await _userManager.GetRolesAsync(user);

            //if (oldAssignedRole is null)
            //    await _userManager.AddToRolesAsync(user, model.SelectedRoles);

            //await _userManager.RemoveFromRolesAsync(user, oldAssignedRole);

            var res = await _userManager.UpdateAsync(user);
            if (res.Succeeded)
            {
                var CurrentRoles = await _userManager.GetRolesAsync(user);

                var UpdatedRoles = !CurrentRoles.SequenceEqual(model.SelectedRoles);  // comparing between Selected Roles and current Role

                if (UpdatedRoles)
                {
                    await _userManager.RemoveFromRolesAsync(user, CurrentRoles);
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }
                // Once Update Roles On User action Taken to signOut

                await _userManager.UpdateSecurityStampAsync(user);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(",", res.Errors.Select(e => e.Description)));
        }

        public async Task<IActionResult> IsExist(UserFormViewModel model)
        {
            ApplicationUser user = new();

            if (model.Email is null)
                user = await _userManager.FindByNameAsync(model.UserName);
            else
                user = await _userManager.FindByEmailAsync(model.Email);

            var isValid = user is null || user.Id.Equals(model.Id);

            return Json(isValid);
        }
    }
}
