namespace MessengerServer.Services.Interfaces
{
    public interface IFilesProvider
    {
        public Task<Stream?> GetFileAsync(string relativePath);

        public Task UpdateFileAsync(Stream file, string relativePath);

        public Task DeleteFileAsync(string relativePath);
    }
}
