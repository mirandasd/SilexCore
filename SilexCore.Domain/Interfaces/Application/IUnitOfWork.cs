using SilexCore.Domain.Constants;
using System.Data;

namespace SilexCore.Domain.Interfaces.Application;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbTransaction Transaction { get; }
    Task CommitAsync();
    Task RollbackAsync();
}

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> CreateAsync(string connectionName = ConnectionNames.Default);
}