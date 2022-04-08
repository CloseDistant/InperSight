using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudioControlLib.Control.TextBox;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TextBox = HandyControl.Controls.TextBox;

namespace InperStudio.ViewModels
{
    public class SignalSettingsViewModel : Screen
    {
        #region properties
        private readonly SignalSettingsTypeEnum @enum;
        private SignalSettingsView view;

        //ellipse
        public List<Grid> EllipseBorder = new List<Grid>();
        private System.Windows.Point startPoint;
        private bool isDown = false;
        private int diameter = 55;
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
        public List<double> FPS { get; set; } = InperParameters.FPS;
        public bool IsContinuous { get; set; } = InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous;
        public bool IsInterval { get; set; } = InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval;
        private string sampling = InperGlobalClass.CameraSignalSettings.Sampling.ToString();
        public string Sampling { get => sampling; set => SetAndNotify(ref sampling, value); }
        private string expourse = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
        public string Expourse { get => expourse; set => SetAndNotify(ref expourse, value); }
        #endregion
        public SignalSettingsViewModel(SignalSettingsTypeEnum typeEnum)
        {
            @enum = typeEnum;
            if (@enum == SignalSettingsTypeEnum.Camera)
            {
                InperDeviceHelper = InperDeviceHelper.Instance;
            }
        }
        protected override void OnViewLoaded()
        {
            view = this.View as SignalSettingsView;

            for (int i = 0; i < 8; i++)
            {
                AnalogChannels.Add(new SignalCameraChannel() { ChannelId = i + 101, Name = "AI-" + (i + 1), BgColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[i])) });
            }
            switch (@enum)
            {
                case SignalSettingsTypeEnum.Camera:
                    Init();
                    break;
                case SignalSettingsTypeEnum.Analog:
                    view.Title = "Analog Signal Settings";
                    view.analog.Visibility = Visibility.Visible;
                    InperGlobalClass.CameraSignalSettings.CameraChannels?.ForEach(x =>
                    {
                        if (x.Type == ChannelTypeEnum.Analog.ToString())
                        {
                            _ = AnalogChannels.Remove(AnalogChannels.FirstOrDefault(c => c.ChannelId == x.ChannelId));
                            var an = new SignalCameraChannel()
                            {
                                ChannelId = x.ChannelId,
                                ChannelType = x.Type,
                                Name = x.Name,
                                YaxisDoubleRange = new SciChart.Data.Model.DoubleRange(x.YBottom, x.YTop),
                                BgColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(x.Color))
                            };
                            AnalogActiveChannels.Add(an);
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

                                item.Filters = new Lib.Bean.Channel.Filters()
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
                                    LightType = -1
                                });
                                item.TimeSpanAxis.VisibleRangeChanged += InperDeviceHelper.Instance.TimeSpanAxis_VisibleRangeChanged;

                                LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = "-1", DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = (Color?)ColorConverter.ConvertFromString(x.Color) };
                                item.RenderableSeries.Add(line);

