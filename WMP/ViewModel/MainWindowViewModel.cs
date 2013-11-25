using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WMP.Model;

namespace WMP
{
    public class MainWindowViewModel : ViewModelBase
    {
        string                      _streamingName;
        Window                      _tips;
        Window                      _about;
        Window                      _stream;
        List<ViewModelBase>         _pages;
        ViewModelBase               _currentPage;
        Konami                      _k;

        RelayCommand _openStreamingcmd;
        RelayCommand _openMediacmd;
        RelayCommand _quitcmd;
        RelayCommand _aboutcmd;
        RelayCommand _tipscmd;
        RelayCommand _konamiUpcmd;
        RelayCommand _konamiDowncmd;
        RelayCommand _konamiRightcmd;
        RelayCommand _konamiLeftcmd;
        RelayCommand _konamiAcmd;
        RelayCommand _konamiBcmd;

        public MainWindowViewModel()
        {
            _openStreamingcmd = new RelayCommand(OpenStreamingCmd, CanStreaming);
            _openMediacmd = new RelayCommand(OpenMediaCmd, () => true);
            _quitcmd = new RelayCommand(QuitCmd, () => true);
            _aboutcmd = new RelayCommand(AboutCmd, () => true);
            _tipscmd = new RelayCommand(TipsCmd, () => true);
            _konamiAcmd = new RelayCommand(KonamiCmd, () => true);
            _konamiBcmd = new RelayCommand(KonamiCmd, () => true);
            _konamiDowncmd = new RelayCommand(KonamiCmd, () => true);
            _konamiLeftcmd = new RelayCommand(KonamiCmd, () => true);
            _konamiRightcmd = new RelayCommand(KonamiCmd, () => true);
            _konamiUpcmd = new RelayCommand(KonamiCmd, () => true);

            PlaylistViewModel playlist = new PlaylistViewModel(this);
            ViewModelBase mainView = new MainViewModel(this, playlist) { IsCurrentPage = true };

            _about = null;
            _tips = null;
            _stream = null;
            _k = new Konami();
            _pages = new List<ViewModelBase>();
            _pages.Add(mainView);
            _pages.Add(playlist);
            _currentPage = _pages[0];
        }

        #region Events

        private void OnCloseHelp(object sender, EventArgs e)
        {
            Window w = sender as Window;

            w.Close();
            _about = null;
        }

        private void OnCloseTips(object sender, EventArgs e)
        {
            Window w = sender as Window;

            w.Close();
            _tips = null;
        }

        private void OnCloseStream(object sender, EventArgs e)
        {
            Streaming stream = sender as Streaming;

            _streamingName = stream.StreamPath;
            stream.Close();
            _stream = null;
        }

        #endregion

        #region Properties

        public ViewModelBase CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
            }
        }

        #endregion

        #region Menu

        public ICommand OpenStreaming
        {
            get
            {
                return _openStreamingcmd;
            }
        }

        public ICommand OpenMedia
        {
            get
            {
                return _openMediacmd;
            }
        }

        public ICommand Quit
        {
            get
            {
                return _quitcmd;
            }
        }

        public ICommand About
        {
            get
            {
                return _aboutcmd;
            }
        }

        public ICommand Tips
        {
            get
            {
                return _tipscmd;
            }
        }

        #endregion

        #region CommandMenu

        private void OpenStreamingCmd()
        {
            _stream = new Streaming();
            _stream.Closed += OnCloseStream;
            if (_stream.ShowDialog() == true)
            {
                ((MainViewModel)_currentPage).OnOpenMedia(_streamingName);
            }
        }

        private bool CanStreaming()
        {
            return _stream == null && _pages[0].IsCurrentPage == true ? true : false;
        }

        private void TipsCmd()
        {
            if (_tips == null)
            {
                _tips = new Tips();

                _tips.Closed += OnCloseTips;
                _tips.Show();
            }
        }

        private void AboutCmd()
        {
            if (_about == null)
            {
                _about = new About();

                _about.Closed += OnCloseHelp;
                _about.Show();
            }
        }

        private void QuitCmd()
        {
            Application.Current.Shutdown();
        }

        private void OpenMediaCmd()
        {
            if (_pages[0].IsCurrentPage == true)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                bool? res;

                dialog.Filter = "Video files|*.avi;*.mpg;*.mov;*.asf|Audio files|*.mp3;*.wav;*.wma;*.ogg;*.pls|Picture Files|*.jpg;*.bmp;*.png|ALL files|*.*";
                res = dialog.ShowDialog();
                if (res == true)
                {
                    ((MainViewModel)_currentPage).OnOpenMedia(dialog.FileName);
                }
            }
        }

        #endregion

        #region Pages

        public void ChangePage()
        {
            if (_pages[0].IsCurrentPage)
            {
                _currentPage.IsCurrentPage = false;
                _pages[1].IsCurrentPage = true;
                _currentPage = _pages[1];
            }
            else
            {
                _currentPage.IsCurrentPage = false;
                _pages[0].IsCurrentPage = true;
                _currentPage = _pages[0];
                ((MainViewModel)_currentPage).OnAddPlaylist();
            }
            OnPropertyChanged("CurrentPage");
        }

        #endregion

        #region Konami

        public ICommand KonamiUp
        {
            get
            {
                return _konamiUpcmd;
            }
        }

        public ICommand KonamiDown
        {
            get
            {
                return _konamiDowncmd;
            }
        }

        public ICommand KonamiLeft
        {
            get
            {
                return _konamiLeftcmd;
            }
        }

        public ICommand KonamiRight
        {
            get
            {
                return _konamiRightcmd;
            }
        }

        public ICommand KonamiA
        {
            get
            {
                return _konamiAcmd;
            }
        }

        public ICommand KonamiB
        {
            get
            {
                return _konamiBcmd;
            }
        }

        private void KonamiCmd(object key)
        {
            string cmd = key as string;
            Key k;

            if (Enum.TryParse<Key>(cmd, out k) == false)
                return;
            if (_k.StepKonami(k) == true && _pages[0].IsCurrentPage == true)
            {
                Console.WriteLine("Call ProcessKonami");
                ((MainViewModel)_currentPage).ProcessKonami();
            }
        }

        #endregion
    }
}
