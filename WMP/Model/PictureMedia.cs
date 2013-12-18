using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WMP.Model
{
    [Serializable]
    public class PictureMedia : Media
    {
        [XmlIgnore]
        public int Width { get; set; }
        [XmlIgnore]
        public int Height { get; set; }
    }
}
