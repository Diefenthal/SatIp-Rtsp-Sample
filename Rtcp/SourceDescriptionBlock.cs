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
using System.Collections.ObjectModel;
using System.Text;

namespace SatIp.RtspSample.Rtcp
{
    /// <summary>
    /// The class that describes a source description block.
    /// </summary>
    public class SourceDescriptionBlock
    {
        #region Private Fields
        /// <summary>
        ///  
        /// </summary>
        private int _blockLength;
        #endregion
        #region Properties
        /// <summary>
        /// Get the length of the block.
        /// </summary>
        public int BlockLength { get { return (_blockLength + (_blockLength % 4)); } }
        /// <summary>
        /// Get the synchronization source.
        /// </summary>
        public string SynchronizationSource { get; private set; }
        /// <summary>
        /// Get the list of source description items.
        /// </summary>
        public Collection<SourceDescriptionItem> Items; 
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
            Items = new Collection<SourceDescriptionItem>();
            int index = 4;
            bool done = false;
            do
            {
                SourceDescriptionItem item = new SourceDescriptionItem();
                item.Process(buffer, offset + index);

                if (item.Type != 0)
                {
                    Items.Add(item);
                    index += item.ItemLength;
                    _blockLength += item.ItemLength;
                }
                else
                {
                    _blockLength++;
                    done = true;
                }
            }
            while (!done);
        } 
        #endregion
        #region public override Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Source Description Block.\n");
            sb.AppendFormat("Block Length : {0} .\n", BlockLength);
            sb.AppendFormat("SynchronizationSource : {0} .\n", SynchronizationSource);
            foreach (var sourceDescriptionItem in Items)
            {
                sourceDescriptionItem.ToString();
            }
            sb.AppendFormat(".\n");
            return sb.ToString();
        } 
        #endregion
    }
}
