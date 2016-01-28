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

using System.ComponentModel;
using System.Collections.Generic;
using System;



public class Service : INotifyPropertyChanged
{
    #region Private Fields

    private string _name;
    private string _source;
    private string _frequency;
    private string _symbolRate;
    private string _polarity;
    private string _pids;
    private string _frontEnd;
    private string _modulationType;
    private string _modulationSystem;
    private string _fecrate;
    private string _rollOff;
    private string _pilotTones;
    private string _bandwidth;
    private string _guardInterval;
    private string _transmissionMode;
    private string _spectrumInversion;
    private string _c2TuningFrequencyType;
    private string _dataSlice;
    private string _plp;
    private string _t2id;
    private string _sm;


    #endregion

    #region Constructor

    public Service()
    { }
    public Service(string name, IEnumerable<string> parameters)
    {
        _name = name;
        foreach (var parameter in parameters)
        {
            if (parameter.Substring(0).Contains("src="))
            {
                var sourceline = parameter.Split('=');
                _source = sourceline[1];
            }
            if (parameter.Substring(0).Contains("fe="))
            {
                var frontendline = parameter.Split('=');
                _frontEnd = frontendline[1];
            }
            if (parameter.Substring(0).Contains("freq="))
            {
                var frequencyLine = parameter.Split('=');
                _frequency = frequencyLine[1];
            }
            if (parameter.Substring(0).Contains("sr="))
            {
                var symbolrateline = parameter.Split('=');
                _symbolRate = symbolrateline[1];
            }
            if (parameter.Substring(0).Contains("pol="))
            {
                var polarityline = parameter.Split('=');
                _polarity = polarityline[1];
            }
            if (parameter.Substring(0).Contains("ro="))
            {
                var rolloffline = parameter.Split('=');
                _rollOff = rolloffline[1];
            }
            if (parameter.Substring(0).Contains("msys="))
            {
                var systemline = parameter.Split('=');
                _modulationSystem = systemline[1];
            }
            if (parameter.Substring(0).Contains("mtype="))
            {
                var modulationline = parameter.Split('=');
                _modulationType = modulationline[1];
            }
            if (parameter.Substring(0).Contains("plts="))
            {
                var pilottonesline = parameter.Split('=');
                _pilotTones = pilottonesline[1];
            }
            if (parameter.Substring(0).Contains("tmode="))
            {
                var transmissionmodeline = parameter.Split('=');
                _transmissionMode = transmissionmodeline[1];
            }
            if (parameter.Substring(0).Contains("gi="))
            {
                var guardIntervalline = parameter.Split('=');
                _guardInterval = guardIntervalline[1];
            }
            if (parameter.Substring(0).Contains("t2id="))
            {
                var t2id = parameter.Split('=');
                _t2id = t2id[1];
            }
            if (parameter.Substring(0).Contains("sm="))
            {
                var sm = parameter.Split('=');
                _sm = sm[1];
            }
            if (parameter.Substring(0).Contains("fec="))
            {
                var fecline = parameter.Split('=');
                _fecrate = fecline[1];
            }
            if (parameter.Substring(0).Contains("pids="))
            {
                var pi = parameter.Split('=');
                _pids = pi[1];
            }
            if (parameter.Substring(0).Contains("specinv="))
            {
                var spin = parameter.Split('=');
                _spectrumInversion = spin[1];
            }
            if (parameter.Substring(0).Contains("ds="))
            {
                var ds = parameter.Split('=');
                _dataSlice = ds[1];
            }
            if (parameter.Substring(0).Contains("plp="))
            {
                var plp = parameter.Split('=');
                _plp = plp[1];
            }
        }


    }

    #endregion

    #region Properties
    /// <summary>
    /// DVBC 
    /// </summary>
    public string SpectrumInversion
    {
        get { return _spectrumInversion; }
        set { if (_spectrumInversion != value) { _spectrumInversion = value; OnPropertyChanged("SpectrumInversion"); } }
    }
    /// <summary>
    /// DVBT DVBC
    /// </summary>
    public string Bandwidth
    {
        get { return _bandwidth; }
        set { if (_bandwidth != value) { _bandwidth = value; OnPropertyChanged("Bandwidth"); } }
    }
    /// <summary>
    /// The Service Name
    /// </summary>
    public string Name
    {
        get { return _name; }
        set { if (_name != value) { _name = value; OnPropertyChanged("Name"); } }
    }
    /// <summary>
    /// DVBS
    /// </summary>
    public string Source
    {
        get { return _source; }
        set { if (_source != value) { _source = value; OnPropertyChanged("Source"); } }
    }
    public string FrontEnd
    {
        get { return _frontEnd; }
        set { if (_frontEnd != value) { _frontEnd = value; OnPropertyChanged("FrontEnd"); } }
    }
    /// <summary>
    /// Frequency is for All ModulationSystem Required 
    /// </summary>
    public string Frequency
    {
        get { return _frequency; }
        set { if (_frequency != value) { _frequency = value; OnPropertyChanged("Frequency"); } }
    }
    /// <summary>
    /// Symbolrate is for ModulationSystem Satelite and Cable Required
    /// </summary>
    public string SymbolRate
    {
        get { return _symbolRate; }
        set { if (_symbolRate != value) { _symbolRate = value; OnPropertyChanged("SymbolRate"); } }
    }
    /// <summary>
    /// Polarity is for Satellite Required
    /// </summary>
    public string Polarity
    {
        get { return _polarity; }
        set { if (_polarity != value) { _polarity = value; OnPropertyChanged("Polarity"); } }
    }
    /// <summary>
    /// ModulationSystem is for all Required
    /// </summary>
    public string ModulationSystem
    {
        get { return _modulationSystem; }
        set { if (_modulationSystem != value) { _modulationSystem = value; OnPropertyChanged("ModulationSystem"); } }
    }

