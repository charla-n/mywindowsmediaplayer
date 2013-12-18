using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMP.Model;

namespace WMP.ViewModel
{
    public class VideoMediaViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public uint Year { get; set; }
        public int Bitrate { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public void FillModel(VideoMedia media)
        {
            Title = media.Title;
            Year = media.Year;
            Bitrate = media.Bitrate;
            Width = media.Width;
            Height = media.Height;
        }
    }
}
