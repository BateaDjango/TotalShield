using System.Collections.Generic;
using System.Xml.Serialization;

namespace TotalShield
{

    [XmlRoot("Reports")]
    public class AV_Reports
    {
        [XmlElement("report")]
        public List<AV_Report> av_reports { get; set; }
    }

    public class AV_Report
    {
        [XmlElement("time")]
        public string time { get; set; }

        [XmlElement("file")]
        public string file { get; set; }

        [XmlElement("hash")]
        public string hash { get; set; }

        [XmlElement("av_results")]
        public List<AV_Result> av_results { get; set; }


    }

    public class AV_Result
    {
        [XmlElement("av_name")]
        public string av_name { get; set; }

        [XmlElement("result")]
        public string result { get; set; }

    }



}
