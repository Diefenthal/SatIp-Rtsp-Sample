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
using System.ComponentModel;
using UPNPLib;


namespace SatIp.RtspSample.Rtsp
{
    public class RtspDevice : INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private String _serverAddress;
        private String _uniqueDeviceName;      
        private String _friendlyName;
        private RtspSession _rtspSession;
        private Boolean _disposed =false;
        #endregion

        #region Constructor

        public RtspDevice(string friendlyName, string address, string uniqueDeviceName)
        {
            _friendlyName = friendlyName;
            _serverAddress = address;
            _uniqueDeviceName = uniqueDeviceName;         
            _rtspSession = new RtspSession(this);
        }

        public RtspDevice(UPnPDevice device)
        {
            var baseHost = new Uri(((IUPnPDeviceDocumentAccess)device).GetDocumentURL());
            _friendlyName = device.FriendlyName;
            _uniqueDeviceName = device.UniqueDeviceName;
            _serverAddress = baseHost.Host;
            _rtspSession = new RtspSession(this);
        }
        ~RtspDevice()
        {
            Dispose(false);
        }   
        #endregion

        #region Properties

        public String ServerAddress
        {
            get { return _serverAddress; }
            set { if (_serverAddress != value){_serverAddress = value;OnPropertyChanged("ServerAddress");}}
        }

        public String UniqueDeviceName
        {
            get { return _uniqueDeviceName; }
            set { if (_uniqueDeviceName != value) { _uniqueDeviceName = value; OnPropertyChanged("UniqueDeviceName"); } }
        }
        
        public String FriendlyName
        {
            get { return _friendlyName; }
            set { if (_friendlyName != value) { _friendlyName = value; OnPropertyChanged("FriendlyName"); } }
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
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_rtspSession != null)
                    {
                        _rtspSession.Dispose();
                    }
                }
            }
            _disposed = true;
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
