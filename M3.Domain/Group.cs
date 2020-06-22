namespace M3.Domain
{
    public class Group
    {
        public int UNIT_GROUP_ID { get; set; }

        public string UNIT_GROUP_EXT_UNIT { get; set; }

        public string UNIT_GROUP_NAME { get; set; }

        public string AndroidTemplate { get; set; }

        public string SmartphoneTemplate { get; set; }

        public string PocketPcTemplate { get; set; }

        public int SoftwareProfile_Id { get; set; }

        public int MonitorProfileId { get; set; }
    }
}
