using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace FamiHub.API.Services
{
    public class FileService
    {
        private readonly Cloudinary _cloudinary;

        public FileService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty");

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "famihub_uploads",
                // Nén ảnh trên Cloudinary để giảm dung lượng lưu trữ và tăng tốc độ tải về
                Transformation = new Transformation().Quality("auto").FetchFormat("auto").Width(1080).Crop("limit")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }
    }
}
