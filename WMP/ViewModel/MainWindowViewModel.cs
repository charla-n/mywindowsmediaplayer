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
        public enum PageEnum
        {
            MAIN,
            PLAYLIST,
            COUNT_PAGE
        };

        string                      _streamingName;
        ViewModelBase[]             _page;
        Window                      _tips;
        Window                      _about;
        Window                      _stream;
        Konami                      _k;

        public MainWindowViewModel()
        {
            PlaylistViewModel playlist = new PlaylistViewModel(this);
            ViewModelBase mainView = new MainViewModel(this, playlist) { IsCurrentPage = true };

            _about = null;
            _tips = null;
            _stream = null;
            _k = new Konami();
            _page = new ViewModelBase[(int)PageEnum.COUNT_PAGE];
            _page[(int)PageEnum.MAIN] = mainView;
            _page[(int)PageEnum.PLAYLIST] = playlist;
            CurrentPageBase = _page[(int)PageEnum.MAIN];
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
                return CurrentPageBase;
            }
            set
            {
                CurrentPageBase = value;
            }
        }

        #endregion

        #region Menu

        public ICommand OpenStreaming
        {
            get
            {
                return new RelayCommand(OpenStreamingCmd, CanStreaming);
            }
        }

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

        public ICommand About
        {
            get
            {
                return new RelayCommand(AboutCmd, () => true);
            }
        }

        public ICommand Tips
        {
            get
            {
                return new RelayCommand(TipsCmd, () => true);
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
                ((MainViewModel)CurrentPageBase).OnOpenMedia(_streamingName);
            }
        }

        private bool CanStreaming()
        {
            return _stream == null && _page[(int)PageEnum.MAIN].IsCurrentPage == true ? true : false;
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
            if (_page[(int)PageEnum.MAIN].IsCurrentPage == true)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                bool? res;

                dialog.Filter = "Video files|*.avi;*.mpg;*.mov;*.asf|Audio files|*.mp3;*.wav;*.wma;*.ogg;*.pls|Picture Files|*.jpg;*.bmp;*.png|ALL files|*.*";
                res = dialog.ShowDialog();
                if (res == true)
                {
                    ((MainViewModel)CurrentPageBase).OnOpenMedia(dialog.FileName);
                }
            }
        }

        #endregion

        #region Pages

        public void ChangePage(PageEnum e)
        {
            for (int i = 0; i < (int)PageEnum.COUNT_PAGE; ++i)
            {
                CurrentPageBase.IsCurrentPage = false;
                _page[i].IsCurrentPage = false;
            }
            CurrentPageBase = _page[(int)e];
            _page[(int)e].IsCurrentPage = true;
            OnChangeView();
            OnPropertyChanged("CurrentPage");
        }

        #endregion

        #region Konami

        public ICommand KonamiUp
        {
            get
            {
                return new RelayCommand(KonamiCmd, () => true);
            }
        }

        public ICommand KonamiDown
        {
            get
            {
                return new RelayCommand(KonamiCmd, () => true);
            }
        }

        public ICommand KonamiLeft
        {
            get
            {
                return new RelayCommand(KonamiCmd, () => true);
            }
        }

        public ICommand KonamiRight
        {
            get
            {
                return new RelayCommand(KonamiCmd, () => true);
            }
        }

        public ICommand KonamiA
        {
            get
            {
                return new RelayCommand(KonamiCmd, () => true);
            }
        }

        public ICommand KonamiB
        {
            get
            {
                return new RelayCommand(KonamiCmd, () => true);
            }
        }

        private void KonamiCmd(object key)
        {
            string cmd = key as string;
            Key k;

            if (Enum.TryParse<Key>(cmd, out k) == false)
                return;
            if (_k.StepKonami(k) == true && _page[(int)PageEnum.MAIN].IsCurrentPage == true)
            {
                Console.WriteLine("Call ProcessKonami");
                ((MainViewModel)CurrentPageBase).ProcessKonami();
            }

        }

        #endregion
    }
}
