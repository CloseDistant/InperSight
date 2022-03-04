﻿using HandyControl.Controls;
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
using System.Linq;
using System.Runtime.InteropServices;
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
    public class SignalData
    {
        public Dictionary<int, Queue<KeyValuePair<long, double>>> ValuePairs { get; set; } = new Dictionary<int, Queue<KeyValuePair<long, double>>>();
    }
    public class SignalDataUsb
    {
        public Dictionary<int, Queue<Tuple<TimeSpan[], double[]>>> ValuePairs { get; set; } = new Dictionary<int, Queue<Tuple<TimeSpan[], double[]>>>();
    }
    public class InperDeviceHelper : PropertyChangedBase
    {
        private static InperDeviceHelper inperDeviceHelper;
        private static object _lock = new object();
        public static InperDeviceHelper Instance
        {
            get
            {
                if (inperDeviceHelper == null)
                {
                    lock (_lock)
                    {
                        if (inperDeviceHelper == null)
                        {
                            inperDeviceHelper = new InperDeviceHelper();
                        }
                    }
                }
                return inperDeviceHelper;
            }
        }
        private readonly object _QLock = new object();
        private readonly object _DisplayQLock = new object();
        private readonly object _SaveDataLock = new object();
        private readonly AutoResetEvent _AREvent = new AutoResetEvent(false);
        public readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
        public readonly DispatcherTimer _DataSaveTimer = new DispatcherTimer(DispatcherPriority.Render);
        private readonly AutoResetEvent _DataEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _DataSaveEvent = new AutoResetEvent(false);
        public readonly object _SignalQsLocker = new object();
        public readonly object _EventQLock = new object();

        private Task updateTask;
        private Task frameProcTask;
        public Task saveDataTask;
        private bool isFirstRecordTiem = true;
        private bool isLoop = true;
        private long _PlottingStartTime;

        private short[] _SwapBuffer;
        private Queue<long> EventTimeSet = new Queue<long>();
        private Dictionary<int, Queue<KeyValuePair<long, double>>> _SaveEventQs = new Dictionary<int, Queue<KeyValuePair<long, double>>>();
        private Dictionary<int, SignalData> _SaveSignalQs = new Dictionary<int, SignalData>();
        private Dictionary<int, SignalDataUsb> _SaveUsbSignalQs = new Dictionary<int, SignalDataUsb>();
        #region
        public int VisionWidth = 720;
        public int VisionHeight = 540;
        public int SelectedWaveType = 1;

        //public event EventHandler<bool> WaveInitEvent;
        public event Action<bool> StartCollectEvent;
        public PhotometryDevice device;
        public long time;

        public System.Timers.Timer _Metronome = new System.Timers.Timer();
        private bool IsInWorkingPeriod = true;

        public Queue<InperDeviceManagement.MarkedMat> _MatQ = new Queue<InperDeviceManagement.MarkedMat>();
        public Queue<InperDeviceManagement.MarkedMat> _DisplayMatQ = new Queue<InperDeviceManagement.MarkedMat>();
        /// <summary>
        /// 每个通道对应的数据
        /// </summary>
        public ConcurrentDictionary<int, SignalData> _SignalQs = new ConcurrentDictionary<int, SignalData>();
        public ConcurrentDictionary<int, Mat> ROIMasks { get; set; } = new ConcurrentDictionary<int, Mat>();
        public ConcurrentDictionary<int, Dictionary<int, Queue<double>>> FilterData { get; set; } = new ConcurrentDictionary<int, Dictionary<int, Queue<double>>>();
        public ConcurrentDictionary<int, Dictionary<int, Queue<double>>> LowFilterData { get; set; } = new ConcurrentDictionary<int, Dictionary<int, Queue<double>>>();
        public ConcurrentDictionary<int, Dictionary<int, Queue<double>>> HeightFilterData { get; set; } = new ConcurrentDictionary<int, Dictionary<int, Queue<double>>>();
        public ConcurrentDictionary<int, Dictionary<int, Queue<double>>> NotchFilterData { get; set; } = new ConcurrentDictionary<int, Dictionary<int, Queue<double>>>();
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
        public void DeviceInit(PhotometryDevice device)
        {
            this.device = device;
            device.DidGrabImage += Instance_OnImageGrabbed;
            //device.OnUsbInfoUpdated += Device_OnUsbInfoUpdated;

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
                            //WaveInitEvent?.Invoke(this, exist);
                            if (!exist)
                            {
                                Growl.Error("未获取到激发光");
                            }
                        }
                        catch (Exception ex)
                        {
                            Growl.Error("获取激发光失败：" + ex.ToString());
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
                Growl.Error("Failed to initialize the basler camera");
                return;
            }
            WBMPPreview = new WriteableBitmap(VisionWidth, VisionHeight, 96, 96, PixelFormats.Gray16, null);

            timer.Interval = TimeSpan.FromMilliseconds(20);

            timer.Tick += (s, e) =>
            {
                _ = _DataEvent.Set();
            };

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

            _DataSaveTimer.Interval = TimeSpan.FromSeconds(3);
            _DataSaveTimer.Tick += (s, e) =>
            {
                _ = _DataSaveEvent.Set();
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
                Monitor.Enter(_EventQLock);
                if (EventTimeSet.Count > 0)
                {
                    foreach (KeyValuePair<int, Queue<KeyValuePair<long, double>>> item in EventChannelChart.EventQs)
                    {
                        if (dev.IOID == item.Key)
                        {
                            item.Value.Enqueue(new KeyValuePair<long, double>(EventTimeSet.Last(), dev.Status));
                            //Console.WriteLine(dev.IOID + ":" + dev.Status);
                        }
                    }
                    if (InperGlobalClass.IsRecord)
                    {
                        foreach (KeyValuePair<int, Queue<KeyValuePair<long, double>>> item in _SaveEventQs)
                        {
                            if (dev.IOID == item.Key)
                            {
                                item.Value.Enqueue(new KeyValuePair<long, double>(EventTimeSet.Last(), dev.Status));
                            }
                        }
                    }
                    EventTimeSet.Clear();
                }

                Monitor.Exit(_EventQLock);
            }
        }
        public bool InitDataStruct()
        {
            try
            {
                if (_SignalQs.Count > 0)
                {
                    FilterData.Clear();
                    LowFilterData.Clear();
                    HeightFilterData.Clear();
                    NotchFilterData.Clear();
                    OffsetData.Clear();
                    DeltaFData.Clear();
                    foreach (var item in CameraChannels)
                    {
                        //item.Offset = false;
                        if (!_SaveSignalQs.ContainsKey(item.ChannelId) && item.Type == ChannelTypeEnum.Camera.ToString())
                        {
                            _SaveSignalQs.Add(item.ChannelId, new SignalData());
                        }
                        if (!_SaveUsbSignalQs.ContainsKey(item.ChannelId) && item.Type == ChannelTypeEnum.Analog.ToString())
                        {
                            _SaveUsbSignalQs.Add(item.ChannelId, new SignalDataUsb());
                            _SaveUsbSignalQs[item.ChannelId].ValuePairs.Add(-1, new Queue<Tuple<TimeSpan[], double[]>>());
                        }
                        _ = FilterData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                        _ = LowFilterData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                        _ = HeightFilterData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                        _ = NotchFilterData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                        _ = OffsetData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                        _ = DeltaFData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                        if (item.LightModes.Count > 0)
                        {
                            item.LightModes.ForEach(x =>
                            {
                                x.OffsetValue = 0;
                                if (item.Type == ChannelTypeEnum.Camera.ToString())
                                {
                                    _SaveSignalQs[item.ChannelId].ValuePairs.Add(x.LightType, new Queue<KeyValuePair<long, double>>());
                                }
                                FilterData[item.ChannelId].Add(x.LightType, new Queue<double>());
                                LowFilterData[item.ChannelId].Add(x.LightType, new Queue<double>());
                                HeightFilterData[item.ChannelId].Add(x.LightType, new Queue<double>());
                                NotchFilterData[item.ChannelId].Add(x.LightType, new Queue<double>());

                                OffsetData[item.ChannelId].Add(x.LightType, new Queue<double>());
                                DeltaFData[item.ChannelId].Add(x.LightType, new Queue<double>());
                            });
                        }
                        //else
                        //{
                        //    _SaveSignalQs[item.ChannelId].ValuePairs.Add(-1, new Queue<KeyValuePair<long, double>>());// 模拟通道只有一种信号
                        //}
                    }
                }
                else
                {
                    return false;
                }
                if (InperGlobalClass.EventSettings.Channels.Count > 0)
                {
                    InperGlobalClass.EventSettings.Channels.ForEach(x =>
                    {
                        if (x.Type == EventSettingsTypeEnum.Marker.ToString() || x.Type == ChannelTypeEnum.Input.ToString())
                        {
                            EventChannelChart.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = x.ChannelId, IsDigitalLine = true, DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = (Color)ColorConverter.ConvertFromString(x.BgColor) });
                            if (!EventChannelChart.EventQs.ContainsKey(x.ChannelId))
                            {
                                EventChannelChart.EventQs.TryAdd(x.ChannelId, new Queue<KeyValuePair<long, double>>());
                            }
                            _SaveEventQs.Add(x.ChannelId, new Queue<KeyValuePair<long, double>>());
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
        private void Instance_OnImageGrabbed(object sender, InperDeviceManagement.MarkedMat e)
        {
            try
            {
                if (InperGlobalClass.IsPreview || InperGlobalClass.IsRecord)
                {
                    //Console.WriteLine(e.Group);
                    if (isFirstRecordTiem)
                    {
                        _PlottingStartTime = e.Timestamp;
                        isFirstRecordTiem = false;
                    }
                    Monitor.Enter(_QLock);
                    _MatQ.Enqueue(e);
                    Monitor.Exit(_QLock);
                    _ = _AREvent.Set();
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

        #region usb data 
        private readonly List<UsbAdData> _AdDatas = new List<UsbAdData>();
        private bool isfirstAD = true;
        private bool isfirstADTime = true;
        public long _firstADTime = 0;
        public Dictionary<int, long> _adPreTime = new Dictionary<int, long>();
        public unsafe void UsbAdProc()
        {
            try
            {
                while (isLoop)
                {
                    _ = device._CS._UARTA._DataAutoResetEvent.WaitOne();
                    if (Monitor.TryEnter(device._CS._UARTA._DataCacheObj))
                    {
                        AdSamplingConv(InperGlobalClass.CameraSignalSettings.AiSampling);
                        Monitor.Exit(device._CS._UARTA._DataCacheObj);
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void AdSamplingConv(double sampling)
        {
            if (sampling >= 10000)
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache512 = new ConcurrentQueue<UsbAdDataStru512>();
                    isfirstAD = false;
                }
                UsbAdDataStru512 data;
                while (device._CS._UARTA._ADDataCache512.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            else if (sampling >= 5000)
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache256 = new ConcurrentQueue<UsbAdDataStru256>();
                    isfirstAD = false;
                }
                UsbAdDataStru256 data;
                while (device._CS._UARTA._ADDataCache256.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            else if (sampling >= 1000)
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache128 = new ConcurrentQueue<UsbAdDataStru128>();
                    isfirstAD = false;
                }
                UsbAdDataStru128 data;
                while (device._CS._UARTA._ADDataCache128.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            else if (sampling >= 500)
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache64 = new ConcurrentQueue<UsbAdDataStru64>();
                    isfirstAD = false;
                }
                UsbAdDataStru64 data;
                while (device._CS._UARTA._ADDataCache64.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            else if (sampling >= 100)
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache32 = new ConcurrentQueue<UsbAdDataStru32>();
                    isfirstAD = false;
                }
                UsbAdDataStru32 data;
                while (device._CS._UARTA._ADDataCache32.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            else if (sampling >= 50)
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache16 = new ConcurrentQueue<UsbAdDataStru16>();
                    isfirstAD = false;
                }
                UsbAdDataStru16 data;
                while (device._CS._UARTA._ADDataCache16.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            else if (sampling >= 30)
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache8 = new ConcurrentQueue<UsbAdDataStru8>();
                    isfirstAD = false;
                }
                UsbAdDataStru8 data;
                while (device._CS._UARTA._ADDataCache8.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            else if (sampling >= 16)
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache4 = new ConcurrentQueue<UsbAdDataStru4>();
                    isfirstAD = false;
                }
                UsbAdDataStru4 data;
                while (device._CS._UARTA._ADDataCache4.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            //else if (sampling >= 8)
            else
            {
                if (isfirstAD)
                {

                    device._CS._UARTA._ADDataCache2 = new ConcurrentQueue<UsbAdDataStru2>();
                    isfirstAD = false;
                }
                UsbAdDataStru2 data;
                while (device._CS._UARTA._ADDataCache2.TryDequeue(out data))
                {
                    if (isfirstADTime)
                    {
                        _firstADTime = data.Time;
                        isfirstADTime = false;
                    }
                    this.UsbDataAppend(data.Channel, data.Time, data.Values);
                }
            }
            //else
            //{
            //    if (isfirstAD)
            //    {

            //        device._CS._UARTA._ADDataCache1 = new ConcurrentQueue<UsbAdDataStru1>();
            //        isfirstAD = false;
            //    }
            //    UsbAdDataStru1 data;
            //    while (device._CS._UARTA._ADDataCache1.TryDequeue(out data))
            //    {
            //        if (isfirstADTime)
            //        {
            //            _firstADTime = data.Time;
            //            isfirstADTime = false;
            //        }
            //        this.UsbDataAppend(data.Channel, data.Time, data.Values);
            //    }
            //}
        }
        private readonly object _UsbDataAppendObj = new object();
        private readonly AutoResetEvent _UsbDataEvent = new AutoResetEvent(false);
        private void UsbDataAppend(uint channel, long time, short[] values)
        {
            Monitor.Enter(_UsbDataAppendObj);
            List<int> res1 = new List<int>();
            List<int> res2 = new List<int>();
            for (int i = 0; i < values.Length; i += 2)
            {
                res1.Add(values[i]);
                res2.Add(values[i + 1]);
            }

            _AdDatas.Add(new UsbAdData(channel * 2 - 1, time, res1));
            _AdDatas.Add(new UsbAdData(channel * 2, time, res2));
            Monitor.Exit(_UsbDataAppendObj);
            _UsbDataEvent.Set();
        }
        private List<UsbAdData> _Data = new List<UsbAdData>();
        private void UsbDataPlot()
        {
            while (isLoop)
            {
                _ = _UsbDataEvent.WaitOne();

                if (Monitor.TryEnter(_UsbDataAppendObj))
                {
                    Copy(_AdDatas, ref _Data);
                    _AdDatas.Clear();
                    Monitor.Exit(_UsbDataAppendObj);

                    foreach (UsbAdData adData in _Data.ToArray())
                    {
                        CameraChannel ccn = CameraChannels.FirstOrDefault(x => x.ChannelId == (adData.ChannelId + 100));
                        if (ccn != null)
                        {
                            XyDataSeries<TimeSpan, double> ds = ccn.RenderableSeries.First().DataSeries as XyDataSeries<TimeSpan, double>;

                            //long ticks = ((TimeSpan)ds.XMax).Ticks > 0 ? ((TimeSpan)ds.XMax).Ticks : 0;

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
                                UsbDataSave(res, ccn.ChannelId);
                            }
                        }
                    }
                    _Data.Clear();
                }
            }
        }
        private readonly object _SaveUsbDataLock = new object();
        private void UsbDataSave(Tuple<TimeSpan[], double[]> data, int channelId, int s_group = -1)
        {
            Monitor.Enter(_SaveUsbDataLock);
            if (_SaveUsbSignalQs.ContainsKey(channelId))
            {
                if (_SaveUsbSignalQs[channelId].ValuePairs.ContainsKey(s_group))
                {
                    _SaveUsbSignalQs[channelId].ValuePairs[s_group].Enqueue(data);

                }
            }
            Monitor.Exit(_SaveUsbDataLock);
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
                //if (r == 0)
                //{
                //    Console.WriteLine(++count);
                //}
                //CameraChannel cn = CameraChannels.FirstOrDefault(x => x.ChannelId == channelId);
                //if (cn != null && cn.Type == ChannelTypeEnum.Analog.ToString())
                //{
                //    if (cn.Offset)
                //    {
                //        r -= Offset(cn, -1, r);
                //    }
                //    if (cn.Filters.IsSmooth)
                //    {
                //        r = Smooth(cn, -1, r);
                //    }
                //    DeltaFCalculate(cn, -1, r, ts.Ticks);
                //    if (cn.Filters.IsHighPass)
                //    {
                //        r = HighPass(cn, -1, r);
                //    }
                //    if (cn.Filters.IsLowPass)
                //    {
                //        r = LowPass(cn, -1, r);
                //    }
                //    if (cn.Filters.IsNotch)
                //    {
                //        r = NotchPass(cn, -1, r);
                //    }
                //}
                values[i] = r;
            }
            _adPreTime[channelId] = timeSpans.Last().Ticks;
            //adData.Values.Clear();
            return new Tuple<TimeSpan[], double[]>(timeSpans, values);
        }
        #endregion
        public void FrameProc()
        {
            while (isLoop)
            {
                MarkedMat[] mmats;
                _ = _AREvent.WaitOne();

                if (Monitor.TryEnter(_QLock))
                {
                    mmats = _MatQ.ToArray();
                    _MatQ.Clear();
                    Monitor.Exit(_QLock);

                    foreach (MarkedMat m in mmats)
                    {
                        long ts = m.Timestamp - _PlottingStartTime;
                        time = ts;
                        //System.Diagnostics.Debug.WriteLine(m.Group + " ====== " + ts);
                        if (Monitor.TryEnter(_EventQLock))
                        {
                            EventTimeSet.Enqueue(ts);

                            Monitor.Exit(_EventQLock);
                        }

                        ConcurrentDictionary<int, double> ploting_data = new ConcurrentDictionary<int, double>();
                        if (CameraChannels.Count > 0)
                        {
                            _ = Parallel.ForEach(CameraChannels, mask =>
                                  {
                                      if (mask.Type == ChannelTypeEnum.Camera.ToString())
                                      {
                                          double r = (double)m.ImageMat.Mean(mask.Mask) / 655.35;

                                          if (r > 0)
                                          {
                                              if (mask.Offset)
                                              {
                                                  r -= Offset(mask, m.Group, r);
                                              }

                                              if (mask.Filters.IsSmooth)
                                              {
                                                  r = Smooth(mask, m.Group, r);
                                              }

                                              DeltaFCalculate(mask, m.Group, r, ts);

                                              if (mask.Filters.IsHighPass)
                                              {
                                                  r = HighPass(mask, m.Group, r);
                                              }

                                              if (mask.Filters.IsLowPass)
                                              {
                                                  r = LowPass(mask, m.Group, r);
                                              }
                                              if (mask.Filters.IsNotch)
                                              {
                                                  r = NotchPass(mask, m.Group, r);
                                              }
                                              ploting_data[mask.ChannelId] = r;
                                          }
                                      }
                                  });
                            if (ploting_data.Count > 0)
                            {
                                this.AppendData(ploting_data, m.Group, ts);
                                if (InperGlobalClass.IsRecord)
                                {
                                    this.SaveData(ploting_data, ts, m.Group);
                                }
                            }
                        }
                    }
                }
            }
        }

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
        private double Smooth(CameraChannel cameraChannel, int group, double r)
        {
            double val = r;
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
            return val;
        }
        private double HighPass(CameraChannel cameraChannel, int group, double r)
        {
            double val = r;
            if (HeightFilterData.ContainsKey(cameraChannel.ChannelId))
            {
                if (HeightFilterData[cameraChannel.ChannelId].ContainsKey(group))
                {
                    HeightFilterData[cameraChannel.ChannelId][group].Enqueue(r);
                    if (HeightFilterData[cameraChannel.ChannelId][group].Count >= 5)
                    {
                        val = FilterTools.FiltterTool.RCHighFilter(HeightFilterData[cameraChannel.ChannelId][group].ToArray(), cameraChannel.Filters.HighPass, InperGlobalClass.CameraSignalSettings.Sampling / InperGlobalClass.CameraSignalSettings.CameraChannels.Count);
                        _ = HeightFilterData[cameraChannel.ChannelId][group].Dequeue();
                    }
                }
            }
            return val;
        }
        private double LowPass(CameraChannel cameraChannel, int group, double r)
        {
            double val = r;
            if (LowFilterData.ContainsKey(cameraChannel.ChannelId))
            {
                if (LowFilterData[cameraChannel.ChannelId].ContainsKey(group))
                {
                    LowFilterData[cameraChannel.ChannelId][group].Enqueue(r);
                    if (LowFilterData[cameraChannel.ChannelId][group].Count >= 5)
                    {
                        val = FilterTools.FiltterTool.RCLowPass(LowFilterData[cameraChannel.ChannelId][group].ToArray(), cameraChannel.Filters.LowPass, InperGlobalClass.CameraSignalSettings.Sampling / InperGlobalClass.CameraSignalSettings.CameraChannels.Count);
                        _ = LowFilterData[cameraChannel.ChannelId][group].Dequeue();
                    }
                }
            }
            return val;
        }
        private double NotchPass(CameraChannel cameraChannel, int group, double r)
        {
            r = FilterTools.FiltterTool.NotchPass(r, InperGlobalClass.CameraSignalSettings.Sampling / InperGlobalClass.CameraSignalSettings.CameraChannels.Count, cameraChannel.Filters.Notch);
            return r;
        }
        private readonly object deltaFObj = new object();
        private void DeltaFCalculate(CameraChannel cameraChannel, int group, double r, long ts)
        {
            _ = Parallel.ForEach(new ConcurrentBag<EventChannelJson>(InperGlobalClass.EventSettings.Channels), x =>
               {
                   lock (deltaFObj)
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
                                                           //SendCommand(x);
                                                       }
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
        }
        private void SetDeltaFEvent(EventChannelJson eventChannelJson, long ts, double deltaF)
        {
            try
            {
                if (AddMarkerByHotkeys(eventChannelJson.ChannelId, eventChannelJson.Name, (Color)ColorConverter.ConvertFromString(eventChannelJson.BgColor)))
                {
                    if (eventChannelJson.Type == ChannelTypeEnum.Output.ToString())
                    {
                        device.OuputIO((uint)eventChannelJson.ChannelId, 1);
                        Thread.Sleep(50);
                        device.OuputIO((uint)eventChannelJson.ChannelId, 0);
                    }
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
        private void SaveData(ConcurrentDictionary<int, double> data, long timestamp, int s_group = -1)
        {
            Monitor.Enter(_SaveDataLock);
            _ = Parallel.ForEach(data, kv =>
            {
                if (_SaveSignalQs.ContainsKey(kv.Key))
                {
                    if (_SaveSignalQs[kv.Key].ValuePairs.ContainsKey(s_group))
                    {
                        _SaveSignalQs[kv.Key].ValuePairs[s_group].Enqueue(new KeyValuePair<long, double>(timestamp, kv.Value));
                    }
                }
            });
            Monitor.Exit(_SaveDataLock);
        }
        private void AppendData(ConcurrentDictionary<int, double> data, int s_group, long timestamp)
        {

            Monitor.Enter(_SignalQsLocker);
            try
            {
                _ = Parallel.ForEach(data, kv =>
                  {
                      if (_SignalQs.ContainsKey(kv.Key))
                      {
                          if (_SignalQs[kv.Key].ValuePairs.ContainsKey(s_group))
                          {
                              _SignalQs[kv.Key].ValuePairs[s_group].Enqueue(new KeyValuePair<long, double>(timestamp, kv.Value));
                          }
                      }
                  });
            }
            finally
            {
                Monitor.Exit(_SignalQsLocker);
            }

            return;
        }
        public void UpdateDataProc()
        {
            while (isLoop)
            {
                _ = _DataEvent.WaitOne();
                try
                {
                    if (CameraChannels.Count > 0)
                    {
                        ConcurrentDictionary<int, SignalData> signalQs = null;
                        if (Monitor.TryEnter(_SignalQsLocker))
                        {

                            Copy(_SignalQs, ref signalQs);

                            _ = Parallel.ForEach(_SignalQs, q =>
                            {
                                foreach (KeyValuePair<int, Queue<KeyValuePair<long, double>>> item in q.Value.ValuePairs)
                                {
                                    item.Value.Clear();
                                }
                            });
                            Monitor.Exit(_SignalQsLocker);
                        }

                        if (signalQs == null)
                        {
                            continue;
                        }

                        _ = Parallel.ForEach(signalQs, kv =>
                        {
                            CameraChannel channel = CameraChannels.FirstOrDefault(x => x.ChannelId == kv.Key && x.Type == ChannelTypeEnum.Camera.ToString());
                            if (channel.LightModes.Count > 0)
                            {
                                _ = Parallel.ForEach(channel.RenderableSeries, item =>
                                 {
                                     int id = int.Parse((item as LineRenderableSeriesViewModel).Tag.ToString());

                                     using (item.DataSeries.SuspendUpdates())
                                     {
                                         Tuple<TimeSpan[], double[]> s0_plot_data = TransposeDataAndRegisterTR(kv.Value.ValuePairs[id]);
                                         if (s0_plot_data.Item1.Length > 0)
                                         {
                                             if ((TimeSpan)item.DataSeries.XMax < s0_plot_data.Item1.First())
                                             {
                                                 (item.DataSeries as XyDataSeries<TimeSpan, double>).Append(s0_plot_data.Item1, s0_plot_data.Item2);
                                             }
                                         }
                                         kv.Value.ValuePairs[id].Clear();
                                     }
                                 });
                            }
                        });

                        if (Monitor.TryEnter(_EventQLock))
                        {
                            ConcurrentDictionary<int, Queue<KeyValuePair<long, double>>> eventQs = null;
                            Copy(EventChannelChart.EventQs, ref eventQs);

                            _ = Parallel.ForEach(EventChannelChart.EventQs, q =>
                            {
                                q.Value.Clear();
                            });
                            Monitor.Exit(_EventQLock);

                            _ = Parallel.ForEach(eventQs, q =>
                            {
                                if (EventChannelChart.RenderableSeries.Count > 0)
                                {
                                    _ = Parallel.ForEach(EventChannelChart.RenderableSeries, item =>
                                    {
                                        int id = int.Parse((item as LineRenderableSeriesViewModel).Tag.ToString());
                                        if (q.Key == id)
                                        {
                                            using (item.DataSeries.SuspendUpdates())
                                            {
                                                Tuple<TimeSpan[], double[]> s0_plot_data = TransposeDataAndRegisterTR(q.Value);

                                                (item.DataSeries as XyDataSeries<TimeSpan, double>).Append(s0_plot_data.Item1, s0_plot_data.Item2);
                                                q.Value.Clear();
                                            }
                                        }
                                    });
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex.ToString());
                }
            }
        }
        public void SaveDateProc()
        {
            while (InperGlobalClass.IsRecord)
            {
                try
                {
                    _ = _DataSaveEvent.WaitOne();

                    if (Monitor.TryEnter(_EventQLock))
                    {
                        Dictionary<int, Queue<KeyValuePair<long, double>>> saveEventQs = null;
                        Copy(_SaveEventQs, ref saveEventQs);
                        _ = Parallel.ForEach(_SaveEventQs, q =>
                        {
                            q.Value.Clear();
                        });
                        Monitor.Exit(_EventQLock);
                        _ = Parallel.ForEach(saveEventQs, q =>
                        {
                            _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                              {
                                  List<Input> inputs = new List<Input>();
                                  q.Value.ToList().ForEach(x =>
                                  {
                                      Input input = new Input()
                                      {
                                          CameraTime = x.Key,
                                          ChannelId = q.Key,
                                          Value = x.Value,
                                          CreateTime = DateTime.Parse(DateTime.Now.ToString("G")),
                                      };
                                      inputs.Add(input);
                                  });

                                  _ = App.SqlDataInit.sqlSugar.Insertable(inputs).AS("Input" + q.Key).ExecuteCommand();
                              });
                            q.Value.Clear();
                        });
                    }

                    if (Monitor.TryEnter(_SaveDataLock, 10))
                    {
                        Dictionary<int, SignalData> saveSignalQS = null;
                        Copy(_SaveSignalQs, ref saveSignalQS);
                        _ = Parallel.ForEach(_SaveSignalQs, kv =>
                        {
                            foreach (KeyValuePair<int, Queue<KeyValuePair<long, double>>> item in kv.Value.ValuePairs)
                            {
                                item.Value.Clear();
                            }
                        });
                        Monitor.Exit(_SaveDataLock);
                        if (saveSignalQS == null)
                        {
                            continue;
                        }

                        _ = Parallel.ForEach(saveSignalQS, kv =>
                        {
                            List<ChannelRecord> records = new List<ChannelRecord>();
                            CameraChannel channel = CameraChannels.FirstOrDefault(x => x.ChannelId == kv.Key);
                            if (App.SqlDataInit.RecordTablePairs.ContainsKey(channel.Type + channel.ChannelId))
                            {
                                _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                                  {
                                      if (App.SqlDataInit.RecordTablePairs[channel.Type + channel.ChannelId].Contains(SignalSettingsTypeEnum.Camera.ToString()))
                                      {
                                          foreach (var item in kv.Value.ValuePairs)
                                          {
                                              foreach (var value in item.Value)
                                              {
                                                  ChannelRecord record = new ChannelRecord()
                                                  {
                                                      ChannelId = kv.Key,
                                                      Type = item.Key,
                                                      Value = value.Value,
                                                      CameraTime = value.Key,
                                                      CreateTime = DateTime.Parse(DateTime.Now.ToString("G")),
                                                  };
                                                  records.Add(record);
                                              }
                                          }
                                      }
                                      _ = App.SqlDataInit.sqlSugar.Insertable(records).AS(App.SqlDataInit.RecordTablePairs[channel.Type + channel.ChannelId]).ExecuteCommand();
                                  });
                            }
                        });
                    }
                    if (isAdstart)
                    {
                        if (Monitor.TryEnter(_SaveUsbDataLock, 10))
                        {
                            Dictionary<int, SignalDataUsb> saveUsbSignalQS = null;
                            Copy(_SaveUsbSignalQs, ref saveUsbSignalQS);
                            _ = Parallel.ForEach(_SaveUsbSignalQs, kv =>
                            {
                                foreach (KeyValuePair<int, Queue<Tuple<TimeSpan[], double[]>>> item in kv.Value.ValuePairs)
                                {
                                    item.Value.Clear();
                                }
                            });
                            Monitor.Exit(_SaveUsbDataLock);
                            if (saveUsbSignalQS == null)
                            {
                                continue;
                            }
                            _ = Parallel.ForEach(saveUsbSignalQS, kv =>
                            {
                                if (kv.Value.ValuePairs.Count > 0)
                                {
                                    List<ChannelRecord> records = new List<ChannelRecord>();
                                    CameraChannel channel = CameraChannels.FirstOrDefault(x => x.ChannelId == kv.Key);
                                    if (App.SqlDataInit.RecordTablePairs.ContainsKey(channel.Type + channel.ChannelId))
                                    {
                                        _ = App.SqlDataInit.sqlSugar.UseTran(() =>
                                        {

                                            kv.Value.ValuePairs[-1].ToList().ForEach(x =>
                                            {
                                                int i = 0;
                                                x.Item1.ToList().ForEach(v =>
                                                {
                                                    ChannelRecord record = new ChannelRecord()
                                                    {
                                                        ChannelId = kv.Key,
                                                        Type = -1,
                                                        Value = x.Item2[i],
                                                        CameraTime = v.Ticks,
                                                        CreateTime = DateTime.Parse(DateTime.Now.ToString("G")),
                                                    };
                                                    records.Add(record);
                                                    i++;
                                                });
                                            });
                                            _ = App.SqlDataInit.sqlSugar.Insertable(records).AS(App.SqlDataInit.RecordTablePairs[channel.Type + channel.ChannelId]).ExecuteCommand();
                                        });
                                    }
                                }
                            });
                        }
                    }

                }
                finally
                {

                }

            }
        }
        private Tuple<TimeSpan[], double[]> TransposeDataAndRegisterTR(Queue<KeyValuePair<long, double>> sigs)
        {
            KeyValuePair<long, double>[] sig_data = sigs.ToArray();
            try
            {
                double[] t_sigs = new double[sig_data.Length];
                TimeSpan[] t_tims = new TimeSpan[sig_data.Length];
                TimeSpan[] _t_tims = new TimeSpan[sig_data.Length];
                for (int i = 0; i < sig_data.Length; i++)
                {
                    t_sigs[i] = sig_data[i].Value;
                    t_tims[i] = TimeSpan.FromTicks(sig_data[i].Key / 100);
                }
                _t_tims = t_tims.OrderBy(x => x.Ticks).ToArray();
                return new Tuple<TimeSpan[], double[]>(_t_tims, t_sigs);

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            return new Tuple<TimeSpan[], double[]>(new TimeSpan[0], new double[0]);
        }
        private readonly object marker = new object();
        public bool AddMarkerByHotkeys(int channelId, string text, Color color, int type = -1)//type 0 start 1 end -1 other
        {
            //lock (marker)
            {
                TimeSpan _time = new TimeSpan(time / 100);
                if (type == 0)
                {
                    _time = new TimeSpan(0);
                }
                if (EventChannelChart.Annotations.Count > 0)
                {
                    IAnnotationViewModel obj = EventChannelChart.Annotations.LastOrDefault((x) => (x as VerticalLineAnnotationViewModel).LabelValue.Equals(text));
                    if (obj != null)
                    {
                        TimeSpan tick = _time.Subtract((TimeSpan)(obj as VerticalLineAnnotationViewModel).X1);
                        EventChannelJson chn = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == channelId && (x.Type == ChannelTypeEnum.Camera.ToString() || x.Type == ChannelTypeEnum.Analog.ToString() || x.Type == ChannelTypeEnum.Output.ToString()));
                        if (chn != null)
                        {
                            if (tick.TotalMilliseconds < chn.RefractoryPeriod * 1000)
                            {
                                return false;
                            }
                        }
                    }
                }
                EventChannelChart.Annotations.Add(new VerticalLineAnnotationViewModel()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    FontSize = 12,
                    ShowLabel = InperGlobalClass.EventPanelProperties.DisplayNameVisible,
                    Stroke = color,
                    LabelValue = text,
                    LabelTextFormatting = "12",
                    LabelPlacement = LabelPlacement.Left,
                    LabelsOrientation = System.Windows.Controls.Orientation.Vertical,
                    StrokeThickness = 1,
                    X1 = _time
                });
                return true;
            }
        }
        public void SendCommand(EventChannelJson channelJson, int type = -1)
        {
            if (AddMarkerByHotkeys(channelJson.ChannelId, channelJson.Name, (Color)ColorConverter.ConvertFromString(channelJson.BgColor), type))
            {
                device.OuputIO((uint)channelJson.ChannelId, 1);
                Thread.Sleep(50);
                device.OuputIO((uint)channelJson.ChannelId, 0);
            }

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


                if (frameProcTask != null)
                {
                    while (frameProcTask.IsCompleted)
                    {
                        frameProcTask.Dispose();
                        break;
                    }
                }
                if (updateTask != null)
                {
                    while (updateTask.IsCompleted)
                    {
                        updateTask.Dispose();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void StopCollect()
        {
            try
            {
                timer.Stop();
                _DataSaveTimer.Stop();

                if (saveDataTask != null)
                {
                    while (saveDataTask.IsCompleted)
                    {
                        saveDataTask.Dispose();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            finally
            {
                _SaveSignalQs.Clear();
                _SaveUsbSignalQs.Clear();
                _SaveEventQs.Clear();
                device.AdIsCollect(false);
            }
        }
        public bool isAdstart = false;
        public void StartCollect()
        {
            try
            {
                _ = device.SetExposure(InperGlobalClass.CameraSignalSettings.Exposure);
                device.SetFrameRate(InperGlobalClass.CameraSignalSettings.Sampling);

                _adPreTime.Clear();

                uint[] chnn = new uint[4] { 0, 0, 0, 0 };
                CameraChannels.ToList().ForEach(x =>
                {
                    if (x.Type == ChannelTypeEnum.Camera.ToString())
                    {
                        x.RenderableSeries.ToList().ForEach(line =>
                        {
                            (line.DataSeries as XyDataSeries<TimeSpan, double>).Clear();
                            line.DataSeries.FifoCapacity = 20 * 60 * (int)InperGlobalClass.CameraSignalSettings.Sampling;
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
                if (EventChannelChart.RenderableSeries.Count > 0)
                {
                    EventChannelChart.RenderableSeries.ToList().ForEach(line =>
                    {
                        line.DataSeries.FifoCapacity = 20 * 60 * (int)InperGlobalClass.CameraSignalSettings.Sampling;
                    });
                }
                foreach (KeyValuePair<int, SignalData> item in _SignalQs)
                {
                    foreach (KeyValuePair<int, Queue<KeyValuePair<long, double>>> data in item.Value.ValuePairs)
                    {
                        data.Value.Clear();
                    }
                }

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

                isFirstRecordTiem = true; isLoop = true; isfirstADTime = isfirstAD = true;
                _firstADTime = 0;
                timer.Start();
                _DataSaveTimer.Start();
                updateTask = Task.Factory.StartNew(() => { UpdateDataProc(); });
                frameProcTask = Task.Factory.StartNew(() => { FrameProc(); });
                if (isAdstart)
                {
                    _ = Task.Factory.StartNew(() => { UsbAdProc(); });
                    _ = Task.Factory.StartNew(() => { UsbDataPlot(); });
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
        private void Copy<T>(T source, ref T destination) where T : class
        {
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            destination = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonStr);
        }

        public bool AllLightOpen()
        {
            bool isExistLight = false;
            LightWaveLength.ToList().ForEach(x =>
             {
                 if (x.IsChecked)
                 {
                     isExistLight = true;
                     device.SwitchLight((uint)x.GroupId, true);
                     device.SetLightPower((uint)x.GroupId, x.LightPower);
                 }
             });
            return isExistLight;
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
