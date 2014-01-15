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
            if (filter != null && filter.Length <= 0)
                return (true);
            return ((Artist != null && Artist.ToLower() == filter.ToLower()) ? true : false);
        }

        protected override bool albumFilter(string filter)
        {
            System.Console.WriteLine("album filter in");
            if (filter != null && filter.Length <= 0)
                return (true);
            return ((Album != null && Album.ToLower() == filter.ToLower()) ? true : false);
        }

        public string Artist { get; set; }
        public uint Year { get; set; }
        public string Album { get; set; }
        [XmlIgnore]
        public int Bitrate { get; set; }
    }
}
