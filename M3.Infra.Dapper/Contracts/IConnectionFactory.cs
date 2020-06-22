using System.Data;

namespace Repository.Dapper.Contracts
{
    public interface IConnectionFactory
    {
        IDbConnection Connection(string connectionString);
    }
}
