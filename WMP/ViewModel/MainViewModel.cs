using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WMP.Model;
using WMP.View;
using WMP.ViewModel;

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
        Random                      _rnd;
        bool                        _isRandEnabled;
        int                         _rand;
        bool                        _fullScreen;
        Timer                       _progress;

        //MEDIA

        MediaElement                _player;
        Media                       _media;

        //MEDIA INFOS
        bool                        _mediaInfosOpen; // is one of the windows open
        Window[]                    _mediaInfos; // Various media infos windows
        Action[]                    _mediaInfosFillers; // Functions to fill corresponding model
        ViewModelBase[]             _infosModels; // Models containing the variables used by the windows
        Func<Window>[]              _createMediaWindow; // Functions to create the corresponding window

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
        RelayCommand _randcmd;
        RelayCommand _mediainfoscmd;

        public MainViewModel(MainWindowViewModel model, PlaylistViewModel playlist)
        {
            _nextcmd = new RelayCommand(NextCmd, CanNext);
            _previouscmd = new RelayCommand(PreviousCmd, CanPrevious);
            _playlistcmd = new RelayCommand(PlaylistCmd, () => true);
            _playcmd = new RelayCommand(PlayCmd, () => true);
            _repeatcmd = new RelayCommand(RepeatCmd, () => true);
            _randcmd = new RelayCommand(RandCmd, () => true);
            _stopcmd = new RelayCommand(StopCmd, () => true);
            _fullscreencmd = new RelayCommand(FullScreenCmd, CanFullScreen);
            _exitfullscreencmd = new RelayCommand(ExitFullScreenCmd, () => true);
            _changevolumecmd = new RelayCommand(ChangeVolumeCmd, () => true);
            _mediainfoscmd = new RelayCommand(MediaInfosCmd, CanMediaInfos);

            _rand = 0;
            _playlist = playlist;
            _isRandEnabled = false;
            _repeatState = repeatStatus.NONE    ;
            _model = model;
            _media = null;
            _mediaInfosOpen = false;
            _mediaInfos = new Window[(int)t_MediaType.NONE];
            _mediaInfos[(int)t_MediaType.AUDIO] = null;
            _mediaInfos[(int)t_MediaType.VIDEO] = null;
            _mediaInfos[(int)t_MediaType.PICTURE] = null;
            _mediaInfosFillers = new Action[(int)t_MediaType.NONE];
            _mediaInfosFillers[(int)t_MediaType.AUDIO] = new Action(FillAudioModel);
            _mediaInfosFillers[(int)t_MediaType.VIDEO] = new Action(FillVideoModel);
            _mediaInfosFillers[(int)t_MediaType.PICTURE] = new Action(FillPictureModel);
            _createMediaWindow = new Func<Window>[(int)t_MediaType.NONE];
            _createMediaWindow[(int)t_MediaType.AUDIO] = new Func<Window>(CreateAudioWindow);
            _createMediaWindow[(int)t_MediaType.VIDEO] = new Func<Window>(CreateVideoWindow);
            _createMediaWindow[(int)t_MediaType.PICTURE] = new Func<Window>(CreatePictureWindow);
            _infosModels = new ViewModelBase[(int)t_MediaType.NONE];
            _infosModels[(int)t_MediaType.AUDIO] = new AudioMediaViewModel();
            _infosModels[(int)t_MediaType.VIDEO] = new VideoMediaViewModel();
            _infosModels[(int)t_MediaType.PICTURE] = new PictureMediaViewModel();
            _fullScreen = false;
            _rnd = new Random();
            _progress = new Timer(200);
            _progress.Elapsed += ProgressElapsed;
            _player = new MediaElement();
            _player.LoadedBehavior = MediaState.Manual;
            _player.MediaOpened += MediaLoaded;
            _player.MediaEnded += OnMediaEnded;
            _player.MediaFailed += OnMediaFailed;
        }

        #region Events

        private void OnCloseMediaInfos(object sender, EventArgs e)
        {
            Window w = sender as Window;

            w.Close();
            _mediaInfosOpen = false;
            _mediaInfos[(int)t_MediaType.AUDIO] = null;
            _mediaInfos[(int)t_MediaType.VIDEO] = null;
            _mediaInfos[(int)t_MediaType.PICTURE] = null;
        }

        private void OnMediaFailed(object sender, RoutedEventArgs evt)
        {
            _progress.Stop();
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
                _progress.Stop();
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
                        _progress.Start();
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
                    _media = Media.CreateMedia(true, FileName, false, ExtensionStatic.GetIconsFromExtension(Path.GetExtension(FileName)));
                    _playlist.ListMedia.Add(_media);
                }
                else
                {
                    _playlist.ListMedia.Remove(_media);
                    _media = Media.CreateMedia(true, FileName, false, ExtensionStatic.GetIconsFromExtension(Path.GetExtension(FileName)));
                    _playlist.ListMedia.Add(_media);
                }
            }
        }

        #endregion

        #region CommandContextMenu

        private bool CanMediaInfos()
        {
            return (_media != null && (_mediaInfos == null || _mediaInfosOpen == false ? true : false));
        }

        # region FillModelsFunctions

        private void FillAudioModel()
        {
            AudioMedia media = _media as AudioMedia;
            AudioMediaInfosModel.FillModel(media);
        }

        private void FillVideoModel()
        {
            VideoMedia media = _media as VideoMedia;
            VideoMediaInfosModel.FillModel(media);
        }

        private void FillPictureModel()
        {
            PictureMedia media = _media as PictureMedia;
            PictureMediaInfosModel.FillModel(media);
        }

        # endregion

        private void FillGoodModel()
        {
            if (_media.MediaType != t_MediaType.NONE)
            {
                _mediaInfosFillers[(int)_media.MediaType]();
            }
        }

        # region CreateWindowFunctions

        private Window CreateAudioWindow()
        {
            return new AudioMediaInfos();
        }

        private Window CreateVideoWindow()
        {
            return new VideoMediaInfos();
        }

        private Window CreatePictureWindow()
        {
            return new PictureMediaInfos();
        }

        #endregion

        private void CreateGoodMediaInfos()
        {
            if (_media.MediaType != t_MediaType.NONE)
                _mediaInfos[(int)_media.MediaType] = _createMediaWindow[(int)_media.MediaType]();
        }

        private void MediaInfosCmd()
        {
            if (_media.MediaType != t_MediaType.NONE)
            {
                FillGoodModel();
                CreateGoodMediaInfos();
                _mediaInfos[(int)_media.MediaType].Closed += OnCloseMediaInfos;
                _mediaInfosOpen = true;
                _mediaInfos[(int)_media.MediaType].Show();
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
                    if (_isRandEnabled)
                    {
                        _rand = _rnd.Next() % (_playlist.ListMedia.Count - 1);
                        return ("Next random media : " + Path.GetFileName(_playlist.ListMedia[_rand].FileName));
                    }
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
                return _player.Position.Hours * 3600000 + _player.Position.Minutes * 60000 + _player.Position.Seconds * 1000 + _player.Position.Milliseconds;
            }
            set
            {
                _player.Position = TimeSpan.FromMilliseconds(value);
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
                        Console.WriteLine("MaxProgressBar=" + _player.NaturalDuration.TimeSpan.TotalMilliseconds);
                        return (int)_player.NaturalDuration.TimeSpan.TotalMilliseconds;
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
        
        public Media Media
        {
            get
            {
                return _media;
            }
            set
            {
                _media = value;
                OnPropertyChanged("Media");
            }
        }

        public AudioMediaViewModel AudioMediaInfosModel
        {
            get
            {
                return _infosModels[(int)t_MediaType.AUDIO] as AudioMediaViewModel;
            }
        }

        public VideoMediaViewModel VideoMediaInfosModel
        {
            get
            {
                return _infosModels[(int)t_MediaType.VIDEO] as VideoMediaViewModel;
            }
        }

        public PictureMediaViewModel PictureMediaInfosModel
        {
            get
            {
                return _infosModels[(int)t_MediaType.PICTURE] as PictureMediaViewModel;
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

        public ICommand Random
        {
            get
            {
                return _randcmd;
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

        public ICommand MediaInfos
        {
            get
            {
                return _mediainfoscmd;
            }
        }

        #endregion

        #region CommandTaskBar

        private void NextCmd()
        {
            bool playingPrev = _media.isPlaying;

            if (_isRandEnabled)
                _media = _playlist.ListMedia[_rand];
            else if (_playlist.ListMedia.IndexOf(_media) == (_playlist.ListMedia.Count - 1) && _repeatState == repeatStatus.REPEAT_ALL)
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

            if (_isRandEnabled)
                _media = _playlist.ListMedia[_rand];
            else if (_playlist.ListMedia.IndexOf(_media) == 0 && _repeatState == repeatStatus.REPEAT_ALL)
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
                if (_repeatState != repeatStatus.NONE || _isRandEnabled)
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
                if (_repeatState != repeatStatus.NONE || _isRandEnabled)
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

        private void RandCmd()
        {
            _isRandEnabled = !_isRandEnabled;
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
                if (_player.NaturalDuration.HasTimeSpan)
                    _progress.Stop();
                _player.Pause();
            }
            else
            {
                _media.isStopped = false;
                if (_player.NaturalDuration.HasTimeSpan)
                {
                    _progress.Start();
                }
                _player.Play();
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
