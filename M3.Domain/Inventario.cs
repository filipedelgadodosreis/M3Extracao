using System;

namespace M3.Domain
{
    public class Inventario
    {
        public DateTime? DATA { get; set; }

        public string DEVICE_NAME { get; set; }

        public string APP_NAME { get; set; }

        public string VERSAO { get; set; }

        public bool? IND_SISTEMA { get; set; }
    }
}
