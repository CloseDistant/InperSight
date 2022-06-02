using HandyControl.Controls;
using InperDeviceManagement;
using InperProtocolStack;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace InperStudio.Lib.Helper
{
    public class WaveGroup
    {
        public bool IsChecked { get; set; } = false;
        public int GroupId { get; set; }
        public string WaveType { get; set; }
        public double LightPower { get; set; } = 0;
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


        private Task updateTask;
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
        private ConcurrentBag<CameraChannel> _LoopCannels = new ConcurrentBag<CameraChannel>();

        private short[] _SwapBuffer;
        #region
        public int VisionWidth = 720;
        public int VisionHeight = 540;
        public int SelectedWaveType = 1;

        public event Action<bool> StartCollectEvent;
        public PhotometryDevice device;
        public long time;

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
                            //Growl.Error("获取激发光失败：" + ex.ToString());
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
                    LightWaveLength.ToList().ForEach(x =>
                    {
                        if (x.IsChecked)
                        {
                            Instance.device.SetLightPower((uint)x.GroupId, 0);
                            Thread.Sleep(50);
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
                            //DevPhotometry.Instance.SwitchLight(x.GroupId, true);
                            device.SetLightPower((uint)x.GroupId, x.LightPower);
                            Thread.Sleep(50);
                        }
                    });
                    next_Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000 == 0 ? 5 * 1000 : InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000;
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
        private void Device_OnDevNotification(object sender, DevNotificationEventArgs e)
        {
            DevInputNotificationEventArgs dev = e as DevInputNotificationEventArgs;
            if (InperGlobalClass.IsPreview || InperGlobalClass.IsRecord)
            {
                if (_eventIsFirst)
                {
                    _EventStartTime = DateTime.Now.Ticks;
                    _eventIsFirst = false;
                }
                Input input = new Input() { ChannelId = (int)dev.IOID, CameraTime = DateTime.Now.Ticks - _EventStartTime, Value = dev.Status, CreateTime = DateTime.Now };
                _InputDataPlot.Enqueue(input);
                if (InperGlobalClass.IsRecord)
                {
                    SaveInputDatas.Enqueue(input);
                }
            }
        }
        public int count = 0;
        public int count1 = 0;
        public bool InitDataStruct()
        {
            try
            {
                if (InperGlobalClass.CameraSignalSettings.Sampling > 60)
                {
                    _cameraSkipCountArray = new int[InperGlobalClass.CameraSignalSettings.LightMode.Count];
                    for (int i = 0; i < _cameraSkipCountArray.Length; i++)
                    {
                        _cameraSkipCountArray[i] = 0;
                    }
                    _cameraSkipCount = (int)Math.Floor(InperGlobalClass.CameraSignalSettings.Sampling / 30) - 1;
                }
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
                            EventChannelChart.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = x.ChannelId, IsDigitalLine = true, DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = (Color)ColorConverter.ConvertFromString(x.BgColor) });
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

                    _MatQ.Enqueue(e);

                    if (InperGlobalClass.IsRecord)
                    {
                        Interlocked.Increment(ref count);
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

                if (mmats.Count() > 0 && (mmats.Last().Group == SelectedWaveType || InperGlobalClass.IsPreview || InperGlobalClass.IsRecord))
                {
                    Mat image_mat = mmats.Last().ImageMat;
                    unsafe
                    {
                        Marshal.Copy(image_mat.Data, _SwapBuffer, 0, VisionWidth * VisionHeight);
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
        public void FrameProc()
        {
            while (isLoop)
            {
                if (_MatQ.TryDequeue(out MarkedMat m))
                {
                    long ts = m.Timestamp - _PlottingStartTime;
                    time = ts;
                    ConcurrentBag<string> values = new ConcurrentBag<string>();
                    ConcurrentDictionary<int, double> _poltValues = new ConcurrentDictionary<int, double>();
                    Parallel.ForEach(_LoopCannels, mask =>
                    {
                        double r = (double)m.ImageMat.Mean(mask.Mask) / 655.35;
                        values.Add(mask.ChannelId + "," + System.Convert.ToBase64String(BitConverter.GetBytes(r)));
                        _poltValues.TryAdd(mask.ChannelId, r);

                        if (mask.Offset)
                        {
                            r -= Offset(mask, m.Group, r);
                        }

                        if (mask.Filters.IsSmooth)
                        {
                            r = Smooth(mask, m.Group, r);
                        }

                        DeltaFCalculate(mask, m.Group, r, ts);

                        if (mask.Filters.IsBandpass)
                        {
                            r = mask.Filters.OnlineFilter.GetBandpassValue(r, m.Group);
                        }
                        if (mask.Filters.IsBandstop)
                        {
                            r = mask.Filters.OnlineFilter.GetBandstopValue(r, m.Group);
                        }

                    });
                    if (_cameraSkipCountArray[m.Group] >= _cameraSkipCount)
                    {
                        _CameraDataPlot.Enqueue(new PlotData(m.Group, _poltValues, ts));
                        _cameraSkipCountArray[m.Group] = 0;
                    }
                    else
                    {
                        _cameraSkipCountArray[m.Group]++;
                    }
                    AllChannelRecord allChannelRecord = new AllChannelRecord() { CameraTime = ts, CreateTime = DateTime.Now, Type = m.Group, Value = string.Join(" ", values.ToArray()) };
                    if (InperGlobalClass.IsRecord)
                    {
                        Interlocked.Increment(ref count1);
                        SaveDatas.Enqueue(allChannelRecord); //mask.ChannelId, m.Group, r, ts, mask.Type
                    }
                }
                Thread.Sleep(0);
            }
        }

        #region usb data 
        private ConcurrentQueue<UsbAdData> _AdDatas = new ConcurrentQueue<UsbAdData>();
        public Dictionary<int, long> _adPreTime = new Dictionary<int, long>();
        public unsafe void UsbAdProc()
        {
            try
            {
                double aiSampling = InperGlobalClass.CameraSignalSettings.AiSampling;
                while (isLoop)
                {
                    if (device._CS._UARTA.ADPtrQueues.TryDequeue(out IntPtr ptr))
                    {
                        AdSamplingConv(aiSampling, ptr);
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
        private void UsbDataAppend(uint channel, long time, short[] values)
        {
            List<double> res1 = new List<double>();
            List<double> res2 = new List<double>();
            for (int i = 0; i < values.Length; i += 2)
            {
                res1.Add(values[i] * 3.3 * 23.81 / 4096 - 5);
                res2.Add(values[i + 1] * 3.3 * 23.81 / 4096 - 5);
            }

            _AdDatas.Enqueue(new UsbAdData(channel * 2 - 1, time, res1));
            _AdDatas.Enqueue(new UsbAdData(channel * 2, time, res2));
        }
        private int _UsbPlotLocked = 0;
        private void UsbDataPlot()
        {
            while (isLoop)
            {
                if (Interlocked.Exchange(ref _UsbPlotLocked, 1) == 0)
                {
                    if (_AdDatas.TryDequeue(out UsbAdData adData))
                    {
                        CameraChannel ccn = CameraChannels.FirstOrDefault(x => x.ChannelId == (adData.ChannelId + 100));
                        if (ccn != null)
                        {
                            XyDataSeries<TimeSpan, double> ds = ccn.RenderableSeries.First().DataSeries as XyDataSeries<TimeSpan, double>;
                            Tuple<TimeSpan[], double[]> res = AdDataTrans(adData, ccn.ChannelId);
                            using (ds.SuspendUpdates())
                            {
                                if ((TimeSpan)ds.XMax < res.Item1.First())
                                {
                                    ds.Append(res.Item1, res.Item2);
                                }
                            }
                            if (InperGlobalClass.IsRecord)
                            {
                                //UsbDataSave(res, ccn.ChannelId);
                                SaveAnalogDatas.Enqueue(new AnalogRecord() { ChannelId = (int)adData.ChannelId, Value = string.Join(",", res.Item2), CameraTime = adData.Time, CreateTime = DateTime.Now });
                            }
                        }
                    }
                    Interlocked.Exchange(ref _UsbPlotLocked, 0);
                }
                Thread.Sleep(1);
            }
        }
        private Tuple<TimeSpan[], double[]> AdDataTrans(UsbAdData adData, int channelId)
        {
            TimeSpan[] timeSpans = new TimeSpan[adData.Values.Count];
            double[] values = new double[adData.Values.Count];

            double adFsTimeInterval = 1 / InperGlobalClass.CameraSignalSettings.AiSampling;

            for (int i = 0; i < adData.Values.Count; i++)
            {
                TimeSpan ts = new TimeSpan(_adPreTime[channelId] + (long)(adFsTimeInterval * (i + 1) * Math.Pow(10, 7)));

                timeSpans[i] = ts;
                double r = adData.Values[i];
                CameraChannel cn = CameraChannels.FirstOrDefault(x => x.ChannelId == channelId);
                if (cn != null && cn.Type == ChannelTypeEnum.Analog.ToString())
                {
                    if (cn.Offset)
                    {
                        r -= Offset(cn, -1, r);
                    }
                    if (cn.Filters.IsSmooth)
                    {
                        r = Smooth(cn, -1, r);
                    }
                    DeltaFCalculate(cn, -1, r, ts.Ticks);
                    if (cn.Filters.IsBandpass)
                    {
                        r = cn.Filters.OnlineFilter.GetBandpassValue(r, 0);
                    }
                    if (cn.Filters.IsBandstop)
                    {
                        r = cn.Filters.OnlineFilter.GetBandstopValue(r, 0);
                    }
                }
                values[i] = r;
            }
            _adPreTime[channelId] = timeSpans.Last().Ticks;
            //adData.Values.Clear();
            return new Tuple<TimeSpan[], double[]>(timeSpans, values);
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
                }
            }
            return offsetValue.OffsetValue;
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
        private void DeltaFCalculate(CameraChannel cameraChannel, int group, double r, long ts)
        {
            if (Interlocked.Exchange(ref deltaFObj, 1) == 0)
            {
                _ = Parallel.ForEach(new ConcurrentBag<EventChannelJson>(InperGlobalClass.EventSettings.Channels), x =>
                   {
                       if (x.IsActive)
                       {
                           if (x.Type == ChannelTypeEnum.Camera.ToString() || x.Type == ChannelTypeEnum.Analog.ToString())
                           {
                               if (x.ChannelId == cameraChannel.ChannelId && x.LightIndex == group)
                               {
                                   if (DeltaFData.ContainsKey(cameraChannel.ChannelId))
                                   {
                                       if (DeltaFData[cameraChannel.ChannelId].ContainsKey(group))
                                       {

                                           DeltaFData[cameraChannel.ChannelId][group].Enqueue(r);
                                           if (DeltaFData[cameraChannel.ChannelId][group].Count > x.WindowSize)
                                           {
                                               _ = DeltaFData[cameraChannel.ChannelId][group].Dequeue();
                                               double deltaF = Math.Abs(r - DeltaFData[cameraChannel.ChannelId][group].ToList().Average()) / DeltaFData[cameraChannel.ChannelId][group].ToList().Average() * 100;
                                               if (deltaF >= x.DeltaF)
                                               {
                                                   SetDeltaFEvent(x, ts, deltaF);
                                               }
                                           }
                                       }
                                   }
                               }
                           }
                           else if (x.Type == ChannelTypeEnum.Output.ToString())
                           {
                               if (x.Condition != null)
                               {
                                   if (x.Condition.Type == ChannelTypeEnum.Camera.ToString() || x.Condition.Type == ChannelTypeEnum.Analog.ToString())
                                   {
                                       if (x.Condition.ChannelId == cameraChannel.ChannelId && x.Condition.LightIndex == group)
                                       {
                                           if (DeltaFData.ContainsKey(cameraChannel.ChannelId))
                                           {
                                               if (DeltaFData[cameraChannel.ChannelId].ContainsKey(group))
                                               {
                                                   DeltaFData[cameraChannel.ChannelId][group].Enqueue(r);
                                                   if (DeltaFData[cameraChannel.ChannelId][group].Count > x.WindowSize)
                                                   {
                                                       _ = DeltaFData[cameraChannel.ChannelId][group].Dequeue();
                                                       double deltaF = Math.Abs(r - DeltaFData[cameraChannel.ChannelId][group].ToList().Average()) / DeltaFData[cameraChannel.ChannelId][group].ToList().Average() * 100;
                                                       if (deltaF >= x.DeltaF)
                                                       {
                                                           SetDeltaFEvent(x, ts, deltaF);
                                                       }
                                                   }
                                               }
                                           }
                                       }
                                   }
                               }
                           }
                       }
                   });
                Interlocked.Exchange(ref deltaFObj, 0);
            }
        }
        private void SetDeltaFEvent(EventChannelJson eventChannelJson, long ts, double deltaF)
        {
            try
            {
                //if (AddMarkerByHotkeys(eventChannelJson.ChannelId, eventChannelJson.Name, (Color)ColorConverter.ConvertFromString(eventChannelJson.BgColor)))
                {
                    AddMarkerByHotkeys(eventChannelJson);
                    //if (eventChannelJson.Type == ChannelTypeEnum.Output.ToString())
                    //{
                    //    device.OuputIO((uint)eventChannelJson.ChannelId, 1);
                    //    //Thread.Sleep(50);
                    //    device.OuputIO((uint)eventChannelJson.ChannelId, 0);
                    //}
                    if (InperGlobalClass.IsRecord)
                    {
                        AIROI aIROI = new AIROI()
                        {
                            ChannelId = eventChannelJson.ChannelId,
                            CameraTime = ts,
                            DeltaF = deltaF,
                            Type = eventChannelJson.Type,
                            CreateTime = DateTime.Now
                        };

                        _ = App.SqlDataInit.sqlSugar.Insertable(aIROI).ExecuteCommand();
                    }
                }
            }
            finally
            {
            }
        }
        #endregion

        #region 数据存储
        public void AllDataSave()
        {
            if (CameraChannels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Camera.ToString()) != null)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveImageData();
                }, _saveDataTaskTokenSource.Token);
            }
            if (CameraChannels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Analog.ToString()) != null)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveAnalogData();
                }, _saveDataTaskTokenSource.Token);
            }
            if (InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Input.ToString()) != null)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveEventData();
                }, _saveDataTaskTokenSource.Token);
            }
        }
        #region 图像信号数据存储
        private ConcurrentQueue<AllChannelRecord> SaveDatas = new ConcurrentQueue<AllChannelRecord>();
        private List<AllChannelRecord> _SaveCameraCache = new List<AllChannelRecord>();
        private void SaveImageData()
        {
            while (!_saveDataTaskTokenSource.IsCancellationRequested)
            {
                if (SaveDatas.TryDequeue(out AllChannelRecord data))
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
                Thread.Sleep(1);
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
            while (true)
            {
                if (Interlocked.Exchange(ref _InputLocked, 1) == 0)
                {
                    if (SaveInputDatas.TryDequeue(out Input data))
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
                    Interlocked.Exchange(ref _InputLocked, 0);
                }
                Thread.Sleep(1);
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
            while (true)
            {
                if (Interlocked.Exchange(ref _AnalogLocked, 1) == 0)
                {
                    if (SaveAnalogDatas.TryDequeue(out AnalogRecord data))
                    {
                        _SaveAnalogCache.Add(data);
                        if (_SaveInputCache.Count > 1000)
                        {
                            AnalogRecord[] analogs = new AnalogRecord[SaveAnalogDatas.Count];
                            _SaveAnalogCache.CopyTo(analogs);
                            _SaveAnalogCache.Clear();
                            AnalogDataCloneToSave(analogs);
                        }
                    }
                    Interlocked.Exchange(ref _AnalogLocked, 0);
                }
                Thread.Sleep(1);
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
        public void DataPlot()
        {
            if (CameraChannels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Camera.ToString()) != null)
            {
                Task.Factory.StartNew(() =>
                {
                    CameraDataUpdateProc();
                });
            }
            if (CameraChannels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Analog.ToString()) != null)
            {
                Task.Factory.StartNew(() =>
                {
                    UsbDataPlot();
                });
            }
            if (InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Input.ToString()) != null)
            {
                Task.Factory.StartNew(() =>
                {
                    InputDataUpdateProc();
                });
            }
        }

        #region 相机数据渲染
        private ConcurrentQueue<PlotData> _CameraDataPlot = new ConcurrentQueue<PlotData>();
        private int _CameraPlotLocked = 0;
        private void CameraDataUpdateProc()
        {
            List<CameraChannel> channels = CameraChannels.ToList().FindAll(x => x.Type == ChannelTypeEnum.Camera.ToString());
            while (isLoop)
            {
                if (Interlocked.Exchange(ref _CameraPlotLocked, 1) == 0)
                {
                    if (_CameraDataPlot.TryDequeue(out PlotData record))
                    {
                        Parallel.ForEach(record.Values, val =>
                        {
                            IRenderableSeriesViewModel render = channels.First(x => x.ChannelId == val.Key).RenderableSeries.First(r => (int)(r as LineRenderableSeriesViewModel).Tag == record.Group);
                            using (render.DataSeries.SuspendUpdates())
                            {
                                (render.DataSeries as XyDataSeries<TimeSpan, double>).Append(new TimeSpan(record.Timestamp / 100), val.Value);
                            }
                        });
                    }
                    Interlocked.Exchange(ref _CameraPlotLocked, 0);
                }
                Thread.Sleep(1);
            }
        }
        #endregion

        #region input数据渲染
        private ConcurrentQueue<Input> _InputDataPlot = new ConcurrentQueue<Input>();
        private int _InputPlotLocked = 0;
        private void InputDataUpdateProc()
        {
            while (isLoop)
            {
                if (Interlocked.Exchange(ref _InputPlotLocked, 1) == 0)
                {
                    if (_InputDataPlot.TryDequeue(out Input input))
                    {
                        if (EventChannelChart.RenderableSeries.Count > 0)
                        {
                            _ = Parallel.ForEach(EventChannelChart.RenderableSeries, item =>
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
                            });
                        }
                    }
                    Interlocked.Exchange(ref _InputPlotLocked, 0);
                }
                Thread.Sleep(1);
            }
        }
        #endregion


        #endregion
        public async void AddMarkerByHotkeys(EventChannelJson chn, int type = -1)//type 0 start 1 end -1 other
        {
            await Task.Factory.StartNew(() =>
            {

                TimeSpan _time = new TimeSpan(time / 100);
                if (type == 0)
                {
                    _time = new TimeSpan(0);
                }
                bool isAddAnnotation = true;

                if (EventChannelChart.Annotations.Count > 0)
                {
                    IAnnotationViewModel obj = EventChannelChart.Annotations.LastOrDefault((x) => (x as VerticalLineAnnotationViewModel).LabelValue.Equals(chn.Name));
                    if (obj != null)
                    {
                        TimeSpan tick = _time.Subtract((TimeSpan)(obj as VerticalLineAnnotationViewModel).X1);
                        chn = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == chn.ChannelId && x.Name == chn.Name && (x.Type == ChannelTypeEnum.Camera.ToString() || x.Type == ChannelTypeEnum.Analog.ToString() || x.Type == ChannelTypeEnum.Output.ToString()));
                        if (chn != null && chn.IsRefractoryPeriod)
                        {
                            if (tick.TotalMilliseconds < chn.RefractoryPeriod * 1000)
                            {
                                isAddAnnotation = false;
                            }
                        }
                    }
                }
                if (isAddAnnotation)
                {
                    if (chn.Type == ChannelTypeEnum.Output.ToString())
                    {
                        device.OuputIO((uint)chn.ChannelId, 1);
                        //Thread.Sleep(50);
                        device.OuputIO((uint)chn.ChannelId, 0);
                    }

                    EventChannelChart.Annotations.Add(new VerticalLineAnnotationViewModel()
                    {
                        VerticalAlignment = VerticalAlignment.Stretch,
                        FontSize = 12,
                        ShowLabel = InperGlobalClass.EventPanelProperties.DisplayNameVisible,
                        Stroke = (Color)ColorConverter.ConvertFromString(chn.BgColor),
                        LabelValue = chn.Name,
                        LabelTextFormatting = "12",
                        LabelPlacement = LabelPlacement.Left,
                        LabelsOrientation = System.Windows.Controls.Orientation.Vertical,
                        StrokeThickness = 1,
                        X1 = _time
                    });
                }
            });

        }
        public void StopPlot()
        {
            try
            {
                if (isAdstart)
                {
                    device.RemoveAdSampling();
                }

                isFirstRecordTiem = false; isLoop = false; isAdstart = false;

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
                //device.SetFrameRate(InperGlobalClass.CameraSignalSettings.Sampling);
                InperGlobalClass.SetSampling(InperGlobalClass.CameraSignalSettings.Sampling);
                _adPreTime.Clear();
                _LoopCannels = new ConcurrentBag<CameraChannel>();

                Instance.CameraChannels.ToList().ForEach(x => { if (x.Type == ChannelTypeEnum.Camera.ToString()) { _LoopCannels.Add(x); } });
                uint[] chnn = new uint[4] { 0, 0, 0, 0 };
                CameraChannels.ToList().ForEach(x =>
                {
                    if (x.Type == ChannelTypeEnum.Camera.ToString())
                    {
                        x.RenderableSeries.Clear();
                        x.LightModes.ForEach(l =>
                        {
                            Debug.WriteLine("light:" + l.LightType);
                            LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = l.LightType, DataSeries = l.XyDataSeries, Stroke = l.WaveColor.Color };
                            line.DataSeries.FifoCapacity = 20 * 60 * (int)InperGlobalClass.CameraSignalSettings.Sampling;
                            x.RenderableSeries.Add(line);
                        });
                    }
                    if (x.Type == ChannelTypeEnum.Analog.ToString())
                    {
                        isAdstart = true;
                        if (x.ChannelId <= 102)
                        {
                            chnn[0] = 1;
                        }
                        else if (x.ChannelId <= 104)
                        {
                            chnn[1] = 1;
                        }
                        else if (x.ChannelId <= 106)
                        {
                            chnn[2] = 1;
                        }
                        else
                        {
                            chnn[3] = 1;
                        }
                        x.RenderableSeries.ToList().ForEach(line =>
                        {
                            (line.DataSeries as XyDataSeries<TimeSpan, double>).Clear();
                            line.DataSeries.FifoCapacity = 20 * 60 * (int)InperGlobalClass.CameraSignalSettings.AiSampling;
                        });
                        _adPreTime.Add(x.ChannelId, 0);
                    }
                });

                InperGlobalClass.EventSettings.Channels.ForEach(x =>
                {
                    if (x.Type == ChannelTypeEnum.Input.ToString())
                    {
                        Instance.device.SetIOMode((uint)x.ChannelId, IOMode.IOM_INPUT);
                    }
                    if (x.Type == ChannelTypeEnum.Output.ToString())
                    {
                        Instance.device.SetIOMode((uint)x.ChannelId, IOMode.IOM_OUTPUT);
                    }
                });
                if (isAdstart)
                {
                    device.SetAdframeRate((uint)InperGlobalClass.CameraSignalSettings.AiSampling, chnn);
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
                updateTask = Task.Factory.StartNew(() => { DataPlot(); }, _updateTaskTokenSource.Token);
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
    }
}
