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
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using SatIp.RtspSample.Rtsp;

namespace SatIp.RtspSample.Upnp
{
    public class UpnpDevice
    {
        private UpnpIcon[] _iconList = new UpnpIcon[4];

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="url">Device URL.</param>
        internal UpnpDevice(string url)
        {
            Frontends = "";
            BaseHost = "";
            PresentationUrl = "";
            Udn = "";
            SerialNumber = "";
            ModelUrl = "";
            ModelNumber = "";
            ModelName = "";
            ModelDescription = "";
            ManufacturerUrl = "";
            Manufacturer = "";
            FriendlyName = "";
            DeviceType = "";
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            Init(url);
        }

        #region method Init

        public string GetImage(string mimetype,int width,int height,int depth)
        {
            UpnpIcon result= null;
            foreach (UpnpIcon icon in _iconList)
            {
                if ((icon.MimeType.Equals(mimetype)) && (icon.Width.Equals(width)) && (icon.Height.Equals(height)) &&
                    (icon.Depth.Equals(depth)))
                {
                    result = icon;
                }
            }
            if(result.Url.StartsWith("HTTP://"))
            {
                return result.Url;
            }
            else
            {
                return string.Format("http://{0}:{1}{2}", BaseHost, BasePort, result.Url);
            }
        }

