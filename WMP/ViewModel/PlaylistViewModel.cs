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

        RelayCommand _savePlaylistcmd;
        RelayCommand _addFromFilecmd;
        RelayCommand _addcmd;
        RelayCommand _deletecmd;
        RelayCommand _clearcmd;
        RelayCommand _playlistcmd;

        public PlaylistViewModel(MainWindowViewModel model)
        {
            _savePlaylistcmd = new RelayCommand(SavePlaylistCmd, CanSavePlaylist);
            _addFromFilecmd = new RelayCommand(AddFromFileCmd, () => true);
            _addcmd = new RelayCommand(AddCmd, () => true);
            _deletecmd = new RelayCommand(DeleteCmd, () => true);
            _clearcmd = new RelayCommand(ClearCmd, () => true);
            _playlistcmd = new RelayCommand(PlaylistCmd, () => true);

            ListMedia = new ObservableCollection<Media>();
            _model = model;
        }

        #region Properties

        public ObservableCollection<Media> ListMedia { get; private set; }

        public ICommand SavePlaylist
        {
            get
            {
                return _savePlaylistcmd;
            }
        }

        public ICommand AddFromFile
        {
            get
            {
                return _addFromFilecmd;
            }
        }

        public ICommand Add
        {
            get
            {
                return _addcmd;
            }
        }

        public ICommand Delete
        {
            get
            {
                return _deletecmd;
            }
        }

        public ICommand Clear
        {
            get
            {
                return _clearcmd;
            }
        }

        public ICommand Playlist
        {
            get
            {
                return _playlistcmd;
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
                    using (FileStream stream = new FileStream(dlg.FileName, FileMode.OpenOrCreate | FileMode.Truncate))
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
                            ListMedia.Add(Media.CreateMedia(false, m.FileName, true, ExtensionStatic.GetIconsFromExtension(Path.GetExtension(m.FileName))));
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

            dialog.Multiselect = true;
            dialog.Filter = "Video files|*.avi;*.mpg;*.mov;*.asf;*.mkv|Audio files|*.mp3;*.wav;*.wma;*.ogg|Picture Files|*.jpg;*.bmp;*.png|ALL files|*.*";
            res = dialog.ShowDialog();
            if (res == true)
            {
                foreach (string file in dialog.FileNames)
                {
                    ListMedia.Add(Media.CreateMedia(false, file, false, ExtensionStatic.GetIconsFromExtension(Path.GetExtension(file))));
                }
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
            _model.ChangePage(MainWindowViewModel.PageEnum.MAIN);
        }

        #endregion
    }
}
