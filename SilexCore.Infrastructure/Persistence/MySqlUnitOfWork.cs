using Microsoft.AspNetCore.Connections;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Interfaces.Application;
using System.Data;

namespace SilexCore.Infrastructure.Persistence;

public sealed class MySqlUnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private bool _completed;

    public IDbTransaction Transaction { get; }

    public MySqlUnitOfWork(IDbConnection connection)
    {
        _connection = connection;
        Transaction = connection.BeginTransaction();
    }

    public Task CommitAsync()
    {
        Transaction.Commit();
        _completed = true;
        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        if (!_completed) Transaction.Rollback();
        _completed = true;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (!_completed) Transaction.Rollback();
        Transaction.Dispose();
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}

public sealed class MySqlUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IConnectionFactory _connectionFactory; // lo que ya use tu nuget internamente

    public MySqlUnitOfWorkFactory(IConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IUnitOfWork> CreateAsync(string connectionName = ConnectionNames.Default)
    {
        var connection = await _connectionFactory.CreateOpenConnectionAsync(connectionName);
        return new MySqlUnitOfWork(connection);
    }
}
