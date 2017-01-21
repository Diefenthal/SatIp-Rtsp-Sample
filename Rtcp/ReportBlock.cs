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
    /// The class that describes a RTCP Report Block
    /// </summary>
    public class ReportBlock
    {
        #region Properties
        /// <summary>
        /// Get the length of the block.
        /// </summary>
        public int BlockLength { get { return (24); } }
        /// <summary>
        /// Get the synchronization source.
        /// </summary>
        public string SynchronizationSource { get; private set; }
        /// <summary>
        /// Get the fraction lost.
        /// </summary>
        public int FractionLost { get; private set; }
        /// <summary>
        /// Get the cumulative packets lost.
        /// </summary>
        public int CumulativePacketsLost { get; private set; }
        /// <summary>
        /// Get the highest number received.
        /// </summary>
        public int HighestNumberReceived { get; private set; }
        /// <summary>
        /// Get the inter arrival jitter.
        /// </summary>
        public int InterArrivalJitter { get; private set; }
        /// <summary>
        /// Get the timestamp of the last report.
        /// </summary>
        public int LastReportTimeStamp { get; private set; }
        /// <summary>
        /// Get the delay since the last report.
        /// </summary>
        public int DelaySinceLastReport { get; private set; } 
        #endregion
        #region Constructor
        /// <summary>
        /// Initialize a new instance of the ReportBlock class.
        /// </summary>
        public ReportBlock() { } 
        #endregion
        #region Public Methods
        /// <summary>
        /// Unpack the data in a packet.
        /// </summary>
        /// <param name="buffer">The buffer containing the packet.</param>
        /// <param name="offset">The offset to the first byte of the packet within the buffer.</param>        
        public void Process(byte[] buffer, int offset)
        {
            SynchronizationSource = Utils.ConvertBytesToString(buffer, offset, 4);
            FractionLost = buffer[offset + 4];
            CumulativePacketsLost = Utils.Convert3BytesToInt(buffer, offset + 5);
            HighestNumberReceived = Utils.Convert4BytesToInt(buffer, offset + 8);
            InterArrivalJitter = Utils.Convert4BytesToInt(buffer, offset + 12);
            LastReportTimeStamp = Utils.Convert4BytesToInt(buffer, offset + 16);
            DelaySinceLastReport = Utils.Convert4BytesToInt(buffer, offset + 20);
        } 
        #endregion
        #region Public Override Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Report Block.\n");
            sb.AppendFormat("Length : {0} .\n", BlockLength);
            sb.AppendFormat("SynchronizationSource : {0} .\n", SynchronizationSource);
            sb.AppendFormat("Fraction Lost : {0} .\n", FractionLost);
            sb.AppendFormat("Cumulative Packets Lost : {0} .\n", CumulativePacketsLost);
            sb.AppendFormat("Highest Number Received : {0} .\n", HighestNumberReceived);
            sb.AppendFormat("Inter Arrival Jitter : {0} .\n", InterArrivalJitter);
            sb.AppendFormat("Last Report TimeStamp : {0} .\n", LastReportTimeStamp);
            sb.AppendFormat("Delay Since Last Report : {0} .\n", DelaySinceLastReport);
            sb.AppendFormat(".\n");
            return sb.ToString();
        }  
        #endregion
    }
}
