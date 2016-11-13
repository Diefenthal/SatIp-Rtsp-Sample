using System.Collections.ObjectModel;
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
using System.Text;

namespace SatIp.RtspSample.Rtcp
{
    /// <summary>
    /// The class that describes a Rtcp Source Description Packet
    /// </summary>
    public class RtcpSourceDescriptionPacket :RtcpPacket
    {
        #region Properties
        /// <summary>
        /// Get the list of source descriptions.
        /// </summary>
        public Collection<SourceDescriptionBlock> Descriptions; 
        #endregion
        #region Public override Methods
        /// <summary>
        /// Unpack the data in a packet.
        /// </summary>
        /// <param name="buffer">The buffer containing the packet.</param>
        /// <param name="offset">The offset to the first byte of the packet within the buffer.</param>
        public override void Parse(byte[] buffer, int offset)
        {
            base.Parse(buffer, offset);
            Descriptions = new Collection<SourceDescriptionBlock>();

            int index = 4;

            while (Descriptions.Count < ReportCount)
            {
                SourceDescriptionBlock descriptionBlock = new SourceDescriptionBlock();
                descriptionBlock.Process(buffer, offset + index);
                Descriptions.Add(descriptionBlock);
                index += descriptionBlock.BlockLength;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Source Description.\n");
            sb.AppendFormat("Version : {0} .\n", Version);
            sb.AppendFormat("Padding : {0} .\n", Padding);
            sb.AppendFormat("Report Count : {0} .\n", ReportCount);
            sb.AppendFormat("PacketType: {0} .\n", Type);
            sb.AppendFormat("Length : {0} .\n", Length);            
            foreach (var description in Descriptions)
            {
                description.ToString();
            }
            sb.AppendFormat(".\n");
            return sb.ToString();
        }
        #endregion        
    }
}
