namespace Bookify.Domain.Constants
{
    public static class Errors
    {
        public const string EmptyImage = "Image Required";
        public const string InvalidNationalID = "National ID should be 14 Digits and Start with 2 Or 3 ";
        public const string InvalidDate = "Date cannot be in the future!";
        public const string InvalidImageSize = "Max. length 2 MB";
        public const string InvalidImageType = "The image should be .jpg , .png";
        public const string InvalidMobileNumber = "Invalid mobile number";
        public const string RequiredField = "Required field";
        public const string InvalidUserName = "The {0} can only contain letters or digits!";
        public const string InvalidEngChar = "The {0} can only contain letters only!";
        public const string IsExist = "The {0} is already exist!";
        public const string MaxLength = "Length can not be more than {1} char. ";
        public const string MaxMinLength = "The {0} must be at least {2} characters and at {1} characters long.";
        public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";
        public const string WeakPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long";
        public const string InvalidSerialNumber = "Invalid serial number";
        public const string NotAvliableForRental = "not avliable for rental";
        public const string BlackListedSubscriber = "This subscriber is BlackListed";
        public const string InactiveSubscriber = "This subscriber is inactive";
        public const string MaximumReachedSubscriber = "This subscriber has maximum Reached of Rental";
        public const string CopyIsRentaled = "This Copy of book already Rentaled";
        public const string SubsucriberNotAllowedToExtend = "Can not Extend Once Renewal ur Membership";
        public const string RentalInvalid = "Can not Extend for this Rental.";
        public const string PenaltiesPaid = "You should paied first.";
    }
}
