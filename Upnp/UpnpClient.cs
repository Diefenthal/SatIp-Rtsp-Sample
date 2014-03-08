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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SatIp.RtspSample.Upnp
{
    public class UpnpClient
    {
        #region method Search

        /// <summary>
        /// Searches the network for UPnP Sat>Ip Server devices.
        /// </summary>
        /// <param name="timeout">Search wait timeout in milliseconds.</param>
        /// <returns>Returns matched UPnP devices.</returns>
        public UpnpDevice[] Search(int timeout)
        {
            return Search("urn:ses-com:device:SatIPServer:1", timeout);
        }
                
        /// <summary>
        /// Searches the network for UPnP Sat>Ip Server devices.
        /// </summary>
        /// <param name="deviceType">UPnP device type. For example: "urn:ses-com:device:SatIPServer:1".</param>
        /// <param name="timeout">Search wait timeout in milliseconds.</param>
        /// <returns>Returns matched UPnP devices.</returns>
        public UpnpDevice[] Search(string deviceType, int timeout)
        {
            if(timeout < 1)
            {
                timeout = 1;
            }
            var query = new StringBuilder();
            query.Append("M-SEARCH * HTTP/1.1\r\n");
            query.Append("HOST: 239.255.255.250:1900\r\n");
            query.Append("MAN: \"ssdp:discover\"\r\n");
            query.Append("MX: 2\r\n");
            query.Append("ST: " +deviceType+"\r\n");
            query.Append("\r\n");

            using(var socket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp))
            {
                socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.Broadcast,1);
                socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.IpTimeToLive,2);
                socket.SendTo(Encoding.UTF8.GetBytes(query.ToString()), new IPEndPoint(IPAddress.Broadcast, 1900));
                socket.SendTo(Encoding.UTF8.GetBytes(query.ToString()), new IPEndPoint(IPAddress.Broadcast, 1900));
                socket.SendTo(Encoding.UTF8.GetBytes(query.ToString()), new IPEndPoint(IPAddress.Broadcast, 1900));
                var deviceLocations = new List<string>();    
                var buffer          = new byte[32000];
                var startTime       = DateTime.Now;
                var endTime = startTime.AddMilliseconds(timeout);
                while (endTime > DateTime.Now)
                {
                    if (socket.Poll(1, SelectMode.SelectRead))
                    {
                        var countReceived = socket.Receive(buffer);
                        var responseLines = Encoding.UTF8.GetString(buffer, 0, countReceived).Split('\n');
                        foreach (var responseLine in responseLines)
                        {
                            string[] locationHeader = responseLine.Split(new char[] {':'}, 2);
                            if ((string.Equals(locationHeader[0], "location", StringComparison.InvariantCultureIgnoreCase)) &&
                                (!deviceLocations.Contains(locationHeader[1].Trim())))
                            {
                                deviceLocations.Add(locationHeader[1].Trim());
                            }
                        }
                    }
                }
                var devices = new List<UpnpDevice>();
                foreach (var location in deviceLocations)
                {
                    try
                    {
                        devices.Add(new UpnpDevice(location));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(@"anything isnt ok with location or description");
                    }
                }
                return devices.ToArray();
            }
        }

        /// <summary>
        /// Searches the network for UPnP devices.
        /// </summary>
        /// <param name="ip">IP address of UPnP device.</param>
        /// <param name="deviceType">UPnP device type. For example: "urn:schemas-upnp-org:device:InternetGatewayDevice:1".</param>
        /// <param name="timeout">Search wait timeout in milliseconds.</param>
        /// <returns>Returns matched UPnP devices.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        public UpnpDevice[] Search(IPAddress ip, string deviceType, int timeout)
        {
            if(ip == null)
            {
                throw new ArgumentNullException("ip");
            }
            if(timeout < 1)
            {
                timeout = 1;
            }
            using(var socket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp)){
                socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.IpTimeToLive,2);
                var query = new StringBuilder();
                query.Append("M-SEARCH * HTTP/1.1\r\n");
                query.Append("MAN: \"ssdp:discover\"\r\n");
                query.Append("MX: 2\r\n");
                query.Append("ST: " + deviceType + "\r\n");
                query.Append("\r\n");
                socket.SendTo(Encoding.UTF8.GetBytes(query.ToString()),new IPEndPoint(ip,1900));
                socket.SendTo(Encoding.UTF8.GetBytes(query.ToString()), new IPEndPoint(ip, 1900));
                socket.SendTo(Encoding.UTF8.GetBytes(query.ToString()), new IPEndPoint(ip, 1900));
                var deviceLocations = new List<string>();    
                var buffer          = new byte[32000];
                var startTime = DateTime.Now;
                var endTime = startTime.AddMilliseconds(timeout);
                while (endTime > DateTime.Now)
                {
                    if(socket.Poll(1,SelectMode.SelectRead))
                    {
                        var countReceived = socket.Receive(buffer);
                        var responseLines = Encoding.UTF8.GetString(buffer,0,countReceived).Split('\n');
                        foreach (var responseLine in responseLines)
                        {
                            string[] locationHeader = responseLine.Split(new char[] { ':' }, 2);
                            if ((string.Equals(locationHeader[0], "location", StringComparison.InvariantCultureIgnoreCase)) &&
                                (!deviceLocations.Contains(locationHeader[1].Trim())))
                            {
                                deviceLocations.Add(locationHeader[1].Trim());
                            }
                        }
                    }
                }
                var devices = new List<UpnpDevice>();
                foreach(var location in deviceLocations){
                    try
                    {
                        devices.Add(new UpnpDevice(location));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(@"anything isnt ok with location or description");
                    }
                }
                return devices.ToArray();
            }        
        }
        #endregion
    }
}
