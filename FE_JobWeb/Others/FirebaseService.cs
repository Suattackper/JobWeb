using Google.Cloud.Storage.V1;
using System.Web;

namespace FE_JobWeb.Others
{
    public class FirebaseService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName = "movieapp-2f052.appspot.com";

        public FirebaseService()
        {
            _storageClient = StorageClient.Create();
        }

        public async Task<string> UploadImageToFirebaseAsync(Stream fileStream, string fileName, string randomname, string type)
        {
            string contentType;
            var extension = Path.GetExtension(fileName)?.ToLower();
            if (extension == ".jpg" || extension == ".jpeg")
            {
                contentType = "image/jpeg";
            }
            else if (extension == ".png")
            {
                contentType = "image/png";
            }
            else if (extension == ".pdf")
            {
                contentType = "application/pdf";
            }
            else
            {
                return "error";
            }

            string typroffile = "";
            if (type == "user") typroffile = "JobWeb/ImageUsers/";
            else if (type == "company") typroffile = "JobWeb/Company/";
            else typroffile = "JobWeb/Resume/";

            var imageObject = await _storageClient.UploadObjectAsync(
                bucket: _bucketName,
                objectName: typroffile + randomname,
                contentType: contentType,
                source: fileStream);

            // Create the file URL directly
            string downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(typroffile + randomname)}?alt=media";

            return downloadUrl; // Return the URL to display the image
        }
        public async Task<bool> DeleteImageFromFirebaseAsync(string fileName)
        {
            try
            {
                // Delete the file from Firebase Storage
                await _storageClient.DeleteObjectAsync(
                    bucket: _bucketName,
                    objectName: "JobWeb/ImageUsers/" + fileName);

                return true; // Return true if the file is deleted successfully
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during deletion
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false; // Return false if there is an error
            }
        }
        public string ExtractFileNameFromUrl(string fileUrl)
        {
            // Extract the object name part from the URL
            Uri uri = new Uri(fileUrl);
            var pathSegments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Kiểm tra xem URL có đúng định dạng không
            if (pathSegments.Length >= 2)
            {
                // Đoạn thứ hai trong URL sẽ chứa tên file (sau "o/")
                string encodedFileName = pathSegments[pathSegments.Length - 1];

                // Giải mã tên file đã mã hóa
                string fileName = Uri.UnescapeDataString(encodedFileName);

                return fileName;
            }

            return null; // Trả về null nếu URL không hợp lệ hoặc không thể trích xuất tên file
        }

    }
}
