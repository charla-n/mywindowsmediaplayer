using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WMP
{
    public class PlaceHolderTextBox : TextBox
    {
        private string _placeHolder;

        public string PlaceHolder
        {
            get
            {
                return _placeHolder;
            }
            set
            {
                _placeHolder = value;
                if (IsActive == true)
                    Text = _placeHolder;
            }
        }
        public bool IsActive { get; set; }

        public PlaceHolderTextBox()
        {
            IsActive = true;
        }

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsActive == true)
            {
                IsActive = false;
                Text = "";
            }
            base.OnMouseDown(e);
        }

        protected override void OnLostFocus(System.Windows.RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(Text))
            {
                IsActive = true;
                Text = PlaceHolder;
            }
            base.OnLostFocus(e);
        }
    }
}
