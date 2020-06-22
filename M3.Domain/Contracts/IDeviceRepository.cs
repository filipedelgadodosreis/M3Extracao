using System.Collections.Generic;
using System.Threading.Tasks;

namespace M3.Domain.Contracts
{
    public interface IDeviceRepository
    {
        Task<IEnumerable<Device>> Get();
        Task<Device> Insert(Device device);
        Task<Device> Update(Device device);
        Task<Device> GetById(int deviceId);
        Task<bool> Delete(int deviceId);
    }
}
