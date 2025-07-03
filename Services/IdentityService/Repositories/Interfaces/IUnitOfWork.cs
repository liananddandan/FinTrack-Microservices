namespace IdentityService.Repositories.Interfaces;

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    Task WithTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    Task<T> WithTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}