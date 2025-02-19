using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class RentalReturnFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Is Penalites Paid ?"),
            AssertThat("(TotalDelayInDays == 0 && IsPenalitesPaied == false) || IsPenalitesPaied == true", ErrorMessage = Errors.PenaltiesPaid)]
        public bool IsPenalitesPaied { get; set; }
        public IList<RentalCopyViewModel> Copies { get; set; } = new List<RentalCopyViewModel>();
        public List<ReturnCopyOption> SelectedCopies { get; set; } = new();
        public bool AllowExtend { get; set; }
        public int TotalDelayInDays
        {
            get
            {
                return Copies.Sum(c => c.DelayInDays);
            }
        }
    }
}
