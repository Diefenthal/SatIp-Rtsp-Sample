using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;



namespace SatIp.Rtsp.Sample
{
    public class UdpState
    {
        public UdpClient U;
        public IPEndPoint E;
    }

    /// <summary>
    /// Represends a SSDP Client 
    /// </summary>
    public class SsdpClient : IDisposable
    {
        private static readonly Regex HttpResponseRegex = new Regex(@"HTTP/(\d+)\.(\d+)\s+(\d+)\s+([^.]+?)\r\n(.*)",
            RegexOptions.Singleline);

        private static readonly Regex MSearchResponseRegex = new Regex(@"M-SEARCH \* HTTP/(\d+)\.(\d+)\r\n(.*)",
            RegexOptions.Singleline);

        private static readonly Regex NotifyResponseRegex = new Regex(@"NOTIFY \* HTTP/(\d+)\.(\d+)\r\n(.*)",
            RegexOptions.Singleline);

        #region Private Fields

        private bool _running;
        private readonly string _multicastIp;
        private readonly int _multicastPort;
        private readonly int _unicastPort;
        private UdpClient _multicastClient;
        private UdpClient _unicastClient;
        private Dictionary<string,object> _locations = new Dictionary<string,object>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a new instance of <see cref="SsdpClient"/> Class.
        /// It send SsdpReqeust(M-Search) and receives SsdpResponses(Http,M-Search,Notify) 
        /// </summary>
        public SsdpClient()
        {
            _multicastIp = "239.255.255.250";
            _multicastPort = 1900;
            _unicastPort = 1901;
            _running = false;
        }

