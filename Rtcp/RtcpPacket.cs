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

namespace SatIp.RtspSample.Rtcp
{
    /// <summary>
    /// The class that describes a Rtcp Packet
    /// </summary>
    public abstract class RtcpPacket
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public int Version { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Padding { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int ReportCount { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int Type { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int Length { get; private set; }  
        #endregion      
        #region Public virtual Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        public virtual void Parse(byte[] buffer, int offset)
        {
            Version = buffer[offset] >> 6;
            Padding = (buffer[offset] & 0x20) != 0;
            ReportCount = buffer[offset] & 0x1f;
            Type = buffer[offset + 1];
            Length = (Utils.Convert2BytesToInt(buffer, offset + 2) * 4) + 4;
        } 
        #endregion
    }
}