        private void Init(string url)
        {
            var document = XDocument.Load(url);
            var xnm= new XmlNamespaceManager(new NameTable());
            XNamespace n1 = "urn:ses-com:satip";
            XNamespace n0 = "urn:schemas-upnp-org:device-1-0";
            xnm.AddNamespace("root", n0.NamespaceName);
            xnm.AddNamespace("satip",n1.NamespaceName);
            if (document.Root != null)
            {
                var deviceElement = document.Root.Element(n0 + "device");
                var addressline = Regex.Split(url, @"://+");
                var address = addressline[1].Split(':');
                BaseHost = address[0];
                var port = address[1].Split('/');
                BasePort = port[0];
                DeviceDescription = document.Declaration+document.ToString();
                if (deviceElement != null)
                {
                    var devicetypeElement = deviceElement.Element(n0+"deviceType");
                    if (devicetypeElement != null)
                        DeviceType = devicetypeElement.Value;
                    var friendlynameElement = deviceElement.Element(n0+"friendlyName");
                    if (friendlynameElement != null)
                        FriendlyName = friendlynameElement.Value;
                    var manufactureElement = deviceElement.Element(n0+"manufacturer");
                    if (manufactureElement != null)
                        Manufacturer = manufactureElement.Value;
                    var manufactureurlElement = deviceElement.Element(n0 + "manufacturerURL");
                    if (manufactureurlElement != null)
                        ManufacturerUrl = manufactureurlElement.Value;
                    var modeldescriptionElement = deviceElement.Element(n0 + "modelDescription");
                    if (modeldescriptionElement != null)
                        ModelDescription = modeldescriptionElement.Value;
                    var modelnameElement = deviceElement.Element(n0 + "modelName");
                    if (modelnameElement != null)
                        ModelName = modelnameElement.Value;
                    var modelnumberElement = deviceElement.Element(n0 + "modelNumber");
                    if (modelnumberElement != null)
                        ModelNumber = modelnumberElement.Value;
                    var modelurlElement = deviceElement.Element(n0 + "modelURL");
                    if (modelurlElement != null)
                        ModelUrl = modelurlElement.Value;
                    var serialnumberElement = deviceElement.Element(n0 + "serialNumber");
                    if (serialnumberElement != null)
                        SerialNumber = serialnumberElement.Value;
                    var uniquedevicenameElement = deviceElement.Element(n0 + "UDN");
                    if (uniquedevicenameElement != null) Udn = uniquedevicenameElement.Value;
                    var iconList = deviceElement.Element(n0 + "iconList");
                    if (iconList != null)
                    {
                        var icons = from query in iconList.Descendants(n0 + "icon")
                            select new UpnpIcon
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
                    var presentationurlElement  = deviceElement.Element(n0 + "presentationURL");
                    if (presentationurlElement != null)
                    	PresentationUrl = presentationurlElement.Value;
                    var satipcapElement = deviceElement.Element(n1 + "X_SATIPCAP");
                    if (satipcapElement != null)
                    	Frontends = satipcapElement.Value;
                    else
                    {
                        RtspResponse response;
                        var client = new RtspClient(BaseHost);
                        var request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}:{1}/", BaseHost, 554), 1, 0);
                        request.Headers.Add("Accept", "application/sdp");
                        client.SendRequest(request, out response);
                        if(response != null)
                        {
                            if (response.StatusCode == RtspStatusCode.Ok)
                            {
                                Match m = Regex.Match(response.Body, @"s=SatIPServer:1\s+([^\s]+)\s+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                                Frontends = m.Success ? m.Groups[1].Captures[0].Value : "";
                            }
                        }
                    }
                }
            }
        }

        //public XDocument Serialise()
        //{
        //    XNamespace n0 = "urn:schemas-upnp-org:device-1-0";
        //    XNamespace n1 = "urn:ses-com:satip";

        //    var satipXNamespace = XNamespace.Get("urn:ses-com:satip");
        //    var description = new XDocument
        //    (
        //        new XElement(n0 + "root",
        //            new XAttribute("configId", "1"),
        //            new XElement(n0 + "specVersion", new XElement(n0 + "major", "1"), new XElement(n0 + "minor", "0")),
        //            new XElement(n0 + "URLBase", BaseHost),
        //            new XElement(n0 + "device",
        //                new XElement(n0 + "deviceType", DeviceType),
        //                new XElement(n0 + "friendlyName", FriendlyName),
        //                new XElement(n0 + "manufacturer", Manufacturer),
        //                new XElement(n0 + "manufacturerURL", ManufacturerUrl),
        //                new XElement(n0 + "modelDescription", ModelDescription),
        //                new XElement(n0 + "modelName", ModelName),
        //                new XElement(n0 + "modelNumber", ModelNumber),
        //                new XElement(n0 + "modelURL", ModelUrl),
        //                new XElement(n0 + "serialNumber", SerialNumber),
        //                new XElement(n0 + "UDN", string.Format("uuid:{0}", Udn)),
                        
        //    foreach (UpnpIcon icon in _iconList)
        //    {
        //       new XElement(n0 + "iconList", new XElement(n0 + "icon", new XElement(n0 + "mimetype", icon.MimeType), new XElement(n0 + "width", icon.Width),
        //                        new XElement(n0 + "height", icon.Height), new XElement(n0 + "depth", icon.Depth), new XElement(n0 + "url", icon.Url))),
        //    }
                            
        //                    //new XElement(n0 + "icon", new XElement(n0 + "mimetype", "image/png"), new XElement(n0 + "width", "48"),
        //                    //    new XElement(n0 + "height", "48"), new XElement(n0 + "depth", "24"), new XElement(n0 + "url", "s")),
        //                    //new XElement(n0 + "icon", new XElement(n0 + "mimetype", "image/jpg"), new XElement(n0 + "width", "120"),
        //                    //    new XElement(n0 + "height", "120"), new XElement(n0 + "depth", "24"), new XElement(n0 + "url", "d")),
        //                    //new XElement(n0 + "icon", new XElement(n0 + "mimetype", "image/jpg"), new XElement(n0 + "width", "48"),
        //                    //    new XElement(n0 + "height", "48"), new XElement(n0 + "depth", "24"), new XElement(n0 + "url", "f"))),
        //                new XElement(n0 + "presentationURL", PresentationUrl),
        //                new XElement(n1 + "X_SATIPCAP", Frontends, new XAttribute(XNamespace.Xmlns + "satip", satipXNamespace)))
        //    ));
        //    description.Declaration = new XDeclaration("1.0", "utf-8", "true");
        //    return description;
        //}


        #endregion

        #region Proeprties implementation

        /// <summary>
        /// Gets device type.
        /// </summary>
        public string DeviceType { get; private set; }

        /// <summary>
        /// Gets device short name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets manufacturer's name.
        /// </summary>
        public string Manufacturer { get; private set; }

        /// <summary>
        /// Gets web site for Manufacturer.
        /// </summary>
        public string ManufacturerUrl { get; private set; }

        /// <summary>
        /// Gets device long description.
        /// </summary>
        public string ModelDescription { get; private set; }

        /// <summary>
        /// Gets model name.
        /// </summary>
        public string ModelName { get; private set; }

        /// <summary>
        /// Gets model number.
        /// </summary>
        public string ModelNumber { get; private set; }

        /// <summary>
        /// Gets web site for model.
        /// </summary>
        public string ModelUrl { get; private set; }

        /// <summary>
        /// Gets serial number.
        /// </summary>
        public string SerialNumber { get; private set; }

        /// <summary>
        /// Gets unique device name.
        /// </summary>
        public string Udn { get; private set; }

        // iconList
        // serviceList
        // deviceList

        /// <summary>
        /// Gets device UI url.
        /// </summary>
        public string PresentationUrl { get; private set; }

        /// <summary>
        /// Gets UPnP device XML description.
        /// </summary>
        public string DeviceDescription { get; private set; }

        public string BaseHost { get; set; }

        public string BasePort { get; set; }

        public string Frontends { get; set; }

        #endregion

        public string Name
        {
            get { return string.Format("{0}  -  {1}", FriendlyName, Udn); }
        }

    }
}
