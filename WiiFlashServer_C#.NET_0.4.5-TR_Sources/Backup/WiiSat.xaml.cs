using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.ComponentModel;

namespace Server
{
	public partial class WiiSat
	{
        public int id;

		public WiiSat()
		{
			this.InitializeComponent();
			// Insert code required on object creation below this point.
		}

        [Bindable(true)]
        [Category("WiiState")]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Visibility WiimoteVisibility
        {
            get 
            {
                return (Visibility)GetValue(WiimoteVisibilityProperty); 
            }
            set 
            {
                SetValue(WiimoteVisibilityProperty, value); 
            }
        }
        // Using a DependencyProperty as the backing store for WiiVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WiimoteVisibilityProperty = DependencyProperty.Register("WiimoteVisibility", typeof(Visibility), typeof(WiiSat), new UIPropertyMetadata(Visibility.Visible));


        [Bindable(true)]
        [Category("WiiState")]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Visibility NunchuckVisibility
        {
            get
            {
                return (Visibility)GetValue(NunchuckVisibilityProperty);
            }
            set
            {
                SetValue(NunchuckVisibilityProperty, value);
            }
        }
        // Using a DependencyProperty as the backing store for WiiVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NunchuckVisibilityProperty = DependencyProperty.Register("NunchuckVisibility", typeof(Visibility), typeof(WiiSat), new UIPropertyMetadata(Visibility.Visible));
       
        
        [Bindable(true)]
        [Category("WiiState")]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Visibility ClassicControllerVisibility
        {
            get
            {
                return (Visibility)GetValue(ClassicControllerVisibilityProperty);
            }
            set
            {
                SetValue(ClassicControllerVisibilityProperty, value);
            }
        }
        // Using a DependencyProperty as the backing store for WiiVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClassicControllerVisibilityProperty = DependencyProperty.Register("ClassicControllerVisibility", typeof(Visibility), typeof(WiiSat), new UIPropertyMetadata(Visibility.Visible));


        [Bindable(true)]
        [Category("WiiState")]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Visibility NunchukShadowVisibility
        {
            get
            {
                return (Visibility)GetValue(NunchukShadowVisibilityProperty);
            }
            set
            {

                SetValue(NunchukShadowVisibilityProperty, value);
                
            }
        }
        // Using a DependencyProperty as the backing store for WiiVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NunchukShadowVisibilityProperty = DependencyProperty.Register("NunchukShadowVisibility", typeof(Visibility), typeof(WiiSat), new UIPropertyMetadata(Visibility.Visible));

	}
}