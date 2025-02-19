namespace Bookify.Domain.Constants
{
    public static class RegexPattern
    {
        public const string PasswordPattern = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";
        public const string UserNamePattern = "^[a-zA-Z0-9-.@+]*$";
        public const string EnglishCharaters = "^[a-zA-Z  ]*$";
        public const string MobileNumber = "^01[0,1,2,5]{1}[0-9]{8}$";
        public const string asd = "^[2,3][0-9]{13}$";
        public const string NationalID = "^[2,3][0-9]{13}$";
    }
}