    /// <summary>
    /// 
    /// </summary>
    public string Pids
    {
        get { return _pids; }
        set { if (_pids != value) { _pids = value; OnPropertyChanged("Pids"); } }
    }
    /// <summary>
    /// PilotTones is for Satellite Required
    /// </summary>
    public string PilotTones
    {
        get { return _pilotTones; }
        set { if (_pilotTones != value) { _pilotTones = value; OnPropertyChanged("PilotTones"); } }
    }
    /// <summary>
    /// RollOff is for Satellite Required
    /// </summary>
    public string RollOff
    {
        get { return _rollOff; }
        set { if (_rollOff != value) { _rollOff = value; OnPropertyChanged("RollOff"); } }
    }
    /// <summary>
    /// 
    /// </summary>
    public string Fecrate
    {
        get { return _fecrate; }
        set { if (_fecrate != value) { _fecrate = value; OnPropertyChanged("FecRate"); } }
    }
    /// <summary>
    /// 
    /// </summary>
    public string ModulationType
    {
        get { return _modulationType; }
        set { if (_modulationType != value) { _modulationType = value; OnPropertyChanged("ModulationType"); } }
    }
    /// <summary>
    /// DVBC2
    /// </summary>
    public string C2TuningFrequencyType
    {
        get { return _c2TuningFrequencyType; }
        set { if (_c2TuningFrequencyType != value) { _c2TuningFrequencyType = value; OnPropertyChanged("C2TuningFrequencyType"); } }
    }
    /// <summary>
    /// DVBC2 DVBT2
    /// </summary>
    public string Plp
    {
        get { return _plp; }
        set { if (_plp != value) { _plp = value; OnPropertyChanged("Plp"); } }
    }

    /// <summary>
    /// DVBC2
    /// </summary>
    public string DataSlice
    {
        get { return _dataSlice; }
        set { if (_dataSlice != value) { _dataSlice = value; OnPropertyChanged("DataSlice"); } }
    }
    /// <summary>
    /// DVBT2
    /// </summary>
    public string T2id
    {
        get { return _t2id; }
        set { if (_t2id != value) { _t2id = value; OnPropertyChanged("T2id"); } }
    }
    /// <summary>
    /// DVBT2
    /// </summary>
    public string Sm
    {
        get { return _sm; }
        set { if (_sm != value) { _sm = value; OnPropertyChanged("Sm"); } }
    }
    #endregion

    #region Public Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region Protected Methods

    protected void OnPropertyChanged(string name)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(name));
        }
    }

    #endregion

    #region Public Overrides

    public override string ToString()
    {
        string value = string.Empty;
        switch (_modulationSystem)
        {
            case "dvbc":
                value = string.Format("freq={0}&msys={1}&mtype={2}&sr={3}&specinv={4}&pids={5}", _frequency, _modulationSystem, _modulationType, _symbolRate, _spectrumInversion, _pids);
                break;
            case "dvbc2":
                value = string.Format("freq={0}&c2tft={1}&bw{2}&msys={3}&mtype={4}&sr={5}&ds={6}&plp={7}&specinv={8}&pids={9}", _frequency, _c2TuningFrequencyType, _bandwidth, _modulationSystem, _modulationType, _symbolRate, _dataSlice, _plp, _spectrumInversion, _pids);
                break;
            case "dvbs":
                value = string.Format("src={0}&freq={1}&pol={2}&msys={3}&sr={4}&fec={5}&mtype={6}&pids={7}", _source, _frequency, _polarity, _modulationSystem, _symbolRate, _fecrate, _modulationType, _pids);
                break;
            case "dvbs2":
                value = string.Format("src={0}&freq={1}&pol={2}&msys={3}&sr={4}&fec={5}&mtype={6}&ro={7}&plts={8}&pids={9}", _source, _frequency, _polarity, _modulationSystem, _symbolRate, _fecrate, _modulationType, _rollOff, _pilotTones, _pids);
                break;
            case "dvbt":
                value = string.Format("freq={0}&bw={1}&msys={2}&tmode={3}&mtype={4}&gi={5}&fec={6}&pids={7}", _frequency, _bandwidth, _modulationSystem, _transmissionMode, _modulationType, _guardInterval, _fecrate, _pids);
                break;
            case "dvbt2":
                value = string.Format("freq={0}&bw={1}&msys={2}&tmode={3}&mtype={4}&gi={5}&fec={6}&plp{7}&t2id{8}&sm{9}&pids={10}", _frequency, _bandwidth, _modulationSystem, _transmissionMode, _modulationType, _guardInterval, _fecrate, _t2id, _sm, _pids);
                //value = string.Format("freq={0}&bw={1}&msys={2}&tmode={3}&mtype={4}&gi={5}&fec={6}&plp={7}&t2id={8}&sm={9}&pids={10}", _frequency, _bandwidth, _modulationSystem, _transmissionMode, _modulationType, _guardInterval, _fecrate, 0, 0, 0, _pids);
                break;
        }
        return value;
    }

    #endregion
}


