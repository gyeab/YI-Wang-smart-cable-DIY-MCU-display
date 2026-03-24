using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingForce
{
    class WFChannelData : INotifyPropertyChanged
    {
        public int Id; 
        public string? ChannelName { get; set; }
        public double _voltage = 0;
        public double _current = 0;


        public event PropertyChangedEventHandler PropertyChanged;

        public double current
        {
            get { return _current; }
            set
            {
                _current = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("current"));
            }
        }

        public double voltage {

            get { return _voltage; }
            set
            {
                _voltage = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("voltage"));
                }

            }
        }

    }
}
