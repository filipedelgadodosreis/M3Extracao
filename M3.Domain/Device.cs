using System;

namespace M3.Domain
{
    public class Device
    {
        /// <summary>
        /// Retorna o ID do dispositivo no Banco
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Retorna a porcentagem da Bateria
        /// </summary>
        public int FreeBatteryPercentage { get; set; }

        /// <summary>
        /// Retorna o IMEI ou SN do dispositivo
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Retorna o DateTime com a ultima comunicação dispositivo - servidor
        /// </summary>
        public DateTime LastConnection { get; set; }

        /// <summary>
        /// Retorna o nome do grupo que o dispositivo pertence
        /// </summary>
        public string UnitGroupName { get; set; }

        /// <summary>
        /// Retorna a porcentagem livre da Memoria RAM do dispositivo
        /// </summary>
        public int FreeMemoryPercentage { get; set; }

        /// <summary>
        /// Retorna a porcentagem livre do armazenamento do dispositivo
        /// </summary>
        public int FreeStoragePercentage { get; set; }

        /// <summary>
        /// Retorna o Nome da unidade a qual o dispositivo pertence
        /// </summary>
        public string DeviceExtUnit { get; set; }

        /// <summary>
        /// Retorna o nome da Operadora instalada no slot 1 do Sim card do Dispositivo
        /// </summary>
        public string MdmPropValue { get; set; }
    }
}
