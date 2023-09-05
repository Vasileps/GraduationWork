using MessengerServer.Database.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Database.Repositories
{
    public class FileMetadataRepository : Repository<FileMetadata>, IFileMetadataRepository
    {
        public FileMetadataRepository(DBContext context) : base(context.FilesMetadata) { }

        public async Task<FileMetadata?> GetByPathAsync(string path)
        {
            var data = await collection.FindAsync(Builders<FileMetadata>.Filter.Eq(x=> x.RelativePath, path));
            return data.SingleOrDefault();
        }
    }
}
