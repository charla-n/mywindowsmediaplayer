using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WMP.Model;

namespace WMP
{
    public class WMPViewModel : ViewModelBase
    {
        public event EventHandler<PlayerEvent> PlayerEvent;

        ObservableCollection<Media> _playList;
        Media _media;

        public WMPViewModel()
        {
            _playList = new ObservableCollection<Media>();
        }

        #region Properties
        public string MovieSource
        {
            get
            {
                return _media == null ? "" : _media.FileName;
            }
            set
            {
                _media.FileName = value;
                _media.isPlaying = true;
                OnPropertyChanged("MovieSource");
                OnPropertyChanged("StopPlay");
            }
        }

        public string StopPlay
        {
            get
            {
                if (_media == null)
                    return "Icons/109.png";
                else if (!_media.isPlaying)
                    return "Icons/98.png";
                return "Icons/93.png";
            }
            set
            {
            }
        }
        #endregion

        #region Menu
        public ICommand OpenMedia
        {
            get
            {
                return new RelayCommand(OpenMediaCmd, () => true);
            }
        }

        private void OpenMediaCmd()
        {
            OpenFileDialog  dialog = new OpenFileDialog();
            bool? res;

            dialog.Filter = "Video files|*.avi;*.mpg;*.mov;*.asf|Audio files|*.mp3;*.wav;*.wma;*.ogg;*.pls|ALL files|*.*";
            res = dialog.ShowDialog();
            if (res == true)
            {
                if (_media == null)
                    _media = new Media();
                if (PlayerEvent != null)
                {
                    PlayerEvent(this, new PlayerEvent(ActionType.PLAY));
                }
                MovieSource = dialog.FileName;
            }
        }

        #endregion

        #region TaskBar

        public ICommand Play
        {
            get
            {
                return new RelayCommand(PlayCmd, () => true);
            }
        }

        public ICommand Stop
        {
            get
            {
                return new RelayCommand(StopCmd, () => true);
            }
        }

        public ICommand FullScreen
        {
            get
            {
                return new RelayCommand(FullScreenCmd, CanFullScreen);
            }
        }

        private void FullScreenCmd()
        {
            if (PlayerEvent != null)
            {
                PlayerEvent(this, new PlayerEvent(ActionType.FULLSCREEN));
            }
        }

        private bool CanFullScreen()
        {
            if (_media != null)
            {
                return true;
            }
            return false;
        }

        private void StopCmd()
        {
            if (PlayerEvent != null)
            {
                _media = null;
                PlayerEvent(this, new PlayerEvent(ActionType.STOP));
            }
        }

        private void PlayCmd()
        {
            if (_media == null)
                return;
            if (_media.isPlaying == true)
            {
                if (PlayerEvent != null)
                {
                    PlayerEvent(this, new PlayerEvent(ActionType.PAUSE));
                }
            }
            else
            {
                if (PlayerEvent != null)
                {
                    PlayerEvent(this, new PlayerEvent(ActionType.PLAY));
                }
            }
            _media.isPlaying = !_media.isPlaying;
            OnPropertyChanged("StopPlay");
        }

        #endregion
    }
}
