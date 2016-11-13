/*  
    Copyright (C) <2007-2016>  <Kay Diefenthal>

    SatIp.RtspSample is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    SatIp.RtspSample is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with SatIp.RtspSample.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SatIp.RtspSample.Logging;
using SatIp.RtspSample.Properties;
using SatIp.RtspSample.Rtsp;
using SatIp.RtspSample.Upnp;
using UPNPLib;
using SatIp.RtspSample.Ssdp;
using System.Reflection;

namespace SatIp.RtspSample
{
    public partial class Form1 : Form
    {
        private readonly SSDPClient _client = new SSDPClient();
        private RtspDevice _rtspDevice;
        private Boolean _isstreaming;       

        public Form1()
        {
            InitializeComponent();
            Logger.SetLogFilePath("Sample.log", Settings.Default.LogLevel);
            
            var source = Utils.GetStationsFromLocalFile_m3u(Application.StartupPath + @"\PlayList.m3u");
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    PlayList.DataSource = source;
                }
            }
            tspgrLevel.Maximum = tspgrLevel.Maximum;
            tspgrQuality.Maximum = tspgrQuality.Maximum;
        }

        
        private void DeviceFound(object sender, SatIpDeviceFoundArgs args)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    DeviceFound(sender, args);
                });
                return;
            }
            var newnode = Devices.Nodes[0].Nodes.Add(args.Device.UniqueDeviceName, args.Device.FriendlyName);
            newnode.ToolTipText = args.Device.DeviceDescription;
            if (Devices.Nodes[0].IsExpanded != true)
                Devices.Nodes[0].Expand();
            if (Devices.Nodes.Count > 0)
            {
                var tn = Devices.Nodes[0].LastNode;
                Devices.SelectedNode = tn;
                Devices.Select();                
            }


        }
        private void DeviceLost(object sender, SatIpDeviceLostArgs args)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    DeviceLost(sender, args);
                });
                return;
            }
            Logger.Info("Device with UUID :{0} restarts,and will removed from the Devices Tree", args.Uuid);
            if (Devices.Nodes[0].Nodes.ContainsKey(args.Uuid))
            {
                var tn = Devices.Nodes[0].Nodes[args.Uuid];
                Devices.Nodes[0].Nodes.Remove(tn);
                Devices.Update();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Logger.Info("Search Started");
            _client.DeviceFound += DeviceFound;
            _client.DeviceLost += DeviceLost;            
            _client.FindByType("urn:ses-com:device:SatIPServer:1");
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                _rtspDevice.Dispose();
                _client.DeviceFound -= DeviceFound;
                _client.DeviceLost -= DeviceLost;
                _client.Dispose();
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("{0}-{1}:{2}", "Form1", "Form1_FormClosed", exception));
            }
        }
        private void PlayList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var service = (M3uService)PlayList.SelectedItem;
                Logger.Info("Selected Service is {0}", service.Name);
                if (_rtspDevice != null) 
                {                    
                    _rtspDevice.RtspSession.Play(service.ToString());                    
                    axWindowsMediaPlayer1.URL = string.Format("rtp://{0}:{1}", _rtspDevice.RtspSession.Destination, _rtspDevice.RtspSession.RtpPort);
                    Text = string.Format("rtp://{0}:{1}", _rtspDevice.RtspSession.Destination,
                        _rtspDevice.RtspSession.RtpPort);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("{0}-{1}:{2}", "Form1", "PlayList_SelectedIndexChanged", exception));
            }             
        }   
        private void Devices_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var device = (SatIpDevice)_client.FindByUDN(e.Node.Name);
            if (device != null)            
            {
                Logger.Info("Selected Sat>Ip Server is {0}", device.FriendlyName);
                

                if ((_rtspDevice != null) && (!_rtspDevice.FriendlyName.Equals(device.FriendlyName)))
                {                    
                    _rtspDevice.Dispose();
                    _rtspDevice = new RtspDevice(device.FriendlyName,device.BaseUrl.Host,device.UniqueDeviceName);                    
                    _isstreaming = false;
                }
                else
                {                    
                    _rtspDevice = new RtspDevice(device.FriendlyName, device.BaseUrl.Host, device.UniqueDeviceName);                    
                }
                var service = (M3uService)PlayList.SelectedItem;
                Logger.Info("Selected Service is {0}", service.Name);
                if (!_isstreaming)
                {
                    _rtspDevice.RtspSession.Setup(service.ToString(), TransmissionMode.Unicast);
                    _rtspDevice.RtspSession.RecieptionInfoChanged += new RecieptionInfoChangedEventHandler(RtspSession_RecieptionInfoChanged);
                    _rtspDevice.RtspSession.Play(String.Empty);
                    _isstreaming = true;
                    axWindowsMediaPlayer1.URL = string.Format("rtp://{0}:{1}", _rtspDevice.RtspSession.Destination,
                        _rtspDevice.RtspSession.RtpPort);
                    Text = string.Format("rtp://{0}:{1}", _rtspDevice.RtspSession.Destination,
                        _rtspDevice.RtspSession.RtpPort);
                }
            }
        }
        
        private void RtspSession_RecieptionInfoChanged(object sender, RecieptionInfoArgs e)
        {
            SetControlPropertyThreadSafe(tspgrLevel.ProgressBar, "Value", e.Level);
            SetControlPropertyThreadSafe(tspgrQuality.ProgressBar, "Value", e.Quality);
        }

        delegate void SetControlPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);
        private static void SetControlPropertyThreadSafe(Control control, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate
                (SetControlPropertyThreadSafe),
                new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(
                    propertyName,
                    BindingFlags.SetProperty,
                    null,
                    control,
                    new object[] { propertyValue });
            }
        }
    }
}
