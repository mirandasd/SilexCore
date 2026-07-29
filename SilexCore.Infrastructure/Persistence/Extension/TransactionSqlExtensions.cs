using System.Data;
using Dapper;

namespace SilexCore.Infrastructure.Persistence.Extensions;

public static class TransactionSqlExtensions
{
    public static async Task<(int RowsAffected, string Mensaje)> ExecuteSpWithOutputAsync(
        this IDbTransaction transaction, string spName, object parameters)
    {
        var p = new DynamicParameters(parameters);
        p.Add("RowsAffected", dbType: DbType.Int32, direction: ParameterDirection.Output);
        p.Add("Mensaje", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

        await transaction.Connection!.ExecuteAsync(
            spName, p, transaction: transaction, commandType: CommandType.StoredProcedure);

        return (p.Get<int>("RowsAffected"), p.Get<string>("Mensaje"));
    }

    public static Task ExecuteSpAsync(
        this IDbTransaction transaction, string spName, object parameters)
        => transaction.Connection!.ExecuteAsync(
            spName, parameters, transaction: transaction, commandType: CommandType.StoredProcedure);
}