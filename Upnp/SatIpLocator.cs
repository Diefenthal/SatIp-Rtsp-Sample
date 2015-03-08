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

using System.Runtime.InteropServices;
using UPNPLib;


namespace SatIp.RtspSample.Upnp
{
    [ComVisible(true), ComImport,
    Guid("415A984A-88B3-49F3-92AF-0508BEDF0D6C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IUPnPDeviceFinderCallback
    {
        [PreserveSig]
        int DeviceAdded([In] int lFindData,
       [In] IUPnPDevice pDevice);

        [PreserveSig]
        int DeviceRemoved([In] int lFindData,
       [In] string bstrUdn);

        [PreserveSig]
        int SearchComplete([In] int lFindData);
    }
    public class DeviceCollector : IUPnPDeviceFinderCallback
    {
        public delegate void DeviceAddedEventHandler(UPnPDevice addeddevice);
        public delegate void DeviceRemovedEventHandler(string sUdn);
        public delegate void SearchCompletedEventHandler();

        public event DeviceAddedEventHandler DeviceAdded;
        public event DeviceRemovedEventHandler DeviceRemoved;
        public event SearchCompletedEventHandler SearchCompleted;


        public DeviceCollector()
        {
            var finder = new UPnPDeviceFinderClass();
            var searchId = finder.CreateAsyncFind("urn:ses-com:device:SatIPServer:1", 0, this);
            finder.StartAsyncFind(searchId);
        }

        #region IUPnPDeviceFinderCallback Members

        int IUPnPDeviceFinderCallback.DeviceAdded(int lFindData, IUPnPDevice pDevice)
        {
            DeviceAdded((UPnPDevice)pDevice);
            return 0;
        }

        int IUPnPDeviceFinderCallback.DeviceRemoved(int lFindData, string bstrUdn)
        {
            DeviceRemoved(bstrUdn);
            return 0;
        }

        int IUPnPDeviceFinderCallback.SearchComplete(int lFindData)
        {
            SearchCompleted();
            return 0;
        }

        #endregion
    }
}
