using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace Server
{
    public class WiiSatBO : INotifyPropertyChanged
    {
        private Visibility WiimoteConnected = Visibility.Hidden;
        public Visibility WiimoteVisibility
        {
            get 
            {
                return WiimoteConnected; 
            }
            set 
            {
                WiimoteConnected = value;
                DoPropertyChanged("WiimoteConnected"); 
            }
        }

        private Visibility NunchuckConnected = Visibility.Hidden;
        public Visibility NunchuckVisibility
        {
            get
            {
                return NunchuckConnected;
            }
            set
            {
                NunchuckConnected = value;
                if (value == Visibility.Visible)
                {
                    ClassicControllerConnected = Visibility.Hidden;
                    DoPropertyChanged("ClassicControllerConnected");
                }
                DoPropertyChanged("NunchuckConnected");
            }
        }

        private Visibility ClassicControllerConnected = Visibility.Hidden;
        public Visibility ClassicControllerVisibility
        {
            get
            {
                return ClassicControllerConnected;
            }
            set
            {
                ClassicControllerConnected = value;
                if (value == Visibility.Visible)
                {
                    NunchuckConnected = Visibility.Hidden;
                    DoPropertyChanged("NunchuckConnected");
                    NunchukShadow = Visibility.Hidden;
                    DoPropertyChanged("NunchukShadow");
                }
                else
                {
                    NunchukShadow = Visibility.Visible;
                    DoPropertyChanged("NunchukShadow");
                }
                DoPropertyChanged("ClassicControllerConnected");
            }
        }

        private Visibility NunchukShadow = Visibility.Visible;
        public Visibility NunchukShadowVisibility
        {
            get
            {
                return NunchukShadow;
            }
            set
            {
                NunchukShadow = value;
            }
        }
        //
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void DoPropertyChanged(string propnam)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propnam));
        }
        #endregion
    }
}