        ~SsdpClient()
        {
            Dispose(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The MulticastReceiveCallback receives M-Search and Notify Responses
        /// M-Search Responses are ignored 
        /// Notify Responses fires SatIpDeviceLost Event if nts.Equals(ssdp:byebye) for removing the Device  
        /// or SatIpDeviceNotify Event if nts.Equals(ssdp:alive) for inform that the SatIp device is alive
        /// it can be used for initialize a new M-Seach Request too 
        /// </summary>
        /// <param name="ar"></param>
        private void MulticastReceiveCallback(IAsyncResult ar)
        {
            var u = ((UdpState) (ar.AsyncState)).U;
            var e = ((UdpState) (ar.AsyncState)).E;
            if (u.Client != null)
            {
                var responseBytes = u.EndReceive(ar, ref e);
                var responseString = Encoding.ASCII.GetString(responseBytes);
                var msearchMatch = MSearchResponseRegex.Match(responseString);
                if (msearchMatch.Success)
                {
                    responseString = msearchMatch.Groups[3].Captures[0].Value;
                    var headerDictionary = GetResponseKeysandValues(responseString);
                    string host;
                    headerDictionary.TryGetValue("host", out host);
                    string man;
                    headerDictionary.TryGetValue("man", out man);
                    string mx;
                    headerDictionary.TryGetValue("mx", out mx);
                    string st;
                    headerDictionary.TryGetValue("st", out st);
                }
                var notifyMatch = NotifyResponseRegex.Match(responseString);
                if (notifyMatch.Success)
                {
                    responseString = notifyMatch.Groups[3].Captures[0].Value;
                    var headerDictionary = GetResponseKeysandValues(responseString);
                    string location;
                    headerDictionary.TryGetValue("location", out location);
                    string host;
                    headerDictionary.TryGetValue("host", out host);
                    string nt;
                    headerDictionary.TryGetValue("nt", out nt);
                    string nts;
                    headerDictionary.TryGetValue("nts", out nts);
                    string usn;
                    headerDictionary.TryGetValue("usn", out usn);
                    string bootId;
                    headerDictionary.TryGetValue("bootid.upnp.org", out bootId);
                    string configId;
                    headerDictionary.TryGetValue("configid.upnp.org", out configId);
                    if (nts != null &&
                        (nt != null && (nt.Equals("urn:ses-com:device:SatIPServer:1") && nts.Equals("ssdp:byebye"))))
                    {
                        if (usn != null)
                        {
                            var usnsections = usn.Split(':');
                            OnDeviceLost(new SatIpDeviceLostArgs(usnsections[0] + ":" + usnsections[1]));
                        }
                    }
                    if (nts != null &&
                        (nt != null && (nt.Equals("urn:ses-com:device:SatIPServer:1") && nts.Equals("ssdp:alive"))))
                    {
                        if (usn != null)
                        {
                            var usnsections = usn.Split(':');
                            OnDeviceNotify(new SatIpDeviceNotifyArgs(usnsections[0] + ":" + usnsections[1]));
                        }
                    }
                }
            }
            if (_running)
                MulticastSetBeginReceive();
        }

        /// <summary>
        /// Listen for Multicast SSDP Responses
        /// </summary>
        private void MulticastSetBeginReceive()
        {
            var ipSsdp = IPAddress.Parse(_multicastIp);
            var ipRxEnd = new IPEndPoint(ipSsdp, _multicastPort);
            UdpState udpListener = new UdpState {E = ipRxEnd};
            if (_multicastClient == null)
                _multicastClient = new UdpClient(_multicastPort);
            udpListener.U = _multicastClient;
            _multicastClient.BeginReceive(MulticastReceiveCallback, udpListener);
        }

        /// <summary>
        /// The UnicastReceiveCallback receives Http Responses 
        /// and Fired the SatIpDeviceFound Event for adding the SatIpDevice  
        /// </summary>
        /// <param name="ar"></param>
        private void UnicastReceiveCallback(IAsyncResult ar)
        {
            var u = ((UdpState) (ar.AsyncState)).U;
            var e = ((UdpState) (ar.AsyncState)).E;
            if (u.Client != null)
            {
                var responseBytes = u.EndReceive(ar, ref e);
                var responseString = Encoding.ASCII.GetString(responseBytes);
                var httpMatch = HttpResponseRegex.Match(responseString);
                if (httpMatch.Success)
                {
                    responseString = httpMatch.Groups[5].Captures[0].Value;
                    var headerDictionary = GetResponseKeysandValues(responseString);
                    string location;
                    headerDictionary.TryGetValue("location", out location);
                    string st;
                    headerDictionary.TryGetValue("st", out st);
                    string uuid;
                    headerDictionary.TryGetValue("usn", out uuid);
                    string bootId;
                    headerDictionary.TryGetValue("bootid.upnp.org", out bootId);
                    string configId;
                    headerDictionary.TryGetValue("configid.upnp.org", out configId);
                    string deviceId;
                    headerDictionary.TryGetValue("deviceid.ses.com", out deviceId);
                    if (!string.IsNullOrEmpty(location))
                    {

                        //var desciption = SatIpDeviceDescription.ProcessDeviceDescription(new Uri(location));
                        //var device = new SatIpDevice(new Uri(location));
                        //OnDeviceFound(new SatIpDeviceFoundArgs(device));
                    }
                }

                if (_running)
                    UnicastSetBeginReceive();
            }
        }        

        /// <summary>
        /// Listen for Unicast SSDP Responses
        /// </summary>
        private void UnicastSetBeginReceive()
        {
            var ipRxEnd = new IPEndPoint(IPAddress.Any, _unicastPort);
            var udpListener = new UdpState {E = ipRxEnd};
            if (_unicastClient == null)
                _unicastClient = new UdpClient(_unicastPort);
            udpListener.U = _unicastClient;
            _unicastClient.BeginReceive(UnicastReceiveCallback, udpListener);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchResponse"></param>
        /// <returns>a Dictionary with Key and Values</returns>
        public Dictionary<string, string> GetResponseKeysandValues(string searchResponse)
        {
            var reader = new StringReader(searchResponse);
            var line = reader.ReadLine();
            var values = new Dictionary<string, string>();
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "")
                {
                    continue;
                }
                var colon = line.IndexOf(':');
                if (colon < 1)
                {
                    return null;
                }
                var name = line.Substring(0, colon).Trim();
                var value = line.Substring(colon + 1).Trim();
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }
                values[name.ToLowerInvariant()] = value;
            }
            return values;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Sends SsdpRequest M-SEARCH 
        /// </summary>
        /// <param name="searchterm"></param>
        public void FindByType(string searchterm = "urn:ses-com:device:SatIPServer:1")
        {
            var query = new StringBuilder();
            query.Append("M-SEARCH * HTTP/1.1\r\n");
            query.Append("HOST: 239.255.255.250:1900\r\n");
            query.Append("MAN: \"ssdp:discover\"\r\n");
            query.Append("MX: 2\r\n");
            query.Append("ST: " + searchterm + "\r\n");
            query.Append("\r\n");
            if (_unicastClient == null)
            {
                _unicastClient = new UdpClient(_unicastPort);
            }
            byte[] req = Encoding.ASCII.GetBytes(query.ToString());
            var ipSsdp = IPAddress.Parse(_multicastIp);
            var ipTxEnd = new IPEndPoint(ipSsdp, _multicastPort);
            for (var i = 0; i < 3; i++)
            {
                if (i > 0)
                    Thread.Sleep(50);
                _unicastClient.Send(req, req.Length, ipTxEnd);
            }
        }

        public void FindByUuid(Uri location, string searchterm = "urn:ses-com:device:SatIPServer:1")
        {
            var query = new StringBuilder();
            query.Append("M-SEARCH * HTTP/1.1\r\n");
            query.Append("HOST: " + location.Host + ":1900\r\n");
            query.Append("MAN: \"ssdp:discover\"\r\n");
            query.Append("MX: 2\r\n");
            query.Append("ST: " + searchterm + "\r\n");
            query.Append("\r\n");
            if (_unicastClient == null)
            {
                _unicastClient = new UdpClient(_unicastPort);
            }
            byte[] req = Encoding.ASCII.GetBytes(query.ToString());
            var ipSsdp = IPAddress.Parse(_multicastIp);
            var ipTxEnd = new IPEndPoint(ipSsdp, _multicastPort);
            for (var i = 0; i < 3; i++)
            {
                if (i > 0)
                    Thread.Sleep(50);
                _unicastClient.Send(req, req.Length, ipTxEnd);
            }
        }

        /// <summary>
        /// Start Unicast and Multicast Listener
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            _unicastClient = new UdpClient(_unicastPort);
            _multicastClient = new UdpClient(_multicastPort);
            var ipSsdp = IPAddress.Parse(_multicastIp);
            _multicastClient.JoinMulticastGroup(ipSsdp);
            _running = true;
            UnicastSetBeginReceive();
            MulticastSetBeginReceive();
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region  Protected Methods

        protected void OnDeviceFound(SatIpDeviceFoundArgs args)
        {
            if (DeviceFound != null)
            {
                DeviceFound(this, args);
            }
        }

        protected void OnDeviceLost(SatIpDeviceLostArgs args)
        {
            if (DeviceLost != null)
            {
                DeviceLost(this, args);
            }
        }

        protected void OnDeviceNotify(SatIpDeviceNotifyArgs args)
        {
            if (DeviceNotify != null)
            {
                DeviceNotify(this, args);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _running = false;
                    _multicastClient.Close();
                    _unicastClient.Close();
                }
            }
            _disposed = true;
        }

