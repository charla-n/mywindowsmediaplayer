using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMP.Model;

namespace WMP.ViewModel
{
    public class AudioMediaViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public uint Year { get; set; }
        public int Bitrate { get; set; }

        public void FillModel(AudioMedia media)
        {
            Title = media.Title;
            Artist = media.Artist;
            Album = media.Album;
            Year = media.Year;
            Bitrate = media.Bitrate;
        }
    }
}
