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
    public class MainViewModel : ViewModelBase
    {
        MainWindowViewModel             _model;
        PlaylistViewModel               _playlist;

        //FEATURES

        bool                        _fullScreen;
        Timer                       _progress;

        //MEDIA

        MediaElement                _player;
        Media                       _media;

        public MainViewModel(MainWindowViewModel model, PlaylistViewModel playlist)
        {
            _playlist = playlist;
            _model = model;
            _media = null;
            _fullScreen = false;
            _progress = new Timer(1000);
            _progress.Elapsed += ProgressElapsed;
            _player = new MediaElement();
            _player.LoadedBehavior = MediaState.Manual;
            _player.MediaOpened += MediaLoaded;
        }

        #region Events
        private void MediaLoaded(object sender, RoutedEventArgs evt)
        {
            Console.WriteLine("Event MediaLoaded");
            if (_player.NaturalDuration.HasTimeSpan)
            {
                if (_media != null)
                {
                    try
                    {
                        _media.Duration = (int)_player.NaturalDuration.TimeSpan.TotalSeconds;
                    }
                    catch (System.InvalidOperationException e)
                    {
                        Console.WriteLine(e);
                    }
                }
                OnPropertyChanged("StopPlay");
                OnPropertyChanged("MaxProgressBar");
            }
        }

        private void ProgressElapsed(object sender, ElapsedEventArgs evt)
        {
            OnPropertyChanged("ProgressBar");
        }

        #endregion Events

        #region Menu

        public void OnOpenMedia(string FileName)
        {
            Console.WriteLine("FileName : " + FileName);
            _progress.Start();
            _player.Source = new Uri(FileName);
            _player.Play();
            if (_media == null)
            {
                _media = new Media { isPlaying = true, FileName = FileName };
                _playlist.ListMedia.Add(_media);
            }
            else
            {
                _media.isPlaying = true;
                _media.FileName = FileName;
            }
        }

        #endregion

        #region Properties

        public double Volume
        {
            get
            {
                return _player.Volume;
            }
            set
            {
                _player.Volume = value;
            }
        }

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
                if (_media != null)
                {
                    if (_player.IsLoaded && _player.NaturalDuration.HasTimeSpan)
                    {
                        Console.WriteLine(_player.NaturalDuration.TimeSpan.TotalSeconds);
                        return (int)_player.NaturalDuration.TimeSpan.TotalSeconds;
                    }
                    else
                        return 0;
                }
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
                    return "../Icons/109.png";
                else if (!_media.isPlaying)
                    return "../Icons/98.png";
                return "../Icons/93.png";
            }
        }
        #endregion

        #region TaskBar

        public ICommand Next
        {
            get
            {
                return new RelayCommand(NextCmd, CanNext);
            }
        }

        public ICommand Previous
        {
            get
            {
                return new RelayCommand(PreviousCmd, CanPrevious);
            }
        }

        public ICommand Playlist
        {
            get
            {
                return new RelayCommand(PlaylistCmd, () => true);
            }
        }

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

        private void NextCmd()
        {
            _media = _playlist.ListMedia[_playlist.ListMedia.IndexOf(_media) + 1];
            _player.Source = new Uri(_media.FileName);
        }

        private void PreviousCmd()
        {
            _media = _playlist.ListMedia[_playlist.ListMedia.IndexOf(_media) - 1];
            _player.Source = new Uri(_media.FileName);
        }

        private bool CanPrevious()
        {
            if (_media != null)
            {
                if (_playlist.ListMedia.Count > 0 && (_playlist.ListMedia.IndexOf(_media) - 1) >= 0)
                    return true;
            }
            return false;
        }

        private bool CanNext()
        {
            if (_media != null)
            {
                if (_playlist.ListMedia.Count > 0 && (_playlist.ListMedia.IndexOf(_media) + 1) < _playlist.ListMedia.Count)
                    return true;
            }
            return false;
        }

        private void PlaylistCmd()
        {
            _player.Stop();
            _player.Close();
            _model.ChangePage();
        }

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
            _player.Close();
            if (_fullScreen)
                FullScreenCmd();
            OnPropertyChanged("StopPlay");
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

        #region Playlist

        public void OnAddPlaylist()
        {
            if (_playlist.ListMedia.Count > 0 && _media == null)
            {
                _media = _playlist.ListMedia[0];
                _player.Source = new Uri(_media.FileName);
            }
        }

        #endregion

        #region Konami

        public void ProcessKonami()
        {
            _player.Stop();
            _player.Source = new Uri(@"Konami/konami.mp3", UriKind.Relative);
            _player.Play();
        }

        #endregion
    }
}
