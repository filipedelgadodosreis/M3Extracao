using System.Collections.Generic;
using System.Threading.Tasks;

namespace M3.Domain.Contracts
{
    public interface IConfigRepository
    {
        Task<IEnumerable<M3EmpresaCad>> Get();
        Task<M3EmpresaCad> Insert(M3EmpresaCad config);
        Task<M3EmpresaCad> Update(M3EmpresaCad config);
        Task<M3EmpresaCad> GetById(int configId);
        Task<bool> Delete(int configId);
    }
}
