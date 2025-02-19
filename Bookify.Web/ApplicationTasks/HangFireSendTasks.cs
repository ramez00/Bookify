using Bookify.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Bookify.Web.ApplicationTasks
{
    public class HangFireSendTasks
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailBodyBuilder _iemailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public HangFireSendTasks(ApplicationDbContext context
            , IEmailBodyBuilder iemailBodyBuilder, IEmailSender emailSender)
        {
            _context = context;
            _iemailBodyBuilder = iemailBodyBuilder;
            _emailSender = emailSender;
        }

        [Obsolete]
        public async Task PrepareSubscriberAlert()
        {
            var subscribers = _context.Subscripers
                            .Include(s => s.Subscriptions)
                            .Where(s => !s.IsBlackListed && s.Subscriptions.OrderByDescending(s => s.EndDate).First().EndDate < DateTime.Today.AddDays(5))
                            .ToList();

            foreach (var subscriber in subscribers)
            {
                var endDate = subscriber.Subscriptions.Last().EndDate;

                var placeHolder = new Dictionary<string, string>()
                {
                    {"imageUrl","https://res.cloudinary.com/masl7a/image/upload/v1714119759/alert-notification-icon_gcc7go.png" },
                    {"header",$"Hello {subscriber.FirstName} ," },
                    {"body",$"your subscribtion will be expired by {endDate.ToString("dd,MMM , yyyy")} 😔 ." },
                };

                var body = _iemailBodyBuilder.getBuilder(EmailTemplates.AlertMail, placeHolder);

                await _emailSender.SendEmailAsync("eng.ramez.mohamed@gmail.com", "Clinify Expiration Alert", body);
            }
        }

        [Obsolete]
        public async Task PrepareSubscriberRentalExpirationAlert()
        {
            var rent = _context.Rentals
                    .Include(r => r.Subscriper)
                    .Include(r => r.RentalCopies)
                    .ThenInclude(c => c.BookCopy)
                    .ThenInclude(bc => bc!.Book)
                    .Where(r => r.RentalCopies.Any(r => r.EndDate.Date == DateTime.Today.AddDays(1)))
                    .ToList();

            var fesa = _context.Rentals
                .Include(r => r.RentalCopies)
                .Include(s => s.Subscriper)
                .Where(r => r.RentalCopies.Any(c => c.EndDate == DateTime.Today.AddDays(1) && !c.ReturnDate.HasValue) && !r.IsDeleted)
                .GroupBy(r => r.Subscriper)
                .ToList();

            var f = string.Empty;

            var rentals = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .Include(r => r.Subscriper)
                //.ThenInclude(bc => bc!.Book)
                .Where(r => r.RentalCopies.Any(c => c.EndDate.Date == DateTime.Today.AddDays(1) && !c.ReturnDate.HasValue) && !r.IsDeleted)
                .ToList();


            foreach (var rental in rentals)
            {
                var form = "";
                var sub = rental.Subscriper!.FirstName;
                var subEmail = rental.Subscriper!.Email;
                form = form + $"Hi {sub},";

                foreach (var c in rental.RentalCopies)
                {
                    var ser = c.BookCopy!.SerialNumber;
                    var retDate = c.EndDate;
                    form = form + $" \r\n This book with Serial Number - {ser} return Date should be {retDate.ToString("dd,MMM , yyyy")}";
                }

                var placeHolder = new Dictionary<string, string>()
                    {
                        { "imageUrl","https://res.cloudinary.com/masl7a/image/upload/v1714119759/alert-notification-icon_gcc7go.png" },
                        { "header",$"Hello {sub} ," },
                        { "body",$" {form} 😔 ." },
                    };

                var body = _iemailBodyBuilder.getBuilder(EmailTemplates.AlertMail, placeHolder);

                await _emailSender.SendEmailAsync("eng.ramez.mohamed@gmail.com", "Clinify Expiration Alert", body);

            }

        }
    }
}
