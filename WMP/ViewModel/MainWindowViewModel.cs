﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WMP
{
    public class MainWindowViewModel : ViewModelBase
    {
        Window                  _tips;
        Window                  _about;
        List<ViewModelBase>     _pages;
        ViewModelBase           _currentPage;
        Konami                  _k;

        public MainWindowViewModel()
        {
            _about = null;
            _tips = null;
            _k = new Konami();
            _pages = new List<ViewModelBase>();
            _pages.Add(new MainViewModel(this) { IsCurrentPage = true });
            _pages.Add(new PlaylistViewModel(this));
            _currentPage = _pages[0];
        }

        #region Events

        private void OnCloseHelp(object sender, EventArgs e)
        {
            _about = null;
        }

        private void OnCloseTips(object sender, EventArgs e)
        {
            _tips = null;
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

        #region Playlist

        public void ChangePage()
        {
            if (_pages[0].IsCurrentPage)
            {
                _pages[1].IsCurrentPage = true;
                _pages[0].IsCurrentPage = false;

                _currentPage = _pages[1];
            }
            else
            {
                _pages[0].IsCurrentPage = true;
                _pages[1].IsCurrentPage = false;

                _currentPage = _pages[0];
            }
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
            if (_k.StepKonami(k) == true && _pages[0].IsCurrentPage == true)
            {
                Console.WriteLine("Call ProcessKonami");
                ((MainViewModel)_currentPage).ProcessKonami();
            }
        }

        #endregion
    }
}