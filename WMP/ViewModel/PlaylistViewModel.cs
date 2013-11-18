using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using WMP.Model;

namespace WMP
{
    public class PlaylistViewModel : ViewModelBase
    {
        MainWindowViewModel         _model;

        public PlaylistViewModel(MainWindowViewModel model)
        {
            ListMedia = new ObservableCollection<Media>();
            _model = model;
        }

        #region Properties

        public ICommand Add
        {
            get
            {
                return new RelayCommand(AddCmd, () => true);
            }
        }

        public ICommand Delete
        {
            get
            {
                return new RelayCommand(DeleteCmd, () => true);
            }
        }

        public ICommand Clear
        {
            get
            {
                return new RelayCommand(ClearCmd, () => true);
            }
        }

        public ICommand Playlist
        {
            get
            {
                return new RelayCommand(PlaylistCmd, () => true);
            }
        }

        #endregion

        public ObservableCollection<Media> ListMedia { get; set; }

        #region Commands

        private void AddCmd()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? res;

            dialog.Filter = "Video files|*.avi;*.mpg;*.mov;*.asf|Audio files|*.mp3;*.wav;*.wma;*.ogg;*.pls|Picture Files|*.jpg;*.bmp;*.png|ALL files|*.*";
            res = dialog.ShowDialog();
            if (res == true)
            {
                ListMedia.Add(new Media { FileName = dialog.FileName, isPlaying = false });
            }
        }

        private void DeleteCmd()
        {

        }

        private void ClearCmd()
        {
            ListMedia.Clear();
        }

        private void PlaylistCmd()
        {
            _model.ChangePage();
        }

        #endregion
    }
}
