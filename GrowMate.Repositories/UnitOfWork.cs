using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;


namespace GrowMate.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EXE201_GrowMateContext _dbContext;

        public UnitOfWork(
            EXE201_GrowMateContext dbContext,
            IUserRepository users,
            ICustomerRepository customers,
            IEmailVerificationRepository emailVerifications,
            IPostRepository posts,
            IFarmerRepository farmers,
            IMediaRepository media,
            ITreeListingRepository treeListing,
            IProductRepository products
        )
        {
            _dbContext = dbContext;
            Users = users;
            Customers = customers;
            EmailVerifications = emailVerifications;
            Posts = posts;
            Farmers = farmers;
            Media = media;
            TreeListings = treeListing;
            Products = products;
        }

        public IUserRepository Users { get; }
        public ICustomerRepository Customers { get; }
        public IEmailVerificationRepository EmailVerifications { get; }
        public IPostRepository Posts { get; }
        public IFarmerRepository Farmers { get; }
        public IMediaRepository Media { get; }
        public ITreeListingRepository TreeListings { get; }
        public IProductRepository Products { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _dbContext.SaveChangesAsync(ct);

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
        {
            await using IDbContextTransaction tx = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                await action(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}
