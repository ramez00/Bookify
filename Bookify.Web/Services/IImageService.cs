namespace Bookify.Web.Services
{
    public interface IImageService
    {
        public Task<(bool isUploaded, string? ErrorMessage)> UploadAsync(IFormFile image,
            string imageName, string folderPath, bool hasThumbnail);

        public void Delete(string imagePath, string? imageThumbnailPath = null);
    }
}
