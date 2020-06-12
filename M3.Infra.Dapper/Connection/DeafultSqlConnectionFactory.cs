using Repository.Dapper.Contracts;
using System.Data;
using System.Data.SqlClient;

namespace Repository.Dapper.Connection
{
    public class DeafultSqlConnectionFactory : IConnectionFactory
    {
        public IDbConnection Connection()
        {
            return new SqlConnection("Database=HeroDB;Data Source=(localdb)\\MSSQLLocalDB;");
        }
    }
}