        #endregion

        #region Properties

        public bool IsRunning()
        {
            return _running;
        }

        #endregion

        #region  Delegates

        public delegate void DeviceFoundHandler(object sender, SatIpDeviceFoundArgs e);

        public delegate void DeviceLostHandler(object sender, SatIpDeviceLostArgs e);

        public delegate void DeviceNotifyHandler(object sender, SatIpDeviceNotifyArgs e);

        #endregion

        #region Public Events

        public event DeviceFoundHandler DeviceFound;
        public event DeviceLostHandler DeviceLost;
        public event DeviceNotifyHandler DeviceNotify;
        private bool _disposed;

        #endregion
    }

    /// <summary>
    /// This class provides data for the <b>RTP_Session.PacketReceived</b> event.
    /// </summary>
    public class SatIpDeviceFoundArgs : EventArgs
    {
        //public SatIpDevice Device { get; private set; }

        //public SatIpDeviceFoundArgs(SatIpDevice device)
        //{
        //    Device = device;
        //}
    }

    /// <summary>
    /// 
    /// </summary>
    public class SatIpDeviceNotifyArgs : EventArgs
    {
        
        public String Uuid { get; private set; }

        public SatIpDeviceNotifyArgs(string uuid)
        {
            Uuid = uuid;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SatIpDeviceLostArgs : EventArgs
    {
        public String Uuid { get; private set; }

        public SatIpDeviceLostArgs(string uuid)
        {
            Uuid = uuid;
        }
    }
}

///// <summary>
    ///// Represents a Sat>Ip Server 
    ///// </summary>
    //public class SatIpDevice
    //{
    //    #region Properties

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string BaseHost { get; private set; }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// 
    //    public SatIpDeviceDescription Description { get; private set; }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<SatIpTuner> Tuners { get; private set; } 

