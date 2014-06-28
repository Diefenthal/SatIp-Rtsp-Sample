/*  
    Copyright (C) <2007-2014>  <Kay Diefenthal>

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
using System.ComponentModel;
using UPNPLib;


namespace SatIp.RtspSample.Rtsp
{
    public class RtspDevice : INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private String _rtspServerAddress;
        private String _rtspServerPort;       
        private String _rtspDeviceName;
        private RtspSession _rtspSession;

        #endregion

        #region Constructor

        public RtspDevice(string devicename, string address, string port)
        {
            _rtspDeviceName = devicename;
            _rtspServerAddress = address;
            _rtspServerPort = port;            
            _rtspSession = new RtspSession(this);
        }

        public RtspDevice(UPnPDevice device)
        {
            var baseHost = new Uri(((IUPnPDeviceDocumentAccess)device).GetDocumentURL());
            _rtspDeviceName = device.FriendlyName;
            _rtspServerAddress = baseHost.Host;
            _rtspSession = new RtspSession(this);
        }

        #endregion

        #region Properties

        public String RtspServerAddress
        {
            get { return _rtspServerAddress; }
            set { if (_rtspServerAddress != value){_rtspServerAddress = value;OnPropertyChanged("RtspServerAddress");}}
        }

        public String RtspServerPort
        {
            get { if (string.IsNullOrEmpty(_rtspServerPort)){_rtspServerPort = "554";}return _rtspServerPort;}
            set { if (_rtspServerPort != value){_rtspServerPort = value;OnPropertyChanged("RtspServerPort");}}
        }
        
        public String RtspDeviceName
        {
            get { return _rtspDeviceName; }
            set { if (_rtspDeviceName != value) { _rtspDeviceName = value; OnPropertyChanged("RtspDeviceName"); } }
        }

        public RtspSession RtspSession
        {
            get { return _rtspSession; }
            set{if (_rtspSession != value){_rtspSession = value; OnPropertyChanged("RtspSession"); } }
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Protected Methods

        protected void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        public void Dispose()
        {
            if (_rtspSession != null)
            {
                _rtspSession.Dispose();
            }
        }
    }
}
