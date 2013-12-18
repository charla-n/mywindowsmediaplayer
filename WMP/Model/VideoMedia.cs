﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WMP.Model
{
    [Serializable]
    public class VideoMedia : Media
    {
        [XmlIgnore]
        public uint Year { get; set; }
        [XmlIgnore]
        public int Bitrate { get; set; }
        [XmlIgnore]
        public int Width { get; set; }
        [XmlIgnore]
        public int Height { get; set; }
    }
}