using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Bookify.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<string> _allowedImageExtentions = new List<string> { ".jpg", ".jpeg", ".png" };
        private int _allowedImageSize = 2097152;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<(bool isUploaded, string? ErrorMessage)> UploadAsync(IFormFile image,
            string imageName, string folderPath, bool hasThumbnail)
        {
            if (image.Length >= _allowedImageSize)
                return (isUploaded: false, ErrorMessage: Errors.InvalidImageSize);

            if (!_allowedImageExtentions.Contains(Path.GetExtension(image.FileName)))
                return (isUploaded: false, ErrorMessage: Errors.InvalidImageType);


            var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);
            var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/thumb", imageName);

            using var stream = System.IO.File.Create(path);
            await image.CopyToAsync(stream);
            stream.Dispose();

            var ImageUrl = $"{folderPath}/{imageName}";


            if (hasThumbnail)
            {
                var ImageThumbUrl = $"{folderPath}/thumb/{imageName}";
                using var LoadedImage = Image.Load(image.OpenReadStream());
                var ratio = (float)LoadedImage.Width / 200;
                var custHeight = LoadedImage.Height / ratio;
                LoadedImage.Mutate(i => i.Resize(width: 200, height: (int)custHeight));
                LoadedImage.Save(thumbPath);
                LoadedImage.Dispose();
            }

            return (isUploaded: true, ErrorMessage: null);
        }

        public void Delete(string imagePath, string? imageThumbnailPath)
        {
            var oldPath = $"{_webHostEnvironment.WebRootPath}{imagePath}";

            if (File.Exists(oldPath))
                File.Delete(oldPath);

            if (!string.IsNullOrEmpty(imageThumbnailPath))
            {
                var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{imageThumbnailPath}";
                if (File.Exists(oldThumbPath))
                    File.Delete(oldThumbPath);
            }
        }
    }
}
