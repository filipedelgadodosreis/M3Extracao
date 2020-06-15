using System;

namespace M3.Domain
{
    public class Device
    {
        public int DeviceId { get; set; }

        public int FreeBatteryPercentage { get; set; }

        public string DeviceName { get; set; }

        public DateTime LastConnection { get; set; }

        public string UnitGroupName { get; set; }

        public int FreeMemoryPercentage { get; set; }

        public int FreeStoragePercentage { get; set; }

        public string DeviceExtUnit { get; set; }

        public string MdmPropValue { get; set; }
    }
}
