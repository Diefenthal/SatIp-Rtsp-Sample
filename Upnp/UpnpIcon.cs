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
namespace SatIp.RtspSample.Upnp
{
    public class UpnpIcon
    {
        public UpnpIcon()
        {
            Url = "";
            MimeType = "";
        }

        public int Depth { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string MimeType { get; set; }
        public string Url { get; set; }
    }
}
