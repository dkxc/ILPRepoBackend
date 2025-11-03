using Supabase;
using Microsoft.Extensions.Configuration;
using IlpRepoBackend.Domain.Interfaces;

namespace IlpRepoBackend.Infrastructure.Services
{
    public class SupabaseStorageService : IFileStorageService
    {
        private readonly Client _supabaseClient;
        private readonly string _bucketName = "docs";

        public SupabaseStorageService(IConfiguration configuration)
        {
            var supabaseUrl = "https://jbbaufdzfgglkjqiwveb.supabase.co";
            var supabaseKey = "sb_secret_CQVrWmE04g3hQtqIGQ_xhA_d0KztkR5";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            _supabaseClient = new Client(supabaseUrl, supabaseKey, options);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Generate unique filename to avoid conflicts
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                
                // Convert stream to byte array
                var bytes = new byte[fileStream.Length];
                await fileStream.ReadAsync(bytes, 0, (int)fileStream.Length);

                // Upload to Supabase Storage
                await _supabaseClient.Storage
                    .From(_bucketName)
                    .Upload(bytes, uniqueFileName, new Supabase.Storage.FileOptions
                    {
                        ContentType = contentType,
                        Upsert = true // This allows replacing existing files
                    });

                // Get the public URL
                var publicUrl = _supabaseClient.Storage
                    .From(_bucketName)
                    .GetPublicUrl(uniqueFileName);

                return publicUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to Supabase: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                await _supabaseClient.Storage
                    .From(_bucketName)
                    .Remove(fileName);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string fileName)
        {
            try
            {
                var publicUrl = _supabaseClient.Storage
                    .From(_bucketName)
                    .GetPublicUrl(fileName);

                return publicUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get file URL: {ex.Message}", ex);
            }
        }
    }
}