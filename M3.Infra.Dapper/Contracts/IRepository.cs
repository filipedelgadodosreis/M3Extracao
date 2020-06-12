using M3.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Dapper.Contracts
{
    public interface IM3Repository
    {
        Task<IEnumerable<Device>> Get();
        Task<Device> Insert(Device hero);
        Task<Device> Update(Device hero);
        Task<Device> GetById(Guid heroId);
        Task<bool> Delete(Guid heroId);
    }
}
