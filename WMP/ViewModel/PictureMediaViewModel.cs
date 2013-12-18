using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMP.Model;

namespace WMP.ViewModel
{
    public class PictureMediaViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public void FillModel(PictureMedia media)
        {
            Title = media.Title;
            Width = media.Width;
            Height = media.Height;
        }
    }
}