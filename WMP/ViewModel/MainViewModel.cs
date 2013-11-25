using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    public class MainViewModel : ViewModelBase, IDisposable
    {
        MainWindowViewModel             _model;
        PlaylistViewModel               _playlist;

        //FEATURES

        bool                        _fullScreen;
        Timer                       _progress;

        //MEDIA

        MediaElement                _player;
        Media                       _media;

        //COMMANDS

        RelayCommand _nextcmd;
        RelayCommand _previouscmd;
        RelayCommand _playlistcmd;
        RelayCommand _playcmd;
        RelayCommand _stopcmd;
        RelayCommand _fullscreencmd;

        public MainViewModel(MainWindowViewModel model, PlaylistViewModel playlist)
        {
            _nextcmd = new RelayCommand(NextCmd, CanNext);
            _previouscmd = new RelayCommand(PreviousCmd, CanPrevious);
            _playlistcmd = new RelayCommand(PlaylistCmd, () => true);
            _playcmd = new RelayCommand(PlayCmd, () => true);
            _stopcmd = new RelayCommand(StopCmd, () => true);
            _fullscreencmd = new RelayCommand(FullScreenCmd, CanFullScreen);

            _playlist = playlist;
            _model = model;
            _media = null;
            _fullScreen = false;
            _progress = new Timer(1000);
            _progress.Elapsed += ProgressElapsed;
            _player = new MediaElement();
            _player.LoadedBehavior = MediaState.Manual;
            _player.MediaOpened += MediaLoaded;
            _player.MediaEnded += OnMediaEnded;
            _player.MediaFailed += OnMediaFailed;
        }

        #region Events

        private void OnMediaFailed(object sender, RoutedEventArgs evt)
        {
            MessageBox.Show(_media.FileName + " doesn't exist.", "Error on loading media", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void OnMediaEnded(object sender, RoutedEventArgs evt)
        {
            Console.WriteLine("Event OnMediaEnded");
            if (!CanNext())
            {
                _media.isPlaying = false;
                _progress.Stop();
                _player.Stop();
            }
            else
            {
                NextCmd();
            }
            OnPropertyChanged("Next");
            OnPropertyChanged("Previous");
            OnPropertyChanged("StopPlay");
        }

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
            }
            OnPropertyChanged("TotalTime");
            OnPropertyChanged("MediaName");
            OnPropertyChanged("MediaNameNext");
            OnPropertyChanged("StopPlay");
            OnPropertyChanged("MaxProgressBar");
        }

        private void ProgressElapsed(object sender, ElapsedEventArgs evt)
        {
            OnPropertyChanged("CurrentTime");
            OnPropertyChanged("ProgressBar");
        }

        #endregion Events

        #region Menu

        public void OnOpenMedia(string FileName)
        {
            if (FileName != null)
            {
                Console.WriteLine("FileName : " + FileName);
                _progress.Start();
                try
                {
                    _player.Source = new Uri(FileName);
                    _player.Play();
                }
                catch (Exception)
                {
                    MessageBox.Show("Error occured when trying to open media", "Open error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (_media == null)
                {
                    _media = new Media { isPlaying = true, FileName = FileName, isStopped = false };
                    _playlist.ListMedia.Add(_media);
                }
                else
                {
                    _media.isPlaying = true;
                    _media.FileName = FileName;
                }
            }
        }

        #endregion

        #region Properties

        public TimeSpan CurrentTime
        {
            get
            {
                if (_player.NaturalDuration.HasTimeSpan)
                    return TimeSpan.FromTicks(_player.Position.Ticks);
                return TimeSpan.Zero;
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                if (_player.NaturalDuration.HasTimeSpan)
                    return _player.NaturalDuration.TimeSpan;
                else
                    return TimeSpan.Zero;
            }
        }

        public string MediaName
        {
            get
            {
                if (_media == null)
                    return "Currently playing : no media";
                else
                    return "Currently playing : " + Path.GetFileName(_media.FileName);
            }
        }

        public string MediaNameNext
        {
            get
            {
                if (_media == null || CanNext() == false)
                    return "Next media : no next media";
                else
                    return "Next media : " + Path.GetFileName(_playlist.ListMedia[_playlist.ListMedia.IndexOf(_media) + 1].FileName);
            }
        }

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
                Console.WriteLine("ProgressBar=" + _player.Position.Hours * 3600 + _player.Position.Minutes * 60 + _player.Position.Seconds);
                return _player.Position.Hours * 3600 + _player.Position.Minutes * 60 + _player.Position.Seconds;
            }
            set
            {
                _player.Position = TimeSpan.FromSeconds(value);
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
                        Console.WriteLine("MaxProgressBar=" + _player.NaturalDuration.TimeSpan.TotalSeconds);
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
                return _nextcmd;
            }
        }

        public ICommand Previous
        {
            get
            {
                return _previouscmd;
            }
        }

        public ICommand Playlist
        {
            get
            {
                return _playlistcmd;
            }
        }

        public ICommand Play
        {
            get
            {
                return _playcmd;
            }
        }

        public ICommand Stop
        {
            get
            {
                return _stopcmd;
            }
        }

        public ICommand FullScreen
        {
            get
            {
                return _fullscreencmd;
            }
        }

        #endregion

        #region CommandTaskBar

        private void NextCmd()
        {
            bool playingPrev = _media.isPlaying;

            _media = _playlist.ListMedia[_playlist.ListMedia.IndexOf(_media) + 1];
            _media.isPlaying = playingPrev;
            _player.Source = new Uri(_media.FileName);
            OnPropertyChanged("MediaName");
            OnPropertyChanged("MediaNameNext");
        }

        private void PreviousCmd()
        {
            bool playingNext = _media.isPlaying;

            _media = _playlist.ListMedia[_playlist.ListMedia.IndexOf(_media) - 1];
            _media.isPlaying = playingNext;
            _player.Source = new Uri(_media.FileName);
            OnPropertyChanged("MediaName");
            OnPropertyChanged("MediaNameNext");
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
            _progress.Stop();
            _player.Stop();
            if (_media != null)
                _media.isPlaying = false;
            OnPropertyChanged("StopPlay");
            _model.ChangePage(MainWindowViewModel.PageEnum.PLAYLIST);
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
            if (_media != null && _media.isStopped == false)
            {
                return true;
            }
            return false;
        }

        private void StopCmd()
        {
            _progress.Stop();
            OnPropertyChanged("ProgressBar");
            if (_playlist.ListMedia.Count > 0)
            {
                _media = _playlist.ListMedia[0];
                _media.isPlaying = false;
                _media.isStopped = true;
                _player.Source = new Uri(_media.FileName);
            }
            _player.Stop();
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
                _player.Pause();
                if (_player.NaturalDuration.HasTimeSpan)
                    _progress.Stop();
            }
            else
            {
                _media.isStopped = false;
                _player.Play();
                if (_player.NaturalDuration.HasTimeSpan)
                {
                    _progress.Start();
                }
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

        public void OnChangeView()
        {
            ((MainViewModel)CurrentPageBase).OnAddPlaylist();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _progress.Close();
                _progress.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
