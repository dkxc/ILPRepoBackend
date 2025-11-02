namespace IlpRepoBackend.Domain.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<bool> DeleteFileAsync(string fileName);
        Task<string> GetFileUrlAsync(string fileName);
    }
}