namespace GrowMate.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        IEmailVerificationRepository EmailVerifications { get; }
        IPostRepository Posts { get; }
        IFarmerRepository Farmers { get; }
        IMediaRepository Media { get; }
        ITreeListingRepository TreeListings { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
    }
}
