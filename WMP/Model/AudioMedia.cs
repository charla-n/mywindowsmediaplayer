using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WMP.Model
{
    [Serializable]
    public class AudioMedia: Media
    {

        protected override bool artistFilter(string filter)
        {
            return ((Artist == filter) ? true : false);
        }

        protected override bool albumFilter(string filter)
        {
            return ((Album == filter) ? true : false);
        }

        [XmlIgnore]
        public string Artist { get; set; }
        [XmlIgnore]
        public string Album { get; set; }
        [XmlIgnore]
        public uint Year { get; set; }
        [XmlIgnore]
        public int Bitrate { get; set; }
    }
}
