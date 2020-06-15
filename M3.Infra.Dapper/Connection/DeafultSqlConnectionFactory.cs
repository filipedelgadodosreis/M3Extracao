using Repository.Dapper.Contracts;
using System.Data;
using System.Data.SqlClient;

namespace Repository.Dapper.Connection
{
    public class DeafultSqlConnectionFactory : IConnectionFactory
    {
        public IDbConnection Connection()
        {
            return new SqlConnection("Server=187.84.234.99;Database=dbMGIAdmin;User ID=userArena;Password=arena@2020");
        }
    }
}
