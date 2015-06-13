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
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using SatIp.RtspSample.Rtsp;

public class RtspSession : INotifyPropertyChanged, IDisposable
{
    #region Private Fields
    private static readonly Regex RegexRtspSessionHeader = new Regex(@"\s*([^\s;]+)(;timeout=(\d+))?");
    private const int DefaultRtspSessionTimeout = 30;    // unit = s
    private static readonly Regex RegexDescribeResponseSignalInfo = new Regex(@";tuner=\d+,(\d+),(\d+),(\d+),", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private RtspDevice _rtspDevice;
    private string _rtspSessionId;
    private int _rtspSessionTimeToLive;
    private string _rtspStreamId;
    private int _clientRtpPort;
    private int _clientRtcpPort;
    private int _serverRtpPort;
    private int _serverRtcpPort;
    private string _rtspStreamUrl;
    private string _destination;
    private string _source;
    private string _transport;
    private int _signalLevel;
    private int _signalQuality;
    private RtspClient _rtspClient;
    private bool _disposed;
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
        get
        {
            if (_rtspSessionTimeToLive == 0)
                _rtspSessionTimeToLive = DefaultRtspSessionTimeout;
            return _rtspSessionTimeToLive * 1000 - 20;
        }
        set { if (_rtspSessionTimeToLive != value) { _rtspSessionTimeToLive = value; OnPropertyChanged("RtspSessionTimeToLive"); } }
    }
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
        get
        {
            if (string.IsNullOrEmpty(_source))
            { _source = _rtspDevice.ServerAddress; }
            return _source;
        }
        set { if (_source != value) { _source = value; OnPropertyChanged("Source"); } }
    }

    /// <summary>
    /// The Media Data Delivery RemoteEndPoint Port if we use Unicast
    /// </summary>
    public int ServerRtpPort
    {
        get { return _serverRtpPort; }
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
        set { if (_signalLevel != value) { _signalLevel = value; OnPropertyChanged("SignalLevel"); } }
    }
    public int SignalQuality
    {
        get { return _signalQuality; }
        set { if (_signalQuality != value) { _signalQuality = value; OnPropertyChanged("SignalQuality"); } }
    }

    #endregion

    #region Private Methods
    private void ProcessSessionHeader(string sessionHeader)
    {
        if(!string.IsNullOrEmpty(sessionHeader))
        {
            var m = RegexRtspSessionHeader.Match(sessionHeader);
            if (!m.Success)
            {
                //Logger.Error("Failed to tune, RTSP SETUP response session header {0} format not recognised", sessionHeader);
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
                            _serverRtpPort = int.Parse(ports[0]);
                            _serverRtcpPort = int.Parse(ports[1]);
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
                            var rtp = int.Parse(ports[0]);
                            var rtcp = int.Parse(ports[1]);
                            if (!rtp.Equals(_clientRtpPort))
                            {
                                //Logger.Error("SAT>IP base: server specified RTP client port {0} instead of {1}", rtp, _clientRtpPort);
                            }
                            if (!rtcp.Equals(_clientRtcpPort))
                            {
                                //Logger.Error("SAT>IP base: server specified RTCP client port {0} instead of {1}", rtcp, _clientRtcpPort);
                            }
                            _clientRtpPort = rtp;
                            _clientRtcpPort = rtcp;
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Public Methods

    public RtspStatusCode Setup(string query, string transporttype)
    {
        RtspRequest request;
        RtspResponse response;
        _rtspClient = new RtspClient(_rtspDevice.ServerAddress);
        if (string.IsNullOrEmpty(_rtspSessionId))
        {
            request = new RtspRequest(RtspMethod.Setup, string.Format("rtsp://{0}:554/?{1}", _rtspDevice.ServerAddress, query), 1, 0);
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
        }
        else
        {
            request = new RtspRequest(RtspMethod.Setup, string.Format("rtsp://{0}:554/?{1}", _rtspDevice.ServerAddress, query), 1, 0);
            switch (transporttype)
            {
                case "multicast":
                    request.Headers.Add("Transport", string.Format("RTP/AVP;multicast"));
                    break;
                case "unicast":
                    request.Headers.Add("Transport", string.Format("RTP/AVP;unicast;client_port={0}-{1}", _clientRtpPort, _clientRtcpPort));
                    break;
            }
        }
        if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
        {
            //Logger.Error("Failed to tune, non-OK RTSP SETUP status code {0} {1}", response.StatusCode, response.ReasonPhrase);
        }
        //Logger.Info("RtspSession-Setup : \r\n {0}", response);
        if (!response.Headers.TryGetValue("com.ses.streamID", out _rtspStreamId))
        {
            //Logger.Error(string.Format("Failed to tune, not able to locate stream ID header in RTSP SETUP response"));
        }
        string sessionHeader;
        if (!response.Headers.TryGetValue("Session", out sessionHeader))
        {
            //Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
        }
        ProcessSessionHeader(sessionHeader);
        string transportHeader;
        if (!response.Headers.TryGetValue("Transport", out transportHeader))
        {
            //Logger.Error(string.Format("Failed to tune, not able to locate transport header in RTSP SETUP response"));
        }
        ProcessTransportHeader(transportHeader);
        return response.StatusCode;
    }

    public RtspStatusCode Play(string query)
    {
        _rtspClient = new RtspClient(_rtspDevice.ServerAddress);
        RtspResponse response;
        string data;
        if (string.IsNullOrEmpty(query))
        {
            data = string.Format("rtsp://{0}:554/stream={1}", _rtspDevice.ServerAddress,
                 _rtspStreamId);
        }
        else
        {
            data = string.Format("rtsp://{0}:554/stream={1}?{2}", _rtspDevice.ServerAddress,
                 _rtspStreamId, query);
        }

        var request = new RtspRequest(RtspMethod.Play, data, 1, 0);
        request.Headers.Add("Session", _rtspSessionId);

        if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
        {
            //Logger.Error("Failed to tune, non-OK RTSP SETUP status code {0} {1}", response.StatusCode, response.ReasonPhrase);
        }
        //Logger.Info("RtspSession-Play : \r\n {0}", response);
        string sessionHeader;
        if (!response.Headers.TryGetValue("Session", out sessionHeader))
        {
            // Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
        }
        ProcessSessionHeader(sessionHeader);
        string rtpinfoHeader;
        if (!response.Headers.TryGetValue("RTP-Info", out rtpinfoHeader))
        {
            //Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
        }
        return response.StatusCode;
    }

    public RtspStatusCode Options()
    {
        _rtspClient = new RtspClient(_rtspDevice.ServerAddress);
        RtspRequest request;
        RtspResponse response;
        request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}:554/", _rtspDevice.ServerAddress), 1, 0);
        if (!string.IsNullOrEmpty(_rtspSessionId))
        {
            request.Headers.Add("Session", _rtspSessionId);
        }
        if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
        {
            //Logger.Error("Failed to tune, non-OK RTSP SETUP status code {0} {1}", response.StatusCode, response.ReasonPhrase);
        }
        string sessionHeader;
        if (!response.Headers.TryGetValue("Session", out sessionHeader))
        {
            // Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
        }
        ProcessSessionHeader(sessionHeader);
        //Logger.Info("RtspSession-Options : \r\n {0}", response);
        string optionsHeader;
        if (!response.Headers.TryGetValue("Public", out optionsHeader))
        {
            //Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP Options response"));
        }
        return response.StatusCode;
    }

    public RtspStatusCode Describe(out int level, out int quality)
    {
        _rtspClient = new RtspClient(_rtspDevice.ServerAddress);
        RtspRequest request;
        RtspResponse response;
        level = 0;
        quality = 0;

        if (string.IsNullOrEmpty(_rtspSessionId))
        {
            request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}:554/", _rtspDevice.ServerAddress), 1, 0);
            request.Headers.Add("Accept", "application/sdp");
        }
        else
        {
            request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}:554/stream={1}", _rtspDevice.ServerAddress, _rtspStreamId), 1, 0);
            request.Headers.Add("Accept", "application/sdp");
            request.Headers.Add("Session", _rtspSessionId);
        }
        if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
        {
            //Logger.Error("Failed to tune, non-OK RTSP SETUP status code {0} {1}", response.StatusCode, response.ReasonPhrase);
        }
        string sessionHeader;
        if (!response.Headers.TryGetValue("Session", out sessionHeader))
        {
            // Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
        }
        ProcessSessionHeader(sessionHeader);
        //Logger.Info("RtspSession-Describe : \r\n {0}", response);
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
        _rtspClient = new RtspClient(_rtspDevice.ServerAddress);
        RtspResponse response;

        var request = new RtspRequest(RtspMethod.Teardown, string.Format("rtsp://{0}:554/stream={1}", _rtspDevice.ServerAddress, _rtspStreamId), 1, 0);
        request.Headers.Add("Session", _rtspSessionId);

        if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
        {
            //Logger.Error("Failed to tune, non-OK RTSP SETUP status code {0} {1}", response.StatusCode, response.ReasonPhrase);
        }
        string sessionHeader;
        if (!response.Headers.TryGetValue("Session", out sessionHeader))
        {
            // Logger.Error(string.Format("Failed to tune, not able to locate session header in RTSP SETUP response"));
        }
        ProcessSessionHeader(sessionHeader);
        //Logger.Info("RtspSession-TearDown : \r\n {0}", response);
        return response.StatusCode;
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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
                TearDown();
                if (_rtspClient != null)
                    _rtspClient.Dispose();
            }
        }
        _disposed = true;
    }
    #endregion
}

