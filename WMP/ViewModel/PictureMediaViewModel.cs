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
        private string _title;
        private int _width;
        private int _height;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        public void FillModel(PictureMedia media)
        {
            Title = media.Title;
            Width = media.Width;
            Height = media.Height;
        }
    }
}