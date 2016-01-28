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
using System.Globalization;
using System.Net.Sockets;

namespace SatIp.RtspSample.Rtsp
{
    /// <summary>
    /// A simple implementation of an RTSP client.
    /// </summary>
    public class RtspClient
    {
        #region variables

        private readonly string _serverHost;
        private TcpClient _client =null;
        private int _cseq = 1;
        private readonly object _lockObject = new object();

        #endregion


        /// <summary>
        /// Initialise a new instance of the <see cref="RtspClient"/> class.
        /// </summary>
        /// <param name="serverHost">The RTSP server host name or IP address.</param>
        public RtspClient(string serverHost)
        {
            _serverHost = serverHost;      
            _client = new TcpClient(serverHost, 554);
        }
        ~RtspClient()
        {
            lock (_lockObject)
            {
                if (_client != null)
                {
                    _client.Close();
                    _client = null;
                }
            }
        }
        /// <summary>
        /// Send an RTSP request and retrieve the response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <returns>the response status code</returns>
        public RtspStatusCode SendRequest(RtspRequest request, out RtspResponse response)
        {
            response = null;
            lock (_lockObject)
            {
                NetworkStream stream = null;
                try
                {
                    stream = _client.GetStream();
                    if (stream == null)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    _client.Close();
                }
                try
                {
                    if (_client == null)
                    {
                        _client = new TcpClient(_serverHost, 554);
                    }
                    // Send the request and get the response.
                    request.Headers.Add("CSeq", _cseq.ToString(CultureInfo.InvariantCulture));                    
                    byte[] requestBytes = request.Serialise();            
                    stream.Write(requestBytes, 0, requestBytes.Length);
                    _cseq++;
                    byte[] responseBytes = new byte[_client.ReceiveBufferSize];
                    int byteCount = stream.Read(responseBytes, 0, responseBytes.Length);
                    response = RtspResponse.Deserialise(responseBytes, byteCount);
                    // Did we get the whole response?
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
                            while (byteCount > 0 && response.Body.Length < contentLength)
                            {
                                byteCount = stream.Read(responseBytes, 0, responseBytes.Length);
                                response.Body += System.Text.Encoding.UTF8.GetString(responseBytes, 0, byteCount);
                            }
                        }
                    }
                    return response.StatusCode;

                }
                finally
                {
                    stream.Close();
                }
            }
        }
    }

}
