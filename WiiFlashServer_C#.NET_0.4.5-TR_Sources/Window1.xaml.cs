/*
 __       __)     ________)            
(, )  |  /  ,  , (, /     /)        /) 
   | /| /          /___, // _   _  (/  
   |/ |/  _(__(_) /     (/_(_(_/_)_/ )_
   /  |        (_/

Copyright (c) 2008 Joa Ebert and Thibault Imbert

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using WiimoteLib;
using System.Windows.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;
using System.Media;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Media.Effects;

namespace Server
{
    public partial class Window1
    {
        public delegate void MyDelegateType();
        private System.Drawing.Point mousePoint;
        private double destX;
        private double destY;
        private byte[] buffer;
        public AsyncCallback pfnWorkerCallBack;
        private Socket mySocket;
        private List<Socket> m_workerSocket = new List<Socket>();
        private NotifyIcon m_notifyicon;
        private System.Windows.Forms.ContextMenu m_menu;
        private bool mouseEnabled;
        private int userScreenW;
        private int userScreenH;
        private int idMouseControl;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.MenuItem wiimotesInfo;
        private System.Windows.Forms.MenuItem mouseControlMenu;
        private Storyboard closingSb;
        private String WFS_VERSION = "0.4.5";
        private String policyFile = "<?xml version='1.0'?><!DOCTYPE cross-domain-policy SYSTEM '/xml/dtds/cross-domain-policy.dtd'><!-- Policy file for xmlsocket://socks.example.com --><cross-domain-policy><site-control permitted-cross-domain-policies='all'/><allow-access-from domain='*' to-ports='19028' /></cross-domain-policy>";
        private const int maxWiimotes = 4;
        private const double friction = .2;
        int nbWiimotes;
        Storyboard sb;
        WiimoteCollection wC;
        private List<WiiSatBO> bos = new List<WiiSatBO>();
        private List<WiiSat> arraySats = new List<WiiSat>();

        public enum MouseEventType : int
        {
            LeftDown = 0x02,
            LeftUp = 0x04,
            RightDown = 0x08,
            RightUp = 0x10
        }

        private void initializeMinimizing()
        {
            m_menu = new ContextMenu();

            wiimotesInfo = new MenuItem("Wiimote(s) connected", new System.EventHandler(WiimotesInfo_Click));
            m_menu.MenuItems.Add(0, wiimotesInfo);
            mouseControlMenu = new MenuItem("Control Mouse", new System.EventHandler(ControlMouse_Click));
            m_menu.MenuItems.Add(1,mouseControlMenu);
            m_menu.MenuItems.Add(2, new MenuItem("Exit", new System.EventHandler(Exit_Click)));

            m_notifyicon = new NotifyIcon();
            m_notifyicon.Text = "WiiFlash Server " + WFS_VERSION;
            m_notifyicon.Visible = true;
            m_notifyicon.Icon = new Icon(GetType(), "wiiflash.ico");
            m_notifyicon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(Dble_Click);
            m_notifyicon.ContextMenu = m_menu;
        }

        private void onExit(object sender, System.Windows.RoutedEventArgs e)
        {  
            WindowState = System.Windows.WindowState.Minimized;
            ShowInTaskbar = false;
        }

        protected void ControlMouse_Click(Object sender, System.EventArgs e)
        {
            mouseEnabled = true;
        }

        protected void Dble_Click(Object sender, System.EventArgs e)
        {
            Activate();
            WindowState = System.Windows.WindowState.Normal;
            ShowInTaskbar = true;
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            destX = destY = 0;
            mousePoint = new System.Drawing.Point(0, 0);

            timer = new Timer();
            timer.Interval = 10;
            timer.Tick += new EventHandler(Timer_Tick);

            userScreenW = Screen.PrimaryScreen.Bounds.Width;
            userScreenH = Screen.PrimaryScreen.Bounds.Height;

            arraySats.Add(WiiSat1);
            arraySats.Add(WiiSat2);
            arraySats.Add(WiiSat3);
            arraySats.Add(WiiSat4);

            closingSb = FindResource("onWindowClosed") as Storyboard;
            closingSb.Completed += new EventHandler(onClosingAnim);

            Title = "WiFlash Server " + WFS_VERSION;

            hideSats();
            initializeMinimizing();

            wC = new WiimoteCollection();

            String infosWiimote;

            try
            {
                wC.FindAllWiimotes();

                initSocket();

                nbWiimotes = Math.Min(wC.Count, maxWiimotes);

                Debug.WriteLine("nombre de wiimotes " +  nbWiimotes);

                infosWiimote = wC.Count + " Wiimote" + ((wC.Count > 1) ? "(s)" : "");

                m_notifyicon.ShowBalloonTip(2, "Informations", "WiiFlash Server is running\n" + infosWiimote + " detected", ToolTipIcon.Info);

                sb = FindResource("onWindowLoadedTL") as Storyboard;
                sb.BeginTime = TimeSpan.Parse("0:0:3");
                sb.Begin(this);

                wiimotesInfo.Text = infosWiimote+" connected";

                WiiSatBO valueObj;
                System.Windows.Controls.ToolTip tt;
                Wiimote wm;

                for (int i = 0; i < nbWiimotes; i++)
                {
                    valueObj = new WiiSatBO();
                    valueObj.WiimoteVisibility = Visibility.Visible;
                    tt = new System.Windows.Controls.ToolTip();
                    tt.Content = "Wiimote " + ((int)i + 1) + " \nWiimote Rumble";

                    arraySats[i].id = i;
                    arraySats[i].ToolTip = tt;
                    arraySats[i].IsEnabled = true;
                    arraySats[i].Cursor = System.Windows.Input.Cursors.Hand;
                    arraySats[i].DataContext = valueObj;
                    arraySats[i].MouseDown += new MouseButtonEventHandler(onWiiSatClicked);

                    wm = wC[i];
                    wm.id = i; 
                    wm.WiimoteChanged += wm_OnWiimoteChanged;
                    wm.WiimoteExtensionChanged += wm_OnWiimoteExtensionChanged;
                    wm.Connect();
                    if (wm.WiimoteState.ExtensionType != ExtensionType.BalanceBoard)
                        wm.SetReportType(InputReport.IRAccel, true);
                    wm.SetLEDs(i+1);
                    wm.SetRumble(true);
                    timer.Start();
                }
            }
            catch (WiimoteNotFoundException)
            {
                wiimotesInfo.Text = "No Wiimote found !";
                infosWiimote = wC.Count + " Wiimote" + ((wC.Count > 1) ? "(s)" : "");
                m_notifyicon.ShowBalloonTip(2, "Informations", infosWiimote + " detected.\nMake sure the wiimote(s) are correctly connected", ToolTipIcon.Info);
                mouseControlMenu.Enabled = false;
                sb = FindResource("onWindowError") as Storyboard;
                sb.BeginTime = TimeSpan.Parse("0:0:3");
                sb.Begin(this);
            }
            catch (WiimoteException)
            {
                wiimotesInfo.Text = "Connection error !";
                infosWiimote = wC.Count + " Wiimote" + ((wC.Count > 1) ? "(s)" : "");
                m_notifyicon.ShowBalloonTip(2, "Informations", infosWiimote + " detected.\nWiimote(s) is detected but connection could not be established.\nPlease try restarting your bluetooth connection", ToolTipIcon.Info);
                mouseControlMenu.Enabled = false;
                sb = FindResource("onWindowError") as Storyboard;
                sb.BeginTime = TimeSpan.Parse("0:0:3");
                sb.Begin(this);
            }
            catch (Exception)
            {
                wiimotesInfo.Text = "Connection error !";
                infosWiimote = wC.Count + " Wiimote" + ((wC.Count > 1) ? "(s)" : "");
                m_notifyicon.ShowBalloonTip(2, "Informations", infosWiimote + " Wiimote(s) detected.\nAn unknown problem has occured\nMake sure the wiimote(s) are correctly connected", ToolTipIcon.Info);
                mouseControlMenu.Enabled = false;
                sb = FindResource("onWindowError") as Storyboard;
                sb.BeginTime = TimeSpan.Parse("0:0:3");
                sb.Begin(this);
            }
        }

        private void onWiiSatClicked(object sender, System.EventArgs evt)
        {
            wC[(sender as WiiSat).id].SetRumble(true);
            timer.Interval = 10;
            timer.Start();
        }

        private void hideSats()
        {
            int lng = arraySats.Count;
            WiiSatBO valueObj;

            for (int i = 0; i < lng; i++)
            { 
                valueObj = new WiiSatBO();
                valueObj.WiimoteVisibility = Visibility.Hidden;
                valueObj.NunchuckVisibility = Visibility.Hidden;
                valueObj.ClassicControllerVisibility = Visibility.Hidden;
                bos.Add(valueObj);
                arraySats[i].DataContext = valueObj;
                System.Windows.Controls.ToolTip tt = new System.Windows.Controls.ToolTip();
                tt.Content = "No Wiimote attached";
                arraySats[i].ToolTip = tt;
            }
        }

        public void Timer_Tick(object sender, EventArgs eArgs)
        {
            (sender as System.Windows.Forms.Timer).Stop();
            for (int i = 0; i < nbWiimotes; i++) 
                wC[i].SetRumble(false);
        }

        protected void disconnectWiimotes ()
        {
            Wiimote wm;
            for ( int i = 0; i< nbWiimotes; i++ ) 
            {
                 wm = wC[i];
                 wm.SetLEDs(false, false, false, false);
                 wm.Disconnect();
            }
        }

        protected void onClosingAnim (Object sender, System.EventArgs e)
        {
            m_notifyicon.Dispose();
            Close();
            disconnectWiimotes();
        }

        protected void Exit_Click(Object sender, System.EventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Minimized) 
                closingSb.Begin(this);

            else
            {
                m_notifyicon.Dispose();
                disconnectWiimotes();
                Close();
            }
        }

        protected void WiimotesInfo_Click(Object sender, System.EventArgs e){}

        private void Window_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        void onIntroComplete(object sender, System.EventArgs e)
        {
            ShowInTaskbar = false;
        }

        private void onWindowClosed(object sender, System.EventArgs e)
        {
            m_notifyicon.Dispose();
        }

        private void initSocket()
        {
            int version = System.Environment.OSVersion.Version.Major;

            if ( version >= 6 )
            {
                // start the socket server using IPV6 (Windows Vista or Windows 7)
                mySocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                mySocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, 0);    
            } else
            {
                // start the socket server normally
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            IPAddress address = Dns.GetHostEntry("localhost").AddressList[0];

            // we get an auto-generated IP
            IPEndPoint ipLocal = new IPEndPoint(address, 0x4a54);

            // we bind the ip address
            mySocket.Bind(ipLocal);

            // we start listening
            mySocket.Listen(1);

            // create a callback, for incoming clients
            mySocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
        }

        String GetIP()
        {
            String strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

            // Grab the first IP addresses
            String IPStr = "";

            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                IPStr = ipaddress.ToString();
                return IPStr;
            }

            return IPStr;
        }

        private void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                Socket currentSocket = mySocket.EndAccept(asyn);
                // Here we complete/end the BeginAccept() asynchronous call
                // by calling EndAccept() - which returns the reference to
                // a new Socket object
                m_workerSocket.Add (currentSocket);
                // Let the worker Socket do the further processing for the 
                // just connected client
                WaitForData(currentSocket);
                // Since the main Socket is now free, it can go back and wait for
                // other clients who are attempting to connect
                mySocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }

            catch (ObjectDisposedException)
            {
                Console.WriteLine("Socket has been closed");
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }

        }

        // Start waiting for data from the client
        public void WaitForData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }

                SocketPacket theSocPkt = new SocketPacket();

                theSocPkt.m_currentSocket = soc;

                // Start receiving any data written by the connected client
                // asynchronously
                try
                {

                    soc.BeginReceive(theSocPkt.dataBuffer, 0,
                                   theSocPkt.dataBuffer.Length,
                                   SocketFlags.None,
                                   pfnWorkerCallBack,
                                   theSocPkt);

                }
                catch (Exception se)
                {
                    Console.WriteLine(se.Message);
                };

            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            };
        }

        public class SocketPacket
        {
            public System.Net.Sockets.Socket m_currentSocket;
            public byte[] dataBuffer = new byte[4];
        }

        // This the call back function which will be invoked when the socket
        // detects any client writing of data on the stream
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket socketData = (SocketPacket)asyn.AsyncState;

                // if flash player request a policy file
                // we send the xml policy
                if ( socketData.dataBuffer[0] == 0x3C )
                {
                     byte[] policyFileBytes = new UTF8Encoding().GetBytes(policyFile);

                     // send the police file
                     sendBytes (policyFileBytes);

                } else
                {
                    int wiimoteID = socketData.dataBuffer[0];
                    int type = socketData.dataBuffer[1];
                    int parameter = socketData.dataBuffer[2];

                    switch (type)
                    {
                        case 0x72:
                            wC[wiimoteID].SetRumble(parameter == 0x31);
                            break;
                        case 0x76:
                            mouseEnabled = (parameter == 0x31);
                            idMouseControl = wiimoteID;
                            break;
                        case 0x6C:
                            wC[wiimoteID].SetLEDs((parameter & 1) != 0, (parameter & 2) != 0, (parameter & 4) != 0, (parameter & 8) != 0);
                            break;

                    }
                    // Continue the waiting for data on the Socket
                    WaitForData(socketData.m_currentSocket);
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Problem receiving data");
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        private void viewbox_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void sendBytes(Byte[] data)
        {
            int lng = m_workerSocket.Count;

            for ( int i = 0; i< lng; i++ )
            {
                if ( m_workerSocket[i].Connected )
                {
                    try
                    {
                        m_workerSocket[i].Send(data);
                    } catch
                    {
                    }
                }
            }
        }

        void wm_OnWiimoteExtensionChanged(object sender, WiimoteExtensionChangedEventArgs args)
        {
            // we can't access controls in the main thread directly
            // we create a delegate, to make an asynchronous call
            MyDelegateType methodForNunchuckExtension = delegate
            {
                // update the Nunchuck visibility state, when the nunchuck is plugged or unplugged
                int id = (sender as Wiimote).id;
                WiiSatBO valueObj = new WiiSatBO();
                valueObj.WiimoteVisibility = Visibility.Visible;
                OuterGlowBitmapEffect myDropShadowEffect = null;

                if (args.Inserted)
                {
                    myDropShadowEffect = new OuterGlowBitmapEffect();
                    myDropShadowEffect.GlowColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#003D64");
                    valueObj.NunchuckVisibility = (args.ExtensionType == ExtensionType.Nunchuk) ? Visibility.Visible : Visibility.Hidden;
                    valueObj.ClassicControllerVisibility = (args.ExtensionType == ExtensionType.ClassicController) ? Visibility.Visible : Visibility.Hidden;
                    wC[id].SetReportType(InputReport.IRExtensionAccel, true);

                } else wC[id].SetReportType(InputReport.IRAccel, true);

                arraySats[id].BitmapEffect = myDropShadowEffect;
                arraySats[id].DataContext = valueObj;
            };
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, methodForNunchuckExtension);
        }

        private static byte[] SingleToBytes(Single f)
        {
            float[] x = new float[1];
            x[0] = f;
            byte[] dest = new byte[4];
            System.Buffer.BlockCopy(x, 0, dest, 0, 4);
            Array.Reverse(dest);
            return dest;
        }

        void wm_OnWiimoteChanged(object sender, WiimoteChangedEventArgs args)
        {
            WiimoteState ws = args.WiimoteState;
            
            MyDelegateType methodForBytes = delegate
            {
                buffer = new byte[80];

                if (mouseEnabled)
                {
                    WiimoteState wsMouse = wC[idMouseControl].WiimoteState;

                    int mouseX = Convert.ToInt32((1 - wsMouse.IRState.IRSensors[0].Position.X) * (userScreenW));
                    int mouseY = Convert.ToInt32(wsMouse.IRState.IRSensors[0].Position.Y * userScreenH);

                    destX -= (destX - mouseX)*friction;
                    destY -= (destY - mouseY)*friction;

                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)destX, (int)destY);

                    mouseEnabled = !wsMouse.ButtonState.Home;
                }

                MemoryStream ms = new MemoryStream(buffer);

                byte buttonStateHI = 0;
                byte buttonStateLO = 0;

                if (ws.ButtonState.One)
                    buttonStateHI |= 1 << 7;
                if (ws.ButtonState.Two)
                    buttonStateHI |= 1 << 6;
                if (ws.ButtonState.A)
                    buttonStateHI |= 1 << 5;
                if (ws.ButtonState.B)
                    buttonStateHI |= 1 << 4;
                if (ws.ButtonState.Plus)
                    buttonStateHI |= 1 << 3;
                if (ws.ButtonState.Minus)
                    buttonStateHI |= 1 << 2;
                if (ws.ButtonState.Home)
                    buttonStateHI |= 1 << 1;
                if (ws.ButtonState.Up)
                    buttonStateHI |= 1;

                if (ws.ButtonState.Down)
                    buttonStateLO |= 1 << 7;
                if (ws.ButtonState.Right)
                    buttonStateLO |= 1 << 6;
                if (ws.ButtonState.Left)
                    buttonStateLO |= 1 << 5;

                // wiimote id
                ms.WriteByte(Convert.ToByte((sender as Wiimote).id));

                // battery level
                ms.WriteByte(ws.Battery);

                ms.WriteByte(buttonStateHI);
                ms.WriteByte(buttonStateLO);

                byte[] xValue = SingleToBytes(ws.AccelState.Values.X);
                byte[] yValue = SingleToBytes(ws.AccelState.Values.Y);
                byte[] zValue = SingleToBytes(ws.AccelState.Values.Z);

                ms.Write(xValue, 0, xValue.Length);
                ms.Write(yValue, 0, yValue.Length);
                ms.Write(zValue, 0, zValue.Length);

                if (ws.ExtensionType == ExtensionType.Nunchuk)
                {
                    // hasNunchuck byte
                    ms.WriteByte(1);

                    byte chuckState = 0;

                    if (ws.NunchukState.C) 
                        chuckState |= 1 << 1;
                    if (ws.NunchukState.Z) 
                        chuckState |= 1 << 0;

                    ms.WriteByte(chuckState);

                    byte[] stickX = SingleToBytes(ws.NunchukState.Joystick.X);
                    byte[] stickY = SingleToBytes(ws.NunchukState.Joystick.Y);

                    byte[] xChuckValue = SingleToBytes(ws.NunchukState.AccelState.Values.X);
                    byte[] yChuckValue = SingleToBytes(ws.NunchukState.AccelState.Values.Y);
                    byte[] zChuckValue = SingleToBytes(ws.NunchukState.AccelState.Values.Z);

                    ms.Write(stickX, 0, stickX.Length);
                    ms.Write(stickY, 0, stickY.Length);

                    ms.Write(xChuckValue, 0, xChuckValue.Length);
                    ms.Write(yChuckValue, 0, yChuckValue.Length);
                    ms.Write(zChuckValue, 0, zChuckValue.Length);

                }
                else if (ws.ExtensionType == ExtensionType.ClassicController)
                {
                    // hasClassicController byte
                    ms.WriteByte(2);

                    WiimoteLib.ClassicControllerButtonState bs = ws.ClassicControllerState.ButtonState;

                    byte buttonStateStandard = 0;
                    byte buttonStatePad = 0;

                    if (bs.X) 
                        buttonStateStandard |= 1 << 7;
                    if (bs.Y) 
                        buttonStateStandard |= 1 << 6;
                    if (bs.A) 
                        buttonStateStandard |= 1 << 5;
                    if (bs.B) 
                        buttonStateStandard |= 1 << 4;
                    if (bs.Plus) 
                        buttonStateStandard |= 1 << 3;
                    if (bs.Minus) 
                        buttonStateStandard |= 1 << 2;
                    if (bs.Home) 
                        buttonStateStandard |= 1 << 1;
                    if (bs.Up) 
                        buttonStateStandard |= 1;

                    if (bs.Down) 
                        buttonStatePad |= 1 << 7;
                    if (bs.Right) 
                        buttonStatePad |= 1 << 6;
                    if (bs.Left) 
                        buttonStatePad |= 1 << 5;
                    if (bs.TriggerL) 
                        buttonStatePad |= 1 << 4;
                    if (bs.TriggerR) 
                        buttonStatePad |= 1 << 3;
                    if (bs.ZL) 
                        buttonStatePad |= 1 << 2;
                    if (bs.ZR) 
                        buttonStatePad |= 1 << 1;

                    ms.WriteByte(buttonStateStandard);
                    ms.WriteByte(buttonStatePad);

                    byte[] xlStickValue = SingleToBytes(ws.ClassicControllerState.JoystickL.X);
                    byte[] ylStickValue = SingleToBytes(ws.ClassicControllerState.JoystickL.Y);

                    byte[] xrStickValue = SingleToBytes(ws.ClassicControllerState.JoystickR.X);
                    byte[] yrStickValue = SingleToBytes(ws.ClassicControllerState.JoystickR.Y);

                    ms.Write(xlStickValue, 0, xlStickValue.Length);
                    ms.Write(ylStickValue, 0, ylStickValue.Length);
                    ms.Write(xrStickValue, 0, xrStickValue.Length);
                    ms.Write(yrStickValue, 0, yrStickValue.Length);

                }
                else if (ws.ExtensionType == ExtensionType.BalanceBoard)
                {
                    // balance board byte
                    ms.WriteByte(3);

                    // balance board support
                    BalanceBoardState bbs = ws.BalanceBoardState;

                    byte[] blKg = SingleToBytes(bbs.SensorValuesKg.BottomLeft);
                    byte[] brKg = SingleToBytes(bbs.SensorValuesKg.BottomRight);

                    byte[] tlKg = SingleToBytes(bbs.SensorValuesKg.TopLeft);
                    byte[] trKg = SingleToBytes(bbs.SensorValuesKg.TopRight);

                    byte[] ttlKg = SingleToBytes(bbs.WeightKg);

                    ms.Write(blKg, 0, blKg.Length);
                    ms.Write(brKg, 0, brKg.Length);
                    ms.Write(tlKg, 0, tlKg.Length);
                    ms.Write(trKg, 0, trKg.Length);
                    ms.Write(ttlKg, 0, ttlKg.Length);
                } 
                else ms.Position += 22;

                if (ws.IRState.IRSensors[0].Found)
                {
                    // IR stuff
                    ms.WriteByte(1);

                    byte[] x1IRValue = SingleToBytes(ws.IRState.IRSensors[0].Position.X);
                    byte[] y1IRValue = SingleToBytes(ws.IRState.IRSensors[0].Position.Y);

                    ms.Write(x1IRValue, 0, x1IRValue.Length);
                    ms.Write(y1IRValue, 0, y1IRValue.Length);

                }
                else ms.Position += 9;

                if (ws.IRState.IRSensors[1].Found)
                {
                    // IR stuff
                    ms.WriteByte(1);

                    byte[] x2IRValue = SingleToBytes(ws.IRState.IRSensors[1].Position.X);
                    byte[] y2IRValue = SingleToBytes(ws.IRState.IRSensors[1].Position.Y);

                    ms.Write(x2IRValue, 0, x2IRValue.Length);
                    ms.Write(y2IRValue, 0, y2IRValue.Length);

                }
                else ms.Position += 9;

                if (ws.IRState.IRSensors[2].Found)
                {
                    // IR stuff
                    ms.WriteByte(1);

                    byte[] x3IRValue = SingleToBytes(ws.IRState.IRSensors[2].Position.X);
                    byte[] y3IRValue = SingleToBytes(ws.IRState.IRSensors[2].Position.Y);

                    ms.Write(x3IRValue, 0, x3IRValue.Length);
                    ms.Write(y3IRValue, 0, y3IRValue.Length);

                }
                else ms.Position += 9;

                if (ws.IRState.IRSensors[3].Found)
                {
                    // IR stuff
                    ms.WriteByte(1);

                    byte[] x4IRValue = SingleToBytes(ws.IRState.IRSensors[3].Position.X);
                    byte[] y4IRValue = SingleToBytes(ws.IRState.IRSensors[3].Position.Y);

                    ms.Write(x4IRValue, 0, x4IRValue.Length);
                    ms.Write(y4IRValue, 0, y4IRValue.Length);

                }
                else ms.Position += 9;

                // dot size
                byte size1 = Convert.ToByte(ws.IRState.IRSensors[0].Size);
                byte size2 = Convert.ToByte(ws.IRState.IRSensors[1].Size);
                byte size3 = Convert.ToByte(ws.IRState.IRSensors[2].Size);
                byte size4 = Convert.ToByte(ws.IRState.IRSensors[3].Size);

                ms.WriteByte(size1);
                ms.WriteByte(size2);
                ms.WriteByte(size3);
                ms.WriteByte(size4);

                byte JTAG = Convert.ToByte(0x4a);
                byte TTAG = Convert.ToByte(0x54);

                // Joa and Thibault end tag bytes ;)
                ms.WriteByte(JTAG);
                ms.WriteByte(TTAG);

                // send bytes to flash
                sendBytes(buffer);
            };

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, methodForBytes);
        }
    }
}