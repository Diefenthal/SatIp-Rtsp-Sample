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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using SatIp.RtspSample.Logging;


namespace SatIp.RtspSample.Rtsp
{
    public class RtspSession : INotifyPropertyChanged ,IDisposable
    {
        #region Private Fields
        private static readonly Regex RegexRtspSessionHeader = new Regex(@"\s*([^\s;]+)(;timeout=(\d+))?");
        private const int DefaultRtspSessionTimeout = 60;    // unit = s
        private static readonly Regex RegexDescribeResponseSignalInfo = new Regex(@";tuner=\d+,(\d+),(\d+),(\d+),", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private RtspDevice _rtspDevice;
        private Socket _rtspSocket;
        private int _rtspSequenceNum;        
        private string _rtspSessionId;
        private int _rtspSessionTimeToLive;
        private string _rtspStreamId;
        private int _clientRtpPort;
        private int _clientRtcpPort;
        private int _serverRtpPort;
        private int _serverRtcpPort;
        private int _rtpPort;
        private int _rtcpPort;        
        private string _rtspStreamUrl;
        private string _destination;
        private string _source;
        private string _transport;
        private int _signalLevel;        
        private int _signalQuality;       

        #endregion

        #region Constructor

        public RtspSession(RtspDevice rtspDevice)
        {
            _rtspDevice = rtspDevice;
            
        }

        #endregion

        #region Properties

        #region Rtsp

        public string RtspStreamId
        {
            get { return _rtspStreamId; }
            set { if (_rtspStreamId != value) { _rtspStreamId = value; OnPropertyChanged("RtspStreamId"); } }
        }
        public string RtspStreamUrl
        {
            get { return _rtspStreamUrl; }
            set { if (_rtspStreamUrl != value) { _rtspStreamUrl = value; OnPropertyChanged("RtspStreamUrl"); } }
        }
        public RtspDevice RtspDevice
        {
            get { return _rtspDevice; }
            set { if (_rtspDevice != value) { _rtspDevice = value; OnPropertyChanged("RtspDevice"); } }
        }
        public int RtspSessionTimeToLive
        {
            get { return _rtspSessionTimeToLive; }
            set { if (_rtspSessionTimeToLive != value) { _rtspSessionTimeToLive = value; OnPropertyChanged("RtspSessionTimeToLive"); } }
        }

        #endregion

        #region Rtp Rtcp

        /// <summary>
        /// The LocalEndPoint Address
        /// </summary>
        public string Destination
        {
            get
            {
                if (string.IsNullOrEmpty(_destination))
                {
                    var result = "";
                    var host = Dns.GetHostName();
                    var hostentry = Dns.GetHostEntry(host);
                    foreach (var ip in hostentry.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
                    {
                        result = ip.ToString();
                    }

                    _destination = result;
                }
                return _destination;
            }
            set
            {
                if (_destination != value)
                {
                    _destination = value;
                    OnPropertyChanged("Destination");
                }
            }
        }

        /// <summary>
        /// The RemoteEndPoint Address
        /// </summary>
        public string Source
        {
            get { return _source; }
            set
            {
                if (_source != value)
                {
                    _source = value;
                    OnPropertyChanged("Source");
                }
            }
        }

        /// <summary>
        /// The Media Data Delivery RemoteEndPoint Port if we use Unicast
        /// </summary>
        public int ServerRtpPort
        {
            get
            {
                return _serverRtpPort;
            }
            set { if (_serverRtpPort != value) { _serverRtpPort = value; OnPropertyChanged("ServerRtpPort"); } }
        }

        /// <summary>
        /// The Media Metadata Delivery RemoteEndPoint Port if we use Unicast
        /// </summary>
        public int ServerRtcpPort
        {
            get { return _serverRtcpPort; }
            set { if (_serverRtcpPort != value) { _serverRtcpPort = value; OnPropertyChanged("ServerRtcpPort"); } }
        }

        /// <summary>
        /// The Media Data Delivery LocalEndPoint Port if we use Unicast
        /// </summary>
        public int ClientRtpPort
        {
            get { return _clientRtpPort; }
            set { if (_clientRtpPort != value) { _clientRtpPort = value; OnPropertyChanged("ClientRtpPort"); } }
        }

        /// <summary>
        /// The Media Metadata Delivery LocalEndPoint Port if we use Unicast
        /// </summary>
        public int ClientRtcpPort
        {
            get { return _clientRtcpPort; }
            set { if (_clientRtcpPort != value) { _clientRtcpPort = value; OnPropertyChanged("ClientRtcpPort"); } }
        }

        /// <summary>
        /// The Media Data Delivery RemoteEndPoint Port if we use Multicast 
        /// </summary>
        public int RtpPort
        {
            get { return _rtpPort; }
            set { if (_rtpPort != value) { _rtpPort = value; OnPropertyChanged("RtpPort"); } }
        }

        /// <summary>
        /// The Media Meta Delivery RemoteEndPoint Port if we use Multicast 
        /// </summary>
        public int RtcpPort
        {
            get { return _rtcpPort; }
            set { if (_rtcpPort != value) { _rtcpPort = value; OnPropertyChanged("RtcpPort"); } }
        }

        #endregion
        
        public string Transport
        {
            get
            {
                if (string.IsNullOrEmpty(_transport))
                {
                    _transport = "unicast";
                }
                return _transport;
            }
            set 
            {
                if (_transport != value)
                {
                    _transport = value;
                    OnPropertyChanged("Transport");
                }
            }
        }        
        public int SignalLevel
        {
            get { return _signalLevel; }
            set { if (_signalLevel != value){_signalLevel = value; OnPropertyChanged("SignalLevel");} }
        }
        public int SignalQuality
        {
            get { return _signalQuality; }
            set { if (_signalQuality != value){_signalQuality = value; OnPropertyChanged("SignalQuality");} }
        }

        #endregion

        #region Private Methods

        private void Connect()
        {
            _rtspSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ip = IPAddress.Parse(RtspDevice.RtspServerAddress);
            var port = Convert.ToInt16(RtspDevice.RtspServerPort);
            var rtspEndpoint = new IPEndPoint(ip, port);
            _rtspSocket.Connect(rtspEndpoint);
        }

        private void Disconnect()
        {
            if (_rtspSocket != null && _rtspSocket.Connected)
            {
                _rtspSocket.Shutdown(SocketShutdown.Both);
                _rtspSocket.Close();
            }
        }
        private void SendRequest(RtspRequest request)
        {            
            if (_rtspSocket == null)
            {
                Connect();
            }
            try
            {
                request.Headers.Add("CSeq", _rtspSequenceNum.ToString(CultureInfo.InvariantCulture));
                _rtspSequenceNum++;
                var requestBytes = request.Serialise();
                if (_rtspSocket != null)
                {
                    var requestBytesCount = _rtspSocket.Send(requestBytes, requestBytes.Length, SocketFlags.None);
                    if (requestBytesCount < 1)
                    {
                    
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }           

        }
        private void ReceiveResponse(out RtspResponse response)
        {
            response = null;
            var responseBytes = new byte[1024];          
            
            try
            {
                var responseBytesCount = _rtspSocket.Receive(responseBytes, responseBytes.Length, SocketFlags.None);
                response = RtspResponse.Deserialise(responseBytes, responseBytesCount);
                string contentLengthString;
                if (response.Headers.TryGetValue("Content-Length", out contentLengthString))
                {
                    int contentLength = int.Parse(contentLengthString);
                    if ((string.IsNullOrEmpty(response.Body) && contentLength > 0) || response.Body.Length < contentLength)
                    {
                        if (response.Body == null)
                        {
                            response.Body = string.Empty;
                        }
                        while (responseBytesCount > 0 && response.Body.Length < contentLength)
                        {
                            responseBytesCount = _rtspSocket.Receive(responseBytes, responseBytes.Length, SocketFlags.None);
                            response.Body += System.Text.Encoding.UTF8.GetString(responseBytes, 0, responseBytesCount);
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                Logger.Error(socketException.Message);
            }                        
        }

        #endregion

        #region Public Methods

        public RtspStatusCode Setup(string query, string transporttype)
        {            
            RtspRequest request;
            RtspResponse response;           
            if ((_rtspSocket==null))
            {
                Connect();
            }
            if (string.IsNullOrEmpty(_rtspSessionId))
            {
                request = new RtspRequest(RtspMethod.Setup, string.Format("rtsp://{0}/?{1}", _rtspDevice.RtspServerAddress, query), 1, 0);
                switch (transporttype)
                {
                    case "multicast":
                        request.Headers.Add("Transport", string.Format("RTP/AVP;multicast"));
                        break;
                    case "unicast":
                        var activeTcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
                        var usedPorts = new HashSet<int>();
                        foreach (var connection in activeTcpConnections)
                        {
                            usedPorts.Add(connection.LocalEndPoint.Port);
                        }
                        for (var port = 40000; port <= 65534; port += 2)
                        {                            
                            if (!usedPorts.Contains(port) && !usedPorts.Contains(port + 1))
                            {

                                _clientRtpPort = port;
                                _clientRtcpPort = port + 1;
                                break;
                            }
                        }                        
                        request.Headers.Add("Transport", string.Format("RTP/AVP;unicast;client_port={0}-{1}", _clientRtpPort, _clientRtcpPort));
                        break;
                }
                request.Headers.Add("Connection", "close");
            }
            else
            {
                request = new RtspRequest(RtspMethod.Setup, string.Format("rtsp://{0}/?{1}", _rtspDevice.RtspServerAddress, query), 1, 0);
                switch (transporttype)
                {
                    case "multicast":
                        request.Headers.Add("Transport", string.Format("RTP/AVP;multicast"));
                        break;
                    case "unicast":                     
                        request.Headers.Add("Transport", string.Format("RTP/AVP;unicast;client_port={0}-{1}", _clientRtpPort, _clientRtcpPort));
                        break;
                }
                request.Headers.Add("Connection", "close");
            }
            SendRequest(request);
            ReceiveResponse(out response);
            if (!response.Headers.TryGetValue("com.ses.streamID", out _rtspStreamId))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate stream ID header in RTSP SETUP response"));
            }
            string sessionHeader;
            if (!response.Headers.TryGetValue("Session", out sessionHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
            }
            var m = RegexRtspSessionHeader.Match(sessionHeader);
            if (!m.Success)
            {
                Logger.Error("Failed to tune, RTSP SETUP response session header {0} format not recognised", sessionHeader);
            }
            _rtspSessionId = m.Groups[1].Captures[0].Value;
            _rtspSessionTimeToLive = m.Groups[3].Captures.Count == 1 ? int.Parse(m.Groups[3].Captures[0].Value) : DefaultRtspSessionTimeout;

            var foundRtpTransport = false;           
            string transportHeader;
            if (!response.Headers.TryGetValue("Transport", out transportHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate transport header in RTSP SETUP response"));
            }
            var transports = transportHeader.Split(',');
            foreach (var transport in transports)
            {
                if (transport.Trim().StartsWith("RTP/AVP"))
                {
                    foundRtpTransport = true;
                    var sections = transport.Split(';');
                    foreach (var section in sections)
                    {
                        var parts = section.Split('=');
                        if (parts[0].Equals("server_port"))
                        {
                            var ports = parts[1].Split('-');
                            _serverRtpPort = int.Parse(ports[0]);
                            _serverRtcpPort = int.Parse(ports[1]);
                        }
                        else if (parts[0].Equals("destination"))
                        {
                            _destination = parts[1];
                        }
                        else if (parts[0].Equals("port"))
                        {
                            var ports = parts[1].Split('-');
                            _rtpPort = int.Parse(ports[0]);
                            _rtcpPort = int.Parse(ports[1]);
                        }
                        else if (parts[0].Equals("ttl"))
                        {
                            _rtspSessionTimeToLive = int.Parse(parts[1]);
                        }
                        else if (parts[0].Equals("source"))
                        {
                            _source = parts[1];
                        }
                        else if (parts[0].Equals("client_port"))
                        {
                            var ports = parts[1].Split('-');
                            if (!ports[0].Equals(_clientRtpPort))
                            {
                                Logger.Error("SAT>IP base: server specified RTP client port {0} instead of {1}", ports[0], _clientRtpPort);
                            }
                            _clientRtpPort = int.Parse(ports[0]);
                            _clientRtcpPort = int.Parse(ports[1]);
                        }
                    }
                }
            }
            if (!foundRtpTransport)
            {
                Logger.Error(string.Format("Failed to tune, not able to locate RTP transport details in RTSP SETUP response transport header"));
            }
            return response.StatusCode;
        }

        public RtspStatusCode Play(string query)
        {
            RtspResponse response;
            if ((_rtspSocket == null) )
            {
                Connect();
            }
            var request = new RtspRequest(RtspMethod.Play, string.Format("rtsp://{0}/stream={1}?{2}", _rtspDevice.RtspServerAddress, _rtspStreamId, query), 1, 0);
            request.Headers.Add("Session", _rtspSessionId);
            request.Headers.Add("Connection", "close");
           
            SendRequest(request);
            ReceiveResponse(out response);

            string sessionHeader;
            if (!response.Headers.TryGetValue("Session", out sessionHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
            }
            string rtpinfoHeader;
            if (!response.Headers.TryGetValue("RTP-Info", out rtpinfoHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
            }            
            return response.StatusCode;
        }
        public RtspStatusCode Options()
        {
            RtspRequest request;
            RtspResponse response;
            if ((_rtspSocket == null) )
            {
                Connect();
            }
            
            if (string.IsNullOrEmpty(_rtspSessionId))
            {
                request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}/", _rtspDevice.RtspServerAddress), 1, 0);
                request.Headers.Add("Connection", "close");
            }
            else
            {
                request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}/", _rtspDevice.RtspServerAddress), 1, 0);
                request.Headers.Add("Session", _rtspSessionId);
                request.Headers.Add("Connection", "close");                
            }
            SendRequest(request);
            ReceiveResponse(out response);
            string optionsHeader;
            if (!response.Headers.TryGetValue("Public", out optionsHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP Options response"));
            }
            return response.StatusCode;
        }

        public RtspStatusCode Describe(out int level, out int quality)
        {
            RtspRequest request;
            RtspResponse response;
            level = 0;
            quality = 0;
            if ((_rtspSocket == null) )
            {
                Connect();
            }            
            if (string.IsNullOrEmpty(_rtspSessionId))
            {
                request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}/", _rtspDevice.RtspServerAddress), 1, 0);
                request.Headers.Add("Accept", "application/sdp");                
                request.Headers.Add("Connection", "close");
            }
            else
            {
                request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}/stream={1}", _rtspDevice.RtspServerAddress, _rtspStreamId), 1, 0);
                request.Headers.Add("Accept", "application/sdp");
                request.Headers.Add("Session", _rtspSessionId);
                request.Headers.Add("Connection", "close");
            }
            SendRequest(request);
            ReceiveResponse(out response);
            var m = RegexDescribeResponseSignalInfo.Match(response.Body);
            if (m.Success)
            {
                
                //isSignalLocked = m.Groups[2].Captures[0].Value.Equals("1");
                level = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;    // level: 0..255 => 0..100
                quality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;   // quality: 0..15 => 0..100
                
            }
            /*              
                v=0
                o=- 1378633020884883 1 IN IP4 192.168.2.108
                s=SatIPServer:1 4
                t=0 0
                a=tool:idl4k
                m=video 52780 RTP/AVP 33
                c=IN IP4 0.0.0.0
                b=AS:5000
                a=control:stream=4
                a=fmtp:33 ver=1.0;tuner=1,0,0,0,12344,h,dvbs2,,off,,22000,34;pids=0,100,101,102,103,106
                =sendonly
             */
           
            
            return response.StatusCode;
            
        }
        public RtspStatusCode TearDown()
        {
            RtspResponse response;
            if ((_rtspSocket == null) )
            {
                Connect();
            }
            var request = new RtspRequest(RtspMethod.Teardown, string.Format("rtsp://{0}/stream={1}", _rtspDevice.RtspServerAddress, _rtspStreamId), 1, 0);
            request.Headers.Add("Session", _rtspSessionId);
            request.Headers.Add("Connection", "close");
            
            SendRequest(request);
            ReceiveResponse(out response);            
            Disconnect();
            return response.StatusCode;
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
            TearDown();
            Disconnect();
        }
    }
}
