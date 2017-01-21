using System.Collections.ObjectModel;
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
using System.Text;

namespace SatIp.RtspSample.Rtcp
{
    /// <summary>
    /// The class that describes a Rtcp Receiver Report Packet
    /// </summary>
    public class RtcpReceiverReportPacket :RtcpPacket
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public string SynchronizationSource { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Collection<ReportBlock> ReportBlocks { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] ProfileExtension { get; private set; } 
        #endregion
        #region Public override Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        public override void Parse(byte[] buffer, int offset)
        {
            base.Parse(buffer, offset);
            SynchronizationSource = Utils.ConvertBytesToString(buffer, offset + 4, 4);

            ReportBlocks = new Collection<ReportBlock>();
            int index = 8;

            while (ReportBlocks.Count < ReportCount)
            {
                ReportBlock reportBlock = new ReportBlock();
                reportBlock.Process(buffer, offset + index);
                ReportBlocks.Add(reportBlock);
                index += reportBlock.BlockLength;
            }

            if (index < Length)
            {
                ProfileExtension = new byte[Length - index];

                for (int extensionIndex = 0; index < Length; index++)
                {
                    ProfileExtension[extensionIndex] = buffer[offset + index];
                    extensionIndex++;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Receiver Report.\n");
            sb.AppendFormat("Version : {0} .\n", Version);
            sb.AppendFormat("Padding : {0} .\n", Padding);
            sb.AppendFormat("Report Count : {0} .\n", ReportCount);
            sb.AppendFormat("PacketType: {0} .\n", Type);
            sb.AppendFormat("Length : {0} .\n", Length);
            sb.AppendFormat("SynchronizationSource : {0} .\n", SynchronizationSource);
            foreach (var reportblock in ReportBlocks)
            {
                reportblock.ToString();
            }
            sb.AppendFormat(".\n");
            return sb.ToString();
        } 
        #endregion
    }
}
