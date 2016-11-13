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
    /// The class that describes a source description item.
    /// </summary>
    public class SourceDescriptionItem
    {
        #region Properties
        /// <summary>
        /// Get the type.
        /// </summary>
        public int Type { get; private set; }
        /// <summary>
        /// Get the text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Get the length of the item.
        /// </summary>
        public int ItemLength { get { return (Text.Length + 2); } }

        /// <summary>
        /// Initialize a new instance of the SourceDescriptionItem class.
        /// </summary>
        public SourceDescriptionItem() { } 
        #endregion
        #region Public Methods
        /// <summary>
        /// Unpack the data in a packet.
        /// </summary>
        /// <param name="buffer">The buffer containing the packet.</param>
        /// <param name="offset">The offset to the first byte of the packet within the buffer.</param>        
        public void Process(byte[] buffer, int offset)
        {
            Type = buffer[offset];
            if (Type != 0)
            {
                int length = buffer[offset + 1];
                Text = Utils.ConvertBytesToString(buffer, offset + 2, length);
            }
        } 
        #endregion
        #region Public overrides Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Source Description Item.\n");
            sb.AppendFormat("Type : {0} .\n", Type);
            sb.AppendFormat("Text : {0} .\n", Text);
            sb.AppendFormat("ItemLength : {0} .\n", ItemLength);
            sb.AppendFormat(".\n");
            return sb.ToString();
        } 
        #endregion
    }
}
