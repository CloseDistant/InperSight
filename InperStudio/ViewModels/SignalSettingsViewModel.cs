using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.DeviceAgency;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
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
        private System.Timers.Timer _Metronome = new System.Timers.Timer();
        private bool IsInWorkingPeriod = true;
        //ellipse
        public List<Grid> EllipseBorder = new List<Grid>();
        private System.Windows.Point startPoint;
        private bool isDown = false;
        private int diameter = 75;
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
        public List<double> Exposures { get; set; } = InperParameters.Exposures;
        public List<double> FPS { get; set; } = InperParameters.FPS;
        public bool IsContinuous { get; set; } = InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous;
        public bool IsInterval { get; set; } = InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval;
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

            switch (@enum)
            {
                case SignalSettingsTypeEnum.Camera:
                    Init();
                    break;
                case SignalSettingsTypeEnum.Analog:
                    this.view.Title = "Analog Signal Settings";
                    this.view.analog.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

            _Metronome.Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000;
            _Metronome.Elapsed += (s, e) =>
            {
                double next_Interval;
                if (IsInWorkingPeriod)
                {
                    InperDeviceHelper.LightWaveLength.ToList().ForEach(x =>
                    {
                        if (x.IsChecked)
                        {
                            DevPhotometry.Instance.SwitchLight(x.GroupId, false);
                        }
                    });
                    next_Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Interval * 1000;
                }
                else
                {
                    InperDeviceHelper.LightWaveLength.ToList().ForEach(x =>
                    {
                        if (x.IsChecked)
                        {
                            DevPhotometry.Instance.SwitchLight(x.GroupId, true);
                            DevPhotometry.Instance.SetLightPower(x.GroupId, x.LightPower);
                        }
                    });
                    next_Interval = InperGlobalClass.CameraSignalSettings.RecordMode.Duration * 1000;
                }
                _Metronome.Interval = next_Interval;
                IsInWorkingPeriod = !IsInWorkingPeriod;
                return;
            };

            if (@enum == SignalSettingsTypeEnum.Analog)
            {
                for (int i = 0; i < 8; i++)
                {
                    AnalogChannels.Add(new SignalCameraChannel() { ChannelId = i + 1, Name = "AL-" + (i + 1) + "-PFC", BgColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[i])) });
                }
            }
        }

        #region methods  Camera
        private void Init()
        {
            try
            {
                view.camera.Visibility = Visibility.Visible;
                view.channelRoi.IsEnabled = false;

                if (InperDeviceHelper.LightWaveLength.Count > 0)
                {
                    if (InperDeviceHelper.LightWaveLength.Count > 1)
                    {
                        view.waveView.SelectedItem = InperDeviceHelper.LightWaveLength[1];
                    }
                    else
                    {
                        view.waveView.SelectedItem = InperDeviceHelper.LightWaveLength[0];
                    }

                    view.lightMode.ItemsSource = InperDeviceHelper.LightWaveLength;
                }

                InperGlobalClass.CameraSignalSettings.CameraChannels?.ForEach(x =>
                {
                    Grid grid = DrawCircle(x.ChannelId + 1, x.ROI, x.YTop, x.YBottom, x.Left, x.Top);

                    this.view.ellipseCanvas.Children.Add(grid);
                });

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
                    if (EllipseBorder.Count > 4)
                    {
                        grid.SetValue(Canvas.LeftProperty, 55.0);
                        grid.SetValue(Canvas.TopProperty, (double)(EllipseBorder.Count - 4) * Diameter - 20);
                    }
                    else
                    {
                        grid.SetValue(Canvas.LeftProperty, 10.0);
                        grid.SetValue(Canvas.TopProperty, (double)EllipseBorder.Count * Diameter - 20);
                    }
                    this.view.ellipseCanvas.Children.Add(grid);
                }
                else
                {
                    Growl.Info(new GrowlInfo() { Message = "已达到最大数量", Token = "SuccessMsg", WaitTime = 1 });
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

                        CameraChannel item = InperDeviceHelper.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1);
                        _ = InperDeviceHelper.CameraChannels.Remove(item);
                        _ = InperDeviceHelper._SignalQs.Remove(int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1);

                        Lib.Helper.JsonBean.Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId);
                        if (channel != null)
                        {
                            _ = InperGlobalClass.CameraSignalSettings.CameraChannels.Remove(channel);
                        }
                        view.channelName.Text = InperDeviceHelper.CameraChannels.Count > 0 ? InperDeviceHelper.CameraChannels.Last().Name : "";
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
        private Grid DrawCircle(int index, double diameter, double ytop, double ybottom, double left = 1, double top = 1)
        {
            Grid grid = new Grid() { Cursor = Cursors.Hand, Background = Brushes.Transparent, Name = "ROI_" + index };
            grid.SetValue(Canvas.LeftProperty, left);
            grid.SetValue(Canvas.TopProperty, top);
            grid.MouseLeftButtonDown += Grid_MouseDown;
            grid.MouseMove += Grid_MouseMove;
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

            view.channelName.Text = "ROI-" + index + "-PFC";

            if (InperDeviceHelper.CameraChannels.FirstOrDefault(x => x.ChannelId == index - 1) == null)
            {
                CameraChannel item = new CameraChannel()
                {
                    ChannelId = index - 1,
                    Name = "ROI-" + index + "-PFC",
                    YVisibleRange = new SciChart.Data.Model.DoubleRange(ybottom, ytop)
                };
                InperDeviceHelper._SignalQs.Add(index - 1, new SignalData());

                foreach (WaveGroup wave in InperDeviceHelper.LightWaveLength)
                {
                    if (wave.IsChecked)
                    {
                        LightMode<TimeSpan, double> mode = new LightMode<TimeSpan, double>()
                        {
                            LightType = wave.GroupId,
                            WaveColor = wave.GroupId == 1 ? InperColorHelper.SCBrushes[index % 9] : (wave.GroupId == 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF008000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000FF"))),
                            XyDataSeries = new XyDataSeries<TimeSpan, double>()
                        };
                        item.LightModes.Add(mode);
                        LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = wave.GroupId.ToString(), DataSeries = mode.XyDataSeries, Stroke = mode.WaveColor.Color };

                        item.RenderableSeries.Add(line);
                        InperDeviceHelper._SignalQs[index - 1].ValuePairs.Add(mode.LightType, new Queue<KeyValuePair<long, double>>());
                    }
                }

                InperDeviceHelper.CameraChannels.Add(item);

                Lib.Helper.JsonBean.Channel channel = InperGlobalClass.CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == index - 1);

                if (channel == null)
                {
                    InperGlobalClass.CameraSignalSettings.CameraChannels.Add(new Lib.Helper.JsonBean.Channel()
                    {
                        ChannelId = index - 1,
                        Name = "ROI-" + index + "-PFC",
                        Left = double.Parse(grid.GetValue(Canvas.LeftProperty).ToString()),
                        Top = double.Parse(grid.GetValue(Canvas.TopProperty).ToString()),
                        ROI = ellipse.Width,
                        YTop = ytop,
                        YBottom = ybottom,
                    }); ;
                }

                Mat m = Mat.Zeros(new OpenCvSharp.Size(InperDeviceHelper.Instance.VisionWidth, InperDeviceHelper.Instance.VisionHeight), MatType.CV_8U);
                SetMat(m, grid);
            }


            return grid;
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

                        if (tb.Text.Length < 6 || !tb.Text.StartsWith(verify))
                        {
                            tb.Text = "ROI-" + (EllipseBorder.Last().Children[0] as TextBlock).Text + "-";
                            tb.SelectionStart = tb.Text.Length;
                            Growl.Error(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                        else
                        {
                            if (moveGrid == null)
                            {
                                InperDeviceHelper.CameraChannels.Last().Name = tb.Text;
                            }
                            else
                            {
                                InperDeviceHelper.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((moveGrid.Children[0] as TextBlock).Text) - 1).Name = tb.Text;
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
                    DevPhotometry.Instance.SwitchLight(sen.GroupId, true);

                    WaveGroup wg = InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == sen.GroupId);
                    if (wg == null)
                    {
                        wg = sen;
                        InperGlobalClass.CameraSignalSettings.LightMode.Add(sen);
                    }
                }

                foreach (CameraChannel item in InperDeviceHelper.CameraChannels)
                {
                    var mode = item.LightModes.FirstOrDefault(x => x.LightType == sen.GroupId);
                    if (mode == null)
                    {
                        mode = new LightMode<TimeSpan, double>()
                        {
                            LightType = sen.GroupId,
                            WaveColor = sen.GroupId == 1 ? InperColorHelper.SCBrushes[item.ChannelId % 9] : (sen.GroupId == 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF008000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000FF"))),
                            XyDataSeries = new XyDataSeries<TimeSpan, double>()
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
                _ = DevPhotometry.Instance.SetGain(gain);
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
                    DevPhotometry.Instance.SwitchLight(sen.GroupId, false);

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
        public void LightMode_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                WaveGroup sen = (sender as TextBox).DataContext as WaveGroup;
                if (sen.IsChecked)
                {
                    DevPhotometry.Instance.SetLightPower(sen.GroupId, sen.LightPower);

                    InperGlobalClass.CameraSignalSettings.LightMode.FirstOrDefault(x => x.GroupId == sen.GroupId).LightPower = sen.LightPower;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void FPS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                double fps = double.Parse((sender as System.Windows.Controls.ComboBox).SelectedItem.ToString());
                InperGlobalClass.CameraSignalSettings.Sampling = fps;

                _ = DevPhotometry.Instance.SetFrameRate(fps);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Exposure_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                double exposure = double.Parse((sender as System.Windows.Controls.ComboBox).SelectedItem.ToString());
                InperGlobalClass.CameraSignalSettings.Exposure = exposure;

                _ = DevPhotometry.Instance.SetExposure(exposure);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void SetMat(Mat mat, Grid grid)
        {
            double scale = InperDeviceHelper.Instance.VisionWidth / this.view.ellipseCanvas.ActualWidth;
            double rect_left = (double)grid.GetValue(Canvas.LeftProperty) * scale;
            double rect_top = (double)grid.GetValue(Canvas.TopProperty) * scale;

            double ellips_diam = (grid.Children[1] as Ellipse).Width * scale;

            OpenCvSharp.Point Center = new OpenCvSharp.Point(rect_left + (ellips_diam / 2), rect_top + (ellips_diam / 2));

            mat.Circle(center: Center, radius: (int)(ellips_diam / 4), color: Scalar.White, thickness: (int)(ellips_diam / 2));

            InperDeviceHelper.CameraChannels.FirstOrDefault(x => x.ChannelId == int.Parse((grid.Children[0] as TextBlock).Text) - 1).Mask = mat;
        }
        public void Interval_Checked(object sender, RoutedEventArgs e)
        {
            _Metronome.Enabled = true;
        }
        public void Interval_UnChecked(object sender, RoutedEventArgs e)
        {
            _Metronome.Enabled = false;
        }
        public void WaveView_Selected(object sender, RoutedEventArgs e)
        {
            InperDeviceHelper.SelectedWaveType = ((sender as System.Windows.Controls.ComboBox).SelectedItem as WaveGroup).GroupId;
        }
        public void Screenshots()
        {
            try
            {
                InperComputerInfoHelper.SaveFrameworkElementToImage(this.view.ellipseCanvas, "CameraChannelScreen.bmp", InperGlobalClass.DataPath + InperGlobalClass.DataFolderName);
                Growl.Info(new GrowlInfo() { Message = "成功获取快照", Token = "SuccessMsg", WaitTime = 1 });
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
                    if (tb.Text.Length < 5 || !tb.Text.StartsWith("AL-" + (view.AnalogChannelCombox.SelectedIndex + 1) + "-"))
                    {
                        tb.Text = "AL-" + (this.view.AnalogChannelCombox.SelectedIndex + 1) + "-";
                        tb.SelectionStart = tb.Text.Length;
                        Growl.Error(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
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
                var ch = this.view.AnalogChannelCombox.SelectedItem as SignalCameraChannel;
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
                    }
                }
                else
                {
                    if (AnalogChannels.Count > 0 && ch != null)
                    {
                        AnalogChannels.Remove(ch);
                        AnalogActiveChannels.Add(ch);

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
                InperGlobalClass.CameraSignalSettings.RecordMode.IsContinuous = IsContinuous;
                InperGlobalClass.CameraSignalSettings.RecordMode.IsInterval = IsInterval;

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
