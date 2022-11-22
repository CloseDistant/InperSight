using InperSight.Lib.Bean;
using InperSight.Lib.Chart;
using InperSight.Lib.Chart.Channel;
using InperSight.Lib.Config;
using InperSight.Lib.Config.Json;
using InperSight.Lib.Enum;
using InperSight.Lib.Helper;
using InperSight.Views;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using SqlSugar;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace InperSight.ViewModels
{
    public class CameraSettingViewModel : Screen
    {
        private CameraSettingView view;
        private InperDeviceHelper deviceHelper;
        public InperDeviceHelper InperDeviceHelper { get => deviceHelper; set => SetAndNotify(ref deviceHelper, value); }
        private CameraSettingJsonBean cameraSetting;
        public CameraSettingJsonBean CameraSetting
        {
            get
            {
                return cameraSetting;
            }
            set => SetAndNotify(ref cameraSetting, value);
        }
        #region 构造和重载
        public CameraSettingViewModel()
        {
            InperDeviceHelper = InperDeviceHelper.GetInstance();
            CameraSetting = InperGlobalClass.CameraSettingJsonBean;
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            if (!InperDeviceHelper.MiniscopeIsStartCaptrue)
            {
                InperDeviceHelper.GetInstance().MiniscopeStartCapture();
                InperDeviceHelper.DevcieParamSet(DeviceParamProperties.LED, cameraSetting.ExcitLowerLevel);
            }

            view = this.View as CameraSettingView;
            view.Owner = InperGlobalFunc.GetWindowByNameChar("inper");
            view.ConfirmClickEvent += (s, e) => { RequestClose(); };
            if (InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs?.Count > 0)
            {
                NeuronLoade();
            }
            if (InperDeviceHelper.GetInstance().isSwitchToFrame)
            {
                DrawFrameRect(InperDeviceHelper.GetInstance().frameLeft, InperDeviceHelper.GetInstance().frameTop);
                RoutedEventArgs eventArgs = new RoutedEventArgs(Button.ClickEvent);
                view.frame.RaiseEvent(eventArgs);
                currentRectangle.Visibility = Visibility.Collapsed;
            }
            view.gain.SelectedIndex = InperGlobalClass.CameraSettingJsonBean.Gain == 1 ? 0 : InperGlobalClass.CameraSettingJsonBean.Gain == 2 ? 1 : 2;
            view.fps.SelectedIndex = InperGlobalClass.CameraSettingJsonBean.FPS == 10 ? 0 : InperGlobalClass.CameraSettingJsonBean.FPS == 15 ? 1 :
                InperGlobalClass.CameraSettingJsonBean.FPS == 20 ? 2 : InperGlobalClass.CameraSettingJsonBean.FPS == 25 ? 3 : InperGlobalClass.CameraSettingJsonBean.FPS == 30 ? 4 : 2;
        }
        protected override void OnClose()
        {
            base.OnClose();
            JsonHelper.SetCameraSetting(InperGlobalClass.CameraSettingJsonBean);
        }
        private void NeuronLoade()
        {
            try
            {
                InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs.ForEach(x =>
                {
                    Shape shape = null;
                    if (x.Type == RoiShapeEnum.Circle.ToString())
                    {
                        shape = new Ellipse()
                        {
                            Width = x.Diameter,
                            Height = x.Diameter,
                            StrokeThickness = 1,
                            Name = "E_" + x.ChannelId,
                            Fill = Brushes.Transparent,
                            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(x.Color)),
                            Cursor = Cursors.Hand
                        };
                    }
                    else if (x.Type == RoiShapeEnum.Rectangle.ToString())
                    {
                        shape = new Rectangle()
                        {
                            Width = x.RectWidth,
                            Height = x.RectHeight,
                            StrokeThickness = 1,
                            Name = "R_" + x.ChannelId,
                            Fill = Brushes.Transparent,
                            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(x.Color)),
                            Cursor = Cursors.Hand
                        };
                    }
                    currentShape = shape;
                    shape.MouseDown += Shape_MouseDown;
                    Canvas.SetLeft(currentShape, x.Left);
                    Canvas.SetTop(currentShape, x.Top);
                    view.drawAreaCanvas.Children.Add(currentShape);
                    var layer = AdornerLayer.GetAdornerLayer(currentShape);
                    layer.Add(new InperAdorner(currentShape, x.ChannelId.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(x.Color)), x.ChannelId > 10 ? -15 : -10, currentShape.Height / 2 - 6));
                    if (InperDeviceHelper.GetInstance().CameraChannels.FirstOrDefault(c => c.ChannelId == x.ChannelId) == null)
                    {
                        _ = AddChannel(x.ChannelId);
                    }
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion

        #region 视野展示切换
        private List<Shape> switchShapes = new();
        public void FrameShowEvent()
        {
            try
            {
                if (currentRectangle != null)
                {
                    InperDeviceHelper.GetInstance().isSwitchToFrame = true;
                    InperDeviceHelper.GetInstance().frameLeft = Canvas.GetLeft(currentRectangle);
                    InperDeviceHelper.GetInstance().frameTop = Canvas.GetTop(currentRectangle);
                    InperDeviceHelper.GetInstance().frameWidth = currentRectangle.Width;
                    InperDeviceHelper.GetInstance().frameHeight = currentRectangle.Height;
                    for (int i = 0; i < view.drawAreaCanvas.Children.Count; i++)
                    {
                        view.drawAreaCanvas.Children[i].Visibility = Visibility.Collapsed;
                        var layer = AdornerLayer.GetAdornerLayer(view.drawAreaCanvas.Children[i]);
                        if (layer != null)
                        {
                            layer.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    InperGlobalFunc.ShowRemainder("无可放大区域", 1);
                    RoutedEventArgs eventArgs = new RoutedEventArgs(Button.ClickEvent);
                    view.home.RaiseEvent(eventArgs);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void HomeShowEvent()
        {
            try
            {
                InperDeviceHelper.GetInstance().isSwitchToFrame = false;
                if (currentRectangle != null)
                {
                    currentRectangle.Visibility = Visibility.Visible;
                }
                if (switchShapes.Count > 0)
                {
                    switchShapes.ForEach(x =>
                    {
                        AdornerLayer layer = AdornerLayer.GetAdornerLayer(x);
                        layer.Visibility = Visibility.Visible;

                        x.Width = currentRectangle.Width / view.drawAreaCanvas.Width * x.Width;
                        x.Height = currentRectangle.Height / view.drawAreaCanvas.Height * x.Height;

                        double currentLeft = Canvas.GetLeft(x);
                        double currentTop = Canvas.GetTop(x);
                        double rectLeft = Canvas.GetLeft(currentRectangle);
                        double rectTop = Canvas.GetTop(currentRectangle);

                        double actualLeft = rectLeft + currentLeft / view.drawAreaCanvas.Width * currentRectangle.Width;
                        double actualTop = rectTop + currentTop / view.drawAreaCanvas.Height * currentRectangle.Height;

                        x.SetValue(Canvas.LeftProperty, actualLeft);
                        x.SetValue(Canvas.TopProperty, actualTop);


                        InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs.ForEach(xx =>
                        {
                            if (xx.ChannelId.ToString() == x.Name.Split('_').Last())
                            {
                                xx.Diameter = x.Width;
                                xx.Left = actualLeft;
                                xx.Top = actualTop;
                                xx.RectWidth = x.Width;
                                xx.RectHeight = x.Height;
                            }
                        });

                        //layer 布局还原
                        Adorner[] adorners = layer.GetAdorners(x);
                        if (adorners.Length > 0)
                        {
                            foreach (var item in adorners)
                            {
                                layer.Remove(item);
                            }
                        }
                        layer.Add(new InperAdorner(x, x.Name.Split('_').Last(), Brushes.Red, -10, x.Height / 2 - 6));
                    });
                    switchShapes.Clear();
                }
                for (int i = 0; i < view.drawAreaCanvas.Children.Count; i++)
                {
                    view.drawAreaCanvas.Children[i].Visibility = Visibility.Visible;
                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(view.drawAreaCanvas.Children[i]);
                    layer.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void DeltaFModeEvent()
        {
            try
            {
                if (!InperDeviceHelper.isDeltaF)
                {
                    view.tb3.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6523A5"));
                }
                else
                {
                    view.tb3.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
                }
                InperDeviceHelper.isDeltaF = !InperDeviceHelper.isDeltaF;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
            }
        }
        #endregion

        #region roi add delete clear RoiType
        private bool addRoiEventState = false;
        private System.Windows.Point startPoint;
        public void AddRoiEvent()
        {
            addRoiEventState = true;
            addFrameEventState = false;
            currentShape = null;
            view.neuronName.Text = "Neuron " + (InperDeviceHelper.GetInstance().CameraChannels.Count + 1);
            this.view.Cursor = Cursors.Cross;
            if (currentRectangle != null)
            {
                currentRectangle.Visibility = Visibility.Collapsed;
            }
        }
        public void DeleteRoiEvent()
        {
            try
            {
                if (currentShape != null)
                {
                    if (int.TryParse(currentShape.Name.Split('_').Last(), out int id))
                    {
                        //删除通道和配置文件
                        if (InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs.FirstOrDefault(x => x.ChannelId == id) is CameraChannelConfig cameraChannelConfig)
                        {
                            InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs.Remove(cameraChannelConfig);
                        }
                        if (InperDeviceHelper.GetInstance().CameraChannels.FirstOrDefault(x => x.ChannelId == id) is CameraChannel cameraChannel)
                        {
                            InperDeviceHelper.GetInstance().CameraChannels.Remove(cameraChannel);
                        }
                        //删除显示控件
                        view.drawAreaCanvas.Children.Remove(currentShape);
                    }
                    currentShape = null;
                }
                this.view.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                InperGlobalFunc.ShowRemainder("Delete failed", 2);
            }
        }
        public void ClearRoiEvent()
        {
            try
            {
                if (MessageBox.Show("是否删除所有Roi", "Ask", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    //删除所有通道和配置文件
                    InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs.Clear();
                    InperDeviceHelper.GetInstance().CameraChannels.Clear();
                    //删除所有展示的控件
                    view.drawAreaCanvas.Children.Clear();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                InperGlobalFunc.ShowRemainder("Clear failed", 2);
            }
        }
        public void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (addRoiEventState)
                {
                    startPoint = e.GetPosition(sender as Canvas);
                    var type = view.roiType.SelectedValue as ComboBoxItem;
                    if (type.Content.ToString().Equals("Circle"))
                    {
                        currentShape = DrawCircle();
                    }
                    else if (type.Content.ToString().Equals("Rectangle"))
                    {
                        currentShape = DrawRectangle();
                    }
                    Canvas.SetLeft(currentShape, startPoint.X);
                    Canvas.SetTop(currentShape, startPoint.Y);
                    view.drawAreaCanvas.Children.Add(currentShape);

                    if (InperDeviceHelper.GetInstance().isSwitchToFrame)
                    {
                        switchShapes.Add(currentShape);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #region 绘制图形
        private Shape currentShape;
        private Rectangle DrawRectangle()
        {
            int id = InperDeviceHelper.GetInstance().CameraChannels.Count == 0 ? InperDeviceHelper.GetInstance().CameraChannels.Count + 1 : InperDeviceHelper.GetInstance().CameraChannels.Last().ChannelId + 1;
            var rect = new Rectangle()
            {
                Width = 10,
                Height = 10,
                StrokeThickness = 1,
                Name = "R_" + id,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[id % InperColorHelper.ColorPresetList.Count])),
                Cursor = Cursors.Hand
            };
            rect.MouseDown += Shape_MouseDown;
            return rect;
        }
        private Ellipse DrawCircle()
        {
            int id = InperDeviceHelper.GetInstance().CameraChannels.Count == 0 ? InperDeviceHelper.GetInstance().CameraChannels.Count + 1 : InperDeviceHelper.GetInstance().CameraChannels.Last().ChannelId + 1;
            Ellipse ellipse = new Ellipse()
            {
                Width = 10,
                Height = 10,
                StrokeThickness = 1,
                Name = "E_" + id,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[id % InperColorHelper.ColorPresetList.Count])),
                Cursor = Cursors.Hand
            };
            ellipse.MouseDown += Shape_MouseDown;

            return ellipse;
        }

        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var ellipse = sender as Shape;
                if (int.TryParse(ellipse.Name.Split('_').Last(), out int id))
                {
                    view.neuronName.Text = InperDeviceHelper.GetInstance().CameraChannels.FirstOrDefault(x => x.ChannelId == id).Name;
                }
                currentShape = ellipse;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion
        public void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (addRoiEventState)
                {
                    _ = int.TryParse(currentShape.Name.Split('_').Last(), out int id);

                    addRoiEventState = false;
                    var layer = AdornerLayer.GetAdornerLayer(currentShape);
                    var color = InperColorHelper.ColorPresetList[id % InperColorHelper.ColorPresetList.Count];

                    layer.Add(new InperAdorner(currentShape, id.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)), id > 10 ? -15 : -10, currentShape.Height / 2 - 6));
                    #region 通道添加
                    CameraChannel cameraChannel = AddChannel(id);
                    //配置文件添加
                    InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs.Add(new Lib.Config.Json.CameraChannelConfig()
                    {
                        ChannelId = cameraChannel.ChannelId,
                        Name = cameraChannel.Name,
                        Color = color,
                        Diameter = currentShape.Width,
                        RectHeight = currentShape.Height,
                        RectWidth = currentShape.Width,
                        Left = (double)currentShape.GetValue(Canvas.LeftProperty),
                        Top = (double)currentShape.GetValue(Canvas.TopProperty),
                        Type = (view.roiType.SelectedItem as ComboBoxItem).Content.ToString() == RoiShapeEnum.Circle.ToString() ? RoiShapeEnum.Circle.ToString() : RoiShapeEnum.Rectangle.ToString()
                    });
                    #endregion
                    currentShape = null;
                    this.view.Cursor = Cursors.Arrow;
                    if (currentRectangle != null && !InperDeviceHelper.GetInstance().isSwitchToFrame)
                    {
                        currentRectangle.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        private CameraChannel AddChannel(int channelId)
        {
            CameraChannel cameraChannel = new CameraChannel
            {
                Name = view.neuronName.Text.ToString(),
                ChannelId = channelId
            };
            cameraChannel.TimeSpanAxis = new SciChart.Charting.Visuals.Axes.TimeSpanAxis()
            {
                DrawMajorBands = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                VisibleRange = cameraChannel.XVisibleRange,
                Visibility = Visibility.Collapsed
            };
            LineRenderableSeriesViewModel line = new LineRenderableSeriesViewModel() { Tag = cameraChannel.ChannelId, DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[channelId % InperColorHelper.ColorPresetList.Count])).Color };
            cameraChannel.RenderableSeries.Add(line);
            cameraChannel.Mask = SetMat(currentShape);

            InperDeviceHelper.GetInstance().CameraChannels.Add(cameraChannel);
            return cameraChannel;
        }
        private Mat SetMat(Shape ellipse)
        {
            Mat m = Mat.Zeros(new OpenCvSharp.Size(InperDeviceHelper.GetInstance().ImageWidth, InperDeviceHelper.GetInstance().ImageHeight), MatType.CV_8U);
            double scale = InperDeviceHelper.GetInstance().ImageWidth / (this.view.drawAreaCanvas.ActualWidth == 0 ? this.view.drawAreaCanvas.Width : this.view.drawAreaCanvas.ActualWidth);
            double rect_left = (double)ellipse.GetValue(Canvas.LeftProperty) * scale;
            double rect_top = (double)ellipse.GetValue(Canvas.TopProperty) * scale;

            double ellips_diam = ellipse.Width * scale;
            double shape_height = ellipse.Height * scale;
            OpenCvSharp.Point Center = new(rect_left + (ellips_diam / 2), rect_top + (shape_height / 2));
            if (ellipse.GetType() == typeof(Ellipse))
            {
                //m.Circle(center: Center, radius: (int)(ellips_diam / 4), color: Scalar.White, -1);
                double angle = ellips_diam > shape_height ? 0 : 180;
                m.Ellipse(Center, new OpenCvSharp.Size(ellips_diam/2, shape_height/2), angle, 0, 360, Scalar.White, -1);
            }
            else
            {
                m.Rectangle(new OpenCvSharp.Rect(new OpenCvSharp.Point(rect_left, rect_top), new OpenCvSharp.Size(ellips_diam, shape_height)), Scalar.White, -1);
            }
            return m;
        }
        public void Image_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (addRoiEventState && currentShape != null)
                {
                    var movePoint = e.GetPosition(sender as Canvas);
                    double currentLeft = Canvas.GetLeft(currentShape), currentTop = Canvas.GetTop(currentShape);
                    if (currentShape.GetType() == typeof(Ellipse))
                    {
                        if (view.drawAreaCanvas.Width - currentLeft < Math.Abs(movePoint.Y - startPoint.Y))
                        {
                            return;
                        }
                        if (Math.Abs(movePoint.Y - startPoint.Y) > 5)
                        {
                            var diameter = Math.Abs(movePoint.Y - startPoint.Y) > Math.Abs(movePoint.X - startPoint.X) ? Math.Abs(movePoint.Y - startPoint.Y) : Math.Abs(movePoint.X - startPoint.X);
                            currentShape.Width = Math.Abs(movePoint.X - startPoint.X);
                            currentShape.Height = Math.Abs(movePoint.Y - startPoint.Y);

                            if (movePoint.Y - startPoint.Y < 0)
                            {
                                Canvas.SetTop(currentShape, startPoint.Y - Math.Abs(movePoint.Y - startPoint.Y));
                            }
                            if (movePoint.X - startPoint.X < 0)
                            {
                                Canvas.SetLeft(currentShape, startPoint.X - Math.Abs(movePoint.X - startPoint.X));
                            }
                        }
                    }
                    else if (currentShape.GetType() == typeof(Rectangle))
                    {
                        if (view.drawAreaCanvas.Width - currentLeft < Math.Abs(movePoint.X - startPoint.X) || view.drawAreaCanvas.Height - currentTop < Math.Abs(movePoint.Y - startPoint.Y))
                        {
                            return;
                        }
                        if (Math.Abs(movePoint.Y - startPoint.Y) > 5 || Math.Abs(movePoint.X - startPoint.X) > 5)
                        {
                            currentShape.Width = Math.Abs(movePoint.X - startPoint.X);
                            currentShape.Height = Math.Abs(movePoint.Y - startPoint.Y);

                            if (movePoint.Y - startPoint.Y < 0)
                            {
                                Canvas.SetTop(currentShape, startPoint.Y - Math.Abs(movePoint.Y - startPoint.Y));
                            }
                            if (movePoint.X - startPoint.X < 0)
                            {
                                Canvas.SetLeft(currentShape, startPoint.X - Math.Abs(movePoint.X - startPoint.X));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (addRoiEventState)
                {
                    Image_MouseLeftButtonUp(null, null);
                    //addRoiEventState = false;
                    //var layer = AdornerLayer.GetAdornerLayer(currentEllipse);
                    //layer.Add(new InperAdorner(currentEllipse, InperDeviceHelper.GetInstance().CameraChannels.Count.ToString(), Brushes.Red, currentEllipse.Width / 2 - 3, currentEllipse.Height / 2 - 6));
                    //currentEllipse = null;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion

        #region frame setting
        private Rectangle currentRectangle = null;
        private bool addFrameEventState = false;
        private System.Windows.Point frameStartPoint;
        public void FrameAddEvent()
        {
            try
            {
                if (currentRectangle != null)
                {
                    InperGlobalFunc.ShowRemainder("已存在", 1);
                    return;
                }
                addFrameEventState = false;
                addRoiEventState = false;

                DrawFrameRect();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        private void DrawFrameRect(double left = 0, double top = 0)
        {
            Rectangle rectangle = new()
            {
                Width = view.drawAreaCanvas.Width / 2,
                Height = view.drawAreaCanvas.Height / 2,
                StrokeThickness = 1,
                Stroke = Brushes.Red,
                Fill = Brushes.Transparent
            };
            rectangle.MouseMove += Rectangle_MouseMove;
            rectangle.MouseLeftButtonDown += Rectangle_MouseDown;
            rectangle.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
            Canvas.SetLeft(rectangle, left == 0 ? rectangle.Width / 2 : left);
            Canvas.SetTop(rectangle, top == 0 ? rectangle.Height / 2 : top);
            currentRectangle = rectangle;
            view.drawAreaCanvas.Children.Add(rectangle);
        }
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                addFrameEventState = true;
                addRoiEventState = false;
                frameStartPoint = e.GetPosition(sender as Rectangle);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                addFrameEventState = false;
                addRoiEventState = false;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (addFrameEventState)
                {
                    var rect = sender as Rectangle;
                    System.Windows.Point movePoint = e.GetPosition(rect);
                    double moveX = movePoint.X - frameStartPoint.X;
                    double moveY = movePoint.Y - frameStartPoint.Y;
                    double currentX = Canvas.GetLeft(rect);
                    double currentY = Canvas.GetTop(rect);

                    double left = currentX + moveX <= 0
                        ? currentX
                        : currentX + moveX > view.drawAreaCanvas.Width / 2 - 1 ? view.drawAreaCanvas.Width / 2 - 1 : currentX + moveX;

                    double top = currentY + moveY <= 0
                        ? currentY
                        : currentY + moveY > view.drawAreaCanvas.Height / 2 - 1 ? view.drawAreaCanvas.Height / 2 - 1 : currentY + moveY;
                    Canvas.SetLeft(rect, left);
                    Canvas.SetTop(rect, top);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }

        public void FrameDeleteEvent()
        {
            try
            {
                if (currentRectangle != null)
                {
                    view.drawAreaCanvas.Children.Remove(currentRectangle);
                    currentRectangle = null;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void FrameClearEvent()
        {

        }
        #endregion

        #region 截图和获取路径
        public void CutViewEvent()
        {
            try
            {
                view.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //InperComputerInfoHelper.SaveFrameworkElementToImage(this.view.ellipseCanvas, DateTime.Now.ToString("HHmmss") + "CameraScreen.bmp", System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
                    //获取控件相对于窗口位置
                    GeneralTransform generalTransform = this.view.drawAreaCanvas.TransformToAncestor(this.view.camera);
                    System.Windows.Point point = generalTransform.Transform(new System.Windows.Point(0, 0));

                    //获取窗口相对于屏幕的位置
                    System.Windows.Point ptLeftUp = new System.Windows.Point(0, 0);
                    ptLeftUp = this.view.PointToScreen(ptLeftUp);

                    //计算DPI缩放
                    var ct = PresentationSource.FromVisual(view.drawAreaCanvas)?.CompositionTarget;
                    var matrix = ct == null ? Matrix.Identity : ct.TransformToDevice;

                    double x = matrix.M11 == 0 ? 1 : matrix.M11;
                    double y = matrix.M22 == 0 ? 1 : matrix.M22;

                    int left = (int)ptLeftUp.X + (int)(point.X * x);
                    int top = (int)ptLeftUp.Y + (int)(point.Y * y);

                    InperGlobalFunc.SaveScreenToImageByPoint(left, top, (int)(Math.Ceiling(view.drawAreaCanvas.ActualWidth) * x), (int)(Math.Ceiling(view.drawAreaCanvas.ActualHeight) * y), System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmssffff") + "CameraScreen.png"));
                    InperGlobalFunc.ShowRemainder("截取成功");
                }));
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void ShowImagePath()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                psi.Arguments = "/e,/select," + System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName);
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion

        #region 相机参数设置
        public void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                string type = (sender as Slider).Name;
                switch (type)
                {
                    case "focusPlane":
                        CameraSetting.FocusPlane = e.NewValue;
                        InperDeviceHelper.DevcieParamSet(DeviceParamProperties.EWL, e.NewValue);
                        break;
                    case "upperLevel":
                        CameraSetting.UpperLevel = e.NewValue;
                        break;
                    case "lowerLevel":
                        CameraSetting.LowerLevel = e.NewValue;
                        break;
                    case "excitLowerLevel": //led
                        CameraSetting.ExcitLowerLevel = e.NewValue;
                        InperDeviceHelper.DevcieParamSet(DeviceParamProperties.LED, e.NewValue);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void FpsAndGain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string type = (sender as ComboBox).Name;
                switch (type)
                {
                    case "fps":
                        double value = double.Parse(((sender as ComboBox).SelectedValue as ComboBoxItem).Content.ToString());
                        CameraSetting.FPS = (int)value;
                        InperDeviceHelper.MiniscopeFpsReset(CameraSetting.FPS);
                        InperDeviceHelper.DevcieParamSet(DeviceParamProperties.FRAMERATE, value);
                        break;
                    case "gain":
                        string _type = ((sender as ComboBox).SelectedValue as ComboBoxItem).Content.ToString();
                        double _vlaue = _type == "Low" ? 1 : _type == "Medium" ? 2 : 3.5;
                        CameraSetting.Gain = _vlaue;
                        InperDeviceHelper.DevcieParamSet(DeviceParamProperties.GAIN, _vlaue);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void Param_Reduce_Event(string param)
        {
            try
            {
                switch (param)
                {
                    case "focusPlane":
                        CameraSetting.FocusPlane -= 1;
                        if (CameraSetting.FocusPlane < -127)
                        {
                            CameraSetting.FocusPlane = -127;
                        }
                        view.focusPlane.Value = CameraSetting.FocusPlane;
                        break;
                    case "upperLevel":
                        CameraSetting.UpperLevel -= 1;
                        if (CameraSetting.UpperLevel < 0)
                        {
                            CameraSetting.UpperLevel = 0;
                        }
                        view.upperLevel.Value = CameraSetting.UpperLevel;
                        break;
                    case "lowerLevel":
                        CameraSetting.LowerLevel -= 1;
                        if (CameraSetting.LowerLevel < 0)
                        {
                            CameraSetting.LowerLevel = 0;
                        }
                        view.lowerLevel.Value = CameraSetting.LowerLevel;
                        break;
                    case "excitLowerLevel":
                        CameraSetting.ExcitLowerLevel -= 1;
                        if (CameraSetting.ExcitLowerLevel < 0)
                        {
                            CameraSetting.ExcitLowerLevel = 0;
                        }
                        view.excitLowerLevel.Value = CameraSetting.ExcitLowerLevel;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void Param_Add_Event(string param)
        {
            try
            {
                switch (param)
                {
                    case "focusPlane":
                        CameraSetting.FocusPlane += 1;
                        if (CameraSetting.FocusPlane > 127)
                        {
                            CameraSetting.FocusPlane = 127;
                        }
                        view.focusPlane.Value = CameraSetting.FocusPlane;
                        break;
                    case "upperLevel":
                        CameraSetting.UpperLevel += 1;
                        if (CameraSetting.UpperLevel > 10)
                        {
                            CameraSetting.UpperLevel = 10;
                        }
                        view.upperLevel.Value = CameraSetting.UpperLevel;
                        break;
                    case "lowerLevel":
                        CameraSetting.LowerLevel += 1;
                        if (CameraSetting.LowerLevel > 100)
                        {
                            CameraSetting.LowerLevel = 100;
                        }
                        view.lowerLevel.Value = CameraSetting.LowerLevel;
                        break;
                    case "excitLowerLevel":
                        CameraSetting.ExcitLowerLevel += 1;
                        if (CameraSetting.ExcitLowerLevel > 100)
                        {
                            CameraSetting.ExcitLowerLevel = 100;
                        }
                        view.excitLowerLevel.Value = CameraSetting.ExcitLowerLevel;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!(sender as TextBox).IsFocused)
                {
                    return;
                }
                string type = (sender as TextBox).Name;
                double value = double.Parse((sender as TextBox).Text);
                switch (type)
                {

                    case "focusPlane":
                        CameraSetting.FocusPlane = value;
                        InperDeviceHelper.DevcieParamSet(DeviceParamProperties.EWL, value);
                        break;
                    case "upperLevel":
                        break;
                    case "lowerLevel":
                        break;
                    case "excitLowerLevel": //led
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion

        #region 加锁解锁
        //public void LockEvent()
        //{
        //    this.view.Topmost = true;
        //    view.unLock.Visibility = Visibility.Visible;
        //    view._lock.Visibility = Visibility.Collapsed;
        //}
        //public void UnLockEvent()
        //{
        //    this.view.Topmost = false;
        //    view.unLock.Visibility = Visibility.Collapsed;
        //    view._lock.Visibility = Visibility.Visible;
        //}
        #endregion
    }
}
