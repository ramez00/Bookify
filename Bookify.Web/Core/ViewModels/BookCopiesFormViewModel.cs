namespace Bookify.Web.Core.ViewModels
{
    public class BookCopiesFormViewModel
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public bool IsAvailableForRental { get; set; }

        [Display(Name = "Edition Number")]
        [Range(1, 1000, ErrorMessage = "Edition should be in between 1 to 1000!")]
        public int EditionNumber { get; set; }

        [Display(Name = "Is Avaliable For Rental")]
        public bool IsBookAvaliableForRental { get; set; }

    }
}
