using HandyControl.Controls;
using InperDeviceManagement;
using InperProtocolStack;
using InperProtocolStack.CmdPhotometry;
using InperProtocolStack.Communication;
using InperProtocolStack.TransmissionCtrl;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper.JsonBean;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Annotations;
using SciChart.Charting.Visuals.Axes;
using Stylet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InperStudio.Lib.Helper
{
    public class WaveGroup
    {
        public bool IsChecked { get; set; } = false;
        public int GroupId { get; set; }
        public string WaveType { get; set; }
        public double LightPower { get; set; } = 40;
    }
    public class PlotData
    {
        public PlotData(int group, ConcurrentDictionary<int, double> values, long timestamp)
        {
            Group = group;
            Values = values;
            Timestamp = timestamp;
        }

        public int Group { get; set; }
        public ConcurrentDictionary<int, double> Values { get; set; }
        public long Timestamp { get; set; }
    }

    public class InperDeviceHelper : PropertyChangedBase
    {
        private static InperDeviceHelper inperDeviceHelper;
        public static InperDeviceHelper Instance
        {
            get
            {
                if (inperDeviceHelper == null)
                {
                    Interlocked.CompareExchange(ref inperDeviceHelper, new InperDeviceHelper(), null);
                }
                return inperDeviceHelper;
            }
        }
        private readonly object _DisplayQLock = new object();
        private Task frameProcTask;
        private CancellationTokenSource _updateTaskTokenSource;
        private CancellationTokenSource _frameProcTaskTokenSource;
        public Task saveDataTask;
        public CancellationTokenSource _saveDataTaskTokenSource = new CancellationTokenSource();
        private bool isFirstRecordTiem = true;
        private bool isLoop = true;
        private long _PlottingStartTime;
        private long _EventStartTime;
        private bool _eventIsFirst = true;
        public ConcurrentBag<CameraChannel> _LoopCannels = new ConcurrentBag<CameraChannel>();
        public uint[] AiChannelsConfig = new uint[4] { 0, 0, 0, 0 };
        private short[] _SwapBuffer;
        #region
        public int VisionWidth = 720;
        public int VisionHeight = 540;
        public int SelectedWaveType = 1;

        public event Action<bool> StartCollectEvent;
        public PhotometryDevice device;
        public long time = 0;

        public System.Timers.Timer _Metronome = new System.Timers.Timer();
        private bool IsInWorkingPeriod = true;
        private object _MatQLock = new object();

        public ConcurrentQueue<MarkedMat> _MatQ = new ConcurrentQueue<MarkedMat>();
        public Queue<MarkedMat> _DisplayMatQ = new Queue<MarkedMat>();

        public ConcurrentDictionary<int, Dictionary<int, Queue<double>>> FilterData { get; set; } = new ConcurrentDictionary<int, Dictionary<int, Queue<double>>>();
        public ConcurrentDictionary<int, Dictionary<int, Queue<double>>> OffsetData { get; set; } = new ConcurrentDictionary<int, Dictionary<int, Queue<double>>>();
        public ConcurrentDictionary<int, Dictionary<int, Queue<double>>> DeltaFData { get; set; } = new ConcurrentDictionary<int, Dictionary<int, Queue<double>>>();

        public BindableCollection<CameraChannel> CameraChannels { get; set; } = new BindableCollection<CameraChannel>();
        public EventChannelChart EventChannelChart { get; set; } = new EventChannelChart();

        private BindableCollection<WaveGroup> lightWaveLength = new BindableCollection<WaveGroup>();
        public BindableCollection<WaveGroup> LightWaveLength { get => lightWaveLength; set => SetAndNotify(ref lightWaveLength, value); }
        private WriteableBitmap _WBMPPreview;
        public WriteableBitmap WBMPPreview
        {
            get => _WBMPPreview;
            set => SetAndNotify(ref _WBMPPreview, value);
        }
        #endregion
        public void DeviceSet(PhotometryDevice device)
        {
            this.device = device;
        }
        public void DeviceInit()
        {
            device.DidGrabImage += Instance_OnImageGrabbed;

            bool isComplete = false;
            device.OnDevInfoUpdated += (s, ee) =>
            {
                if (!isComplete)
                {
                    device.LightSourceList.ForEach(e =>
                    {
                        try
                        {
                            bool exist = false;
                            if (e.WaveLength > 0)
                            {
                                WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == 0);
                                if (wg != null)
                                {
                                    LightWaveLength.Add(wg);
                                    device.SwitchLight((uint)wg.GroupId, true);
                                    device.SetLightPower((uint)wg.GroupId, wg.LightPower);
                                }
                                else
                                {
                                    LightWaveLength.Add(new WaveGroup() { GroupId = (int)e.LightID, WaveType = e.WaveLength + " nm" });
                                }
                                exist = true;
                            }
                            if (!exist)
                            {
                                Growl.Error("未获取到激发光");
                            }
                        }
                        catch (Exception ex)
                        {
                            App.Log.Error(ex.ToString());
                        }
                        finally
                        {
                            if (LightWaveLength.Count > 0)
                            {
                                isComplete = true;
                            }
                        }
                    });
                }
            };
            device.SyncDeviceInfo();

            device.OnDevNotification += Device_OnDevNotification;

            VisionWidth = device.GetVisionWidth();
            VisionHeight = device.GetVisionHeight();
            if (VisionWidth <= 0)
            {
                Growl.Error("Device initialization failed");
                return;
            }
            WBMPPreview = new WriteableBitmap(VisionWidth, VisionHeight, 96, 96, PixelFormats.Gray16, null);

            _Metronome.Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000 == 0 ? 1000 : InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000;
            _Metronome.Elapsed += (s, e) =>
            {
                double next_Interval;
                if (IsInWorkingPeriod)
                {
                    if (InperGlobalClass.IsRecord)
                    {
                        Task.Factory.StartNew(() => { SetIntervalData(time / 100, 0); });
                    }
                    LightWaveLength.ToList().ForEach(x =>
                    {
                        if (x.IsChecked)
                        {
                            Instance.device.SetLightPower((uint)x.GroupId, 0);
                        }
                    });
                    next_Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Interval * 1000 == 0 ? 5 * 1000 : InperGlobalClass.CameraSignalSettings.RecordMode.Interval * 1000;
                }
                else
                {
                    LightWaveLength.ToList().ForEach(x =>
                    {
                        if (x.IsChecked)
                        {
                            device.SetLightPower((uint)x.GroupId, x.LightPower);
                        }
                    });
                    next_Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000 == 0 ? 5 * 1000 : InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000;
                    if (InperGlobalClass.IsRecord)
                    {
                        Task.Factory.StartNew(() => { SetIntervalData(time / 100, 1); });
                    }
                }
                _Metronome.Interval = next_Interval;
                IsInWorkingPeriod = !IsInWorkingPeriod;
                return;
            };

            EventChannelChart.TimeSpanAxis = new TimeSpanAxis()
            {
                DrawMajorBands = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                VisibleRange = EventChannelChart.XVisibleRange
            };
            EventChannelChart.EventTimeSpanAxis = new TimeSpanAxis()
            {
                DrawMajorBands = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                VisibleRange = EventChannelChart.XVisibleRange,
                Visibility = Visibility.Collapsed
            };
            EventChannelChart.EventTimeSpanAxisFixed = new TimeSpanAxis()
            {
                DrawMajorBands = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                VisibleRange = EventChannelChart.XVisibleRange,
                Visibility = Visibility.Collapsed
            };
        }

        #region 间隔模式标记存储
        private int _IntervalScaleLock = 0;
        private void SetIntervalData(long times, int type)
        {
            if (Interlocked.Exchange(ref _IntervalScaleLock, 1) == 0)
            {
                _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                {
                    _ = App.SqlDataInit.sqlSugar.Storageable(new IntervalScale() { CreateTime = times, Type = type }).ToStorage().BulkCopy();
                });
                Interlocked.Exchange(ref _IntervalScaleLock, 0);
            }
        }
        #endregion

        private long synTime = 0;
        private void Device_OnDevNotification(object sender, DevNotificationEventArgs e)
        {
            try
            {
                DevInputNotificationEventArgs dev = e as DevInputNotificationEventArgs;

                if (InperGlobalClass.IsPreview || InperGlobalClass.IsRecord)
                {
                    if (_eventIsFirst)
                    {
                        _EventStartTime = (long)dev.Timestamp;
                        synTime = time / 100;
                        _eventIsFirst = false;
                    }
                    Input input = new Input() { ChannelId = (int)dev.IOID, Index = (int)dev.Index, CameraTime = synTime + (long)dev.Timestamp - _EventStartTime, Value = dev.Status, CreateTime = DateTime.Now };
                    _InputDataPlot.Enqueue(input);
                    Task.Run(() => { InputDataUpdateProc(); });
                    if (InperGlobalClass.IsRecord)
                    {
                        SaveInputDatas.Enqueue(input);
                        _ = Task.Run(() => { SaveEventData(); });
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public bool InitDataStruct()
        {
            try
            {
                _cameraSkipCountArray = new int[6] { 0, 0, 0, 0, 0, 0 };

                _cameraSkipCount = (int)Math.Floor(InperGlobalClass.CameraSignalSettings.Sampling / 30) - 1 < 0 ? 0 : (int)Math.Floor(InperGlobalClass.CameraSignalSettings.Sampling / 30) - 1;
                if (InperGlobalClass.CameraSignalSettings.AiSampling >= 100)
                {
                }
                FilterData.Clear();
                OffsetData.Clear();
                DeltaFData.Clear();
                foreach (var item in CameraChannels)
                {
                    _ = FilterData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                    _ = OffsetData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                    _ = DeltaFData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                    if (item.LightModes.Count > 0)
                    {
                        if (item.Filters.IsBandpass)
                        {
                            double high = item.Filters.Bandpass2 < item.Filters.Bandpass1 ? item.Filters.Bandpass1 : item.Filters.Bandpass2;
                            double low = item.Filters.Bandpass1 > item.Filters.Bandpass2 ? item.Filters.Bandpass2 : item.Filters.Bandpass1;
                            item.Filters.OnlineFilter.Bandpass(InperGlobalClass.CameraSignalSettings.Sampling, low, high);
                        }
                        if (item.Filters.IsBandstop)
                        {
                            double high = item.Filters.Bandstop2 < item.Filters.Bandstop1 ? item.Filters.Bandstop1 : item.Filters.Bandstop2;
                            double low = item.Filters.Bandstop1 > item.Filters.Bandstop2 ? item.Filters.Bandstop2 : item.Filters.Bandstop1;
                            item.Filters.OnlineFilter.Bandstop(InperGlobalClass.CameraSignalSettings.Sampling, item.Filters.Bandstop1, item.Filters.Bandstop2);
                        }
                        item.LightModes.ForEach(x =>
                        {
                            x.OffsetValue = 0;

                            if (!FilterData[item.ChannelId].ContainsKey(x.LightType))
                            {
                                FilterData[item.ChannelId].Add(x.LightType, new Queue<double>());
                            }

                            if (!OffsetData[item.ChannelId].ContainsKey(x.LightType))
                            {
                                OffsetData[item.ChannelId].Add(x.LightType, new Queue<double>());
                            }
                            if (!DeltaFData[item.ChannelId].ContainsKey(x.LightType))
                            {
                                DeltaFData[item.ChannelId].Add(x.LightType, new Queue<double>());
                            }
                        });
                    }

                }


                if (InperGlobalClass.EventSettings.Channels.Count > 0)
                {
                    InperGlobalClass.EventSettings.Channels.ForEach(x =>
                    {
                        if (x.Type == ChannelTypeEnum.Input.ToString())//x.Type == EventSettingsTypeEnum.Marker.ToString() || 
                        {
                            EventChannelChart.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = x.ChannelId, IsDigitalLine = true, DataSeries = new XyDataSeries<TimeSpan, double>() { FifoCapacity = 5000 }, Stroke = (Color)ColorConverter.ConvertFromString(x.BgColor) });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
                return false;
            }
            return true;
        }
        private void Instance_OnImageGrabbed(object sender, MarkedMat e)
        {
            try
            {
                if (InperGlobalClass.IsPreview || InperGlobalClass.IsRecord)
                {
                    if (isFirstRecordTiem)
                    {
                        _PlottingStartTime = e.Timestamp;

                        isFirstRecordTiem = false;
                    }
                    if (e.Group > -1 && _LoopCannels.Count > 0)
                    {
                        _MatQ.Enqueue(e);
                        _ = Task.Run(() => { FrameProc(); });
                    }
                }

                if (Monitor.TryEnter(_DisplayQLock))
                {
                    try
                    {
                        _DisplayMatQ.Enqueue(e);
                    }
                    finally
                    {
                        Monitor.Exit(_DisplayQLock);
                    }
                }

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public Mat _ImageShowMat = new Mat();
        public void DisplayProc()
        {
            _SwapBuffer = new short[VisionWidth * VisionHeight];

            while (true)
            {
                Thread.Sleep(16);
                if (_DisplayMatQ.Count() == 0)
                {
                    continue;
                }

                Monitor.Enter(_DisplayQLock);
                MarkedMat[] mmats = _DisplayMatQ.ToArray();
                _DisplayMatQ.Clear();
                Monitor.Exit(_DisplayQLock);

                _ImageShowMat = mmats.Last().ImageMat;
                if (mmats.Count() > 0 && (mmats.Last().Group == SelectedWaveType || InperGlobalClass.IsPreview || InperGlobalClass.IsRecord))
                {
                    unsafe
                    {
                        Marshal.Copy(_ImageShowMat.Data, _SwapBuffer, 0, VisionWidth * VisionHeight);
                        System.Windows.Application.Current?.Dispatcher.Invoke(new Action(() =>
                        {
                            _WBMPPreview.Lock();
                            Marshal.Copy(_SwapBuffer, 0, _WBMPPreview.BackBuffer, VisionWidth * VisionHeight);
                            _WBMPPreview.AddDirtyRect(new Int32Rect(0, 0, VisionWidth, VisionHeight));
                            _WBMPPreview.Unlock();
                        }));
                    }
                }
            }
        }
        private int[] _cameraSkipCountArray = new int[4];
        private int _cameraSkipCount = 0;
        private int _FrameProcLock = 0;
        public void FrameProc()
        {
            if (Interlocked.Exchange(ref _FrameProcLock, 1) == 0)
            {
                while (_MatQ.TryDequeue(out MarkedMat m))
                {
                    long ts = m.Timestamp - _PlottingStartTime;
                    time = ts;
                    ConcurrentBag<string> values = new ConcurrentBag<string>();
                    ConcurrentDictionary<int, double> _poltValues = new ConcurrentDictionary<int, double>();
                    foreach (var mask in _LoopCannels)
                    {

                        double r = (double)m.ImageMat.Mean(mask.Mask) / 655.35;

                        if (mask.Offset)
                        {
                            r -= Offset(mask, m.Group, r);
                        }

                        if (mask.Filters.IsSmooth)
                        {
                            r = Smooth(mask, m.Group, r);
                        }

                        if (mask.IsDeltaFCalculate)
                        {
                            Task.Run(() =>
                            {
                                DeltaFCalculate(mask.ChannelId, m.Group, r, ts / 100);
                            });
                        }

                        if (mask.Filters.IsBandpass)
                        {
                            r = mask.Filters.OnlineFilter.GetBandpassValue(r, m.Group);
                        }
                        if (mask.Filters.IsBandstop)
                        {
                            r = mask.Filters.OnlineFilter.GetBandstopValue(r, m.Group);
                        }
                        values.Add(mask.ChannelId + "," + System.Convert.ToBase64String(BitConverter.GetBytes(r)));
                        _poltValues.TryAdd(mask.ChannelId, r);

                    }
                    if (_cameraSkipCountArray[m.Group] >= _cameraSkipCount)
                    {
                        _CameraDataPlot.Enqueue(new PlotData(m.Group, _poltValues, ts));
                        Task.Run(() => { CameraDataUpdateProc(); });
                        _cameraSkipCountArray[m.Group] = 0;
                    }
                    else
                    {
                        _cameraSkipCountArray[m.Group]++;
                    }

                    AllChannelRecord allChannelRecord = new AllChannelRecord() { CameraTime = ts, CreateTime = DateTime.Now, Type = m.Group, Value = string.Join(" ", values.ToArray()) };
                    if (InperGlobalClass.IsRecord)
                    {
                        InperGlobalClass.RunTime = new DateTime(ts / 100, DateTimeKind.Unspecified);
                        SaveDatas.Enqueue(allChannelRecord); //mask.ChannelId, m.Group, r, ts, mask.Type
                        Task.Run(() => { SaveImageData(); });
                    }
                }
                _ = Interlocked.Exchange(ref _FrameProcLock, 0);
                if (!_MatQ.IsEmpty)
                {
                    Task.Run(() => { FrameProc(); });
                }
            }
        }

        #region usb data 
        private ConcurrentQueue<UsbAdData> _AdDatas = new ConcurrentQueue<UsbAdData>();
        public Dictionary<int, long> _adPreTime = new Dictionary<int, long>();
        private int _adConvLock = 0;
        public unsafe void UsbAdProc()
        {
            try
            {
                //double aiSampling = InperGlobalClass.CameraSignalSettings.AiSampling;
                while (isLoop)
                {
                    if (Interlocked.Exchange(ref _adConvLock, 1) == 0)
                    {
                        if (device._CS._UARTA.ADPtrQueues.TryDequeue(out byte[] e))
                        {
                            fixed (byte* pb = &e[0])
                            {
                                AdSamplingConv(InperGlobalClass.CameraSignalSettings.AiSampling, (IntPtr)pb);
                            }
                        }
                        Interlocked.Exchange(ref _adConvLock, 0);
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void AdSamplingConv(double sampling, IntPtr ptr)
        {

            if (sampling >= 10000)
            {
                UsbAdDataStru512 data = Marshal.PtrToStructure<UsbAdDataStru512>(ptr);

                this.UsbDataAppend(data.Channel, data.Time, data.Values);

            }
            else if (sampling >= 5000)
            {
                UsbAdDataStru256 data = Marshal.PtrToStructure<UsbAdDataStru256>(ptr);

                this.UsbDataAppend(data.Channel, data.Time, data.Values);

            }
            else if (sampling >= 1000)
            {
                UsbAdDataStru128 data = Marshal.PtrToStructure<UsbAdDataStru128>(ptr);

                this.UsbDataAppend(data.Channel, data.Time, data.Values);

            }
            else if (sampling >= 500)
            {
                UsbAdDataStru64 data = Marshal.PtrToStructure<UsbAdDataStru64>(ptr);

                this.UsbDataAppend(data.Channel, data.Time, data.Values);
            }
            else if (sampling >= 100)
            {
                UsbAdDataStru32 data = Marshal.PtrToStructure<UsbAdDataStru32>(ptr);
                this.UsbDataAppend(data.Channel, data.Time, data.Values);
            }
            else if (sampling >= 50)
            {
                UsbAdDataStru16 data = Marshal.PtrToStructure<UsbAdDataStru16>(ptr);

                this.UsbDataAppend(data.Channel, data.Time, data.Values);
            }
            else if (sampling >= 30)
            {
                UsbAdDataStru8 data = Marshal.PtrToStructure<UsbAdDataStru8>(ptr);

                this.UsbDataAppend(data.Channel, data.Time, data.Values);
            }
            else if (sampling >= 16)
            {
                UsbAdDataStru4 data = Marshal.PtrToStructure<UsbAdDataStru4>(ptr);

                this.UsbDataAppend(data.Channel, data.Time, data.Values);
            }
            //else if (sampling >= 8)
            else
            {
                UsbAdDataStru2 data = Marshal.PtrToStructure<UsbAdDataStru2>(ptr);

                this.UsbDataAppend(data.Channel, data.Time, data.Values);
            }
        }
        public double adFsTimeInterval = 0;
        private void UsbDataAppend(uint channel, long time, short[] values)
        {
            UsbAdData usbAdData = new UsbAdData()
            {
                ChannelId1 = channel * 2 - 1,
                ChannelId2 = channel * 2,
                Time = time,
                Times1 = new List<TimeSpan>(),
                Times2 = new List<TimeSpan>(),
                Values1 = new List<double>(),
                Values2 = new List<double>()
            };

            CameraChannel ccn1 = aiChannels.FirstOrDefault(x => x.ChannelId == ((int)(channel * 2 - 1) + 100));
            CameraChannel ccn2 = aiChannels.FirstOrDefault(x => x.ChannelId == ((int)(channel * 2) + 100));
            int count = 0;

            for (int i = 0; i < values.Length; i += 2)
            {
                count++;
                if (ccn1 != null)
                {
                    double v1 = (values[i] * 2.95 * 24.3520 / 4096) - 5.1;
                    TimeSpan ts1 = new TimeSpan(_adPreTime[(int)(channel * 2 - 1) + 100] + (long)(adFsTimeInterval * count * Math.Pow(10, 7)));
                    v1 = Math.Round(GetAiFilterValue(ccn1, v1, ts1.Ticks), 5);
                    usbAdData.Values1.Add(v1);
                    usbAdData.Times1.Add(ts1);
                }
                if (ccn2 != null)
                {
                    double v2 = (values[i + 1] * 2.95 * 24.3520 / 4096) - 5.1;
                    TimeSpan ts2 = new TimeSpan(_adPreTime[(int)(channel * 2) + 100] + (long)(adFsTimeInterval * count * Math.Pow(10, 7)));
                    v2 = Math.Round(GetAiFilterValue(ccn2, v2, ts2.Ticks), 5);
                    usbAdData.Values2.Add(v2);
                    usbAdData.Times2.Add(ts2);
                }
            }
            if (usbAdData.Values1.Count > 0)
            {
                _adPreTime[(int)((channel * 2) - 1) + 100] = usbAdData.Times1.Last().Ticks;
                if (InperGlobalClass.IsRecord)
                {
                    SaveAnalogDatas.Enqueue(new AnalogRecord() { ChannelId = (int)usbAdData.ChannelId1, CameraTime = time, Value = string.Join(",", usbAdData.Values1), CreateTime = DateTime.Now });
                    Task.Run(() => { SaveAnalogData(); });
                }
            }
            if (usbAdData.Values2.Count > 0)
            {
                _adPreTime[(int)(channel * 2) + 100] = usbAdData.Times2.Last().Ticks;
                if (InperGlobalClass.IsRecord)
                {
                    SaveAnalogDatas.Enqueue(new AnalogRecord() { ChannelId = (int)usbAdData.ChannelId2, CameraTime = time, Value = string.Join(",", usbAdData.Values2), CreateTime = DateTime.Now });
                    Task.Run(() => { SaveAnalogData(); });
                }
            }
            _AdDatas.Enqueue(usbAdData);
            Task.Run(() => { UsbDataPlot(); });
        }
        private int _UsbPlotLocked = 0;
        public ConcurrentBag<CameraChannel> aiChannels = new ConcurrentBag<CameraChannel>();
        private void UsbDataPlot()
        {
            if (Interlocked.Exchange(ref _UsbPlotLocked, 1) == 0)
            {
                while (_AdDatas.TryDequeue(out UsbAdData adData))
                {
                    if (adData.Values1.Count > 0)
                    {
                        AiDataPlot((int)adData.ChannelId1, adData.Values1, adData.Times1);
                    }
                    if (adData.Values2.Count > 0)
                    {
                        AiDataPlot((int)adData.ChannelId2, adData.Values2, adData.Times2);
                    }
                }
                Interlocked.Exchange(ref _UsbPlotLocked, 0);
                if (!_AdDatas.IsEmpty)
                {
                    Task.Run(() => { UsbDataPlot(); });
                }
            }
        }
        private void AiDataPlot(int id, List<double> values, List<TimeSpan> timeSpans)
        {
            try
            {
                CameraChannel ccn = aiChannels.FirstOrDefault(x => x.ChannelId == (id + 100));
                if (ccn != null)
                {
                    XyDataSeries<TimeSpan, double> ds = ccn.RenderableSeries.First().DataSeries as XyDataSeries<TimeSpan, double>;
                    using (ds.SuspendUpdates())
                    {
                        if ((TimeSpan)ds.XMax < timeSpans.First())
                        {
                            ds.Append(timeSpans, values);
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }
        private double GetAiFilterValue(CameraChannel channel, double value, long ticks)
        {
            if (channel.Offset)
            {
                value -= Offset(channel, -1, value);
            }
            if (channel.Filters.IsSmooth)
            {
                value = Smooth(channel, -1, value);
            }
            if (channel.IsDeltaFCalculate)
            {
                Task.Run(() =>
                {
                    DeltaFCalculate(channel.ChannelId, -1, value, ticks);
                });
            }
            if (channel.Filters.IsBandpass)
            {
                value = channel.Filters.OnlineFilter.GetBandpassValue(value, 0);
            }
            if (channel.Filters.IsBandstop)
            {
                value = channel.Filters.OnlineFilter.GetBandstopValue(value, 0);
            }
            return value;
        }
        #endregion

        #region 滤波 offset smooth
        private double Offset(CameraChannel cameraChannel, int group, double r)
        {
            //double value = r;
            LightMode<TimeSpan, double> offsetValue = cameraChannel.LightModes.FirstOrDefault(x => x.LightType == group);
            if (OffsetData.ContainsKey(cameraChannel.ChannelId))
            {
                if (OffsetData[cameraChannel.ChannelId].ContainsKey(group))
                {
                    if (OffsetData[cameraChannel.ChannelId][group].Count < cameraChannel.OffsetWindowSize)
                    {
                        OffsetData[cameraChannel.ChannelId][group].Enqueue(r);
                        //_ = OffsetData[cameraChannel.ChannelId][group].Dequeue();
                        if (OffsetData[cameraChannel.ChannelId][group].Count == cameraChannel.OffsetWindowSize)
                        {
                            offsetValue.OffsetValue = OffsetData[cameraChannel.ChannelId][group].ToList().Average();
                        }
                    }
                    else
                    {
                        return offsetValue.OffsetValue;
                    }
                }
            }
            return r;
        }
        public int _InterSmooth = 0;
        private double Smooth(CameraChannel cameraChannel, int group, double r)
        {
            double val = r;
            if (Interlocked.Exchange(ref _InterSmooth, 1) == 0)
            {
                if (FilterData.ContainsKey(cameraChannel.ChannelId))
                {
                    if (FilterData[cameraChannel.ChannelId].ContainsKey(group))
                    {
                        FilterData[cameraChannel.ChannelId][group].Enqueue(r);
                        if (FilterData[cameraChannel.ChannelId][group].Count > cameraChannel.Filters.Smooth)
                        {
                            _ = FilterData[cameraChannel.ChannelId][group].Dequeue();
                            val = FilterData[cameraChannel.ChannelId][group].ToList().Average();
                        }
                    }
                }
                Interlocked.Exchange(ref _InterSmooth, 0);
            }
            return val;
        }
        private int deltaFObj = 0;
        public List<EventChannelJson> DeltaFCalculateList = new List<EventChannelJson>();
        private void DeltaFCalculate(int cid, int group, double r, long ts)
        {
            try
            {
                if (Interlocked.Exchange(ref deltaFObj, 1) == 0)
                {
                    DeltaFCalculateList.ForEach(cameraChannel =>
                    {
                        if (cameraChannel.Type == ChannelTypeEnum.Output.ToString())
                        {
                            if (cameraChannel.Condition.ChannelId == cid && cameraChannel.LightIndex == group)
                            {
                                DeltaFData[cid][group].Enqueue(r);
                                if (DeltaFData[cid][group].Count > cameraChannel.WindowSize)
                                {
                                    _ = DeltaFData[cid][group].Dequeue();
                                    double deltaF = Math.Abs(r - DeltaFData[cid][group].ToList().Average()) / DeltaFData[cid][group].ToList().Average() * 100;
                                    if (deltaF >= cameraChannel.DeltaF)
                                    {
                                        SetMarkers(new BaseMarker()
                                        {
                                            ChannelId = cameraChannel.ChannelId,
                                            Color = cameraChannel.BgColor,
                                            IsIgnore = cameraChannel.Type == ChannelTypeEnum.Output.ToString() ? false : true,
                                            RefractoryPeriod = cameraChannel.RefractoryPeriod,
                                            DeltaF = deltaF,
                                            CameraTime = ts,
                                            Name = cameraChannel.Name,
                                            Type = cameraChannel.Condition.Type,
                                            ConditionId = cid,
                                            CreateTime = DateTime.Now
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (cameraChannel.ChannelId == cid && cameraChannel.LightIndex == group && cameraChannel.Type != ChannelTypeEnum.Manual.ToString())
                            {
                                DeltaFData[cid][group].Enqueue(r);
                                if (DeltaFData[cid][group].Count > cameraChannel.WindowSize)
                                {
                                    _ = DeltaFData[cid][group].Dequeue();
                                    double deltaF = Math.Abs(r - DeltaFData[cid][group].ToList().Average()) / DeltaFData[cid][group].ToList().Average() * 100;
                                    if (deltaF >= cameraChannel.DeltaF)
                                    {
                                        SetMarkers(new BaseMarker()
                                        {
                                            ChannelId = cameraChannel.ChannelId,
                                            Color = cameraChannel.BgColor,
                                            IsIgnore = cameraChannel.Type == ChannelTypeEnum.Output.ToString() ? false : true,
                                            RefractoryPeriod = cameraChannel.RefractoryPeriod,
                                            DeltaF = deltaF,
                                            CameraTime = ts,
                                            Name = cameraChannel.Name,
                                            Type = cameraChannel.Type,
                                            ConditionId = cid,
                                            CreateTime = DateTime.Now
                                        });
                                    }
                                }
                            }
                        }
                    });
                    Interlocked.Exchange(ref deltaFObj, 0);
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
                Interlocked.Exchange(ref deltaFObj, 0);
            }
        }
        #endregion

        #region 数据存储
        #region 图像信号数据存储
        private ConcurrentQueue<AllChannelRecord> SaveDatas = new ConcurrentQueue<AllChannelRecord>();
        private List<AllChannelRecord> _SaveCameraCache = new List<AllChannelRecord>();
        private int _ImageDataLock = 0;
        private void SaveImageData()
        {
            if (Interlocked.Exchange(ref _ImageDataLock, 1) == 0)
            {
                while (SaveDatas.TryDequeue(out AllChannelRecord data))
                {
                    _SaveCameraCache.Add(data);
                    if (_SaveCameraCache.Count > InperGlobalClass.CameraSignalSettings.Sampling * 10)
                    {
                        AllChannelRecord[] datas = new AllChannelRecord[_SaveCameraCache.Count];
                        _SaveCameraCache.CopyTo(datas);
                        _SaveCameraCache.Clear();
                        ImageDataCloneToSave(datas);
                    }
                }
                _ = Interlocked.Exchange(ref _ImageDataLock, 0);
                if (!SaveDatas.IsEmpty)
                {
                    Task.Run(() => { SaveImageData(); });
                }
            }
        }
        private void ImageDataCloneToSave(AllChannelRecord[] datas)
        {
            _ = Task.Factory.StartNew(() =>
              {
                  IEnumerable<IGrouping<int, AllChannelRecord>> res = datas.GroupBy(x => x.Type);

                  Parallel.ForEach(res, data =>
                  {
                      _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                      {
                          //_ = App.SqlDataInit.sqlSugar.Insertable(datas).ExecuteCommand();
                          _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As(nameof(AllChannelRecord) + data.Key).ToStorage().BulkCopy();
                      });
                  });
              });
        }
        #endregion

        #region input 数据存储
        private ConcurrentQueue<Input> SaveInputDatas = new ConcurrentQueue<Input>();
        private List<Input> _SaveInputCache = new List<Input>();
        private int _InputLocked = 0;
        private void SaveEventData()
        {
            if (Interlocked.Exchange(ref _InputLocked, 1) == 0)
            {
                while (SaveInputDatas.TryDequeue(out Input data))
                {
                    _SaveInputCache.Add(data);
                    if (_SaveInputCache.Count > 1000)
                    {
                        Input[] inputs = new Input[_SaveInputCache.Count];
                        _SaveInputCache.CopyTo(inputs);
                        _SaveInputCache.Clear();
                        InputDataCloneToSave(inputs);
                    }
                }
                _ = Interlocked.Exchange(ref _InputLocked, 0);
                if (!SaveInputDatas.IsEmpty)
                {
                    _ = Task.Run(() => { SaveEventData(); });
                }
            }
        }
        private void InputDataCloneToSave(Input[] datas)
        {
            Task.Factory.StartNew(() =>
            {
                IEnumerable<IGrouping<int, Input>> res = datas.GroupBy(x => x.ChannelId);
                Parallel.ForEach(res, data =>
                {
                    _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                    {
                        //_ = App.SqlDataInit.sqlSugar.Insertable(data.ToArray()).AS("Input" + data.Key).ExecuteCommand();
                        _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As("Input" + data.Key).ToStorage().BulkCopy();
                    });
                });
            });
        }
        #endregion

        #region ad数据存储
        private ConcurrentQueue<AnalogRecord> SaveAnalogDatas = new ConcurrentQueue<AnalogRecord>();
        private List<AnalogRecord> _SaveAnalogCache = new List<AnalogRecord>();
        private int _AnalogLocked = 0;
        private void SaveAnalogData()
        {
            if (Interlocked.Exchange(ref _AnalogLocked, 1) == 0)
            {
                while (SaveAnalogDatas.TryDequeue(out AnalogRecord data))
                {
                    _SaveAnalogCache.Add(data);
                    if (_SaveAnalogCache.Count > InperGlobalClass.CameraSignalSettings.AiSampling * 5)
                    {
                        AnalogRecord[] analogs = new AnalogRecord[_SaveAnalogCache.Count];
                        _SaveAnalogCache.CopyTo(analogs);
                        _SaveAnalogCache.Clear();
                        AnalogDataCloneToSave(analogs);
                    }
                }
                Interlocked.Exchange(ref _AnalogLocked, 0);
                if (!SaveAnalogDatas.IsEmpty)
                {
                    Task.Run(() => { SaveAnalogData(); });
                }
            }
        }
        private void AnalogDataCloneToSave(AnalogRecord[] datas)
        {
            Task.Factory.StartNew(() =>
            {
                IEnumerable<IGrouping<int, AnalogRecord>> res = datas.GroupBy(x => x.ChannelId);
                Parallel.ForEach(res, data =>
                {
                    _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                    {
                        //_ = App.SqlDataInit.sqlSugar.Insertable(data.ToArray()).AS(App.SqlDataInit.RecordTablePairs[SignalSettingsTypeEnum.Analog.ToString() + (100 + data.Key)]).ExecuteCommand();
                        _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As(App.SqlDataInit.RecordTablePairs[SignalSettingsTypeEnum.Analog.ToString() + (100 + data.Key)]).ToStorage().BulkCopy();
                    });
                });
            });
        }
        #endregion
        #endregion

        #region 数据渲染
        #region 相机数据渲染
        private ConcurrentQueue<PlotData> _CameraDataPlot = new ConcurrentQueue<PlotData>();
        private int _CameraPlotLocked = 0;
        private void CameraDataUpdateProc()
        {
            if (Interlocked.Exchange(ref _CameraPlotLocked, 1) == 0)
            {
                while (_CameraDataPlot.TryDequeue(out PlotData record))
                {
                    Parallel.ForEach(record.Values, val =>
                    {
                        try
                        {
                            IRenderableSeriesViewModel render = _LoopCannels.First(x => x.ChannelId == val.Key).RenderableSeries.FirstOrDefault(r => (int)(r as LineRenderableSeriesViewModel).Tag == record.Group);
                            if (render != null)
                            {
                                using (render.DataSeries.SuspendUpdates())
                                {
                                    (render.DataSeries as XyDataSeries<TimeSpan, double>).Append(new TimeSpan(record.Timestamp / 100), val.Value);
                                }
                            }
                        }
                        catch
                        {

                        }
                    });
                }
                Interlocked.Exchange(ref _CameraPlotLocked, 0);
                if (!_CameraDataPlot.IsEmpty)
                {
                    Task.Run(() => { CameraDataUpdateProc(); });
                }
            }
        }
        #endregion

        #region input数据渲染
        private ConcurrentQueue<Input> _InputDataPlot = new ConcurrentQueue<Input>();
        private int _InputPlotLocked = 0;
        private void InputDataUpdateProc()
        {
            if (Interlocked.Exchange(ref _InputPlotLocked, 1) == 0)
            {
                while (_InputDataPlot.TryDequeue(out Input input))
                {
                    if (EventChannelChart.RenderableSeries.Count > 0)
                    {
                        _ = Parallel.ForEach(EventChannelChart.RenderableSeries, item =>
                        {
                            try
                            {
                                int id = int.Parse((item as LineRenderableSeriesViewModel).Tag.ToString());
                                if (input.ChannelId == id)
                                {
                                    TimeSpan time = new TimeSpan(input.CameraTime);
                                    using (item.DataSeries.SuspendUpdates())
                                    {
                                        if (((TimeSpan)item.DataSeries.XMax) < time)
                                        {
                                            (item.DataSeries as XyDataSeries<TimeSpan, double>).Append(time, input.Value);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                    }
                }
                Interlocked.Exchange(ref _InputPlotLocked, 0);
                if (!_InputDataPlot.IsEmpty)
                {
                    Task.Run(() => { InputDataUpdateProc(); });
                }
            }
        }
        #endregion

        #endregion

        #region marker相关
        public ConcurrentQueue<BaseMarker> BaseMarkers = new ConcurrentQueue<BaseMarker>();
        private int _DrawMarker = 0;
        public void SetMarkers(BaseMarker baseMarker)
        {
            BaseMarkers.Enqueue(baseMarker);
            Task.Run(() => { DrawMarkers(); });
        }
        public void DrawMarkers()
        {
            try
            {
                if (Interlocked.Exchange(ref _DrawMarker, 1) == 0)
                {
                    while (BaseMarkers.TryDequeue(out BaseMarker marker))
                    {
                        bool isAddAnnotation = true;
                        TimeSpan _time = new TimeSpan(marker.CameraTime);

                        if (EventChannelChart.Annotations.Count > 0 && marker.RefractoryPeriod > 0)
                        {
                            IAnnotationViewModel obj = EventChannelChart.Annotations.LastOrDefault((x) => (x as VerticalLineAnnotationViewModel).LabelValue.Equals(marker.Name));
                            if (obj != null)
                            {
                                TimeSpan tick = _time.Subtract(new TimeSpan((long)obj.X1));
                                if (tick.TotalMilliseconds < marker.RefractoryPeriod * 1000)
                                {
                                    isAddAnnotation = false;
                                }
                            }
                        }
                        if (isAddAnnotation)
                        {
                            if (!marker.IsIgnore)
                            {
                                Task.Run(() =>
                                {
                                    device.OuputIO((uint)marker.ChannelId, 1);
                                    Thread.Sleep(20);
                                    device.OuputIO((uint)marker.ChannelId, 0);
                                });
                            }
                            if (EventChannelChart.Annotations.Count > 500)
                            {
                                EventChannelChart.Annotations.RemoveAt(0);
                            }
                            EventChannelChart.Annotations.Add(new VerticalLineAnnotationViewModel()
                            {
                                VerticalAlignment = VerticalAlignment.Stretch,
                                FontSize = 12,
                                ShowLabel = InperGlobalClass.EventPanelProperties.DisplayNameVisible,
                                Stroke = (Color)ColorConverter.ConvertFromString(marker.Color),
                                LabelValue = marker.Name,
                                LabelTextFormatting = "12",
                                LabelPlacement = LabelPlacement.Left,
                                LabelsOrientation = System.Windows.Controls.Orientation.Vertical,
                                StrokeThickness = 1,
                                X1 = marker.CameraTime
                            });
                            if (InperGlobalClass.IsRecord || marker.Type == ChannelTypeEnum.Stop.ToString())
                            {
                                if (marker.IsIgnore)//marker
                                {
                                    _ = App.SqlDataInit?.sqlSugar.UseTran(() =>
                                      {
                                          App.SqlDataInit.sqlSugar.Insertable(marker).AS(nameof(Marker)).ExecuteCommand();
                                      });
                                }
                                else//output
                                {
                                    _ = App.SqlDataInit?.sqlSugar.UseTran(() =>
                                      {
                                          App.SqlDataInit.sqlSugar.Insertable(marker).AS(nameof(OutputMarker)).ExecuteCommand();
                                      });
                                }
                            }
                        }
                    }
                    Interlocked.Exchange(ref _DrawMarker, 0);
                    if (!BaseMarkers.IsEmpty)
                    {
                        Task.Run(() => { DrawMarkers(); });
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
                Interlocked.Exchange(ref _DrawMarker, 0);
            }
        }
        #endregion

        #region start & stop
        public void StopPlot()
        {
            try
            {
                if (isAdstart)
                {
                    device.RemoveAdSampling();
                }
                isFirstRecordTiem = false; isLoop = false; isAdstart = false;
                time = 0;
                _frameProcTaskTokenSource.Cancel();
                _updateTaskTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void StopCollect(CancellationTokenSource tokenSource)
        {
            try
            {
                _saveDataTaskTokenSource.Cancel();
                if (CameraChannels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Camera.ToString()) != null)
                {
                    if (SaveDatas.Count > 0)
                    {
                        IEnumerable<AllChannelRecord> items = _SaveCameraCache.Concat(SaveDatas.ToList());
                        SaveDatas = new ConcurrentQueue<AllChannelRecord>();
                        Parallel.ForEach(items.GroupBy(x => x.Type), data =>
                          {
                              _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                              {
                                  _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As(nameof(AllChannelRecord) + data.Key).ToStorage().BulkCopy();
                              });
                          });
                    }
                    else if (_SaveCameraCache.Count > 0)
                    {
                        IEnumerable<IGrouping<int, AllChannelRecord>> res = _SaveCameraCache.GroupBy(x => x.Type);
                        Parallel.ForEach(res, data =>
                        {
                            _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                            {
                                _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As(nameof(AllChannelRecord) + data.Key).ToStorage().BulkCopy();
                            });
                        });
                    }
                    _SaveCameraCache.Clear();
                    _CameraDataPlot = new ConcurrentQueue<PlotData>();
                }
                if (CameraChannels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Analog.ToString()) != null)
                {
                    if (SaveAnalogDatas.Count > 0)
                    {
                        var items = _SaveAnalogCache.Concat(SaveAnalogDatas.ToList());
                        SaveAnalogDatas = new ConcurrentQueue<AnalogRecord>();
                        Parallel.ForEach(items.GroupBy(x => x.ChannelId), data =>
                          {
                              _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                              {
                                  _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As(App.SqlDataInit.RecordTablePairs[SignalSettingsTypeEnum.Analog.ToString() + (100 + data.Key)]).ToStorage().BulkCopy();
                              });
                          });
                    }
                    else if (_SaveAnalogCache.Count > 0)
                    {
                        IEnumerable<IGrouping<int, AnalogRecord>> res = _SaveAnalogCache.GroupBy(x => x.ChannelId);
                        Parallel.ForEach(res, data =>
                        {
                            _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                            {
                                _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As(App.SqlDataInit.RecordTablePairs[SignalSettingsTypeEnum.Analog.ToString() + (100 + data.Key)]).ToStorage().BulkCopy();
                            });
                        });
                    }
                    _SaveAnalogCache.Clear();
                    _AdDatas = new ConcurrentQueue<UsbAdData>();
                }
                if (InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Input.ToString()) != null)
                {
                    if (SaveInputDatas.Count > 0)
                    {
                        IEnumerable<Input> items = _SaveInputCache.Concat(SaveInputDatas.ToList());
                        SaveInputDatas = new ConcurrentQueue<Input>();
                        Parallel.ForEach(items.GroupBy(x => x.ChannelId), data =>
                          {
                              _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                              {
                                  _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As("Input" + data.Key).ToStorage().BulkCopy();
                              });
                          });
                    }
                    else if (_SaveInputCache.Count > 0)
                    {
                        IEnumerable<IGrouping<int, Input>> res = _SaveInputCache.GroupBy(x => x.ChannelId);
                        Parallel.ForEach(res, data =>
                        {
                            _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                            {
                                _ = App.SqlDataInit.sqlSugar.Storageable(data.ToList()).As("Input" + data.Key).ToStorage().BulkCopy();
                            });
                        });
                    }
                    _SaveInputCache.Clear();
                    _InputDataPlot = new ConcurrentQueue<Input>();
                }
                _saveDataTaskTokenSource = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            finally
            {
                tokenSource.Cancel();
                device.AdIsCollect(false);
            }
        }
        public bool isAdstart = false;
        public void StartCollect()
        {
            try
            {
                _ = device.SetExposure(InperGlobalClass.CameraSignalSettings.Exposure);
                InperGlobalClass.SetSampling(InperGlobalClass.CameraSignalSettings.Sampling);

                _adPreTime.Clear();
                adFsTimeInterval = 1 / InperGlobalClass.CameraSignalSettings.AiSampling;
                aiChannels = new ConcurrentBag<CameraChannel>(Instance.CameraChannels.ToList().FindAll(x => x.Type == ChannelTypeEnum.Analog.ToString()));
                _LoopCannels = new ConcurrentBag<CameraChannel>();
                Instance.CameraChannels.ToList().ForEach(x => { if (x.Type == ChannelTypeEnum.Camera.ToString()) { _LoopCannels.Add(x); } });
                DeltaFCalculateList = new List<EventChannelJson>();
                InperGlobalClass.EventSettings.Channels.ForEach(x =>
                {
                    if (x.Type == ChannelTypeEnum.Camera.ToString() || x.Type == ChannelTypeEnum.Analog.ToString() || x.Type == ChannelTypeEnum.Output.ToString())
                    {
                        if (x.Type == ChannelTypeEnum.Output.ToString())
                        {
                            if (x.Condition != null && (x.Condition.Type == ChannelTypeEnum.Camera.ToString() || x.Condition.Type == ChannelTypeEnum.Analog.ToString()))
                            {
                                DeltaFCalculateList.Add(x);

                                if (CameraChannels.FirstOrDefault(chn => chn.ChannelId == x.Condition.ChannelId) is CameraChannel channel)
                                {
                                    channel.IsDeltaFCalculate = true;
                                }
                            }
                        }
                        else
                        {
                            DeltaFCalculateList.Add(x);
                            if (CameraChannels.FirstOrDefault(chn => chn.ChannelId == x.ChannelId) is CameraChannel channel)
                            {
                                channel.IsDeltaFCalculate = true;
                            }
                        }
                    }
                });
                AiChannelsConfig = new uint[4] { 0, 0, 0, 0 };
                CameraChannels.ToList().ForEach(x =>
                {
                    if (x.Type == ChannelTypeEnum.Camera.ToString())
                    {
                        x.RenderableSeries.Clear();
                        x.LightModes.ForEach(l =>
                        {
                            LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = l.LightType, DataSeries = l.XyDataSeries, Stroke = l.WaveColor.Color, YAxisId = "Ch" + l.LightType };
                            line.DataSeries.FifoCapacity = 10 * 60 * (int)InperGlobalClass.CameraSignalSettings.Sampling;
                            x.RenderableSeries.Add(line);
                        });
                    }
                    if (x.Type == ChannelTypeEnum.Analog.ToString())
                    {
                        isAdstart = true;
                        AiSettingFunc(x.ChannelId, 1);
                        x.RenderableSeries.ToList().ForEach(line =>
                        {
                            (line.DataSeries as XyDataSeries<TimeSpan, double>).Clear();
                            line.DataSeries.FifoCapacity = 10 * 60 * (int)InperGlobalClass.CameraSignalSettings.AiSampling;
                        });
                        _adPreTime.Add(x.ChannelId, 0);
                    }
                });
                InperGlobalClass.EventSettings.Channels.ForEach(x =>
                {
                    if (x.Type == ChannelTypeEnum.Input.ToString())
                    {
                        Instance.device.SetIOMode((uint)x.ChannelId, IOMode.IOM_INPUT);
                        //Thread.Sleep(50);
                    }
                    if (x.Type == ChannelTypeEnum.Output.ToString())
                    {
                        DioBindingLightSet(x);
                    }
                });

                if (isAdstart)
                {
                    AiConfigSend();
                }
                //确保绘制间隔
                if (InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval)
                {
                    _Metronome.Stop();
                    IsInWorkingPeriod = true;
                    _Metronome.Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000 == 0 ? 1000 : InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000;
                    _Metronome.Start();
                }

                _eventIsFirst = true; isFirstRecordTiem = true; isLoop = true;
                _updateTaskTokenSource = new CancellationTokenSource();
                _frameProcTaskTokenSource = new CancellationTokenSource();
                frameProcTask = Task.Factory.StartNew(() => { FrameProc(); }, _frameProcTaskTokenSource.Token);
                if (isAdstart)
                {
                    _ = Task.Factory.StartNew(() => { UsbAdProc(); });
                    device.AdIsCollect(true);
                }

                StartCollectEvent?.Invoke(true);
                //saveDataTask = Task.Factory.StartNew(() => { SaveDateProc(); });
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void DioBindingLightSet(EventChannelJson json)
        {
            Instance.device.SetIOMode((uint)json.ChannelId, IOMode.IOM_OUTPUT);
            //Thread.Sleep(50);
            //binding dio 输出
            if (json.Condition != null)
            {
                if (json.Condition.Type == ChannelTypeEnum.Light.ToString() || json.Condition.Type == ChannelTypeEnum.AfterExcitation.ToString())
                {
                    if (json.Condition.Type == ChannelTypeEnum.AfterExcitation.ToString())
                    {
                        device.SwitchLight((uint)json.Condition.ChannelId, true);
                    }
                    List<byte> bindDios = new List<byte>();

                    bindDios.Add((byte)json.Condition.ChannelId);
                    string ob = "00000000";
                    ob = ob.Insert(7 - json.ChannelId, "1").Remove(8, 1);
                    bindDios.Add(System.Convert.ToByte(ob, 2));

                    device.SetBindDio(bindDios);
                }
            }
        }
        public void AiSettingFunc(int channelId, uint statu)//statu=1 是配置  =0 是取消配置
        {
            if (channelId <= 102)
            {
                AiStatuCheck(102, channelId, statu);
            }
            else if (channelId <= 104)
            {
                AiStatuCheck(104, channelId, statu);
            }
            else if (channelId <= 106)
            {
                AiStatuCheck(106, channelId, statu);
            }
            else
            {
                AiStatuCheck(108, channelId, statu);
            }
        }
        private void AiStatuCheck(int level, int channelId, uint statu)
        {
            if (statu == 0)
            {
                if (channelId != level)
                {
                    var cn = CameraChannels.FirstOrDefault(x => x.ChannelId == channelId + 1);
                    if (cn == null)
                    {
                        AiChannelsConfig[0] = statu;
                    }
                }
                else
                {
                    var cn = CameraChannels.FirstOrDefault(x => x.ChannelId == channelId - 1);
                    if (cn == null)
                    {
                        AiChannelsConfig[0] = statu;
                    }
                }
            }
            else
            {
                AiChannelsConfig[(level - 100) / 2 - 1] = statu;
            }
        }
        public void AiConfigSend()
        {
            device.RemoveAdSampling();
            device.SetAdframeRate((uint)InperGlobalClass.CameraSignalSettings.AiSampling, AiChannelsConfig);
        }
        public void TimeSpanAxis_VisibleRangeChanged(object sender, SciChart.Charting.Visuals.Events.VisibleRangeChangedEventArgs e)
        {
            foreach (var item in Instance.CameraChannels)
            {
                item.TimeSpanAxis.VisibleRange = e.NewVisibleRange;
            }

            Instance.EventChannelChart.TimeSpanAxis.VisibleRange = e.NewVisibleRange;
            Instance.EventChannelChart.EventTimeSpanAxis.VisibleRange = e.NewVisibleRange;
            Instance.EventChannelChart.EventTimeSpanAxisFixed.VisibleRange = e.NewVisibleRange;
        }
        public bool AllLightOpen()
        {
            bool isExistLight = false;
            if (CameraChannels.Count(c => c.Type == ChannelTypeEnum.Camera.ToString()) > 0)
            {
                LightWaveLength.ToList().ForEach(x =>
                 {
                     if (x.IsChecked)
                     {
                         isExistLight = true;
                         device.SwitchLight((uint)x.GroupId, true);
                         device.SetLightPower((uint)x.GroupId, x.LightPower);
                     }
                     else
                     {
                         device.SwitchLight((uint)x.GroupId, false);
                         device.SetLightPower((uint)x.GroupId, 0);
                     }
                 });
                return isExistLight;
            }
            return true;
        }
        public void AllLightClose()
        {
            LightWaveLength.ToList().ForEach(x =>
            {
                device.SwitchLight((uint)x.GroupId, false);
                device.SetLightPower((uint)x.GroupId, 0);
            });
        }
        #endregion
    }
}
