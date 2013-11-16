using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMP
{
    public class PlaylistViewModel : ViewModelBase
    {
        MainWindowViewModel _model;

        public PlaylistViewModel(MainWindowViewModel model)
        {
            _model = model;
        }
    }
}
