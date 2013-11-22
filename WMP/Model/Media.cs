using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WMP.Model
{
    [Serializable]
    public class Media
    {
        public string FileName { get; set; }
        [XmlIgnore]
        public int Duration { get; set; }
        [XmlIgnore]
        public bool isPlaying { get; set; }
        [XmlIgnore]
        public bool IsSelected { get; set; }
    }
}
