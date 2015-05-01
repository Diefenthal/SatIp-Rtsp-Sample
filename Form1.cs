/*  
    Copyright (C) <2007-2015>  <Kay Diefenthal>

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

namespace SatIp.RtspSample
{
    public partial class Form1 : Form
    {
        private DeviceCollector _deviceCollector;
        private RtspDevice _rtspDevice;
        private Boolean _isstreaming;
        private Timer _keepaLiveTimer;

        public Form1()
        {
            InitializeComponent();
            Logger.SetLogFilePath("Sample.log", Settings.Default.LogLevel);
            var source = GetStationsFromLocalFile_m3u(Application.StartupPath + @"\PlayList.m3u");
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

        void _keepaLiveTimer_Tick(object sender, EventArgs e)
        {
            try 
            { 
                _rtspDevice.RtspSession.Options();
                int level;
                int quality;
                _rtspDevice.RtspSession.Describe(out level, out quality);
                tspgrLevel.Value = level;
                tspgrQuality.Value = quality;
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("{0}-{1}:{2}", "Form1", "_keepaLiveTimer_Tick", exception));
            }           
            
        }
        public void DeviceAdded(UPnPDevice device)
        {
            var newnode = treeView1.Nodes[0].Nodes.Add(device.UniqueDeviceName, device.FriendlyName);
            newnode.ToolTipText = device.Description;
            if (device.HasChildren)
            {
                var childdevices = device.Children;
                foreach (UPnPDevice child in childdevices)
                {
                    newnode.Nodes.Add(child.UniqueDeviceName, child.FriendlyName).ToolTipText = child.Description;
                }
            }
            if (treeView1.Nodes[0].IsExpanded != true)
                treeView1.Nodes[0].Expand();
        }

        private void SearchCompleted()
        {
            var tn = treeView1.Nodes[0].LastNode;
            treeView1.SelectedNode = tn;
            treeView1.Select();
            _keepaLiveTimer.Enabled = true;
        }

        public void DeviceRemoved(string sUdn)
        {
            treeView1.Nodes[sUdn].Remove();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            _deviceCollector = new DeviceCollector();
            _deviceCollector.DeviceAdded += DeviceAdded;
            _deviceCollector.SearchCompleted += SearchCompleted;
        }

        private void PlayList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var service = (Service)PlayList.SelectedItem;

                Logger.Info(string.Format("{0}{1}{2}", "Channel ",service.Name , " is selected"));
                if (_rtspDevice != null) 
                {                    
                    _rtspDevice.RtspSession.Play(service.ToString());                   
                    axWindowsMediaPlayer1.URL = string.Format("rtp://{0}:{1}", _rtspDevice.RtspSession.Destination, _rtspDevice.RtspSession.ClientRtpPort);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("{0}-{1}:{2}", "Form1", "PlayList_SelectedIndexChanged", exception));
            }             
        }   

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try 
            { 
                _rtspDevice.Dispose();
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("{0}-{1}:{2}", "Form1", "Form1_FormClosed", exception));
            }            
        }

        public static List<Service> GetStationsFromLocalFile_m3u(string fileName)
        {
            using (StreamReader reader = File.OpenText(fileName))
            {
                string[] strArray = reader.ReadToEnd().Split(new[] { "\n", "\r", "\n\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var list = new List<Service>();
                if (strArray[0].Trim().ToUpper() == "#EXTM3U")
                {
                    var name = string.Empty;
                    var parameters = new string[15];
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        if (strArray[i].StartsWith("#EXTINF", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var strArray2 = strArray[i].Split(new[] { ":", "," }, StringSplitOptions.None);
                            if (strArray2.Length > 2)
                            {
                                name = strArray2[2];
                                parameters = strArray[++i].Split('&');
                            }
                            list.Add(new Service(name, parameters));
                        }
                        else if (strArray[i].StartsWith("# ", StringComparison.CurrentCultureIgnoreCase))
                        {
                            name = strArray[i].Substring(2);
                        }
                    }
                }
                return list;
            }
        }      

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var finder = new UPnPDeviceFinderClass();
            var device = finder.FindByUDN(e.Node.Name);
            if (device != null)
            {
                _keepaLiveTimer = new Timer { Enabled = true };
                _keepaLiveTimer.Tick += _keepaLiveTimer_Tick;

                if ((_rtspDevice != null) && (!_rtspDevice.RtspDeviceName.Equals(device.FriendlyName)))
                {
                    _keepaLiveTimer.Stop();
                    _rtspDevice.Dispose();
                    _rtspDevice = new RtspDevice(device);
                    _keepaLiveTimer.Interval = _rtspDevice.RtspSession.RtspSessionTimeToLive;
                    _keepaLiveTimer.Start();
                    _isstreaming = false;
                }
                else
                {
                    _keepaLiveTimer.Stop();
                    _rtspDevice = new RtspDevice(device);
                    _keepaLiveTimer.Interval = _rtspDevice.RtspSession.RtspSessionTimeToLive;
                    _keepaLiveTimer.Start();
                }
                var service = (Service)PlayList.SelectedItem;

                if (!_isstreaming)
                {
                    _rtspDevice.RtspSession.Setup(service.ToString(), "unicast");
                    _rtspDevice.RtspSession.Play(String.Empty);
                    _isstreaming = true;
                    axWindowsMediaPlayer1.URL = string.Format("rtp://{0}:{1}", _rtspDevice.RtspSession.Destination,
                        _rtspDevice.RtspSession.ClientRtpPort);
                }

            }
        }
    }
}
