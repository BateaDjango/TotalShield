using System.Collections.Generic;
using System.Xml.Serialization;

namespace TotalShield
{
    [XmlRoot("AVList")]
    public class AV_List
    {
        [XmlElement("av")]
        public List<AV> av { get; set; }
    }

    public class AV
    {
        [XmlElement("name")]
        public string name { get; set; }
        [XmlElement("enabled")]
        public bool enabled { get; set; }
    }
}