                                InperDeviceHelper.Instance.CameraChannels.Add(item);
                            }
                        }
                    });
                    if (AnalogChannels.Count > 0)
                    {
                        view.PopButton.Background = AnalogChannels?.FirstOrDefault().BgColor;
                        view.AnalogChannelCombox.SelectedIndex = 0;
                    }
                    break;
                default:
                    break;
            }

            view.ConfirmClickEvent += (s, e) =>
            {
                RequestClose();
            };
        }

        #region methods  Camera
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
                App.Log.Error(ex.ToString());
            }
        }
        public void Exposure_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.ComboBox com = sender as System.Windows.Controls.ComboBox;
                if (double.Parse(com.Text) < 0)
                {
                    Growl.Warning("不能小于0", "SuccessMsg");
                    com.Text = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
                    return;
                }
                if (double.Parse(com.Text) > 100)
                {
                    Growl.Warning("不能大于100", "SuccessMsg");
                    com.Text = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
                    return;
                }
                _ = InperDeviceHelper.Instance.device.SetExposure(double.Parse(expourse));
                InperGlobalClass.CameraSignalSettings.Exposure = double.Parse(expourse);
                if (InperGlobalClass.CameraSignalSettings.Exposure * InperGlobalClass.CameraSignalSettings.Sampling > 1000)
                {
                    InperGlobalClass.CameraSignalSettings.Sampling = Math.Floor(1000 / InperGlobalClass.CameraSignalSettings.Exposure);
                    this.view.Sampling.Text = sampling = InperGlobalClass.CameraSignalSettings.Sampling.ToString();
                    InperDeviceHelper.Instance.device.SetFrameRate(double.Parse(sampling));
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Sampling_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.ComboBox com = sender as System.Windows.Controls.ComboBox;
                if (double.Parse(com.Text) < 1)
                {
                    Growl.Warning("不能小于1", "SuccessMsg");
                    com.Text = InperGlobalClass.CameraSignalSettings.Sampling.ToString();
                    return;
                }
                if (double.Parse(com.Text) > 320)
                {
                    Growl.Warning("不能大于320", "SuccessMsg");
                    com.Text = InperGlobalClass.CameraSignalSettings.Sampling.ToString();
                    return;
                }
                InperDeviceHelper.Instance.device.SetFrameRate(double.Parse(sampling));
                InperGlobalClass.CameraSignalSettings.Sampling = double.Parse(sampling);
                if (InperGlobalClass.CameraSignalSettings.Exposure * InperGlobalClass.CameraSignalSettings.Sampling > 1000)
                {
                    InperGlobalClass.CameraSignalSettings.Exposure = Math.Floor(1000 / InperGlobalClass.CameraSignalSettings.Sampling);
                    this.view.Exposure.Text = expourse = InperGlobalClass.CameraSignalSettings.Exposure.ToString();
                    _ = InperDeviceHelper.Instance.device.SetExposure(double.Parse(expourse));
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void Init()
        {
            try
            {
                view.camera.Visibility = Visibility.Visible;
                view.channelRoi.IsEnabled = false;
                view.waveView.IsEnabled = false;
                InperDeviceHelper.Instance.SelectedWaveType = -1;
                if (InperDeviceHelper.Instance.LightWaveLength.Count > 0)
                {
                    view.waveView.SelectedItem = InperDeviceHelper.Instance.LightWaveLength.Count > 1
                        ? InperDeviceHelper.Instance.LightWaveLength[1]
                        : InperDeviceHelper.Instance.LightWaveLength[0];

                    view.lightMode.ItemsSource = InperDeviceHelper.Instance.LightWaveLength;
                }
                InperGlobalClass.CameraSignalSettings.CameraChannels?.ForEach(x =>
                {
                    if (x.Type == ChannelTypeEnum.Camera.ToString())
                    {
                        Grid grid = DrawCircle(x.ChannelId + 1, x.ROI, x.YTop, x.YBottom, x);
                        _ = view.ellipseCanvas.Children.Add(grid);
                    }
                });
                _ = InperDeviceHelper.Instance.device.SetGain(InperGlobalClass.CameraSignalSettings.Gain);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void AddCircle(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EllipseBorder.Count < 9)
                {
                    int index = EllipseBorder.Count == 0 ? 1 : int.Parse((EllipseBorder.Last().Children[0] as TextBlock).Text) + 1;
                    Grid grid = DrawCircle(index, Diameter, 10, 0);
                    if (EllipseBorder.Count > 3 && EllipseBorder.Count <= 6)
                    {
                        grid.SetValue(Canvas.LeftProperty, 55.0);
                        grid.SetValue(Canvas.TopProperty, (double)(EllipseBorder.Count - 3) * Diameter - 40);
                    }
                    else if (EllipseBorder.Count > 6)
                    {
                        grid.SetValue(Canvas.LeftProperty, 105.0);
                        grid.SetValue(Canvas.TopProperty, (double)(EllipseBorder.Count - 6) * Diameter - 40);
                    }
                    else
                    {
                        grid.SetValue(Canvas.LeftProperty, 10.0);
                        grid.SetValue(Canvas.TopProperty, (double)EllipseBorder.Count * Diameter - 40);
                    }
                    this.view.ellipseCanvas.Children.Add(grid);
                }
                else
                {
                    Growl.Info(new GrowlInfo() { Message = "No more than 9 channels", Token = "SuccessMsg", WaitTime = 1 });
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void ReduceCircle(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EllipseBorder.Count > 0)
                {
                    if (moveGrid != null)
                    {
                        view.ellipseCanvas.Children.Remove(moveGrid);
                        _ = EllipseBorder.Remove(moveGrid);

                        CameraChannel item = InperDeviceHelper.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1 && x.Type == ChannelTypeEnum.Camera.ToString());
                        _ = InperDeviceHelper.CameraChannels.Remove(item);
                        _ = InperDeviceHelper._SignalQs.TryRemove(int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1);

                        Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == ChannelTypeEnum.Camera.ToString());
                        if (channel != null)
                        {
                            _ = InperGlobalClass.CameraSignalSettings.CameraChannels.Remove(channel);
                        }
                        view.channelName.Text = InperDeviceHelper.CameraChannels.Count > 0 ? InperDeviceHelper.CameraChannels.Last().Name : "";

                        SetDefaultCircle(EllipseBorder.Last());
                    }
                    if (EllipseBorder.Count == 0)
                    {
                        view.channelRoi.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
                    Name = "ROI-" + index + "-",
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
                    Visibility = Visibility.Collapsed
                };
                item.TimeSpanAxis.VisibleRangeChanged += InperDeviceHelper.Instance.TimeSpanAxis_VisibleRangeChanged;

                InperDeviceHelper.Instance._SignalQs.TryAdd(index - 1, new SignalData());

                foreach (WaveGroup wave in InperDeviceHelper.Instance.LightWaveLength)
                {
                    if (wave.IsChecked)
                    {
                        LightMode<TimeSpan, double> mode = new LightMode<TimeSpan, double>()
                        {
                            LightType = wave.GroupId,
                            WaveColor = new SolidColorBrush(InperColorHelper.WavelengthToRGB(int.Parse(wave.WaveType.Split(' ').First()))),
                            //WaveColor = wave.GroupId == 1 ? InperColorHelper.SCBrushes[index % 9] : (wave.GroupId == 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF008000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000FF"))),
                            XyDataSeries = new XyDataSeries<TimeSpan, double>()
                        };
                        item.LightModes.Add(mode);
                        LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = wave.GroupId.ToString(), DataSeries = mode.XyDataSeries, Stroke = mode.WaveColor.Color };

                        item.RenderableSeries.Add(line);
                        InperDeviceHelper.Instance._SignalQs[index - 1].ValuePairs.Add(mode.LightType, new Queue<KeyValuePair<long, double>>());
                    }
                }

                InperDeviceHelper.Instance.CameraChannels.Add(item);

                Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == index - 1 && x.Type == ChannelTypeEnum.Camera.ToString());

                if (channel == null)
                {
                    InperGlobalClass.CameraSignalSettings.CameraChannels.Add(new Channel()
                    {
                        ChannelId = index - 1,
                        Name = "ROI-" + index + "-",
                        Left = double.Parse(grid.GetValue(Canvas.LeftProperty).ToString()),
                        Top = double.Parse(grid.GetValue(Canvas.TopProperty).ToString()),
                        ROI = ellipse.Width,
                        YTop = ytop,
                        YBottom = ybottom,
                        Type = ChannelTypeEnum.Camera.ToString()
                    });
                }

                Mat m = Mat.Zeros(new OpenCvSharp.Size(InperDeviceHelper.Instance.VisionWidth, InperDeviceHelper.Instance.VisionHeight), MatType.CV_8U);
                SetMat(m, grid);
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

                        Mat mat = InperDeviceHelper.CameraChannels.First(x => x.ChannelId == int.Parse(tb.Text) - 1).Mask;
                        SetMat(mat, moveGrid);
                    }));
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
                TextBox tb = sender as TextBox;
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
                            Growl.Warning(new GrowlInfo() { Message = "Fixed character, cannot be changed", Token = "SuccessMsg", WaitTime = 1 });
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
                App.Log.Error(ex.ToString());
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
                    ellipse.Width = ellipse.Height = double.Parse((sender as TextBox).Text);

                    Lib.Helper.JsonBean.Channel item = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse(tb.Text) - 1);
                    if (item != null)
                    {
                        item.ROI = double.Parse((sender as TextBox).Text);
                    }

                    Mat mat = InperDeviceHelper.CameraChannels.First(x => x.ChannelId == int.Parse(tb.Text) - 1).Mask;
                    SetMat(mat, moveGrid);
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void LightMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaveGroup sen = (sender as CheckBox).DataContext as WaveGroup;

                if (sen.IsChecked)
                {
                    //InperDeviceHelper.Instance.device.SwitchLight((uint)sen.GroupId, true);
                    //InperDeviceHelper.Instance.device.SetLightPower((uint)sen.GroupId, sen.LightPower);

                    this.view.waveView.SelectedItem = sen;

                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == sen.GroupId);
                    if (wg == null)
                    {
                        wg = sen;
                        InperGlobalClass.CameraSignalSettings.LightMode.Add(sen);
                    }
                }

                foreach (CameraChannel item in InperDeviceHelper.CameraChannels)
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
                            item.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = sen.GroupId.ToString(), DataSeries = mode.XyDataSeries, Stroke = mode.WaveColor.Color });
                        }
                        Monitor.Enter(InperDeviceHelper._SignalQsLocker);
                        InperDeviceHelper._SignalQs[item.ChannelId].ValuePairs[sen.GroupId] = new Queue<KeyValuePair<long, double>>();
                        Monitor.Exit(InperDeviceHelper._SignalQsLocker);
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Gain_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double gain = double.Parse((sender as System.Windows.Controls.TextBox).Text);
                _ = InperDeviceHelper.Instance.device.SetGain(gain);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void LightMode_UnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaveGroup sen = (sender as CheckBox).DataContext as WaveGroup;

                if (!sen.IsChecked)
                {
                    //InperDeviceHelper.Instance.device.SwitchLight((uint)sen.GroupId, false);

                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == sen.GroupId);
                    if (wg != null)
                    {
                        _ = InperGlobalClass.CameraSignalSettings.LightMode.Remove(wg);
                    }

                }

                foreach (CameraChannel item in InperDeviceHelper.CameraChannels)
                {
                    LightMode<TimeSpan, double> mode = item.LightModes.FirstOrDefault(x => x.LightType == sen.GroupId);
                    if (mode != null)
                    {
                        _ = item.LightModes.Remove(mode);
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
                    Monitor.Enter(InperDeviceHelper._SignalQsLocker);

                    _ = InperDeviceHelper._SignalQs[item.ChannelId].ValuePairs.Remove(sen.GroupId);

                    Monitor.Exit(InperDeviceHelper._SignalQsLocker);
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void LightTestMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton tb = sender as ToggleButton;
                if ((bool)tb.IsChecked)
                {
                    if ((bool)this.view.interval.IsChecked)
                    {
                        InperDeviceHelper.Instance._Metronome.Stop();
                    }
                    this.view.waveView.IsEnabled = true;
                    InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                    {
                        //if (x.IsChecked)
                        {
                            InperDeviceHelper.Instance.device.SwitchLight((uint)x.GroupId, false);
                            InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, 0);
                            Thread.Sleep(50);
                        }
                    });
                    InperDeviceHelper.Instance.device.Start();
                    _ = InperDeviceHelper.Instance.device.SetExposure(20);
                    InperDeviceHelper.Instance.device.SetFrameRate(50);
                }
                else
                {
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
                        Thread.Sleep(50);
                    });
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
                App.Log.Error(ex.ToString());
            }
        }
        public void TestMode_rb_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if ((bool)this.view.lightTestMode.IsChecked)
                {
                    WaveGroup sen = (sender as RadioButton).DataContext as WaveGroup;
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
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void LightMode_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                WaveGroup sen = (sender as InperTextBox).DataContext as WaveGroup;
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
                App.Log.Error(ex.ToString());
            }
        }
        private void SetMat(Mat mat, Grid grid)
        {
            double scale = InperDeviceHelper.Instance.VisionWidth / (this.view.ellipseCanvas.ActualWidth == 0 ? this.view.ellipseCanvas.Width : this.view.ellipseCanvas.ActualWidth);
            double rect_left = (double)grid.GetValue(Canvas.LeftProperty) * scale;
            double rect_top = (double)grid.GetValue(Canvas.TopProperty) * scale;

            double ellips_diam = (grid.Children[1] as Ellipse).Width * scale;

            OpenCvSharp.Point Center = new OpenCvSharp.Point(rect_left + (ellips_diam / 2), rect_top + (ellips_diam / 2));

            mat.Circle(center: Center, radius: (int)(ellips_diam / 4), color: Scalar.White, thickness: (int)(ellips_diam / 2));

            InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((grid.Children[0] as TextBlock).Text) - 1).Mask = mat;
        }
        public void Interval_Checked(object sender, RoutedEventArgs e)
        {
            InperDeviceHelper.Instance._Metronome.Start();
        }
        public void Continus_Checked(object sender, RoutedEventArgs e)
        {
            InperDeviceHelper.Instance._Metronome.Stop();
            InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
            {
                if (x.IsChecked)
                {
                    InperDeviceHelper.Instance.device.SetLightPower((uint)x.GroupId, x.LightPower);
                    Thread.Sleep(50);
                }
            });
        }
        public void WaveView_Selected(object sender, RoutedEventArgs e)
        {
            if ((bool)view.lightTestMode.IsChecked)
            {
                InperDeviceHelper.Instance.SelectedWaveType = ((sender as System.Windows.Controls.ComboBox).SelectedItem as WaveGroup).GroupId;
            }
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

                     InperComputerInfoHelper.SaveScreenToImageByPoint(left, top, (int)(Math.Ceiling(view.image.ActualWidth) * x), (int)(Math.Ceiling(view.image.ActualHeight) * y), System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss") + "CameraScreen.bmp"));
                     Growl.Info(new GrowlInfo() { Message = "Saved successfully.", Token = "SuccessMsg", WaitTime = 1 });
                 }));
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion

        #region methods Analog
        public void PopButton_Click(object sender, RoutedEventArgs e)
        {
            this.view.pop.IsOpen = true;
        }
        public void AnalogName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox tb = sender as TextBox;
                if (tb.IsFocused)
                {
                    if (tb.Text.Length < 4 || !tb.Text.StartsWith("AI-" + (view.AnalogChannelCombox.SelectedIndex + 1)))
                    {
                        tb.Text = "AI-" + (this.view.AnalogChannelCombox.SelectedIndex + 1);
                        tb.SelectionStart = tb.Text.Length;
                        Growl.Error(new GrowlInfo() { Message = "Fixed character, cannot be changed", Token = "SuccessMsg", WaitTime = 1 });
                        return;
                    }
                    AnalogChannels[view.AnalogChannelCombox.SelectedIndex].Name = tb.Text;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
                App.Log.Error(ex.ToString());
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
                App.Log.Error(ex.ToString());
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
                        AnalogChannels.Add(ch_active);
                        if (AnalogChannels.Count <= 1)
                        {
                            view.AnalogChannelCombox.SelectedIndex = 0;
                        }
                        CameraChannel cameraChannel = InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && x.Type == ChannelTypeEnum.Analog.ToString());
                        if (cameraChannel != null)
                        {
                            _ = InperDeviceHelper.Instance.CameraChannels.Remove(cameraChannel);
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
                        item.TimeSpanAxis.VisibleRangeChanged += InperDeviceHelper.Instance.TimeSpanAxis_VisibleRangeChanged;

                        LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = "-1", DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = ch.BgColor.Color };
                        item.RenderableSeries.Add(line);

                        InperDeviceHelper.Instance.CameraChannels.Add(item);

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
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion

        protected override void OnClose()
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
                App.Log.Error(ex.ToString());
            }
            GC.Collect(0);
        }
    }
}
