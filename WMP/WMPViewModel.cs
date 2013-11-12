using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WMP.Model;

namespace WMP
{
    public class WMPViewModel : ViewModelBase
    {
        bool _fullScreen;
        Timer _progress;
        MediaElement _player;
        ObservableCollection<Media> _playList;
        Media _media;

        public WMPViewModel()
        {
            _fullScreen = false;
            _progress = new Timer(1000);
            _progress.Elapsed += ProgressElapsed;
            _player = new MediaElement();
            _player.LoadedBehavior = MediaState.Manual;
            _player.MediaOpened += MediaLoaded;
            _playList = new ObservableCollection<Media>();
        }

        #region Events
        private void MediaLoaded(object sender, RoutedEventArgs evt)
        {
            _media.Duration = (int)_player.NaturalDuration.TimeSpan.TotalSeconds;
            OnPropertyChanged("StopPlay");
            OnPropertyChanged("MaxProgressBar");
        }

        private void ProgressElapsed(object sender, ElapsedEventArgs evt)
        {
            OnPropertyChanged("ProgressBar");
        }
        #endregion Events

        #region Properties

        public MediaElement MyMediaElement
        {
            get
            {
                return _player;
            }
        }

        public int ProgressBar
        {
            get
            {
                return _player.Position.Seconds;
            }
            set
            {
                OnPropertyChanged("ProgressBar");
            }
        }

        public int MaxProgressBar
        {
            get
            {
                if (_player.IsLoaded)
                {
                    Console.WriteLine(_player.NaturalDuration.TimeSpan.TotalSeconds);
                    return (int)_player.NaturalDuration.TimeSpan.TotalSeconds;
                }
                else
                    return 0;
            }
            set
            {
                OnPropertyChanged("MaxProgressBar");
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

        public ICommand Quit
        {
            get
            {
                return new RelayCommand(QuitCmd, () => true);
            }
        }

        #endregion

        #region CommandMenu

        private void QuitCmd()
        {
            _player.Close();
            Application.Current.Shutdown();
        }

        private void OpenMediaCmd()
        {
            OpenFileDialog  dialog = new OpenFileDialog();
            bool? res;

            dialog.Filter = "Video files|*.avi;*.mpg;*.mov;*.asf|Audio files|*.mp3;*.wav;*.wma;*.ogg;*.pls|ALL files|*.*";
            res = dialog.ShowDialog();
            if (res == true)
            {
                _progress.Start();
                _player.Source = new Uri(dialog.FileName);
                _player.Play();
                if (_media == null)
                {
                    _media = new Media { isPlaying = true, FileName = dialog.FileName };
                }
                else
                {
                    _media.isPlaying = true;
                    _media.FileName = dialog.FileName;
                }
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

        #endregion

        #region CommandTaskBar
        private void FullScreenCmd()
        {
            if (!_fullScreen)
            {
                Application.Current.MainWindow.WindowStyle = WindowStyle.None;
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Application.Current.MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
            _fullScreen = !_fullScreen;
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
            _media = null;
            _progress.Stop();
            _player.Stop();
        }

        private void PlayCmd()
        {
            if (_media == null)
                return;
            if (_media.isPlaying == true)
            {
                _progress.Stop();
                _player.Pause();
            }
            else
            {
                _progress.Start();
                _player.Play();
            }
            _media.isPlaying = !_media.isPlaying;
            OnPropertyChanged("StopPlay");
        }

        #endregion
    }
}
