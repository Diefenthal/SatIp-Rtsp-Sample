using SatIp.RtspSample.Rtsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using UPNPLib;

namespace SatIp.RtspSample.SatIp
{
    public class SatIpDevice
    {
        private string _baseUrl;        
        private string _friendlyName;
        private string _uniqueDeviceName;
        private string _description;
        private string _capabilities;
        private bool _hasSatelliteBroadcastSupport;        
        private bool _hasCableBroadcastSupport;        
        private bool _hasTerrestrialBroadcastSupport;
        
        public SatIpDevice(UPnPDevice device)
        {
            var descriptionUrl = new Uri(((IUPnPDeviceDocumentAccess)device).GetDocumentURL());
            ReadDescription(descriptionUrl);
            _baseUrl = descriptionUrl.Host;            
            _friendlyName = device.FriendlyName;
            _uniqueDeviceName = device.UniqueDeviceName;
        }

        public string BaseUrl
        {
            get { return _baseUrl; }
            set { _baseUrl = value; }
        }

        public string FriendlyName
        {
            get { return _friendlyName; }
            set { _friendlyName = value; }
        }

        public string UniqueDeviceName
        {
            get { return _uniqueDeviceName; }
            set { _uniqueDeviceName = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Capabilities
        {
            get { return _capabilities; }
            set { _capabilities = value; }
        }

        public bool HasSatelliteBroadcastSupport
        {
            get { return _hasSatelliteBroadcastSupport; }
            set { _hasSatelliteBroadcastSupport = value; }
        }
        public bool HasCableBroadcastSupport
        {
            get { return _hasCableBroadcastSupport; }
            set { _hasCableBroadcastSupport = value; }
        }
        public bool HasTerrestrialBroadcastSupport
        {
            get { return _hasTerrestrialBroadcastSupport; }
            set { _hasTerrestrialBroadcastSupport = value; }
        }   

        private void ReadDescription(Uri locationUri)
        {
            var document = XDocument.Load(locationUri.AbsoluteUri);
            var xnm= new XmlNamespaceManager(new NameTable());
            XNamespace n1 = "urn:ses-com:satip";
            XNamespace n0 = "urn:schemas-upnp-org:device-1-0";
            xnm.AddNamespace("root", n0.NamespaceName);
            xnm.AddNamespace("satip:",n1.NamespaceName);
            if (document.Root != null)
            {
                var deviceElement = document.Root.Element(n0 + "device");
                _description = document.Declaration + document.ToString();
                if (deviceElement != null)
                {
                    var capabilitiesElement = deviceElement.Element(n1 + "X_SATIPCAP");
                    if (capabilitiesElement != null)
                    { _capabilities = capabilitiesElement.Value; CheckCapabilities(_capabilities); }
                       

                    //var m3uElement = deviceElement.Element(n1 + "X_SATIPM3U");
                    //if (m3uElement != null) _m3u = m3uElement.Value;

                }

            }
        }
        private void CheckCapabilities(string capabilities)
        {
            var dvbtTunerCount = 0;
            var dvbt2TunerCount = 0;
            var dvbcTunerCount = 0;
            var dvbc2TunerCount = 0;
            var dvbs2TunerCount = 0;
            if (!string.IsNullOrEmpty(capabilities))
            {
                // If SatIp support's multible Broadcast Types as Exsample DVBS2-2,DVBT2-1,DVBC2-8
                // must the capabilities Property splitted by , Char so become we a Array with avaible Broadcasts and Tuner Counts
                // the result must be split with the - Char 
                if (capabilities.Contains(','))
                {
                    var capsections = capabilities.Split(',');
                    foreach (var capsection in capsections)
                    {
                        var info = capsection.Split('-');
                        switch (info[0])
                        {
                            case "DVBS":
                            case "DVBS2":
                                dvbs2TunerCount = int.Parse(info[1]);                                
                                break;
                            case "DVBT":
                                dvbtTunerCount = int.Parse(info[1]);                                
                                break;
                            case "DVBT2":
                                dvbt2TunerCount = int.Parse(info[1]);                                
                                break;
                            case "DVBC":
                                dvbcTunerCount = int.Parse(info[1]);
                                break;
                            case "DVBC2":
                                dvbc2TunerCount = int.Parse(info[1]);
                                break;
                        }
                    }
                }
                // the SatIp Server Supports only one Broadcast Type so let us check wich one it is and how many tuner it has
                // so must we only split the Capabilities Property with - Char
                else
                {
                    var info = capabilities.Split('-');
                    switch (info[0])
                    {
                        case "DVBS":
                        case "DVBS2":
                            dvbs2TunerCount = int.Parse(info[1]);                            
                            break;
                        case "DVBT":
                            dvbtTunerCount = int.Parse(info[1]);                            
                            break;
                        case "DVBT2":
                            dvbt2TunerCount = int.Parse(info[1]);                            
                            break;
                        case "DVBC":
                            dvbcTunerCount = int.Parse(info[1]);
                            break;
                        case "DVBC2":
                            dvbc2TunerCount = int.Parse(info[1]);
                            break;
                    }
                }

            }
            // the Desciption.Capabilities Property is null or empty so can we check it over the Rtsp Describe 
            // but is not Rtsp Session avaible becomes you a RtspResponse with StatusCode 404
            // and read there the SDP Infos the count is stored in the SessionName (s) 
            // and the Broadcast Type is stored Media Attribute (a) 
            // alternativ add one Dummy SatIpTuner 
            else
            {
                RtspResponse response = null;
                var request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}/", _baseUrl), 1, 0);
                request.Headers.Add("Accept", "application/sdp");
                request.Headers.Add("Connection", "close");
                var client = new RtspClient(_baseUrl);
                client.SendRequest(request, out response);
                if (response != null)
                {
                    if (response.StatusCode.Equals(RtspStatusCode.Ok))
                    {
                        Match m = Regex.Match(response.Body, @"s=SatIPServer:1\s+([^\s]+)\s+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            string frontEndInfo = m.Groups[1].Captures[0].Value;
                            string[] frontEndCounts = frontEndInfo.Split(',');
                            dvbs2TunerCount = int.Parse(frontEndCounts[0]);
                            if (frontEndCounts.Length >= 2)
                            {
                                dvbtTunerCount = int.Parse(frontEndCounts[1]);
                                if (frontEndCounts.Length > 2)
                                {
                                    dvbcTunerCount = int.Parse(frontEndCounts[2]);
                                    if (frontEndCounts.Length > 3)
                                    {

                                    }
                                }
                            }
                        }
                    }
                    else if (response.StatusCode.Equals(RtspStatusCode.NotFound))
                    {
                        // the Sat>Ip server has no active Stream 
                    }

                    else
                    {

                    }
                }
                if (dvbcTunerCount == 0 && dvbc2TunerCount == 0 && dvbtTunerCount == 0 && dvbt2TunerCount == 0 && dvbs2TunerCount == 0)
                {
                    dvbs2TunerCount = 1;
                    _hasSatelliteBroadcastSupport = true;
                    _hasTerrestrialBroadcastSupport = false;
                    _hasCableBroadcastSupport = false;
                }
                var i = 1;
                var j = 0;
                for (; i <= dvbcTunerCount; i++)
                {
                    _hasCableBroadcastSupport = true;
                    //retvalSatIpTuners.Add(new SatIpCableTuner(description, i));
                }
                j += dvbcTunerCount;
                for (; i <= dvbc2TunerCount + j; i++)
                {
                    _hasCableBroadcastSupport = true;
                    //retvalSatIpTuners.Add(new SatIpCableTuner(description, i));
                }
                j += dvbc2TunerCount;

                // Currently the Digital Devices Octopus Net is the only SAT>IP product
                // to support DVB-T/T2. The DVB-T/T2 tuners also support DVB-C/C2. In
                // general we'll assume that if the DVB-C/C2 and DVB-T/T2 counts are
                // equal the tuners are hybrid.
                if (dvbcTunerCount + dvbc2TunerCount > 0 && (dvbcTunerCount + dvbc2TunerCount) == (dvbtTunerCount + dvbt2TunerCount))
                {
                    i = 1;
                    j = 0;
                }

                for (; i <= dvbtTunerCount + j; i++)
                {
                    _hasTerrestrialBroadcastSupport = true;
                    //retvalSatIpTuners.Add(new SatIpTerrestrialTuner(description, i));
                }
                j += dvbtTunerCount;
                ////for (; i <= dvbt2TunerCount + j; i++)
                {
                    _hasTerrestrialBroadcastSupport = true;
                    //retvalSatIpTuners.Add(new SatIpTerrestrialTuner(description, i));
                }
                j += dvbs2TunerCount;

                for (; i <= dvbs2TunerCount + j; i++)
                {
                    _hasSatelliteBroadcastSupport = true;
                    //retvalSatIpTuners.Add(new SatIpSatelliteTuner(description, i));
                }



            }
            //return retvalSatIpTuners;
        }
    }
}
