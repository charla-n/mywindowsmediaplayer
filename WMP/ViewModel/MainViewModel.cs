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
        private enum repeatStatus
        {
            REPEAT_ALL,
            REPEAT_ONE,
            NONE
        };

        MainWindowViewModel             _model;
        PlaylistViewModel               _playlist;

        //FEATURES

        repeatStatus                _repeatState;
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
        RelayCommand _exitfullscreencmd;
        RelayCommand _changevolumecmd;
        RelayCommand _repeatcmd;

        public MainViewModel(MainWindowViewModel model, PlaylistViewModel playlist)
        {
            _nextcmd = new RelayCommand(NextCmd, CanNext);
            _previouscmd = new RelayCommand(PreviousCmd, CanPrevious);
            _playlistcmd = new RelayCommand(PlaylistCmd, () => true);
            _playcmd = new RelayCommand(PlayCmd, () => true);
            _repeatcmd = new RelayCommand(RepeatCmd, () => true);
            _stopcmd = new RelayCommand(StopCmd, () => true);
            _fullscreencmd = new RelayCommand(FullScreenCmd, CanFullScreen);
            _exitfullscreencmd = new RelayCommand(ExitFullScreenCmd, () => true);
            _changevolumecmd = new RelayCommand(ChangeVolumeCmd, () => true);

            _playlist = playlist;
            _repeatState = repeatStatus.NONE;
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
                    _media = new Media { isPlaying = true, FileName = FileName, isStopped = false, Icon = ExtensionStatic.GetIconsFromExtension(Path.GetExtension(FileName)) };
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
                {
                    if (_repeatState == repeatStatus.REPEAT_ALL && _playlist.ListMedia.IndexOf(_media) == (_playlist.ListMedia.Count - 1))
                        return "Next media : " + Path.GetFileName(_playlist.ListMedia[0].FileName);
                    if (_repeatState == repeatStatus.REPEAT_ONE)
                        return ("Next media : " + Path.GetFileName(_media.FileName));
                    return "Next media : " + Path.GetFileName(_playlist.ListMedia[_playlist.ListMedia.IndexOf(_media) + 1].FileName);
                }
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
                if (value > 1)
                    _player.Volume = 1;
                else if (value < 0)
                    _player.Volume = 0;
                else
                    _player.Volume = value;
                OnPropertyChanged("Volume");
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
                OnPropertyChanged("ProMaxgressBar");
            }
        }

        public string RepeatIcon
        {
            get
            {
                if (_repeatState == repeatStatus.NONE)
                    return "../Icons/norepeat.png";
                else if (_repeatState == repeatStatus.REPEAT_ONE)
                    return "../Icons/repeat.png";
                return "../Icons/repeat_infinite.png";
            }
        }

        public string StopPlay
        {
            get
            {
                if (_media == null)
                    return "../Icons/stop.png";
                else if (!_media.isPlaying)
                    return "../Icons/play_icon.png";
                return "../Icons/pause_icon.png";
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

        public ICommand Repeat
        {
            get
            {
                return _repeatcmd;
            }
        }

        public ICommand FullScreen
        {
            get
            {
                return _fullscreencmd;
            }
        }

        public ICommand ExitFullScreen
        {
            get
            {
                return _exitfullscreencmd;
            }
        }

        public ICommand ChangeVolume
        {
            get
            {
                return _changevolumecmd;
            }
        }

        #endregion

        #region CommandTaskBar

        private void NextCmd()
        {
            bool playingPrev = _media.isPlaying;

            if (_playlist.ListMedia.IndexOf(_media) == (_playlist.ListMedia.Count - 1) && _repeatState == repeatStatus.REPEAT_ALL)
                _media = _playlist.ListMedia[0];
            else if (_repeatState != repeatStatus.REPEAT_ONE)
                _media = _playlist.ListMedia[_playlist.ListMedia.IndexOf(_media) + 1];
            _media.isPlaying = playingPrev;
            _player.Source = new Uri(_media.FileName);
            OnPropertyChanged("MediaName");
            OnPropertyChanged("MediaNameNext");
        }

        private void PreviousCmd()
        {
            bool playingNext = _media.isPlaying;

            if (_playlist.ListMedia.IndexOf(_media) == 0 && _repeatState == repeatStatus.REPEAT_ALL)
                _media = _playlist.ListMedia[_playlist.ListMedia.Count - 1];
            else if (_repeatState != repeatStatus.REPEAT_ONE)
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
                if (_repeatState != repeatStatus.NONE)
                    return true;
                if (_playlist.ListMedia.Count > 0 && (_playlist.ListMedia.IndexOf(_media) - 1) >= 0)
                    return true;
            }
            return false;
        }

        private bool CanNext()
        {
            if (_media != null)
            {
                if (_repeatState != repeatStatus.NONE)
                    return true;
                if (_playlist.ListMedia.Count > 0 && (_playlist.ListMedia.IndexOf(_media) + 1) < _playlist.ListMedia.Count)
                    return true;
            }
            return false;
        }

        private void RepeatCmd()
        {
            ++_repeatState;
            if (_repeatState > repeatStatus.NONE)
                _repeatState = repeatStatus.REPEAT_ALL;
            OnPropertyChanged("RepeatIcon");
            OnPropertyChanged("MediaNameNext");
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

        private void ExitFullScreenCmd()
        {
            if (_fullScreen)
            {
                Application.Current.MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                _fullScreen = false;
            }
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

        public void ChangeVolumeCmd(object parameter)
        {
            string action = (string) parameter;

            if (action == "Increase")
                Volume += .05;
            else
                Volume -= .05;
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

        public override void OnChangeView()
        {
            Console.WriteLine("ON CHANGE VIEW");
            OnAddPlaylist();
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
