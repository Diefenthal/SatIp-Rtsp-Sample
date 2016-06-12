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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SatIp.RtspSample.Logging;

namespace SatIp.RtspSample.Rtcp
{
    public class RtcpListener
    {
        private Thread _rtcpListenerThread;
        private AutoResetEvent _rtcpListenerThreadStopEvent = null;
        private UdpClient _udpClient;
        private IPEndPoint _multicastEndPoint;
        private IPEndPoint _serverEndPoint;
        private TransmissionMode _transmissionMode;

        public RtcpListener(String address, int port, TransmissionMode mode)
        {
            _transmissionMode = mode;
            switch (mode)
            {
                case TransmissionMode.Unicast:
                    _udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(address), port));
                    _serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    break;
                case TransmissionMode.Multicast:
                    _multicastEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
                    _serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    _udpClient = new UdpClient();
                    _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                    _udpClient.ExclusiveAddressUse = false;
                    _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                    _udpClient.JoinMulticastGroup(_multicastEndPoint.Address);
                    break;
            }
            //StartRtcpListenerThread();
        }

        public void StartRtcpListenerThread()
        {
            // Kill the existing thread if it is in "zombie" state.
            if (_rtcpListenerThread != null && !_rtcpListenerThread.IsAlive)
            {
                StopRtcpListenerThread();
            }

            if (_rtcpListenerThread == null)
            {
                Logger.Info("SAT>IP : starting new RTCP listener thread");
                _rtcpListenerThreadStopEvent = new AutoResetEvent(false);
                _rtcpListenerThread = new Thread(new ThreadStart(RtcpListenerThread));
                _rtcpListenerThread.Name = string.Format("SAT>IP tuner  RTCP listener");
                _rtcpListenerThread.IsBackground = true;
                _rtcpListenerThread.Priority = ThreadPriority.Lowest;
                _rtcpListenerThread.Start();
            }
        }

        public void StopRtcpListenerThread()
        {
            if (_rtcpListenerThread != null)
            {
                if (!_rtcpListenerThread.IsAlive)
                {
                    Logger.Warn("SAT>IP : aborting old RTCP listener thread");
                    _rtcpListenerThread.Abort();
                }
                else
                {
                    _rtcpListenerThreadStopEvent.Set();
                    if (!_rtcpListenerThread.Join(400 * 2))
                    {
                        Logger.Warn("SAT>IP : failed to join RTCP listener thread, aborting thread");
                        _rtcpListenerThread.Abort();
                    }
                }
                _rtcpListenerThread = null;
                if (_rtcpListenerThreadStopEvent != null)
                {
                    _rtcpListenerThreadStopEvent.Close();
                    _rtcpListenerThreadStopEvent = null;
                }
            }
        }

        private void RtcpListenerThread()
        {
            try
            {               
                bool receivedGoodBye = false;                
                try
                {
                    _udpClient.Client.ReceiveTimeout = 400;
                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    while (!receivedGoodBye && !_rtcpListenerThreadStopEvent.WaitOne(1))
                    {
                        byte[] packets = _udpClient.Receive(ref serverEndPoint);
                        if (packets == null)
                        {
                            continue;
                        }
                        
                        int offset = 0;
                        while (offset < packets.Length)
                        {
                            switch (packets[offset + 1])
                            {
                                case 200: //sr
                                    var sr = new RtcpSenderReportPacket();
                                    sr.Parse(packets, offset);                                    
                                    offset += sr.Length;
                                    break;
                                case 201: //rr
                                    var rr = new RtcpReceiverReportPacket();
                                    rr.Parse(packets, offset);                                    
                                    offset += rr.Length;
                                    break;
                                case 202: //sd
                                    var sd = new RtcpSourceDescriptionPacket();
                                    sd.Parse(packets, offset);                                    
                                    offset += sd.Length;
                                    break;
                                case 203: // bye
                                    var bye = new RtcpByePacket();
                                    bye.Parse(packets, offset);                                    
                                    receivedGoodBye = true;
                                    OnPacketReceived(new RtcpPacketReceivedArgs(bye));
                                    offset += bye.Length;                                    
                                    break;
                                case 204: // app
                                    var app = new RtcpAppPacket();
                                    app.Parse(packets, offset);                                    
                                    OnPacketReceived(new RtcpPacketReceivedArgs(app));
                                    offset += app.Length;
                                    break;
                            }

                            //RtcpPacket packet = RtcpPacket.Parse(packets, offset);
                            //Console.WriteLine(packet.Type.ToString());
                            //offset += packet.Lenght;
                            //// Refer to RFC 3550.
                            //// https://www.ietf.org/rfc/rfc3550.txt
                            //byte packetType = packets[offset + 1];
                            //int packetByteCount = ((packets[offset + 2] << 8) + packets[offset + 3] + 1) * 4;
                            //if (offset + packetByteCount > packets.Length)
                            //{
                            //    //Logger.Warn("SAT>IP : received incomplete RTCP packet, offset = {0}", offset);
                            //    ////Dump.DumpBinary(packets);
                            //    break;
                            //}

                            //if (packetType == 203)  // goodbye
                            //{
                            //    receivedGoodBye = true;
                            //    break;
                            //}
                            //else if (packetType == 204) // application-defined
                            //{
                            //    int offsetStartOfPacket = offset;
                            //    offset += 8;  // skip to the start of the name SSRC/CSRC
                            //    if (offset + 4 > packets.Length)
                            //    {
                            //        //Logger.Warn("SAT>IP : received RTCP application-defined packet too short to contain name, offset = {0}", offsetStartOfPacket);
                            //        //Dump.DumpBinary(packets);
                            //        break;
                            //    }
                            //    string name = System.Text.Encoding.ASCII.GetString(packets, offset, 4);
                            //    offset += 4;
                            //    if (!name.Equals("SES1"))
                            //    {
                            //        // Not SAT>IP data. Odd but okay.
                            //        offset = offsetStartOfPacket + packetByteCount;
                            //        continue;
                            //    }
                            //    if (offset + 4 > packets.Length)
                            //    {
                            //        //Logger.Warn("SAT>IP : received SAT>IP RTCP packet too short to contain string length, offset = {0}", offsetStartOfPacket);
                            //        //Dump.DumpBinary(packets);
                            //        break;
                            //    }
                            //    int stringByteCount = (packets[offset + 2] << 8) + packets[offset + 3];
                            //    offset += 4;
                            //    if (offset + stringByteCount > packets.Length)
                            //    {
                            //        //Logger.Warn("SAT>IP : received SAT>IP RTCP packet too short to contain string, offset = {0}", offsetStartOfPacket);
                            //        //Dump.DumpBinary(packets);
                            //        break;
                            //    }
                            //    string description = System.Text.Encoding.UTF8.GetString(packets, offset, stringByteCount);
                            //    //Match m = REGEX_DESCRIBE_RESPONSE_SIGNAL_INFO.Match(description);
                            //    //if (m.Success)
                            //    //{
                            //    //    //    _isSignalLocked = m.Groups[2].Captures[0].Value.Equals("1");
                            //    //    level = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;   // strength: 0..255 => 0..100
                            //    //    quality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;     // quality: 0..15 => 0..100

                            //    //}
                            //    //Update(level, quality);
                            //    offset = offsetStartOfPacket + packetByteCount;
                            //}
                            //else
                            //{
                            //    offset += packetByteCount;
                            //}
                        }
                        
                    }
                }
                finally
                {
                    switch (_transmissionMode)
                    {
                        case TransmissionMode.Multicast:
                            _udpClient.DropMulticastGroup(_multicastEndPoint.Address);
                            _udpClient.Close();
                            break;
                        case TransmissionMode.Unicast:
                            _udpClient.Close();
                            break;
                    }   
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("SAT>IP : RTCP listener thread exception"), ex);
                return;
            }
            Logger.Warn("SAT>IP : RTCP listener thread stopping");
        }
        public delegate void PacketReceivedHandler(object sender, RtcpPacketReceivedArgs e);
        public event PacketReceivedHandler PacketReceived;
        public class RtcpPacketReceivedArgs : EventArgs
        {
            public Object Packet { get; private set; }

            public RtcpPacketReceivedArgs(Object packet)
            {
                Packet = packet;
            }
        }
        protected void OnPacketReceived(RtcpPacketReceivedArgs args)
        {
            if (PacketReceived != null)
            {
                PacketReceived(this, args);
            }
        }
    }
}
