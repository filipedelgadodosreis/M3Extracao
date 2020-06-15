using Dapper;
using M3.Domain;
using Repository.Dapper.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Dapper
{
    public class ConfigRepository : IConfigRepository
    {
        private readonly IConnectionFactory _connection;

        public ConfigRepository(IConnectionFactory connection)
        {
            _connection = connection;
        }

        public Task<bool> Delete(int configId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<M3EmpresaCad>> Get()
        {
            string sql = $"Select IdEmpresa, Status, ConsumerKey, ConsumerSecret From [m3].[C4MEmpresaCad] Where Status = 1";

            using var connectionDb = _connection.Connection();
            connectionDb.Open();

            return await connectionDb.QueryAsync<M3EmpresaCad>(sql);
        }

        public Task<Device> GetById(int configId)
        {
            throw new NotImplementedException();
        }

        public Task<Device> Insert(M3EmpresaCad config)
        {
            throw new NotImplementedException();
        }

        public Task<Device> Update(M3EmpresaCad config)
        {
            throw new NotImplementedException();
        }
    }
}
