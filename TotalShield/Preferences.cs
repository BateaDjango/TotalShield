using System.Xml.Serialization;

namespace TotalShield
{

    [XmlRoot("Preferences")]
    public class Preferences
    {
        [XmlElement("start_with_windows")]
        public bool start_with_windows { get; set; }

        [XmlElement("run_minimized")]
        public bool run_minimized { get; set; }

        [XmlElement("scan_shortcut")]
        public bool scan_shortcut { get; set; }

        [XmlElement("run_shortcut")]
        public bool run_shortcut { get; set; }


    }



}