    //    #endregion

    //    #region Static Methods
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="location"></param>
    //    /// <param name="description"></param>
    //    /// <returns></returns>
    //    public static SatIpDevice CreateDeviceByDescription(Uri location, SatIpDeviceDescription description)
    //    {
    //        var uri = location;

    //        var device = new SatIpDevice
    //        {

    //            BaseHost = uri.Host,
    //            Description = description,
    //            Tuners = CheckBroadcastSupport(location, description),
    //        };
    //        return device;

    //    }

    //    /// <summary>
    //    /// Recieves a List of SatIpTuners By Description
    //    /// </summary>
    //    /// <param name="description"></param>
    //    /// <returns></returns>
        //private static List<SatIpTuner> CheckBroadcastSupport(Uri location,SatIpDeviceDescription description)
        //{
        //    var serverAddress = location.Host;
        //    var retvalSatIpTuners= new List<SatIpTuner>();
        //    var dvbtTunerCount = 0;
        //    var dvbt2TunerCount = 0;
        //    var dvbcTunerCount = 0;
        //    var dvbc2TunerCount = 0;
        //    var dvbs2TunerCount = 0;
        //    if (!string.IsNullOrEmpty(description.Capabilities))
        //    {
        //        // If SatIp support's multible Broadcast Types as Exsample DVBS2-2,DVBT2-1,DVBC2-8
        //        // must the capabilities Property splitted by , Char so become we a Array with avaible Broadcasts and Tuner Counts
        //        // the result must be split with the - Char 
        //        if (description.Capabilities.Contains(','))
        //        {
        //            var capsections = description.Capabilities.Split(',');
        //            foreach (var capsection in capsections)
        //            {
        //                var info = capsection.Split('-');
        //                switch (info[0])
        //                {
        //                    case "DVBS":
        //                    case "DVBS2":
        //                        dvbs2TunerCount = int.Parse(info[1]);
        //                        break;
        //                    case "DVBT":
        //                        dvbtTunerCount = int.Parse(info[1]);
        //                        break;
        //                    case "DVBT2":
        //                        dvbt2TunerCount = int.Parse(info[1]);
        //                        break;
        //                    case "DVBC":
        //                        dvbcTunerCount = int.Parse(info[1]);
        //                        break;
        //                    case "DVBC2":
        //                        dvbc2TunerCount = int.Parse(info[1]);
        //                        break;
        //                }
        //            }
        //        }
        //        // the SatIp Server Supports only one Broadcast Type so let us check wich one it is and how many tuner it has
        //        // so must we only split the Capabilities Property with - Char
        //        else
        //        {
        //            var info = description.Capabilities.Split('-');
        //            switch (info[0])
        //            {
        //                case "DVBS":
        //                case "DVBS2":
        //                    dvbs2TunerCount = int.Parse(info[1]);
        //                    break;
        //                case "DVBT":
        //                    dvbtTunerCount = int.Parse(info[1]);
        //                    break;
        //                case "DVBT2":
        //                    dvbt2TunerCount = int.Parse(info[1]);
        //                    break;
        //                case "DVBC":
        //                    dvbcTunerCount = int.Parse(info[1]);
        //                    break;
        //                case "DVBC2":
        //                    dvbc2TunerCount = int.Parse(info[1]);
        //                    break;
        //            }
        //        }
               
