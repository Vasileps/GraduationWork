namespace MessengerServer.Database.Interfaces
{
    public interface IFileMetadataRepository : IRepository<FileMetadata>
    {
        public Task<FileMetadata?> GetByPathAsync(string path);
    }
}
