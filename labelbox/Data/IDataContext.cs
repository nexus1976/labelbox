using Microsoft.EntityFrameworkCore;

namespace labelbox.Data
{
    public interface IDataContext
    {
        DbSet<Asset> Assets { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}