        //    }
        //    // the Desciption.Capabilities Property is null or empty so can we check it over the Rtsp Describe 
        //    // but is not Rtsp Session avaible becomes you a RtspResponse with StatusCode 404
        //    // and read there the SDP Infos the count is stored in the SessionName (s) 
        //    // and the Broadcast Type is stored Media Attribute (a) 
        //    // alternativ add one Dummy SatIpTuner 
        //    else
        //    {
        //        RtspResponse response = null;
        //        var request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}/", serverAddress),1,0);
        //        request.Headers.Add("Accept", "application/sdp");
        //        request.Headers.Add("Connection", "close");
        //        var client = new RtspClient(serverAddress, 554);
        //        client.SendRequest(request, out response);
        //        if (response != null)
        //        {
        //            if (response.StatusCode.Equals(RtspStatusCode.Ok))
        //            {
        //                Match m = Regex.Match(response.Body, @"s=SatIPServer:1\s+([^\s]+)\s+",RegexOptions.Singleline | RegexOptions.IgnoreCase);
        //                if (m.Success)
        //                {
        //                    string frontEndInfo = m.Groups[1].Captures[0].Value;
        //                    string[] frontEndCounts = frontEndInfo.Split(',');
        //                    dvbs2TunerCount = int.Parse(frontEndCounts[0]);
        //                    if (frontEndCounts.Length >= 2)
        //                    {
        //                        dvbtTunerCount = int.Parse(frontEndCounts[1]);
        //                        if (frontEndCounts.Length > 2)
        //                        {
        //                            dvbcTunerCount = int.Parse(frontEndCounts[2]);
        //                            if (frontEndCounts.Length > 3)
        //                            {

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else if (response.StatusCode.Equals(RtspStatusCode.NotFound))
        //            {
        //                // the Sat>Ip server has no active Stream 
        //            }
                
        //            else
        //            {

        //            }
        //        }
        //        if (dvbcTunerCount == 0 && dvbc2TunerCount == 0 && dvbtTunerCount == 0 && dvbt2TunerCount == 0 &&  dvbs2TunerCount==0)
        //        {
        //            dvbs2TunerCount= 1;
        //        }
        //        var i = 1;
        //        var j = 0;
        //        for (; i <= dvbcTunerCount; i++)
        //        {
        //            retvalSatIpTuners.Add(new SatIpCableTuner(description, i));
        //        }
        //        j += dvbcTunerCount;
        //        for (; i <= dvbc2TunerCount + j; i++)
        //        {
        //            retvalSatIpTuners.Add(new SatIpCableTuner(description, i));
        //        }
        //        j += dvbc2TunerCount;

        //        // Currently the Digital Devices Octopus Net is the only SAT>IP product
        //        // to support DVB-T/T2. The DVB-T/T2 tuners also support DVB-C/C2. In
        //        // general we'll assume that if the DVB-C/C2 and DVB-T/T2 counts are
        //        // equal the tuners are hybrid.
        //        if (dvbcTunerCount + dvbc2TunerCount > 0 && (dvbcTunerCount + dvbc2TunerCount) == (dvbtTunerCount + dvbt2TunerCount))
        //        {
        //            i = 1;
        //            j = 0;
        //        }

