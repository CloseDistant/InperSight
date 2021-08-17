using HandyControl.Controls;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.ViewModels;
using InperStudioControlLib.Lib.DeviceAgency;
using InperStudioControlLib.Lib.DeviceAgency.ControlDept;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Annotations;
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
using System.Web.UI.WebControls;
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
        #region
        public int VisionWidth = 720;
        public int VisionHeight = 540;
        public int SelectedWaveType = 1;

        public event EventHandler<bool> WaveInitEvent;

        public Queue<MarkedMat> _MatQ = new Queue<MarkedMat>();
        public Queue<MarkedMat> _DisplayMatQ = new Queue<MarkedMat>();
        /// <summary>
        /// 每个通道对应的数据
        /// </summary>
        public Dictionary<int, SignalData> _SignalQs = new Dictionary<int, SignalData>();
        public ConcurrentDictionary<int, Mat> ROIMasks { get; set; } = new ConcurrentDictionary<int, Mat>();
        public ConcurrentDictionary<int, Dictionary<int, Queue<double>>> FilterData { get; set; } = new ConcurrentDictionary<int, Dictionary<int, Queue<double>>>();
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
            if (DevPhotometry.Instance.VisionWidth == 0)
            {
                HandyControl.Controls.Growl.Error("Failed to initialize the basler camera", "SuccessMsg");
                return;
            }
            WBMPPreview = new WriteableBitmap(VisionWidth, VisionHeight, 96, 96, PixelFormats.Gray16, null);

            timer.Interval = TimeSpan.FromMilliseconds(20);

            timer.Tick += (s, e) =>
            {
                _ = _DataEvent.Set();
            };

            _DataSaveTimer.Interval = TimeSpan.FromSeconds(3);
            _DataSaveTimer.Tick += (s, e) =>
            {
                _ = _DataSaveEvent.Set();
            };

        }
        public bool InitDataStruct()
        {
            try
            {
                if (_SignalQs.Count > 0)
                {
                    FilterData.Clear();
                    foreach (var item in CameraChannels)
                    {
                        item.Offset = false;
                        _SaveSignalQs.Add(item.ChannelId, new SignalData());
                        _ = FilterData.TryAdd(item.ChannelId, new Dictionary<int, Queue<double>>());
                        if (item.LightModes.Count > 0)
                        {
                            item.LightModes.ForEach(x =>
                            {
                                x.OffsetValue = 0;
                                _SaveSignalQs[item.ChannelId].ValuePairs.Add(x.LightType, new Queue<KeyValuePair<long, double>>());
                                FilterData[item.ChannelId].Add(x.LightType, new Queue<double>());
                            });
                        }
                        else
                        {
                            _SaveSignalQs[item.ChannelId].ValuePairs.Add(-1, new Queue<KeyValuePair<long, double>>());// 模拟通道只有一种信号
                        }
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
                        if (x.Type == EventSettingsTypeEnum.Marker.ToString())
                        {
                            EventChannelChart.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = x.ChannelId, IsDigitalLine = true, DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = (Color)ColorConverter.ConvertFromString(x.BgColor) });
                            if (!EventChannelChart.EventQs.ContainsKey(x.ChannelId))
                            {
                                EventChannelChart.EventQs.Add(x.ChannelId, new Queue<KeyValuePair<long, double>>());
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
                    if (InperGlobalClass.IsRecord)
                    {
                        foreach (KeyValuePair<int, Queue<KeyValuePair<long, double>>> item in _SaveEventQs)
                        {
                            item.Value.Enqueue(new KeyValuePair<long, double>(EventTimeSet.Last(), e.PortStatus[item.Key]));
                        }
                    }
                    EventTimeSet.Clear();
                }

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
                bool exist = false;
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
                    exist = true;
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
                    exist = true;
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
                    exist = true;
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
                    exist = true;
                }
                WaveInitEvent?.Invoke(this, exist);
                if (!exist)
                    Growl.Error("未获取到激发光");
            }
            catch (Exception ex)
            {
                Growl.Error("获取激发光失败：" + ex.ToString());
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

                if (mmats.Count() > 0 && mmats.Last().Group == SelectedWaveType)
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
                                  double r = (double)m.ImageMat.Mean(mask.Mask) / 655.35;
                                  r -= Offset(mask, m.Group);

                                  r = Smooth(mask, m.Group, r);

                                  DeltaFCalculate(mask, m.Group, r, ts);

                                  ploting_data[mask.ChannelId] = r;
                              });

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
        #region 滤波 offset smooth
        private double Offset(CameraChannel cameraChannel, int group)
        {
            double value = 0;
            if (cameraChannel.Offset)
            {
                LightMode<TimeSpan, double> offsetValue = cameraChannel.LightModes.FirstOrDefault(x => x.LightType == group);
                XyDataSeries<TimeSpan, double> linedata = offsetValue.XyDataSeries;
                if (linedata.Count > cameraChannel.OffsetWindowSize && offsetValue.OffsetValue == 0)
                {
                    offsetValue.OffsetValue = linedata.YValues.ToList().GetRange(linedata.Count - cameraChannel.OffsetWindowSize, cameraChannel.OffsetWindowSize).Average();
                }
                value = offsetValue.OffsetValue;
            }
            return value;
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
                        FilterData[cameraChannel.ChannelId][group].Dequeue();
                    }
                    if (cameraChannel.Filters.IsSmooth)
                    {
                        val = FilterData[cameraChannel.ChannelId][group].ToList().Average();
                    }
                }
            }
            return val;
        }
        private void DeltaFCalculate(CameraChannel cameraChannel, int group, double r, long ts)
        {
            InperGlobalClass.EventSettings.Channels.ForEach(x =>
            {
                if (x.IsActive)
                {
                    if (x.Type == ChannelTypeEnum.Camera.ToString() || x.Type == ChannelTypeEnum.Analog.ToString())
                    {
                        if (x.ChannelId == cameraChannel.ChannelId)
                        {
                            _ = Parallel.ForEach(cameraChannel.LightModes, mode =>
                              {
                                  if (mode.LightType == group)
                                  {
                                      double deltaF = mode.Derivative.ProcessSignal(r);
                                      if (deltaF >= (x.DeltaF / 100))
                                      {
                                          WaveGroup light = LightWaveLength.FirstOrDefault(l => l.GroupId == group);
                                          AddMarkerByHotkeys(cameraChannel.ChannelId, x.Name + light.WaveType, (Color)ColorConverter.ConvertFromString(x.BgColor));
                                          AIROI aIROI = new AIROI()
                                          {
                                              ChannelId = cameraChannel.ChannelId,
                                              CameraTime = ts,
                                              DeltaF = deltaF,
                                              Type = x.Type == ChannelTypeEnum.Camera.ToString() ? 0 : 1,
                                              CreateTime = DateTime.Now
                                          };
                                          _ = App.SqlDataInit.sqlSugar.Insertable(aIROI).ExecuteCommand();
                                      }
                                  }
                              });
                        }
                    }
                }
            });
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
                        Monitor.Exit(_SignalQsLocker);

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
                                                if (s0_plot_data.Item1.Count() != 0)
                                                {
                                                    (item.DataSeries as XyDataSeries<TimeSpan, double>).Append(s0_plot_data.Item1, s0_plot_data.Item2);
                                                }
                                                q.Value.Clear();
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
            }
        }
        public void SaveDateProc()
        {
            while (InperGlobalClass.IsRecord)
            {
                _ = _DataSaveEvent.WaitOne();
                if (Monitor.TryEnter(_EventQLock))
                {
                    Dictionary<int, Queue<KeyValuePair<long, double>>> saveEventQs = null;
                    Copy(_SaveEventQs, ref saveEventQs);
                    _ = Parallel.ForEach(_SaveEventQs, q =>
                    {
                        saveEventQs.Add(q.Key, q.Value);
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
                        foreach (var item in kv.Value.ValuePairs)
                        {
                            item.Value.Clear();
                        }
                    });
                    Monitor.Exit(_SaveDataLock);
                    if (saveSignalQS == null) return;
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
                                  else
                                  {
                                      kv.Value.ValuePairs[-1].ToList().ForEach(x =>
                                      {
                                          ChannelRecord record = new ChannelRecord()
                                          {
                                              ChannelId = kv.Key,
                                              Type = -1,
                                              Value = x.Value,
                                              CameraTime = x.Key,
                                              CreateTime = DateTime.Parse(DateTime.Now.ToString("G")),
                                          };
                                          records.Add(record);
                                      });
                                  }
                                  _ = App.SqlDataInit.sqlSugar.Insertable(records).AS(App.SqlDataInit.RecordTablePairs[channel.Type + channel.ChannelId]).ExecuteCommand();
                              });
                        }
                    });
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
                t_tims.OrderBy(x => x.Ticks);
                return new Tuple<TimeSpan[], double[]>(t_tims, t_sigs);

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            return new Tuple<TimeSpan[], double[]>(new TimeSpan[0], new double[0]);
        }
        public void AddMarkerByHotkeys(int channelId, string text, Color color)
        {
            //int count = EventChannelChart.RenderableSeries.First().DataSeries.XValues.Count;
            int count = CameraChannels[0].RenderableSeries.First().DataSeries.XValues.Count;

            EventChannelChart.Annotations.Add(new VerticalLineAnnotationViewModel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                FontSize = 12,
                ShowLabel = true,
                Stroke = color,
                LabelValue = text,
                LabelTextFormatting = "12",
                LabelPlacement = LabelPlacement.Left,
                LabelsOrientation = System.Windows.Controls.Orientation.Vertical,
                StrokeThickness = 1,
                X1 = (IComparable)CameraChannels[0].RenderableSeries.First().DataSeries.XValues[count - 1]
            });
            //TimeSpan time = (TimeSpan)CameraChannels[0].RenderableSeries.First().DataSeries.XValues[count - 1];

            //Manual manual = new Manual()
            //{
            //    ChannelId = channelId,
            //    Color = color.ToString(),
            //    CameraTime = time.Ticks,
            //    Name = text,
            //    Type = type,
            //    DateTime = DateTime.Parse(DateTime.Now.ToString("G"))
            //};

            //_ = (App.SqlDataInit?.sqlSugar.Insertable(manual).ExecuteCommand());
        }
        public void SendCommand()
        {

        }
        public void StopCollect()
        {
            try
            {
                isFirstRecordTiem = false; isLoop = false;
                timer.Stop();
                _DataSaveTimer.Stop();
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
                _SaveEventQs.Clear();
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
                _DataSaveTimer.Start();

                updateTask = Task.Factory.StartNew(() => { UpdateDataProc(); });
                frameProcTask = Task.Factory.StartNew(() => { FrameProc(); });
                //saveDataTask = Task.Factory.StartNew(() => { SaveDateProc(); });
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void Copy<T>(T source, ref T destination) where T : class
        {
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            destination = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonStr);
        }

        public void AllLightOpen()
        {
            LightWaveLength.ToList().ForEach(x =>
             {
                 DevPhotometry.Instance.SwitchLight(x.GroupId, true);
                 DevPhotometry.Instance.SetLightPower(x.GroupId, x.LightPower);
             });
        }
        public void AllLightClose()
        {
            LightWaveLength.ToList().ForEach(x =>
            {
                DevPhotometry.Instance.SwitchLight(x.GroupId, false);
            });
        }
    }
}
