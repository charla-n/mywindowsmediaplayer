using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using WMP.Model;
using System.Timers;
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
        String                      _search;
        WMP.Model.MediaFilter       _typeFilter;

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

        RelayCommand _addvideoscmd;
        RelayCommand _addsongscmd;
        RelayCommand _addpicturescmd;
        
        RelayCommand _clearvideos;
        RelayCommand _clearsongs;
        RelayCommand _clearpictures;
        
        RelayCommand _deletevideocmd;
        RelayCommand _deletesongcmd;
        RelayCommand _deletepicturecmd;
        
        RelayCommand _addsongtoplaylist;
        RelayCommand _addvideotoplaylist;
        RelayCommand _addpicturetoplaylist;
        RelayCommand _addToPlaylist;
        
        RelayCommand _playmediacmd;
        RelayCommand _addcmd;
        RelayCommand _clearallcmd;

        public ObservableCollection<VideoMedia> ListVideos { get; private set; }
        public ObservableCollection<AudioMedia> ListSongs { get; private set; }
        public ObservableCollection<PictureMedia> ListPictures { get; private set; }

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

            _typeFilter = WMP.Model.MediaFilter.TITLE;
            _playmediacmd = new RelayCommand(playMediaCmd, () => true);
            _addcmd = new RelayCommand(addCmd, () => true);
            _addvideoscmd = new RelayCommand(addVideoCmd, () => true);
            _addsongscmd = new RelayCommand(addSongCmd, () => true);
            _addpicturescmd = new RelayCommand(addPictureCmd, () => true);
            _addsongtoplaylist = new RelayCommand(addSongToPlayList, () => true);
            _addvideotoplaylist = new RelayCommand(addVideoToPlayList, () => true);
            _addpicturetoplaylist = new RelayCommand(addPictureToPlayList, () => true);
            _addToPlaylist = new RelayCommand(addToPlaylistCmd, () => true);
            _deletevideocmd = new RelayCommand(deleteVideoCmd, () => true);
            _deletesongcmd = new RelayCommand(deleteSongCmd, () => true);
            _deletepicturecmd = new RelayCommand(deletePictureCmd, () => true);
            _clearvideos = new RelayCommand(clearVideos, () => true);
            _clearpictures = new RelayCommand(clearPictures, () => true);
            _clearsongs = new RelayCommand(clearSongs, () => true);
            _clearallcmd = new RelayCommand(clearAll, () => true);

            ListVideos = new ObservableCollection<VideoMedia>();
            ListSongs = new ObservableCollection<AudioMedia>();
            ListPictures = new ObservableCollection<PictureMedia>();

            _rand = 0;
            _playlist = playlist;
            _isRandEnabled = false;
            _repeatState = repeatStatus.NONE    ;
            _model = model;
            _media = null;
            _search = "";
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

            loadPicturesLibrary();
            loadSongsLibrary();
            loadVideosLibrary();
        }


        #region Library

        public string SearchTxt
        {
            get
            {
                return _search;
            }
            set
            {
                _search = value;
                ListVideos.Clear();
                ListSongs.Clear();
                ListPictures.Clear();
                loadPicturesLibrary();
                loadSongsLibrary();
                loadVideosLibrary();
            }
           
        }

        public MediaFilter ChangeTypeFilter
        {
            get
            {
                return _typeFilter;
            }
            set
            {
                _typeFilter = value;
                ListVideos.Clear();
                ListSongs.Clear();
                ListPictures.Clear();
                loadPicturesLibrary();
                loadSongsLibrary();
                loadVideosLibrary();
            }

        }

        private void loadVideosLibrary()
        {
            try
            {
                using (FileStream stream = new FileStream(@"../../library/videos.xml", FileMode.OpenOrCreate, FileAccess.Read))
                {
                    TextReader reader = new StreamReader(stream);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<VideoMedia>));
                    List<VideoMedia> list = (List<VideoMedia>)serializer.Deserialize(reader);
                    foreach (VideoMedia m in list)
                    {
                        if (m.Title == null)
                            m.Title = Path.GetFileNameWithoutExtension(m.FileName);
                        m.Icon = ExtensionStatic.GetIconsFromExtension(Path.GetExtension(m.FileName));
                        if (m.isDisplayable(_search, _typeFilter))
                            ListVideos.Add(m);
                    }
                };
            }
            catch (Exception)
            {
                //MessageBox.Show("Error occured when loading library" + Environment.NewLine + "Be sure you've correct permissions or you open a well-formated file", "Library Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void loadSongsLibrary()
        {
            try
            {
                using (FileStream stream = new FileStream(@"../../library/songs.xml", FileMode.OpenOrCreate, FileAccess.Read))
                {
                    TextReader reader = new StreamReader(stream);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<AudioMedia>));

                    List<AudioMedia> list = (List<AudioMedia>)serializer.Deserialize(reader);
                    foreach (AudioMedia m in list)
                    {
                        if (m.Title == null)
                            m.Title = Path.GetFileNameWithoutExtension(m.FileName);
                        System.Console.WriteLine("ddddd-> : " + m.Album);
                        m.Icon = ExtensionStatic.GetIconsFromExtension(Path.GetExtension(m.FileName));
                        if (m.isDisplayable(_search, _typeFilter))
                            ListSongs.Add(m);
                    }
                };
            }
            catch (Exception)
            {
                //MessageBox.Show("Error occured when loading library" + Environment.NewLine + "Be sure you've correct permissions or you open a well-formated file", "Library Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void loadPicturesLibrary()
        {
            try
            {
                using (FileStream stream = new FileStream(@"../../library/pictures.xml", FileMode.OpenOrCreate, FileAccess.Read))
                {
                    TextReader reader = new StreamReader(stream);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<PictureMedia>));

                    List<PictureMedia> list = (List<PictureMedia>)serializer.Deserialize(reader);
                    foreach (PictureMedia m in list)
                    {
                        if (m.Title == null)
                            m.Title = Path.GetFileNameWithoutExtension(m.FileName);
                        m.Icon = ExtensionStatic.GetIconsFromExtension(Path.GetExtension(m.FileName));
                        if (m.isDisplayable(_search, _typeFilter)) 
                            ListPictures.Add(m);
                    }
                };
            }
            catch (Exception)
            {
                //MessageBox.Show("Error occured when loading library" + Environment.NewLine + "Be sure you've correct permissions or you open a well-formated file", "Library Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ICommand AddToPlaylist
        {
            get
            {
                return _addToPlaylist;
            }
        }

        public ICommand PlayMedia
        {
            get
            {
                return _playmediacmd;
            }
        }

        public ICommand AddSongToPlaylist
        {
            get
            {
                return _addsongtoplaylist;
            }
        }

        public ICommand AddVideoToPlaylist
        {
            get
            {
                return _addvideotoplaylist;
            }
        }

        public ICommand AddPictureToPlaylist
        {
            get
            {
                return _addpicturetoplaylist;
            }
        }

        public ICommand add
        {
            get
            {
                return _addcmd;
            }
        }

        public ICommand addVideo
        {
            get
            {
                return _addvideoscmd;
            }
        }

        public ICommand addSong
        {
            get
            {
                return _addsongscmd;
            }
        }

        public ICommand addPicture
        {
            get
            {
                return _addpicturescmd;
            }
        }

        public ICommand DeleteVideo
        {
            get
            {
                return _deletevideocmd;
            }
        }

        public ICommand DeletePicture
        {
            get
            {
                return _deletesongcmd;
            }
        }

        public ICommand DeleteSong
        {
            get
            {
                return _deletesongcmd;
            }
        }

        public ICommand ClearVideos
        {
            get
            {
                return _clearvideos;
            }
        }

        public ICommand ClearSongs
        {
            get
            {
                return _clearsongs;
            }
        }

        public ICommand ClearPictures
        {
            get
            {
                return _clearpictures;
            }
        }

        public ICommand ClearAll
        {
            get
            {
                return _clearallcmd;
            }
        }

        private void addToPlaylistCmd()
        {
            //addSongToPlayList();
            //addPictureToPlayList();
            //addVideoToPlayList();
        }

        private void playMediaCmd()
        {
            IEnumerable<Media> lv = ListVideos.Where(m => m.IsSelected == true);
            IEnumerable<Media> lm = ListSongs.Where(m => m.IsSelected == true);
            IEnumerable<Media> lp = ListPictures.Where(m => m.IsSelected == true);

            if (lv.Count() > 0)
            {
                _player.Stop();
                _media = lv.ElementAt(0);
                OnOpenMedia(_media.FileName);
            }
            else if (lm.Count() > 0)
            {
                _player.Stop();
                _media = lm.ElementAt(0);
                OnOpenMedia(_media.FileName);
            }
            else if (lp.Count() > 0)
            {
                _player.Stop();
                _media = lp.ElementAt(0);
                OnOpenMedia(_media.FileName);
            }
        }

        private void addSongToPlayList()
        {
            IEnumerable<Media> lm = ListSongs.Where(m => m.IsSelected == true);

            foreach (Media m in lm)
            {
                _playlist.ListMedia.Add(m);
            }
        }

        private void addVideoToPlayList()
        {
            IEnumerable<Media> lm = ListVideos.Where(m => m.IsSelected == true);

            foreach (Media m in lm)
            {
                _playlist.ListMedia.Add(m);
            }
        }

        private void addPictureToPlayList()
        {
            IEnumerable<Media> lm = ListPictures.Where(m => m.IsSelected == true);

            foreach (Media m in lm)
            {
                _playlist.ListMedia.Add(m);
            }
        }

        private void clearPictures()
        {
            ListPictures.Clear();
            savePictures();
        }

        private void clearSongs()
        {
            ListSongs.Clear();
            saveSongs();
        }

        private void clearVideos()
        {
            ListVideos.Clear();
            saveVideos();
        }

        private void clearAll()
        {
            clearSongs();
            clearVideos();
            clearPictures();
        }

        private void saveVideos()
        {
            try
            {
                using (FileStream stream = new FileStream(@"../../library/videos.xml", FileMode.Truncate))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<VideoMedia>));

                    serializer.Serialize(stream, ListVideos.ToList());
                };
            }
            catch (Exception)
            {
                MessageBox.Show("Error occured when saving songs library" + Environment.NewLine + "Be sure you've correct permissions", "Playlist Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void saveSongs()
        {
            try
            {
                using (FileStream stream = new FileStream(@"../../library/songs.xml", FileMode.Truncate))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<AudioMedia>));

                    serializer.Serialize(stream, ListSongs.ToList());
                };
            }
            catch (Exception)
            {
                MessageBox.Show("Error occured when saving songs library" + Environment.NewLine + "Be sure you've correct permissions", "Library Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void savePictures()
        {
            try
            {
                using (FileStream stream = new FileStream(@"../../library/pictures.xml", FileMode.OpenOrCreate | FileMode.Truncate, FileAccess.ReadWrite))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<PictureMedia>));

                    serializer.Serialize(stream, ListPictures.ToList());
                };
            }
            catch (Exception)
            {
                MessageBox.Show("Error occured when saving pictures library" + Environment.NewLine + "Be sure you've correct permissions", "Library Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void deleteVideoCmd()
        {
            List<VideoMedia> tmp = new List<VideoMedia>();
            IEnumerable<VideoMedia> lm = ListVideos.Where(m => m.IsSelected == true);


            foreach (VideoMedia m in lm)
            {
                tmp.Add(m);
            }
            foreach (VideoMedia m in tmp)
            {
                ListVideos.Remove(m);
            }
            saveVideos();
        }

        private void addVideoCmd()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? res;

            dialog.Multiselect = true;
            dialog.Filter = "Video files|*.wmv;*.avi;*.mpg;*.mov;*.asf;*.mkv";
            res = dialog.ShowDialog();
            if (res == true)
            {
                foreach (string file in dialog.FileNames)
                {
                    VideoMedia tmp = Media.CreateMedia(false, file, false, ExtensionStatic.GetIconsFromExtension(Path.GetExtension(file))) as VideoMedia;

                    if (tmp != null)
                        ListVideos.Add(tmp);
                }
            }
            saveVideos();
        }

        private void deleteSongCmd()
        {
            List<AudioMedia> tmp = new List<AudioMedia>();
            IEnumerable<AudioMedia> lm = ListSongs.Where(m => m.IsSelected == true);


            foreach (AudioMedia m in lm)
            {
                tmp.Add(m);
            }
            foreach (AudioMedia m in tmp)
            {
                ListSongs.Remove(m);
            }
            saveSongs();
        }

        private void addSongCmd()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? res;

            dialog.Multiselect = true;
            dialog.Filter = "Audio files|*.mp3;*.wav;*.wma;*.ogg";
            res = dialog.ShowDialog();
            if (res == true)
            {
                foreach (string file in dialog.FileNames)
                {
                    AudioMedia tmp = Media.CreateMedia(false, file, false, ExtensionStatic.GetIconsFromExtension(Path.GetExtension(file))) as AudioMedia;

                    if (tmp != null)
                        ListSongs.Add(tmp);
                }
            }
            saveSongs();
        }

        private void deletePictureCmd()
        {
            List<PictureMedia> tmp = new List<PictureMedia>();
            IEnumerable<PictureMedia> lm = ListPictures.Where(m => m.IsSelected == true);


            foreach (PictureMedia m in lm)
            {
                tmp.Add(m);
            }
            foreach (PictureMedia m in tmp)
            {
                ListPictures.Remove(m);
            }
            savePictures();
        }

        private void addCmd()
        {
            //OpenFileDialog dialog = new OpenFileDialog();
            //bool? res;

            //dialog.Multiselect = true;
            //dialog.Filter = "Video files|*.wmv;*.avi;*.mpg;*.mov;*.asf;*.mkv|Audio files|*.mp3;*.wav;*.wma;*.ogg|Picture Files|*.jpg;*.bmp;*.png|ALL files|*.*";
            //res = dialog.ShowDialog();

            //if (res == true)
            //{
            //    foreach (string file in dialog.FileNames)
            //    {
            //        string ext = Path.GetExtension(file);
            //        if (ext == ".wmv" || ext == ".avi" || ext == ".mpg" || ext == ".mov" || ext == ".mkv" || ext == ".asf")
            //            ListVideos.Add(new VideoMedia { FileName = file, isPlaying = false, Icon = ExtensionStatic.GetIconsFromExtension(Path.GetExtension(file)), Title = Path.GetFileNameWithoutExtension(file) });
            //        else if (ext == ".mp3" || ext == ".wav" || ext == ".wma" || ext == ".ogg")
            //            ListSongs.Add(new AudioMedia { FileName = file, isPlaying = false, Icon = ExtensionStatic.GetIconsFromExtension(Path.GetExtension(file)), Title = Path.GetFileNameWithoutExtension(file) });
            //        else if (ext == ".png" || ext == ".bmp" || ext == ".jpg")
            //            ListPictures.Add(new PictureMedia { FileName = file, isPlaying = false, Icon = ExtensionStatic.GetIconsFromExtension(Path.GetExtension(file)), Title = Path.GetFileNameWithoutExtension(file) });
            //        else
            //            MessageBox.Show(file + "file type not recognized !");
            //    }
            //}
            //savePictures();
            //saveVideos();
            //saveSongs();
        }

        private void addPictureCmd()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? res;

            dialog.Multiselect = true;
            dialog.Filter = "Picture Files|*.jpg;*.bmp;*.png";
            res = dialog.ShowDialog();
            if (res == true)
            {
                foreach (string file in dialog.FileNames)
                {
                    PictureMedia tmp = Media.CreateMedia(false, file, false, ExtensionStatic.GetIconsFromExtension(Path.GetExtension(file))) as PictureMedia;

                    if (tmp != null)
                        ListPictures.Add(tmp);
                }
            }
            savePictures();
        }

        #endregion

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
                Console.WriteLine("Can't next");
                _media.isPlaying = false;
                _progress.Stop();
                _player.Stop();
            }
            else
            {
                Console.WriteLine("Can next");
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
            if (_media != null && _media.MediaType != t_MediaType.NONE)
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
                _mediaInfos[(int)_media.MediaType].DataContext = _model;
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
                    if (_isRandEnabled && _playlist.ListMedia.Count > 1)
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
            if (_playlist.ListMedia.Count == 1)
                MediaLoaded(this, null);
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
                if (_repeatState != repeatStatus.NONE || (_isRandEnabled && _playlist.ListMedia.Count > 1))
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
                if (_repeatState != repeatStatus.NONE || (_isRandEnabled && _playlist.ListMedia.Count > 1))
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
