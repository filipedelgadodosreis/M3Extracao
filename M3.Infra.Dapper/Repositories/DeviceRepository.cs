using Dapper;
using M3.Domain;
using M3.Domain.Contracts;
using Repository.Dapper.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Dapper.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string _connectionString;
        private readonly IConnectionFactory _connection;

        public DeviceRepository(IConnectionFactory connection, string connectionString)
        {
            _connection = connection;
            _connectionString = connectionString;
        }

        public Task<bool> Delete(int deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Device>> Get()
        {
            string sql = $"SELECT d.Device_ID, " +
                $"d.FreeBatteryPercentage, d.DEVICE_NAME, d.LastConnection, u.unit_group_name, d.FreeMemoryPercentage, d.FreeStoragePercentage, d.DEVICE_EXT_UNIT, dp.MDM_PROP_VALUE " +
                $"from[device] d inner join[unit_group] u on u.[UNIT_GROUP_ID] = d.[UNIT_GROUP_ID] " +
                $"left join[DEVICE_MDM_PROPERTY] dp on d.[DEVICE_ID] = dp.[DEVICE_ID] and dp.[MDM_PROP_KEY] = 'TELEPHONY_OPERATOR'  " +
                $"where d.[DEVICE_EXT_UNIT] in ('rde', 'rd6', 'army') and d.[ENABLED] = 1;";

            using var connectionDb = _connection.Connection(_connectionString);
            connectionDb.Open();

            var result = await connectionDb.QueryAsync(sql);

            return new List<Device>();

            //return connectionDb.QueryAsync<Device, Group, DeviceProperty>(
            //       "SELECT * " +
            //       "FROM dbo.Estados E " +
            //       "INNER JOIN dbo.Regioes R ON R.IdRegiao = E.IdRegiao " +
            //       "ORDER BY E.NomeEstado",
            //       map: (d, u, dp) =>
            //       {
            //           d.Group = u;
            //           d.DeviceProperty = dp;

            //           return d;
            //       },
            //       splitOn: "SiglaEstado,IdRegiao");
        }

        public Task<Device> GetById(int deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<Device> Insert(Device device)
        {
            throw new NotImplementedException();
        }

        public Task<Device> Update(Device device)
        {
            throw new NotImplementedException();
        }
    }
}
