using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Exam.Application.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string folderPath, CancellationToken cancellationToken = default);
        Task DeleteAsync(string fileUrl);
        string GetUrl(string relativePath);
    }
}
