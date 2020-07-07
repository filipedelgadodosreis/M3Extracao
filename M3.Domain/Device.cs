using System;

namespace M3.Domain
{
    public class Device
    {
        public DateTime DtLeitura { get; set; }
        public int IdEmpresa { get; set; }
        public int DEVICE_ID { get; set; }

        //public int DEV_UPDATE_PKG_ID { get; set; }

        //public int DEVICE_TYPE_ID { get; set; }

        public int UNIT_GROUP_ID { get; set; }

        //public int EXT_NODE_ID { get; set; }

        public bool ENABLED { get; set; }

       public string M3CLIENT_VERSION { get; set; }

        //public int M3C_TYPE { get; set; }

        //public string TRANSFER_DIR { get; set; }

        public string IDH { get; set; }

        //public bool IDH_CONTROL { get; set; }

        //public bool IDH_UPDATE { get; set; }

        //public string EXTAPP_VERSION { get; set; }

        //public string ADAPTERS_VERSION { get; set; }

        //public bool IS_REMOTE_SUPPORT_CLIENT { get; set; }

        //public bool UPDATE_BLACK_LIST { get; set; }

        //public bool UPDATE_CONFIG { get; set; }

        public string DEVICE_EXT_UNIT { get; set; }

        //public string LOG_LEVEL { get; set; }

        //public string IP { get; set; }

        //public bool NotificationEnabled { get; set; }

        //public int PushTimeOut { get; set; }

        //public int PullingInterval { get; set; }

        //public int NotificationType { get; set; }

        public DateTime? LastConnection { get; set; }

        public int FreeMemoryPercentage { get; set; }

        public int FreeStoragePercentage { get; set; }

        public int FreeBatteryPercentage { get; set; }

        //public DateTime LastMemoryCollectionDate { get; set; }

        public DateTime? LastStorageCollectionDate { get; set; }

        //public DateTime LastBatteryCollectionDate { get; set; }

        //public DateTime LastSoftwareCollectionDate { get; set; }

        //public DateTime LastProcessCollectionDate { get; set; }

        //public DateTime LastSystemCollectionDate { get; set; }

        //public string LastLocationInfo { get; set; }

        //public string DefaultTemplate { get; set; }

        //public string PnsToken { get; set; }

        //public DateTime LastLocationCollectionDate { get; set; }

        //public DateTime LastConfigurationCollectionDate { get; set; }

        //public string DeviceStatusNote { get; set; }

        //public int DeviceStatus_Id { get; set; }

        public string UNIT_GROUP_NAME { get; set; }

        public string UNIT_GROUP_EXT_UNIT { get; set; }

        public string MDM_PROP_VALUE { get; set; }

        public string DEVICE_NAME { get; set; }

    }
}
