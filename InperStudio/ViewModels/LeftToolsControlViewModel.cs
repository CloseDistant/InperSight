using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.ImageRecognition;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudioControlLib.Control.TextBox;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using InperStudioControlLib.Lib.Config;
using SciChart.Core.Extensions;
using SqlSugar;

namespace InperStudio.ViewModels
{
    public class LeftToolsControlViewModel : Screen
    {
        #region properties
        public LeftToolsControlView view;
        private IWindowManager windowManager;
        public List<Grid> EllipseBorder = new List<Grid>();
        private System.Windows.Point startPoint;
        private bool isDown = false;
        private int diameter = 45;
        private Grid moveGrid = null;
        public Grid MoveGrid { get => moveGrid; set => SetAndNotify(ref moveGrid, value); }
        public int Diameter { get => diameter; set => SetAndNotify(ref diameter, value); }
        //
        private BindableCollection<SignalCameraChannel> analogChannels = new BindableCollection<SignalCameraChannel>();
        public BindableCollection<SignalCameraChannel> AnalogChannels { get => analogChannels; set => SetAndNotify(ref analogChannels, value); }

        private BindableCollection<SignalCameraChannel> analogActiveChannels = new BindableCollection<SignalCameraChannel>();
        public BindableCollection<SignalCameraChannel> AnalogActiveChannels { get => analogActiveChannels; set => SetAndNotify(ref analogActiveChannels, value); }
        public List<string> AnalogColorList { get; set; } = InperColorHelper.ColorPresetList;
        private InperDeviceHelper deviceHelper;
        public InperDeviceHelper InperDeviceHelper { get => deviceHelper; set => SetAndNotify(ref deviceHelper, value); }
        public List<string> Exposures { get; set; } = InperParameters.Exposures;
        //private List<double> fps = InperParameters.FPS.ToList();
        public List<double> FPS { get; set; } = InperParameters.FPS.ToList();
        public bool IsContinuous { get; set; } = InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous;
        public bool IsInterval { get; set; } = InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval;
        private double sampling = InperGlobalClass.CameraSignalSettings.Sampling;
        public double Sampling { get => sampling; set => SetAndNotify(ref sampling, value); }
        private string expourse = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
        public string Expourse { get => expourse; set => SetAndNotify(ref expourse, value); }
        #endregion

