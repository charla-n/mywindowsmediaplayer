﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WMP
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl
    {
        Point _p;

        public Playlist()
        {
            InitializeComponent();
        }

        private void listView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _p = e.GetPosition(null);
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            PlaylistViewModel ctx = (PlaylistViewModel)DataContext;

            ctx.DeleteCmd();
        }

        private void listView_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = e.GetPosition(null);
            Vector diff = _p - mousePoint;
 
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                Console.WriteLine("COUCOU");
                if (listView.SelectedItems.Count > 0)
                {
                    Console.WriteLine("PREVIEW MOUSE LEFT BUTTON DOWN");
                    DataObject obj = new DataObject("myFormat", listView.SelectedItems.Cast<object>());
                    DragDrop.DoDragDrop(listView, obj, DragDropEffects.Move);
                }
            }
        }
    }
}
