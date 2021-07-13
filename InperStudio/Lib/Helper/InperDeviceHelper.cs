using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Enum;
using InperStudio.ViewModels;
using InperStudioControlLib.Lib.DeviceAgency;
using InperStudioControlLib.Lib.DeviceAgency.ControlDept;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using Stylet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

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
    public class InperDeviceHelper : PropertyChangedBase
    {
        public static InperDeviceHelper Instance { get; } = new InperDeviceHelper();

        private readonly object _QLock = new object();
        private readonly object _DisplayQLock = new object();
        private readonly AutoResetEvent _AREvent = new AutoResetEvent(false);
        public readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
        private readonly AutoResetEvent _DataEvent = new AutoResetEvent(false);
        public readonly object _SignalQsLocker = new object();
        private readonly object _EventQLock = new object();

        private Task updateTask;
        private Task frameProcTask;
        private bool isFirstRecordTiem = true;
        private bool isLoop = true;
        private long _PlottingStartTime;

        private short[] _SwapBuffer;
        private Queue<long> EventTimeSet = new Queue<long>();
        #region
        public int VisionWidth = 720;
        public int VisionHeight = 540;

        public event EventHandler<bool> WaveInitEvent;

        public Queue<MarkedMat> _MatQ = new Queue<MarkedMat>();
        public Queue<MarkedMat> _DisplayMatQ = new Queue<MarkedMat>();
        /// <summary>
        /// 每个通道对应的数据
        /// </summary>
        public Dictionary<int, SignalData> _SignalQs = new Dictionary<int, SignalData>();
        public ConcurrentDictionary<int, Mat> ROIMasks { get; set; } = new ConcurrentDictionary<int, Mat>();
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
        public void DeviceInit()
        {
            DevPhotometry.Instance.OnImageGrabbed += Instance_OnImageGrabbed;
            DevPhotometry.Instance.OnLightStatusChanged += Instance_OnLightStatusChanged;
            DevPhotometry.Instance.OnPortStatusChanged += Instance_OnPortStatusChanged;

            VisionWidth = DevPhotometry.Instance.VisionWidth;
            VisionHeight = DevPhotometry.Instance.VisionHeight;
            WBMPPreview = new WriteableBitmap(VisionWidth, VisionHeight, 96, 96, PixelFormats.Gray16, null);

            timer.Interval = TimeSpan.FromMilliseconds(20);

            timer.Tick += (s, e) =>
            {
                _ = _DataEvent.Set();
            };
            //这里用来初始化event chart
            if (InperGlobalClass.EventSettings.Channels.Count > 0)
            {
                InperGlobalClass.EventSettings.Channels.ForEach(x =>
                {
                    if (x.Type == EventSettingsTypeEnum.Marker.ToString())
                    {
                        EventChannelChart.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = x.ChannelId, IsDigitalLine = true, DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = (Color)ColorConverter.ConvertFromString(x.BgColor) });
                        EventChannelChart.EventQs.Add(x.ChannelId, new Queue<KeyValuePair<long, double>>());
                    }
                });
            }
        }

        private void Instance_OnPortStatusChanged(object sender, PortStatusChangedEventArgs e)
        {
            if (InperGlobalClass.IsPreview || InperGlobalClass.IsRecord)
            {
                Monitor.Enter(_EventQLock);
                if (EventTimeSet.Count > 0)
                {
                    foreach (KeyValuePair<int, Queue<KeyValuePair<long, double>>> item in EventChannelChart.EventQs)
                    {
                        item.Value.Enqueue(new KeyValuePair<long, double>(EventTimeSet.Last(), e.PortStatus[item.Key]));
                    }
                    EventTimeSet.Clear();
                }
                //Console.WriteLine(" value0:" + e.PortStatus[0] + " value1:" + e.PortStatus[1] + " value2:" + e.PortStatus[2] + " value3:" + e.PortStatus[3] + " value4:" + e.PortStatus[4] + " value5:" + e.PortStatus[5] + " value6:" + e.PortStatus[6] + " value7:" + e.PortStatus[7]);
                Monitor.Exit(_EventQLock);
            }

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
        private void Instance_OnLightStatusChanged(object sender, LightStatusChangedEventArgs e)
        {
            try
            {
                if (e.Light0WaveLength > 0)
                {
                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == 0);
                    if (wg != null)
                    {
                        LightWaveLength.Add(wg);
                        DevPhotometry.Instance.SwitchLight(wg.GroupId, true);
                        DevPhotometry.Instance.SetLightPower(wg.GroupId, wg.LightPower);
                    }
                    else
                    {
                        LightWaveLength.Add(new WaveGroup() { GroupId = 0, WaveType = e.Light0WaveLength + " nm" });
                    }
                }
                if (e.Light1WaveLength > 0)
                {
                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == 1);
                    if (wg != null)
                    {
                        LightWaveLength.Add(wg);
                        DevPhotometry.Instance.SwitchLight(wg.GroupId, true);
                        DevPhotometry.Instance.SetLightPower(wg.GroupId, wg.LightPower);
                    }
                    else
                    {
                        LightWaveLength.Add(new WaveGroup() { GroupId = 1, WaveType = e.Light1WaveLength + " nm" });
                    }
                }
                if (e.Light2WaveLength > 0)
                {
                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == 2);
                    if (wg != null)
                    {
                        LightWaveLength.Add(wg);
                        DevPhotometry.Instance.SwitchLight(wg.GroupId, true);
                        DevPhotometry.Instance.SetLightPower(wg.GroupId, wg.LightPower);
                    }
                    else
                    {
                        LightWaveLength.Add(new WaveGroup() { GroupId = 2, WaveType = e.Light2WaveLength + " nm" });
                    }
                }
                if (e.Light3WaveLength > 0)
                {
                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == 3);
                    if (wg != null)
                    {
                        LightWaveLength.Add(wg);
                        DevPhotometry.Instance.SwitchLight(wg.GroupId, true);
                        DevPhotometry.Instance.SetLightPower(wg.GroupId, wg.LightPower);
                    }
                    else
                    {
                        LightWaveLength.Add(new WaveGroup() { GroupId = 3, WaveType = e.Light3WaveLength + " nm" });
                    }
                }
                WaveInitEvent?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }

        public void DisplayProc()
        {
            _SwapBuffer = new short[DevPhotometry.Instance.VisionWidth * DevPhotometry.Instance.VisionHeight];

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

                if (mmats.Count() > 0)
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

                        EventTimeSet.Enqueue(ts);

                        ConcurrentDictionary<int, double> ploting_data = new ConcurrentDictionary<int, double>();
                        if (CameraChannels.Count > 0)
                        {
                            _ = Parallel.ForEach(CameraChannels, mask =>
                              {
                                  double r = (double)m.ImageMat.Mean(mask.Mask) / 655.35;

                                  ploting_data[mask.ChannelId] = r;
                              });

                            this.AppendData(ploting_data, m.Group, ts);
                        }
                    }
                }
            }
        }

        private void AppendData(ConcurrentDictionary<int, double> data, int s_group, long timestamp)
        {

            if (Monitor.TryEnter(_SignalQsLocker))
            {
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

            }

            return;
        }
        public void UpdateDataProc()
        {
            while (isLoop)
            {
                _ = _DataEvent.WaitOne();
                Monitor.Enter(_SignalQsLocker);
                try
                {
                    object synSpan = new object();
                    if (CameraChannels.Count > 0)
                    {
                        _ = Parallel.ForEach(_SignalQs, kv =>
                        {
                            CameraChannel channel = CameraChannels.FirstOrDefault(x => x.ChannelId == kv.Key);
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
                                             synSpan = s0_plot_data.Item1;
                                             kv.Value.ValuePairs[id].Clear();
                                             (item.DataSeries as XyDataSeries<TimeSpan, double>).Append(s0_plot_data.Item1, s0_plot_data.Item2);
                                         }
                                     }
                                 });
                            }
                        });
                        if (Monitor.TryEnter(_EventQLock))
                        {
                            _ = Parallel.ForEach(EventChannelChart.EventQs, q =>
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
                                                q.Value.Clear();

                                                if (s0_plot_data.Item1.Count() == 0)
                                                {
                                                    if ((synSpan as TimeSpan[]) != null)
                                                    {
                                                        int length = (synSpan as TimeSpan[]).Length;
                                                        s0_plot_data = new Tuple<TimeSpan[], double[]>(new TimeSpan[length], new double[length]);

                                                        for (int i = 0; i < (synSpan as TimeSpan[]).Length; i++)
                                                        {
                                                            s0_plot_data.Item1[i] = (synSpan as TimeSpan[])[i];
                                                            s0_plot_data.Item2[i] = item.DataSeries.YValues.Count > 0 ? (double)item.DataSeries.YValues[item.DataSeries.YValues.Count - 1] : 0;
                                                        }
                                                    }
                                                }
                                                (item.DataSeries as XyDataSeries<TimeSpan, double>).Append(s0_plot_data.Item1, s0_plot_data.Item2);

                                            }
                                        }
                                    });
                                }
                            });
                            Monitor.Exit(_EventQLock);
                        }

                    }
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex.ToString());
                }
                finally
                {
                    Monitor.Exit(_SignalQsLocker);
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
                for (int i = 0; i < sig_data.Length; i++)
                {
                    t_sigs[i] = Math.Round(sig_data[i].Value, 2);
                    t_tims[i] = TimeSpan.FromTicks(sig_data[i].Key / 100);
                }
                return new Tuple<TimeSpan[], double[]>(t_tims, t_sigs);

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            return new Tuple<TimeSpan[], double[]>(new TimeSpan[0], new double[0]);
        }

        public void StopCollect()
        {
            try
            {
                isFirstRecordTiem = false; isLoop = false;
                timer.Stop();
                if (frameProcTask != null)
                {
                    while (frameProcTask.IsCompleted)
                    {
                        frameProcTask.Dispose();
                    }
                }
                if (updateTask != null)
                {
                    while (updateTask.IsCompleted)
                    {
                        updateTask.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void StartCollect()
        {
            try
            {
                CameraChannels.ToList().ForEach(x =>
                {
                    x.RenderableSeries.ToList().ForEach(line =>
                    {
                        (line.DataSeries as XyDataSeries<TimeSpan, double>).Clear();
                    });
                });
                foreach (KeyValuePair<int, SignalData> item in _SignalQs)
                {
                    foreach (KeyValuePair<int, Queue<KeyValuePair<long, double>>> data in item.Value.ValuePairs)
                    {
                        data.Value.Clear();
                    }
                }
                isFirstRecordTiem = true; isLoop = true;
                timer.Start();
                updateTask = Task.Factory.StartNew(() => { UpdateDataProc(); });
                frameProcTask = Task.Factory.StartNew(() => { FrameProc(); });
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
    }
}
