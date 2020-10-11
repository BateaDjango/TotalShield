using System.Collections.Generic;
using System.Xml.Serialization;

namespace TotalShield
{
    [XmlRoot("Keys")]
    public class Keys
    {
        [XmlElement("key")]
        public List<Key> key { get; set; }
    }

    public class Key
    {
        [XmlElement("type")]
        public string type { get; set; }
        [XmlElement("value")]
        public string value { get; set; }
        [XmlElement("active")]
        public bool active { get; set; }
    }
}