        #region
        public LeftToolsControlViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            InperDeviceHelper = InperDeviceHelper.Instance;
        }
        protected override void OnViewLoaded()
        {
            view = this.View as LeftToolsControlView;
            InitConfig();
            base.OnViewLoaded();
        }
        public void InitConfig(bool isImport = false)
        {
            if (AnalogChannels.Count < 1)
            {
                for (int i = 0; i < 8; i++)
                {
                    AnalogChannels.Add(new SignalCameraChannel() { ChannelId = i + 101, NickName = "AI-" + (i + 1), Name = "AI-" + (i + 1) + "-", BgColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[i])) });
                }
            }
            if (isImport)
            {
                foreach (var item in AnalogActiveChannels)
                {
                    AnalogChannels.Insert(item.ChannelId - 101, item);
                }
                AnalogActiveChannels.Clear();
                this.view.ellipseCanvas.Children.Clear();
                EllipseBorder.Clear();
                CameraShow();

                //InperDeviceHelper.Instance.CameraChannels.Clear();
                view.Exposure.Text = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
                view.Sampling.Text = InperGlobalClass.CameraSignalSettings.Sampling.ToString();
                view.continuous.IsChecked = InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous;
                view.interval.IsChecked = InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval;
                view._duration.Text = InperGlobalClass.CameraSignalSettings.RecordMode.Duration.ToString();
                view._interval.Text = InperGlobalClass.CameraSignalSettings.RecordMode.Interval.ToString();
                view._sampling.Text = InperGlobalClass.CameraSignalSettings.AiSampling.ToString();
            }
            Init(isImport);
            InperGlobalClass.CameraSignalSettings.CameraChannels?.ForEach(x =>
            {
                //if (AnalogActiveChannels.Count(c => c.ChannelId == x.ChannelId) == 0)
                {
                    if (x.Type == ChannelTypeEnum.Analog.ToString() && InperGlobalClass.IsDisplayAnalog)
                    {
                        var chn = AnalogChannels.FirstOrDefault(c => c.ChannelId == x.ChannelId);
                        if (chn != null)
                        {
                            if (chn.Name.EndsWith("-"))
                            {
                                chn.Name = chn.Name.Substring(0, chn.Name.Length - 1);
                            }
                        }
                        if (analogActiveChannels.Count(c => c.ChannelId == x.ChannelId) == 0)
                        {
                            AnalogActiveChannels.Add(chn);
                        }
                        _ = AnalogChannels.Remove(chn);
                        var an = new SignalCameraChannel()
                        {
                            ChannelId = x.ChannelId,
                            ChannelType = x.Type,
                            Name = x.Name,
                            YaxisDoubleRange = new SciChart.Data.Model.DoubleRange(x.YBottom, x.YTop),
                            BgColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(x.Color))
                        };

                        if (InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(c => c.ChannelId == x.ChannelId && c.Type == x.Type) == null)
                        {
                            CameraChannel item = new CameraChannel()
                            {
                                ChannelId = x.ChannelId,
                                Name = x.Name,
                                YVisibleRange = new SciChart.Data.Model.DoubleRange(x.YBottom, x.YTop),
                                Type = ChannelTypeEnum.Analog.ToString()
                            };
                            item.TimeSpanAxis = new TimeSpanAxis()
                            {
                                DrawMajorBands = false,
                                DrawMajorGridLines = false,
                                DrawMinorGridLines = false,
                                VisibleRange = item.XVisibleRange,
                                Visibility = Visibility.Collapsed
                            };
                            item.Offset = x.Offset;
                            item.OffsetWindowSize = x.OffsetWindowSize;

                            item.Filters = new Filters()
                            {
                                HighPass = x.Filters.HighPass,
                                IsHighPass = x.Filters.IsHighPass,
                                IsLowPass = x.Filters.IsLowPass,
                                IsNotch = x.Filters.IsNotch,
                                IsSmooth = x.Filters.IsSmooth,
                                LowPass = x.Filters.LowPass,
                                Notch = x.Filters.Notch,
                                Smooth = x.Filters.Smooth
                            };
                            item.LightModes.Add(new LightMode<TimeSpan, double>()
                            {
                                LightType = -1,
                                OffsetValue = double.Parse(x.OffsetValue)
                            });
                            //item.TimeSpanAxis.VisibleRangeChanged += InperDeviceHelper.Instance.TimeSpanAxis_VisibleRangeChanged;

                            LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = "-1", YAxisId = "Ch0", DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = (Color?)ColorConverter.ConvertFromString(x.Color) };
                            item.RenderableSeries.Add(line);

                            InperDeviceHelper.Instance.CameraChannels.Add(item);
                        }
                    }
                }
            });
            if (AnalogChannels.Count > 0)
            {
                view.PopButton.Background = AnalogChannels?.FirstOrDefault().BgColor;
                view.AnalogChannelCombox.SelectedIndex = 0;
            }
            InperGlobalClass.CameraSignalSettings.CameraChannels.OrderBy(x => x.ChannelId);
        }
        public void CameraShow()
        {
            view.camera.Visibility = Visibility.Visible;
            view.analog.Visibility = Visibility.Collapsed;
            //状态切换
            RoutedEventArgs eventArgs = new RoutedEventArgs(Button.ClickEvent, view._camera);
            view._camera.RaiseEvent(eventArgs);
        }
        public void AnalogShow()
        {
            view.analog.Visibility = Visibility.Visible;
            view.camera.Visibility = Visibility.Collapsed;

        }
        #endregion

        #region fibers
        public void InperTextBox_InperTextChanged(object arg1, TextChangedEventArgs arg2)
        {
            try
            {
                InperTextBox sen = arg1 as InperTextBox;
                if (sen.Name.Equals("duration"))
                {
                    //_Metronome.Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000 == 0 ? 5 * 1000 : InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000;
                }
                else
                {
                    //_Metronome.Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Interval * 1000 == 0 ? 5 * 1000 : InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Exposure_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.ComboBox com = sender as System.Windows.Controls.ComboBox;
                if (!string.IsNullOrEmpty(com.Text))
                {
                    if (double.Parse(com.Text) < 0)
                    {
                        string text = "不能小于0";
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "Can't be less than 0";
                        }
                        Growl.Warning(text, "SuccessMsg");
                        com.Text = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
                        return;
                    }
                    if (double.Parse(com.Text) > 100)
                    {
                        string text = "不能大于100";
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "Not more than 100";
                        }
                        Growl.Warning(text, "SuccessMsg");
                        com.Text = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
                        return;
                    }
                    Regex rx = new Regex(@"^([0-9]{1,})$");
                    if (!rx.IsMatch(com.Text))
                    {
                        com.Text = Math.Floor(double.Parse(com.Text)).ToString();
                        return;
                    }
                    this.view.Exposure.Text = Expourse = com.Text;
                    _ = InperDeviceHelper.Instance.device.SetExposure(double.Parse(com.Text));

                    InperDeviceHelper.Instance.device.SendExposure((uint)InperGlobalClass.CameraSignalSettings.Exposure);

                    InperGlobalClass.CameraSignalSettings.Exposure = double.Parse(com.Text);
                    if (InperGlobalClass.CameraSignalSettings.Exposure * InperGlobalClass.CameraSignalSettings.Sampling * (InperGlobalClass.CameraSignalSettings.LightMode.Count < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count) > 1000)
                    {
                        InperGlobalClass.CameraSignalSettings.Sampling = Math.Floor(1000 / InperGlobalClass.CameraSignalSettings.Exposure / (InperGlobalClass.CameraSignalSettings.LightMode.Count < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count));
                        Sampling = InperGlobalClass.CameraSignalSettings.Sampling;
                        //InperDeviceHelper.Instance.device.SetFrameRate(InperGlobalClass.CameraSignalSettings.Sampling);
                        InperGlobalClass.SetSampling(Sampling);
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.Warning(new GrowlInfo() { Message = "Error!", Token = "SuccessMsg", WaitTime = 1 });
                Expourse = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
                (sender as System.Windows.Controls.ComboBox).Text = Expourse;
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Sampling_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.ComboBox com = sender as System.Windows.Controls.ComboBox;
                if (!string.IsNullOrEmpty(com.Text))
                {
                    if (double.Parse(com.Text) < 1)
                    {
                        string text = "不能小于1";
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "Can't be less than 1";
                        }
                        Growl.Warning(text, "SuccessMsg");
                        //com.Text = InperGlobalClass.CameraSignalSettings.Sampling.ToString();
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Sampling = 1;
                        }));
                        return;
                    }
                    if (double.Parse(com.Text) > 300 / (InperGlobalClass.CameraSignalSettings.LightMode.Count < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count))
                    {
                        string text = "不能大于 " + (300 / (InperGlobalClass.CameraSignalSettings.LightMode.Count < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count));
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "Can't be greater than " + (300 / (InperGlobalClass.CameraSignalSettings.LightMode.Count < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count));
                        }
                        Growl.Warning(text, "SuccessMsg");
                        //com.Text = InperGlobalClass.CameraSignalSettings.Sampling.ToString();
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Sampling = 100;
                        }));
                        return;
                    }
                    Regex rx = new Regex(@"^([0-9]{1,})$");
                    if (!rx.IsMatch(com.Text))
                    {
                        com.Text = Math.Floor(double.Parse(com.Text)).ToString();
                        return;
                    }
                    InperGlobalClass.SetSampling(Sampling);
                    if (InperGlobalClass.CameraSignalSettings.Exposure * InperGlobalClass.CameraSignalSettings.Sampling * (InperGlobalClass.CameraSignalSettings.LightMode.Count < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count) > 1000)
                    {
                        InperGlobalClass.CameraSignalSettings.Exposure = Math.Floor(1000 / (InperGlobalClass.CameraSignalSettings.Sampling * (InperGlobalClass.CameraSignalSettings.LightMode.Count < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count)));
                        this.view.Exposure.Text = Expourse = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
                        _ = InperDeviceHelper.Instance.device.SetExposure(double.Parse(Expourse));
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.Warning(new GrowlInfo() { Message = "Error!", Token = "SuccessMsg", WaitTime = 1 });
                Sampling = InperGlobalClass.CameraSignalSettings.Sampling;
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private async void Init(bool isImport = false)
        {
            try
            {
                if (InperGlobalClass.IsStop)
                {
                    view.lightTestMode.IsChecked = true;
                    await SetExpourseAndGain();
                }
                //view.camera.Visibility = Visibility.Visible;
                CameraShow();
                view.channelRoi.IsEnabled = false;
                view.waveView.IsEnabled = false;
                InperDeviceHelper.Instance.SelectedWaveType = -1;
                if (InperDeviceHelper.Instance.LightWaveLength.Count > 0)
                {
                    view.waveView.SelectedItem = InperDeviceHelper.Instance.LightWaveLength.Count > 1
                        ? InperDeviceHelper.Instance.LightWaveLength[1]
                        : InperDeviceHelper.Instance.LightWaveLength[0];
                    view.lightMode.ItemsSource = null;
                    view.lightMode.ItemsSource = InperDeviceHelper.Instance.LightWaveLength;
                }
                InperGlobalClass.CameraSignalSettings.CameraChannels?.ForEach(x =>
                {
                    if (x.Type == ChannelTypeEnum.Camera.ToString())
                    {
                        Grid grid = DrawCircle(x.ChannelId + 1, x.ROI, x.YTop, x.YBottom, x);
                        if (grid != null)
                        {
                            _ = view.ellipseCanvas.Children.Add(grid);
                        }
                    }
                });
                if (InperGlobalClass.IsStop)
                {
                    if (!isImport)
                    {
                        AutoFindFiber();
                    }
                    _ = InperDeviceHelper.Instance.device.SetGain(InperGlobalClass.CameraSignalSettings.Gain);
                    _ = InperDeviceHelper.Instance.device.SetExposure(InperGlobalClass.CameraSignalSettings.Exposure);

                    if (InperDeviceHelper.Instance.LightWaveLength.Count(c => c.IsChecked) == 0)
                    {
                        InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                        {
                            if (x.GroupId < 2)
                            {
                                x.IsChecked = true;
                                x.LightPower = 40;
                                SetLightStatuAndSampling(x);
                            }
                            else
                            {
                                x.LightPower = 40;
                            }
                        });
                    }
                }

                view.Exposure.ItemsSource = Exposures;
                view.Sampling.ItemsSource = FPS;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
            finally
            {
                if (InperGlobalClass.IsStop)
                {
                    view.lightTestMode.IsChecked = false;
                }
            }
        }
        public async void AutoFindFiberRest()
        {
            try
            {
                int count = InperDeviceHelper.CameraChannels.Count;
                for (int i = 0; i < count; i++)
                {
                    ReduceCircle(null, null);
                }
                await SetExpourseAndGain();
                AutoFindFiber();
                _ = InperDeviceHelper.Instance.device.SetGain(InperGlobalClass.CameraSignalSettings.Gain);
                _ = InperDeviceHelper.Instance.device.SetExposure(InperGlobalClass.CameraSignalSettings.Exposure);
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private void AutoFindFiber()
        {
            #region 自动识别光纤端面
            if (InperGlobalClass.CameraSignalSettings.CameraChannels.Count(x => x.Type == ChannelTypeEnum.Camera.ToString()) == 0)
            {
                //_ = InperDeviceHelper.Instance.device.SetGain(20);

                //_ = InperDeviceHelper.Instance.device.SetExposure(10);

                int _loopCount = 0;
                while (true)
                {
                    if (!InperDeviceHelper.Instance._ImageShowMat.Empty())
                    {
                        Mat mat = InperDeviceHelper.Instance._ImageShowMat.Clone();
                        CircleSegment[] centers = AutoFocusHelper.GetCirclesCenterPoint(mat);
                        double scaleX = view.ellipseCanvas.Width / mat.Width;
                        double scaleY = view.ellipseCanvas.Height / mat.Height;
                        int radius = 0;
                        if (centers.Length < 5 && centers.Length != 0)
                        {
                            for (int i = 0; i < centers.Length; i++)
                            {
                                if (radius <= 0)
                                {
                                    radius = (int)Math.Floor(centers[i].Radius * 0.85);
                                }
                                else
                                {
                                    radius = radius < (int)Math.Floor(centers[i].Radius * 0.85) ? radius : (int)Math.Floor(centers[i].Radius * 0.85);
                                }
                                if (radius < 110)
                                {
                                    Grid grid = DrawCircle(i + 1, radius, 10, 0, new Channel() { Offset = false, Top = centers[i].Center.Y * scaleY - radius / 2, Left = centers[i].Center.X * scaleY - radius / 2 });
                                    if (grid != null)
                                    {
                                        this.view.ellipseCanvas.Children.Add(grid);
                                    }
                                }
                            }
                            break;
                        }
                    }
                    _loopCount++;
                    if (_loopCount > 10)
                    {
                        //InperGlobalClass.ShowReminderInfo("未找到光纤端面，请手动添加");
                        break;
                    }
                    Thread.Sleep(10);
                }
            }

            #endregion
        }
        public void AddCircle(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EllipseBorder.Count < 9)
                {
                    int index = EllipseBorder.Count == 0 ? 1 : int.Parse((EllipseBorder.Last().Children[0] as TextBlock).Text) + 1;
                    Grid grid = DrawCircle(index, Diameter, 10, 0);
                    if (grid != null)
                    {
                        this.view.ellipseCanvas.Children.Add(grid);
                    }
                    foreach (var item in view.lightMode.Items)
                    {
                        var listBoxItem = view.lightMode.ItemContainerGenerator.ContainerFromItem(item) as Visual;
                        var checkbox = InperClassHelper.FindVisualChild<CheckBox>(listBoxItem);
                        if (checkbox != null)
                        {
                            if (checkbox.IsChecked == false)
                            {
                                checkbox.IsChecked = true;
                                checkbox.IsChecked = false;
                            }
                        }
                    }
                }
                else
                {
                    //Growl.Info(new GrowlInfo() { Message = "No more than 9 channels", Token = "SuccessMsg", WaitTime = 1 });
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void WaveView_Selected(object sender, RoutedEventArgs e)
        {
            if ((bool)view.lightTestMode.IsChecked)
            {
                InperDeviceHelper.Instance.SelectedWaveType = ((sender as System.Windows.Controls.ComboBox).SelectedItem as WaveGroup).GroupId;
            }
        }
        public void ReduceCircle(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InperGlobalClass.IsPreview && EllipseBorder.Count == 1)
                {
                    string text = "至少存在一个ROI";
                    if (InperConfig.Instance.Language == "en_us")
                    {
                        text = "At least one ROI exists";
                    }
                    InperGlobalClass.ShowReminderInfo(text);
                    return;
                }
                if (EllipseBorder.Count > 0)
                {
                    if (moveGrid != null)
                    {
                        view.ellipseCanvas.Children.Remove(moveGrid);
                        _ = EllipseBorder.Remove(moveGrid);

                        CameraChannel item = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1 && x.Type == ChannelTypeEnum.Camera.ToString());

                        Monitor.Enter(InperDeviceHelper.Instance._FrameProcLock);
                        _ = InperDeviceHelper.Instance.CameraChannels.Remove(item);
                        Monitor.Exit(InperDeviceHelper.Instance._FrameProcLock);

                        //InperDeviceHelper.Instance._LoopCannels = new System.Collections.Concurrent.ConcurrentBag<CameraChannel>(InperDeviceHelper.Instance._LoopCannels.Where(x => x.ChannelId != item.ChannelId));
                        Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == ChannelTypeEnum.Camera.ToString());
                        if (channel != null)
                        {
                            _ = InperGlobalClass.CameraSignalSettings.CameraChannels.Remove(channel);
                        }
                        view.channelName.Text = InperDeviceHelper.Instance.CameraChannels.Count(x => x.Type == ChannelTypeEnum.Camera.ToString()) > 0 ? InperDeviceHelper.Instance.CameraChannels.Last(x => x.Type == ChannelTypeEnum.Camera.ToString()).Name : "";
                        if (EllipseBorder.Count > 0)
                        {
                            SetDefaultCircle(EllipseBorder.Last());
                        }

                    }
                    if (EllipseBorder.Count == 0)
                    {
                        view.channelRoi.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private Grid DrawCircle(int index, double diameter, double ytop, double ybottom, Channel _channel = null)
        {
            if (_channel == null)
            {
                _channel = new Channel()
                {
                    Left = 0,
                    Top = 0,
                    Offset = false,
                };
            }
            foreach (var item in view.ellipseCanvas.Children)
            {
                if ((item as FrameworkElement).Name == ("ROI_" + index))
                {
                    return null;
                }
            }
            Grid grid = new Grid() { Cursor = Cursors.Hand, Background = Brushes.Transparent, Name = "ROI_" + index };
            grid.SetValue(Canvas.LeftProperty, _channel.Left);
            grid.SetValue(Canvas.TopProperty, _channel.Top);
            grid.MouseLeftButtonDown += Grid_MouseDown;
            //grid.MouseMove += Grid_MouseMove;
            grid.MouseLeftButtonUp += Grid_MouseUp;
            Ellipse ellipse = new Ellipse()
            {
                Stroke = InperColorHelper.SCBrushes[index % 9],
                StrokeThickness = 1,
                Width = diameter,
                Height = diameter
            };
            TextBlock tb = new TextBlock()
            {
                Text = index.ToString(),
                Foreground = InperColorHelper.SCBrushes[index % 9],
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12
            };
            _ = grid.Children.Add(tb);
            _ = grid.Children.Add(ellipse);
            EllipseBorder.Add(grid);

            view.channelName.Text = "ROI-";

            if (InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == index - 1 && x.Type == ChannelTypeEnum.Camera.ToString()) == null)
            {
                CameraChannel item = new CameraChannel()
                {
                    ChannelId = index - 1,
                    Name = _channel.Name == null ? "ROI-" + index + "-" : _channel.Name,
                    YVisibleRange = new SciChart.Data.Model.DoubleRange(ybottom, ytop),
                    Offset = _channel.Offset,
                    OffsetWindowSize = _channel.OffsetWindowSize,
                    Height = _channel.Height,
                    Type = ChannelTypeEnum.Camera.ToString()
                };
                item.Filters.IsSmooth = _channel.Filters.IsSmooth;
                item.Filters.Smooth = _channel.Filters.Smooth;
                item.Filters.IsHighPass = _channel.Filters.IsHighPass;
                item.Filters.HighPass = _channel.Filters.HighPass;
                item.Filters.IsLowPass = _channel.Filters.IsLowPass;
                item.Filters.LowPass = _channel.Filters.LowPass;
                item.Filters.IsNotch = _channel.Filters.IsNotch;
                item.Filters.Notch = _channel.Filters.Notch;

                item.TimeSpanAxis = new TimeSpanAxis()
                {
                    DrawMajorBands = false,
                    DrawMajorGridLines = false,
                    DrawMinorGridLines = false,
                    VisibleRange = item.XVisibleRange,
                    Visibility = Visibility.Collapsed,
                };
                //item.TimeSpanAxis.VisibleRangeChanged += InperDeviceHelper.Instance.TimeSpanAxis_VisibleRangeChanged;

                foreach (WaveGroup wave in InperDeviceHelper.Instance.LightWaveLength)
                {
                    SetYaxidVisible(item, wave.GroupId, wave.IsChecked);
                    if (wave.IsChecked)
                    {
                        LightMode<TimeSpan, double> mode = new LightMode<TimeSpan, double>()
                        {
                            LightType = wave.GroupId,
                            WaveColor = new SolidColorBrush(InperColorHelper.WavelengthToRGB(int.Parse(wave.WaveType.Split(' ').First()))),
                            //WaveColor = wave.GroupId == 1 ? InperColorHelper.SCBrushes[index % 9] : (wave.GroupId == 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF008000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000FF"))),
                            XyDataSeries = new XyDataSeries<TimeSpan, double>(),
                        };
                        item.LightModes.Add(mode);
                        LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = wave.GroupId, DataSeries = mode.XyDataSeries, Stroke = mode.WaveColor.Color, YAxisId = "Ch" + wave.GroupId };

                        item.RenderableSeries.Add(line);
                    }
                }
                if (InperDeviceHelper.Instance.CameraChannels.Count > 0)
                {
                    item.XVisibleRange = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault().XVisibleRange;
                    item.ViewportManager = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault().ViewportManager;
                }
                else
                {
                    item.XVisibleRange = new TimeSpanRange(new TimeSpan(0), new TimeSpan(0, 0, (int)DataShowControlViewModel.ShowVisibleValue));
                    item.ViewportManager = new ScrollingViewportManager(DataShowControlViewModel.ShowVisibleValue);
                }
                if (!string.IsNullOrEmpty(_channel.LightOffsetValue))
                {
                    var offsetValue = _channel.LightOffsetValue.Split(' ');
                    offsetValue.ForEachDo(off =>
                    {
                        if (!string.IsNullOrEmpty(off))
                        {
                            if (item.LightModes.First(f => f.LightType == int.Parse(off.Split(',').First())) is var ltp)
                            {
                                if (double.TryParse(off.Split(',').Last(), out var d))
                                {
                                    ltp.OffsetValue = d;
                                }
                            }
                        }
                    });
                }
                Monitor.Enter(InperDeviceHelper.Instance._FrameProcLock);
                InperDeviceHelper.Instance.CameraChannels.Add(item);
                Monitor.Exit(InperDeviceHelper.Instance._FrameProcLock);
                //InperDeviceHelper.Instance._LoopCannels.Add(item);
                Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == index - 1 && x.Type == ChannelTypeEnum.Camera.ToString());

                if (InperGlobalClass.IsPreview)
                {
                    MainWindowViewModel main = null;
                    foreach (System.Windows.Window window in Application.Current.Windows)
                    {
                        if (window.Name.Contains("MainWindow"))
                        {
                            main = window.DataContext as MainWindowViewModel;
                            break;
                        }
                    }
                    if (InperDeviceHelper.Instance.CameraChannels.Count == 1 && main != null)
                    {
                        main.DataShowControlViewModel.SciScrollSet();
                    }
                    if (main != null)
                    {
                        main.DataShowControlViewModel.ChartZoomExtentsExport();
                    }
                }
                if (channel == null)
                {
                    InperGlobalClass.CameraSignalSettings.CameraChannels.Add(new Channel()
                    {
                        ChannelId = index - 1,
                        //Name = "ROI-" + index + "-",
                        Name = "ROI-" + index,
                        Left = double.Parse(grid.GetValue(Canvas.LeftProperty).ToString()),
                        Top = double.Parse(grid.GetValue(Canvas.TopProperty).ToString()),
                        ROI = ellipse.Width,
                        YTop = ytop,
                        YBottom = ybottom,
                        Type = ChannelTypeEnum.Camera.ToString()
                    });
                }

                
                SetMat(null, grid);
            }

            SetDefaultCircle(grid);

            return grid;
        }
        private void SetDefaultCircle(Grid grid)
        {
            MouseButtonEventArgs eventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
            eventArgs.RoutedEvent = Grid.MouseLeftButtonDownEvent;
            grid.RaiseEvent(eventArgs);
            isDown = false;
        }
        public void EllipseCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            isDown = false;
        }
        public void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDown = false;

            InperDeviceHelper.CameraChannels.ForEachDo(x =>
            {
                x.Mask.SaveImage(x.ChannelId + ".bmp");
            });
        }
        public void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (isDown)
                {
                    this.view.Dispatcher.Invoke(new Action(() =>
                    {
                        System.Windows.Point movePoint = e.GetPosition(this.view.ellipseCanvas);

                        double currentLeft = (double)moveGrid.GetValue(Canvas.LeftProperty);
                        double currentTop = (double)moveGrid.GetValue(Canvas.TopProperty);

                        if (currentLeft < 0 || currentTop < 0)
                        {
                            return;
                        }
                        double moveLeft = Math.Abs(movePoint.X - startPoint.X - Diameter / 2);
                        double moveTop = Math.Abs(movePoint.Y - startPoint.Y - Diameter / 2);
                        if (moveLeft + Diameter >= this.view.ellipseCanvas.ActualWidth || moveTop + Diameter >= this.view.ellipseCanvas.ActualHeight)
                        {
                            return;
                        }
                        moveGrid.SetValue(Canvas.LeftProperty, moveLeft);
                        moveGrid.SetValue(Canvas.TopProperty, moveTop);

                        TextBlock tb = moveGrid.Children[0] as TextBlock;

                        Lib.Helper.JsonBean.Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse(tb.Text) - 1);
                        if (channel != null)
                        {
                            channel.Left = moveLeft;
                            channel.Top = moveTop;
                        }

                        //Mat mat = InperDeviceHelper.CameraChannels.First(x => x.ChannelId == int.Parse(tb.Text) - 1).Mask;
                        SetMat(null, moveGrid);
                    }));
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (moveGrid != null)
            {
                (moveGrid.Children[1] as Ellipse).StrokeThickness = 1;
            }

            moveGrid = sender as Grid;
            (moveGrid.Children[1] as Ellipse).StrokeThickness = 3;

            view.channelName.Text = InperDeviceHelper.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1).Name;
            view.channelName.SelectionStart = view.channelName.Text.Length;
            view.channelRoi.Text = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1).ROI.ToString();
            view.channelRoi.IsEnabled = true;

            isDown = true;
            startPoint = new System.Windows.Point(0, 0);
        }
        public void ChannelName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.TextBox tb = sender as System.Windows.Controls.TextBox;
                if (tb.IsFocused)
                {
                    if (tb.Name == "channelName")
                    {
                        string verify = string.Empty;

                        verify = moveGrid == null ? "ROI-" + (EllipseBorder.Last().Children[0] as TextBlock).Text + "-" : "ROI-" + (moveGrid.Children[0] as TextBlock).Text + "-";

                        if (tb.Text.Length < 5 || !tb.Text.StartsWith(verify))
                        {
                            tb.Text = verify;
                            tb.SelectionStart = tb.Text.Length;
                            //Growl.Warning(new GrowlInfo() { Message = "Fixed character, cannot be changed", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                        else
                        {
                            if (moveGrid == null)
                            {
                                InperDeviceHelper.CameraChannels.Last().Name = tb.Text;
                                InperGlobalClass.CameraSignalSettings.CameraChannels.Last().Name = tb.Text;
                            }
                            else
                            {
                                InperDeviceHelper.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1).Name = tb.Text;
                                InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1).Name = tb.Text;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void ChannelRoi_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (moveGrid != null)
                {
                    Ellipse ellipse = moveGrid.Children[1] as Ellipse;
                    TextBlock tb = moveGrid.Children[0] as TextBlock;
                    double width = double.Parse((sender as System.Windows.Controls.TextBox).Text);

                    if (width >= Math.Floor(view.ellipseCanvas.Height))
                    {
                        width = Math.Floor(view.image.ActualHeight);
                        moveGrid.SetValue(Canvas.LeftProperty, 10.0);
                        moveGrid.SetValue(Canvas.TopProperty, 0.0);
                        string text = "取值不能超过" + width;
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "The value cannot exceed " + width;
                        }
                        InperGlobalClass.ShowReminderInfo(text);
                    }

                    ellipse.Width = ellipse.Height = width;

                    Channel item = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse(tb.Text) - 1);
                    if (item != null)
                    {
                        item.ROI = double.Parse((sender as System.Windows.Controls.TextBox).Text);
                    }

                    //Mat mat = InperDeviceHelper.CameraChannels.First(x => x.ChannelId == int.Parse(tb.Text) - 1).Mask;
                    SetMat(null, moveGrid);
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #region 灯光和采样率属性配置
        private Task SetExpourseAndGain()
        {
            return Task.Factory.StartNew(async () =>
            {
                _ = InperDeviceHelper.Instance.device.SetGain(12);

                _ = InperDeviceHelper.Instance.device.SetExposure(10);
                await Task.Delay(50);
            });
        }
        private void SetLightStatuAndSampling(WaveGroup sen)
        {
            try
            {
                if (sen.IsChecked)
                {
                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == sen.GroupId);
                    if (wg == null)
                    {
                        //wg = sen;
                        InperGlobalClass.CameraSignalSettings.LightMode.Add(sen);

                        InperGlobalClass.SetSampling(Math.Round(InperGlobalClass.CameraSignalSettings.Sampling * (InperGlobalClass.CameraSignalSettings.LightMode.Count - 1 < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count - 1) / InperGlobalClass.CameraSignalSettings.LightMode.Count, 0));
                        Sampling = InperGlobalClass.CameraSignalSettings.Sampling;
                    }
                    else
                    {
                        wg.IsChecked = true;
                        wg.LightPower = sen.LightPower;
                    }
                }
                foreach (CameraChannel item in InperDeviceHelper.Instance.CameraChannels)
                {
                    if (item.Type == ChannelTypeEnum.Camera.ToString())
                    {
                        var mode = item.LightModes.FirstOrDefault(x => x.LightType == sen.GroupId);
                        if (mode == null)
                        {
                            mode = new LightMode<TimeSpan, double>()
                            {
                                LightType = sen.GroupId,
                                WaveColor = new SolidColorBrush(InperColorHelper.WavelengthToRGB(int.Parse(sen.WaveType.Split(' ').First()))),
                                //WaveColor = sen.GroupId == 1 ? InperColorHelper.SCBrushes[item.ChannelId % 9] : (sen.GroupId == 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF008000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000FF"))),
                                XyDataSeries = new XyDataSeries<TimeSpan, double>(),
                            };
                            item.LightModes.Add(mode);
                            if (InperGlobalClass.IsPreview)
                            {
                                InperDeviceHelper.Instance.device.SwitchLight((uint)sen.GroupId, true);
                                InperDeviceHelper.Instance.device.SetLightPower((uint)sen.GroupId, sen.LightPower);
                                CameraChannel loopChn = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId);
                                LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = mode.LightType, DataSeries = mode.XyDataSeries, Stroke = mode.WaveColor.Color, YAxisId = "Ch" + sen.GroupId };
                                line.DataSeries.FifoCapacity = 10 * 60 * (int)InperGlobalClass.CameraSignalSettings.Sampling;
                                loopChn.RenderableSeries.Add(line);
                            }

                        }
                        LineRenderableSeriesViewModel fast = null;
                        foreach (LineRenderableSeriesViewModel line in item.RenderableSeries)
                        {
                            if (line.Tag.ToString() == sen.GroupId.ToString())
                            {
                                fast = line;
                            }
                        }
                        if (fast == null)
                        {
                            item.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = sen.GroupId.ToString(), DataSeries = mode.XyDataSeries, Stroke = mode.WaveColor.Color, YAxisId = "Ch" + sen.GroupId });
                        }
                        SetYaxidVisible(item, sen.GroupId, true);
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #endregion
        public void LightMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaveGroup sen = (sender as CheckBox).DataContext as WaveGroup;
                sen.IsChecked = true;
                SetLightStatuAndSampling(sen);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Gain_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (double.TryParse((sender as System.Windows.Controls.TextBox).Text, out double gain))
                {
                    if (gain > (sender as InperTextBox).InperMaxValue)
                    {
                        (sender as InperTextBox).Text = (sender as InperTextBox).InperMaxValue.ToString();
                        string text = "最大值是：" + (sender as InperTextBox).InperMaxValue;
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "The maximum value is" + (sender as InperTextBox).InperMaxValue;
                        }
                        InperGlobalClass.ShowReminderInfo(text);
                    }
                    if (gain < (sender as InperTextBox).InperMinValue)
                    {
                        (sender as InperTextBox).Text = (sender as InperTextBox).InperMinValue.ToString();
                        string text = "最小值是：" + (sender as InperTextBox).InperMinValue;
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "The minimum value is" + (sender as InperTextBox).InperMinValue;
                        }
                        InperGlobalClass.ShowReminderInfo(text);
                    }
                    InperGlobalClass.CameraSignalSettings.Gain = gain;
                    _ = InperDeviceHelper.Instance.device.SetGain(gain);
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void CheckBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (InperGlobalClass.CameraSignalSettings.LightMode.Count < 2 && InperGlobalClass.IsPreview && (bool)((CheckBox)sender).IsChecked)
                {
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void LightMode_UnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaveGroup sen = (sender as CheckBox).DataContext as WaveGroup;
                sen.IsChecked = false;

                if (!sen.IsChecked)
                {
                    //InperDeviceHelper.Instance.device.SwitchLight((uint)sen.GroupId, false);

                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == sen.GroupId);
                    if (wg != null)
                    {
                        if (InperGlobalClass.CameraSignalSettings.LightMode.Remove(wg))
                        {
                            InperGlobalClass.SetSampling(Math.Round(InperGlobalClass.CameraSignalSettings.Sampling * (InperGlobalClass.CameraSignalSettings.LightMode.Count + 1) / (InperGlobalClass.CameraSignalSettings.LightMode.Count < 1 ? 1 : InperGlobalClass.CameraSignalSettings.LightMode.Count), 0));
                            Sampling = InperGlobalClass.CameraSignalSettings.Sampling;
                        }
                    }

                }

                foreach (CameraChannel item in InperDeviceHelper.CameraChannels)
                {
                    if (item.Type == ChannelTypeEnum.Camera.ToString())
                    {

                        LightMode<TimeSpan, double> mode = item.LightModes.FirstOrDefault(x => x.LightType == sen.GroupId);
                        if (mode != null)
                        {
                            _ = item.LightModes.Remove(mode);
                            if (InperGlobalClass.IsPreview)
                            {
                                InperDeviceHelper.Instance.device.SwitchLight((uint)sen.GroupId, false);
                                InperDeviceHelper.Instance.device.SetLightPower((uint)sen.GroupId, 0);
                                CameraChannel loopChn = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId);

                                if (loopChn.RenderableSeries.FirstOrDefault(x => (x as LineRenderableSeriesViewModel).Tag.ToString() == mode.LightType.ToString()) is LineRenderableSeriesViewModel line)
                                {
                                    loopChn.RenderableSeries.Remove(line);
                                }
                            }
                        }
                        LineRenderableSeriesViewModel fast = null;
                        foreach (LineRenderableSeriesViewModel line in item.RenderableSeries)
                        {
                            if (line.Tag.ToString() == sen.GroupId.ToString())
                            {
                                fast = line;
                            }
                        }
                        if (fast != null)
                        {
                            _ = item.RenderableSeries.Remove(fast);
                        }
                        SetYaxidVisible(item, sen.GroupId, false);
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private void SetYaxidVisible(CameraChannel cameraChannel, int groupId, bool statu)
        {
            switch (groupId)
            {
                case 0:
                    cameraChannel.S1Visible = statu;
                    break;
                case 1:
                    cameraChannel.S0Visible = statu;
                    break;
                case 2:
                    cameraChannel.S2Visible = statu;
                    break;
                case 3:
                    cameraChannel.S3Visible = statu;
                    break;
            }
            MainWindowViewModel main = null;
            foreach (System.Windows.Window window in Application.Current.Windows)
            {
                if (window.Name.Contains("MainWindow"))
                {
                    main = window.DataContext as MainWindowViewModel;
                    break;
                }
            }
            (main.ActiveItem as DataShowControlViewModel).AllYaxisSetMerge();
            (main.ActiveItem as DataShowControlViewModel).AllYaxisSetSeparate();
        }
        public void LightTestMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton tb = sender as ToggleButton;
                //if (tb.IsFocused)
                {
                    if ((bool)tb.IsChecked)
                    {
                        InperGlobalClass.IsOpenLightMeasureMode = true;
                        InperDeviceHelper.device.SetMeasureMode(true);
                        if ((bool)this.view.interval.IsChecked)
                        {
                            InperDeviceHelper.Instance._Metronome.Stop();
                        }
                        //this.view.waveView.IsEnabled = true;
                        InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                        {
                            //if (x.IsChecked)
                            {
                                InperDeviceHelper.Instance.device.SwitchLight((uint)x.GroupId, false);
                                InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, 0);
                                //Thread.Sleep(50);
                            }
                        });
                        InperDeviceHelper.Instance.device.Start();
                        _ = InperDeviceHelper.Instance.device.SetExposure(20);
                        InperDeviceHelper.Instance.device.SetFrameRate(50);

                        #region 默认选中第一项
                        var item = this.view.lightMode.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                        if (item != null)
                        {
                            RadioButton rb = InperClassHelper.FindVisualChild<RadioButton>(item);
                            rb.IsChecked = true;
                            InperTextBox textBox = InperClassHelper.FindVisualChild<InperTextBox>(item);
                            if (string.IsNullOrEmpty(textBox.Text))
                            {
                                textBox.Text = "40";
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        InperGlobalClass.IsOpenLightMeasureMode = false;
                        InperDeviceHelper.device.SetMeasureMode(false);
                        if ((bool)this.view.interval.IsChecked)
                        {
                            InperDeviceHelper.Instance._Metronome.Start();
                        }
                        InperDeviceHelper.Instance.device.Stop();
                        this.view.waveView.IsEnabled = false;
                        InperDeviceHelper.Instance.SelectedWaveType = -1;
                        _ = InperDeviceHelper.Instance.device.SetExposure(InperGlobalClass.CameraSignalSettings.Exposure);
                        InperDeviceHelper.Instance.device.SetFrameRate(InperGlobalClass.CameraSignalSettings.Sampling);
                        InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                        {
                            InperDeviceHelper.Instance.device.SwitchLight((uint)x.GroupId, false);
                            InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, 0);
                            //Thread.Sleep(50);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void TestMode_rb_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaveGroup sen = (sender as RadioButton).DataContext as WaveGroup;
                InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                {
                    if (x.GroupId == sen.GroupId)
                    {
                        //x.IsChecked = true;
                        InperDeviceHelper.Instance.device.SwitchLight((uint)x.GroupId, true);
                        InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, x.LightPower);

                        this.view.waveView.SelectedItem = x;
                    }
                    else
                    {
                        //x.IsChecked = false;
                        InperDeviceHelper.Instance.device.SwitchLight((uint)x.GroupId, false);
                        InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, 0);
                    }
                });

            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void TestMode_rb_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if ((bool)this.view.lightTestMode.IsChecked)
                {
                    if ((sender as RadioButton).DataContext is WaveGroup sen)
                    {
                        InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                        {
                            if (x.GroupId == sen.GroupId && (bool)(sender as RadioButton).IsChecked)
                            {
                                InperDeviceHelper.Instance.device.SwitchLight((uint)x.GroupId, true);
                                InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, x.LightPower);

                                InperDeviceHelper.Instance.SelectedWaveType = x.GroupId;
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void LightMode_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                WaveGroup sen = (sender as InperTextBox).DataContext as WaveGroup;
                if (sen.LightPower > (sender as InperTextBox).InperMaxValue)
                {
                    string text = "最大值是：" + (sender as InperTextBox).InperMaxValue;
                    if (InperConfig.Instance.Language == "en_us")
                    {
                        text = "The maximum value is" + (sender as InperTextBox).InperMaxValue;
                    }
                    InperGlobalClass.ShowReminderInfo(text);
                    (sender as InperTextBox).Text = (sender as InperTextBox).InperMaxValue.ToString();
                }
                if (sen.LightPower < (sender as InperTextBox).InperMinValue)
                {
                    string text = "最小值是：" + (sender as InperTextBox).InperMinValue;
                    if (InperConfig.Instance.Language == "en_us")
                    {
                        text = "The minimum value is" + (sender as InperTextBox).InperMinValue;
                    }
                    InperGlobalClass.ShowReminderInfo(text);
                    (sender as InperTextBox).Text = (sender as InperTextBox).InperMinValue.ToString();
                }
                //if (sen.IsChecked)
                {
                    InperDeviceHelper.Instance.device.SetLightPower((uint)sen.GroupId, sen.LightPower);
                    if (InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == sen.GroupId) != null)
                    {
                        InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == sen.GroupId).LightPower = sen.LightPower;
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private void SetMat(Mat _mat, Grid grid)
        {
            Mat mat = Mat.Zeros(new OpenCvSharp.Size(InperDeviceHelper.Instance.VisionWidth, InperDeviceHelper.Instance.VisionHeight), MatType.CV_8U);
            double scale = InperDeviceHelper.Instance.VisionWidth / (this.view.ellipseCanvas.ActualWidth == 0 ? this.view.ellipseCanvas.Width : this.view.ellipseCanvas.ActualWidth);
            double rect_left = (double)grid.GetValue(Canvas.LeftProperty) * scale;
            double rect_top = (double)grid.GetValue(Canvas.TopProperty) * scale;

            double ellips_diam = (grid.Children[1] as Ellipse).Width * scale;

            OpenCvSharp.Point Center = new OpenCvSharp.Point(rect_left + (ellips_diam / 2), rect_top + (ellips_diam / 2));

            mat.Circle(center: Center, radius: (int)(ellips_diam / 4), color: Scalar.White, -1);

            InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((grid.Children[0] as TextBlock).Text) - 1).Mask = mat;
        }
        public void Interval_Checked(object sender, RoutedEventArgs e)
        {
            InperDeviceHelper.Instance._Metronome.Start();
            InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous = false;
            InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval = true;
        }
        public void Continus_Checked(object sender, RoutedEventArgs e)
        {
            InperDeviceHelper.Instance._Metronome.Stop();
            InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
            {
                if (x.IsChecked)
                {
                    InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, x.LightPower);
                    //Thread.Sleep(50);
                }
            });
            InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous = true;
            InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval = false;
        }
        public void Screenshots()
        {
            try
            {
                view.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //InperComputerInfoHelper.SaveFrameworkElementToImage(this.view.ellipseCanvas, DateTime.Now.ToString("HHmmss") + "CameraScreen.bmp", System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                    //获取控件相对于窗口位置
                    GeneralTransform generalTransform = this.view.image.TransformToAncestor(this.view.signal);
                    System.Windows.Point point = generalTransform.Transform(new System.Windows.Point(0, 0));

                    //获取窗口相对于屏幕的位置
                    System.Windows.Point ptLeftUp = new System.Windows.Point(0, 0);
                    ptLeftUp = this.view.PointToScreen(ptLeftUp);

                    //计算DPI缩放
                    var ct = PresentationSource.FromVisual(view.image)?.CompositionTarget;
                    var matrix = ct == null ? Matrix.Identity : ct.TransformToDevice;

                    double x = matrix.M11 == 0 ? 1 : matrix.M11;
                    double y = matrix.M22 == 0 ? 1 : matrix.M22;

                    int left = (int)ptLeftUp.X + (int)(point.X * x);
                    int top = (int)ptLeftUp.Y + (int)(point.Y * y);

                    InperComputerInfoHelper.SaveScreenToImageByPoint(left, top, (int)(Math.Ceiling(view.image.ActualWidth) * x), (int)(Math.Ceiling(view.image.ActualHeight) * y), System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmssffff") + "CameraScreen.bmp"));
                    string text = "保存成功";
                    if (InperConfig.Instance.Language == "en_us")
                    {
                        text = "Saved successfully.";
                    }
                    Growl.Info(new GrowlInfo() { Message = text, Token = "SuccessMsg", WaitTime = 1 });
                }));
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #endregion

        #region methods Analog
        public void AInperTextBox_InperTextChanged(object arg1, TextChangedEventArgs arg2)
        {
            try
            {
                InperTextBox textBox = arg1 as InperTextBox;
                var adFrame = uint.Parse(textBox.Text);
                InperDeviceHelper.Instance.AiConfigSend();
                InperDeviceHelper.Instance.adFsTimeInterval = 1d / adFrame;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void PopButton_Click(object sender, RoutedEventArgs e)
        {
            this.view.pop.IsOpen = true;
        }
        public void AnalogName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.TextBox tb = sender as System.Windows.Controls.TextBox;
                if (tb.IsFocused)
                {
                    SignalCameraChannel obj = view.AnalogChannelCombox.SelectedItem as SignalCameraChannel;
                    if (tb.Text.Length < 4 || !tb.Text.StartsWith("AI-" + (obj.ChannelId - 100) + "-"))
                    {
                        string name = "AI-" + (obj.ChannelId - 100) + "-";
                        //tb.SelectionStart = name.Length - 1;
                        tb.Text = name;
                        //Growl.Error(new GrowlInfo() { Message = "Fixed character, cannot be changed", Token = "SuccessMsg", WaitTime = 1 });
                        //return;
                    }
                    if (tb.Text.Length > 15)
                    {
                        //tb.SelectionStart = name.Length - 1;
                        tb.Text = tb.Text.Substring(0, 15);
                        //return;
                    }
                    AnalogChannels[view.AnalogChannelCombox.SelectedIndex].Name = tb.Text;
                    AnalogChannels[view.AnalogChannelCombox.SelectedIndex].NickName = tb.Text;
                    tb.SelectionStart = tb.Text.Length;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void AnalogChannelCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                object cb = (sender as System.Windows.Controls.ComboBox).SelectedItem;
                if (cb != null)
                {
                    var item = cb as SignalChannel;
                    view.PopButton.Background = item.BgColor;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void AnalogColorList_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString((sender as ListBox).SelectedValue.ToString()));
                AnalogChannels[view.AnalogChannelCombox.SelectedIndex].BgColor = brush;
                this.view.PopButton.Background = brush;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void AnalogMover(string moveType)
        {
            try
            {
                SignalCameraChannel ch = this.view.AnalogChannelCombox.SelectedItem as SignalCameraChannel;
                var ch_active = this.view.analogActiveChannel.SelectedItem as SignalCameraChannel;
                if (moveType == "leftMove")//右移是激活 左移是取消激活
                {
                    if (AnalogActiveChannels.Count > 0 && ch_active != null)
                    {
                        _ = AnalogActiveChannels.Remove(ch_active);
                        ch_active.NickName = "AI-" + (ch_active.ChannelId - 100);
                        ch_active.Name = "AI-" + (ch_active.ChannelId - 100) + "-";
                        AnalogChannels.Add(ch_active);
                        if (AnalogChannels.Count <= 1)
                        {
                            view.AnalogChannelCombox.SelectedIndex = 0;
                        }
                        CameraChannel cameraChannel = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && x.Type == ChannelTypeEnum.Analog.ToString());
                        if (cameraChannel != null)
                        {
                            Monitor.Enter(InperDeviceHelper.Instance._FrameProcLock);
                            _ = InperDeviceHelper.Instance.CameraChannels.Remove(cameraChannel);
                            Monitor.Exit(InperDeviceHelper.Instance._FrameProcLock);
                            InperDeviceHelper.Instance.aiChannels = new System.Collections.Concurrent.ConcurrentBag<CameraChannel>(InperDeviceHelper.Instance.aiChannels.Where(x => x.ChannelId != cameraChannel.ChannelId));
                            InperDeviceHelper.Instance.AiSettingFunc(cameraChannel.ChannelId, 0);
                            InperDeviceHelper.Instance._adPreTime.Remove(cameraChannel.ChannelId);
                        }
                        Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && x.Type == ChannelTypeEnum.Analog.ToString());
                        if (channel != null)
                        {
                            _ = InperGlobalClass.CameraSignalSettings.CameraChannels.Remove(channel);
                        }
                    }
                }
                else
                {
                    if (AnalogChannels.Count > 0 && ch != null)
                    {
                        _ = AnalogChannels.Remove(ch);
                        if (ch.Name.EndsWith("-"))
                        {
                            ch.Name = ch.Name.Substring(0, ch.Name.Length - 1);
                        }
                        AnalogActiveChannels.Add(ch);

                        CameraChannel item = new CameraChannel()
                        {
                            ChannelId = ch.ChannelId,
                            Name = ch.Name,
                            YVisibleRange = new SciChart.Data.Model.DoubleRange(-10, 10),
                            Type = ChannelTypeEnum.Analog.ToString()
                        };

                        item.TimeSpanAxis = new TimeSpanAxis()
                        {
                            DrawMajorBands = false,
                            DrawMajorGridLines = false,
                            DrawMinorGridLines = false,
                            VisibleRange = item.XVisibleRange,
                            Visibility = Visibility.Collapsed,
                        };
                        item.Filters = new Lib.Bean.Channel.Filters();
                        item.LightModes.Add(new LightMode<TimeSpan, double>()
                        {
                            LightType = -1
                        });
                        //item.TimeSpanAxis.VisibleRangeChanged += InperDeviceHelper.Instance.TimeSpanAxis_VisibleRangeChanged;

                        LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = "-1", DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = ch.BgColor.Color, YAxisId = "Ch0" };
                        item.RenderableSeries.Add(line);
                        Monitor.Enter(InperDeviceHelper.Instance._FrameProcLock);
                        InperDeviceHelper.Instance.CameraChannels.Add(item);
                        Monitor.Exit(InperDeviceHelper.Instance._FrameProcLock);
                        InperDeviceHelper.Instance.aiChannels.Add(item);
                        if (!InperDeviceHelper.Instance._adPreTime.ContainsKey(item.ChannelId))
                        {
                            InperDeviceHelper.Instance._adPreTime.Add(item.ChannelId, InperDeviceHelper.Instance._adPreTime.Count == 0 ? 0 : InperDeviceHelper.Instance._adPreTime.First().Value);
                        }
                        InperDeviceHelper.Instance.AiSettingFunc(item.ChannelId, 1);
                        Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == ChannelTypeEnum.Analog.ToString());
                        if (channel == null)
                        {
                            InperGlobalClass.CameraSignalSettings.CameraChannels.Add(new Channel()
                            {
                                ChannelId = item.ChannelId,
                                Name = item.Name,
                                YTop = 10,
                                YBottom = 0,
                                Type = ChannelTypeEnum.Analog.ToString(),
                                Color = ch.BgColor.Color.ToString()
                            });
                        }
                        view.PopButton.Background = AnalogChannels.First().BgColor;
                        view.AnalogChannelCombox.SelectedIndex = 0;
                    }
                }
                if (InperGlobalClass.IsPreview || InperGlobalClass.IsRecord)
                {
                    InperDeviceHelper.Instance.AiConfigSend();
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #endregion
        protected override void OnDeactivate()
        {
            try
            {
                this.view.lightTestMode.IsChecked = false;
                InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous = IsContinuous;
                InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval = IsInterval;

                InperGlobalClass.CameraSignalSettings.CameraChannels.ForEach(x =>
                {
                    if (x.Name.EndsWith("-"))
                    {
                        x.Name = x.Name.Substring(0, x.Name.Length - 1);
                    }
                });

                InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(x =>
                {
                    if (x.Name.EndsWith("-"))
                    {
                        x.Name = x.Name.Substring(0, x.Name.Length - 1);
                    }
                });

                InperJsonHelper.SetCameraSignalSettings(InperGlobalClass.CameraSignalSettings);
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
            //finally
            //{
            //    this.RequestClose();
            //}
        }
    }
}
