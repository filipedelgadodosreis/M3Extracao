using M3.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Dapper.Contracts
{
    public interface IConfigRepository
    {
        Task<IEnumerable<M3EmpresaCad>> Get();
        Task<Device> Insert(M3EmpresaCad config);
        Task<Device> Update(M3EmpresaCad config);
        Task<Device> GetById(int configId);
        Task<bool> Delete(int configId);
    }
}
