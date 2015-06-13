/*  
    Copyright (C) <2007-2014>  <Kay Diefenthal>

    SatIp.RtspSample is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    SatIp.UI.RtspSample is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with SatIp.UI.RtspSample.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;

namespace SatIp.RtspSample.Logging
{
    public class LogDurationScope : IDisposable
    {
        private readonly string _name;
        private readonly DateTime _startDateTime;

        public LogDurationScope(string name)
        {
            _name = name;
            _startDateTime = DateTime.Now;
        }

        public static LogDurationScope Start(string name)
        {
            return new LogDurationScope(name);
        }

        #region IDisposable Pattern

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Logger.Verbose("{0}, duration: {1}", _name, (DateTime.Now - _startDateTime).TotalMilliseconds);
                }
            }
            _disposed = true;
        }

        ~LogDurationScope()
        {
            Dispose(false);
        }

        #endregion
    }
}