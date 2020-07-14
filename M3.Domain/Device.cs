using System;

namespace M3.Domain
{
    public class Device
    {
        public DateTime DtLeitura { get; set; }
        public int IdEmpresa { get; set; }
        public int DEVICE_ID { get; set; }
        public int UNIT_GROUP_ID { get; set; }
        public bool ENABLED { get; set; }
        public string M3CLIENT_VERSION { get; set; }
        public string IDH { get; set; }


        public string DEVICE_EXT_UNIT { get; set; }
        public DateTime? LastConnection { get; set; }
        public int FreeMemoryPercentage { get; set; }
        public int FreeStoragePercentage { get; set; }
        public int FreeBatteryPercentage { get; set; }
        public DateTime? LastStorageCollectionDate { get; set; }
        public string UNIT_GROUP_NAME { get; set; }
        public string UNIT_GROUP_EXT_UNIT { get; set; }
        public string MDM_PROP_VALUE { get; set; }
        public string DEVICE_NAME { get; set; }
        public string Operadora { get; set; }
        public string Modelo_Equipamento { get; set; }
        public string Sistema_Operacional { get; set; }
        public string InicioCiclo { get; set; }
        public DateTime? Ultima_Coleta { get; set; }
        public float Download_MB { get; set; }
        public float Upload_MB { get; set; }
        public float TotalConsumo_MB { get; set; }
    }
}
