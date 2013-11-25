using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMP.Model;

namespace WMP
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void OnChangeView() 
        {
            Console.WriteLine("ON CHANGE VIEW MODEL BASE");
        }

        public bool IsCurrentPage { get; set; }

        public ViewModelBase CurrentPageBase { get; set; }

    }
}