        //        for (; i <= dvbtTunerCount + j; i++)
        //        {
        //            retvalSatIpTuners.Add(new SatIpTerrestrialTuner(description, i));
        //        }
        //        j += dvbtTunerCount;
        //        ////for (; i <= dvbt2TunerCount + j; i++)
        //        {
        //            retvalSatIpTuners.Add(new SatIpTerrestrialTuner(description, i));
        //        }
        //        j += dvbs2TunerCount;

        //        for (; i <= dvbs2TunerCount + j; i++)
        //        {
        //            retvalSatIpTuners.Add(new SatIpSatelliteTuner(description, i));
        //        }



        //    }
        //    return retvalSatIpTuners;
        //} 
    //    #endregion

    //    /// <summary>
    //    /// StopAll is for the reason if the Sat>Ip server is going Lost so should every Tuner Rtsp, Rtp, Rtcp Communication stopped!
    //    /// </summary>
    //    public void StopAll()
    //    {
    //        foreach (SatIpTuner tuner in Tuners)
    //        {
    //            tuner.Stop("ssdp:byebye");
    //        }
    //    }
    //}

    ///// <summary>
    ///// Represent the Sat>Ip Server Description
    ///// </summary>
    //public class SatIpDeviceDescription
    //{
    //    public string DeviceType { get; private set; }
    //    public string FriendlyName { get; private set; }
    //    public string Manufacturer { get; private set; }
    //    public string ManufacturerUrl { get; private set; }
    //    public string ModelDescription { get; private set; }
    //    public string ModelName { get; private set; }
    //    public string ModelNumber { get; private set; }
    //    public string ModelUrl { get; private set; }
    //    public string SerialNumber { get; private set; }
    //    public string UniqueDeviceName { get; private set; }
    //    public string PresentationUrl { get; private set; }
    //    public string Capabilities { get; private set; }

    //    public static SatIpDeviceDescription ProcessDeviceDescription(Uri location)
    //    {
    //        var description = new SatIpDeviceDescription();
    //        var document = XDocument.Load(location.AbsoluteUri);
    //        var xnm = new XmlNamespaceManager(new NameTable());
    //        XNamespace n0 = "urn:schemas-upnp-org:device-1-0";
    //        XNamespace n1 = "urn:ses-com:satip";
    //        xnm.AddNamespace("root", n0.NamespaceName);
    //        xnm.AddNamespace("satip", n1.NamespaceName);
    //        if (document.Root != null)
    //        {
    //            var deviceElement = document.Root.Element(n0 + "device");
    //            if (deviceElement != null)
    //            {
    //                var devicetypeElement = deviceElement.Element(n0 + "deviceType");
    //                if (devicetypeElement != null)
    //                    description.DeviceType = devicetypeElement.Value;
    //                var friendlynameElement = deviceElement.Element(n0 + "friendlyName");
    //                if (friendlynameElement != null)
    //                    description.FriendlyName = friendlynameElement.Value;
    //                var manufactureElement = deviceElement.Element(n0 + "manufacturer");
    //                if (manufactureElement != null)
    //                    description.Manufacturer = manufactureElement.Value;
    //                var manufactureurlElement = deviceElement.Element(n0 + "manufacturerURL");
    //                if (manufactureurlElement != null)
    //                    description.ManufacturerUrl = manufactureurlElement.Value;
    //                var modeldescriptionElement = deviceElement.Element(n0 + "modelDescription");
    //                if (modeldescriptionElement != null)
    //                    description.ModelDescription = modeldescriptionElement.Value;
    //                var modelnameElement = deviceElement.Element(n0 + "modelName");
    //                if (modelnameElement != null)
    //                    description.ModelName = modelnameElement.Value;
    //                var modelnumberElement = deviceElement.Element(n0 + "modelNumber");
    //                if (modelnumberElement != null)
    //                    description.ModelNumber = modelnumberElement.Value;
    //                var modelurlElement = deviceElement.Element(n0 + "modelURL");
    //                if (modelurlElement != null)
    //                    description.ModelUrl = modelurlElement.Value;
    //                var serialnumberElement = deviceElement.Element(n0 + "serialNumber");
    //                if (serialnumberElement != null)
    //                    description.SerialNumber = serialnumberElement.Value;
    //                var uniquedevicenameElement = deviceElement.Element(n0 + "UDN");
    //                if (uniquedevicenameElement != null)
    //                    description.UniqueDeviceName = uniquedevicenameElement.Value;
    //                var iconList = deviceElement.Element(n0 + "iconList");
    //                //if (iconList != null)
    //                //{
    //                //    var icons = from query in iconList.Descendants(n0 + "icon")
    //                //        select new UpnpIcon
    //                //        {
    //                //            MimeType = (string) query.Element(n0 + "mimetype"),
    //                //            Url = (string) query.Element(n0 + "url"),
    //                //            Height = (int) query.Element(n0 + "height"),
    //                //            Width = (int) query.Element(n0 + "width"),
    //                //            Depth = (int) query.Element(n0 + "depth"),
    //                //        };
    //                //    _iconList = icons.ToArray();
    //                //}
    //                var presentationElement = deviceElement.Element(n0 + "presentationURL");
    //                if (presentationElement != null)
    //                    description.PresentationUrl = presentationElement.Value;
    //                var capabilitiesElement = deviceElement.Element(n1 + "X_SATIPCAP");
    //                if (capabilitiesElement != null)
    //                    description.Capabilities = capabilitiesElement.Value;
    //            }
    //        }
    //        return description;
    //    }
    //}
    ///// <summary>
    ///// Represents a abstract Sat>Ip Tuner Class.
    ///// </summary>
    //public abstract class SatIpTuner
    //{
    //    private SatIpDeviceDescription _description;
    //    private int _tunerId;

