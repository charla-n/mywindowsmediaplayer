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

namespace WMP
{
    public class PlaylistViewModel : ViewModelBase
    {
        MainWindowViewModel         _model;

        public PlaylistViewModel(MainWindowViewModel model)
        {
            ListMedia = new ObservableCollection<Media>();
            _model = model;
        }

        #region Properties

        public ObservableCollection<Media> ListMedia { get; private set; }

        public ICommand SavePlaylist
        {
            get
            {
                return new RelayCommand(SavePlaylistCmd, CanSavePlaylist);
            }
        }

        public ICommand AddFromFile
        {
            get
            {
                return new RelayCommand(AddFromFileCmd, () => true);
            }
        }

        public ICommand Add
        {
            get
            {
                return new RelayCommand(AddCmd, () => true);
            }
        }

        public ICommand Delete
        {
            get
            {
                return new RelayCommand(DeleteCmd, () => true);
            }
        }

        public ICommand Clear
        {
            get
            {
                return new RelayCommand(ClearCmd, () => true);
            }
        }

        public ICommand Playlist
        {
            get
            {
                return new RelayCommand(PlaylistCmd, () => true);
            }
        }

        #endregion

        #region Commands

        private void SavePlaylistCmd()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            bool? res;

            dlg.Filter = "Playlist Files|*.pls";
            res = dlg.ShowDialog();
            if (res == true)
            {
                try
                {
                    using (FileStream stream = new FileStream(dlg.FileName, FileMode.OpenOrCreate))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Media>));

                        serializer.Serialize(stream, ListMedia.ToList());
                    };
                }
                catch (Exception)
                {
                    MessageBox.Show("Error occured when saving playlist" + Environment.NewLine + "Be sure you've correct permissions", "Playlist Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanSavePlaylist()
        {
            if (ListMedia.Count > 0)
                return true;
            return false;
        }

        private void AddFromFileCmd()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool? res;

            dlg.Filter = "Playlist Files|*.pls";
            res = dlg.ShowDialog();
            if (res == true)
            {
                try
                {
                    using (FileStream stream = new FileStream(dlg.FileName, FileMode.Open))
                    {
                        TextReader reader = new StreamReader(stream);
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Media>));

                        List<Media> list = (List<Media>)serializer.Deserialize(reader);
                        foreach (Media m in list)
                        {
                            ListMedia.Add(m);
                        }
                    };
                }
                catch (Exception)
                {
                    MessageBox.Show("Error occured when loading playlist" + Environment.NewLine + "Be sure you've correct permissions or you open a well-formated file", "Playlist Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddCmd()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? res;

            dialog.Filter = "Video files|*.avi;*.mpg;*.mov;*.asf|Audio files|*.mp3;*.wav;*.wma;*.ogg;*.pls|Picture Files|*.jpg;*.bmp;*.png|ALL files|*.*";
            res = dialog.ShowDialog();
            if (res == true)
            {
                ListMedia.Add(new Media { FileName = dialog.FileName, isPlaying = false });
            }
        }

        private void DeleteCmd()
        {
            List<Media> tmp = new List<Media>();
            IEnumerable<Media> lm = ListMedia.Where(m => m.IsSelected == true);

           
            foreach (Media m in lm)
            {
                tmp.Add(m);
            }
            foreach (Media m in tmp)
            {
                ListMedia.Remove(m);
            }
        }

        private void ClearCmd()
        {
            ListMedia.Clear();
        }

        private void PlaylistCmd()
        {
            _model.ChangePage();
        }

        #endregion
    }
}
