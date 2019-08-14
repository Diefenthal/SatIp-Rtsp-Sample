/*  
    Copyright (C) <2007-2017>  <Kay Diefenthal>

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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using SatIp.RtspSample.Logging;
using SatIp.RtspSample.Rtp;
using SatIp.RtspSample.Rtcp;


namespace SatIp.RtspSample.Rtsp
{
    public class RtspSession : INotifyPropertyChanged ,IDisposable
    {

        #region Private Fields
        private static readonly Regex RegexRtspSessionHeader = new Regex(@"\s*([^\s;]+)(;timeout=(\d+))?");
        private const int DefaultRtspSessionTimeout = 30;    // unit = s
        private static readonly Regex RegexDescribeResponseSignalInfo = new Regex(@";tuner=\d+,(\d+),(\d+),(\d+),", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private RtspDevice _rtspDevice;
        /// <summary>
        /// The current RTSP session ID. Used in the header of all RTSP messages
        /// sent to the server.
        /// </summary>
        private string _rtspSessionId;
        /// <summary>
        /// The time after which the SAT>IP server will stop streaming if it does
        /// not receive some kind of interaction.
        /// </summary>
        private int _rtspSessionTimeToLive =0;
        private string _rtspStreamId;
        /// <summary>
        /// The port that the RTP listener thread listens to.
        /// </summary>
        private int _serverRtpPort;
        /// <summary>
        /// The port that the RTCP listener thread listens to.
        /// </summary>
        private int _serverRtcpPort;
        /// <summary>
        /// The port on which the RTP listener thread listens.
        /// </summary>
        private int _rtpPort;
        /// <summary>
        /// The port on which the RTCP listener thread listens.
        /// </summary>
        private int _rtcpPort;        
        //private string _rtspStreamUrl;
        /// <summary>
        /// The Address on which the RTP RTCP listener thread listens.
        /// </summary>
        private string _destination;
        /// <summary>
        /// The Address that the RTP RTCP listener thread listens to.
        /// </summary>
        private string _source;
       
        private int _signalLevel;        
        private int _signalQuality;
        private bool _signalLocked;
        private Socket _rtspSocket;
        /// <summary>
        /// A thread, used to periodically send RTSP OPTIONS to tell the SAT>IP
        /// server not to stop streaming.
        /// </summary>
        private Thread _keepAliveThread = null;
        /// <summary>
        /// An event, used to stop the streaming keep-alive thread.
        /// </summary>
        private AutoResetEvent _keepAliveThreadStopEvent = null;

        private int _rtspSequenceNum = 1;
        private bool _disposed = false;
        private RtcpListener _rtcpListener;
        private RtpListener _rtpListener;

        #endregion

        #region Constructor

        public RtspSession(RtspDevice rtspDevice)
        {            
            _rtspDevice = rtspDevice;           
        }
        ~RtspSession()
        {
            Dispose(false);
        }   
        #endregion

        #region Properties

        #region Rtsp

        public string RtspStreamId
        {
            get { return _rtspStreamId; }
            set { if (_rtspStreamId != value) { _rtspStreamId = value; OnPropertyChanged("RtspStreamId"); } }
        }
        
        public RtspDevice RtspDevice
        {
            get { return _rtspDevice; }
            set { if (_rtspDevice != value) { _rtspDevice = value; OnPropertyChanged("RtspDevice"); } }
        }

        public int RtspSessionTimeToLive
        {
            get
            {
                if (_rtspSessionTimeToLive == 0)
                    _rtspSessionTimeToLive = DefaultRtspSessionTimeout;
                return _rtspSessionTimeToLive * 1000 - 20;
            }
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
                    var result = Utils.GetLocalIPAddress();
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

        #endregion

        #region Private Methods

        private void ProcessSessionHeader(string sessionHeader ,string response)
        {
            if (!string.IsNullOrEmpty(sessionHeader))
            {
                var m = RegexRtspSessionHeader.Match(sessionHeader);
                if (!m.Success)
                {
                    Logger.Error("Failed to tune, RTSP {0} response session header {1} format not recognised",response, sessionHeader);
                }
                _rtspSessionId = m.Groups[1].Captures[0].Value;
                _rtspSessionTimeToLive = m.Groups[3].Captures.Count == 1 ? int.Parse(m.Groups[3].Captures[0].Value) : DefaultRtspSessionTimeout;
            }
        }
        private void ProcessTransportHeader(string transportHeader)
        {
            if (!string.IsNullOrEmpty(transportHeader))
            {
                var transports = transportHeader.Split(',');
                foreach (var transport in transports)
                {
                    if (transport.Trim().StartsWith("RTP/AVP"))
                    {
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
                                _rtpPort = int.Parse(ports[0]);
                                _rtcpPort = int.Parse(ports[1]);
                            }
                        }
                    }
                }
            }
        }
        private void Connect()
        {
            _rtspSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ip = IPAddress.Parse(_rtspDevice.ServerAddress);
            var rtspEndpoint = new IPEndPoint(ip, 554);
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
                request.Headers.Add("CSeq", _rtspSequenceNum.ToString());
                _rtspSequenceNum++;
                byte[] requestBytes = request.Serialise();
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
            var responseBytesCount = 0;
            byte[] responseBytes = new byte[1024];
            try
            {
                responseBytesCount = _rtspSocket.Receive(responseBytes, responseBytes.Length, SocketFlags.None);
                response = RtspResponse.Deserialise(responseBytes, responseBytesCount);
                string contentLengthString;
                int contentLength = 0;
                if (response.Headers.TryGetValue("Content-Length", out contentLengthString))
                {
                    contentLength = int.Parse(contentLengthString);
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
            catch (SocketException)
            {
            }
        }        
        private void StartKeepAliveThread()
        {

            if (_keepAliveThread != null && !_keepAliveThread.IsAlive)
            {
                StopKeepAliveThread();
            }

            if (_keepAliveThread == null)
            {
                Logger.Info("SAT>IP : starting new keep-alive thread");
                _keepAliveThreadStopEvent = new AutoResetEvent(false);
                _keepAliveThread = new Thread(new ThreadStart(KeepAlive));
                _keepAliveThread.Name = string.Format("SAT>IP tuner  keep-alive");
                _keepAliveThread.IsBackground = true;
                _keepAliveThread.Priority = ThreadPriority.Lowest;
                _keepAliveThread.Start();
            }
        }
        private void StopKeepAliveThread()
        {
            if (_keepAliveThread != null)
            {
                if (!_keepAliveThread.IsAlive)
                {
                    Logger.Critical("SAT>IP : aborting old keep-alive thread");
                    _keepAliveThread.Abort();
                }
                else
                {
                    _keepAliveThreadStopEvent.Set();
                    if (!_keepAliveThread.Join(RtspSessionTimeToLive))
                    {
                        Logger.Critical("SAT>IP : failed to join keep-alive thread, aborting thread");
                        _keepAliveThread.Abort();
                    }
                }
                _keepAliveThread = null;
                if (_keepAliveThreadStopEvent != null)
                {
                    _keepAliveThreadStopEvent.Close();
                    _keepAliveThreadStopEvent = null;
                }
            }
        }
        private void KeepAlive()
        {
            try
            {
                while (!_keepAliveThreadStopEvent.WaitOne(RtspSessionTimeToLive))    // -5 seconds to avoid timeout
                {
                    if ((_rtspSocket == null))
                    {
                        Connect();
                    }
                    RtspRequest request;
                    if (string.IsNullOrEmpty(_rtspSessionId))
                    {
                        request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}:{1}/", _rtspDevice.ServerAddress, 554), 1, 0);
                    }
                    else
                    {
                        request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}:{1}/", _rtspDevice.ServerAddress, 554), 1, 0);
                        request.Headers.Add("Session", _rtspSessionId);
                    }
                    RtspResponse response;
                    SendRequest(request);
                    ReceiveResponse(out response);
                    if (response.StatusCode != RtspStatusCode.Ok)
                    {
                        Logger.Critical("SAT>IP : keep-alive request/response failed, non-OK RTSP OPTIONS status code {0} {1}", response.StatusCode, response.ReasonPhrase);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), "SAT>IP : keep-alive thread exception");
                return;
            }
            Logger.Info("SAT>IP : keep-alive thread stopping");
        }
        
        #endregion

        #region Public Methods

        public RtspStatusCode Setup(string query, TransmissionMode transmissionmode)
        {   
            RtspRequest request;
            RtspResponse response;            
            if ((_rtspSocket == null))
            {
                Connect();
            }
            if (string.IsNullOrEmpty(_rtspSessionId))
            {
                request = new RtspRequest(RtspMethod.Setup, string.Format("rtsp://{0}:{1}/?{2}", _rtspDevice.ServerAddress,554, query), 1, 0);
                switch (transmissionmode)
                {
                    case TransmissionMode.Multicast:
                        request.Headers.Add("Transport", string.Format("RTP/AVP;{0}", transmissionmode.ToString().ToLower()));
                        break;
                    case TransmissionMode.Unicast:
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
                                _rtpPort = port;
                                _rtcpPort = port + 1;
                                break;
                            }
                        }
                        request.Headers.Add("Transport", string.Format("RTP/AVP;{0};client_port={1}-{2}", transmissionmode.ToString().ToLower(), _rtpPort, _rtcpPort));
                        break;
                }
            }
            else
            {
                request = new RtspRequest(RtspMethod.Setup, string.Format("rtsp://{0}:{1}/?{2}", _rtspDevice.ServerAddress,554, query), 1, 0);
                switch (transmissionmode)
                {
                    case TransmissionMode.Multicast:
                        request.Headers.Add("Session", _rtspSessionId);
                        request.Headers.Add("Transport", string.Format("RTP/AVP;{0}", transmissionmode.ToString().ToLower()));
                        break;
                    case TransmissionMode.Unicast:
                        request.Headers.Add("Session", _rtspSessionId);
                        request.Headers.Add("Transport", string.Format("RTP/AVP;{0};client_port={1}-{2}", transmissionmode.ToString().ToLower(), _rtpPort, _rtcpPort));
                        break;
                }                
            }
            SendRequest(request);
            ReceiveResponse(out response);
            if (response.StatusCode == RtspStatusCode.Ok)
            {
                if (!response.Headers.TryGetValue("com.ses.streamID", out _rtspStreamId))
                {
                    Logger.Error(string.Format("Failed to tune, not able to locate Stream ID header in RTSP SETUP response"));
                }
                string sessionHeader;
                if (!response.Headers.TryGetValue("Session", out sessionHeader))
                {
                    Logger.Error(string.Format("Failed to tune, not able to locate Session header in RTSP SETUP response"));
                }
                ProcessSessionHeader(sessionHeader,"Setup");                      
                string transportHeader;
                if (!response.Headers.TryGetValue("Transport", out transportHeader))
                {
                    Logger.Error(string.Format("Failed to tune, not able to locate Transport header in RTSP SETUP response"));
                }
                ProcessTransportHeader(transportHeader);

                StartKeepAliveThread();
                _rtpListener = new RtpListener(Destination, RtpPort, transmissionmode);                
                _rtpListener.StartRtpListenerThread();

                _rtcpListener = new RtcpListener(Destination, RtcpPort, transmissionmode);
                _rtcpListener.PacketReceived += new RtcpListener.PacketReceivedHandler(RtcpPacketReceived);
                _rtcpListener.StartRtcpListenerThread();
            }
            return response.StatusCode;
        }        

        public RtspStatusCode Play(string query)
        {
            if ((_rtspSocket == null))
            {
                Connect();
            }            
            RtspResponse response;
            string data;
            if (string.IsNullOrEmpty(query))
            {
                data = string.Format("rtsp://{0}:{1}/stream={2}", _rtspDevice.ServerAddress,
                    554, _rtspStreamId);
            }
            else
            {
                data = string.Format("rtsp://{0}:{1}/stream={2}?{3}", _rtspDevice.ServerAddress,
                    554, _rtspStreamId, query);
            }
            var request = new RtspRequest(RtspMethod.Play, data, 1, 0);
            request.Headers.Add("Session", _rtspSessionId);
            SendRequest(request);
            ReceiveResponse(out response);
            string sessionHeader;
            if (!response.Headers.TryGetValue("Session", out sessionHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate Session header in RTSP Play response"));
            }
            ProcessSessionHeader(sessionHeader,"Play");
            string rtpinfoHeader;
            if (!response.Headers.TryGetValue("RTP-Info", out rtpinfoHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate Rtp-Info header in RTSP Play response"));
            }
            return response.StatusCode;
        }

        public RtspStatusCode Options()
        {
            if ((_rtspSocket == null))
            {
                Connect();
            }            
            RtspRequest request;
            RtspResponse response;
            if (string.IsNullOrEmpty(_rtspSessionId))
            {
                request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}:{1}/", _rtspDevice.ServerAddress,554), 1, 0);
            }
            else
            {
                request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}:{1}/", _rtspDevice.ServerAddress, 554), 1, 0);
                request.Headers.Add("Session", _rtspSessionId);
            }
            SendRequest(request);
            ReceiveResponse(out response);           
            string sessionHeader;
            if (!response.Headers.TryGetValue("Session", out sessionHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP Options response"));
            }
            ProcessSessionHeader(sessionHeader,"Options");
            string optionsHeader;
            if (!response.Headers.TryGetValue("Public", out optionsHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to Options header in RTSP Options response"));
            }
            return response.StatusCode;
        }

        public RtspStatusCode Describe()
        {
            if ((_rtspSocket == null))
            {
                Connect();
            }            
            RtspRequest request;
            RtspResponse response;
                               
            if (string.IsNullOrEmpty(_rtspSessionId))
            {
                request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}:{1}/", _rtspDevice.ServerAddress, 554), 1, 0);
                request.Headers.Add("Accept", "application/sdp");                
            }
            else
            {
                request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}:{1}/stream={2}", _rtspDevice.ServerAddress, 554, _rtspStreamId), 1, 0);
                request.Headers.Add("Accept", "application/sdp");
                request.Headers.Add("Session", _rtspSessionId);
            }
            SendRequest(request);
            ReceiveResponse(out response);            
            string sessionHeader;
            if (!response.Headers.TryGetValue("Session", out sessionHeader))
            {
                Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP Describe response"));
            }
            ProcessSessionHeader(sessionHeader,"Describe");
            var m = RegexDescribeResponseSignalInfo.Match(response.Body);
            if (m.Success)
            {                
                _signalLocked = m.Groups[2].Captures[0].Value.Equals("1");
                _signalLevel = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;    // level: 0..255 => 0..100
                _signalQuality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;   // quality: 0..15 => 0..100
                
            }
            OnRecieptionInfoChanged(new RecieptionInfoArgs(_signalLocked, _signalLevel, _signalQuality));
              
            return response.StatusCode;            
        }

        public RtspStatusCode TearDown()
        {
            if ((_rtspSocket == null))
            {
                Connect();
            }
            RtspResponse response;            
            var request = new RtspRequest(RtspMethod.Teardown, string.Format("rtsp://{0}:{1}/stream={2}", _rtspDevice.ServerAddress,554, _rtspStreamId), 1, 0);
            request.Headers.Add("Session", _rtspSessionId);
            SendRequest(request);
            ReceiveResponse(out response);

            if (_rtpListener != null)
            {
                _rtpListener.Dispose();                
                _rtpListener = null;
            }
            if (_rtcpListener != null)
            {
                _rtcpListener.Dispose();          
                _rtcpListener.PacketReceived -= new RtcpListener.PacketReceivedHandler(RtcpPacketReceived);                
                _rtcpListener = null;
            }
            StopKeepAliveThread();
            return response.StatusCode;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//Disconnect();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;
        public event RecieptionInfoChangedEventHandler RecieptionInfoChanged;        

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
        protected void RtcpPacketReceived(object sender, RtcpPacketReceivedArgs e)
        {
            if (e.Packet is RtcpAppPacket)
            {
                RtcpAppPacket apppacket = (RtcpAppPacket)e.Packet;
                var m = RegexDescribeResponseSignalInfo.Match(apppacket.Data);
                if (m.Success)
                {
                    _signalLocked = m.Groups[2].Captures[0].Value.Equals("1");
                    _signalLevel = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;    // level: 0..255 => 0..100
                    _signalQuality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;   // quality: 0..15 => 0..100
                }
                OnRecieptionInfoChanged(new RecieptionInfoArgs(_signalLocked,_signalLevel,_signalQuality));
            }

            else if (e.Packet is RtcpByePacket)
            {
                TearDown();
                
            }
        }
        protected void OnRecieptionInfoChanged(RecieptionInfoArgs args)
        {
            if (RecieptionInfoChanged != null)
            {
                RecieptionInfoChanged(this, args);
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_rtcpListener != null)
                    {
                        _rtcpListener.StopRtcpListenerThread();
                        _rtcpListener.PacketReceived -= new RtcpListener.PacketReceivedHandler(RtcpPacketReceived);
                        _rtcpListener.Dispose();
                    }
                    TearDown();
                    Disconnect();
                }
            }
            _disposed = true;
        }
        #endregion
    }
    public delegate void RecieptionInfoChangedEventHandler(object sender, RecieptionInfoArgs e);
    public class RecieptionInfoArgs : EventArgs
    {
        public bool Locked { get; private set; }
        public int Level { get; private set; }
        public int Quality { get; private set; }
        public RecieptionInfoArgs(bool locked, int level, int quality)
        {
            Locked = locked;
            Level = level;
            Quality = quality;
        }
    }
}