    //    protected SatIpTuner(SatIpDeviceDescription description, int tunerId)
    //    {
    //        // TODO: Complete member initialization
    //        _description = description;
    //        _tunerId = tunerId;
            
    //    }
    //    public abstract string Type { get; }
    //    public abstract void Tune();
    //    public abstract void Stop(string reason);
        
    //}

    ///// <summary>
    ///// Represents a Sat>Ip Satellite Tuner derivative from abstract <see cref="SatIpTuner"/> Class.
    ///// </summary>
    //public class SatIpSatelliteTuner : SatIpTuner
    //{
    //    public override string Type
    //    {
    //        get { return "DVBS"; }
    //    }
    //    public SatIpSatelliteTuner(SatIpDeviceDescription description, int tunerId)
    //        : base(description, tunerId)
    //    { }

    //    public override void Tune()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Stop(string reason)
    //    {
    //        if (reason.Equals("ssdp:byebye"))
    //        {

    //        }
    //    }
    //}
    ///// <summary>
    ///// Represents a Sat>Ip Terrestrial Tuner derivative from abstract <see cref="SatIpTuner"/> Class.
    ///// </summary>
    //public class SatIpTerrestrialTuner : SatIpTuner
    //{
    //    public override string Type
    //    {
    //        get { return "DVBT"; }
    //    }

    //    public override void Tune()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Stop(string reason)
    //    {
    //        if (reason.Equals("ssdp:byebye"))
    //        {

    //        }
    //    }

    //    public SatIpTerrestrialTuner(SatIpDeviceDescription description, int tunerId)
    //        : base(description, tunerId)
    //    {
    //    }
    //}
    ///// <summary>
    ///// Represents a Sat>Ip Cable Tuner derivative from abstract <see cref="SatIpTuner"/> Class.
    ///// </summary>
    //public class SatIpCableTuner : SatIpTuner
    //{
    //    public override string Type
    //    {
    //        get { return "DVBC"; }
    //    }

    //    public override void Tune()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Stop(string reason)
    //    {
    //        if (reason.Equals("ssdp:byebye"))
    //        {

    //        }
    //    }

    //    public SatIpCableTuner(SatIpDeviceDescription description, int tunerId)
    //        : base(description, tunerId)
    //    {
    //    }
    //}
  

