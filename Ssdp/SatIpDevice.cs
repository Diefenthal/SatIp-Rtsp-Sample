/*  
    Copyright (C) <2007-2017>  <Kay Diefenthal>

    SatIp.DiscoverySample is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    SatIp.DiscoverySample is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with SatIp.DiscoverySample.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SatIp.RtspSample.Logging;

namespace SatIp.RtspSample.Ssdp
{
    public class SatIpDevice
    {
        #region Private Fields
        private Uri _baseUrl;
        private string _deviceType = "";
        private string _friendlyName = "";
        private string _manufacturer = "";
        private string _manufacturerUrl = "";
        private string _modelDescription = "";
        private string _modelName = "";
        private string _modelNumber = "";
        private string _modelUrl = "";
        private string _serialNumber = "";
        private string _uniqueDeviceName = "";
        private string _presentationUrl = "";
        private string _deviceDescription;
        private SatIpDeviceIcon[] _iconList = new SatIpDeviceIcon[4];
        private string _capabilities = "";
        private string _m3u = "";
        private bool _supportsDVBC;
        private bool _supportsDVBS;
        private bool _supportsDVBT; 
        #endregion
        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="url">Device URL.</param>
        internal SatIpDevice(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            Init(new Uri(url));
        } 
        #endregion
        #region Method

        public string GetImage(int index)
        {
            var icon = (SatIpDeviceIcon)_iconList.GetValue(index);
            return icon.Url;
        }

        private void Init(Uri locationUri)
        {
            try
            {
                Logger.Info("the Description Url is {0}", locationUri);
                BaseUrl = locationUri;
                var document = XDocument.Load(locationUri.AbsoluteUri);
                var xnm = new XmlNamespaceManager(new NameTable());
                XNamespace n1 = "urn:ses-com:satip";
                XNamespace n0 = "urn:schemas-upnp-org:device-1-0";
                xnm.AddNamespace("root", n0.NamespaceName);
                xnm.AddNamespace("satip:", n1.NamespaceName);
                if (document.Root != null)
                {
                    var deviceElement = document.Root.Element(n0 + "device");

                    _deviceDescription = document.Declaration + document.ToString();
                    Logger.Info("The Description has this Content \r\n{0}", _deviceDescription);
                    if (deviceElement != null)
                    {
                        var devicetypeElement = deviceElement.Element(n0 + "deviceType");
                        if (devicetypeElement != null)
                            _deviceType = devicetypeElement.Value;
                        var friendlynameElement = deviceElement.Element(n0 + "friendlyName");
                        if (friendlynameElement != null)
                            _friendlyName = friendlynameElement.Value;
                        var manufactureElement = deviceElement.Element(n0 + "manufacturer");
                        if (manufactureElement != null)
                            _manufacturer = manufactureElement.Value;
                        var manufactureurlElement = deviceElement.Element(n0 + "manufacturerURL");
                        if (manufactureurlElement != null)
                            _manufacturerUrl = manufactureurlElement.Value;
                        var modeldescriptionElement = deviceElement.Element(n0 + "modelDescription");
                        if (modeldescriptionElement != null)
                            _modelDescription = modeldescriptionElement.Value;
                        var modelnameElement = deviceElement.Element(n0 + "modelName");
                        if (modelnameElement != null)
                            _modelName = modelnameElement.Value;
                        var modelnumberElement = deviceElement.Element(n0 + "modelNumber");
                        if (modelnumberElement != null)
                            _modelNumber = modelnumberElement.Value;
                        var modelurlElement = deviceElement.Element(n0 + "modelURL");
                        if (modelurlElement != null)
                            _modelUrl = modelurlElement.Value;
                        var serialnumberElement = deviceElement.Element(n0 + "serialNumber");
                        if (serialnumberElement != null)
                            _serialNumber = serialnumberElement.Value;
                        var uniquedevicenameElement = deviceElement.Element(n0 + "UDN");
                        if (uniquedevicenameElement != null) _uniqueDeviceName = uniquedevicenameElement.Value;
                        var iconList = deviceElement.Element(n0 + "iconList");
                        if (iconList != null)
                        {
                            var icons = from query in iconList.Descendants(n0 + "icon")
                                        select new SatIpDeviceIcon
                                        {
                                            // Needed to change mimeType to mimetype. XML is case sensitive 
                                            MimeType = (string)query.Element(n0 + "mimetype"),
                                            Url = (string)query.Element(n0 + "url"),
                                            Height = (int)query.Element(n0 + "height"),
                                            Width = (int)query.Element(n0 + "width"),
                                            Depth = (int)query.Element(n0 + "depth"),
                                        };

                            _iconList = icons.ToArray();
                        }

                        var presentationUrlElement = deviceElement.Element(n0 + "presentationURL");
                        if (presentationUrlElement != null)
                        {
                            if (presentationUrlElement.Value.StartsWith("Http;//"))
                                _presentationUrl = presentationUrlElement.Value;
                            _presentationUrl = locationUri.Scheme + "://" + locationUri.Host;
                        }
                        if (presentationUrlElement == null)
                        {
                            _presentationUrl = locationUri.Scheme + "://" + locationUri.Host;
                        }
                        var capabilitiesElement = deviceElement.Element(n1 + "X_SATIPCAP");
                        if (capabilitiesElement != null)
                        {
                            _capabilities = capabilitiesElement.Value;
                            if (capabilitiesElement.Value.Contains(','))
                            {
                                string[] capabilities = capabilitiesElement.Value.Split(',');
                                foreach (var capability in capabilities)
                                {
                                    ReadCapability(capability);
                                }
                            }
                            else
                            {
                                ReadCapability(capabilitiesElement.Value);
                            }
                        }
                        else
                        {
                            _supportsDVBS = true;
                            //ToDo Create only one Digital Recorder / Capture Instance here
                        }

                        var m3uElement = deviceElement.Element(n1 + "X_SATIPM3U");
                        if (m3uElement != null) _m3u = locationUri.Scheme + "://" + locationUri.Host + ":" + locationUri.Port + m3uElement.Value;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error("It give a Problem with the Description {0}", exception);
            }
        }
        private void ReadCapability(string capability)
        {

            string[] cap = capability.Split('-');
            switch (cap[0].ToLower())
            {
                case "dvbs":
                case "dvbs2":
                    {
                        // Optional that you know what an device Supports can you add an flag 
                        _supportsDVBS = true;

                        for (int i = 0; i < int.Parse(cap[1]); i++)
                        {
                            //ToDo Create Digital Recorder / Capture Instance here
                        }

                        break;
                    }
                case "dvbc":
                case "dvbc2":
                    {
                        // Optional that you know what an device Supports can you add an flag 
                        _supportsDVBC = true;

                        for (int i = 0; i < int.Parse(cap[1]); i++)
                        {
                            //ToDo Create Digital Recorder / Capture Instance here
                        }

                        break;
                    }
                case "dvbt":
                case "dvbt2":
                    {
                        // Optional that you know what an device Supports can you add an flag 
                        _supportsDVBT = true;


                        for (int i = 0; i < int.Parse(cap[1]); i++)
                        {
                            //ToDo Create Digital Recorder / Capture Instance here

                        }

                        break;
                    }
            }

        }
        #endregion
        #region Proeprties implementation

        /// <summary>
        /// Gets device type.
        /// </summary>
        public string DeviceType
        {
            get { return _deviceType; }
        }

        /// <summary>
        /// Gets device short name.
        /// </summary>
        public string FriendlyName
        {
            get { return _friendlyName; }
        }

        /// <summary>
        /// Gets manufacturer's name.
        /// </summary>
        public string Manufacturer
        {
            get { return _manufacturer; }
        }

        /// <summary>
        /// Gets web site for Manufacturer.
        /// </summary>
        public string ManufacturerUrl
        {
            get { return _manufacturerUrl; }
        }

        /// <summary>
        /// Gets device long description.
        /// </summary>
        public string ModelDescription
        {
            get { return _modelDescription; }
        }

        /// <summary>
        /// Gets model name.
        /// </summary>
        public string ModelName
        {
            get { return _modelName; }
        }

        /// <summary>
        /// Gets model number.
        /// </summary>
        public string ModelNumber
        {
            get { return _modelNumber; }
        }

        /// <summary>
        /// Gets web site for model.
        /// </summary>
        public string ModelUrl
        {
            get { return _modelUrl; }
        }

        /// <summary>
        /// Gets serial number.
        /// </summary>
        public string SerialNumber
        {
            get { return _serialNumber; }
        }

        /// <summary>
        /// Gets unique device name.
        /// </summary>
        public string UniqueDeviceName
        {
            get { return _uniqueDeviceName; }
        }

        /// <summary>
        /// Gets device UI url.
        /// </summary>
        public string PresentationUrl
        {
            get { return _presentationUrl; }
        }

        /// <summary>
        /// Gets UPnP device XML description.
        /// </summary>
        public string DeviceDescription
        {
            get { return _deviceDescription; }
        }

        public Uri BaseUrl
        {
            get { return _baseUrl; }
            set { _baseUrl = value; }
        }

        public string M3U
        {
            get { return _m3u; }
            set { _m3u = value; }
        }

        public string Capabilities
        {
            get { return _capabilities; }
            set { _capabilities = value; }
        }

        public bool SupportsDVBC
        {
            get { return _supportsDVBC; }
            set { _supportsDVBC = value; }
        }

        public bool SupportsDVBS
        {
            get { return _supportsDVBS; }
            set { _supportsDVBS = value; }
        }

        public bool SupportsDVBT
        {
            get { return _supportsDVBT; }
            set { _supportsDVBT = value; }
        }

        #endregion        
    }
    public class SatIpDeviceIcon
    {
        public SatIpDeviceIcon()
        {
            Url = "";
            MimeType = "";
        }

        public int Depth { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string MimeType { get; set; }
        public string Url { get; set; }
    }
}
