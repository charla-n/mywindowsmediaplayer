using System;
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
using System.Windows.Shapes;

namespace WMP
{
    /// <summary>
    /// Interaction logic for Streaming.xaml
    /// </summary>
    public partial class Streaming : Window
    {
        public Streaming()
        {
            InitializeComponent();
        }

        public string StreamPath
        {
            get
            {
                return textbox.Text;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
