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
            string sql = $"Select IdEmpresa, NmRazaoSocial From [config].[CadEmpresas]";

            using var connectionDb = _connection.Connection();
            connectionDb.Open();

            return await connectionDb.QueryAsync<M3EmpresaCad>(sql);
        }

        public async Task<M3EmpresaCad> GetById(int configId)
        {
            string sql = $"Select IdEmpresa, NmRazaoSocial From [config].[CadEmpresas] where [IdEmpresa] = @ConfigId";

            using var connectionDb = _connection.Connection();
            connectionDb.Open();

            return await connectionDb.QueryFirstOrDefaultAsync<M3EmpresaCad>(sql, new { ConfigId = configId });
        }

        public Task<M3EmpresaCad> Insert(M3EmpresaCad config)
        {
            throw new NotImplementedException();
        }

        public Task<M3EmpresaCad> Update(M3EmpresaCad config)
        {
            throw new NotImplementedException();
        }
    }
}
