﻿#pragma checksum "..\..\WiiSat.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D7549BB22B0616C30F5CB29E4D9DB4E3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Server {
    
    
    /// <summary>
    /// WiiSat
    /// </summary>
    public partial class WiiSat : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\WiiSat.xaml"
        internal Server.WiiSat UserControl;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\WiiSat.xaml"
        internal System.Windows.Shapes.Ellipse fondBleu1;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\WiiSat.xaml"
        internal System.Windows.Shapes.Ellipse fondBleu2;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\WiiSat.xaml"
        internal System.Windows.Shapes.Rectangle wiimoteIcoOmbre;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\WiiSat.xaml"
        internal System.Windows.Shapes.Rectangle nunchuckIcoOmbre;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\WiiSat.xaml"
        internal System.Windows.Shapes.Rectangle wiimoteIco;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\WiiSat.xaml"
        internal System.Windows.Shapes.Rectangle nunchuckIco;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\WiiSat.xaml"
        internal System.Windows.Shapes.Rectangle ClassicControllerIco;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/WiiFlashServer;component/wiisat.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\WiiSat.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.UserControl = ((Server.WiiSat)(target));
            return;
            case 2:
            this.fondBleu1 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 3:
            this.fondBleu2 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 4:
            this.wiimoteIcoOmbre = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 5:
            this.nunchuckIcoOmbre = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 6:
            this.wiimoteIco = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 7:
            this.nunchuckIco = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 8:
            this.ClassicControllerIco = ((System.Windows.Shapes.Rectangle)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
