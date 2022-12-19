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
using SciChart.Core.Extensions;
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
            view.ConfirmClickEvent += (s, e) => { this.view.Hide(); /*RequestClose();*/ };
            if (InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs?.Count > 0)
            {
                NeuronLoade();
            }
            view.Owner = InperGlobalFunc.GetWindowByNameChar("inper");
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
                    else if (x.Type == RoiShapeEnum.Polygon.ToString())
                    {
                        shape = new Polygon()
                        {
                            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(x.Color)),
                            Cursor = Cursors.Hand,
                            StrokeThickness = 1,
                            Name = "P_" + x.ChannelId,
                            Fill = Brushes.Transparent,
                            Points = new PointCollection(x.Points)
                        };
                    }
                    currentShape = shape;
                    currentShape.MouseDown += Shape_MouseDown;
                    view.drawAreaCanvas.Children.Add(currentShape);
                    if (InperDeviceHelper.GetInstance().CameraChannels.FirstOrDefault(c => c.ChannelId == x.ChannelId) == null)
                    {
                        _ = AddChannel(x.ChannelId, x.Name);
                    }

                    if (x.Type != RoiShapeEnum.Polygon.ToString())
                    {
                        Canvas.SetLeft(currentShape, x.Left);
                        Canvas.SetTop(currentShape, x.Top);
                        var layer = AdornerLayer.GetAdornerLayer(currentShape);
                        layer.Add(new InperAdorner(currentShape, x.ChannelId.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(x.Color)), x.ChannelId > 10 ? -15 : -10, currentShape.Height / 2 - 6));
                    }
                    else
                    {
                        var layer = AdornerLayer.GetAdornerLayer(currentShape);
                        var p = (currentShape as Polygon).Points.First();
                        layer.Add(new InperAdorner(currentShape, x.ChannelId.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(x.Color)), p.X - 10, p.Y));
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
                    view.zoomCoefficient.IsEnabled = false;
                    double scale = InperDeviceHelper.GetInstance().ImageWidth / view.drawAreaCanvas.Width;
                    InperDeviceHelper.GetInstance().frameLeft = Canvas.GetLeft(currentRectangle) * scale;
                    InperDeviceHelper.GetInstance().frameTop = Canvas.GetTop(currentRectangle) * scale;
                    InperDeviceHelper.GetInstance().frameWidth = currentRectangle.Width * scale;
                    InperDeviceHelper.GetInstance().frameHeight = currentRectangle.Height * scale;
                    InperDeviceHelper.GetInstance().isSwitchToFrame = true;
                    for (int i = 0; i < view.drawAreaCanvas.Children.Count; i++)
                    {
                        view.drawAreaCanvas.Children[i].Visibility = Visibility.Collapsed;
                        var layer = AdornerLayer.GetAdornerLayer(view.drawAreaCanvas.Children[i]);
                        if (layer != null)
                        {
                            layer.Visibility = Visibility.Collapsed;
                        }
                    }
                    InperDeviceHelper.GetInstance().RestDeltaF();
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
                view.zoomCoefficient.IsEnabled = true;
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
                        double currentLeft = Canvas.GetLeft(x);
                        double currentTop = Canvas.GetTop(x);
                        double rectLeft = Canvas.GetLeft(currentRectangle);
                        double rectTop = Canvas.GetTop(currentRectangle);
                        if (!x.GetType().Equals(typeof(Polygon)))
                        {
                            x.Width = currentRectangle.Width / view.drawAreaCanvas.Width * x.Width;
                            x.Height = currentRectangle.Height / view.drawAreaCanvas.Height * x.Height;
                        }
                        else
                        {
                            PointCollection points = new PointCollection();
                            (x as Polygon).Points.ForEachDo(x =>
                            {
                                x.X = rectLeft + currentRectangle.Width / view.drawAreaCanvas.Width * x.X;
                                x.Y = rectTop + currentRectangle.Height / view.drawAreaCanvas.Height * x.Y;
                                points.Add(x);
                            });
                            (x as Polygon).Points = points;
                        }


                        double actualLeft = rectLeft + currentLeft / view.drawAreaCanvas.Width * currentRectangle.Width;
                        double actualTop = rectTop + currentTop / view.drawAreaCanvas.Height * currentRectangle.Height;
                        if (actualLeft.ToString() != double.NaN.ToString() && actualTop.ToString() != double.NaN.ToString())
                        {
                            x.SetValue(Canvas.LeftProperty, actualLeft);
                            x.SetValue(Canvas.TopProperty, actualTop);
                        }


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
                            if (x.GetType().Equals(typeof(Polygon)))
                            {
                                xx.Points = (x as Polygon).Points.ToList();
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
                        if (!x.GetType().Equals(typeof(Polygon)))
                        {
                            layer.Add(new InperAdorner(x, x.Name.Split('_').Last(), Brushes.Red, -10, x.Height / 2 - 6));
                        }
                        else
                        {
                            var p = (x as Polygon).Points.First();
                            layer.Add(new InperAdorner(x, x.Name.Split('_').Last(), Brushes.Red, p.X - 10, p.Y));
                        }
                    });
                    switchShapes.Clear();
                }
                for (int i = 0; i < view.drawAreaCanvas.Children.Count; i++)
                {
                    view.drawAreaCanvas.Children[i].Visibility = Visibility.Visible;
                    if ((view.drawAreaCanvas.Children[i] as Shape).Name.Contains('_'))
                    {
                        Panel.SetZIndex(view.drawAreaCanvas.Children[i], 9);
                    }
                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(view.drawAreaCanvas.Children[i]);
                    layer.Visibility = Visibility.Visible;
                }
                InperDeviceHelper.GetInstance().RestDeltaF();
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
            int id = InperDeviceHelper.GetInstance().CameraChannels.Count == 0 ? InperDeviceHelper.GetInstance().CameraChannels.Count + 1 : InperDeviceHelper.GetInstance().CameraChannels.Last().ChannelId + 1;
            view.neuronName.Text = "Neuron " + id;
            this.view.Cursor = Cursors.Cross;
            if (currentRectangle != null)
            {
                currentRectangle.Visibility = Visibility.Collapsed;
            }
            view.addRoiBut.Visibility = Visibility.Collapsed;
            view.addInperThemeRoiBut.Visibility = Visibility.Visible;
        }
        public void DeleteRoiEvent()
        {
            try
            {
                if (InperGlobalClass.IsRecord && InperDeviceHelper.GetInstance().CameraChannels.Count < 2)
                {
                    InperGlobalFunc.ShowRemainder("运行过程中，Neuron 数量不能小于1");
                    return;
                }
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
                addRoiEventState = false;
                if (currentRectangle != null)
                {
                    currentRectangle.Visibility = Visibility.Visible;
                }
                view.addRoiBut.Visibility = Visibility.Visible;
                view.addInperThemeRoiBut.Visibility = Visibility.Collapsed;
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
        private void UpdateAdorner()
        {
            foreach (UIElement shpe in view.drawAreaCanvas.Children)
            {
                var shape = shpe as Shape;
                if (shape != null)
                {
                    if (int.TryParse(shape.Name.Split('_').Last(), out int _id))
                    {
                        var _layer = AdornerLayer.GetAdornerLayer(shape);
                        var _ados = _layer.GetAdorners(shape);
                        if (_ados.Length > 0)
                        {
                            for (int i = 0; i < _ados.Length; i++)
                            {
                                _layer.Remove(_ados[i]);
                            }
                        }
                        if (shape.GetType() == typeof(Polygon))
                        {
                            var polygon = ((Polygon)shape).Points.First();
                            _layer.Add(new InperAdorner(shape, _id.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[_id % InperColorHelper.ColorPresetList.Count])), polygon.X - 10, polygon.Y, false));
                        }
                        else
                        {
                            double top = shape.Height.ToString() == double.NaN.ToString() ? shape.ActualHeight / 2 - 6 : shape.Height / 2 - 6;
                            _layer.Add(new InperAdorner(shape, _id.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[_id % InperColorHelper.ColorPresetList.Count])), _id > 10 ? -15 : -10, top, false));
                        }
                    }
                }
            }
        }
        private void UpdateAloneAdorner(UIElement uIElement, int id)
        {
            #region 更新当前layer
            var layer = AdornerLayer.GetAdornerLayer(uIElement);
            var ados = layer.GetAdorners(uIElement);
            if (ados.Length > 0)
            {
                for (int i = 0; i < ados.Length; i++)
                {
                    layer.Remove(ados[i]);
                }
            }

            if (uIElement.GetType() == typeof(Polygon))
            {
                var p = (uIElement as Polygon).Points.First();
                layer.Add(new InperAdorner(uIElement, id.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[id % InperColorHelper.ColorPresetList.Count])), p.X - 10, p.Y, true));
            }
            else
            {
                var top = (uIElement as Shape).Height.ToString() == double.NaN.ToString() ? (uIElement as Shape).ActualHeight / 2 - 6 : (uIElement as Shape).Height / 2 - 6;
                layer.Add(new InperAdorner((uIElement as Shape), id.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[id % InperColorHelper.ColorPresetList.Count])), id > 10 ? -15 : -10, top, true));
            }
            #endregion
        }
        public void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (addRoiEventState)
                {
                    startPoint = e.GetPosition(sender as Canvas);
                    var type = view.roiType.SelectedValue as ComboBoxItem;

                    UpdateAdorner();

                    if (type.Content.ToString().Equals(RoiShapeEnum.Circle.ToString()))
                    {
                        currentShape = DrawCircle();
                    }
                    else if (type.Content.ToString().Equals(RoiShapeEnum.Rectangle.ToString()))
                    {
                        currentShape = DrawRectangle();
                    }
                    else if (type.Content.ToString().Equals(RoiShapeEnum.Polygon.ToString()))
                    {
                        currentShape = DrawPolygon(startPoint);
                    }
                    if (!type.Content.ToString().Equals(RoiShapeEnum.Polygon.ToString()))
                    {
                        Canvas.SetLeft(currentShape, startPoint.X);
                        Canvas.SetTop(currentShape, startPoint.Y);
                    }
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
        private Polygon DrawPolygon(System.Windows.Point point)
        {
            int id = InperDeviceHelper.GetInstance().CameraChannels.Count == 0 ? InperDeviceHelper.GetInstance().CameraChannels.Count + 1 : InperDeviceHelper.GetInstance().CameraChannels.Last().ChannelId + 1;
            Polygon polygon = new()
            {
                Name = "P_" + id,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[id % InperColorHelper.ColorPresetList.Count])),
                StrokeThickness = 1,
                Cursor = Cursors.Hand,
                Points = new PointCollection()
            };
            polygon.Points.Add(point);
            polygon.MouseDown += Shape_MouseDown;
            return polygon;
        }

        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!addRoiEventState)
                {
                    var ellipse = sender as Shape;
                    if (int.TryParse(ellipse.Name.Split('_').Last(), out int id))
                    {
                        view.neuronName.Text = InperDeviceHelper.GetInstance().CameraChannels.FirstOrDefault(x => x.ChannelId == id).Name;
                    }
                    UpdateAdorner();
                    UpdateAloneAdorner(ellipse, id);

                    currentShape = ellipse;
                    currentShape.MouseMove += Ellipse_MouseMove;
                    currentShape.MouseUp += Ellipse_MouseUp;
                    currentShape.MouseLeave += Ellipse_MouseLeave;

                    shapeIsMove = true;
                    shapeMoverStartPoint = e.GetPosition(ellipse);
                    if (ellipse.GetType() == typeof(Polygon))
                    {
                        polygonPoints.Clear();
                        (ellipse as Polygon).Points.ToList().ForEach(x =>
                        {
                            polygonPoints.Add(x);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #region shape 移动
        bool shapeIsMove = false;
        System.Windows.Point shapeMoverStartPoint;
        System.Windows.Point shapeMovePoint;
        List<System.Windows.Point> polygonPoints = new();
        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (shapeIsMove && currentShape != null)
            {
                var movePoint = e.GetPosition(sender as Shape);
                if (currentShape.GetType() != typeof(Polygon))
                {
                    double currentLeft = Canvas.GetLeft(currentShape), currentTop = Canvas.GetTop(currentShape);

                    if ((currentLeft < 3 && (movePoint.X - shapeMovePoint.X) < 0) ||
                        (currentLeft > (view.drawAreaCanvas.Width - currentShape.Width - 3) && (movePoint.X - shapeMovePoint.X) > 0) ||
                        (currentTop < 3 && (movePoint.Y - shapeMovePoint.Y) < 0) ||
                        (currentTop > (view.drawAreaCanvas.Height - currentShape.Height - 3) && (movePoint.Y - shapeMovePoint.Y) > 0))
                    {
                        return;
                    }

                    shapeMovePoint = movePoint;
                    Canvas.SetTop(currentShape, currentTop + shapeMovePoint.Y - shapeMoverStartPoint.Y);
                    Canvas.SetLeft(currentShape, currentLeft + shapeMovePoint.X - shapeMoverStartPoint.X);
                }
                else
                {
                    double moveLeft = movePoint.X - shapeMoverStartPoint.X, moveTop = movePoint.Y - shapeMoverStartPoint.Y;
                    var poly = currentShape as Polygon;

                    var xMin = polygonPoints.Min(x => x.X);
                    var xMax = polygonPoints.Max(x => x.X);
                    var yMin = polygonPoints.Min(y => y.Y);
                    var yMax = polygonPoints.Max(y => y.Y);

                    if (moveLeft + xMin < 3 || moveLeft + xMax > view.drawAreaCanvas.Width - 3 || moveTop + yMin < 3 || moveTop + yMax > view.drawAreaCanvas.Height - 3)
                    {
                        return;
                    }

                    if (polygonPoints.Count > 0)
                    {
                        for (int i = 0; i < polygonPoints.Count; i++)
                        {
                            poly.Points[i] = new System.Windows.Point(polygonPoints[i].X + moveLeft, polygonPoints[i].Y + moveTop);
                        }
                    }
                }
                #region 更新当前layer

                var id = int.Parse((sender as Shape).Name.Split('_').Last());
                UpdateAloneAdorner(sender as Shape, id);
                #endregion
            }
        }
        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                shapeIsMove = false;
                if (currentShape != null)
                {
                    currentShape.MouseMove -= Ellipse_MouseMove;
                    currentShape.MouseUp -= Ellipse_MouseUp;
                    currentShape.MouseLeave -= Ellipse_MouseLeave;
                    //更新坐标
                    if (currentShape.GetType() == typeof(Polygon))
                    {
                        var poly = currentShape as Polygon;
                        _ = int.TryParse(currentShape.Name.Split('_').Last(), out int id);
                        InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs.FirstOrDefault(x => x.ChannelId == id && x.Type == RoiShapeEnum.Polygon.ToString()).Points = new List<System.Windows.Point>(poly.Points);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
            }
        }
        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            Ellipse_MouseUp(null, null);
        }
        #endregion

        #endregion
        public void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (addRoiEventState && currentShape != null)
                {
                    _ = int.TryParse(currentShape.Name.Split('_').Last(), out int id);

                    addRoiEventState = false;
                    var layer = AdornerLayer.GetAdornerLayer(currentShape);
                    var color = InperColorHelper.ColorPresetList[id % InperColorHelper.ColorPresetList.Count];
                    if (currentShape.GetType() == typeof(Polygon))
                    {
                        var point = (currentShape as Polygon).Points.First();
                        layer.Add(new InperAdorner(currentShape, id.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)), point.X - 10, point.Y, true));
                    }
                    else
                    {
                        double top = currentShape.Height.ToString() == double.NaN.ToString() ? currentShape.ActualHeight / 2 - 6 : currentShape.Height / 2 - 6;
                        layer.Add(new InperAdorner(currentShape, id.ToString(), new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)), id > 10 ? -15 : -10, top, true));
                    }
                    #region 通道添加
                    CameraChannel cameraChannel = AddChannel(id);
                    //配置文件添加
                    var chn = new Lib.Config.Json.CameraChannelConfig()
                    {
                        ChannelId = cameraChannel.ChannelId,
                        Name = cameraChannel.Name,
                        Color = color,
                        Diameter = currentShape.Width,
                        RectHeight = currentShape.Height,
                        RectWidth = currentShape.Width,
                        Left = (double)currentShape.GetValue(Canvas.LeftProperty),
                        Top = (double)currentShape.GetValue(Canvas.TopProperty),
                        Type = (view.roiType.SelectedItem as ComboBoxItem).Content.ToString() == RoiShapeEnum.Circle.ToString() ? RoiShapeEnum.Circle.ToString() : (view.roiType.SelectedItem as ComboBoxItem).Content.ToString() == RoiShapeEnum.Rectangle.ToString() ? RoiShapeEnum.Rectangle.ToString() : RoiShapeEnum.Polygon.ToString(),
                    };
                    if (currentShape.GetType() == typeof(Polygon))
                    {
                        chn.Points = (currentShape as Polygon).Points.ToList();
                    }
                    InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs.Add(chn);
                    #endregion
                    //currentShape = null;
                    this.view.Cursor = Cursors.Arrow;
                    if (currentRectangle != null && !InperDeviceHelper.GetInstance().isSwitchToFrame)
                    {
                        currentRectangle.Visibility = Visibility.Visible;
                    }
                    view.addRoiBut.Visibility = Visibility.Visible;
                    view.addInperThemeRoiBut.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        private CameraChannel AddChannel(int channelId, string cName = "")
        {
            CameraChannel cameraChannel = new CameraChannel
            {
                Name = string.IsNullOrEmpty(cName) ? view.neuronName.Text.ToString() : cName,
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
                m.Ellipse(Center, new OpenCvSharp.Size(ellips_diam / 2, shape_height / 2), angle, 0, 360, Scalar.White, -1);
            }
            else if (ellipse.GetType() == typeof(Rectangle))
            {
                m.Rectangle(new OpenCvSharp.Rect(new OpenCvSharp.Point(rect_left, rect_top), new OpenCvSharp.Size(ellips_diam, shape_height)), Scalar.White, -1);
            }
            else
            {
                OpenCvSharp.Point[] ps = new OpenCvSharp.Point[(ellipse as Polygon).Points.Count];
                for (int i = 0; i < ps.Length; i++)
                {
                    ps[i] = new OpenCvSharp.Point((ellipse as Polygon).Points[i].X * scale, (ellipse as Polygon).Points[i].Y * scale);
                }

                m.FillPoly(new OpenCvSharp.Point[][] { ps }, Scalar.White);
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
                    if (!currentShape.Name.StartsWith("P"))
                    {
                        double currentLeft = Canvas.GetLeft(currentShape), currentTop = Canvas.GetTop(currentShape);
                        if (view.drawAreaCanvas.Width - currentLeft < Math.Abs(movePoint.Y - startPoint.Y))
                        {
                            return;
                        }
                        if (Math.Abs(movePoint.Y - startPoint.Y) > 5)
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
                    else
                    {
                        (currentShape as Polygon).Points.Add(movePoint);
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
                Width = view.drawAreaCanvas.Width / Math.Sqrt(view.zoomCoefficient.Value),
                Height = view.drawAreaCanvas.Height / Math.Sqrt(view.zoomCoefficient.Value),
                StrokeThickness = 1,
                Stroke = Brushes.Red,
                Fill = Brushes.Transparent
            };
            rectangle.MouseMove += Rectangle_MouseMove;
            rectangle.MouseLeftButtonDown += Rectangle_MouseDown;
            rectangle.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
            Canvas.SetLeft(rectangle, left == 0 ? (view.drawAreaCanvas.Width - rectangle.Width) / 2 : left);
            Canvas.SetTop(rectangle, top == 0 ? (view.drawAreaCanvas.Height - rectangle.Height) / 2 : top);
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
                        : currentX + moveX > view.drawAreaCanvas.Width - rect.Width - 1 ? view.drawAreaCanvas.Width - rect.Width - 1 : currentX + moveX;

                    double top = currentY + moveY <= 0
                        ? currentY
                        : currentY + moveY > view.drawAreaCanvas.Height - rect.Height - 1 ? view.drawAreaCanvas.Height - rect.Height - 1 : currentY + moveY;
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
        public void ZoomCoefficient_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (currentRectangle != null)
                {
                    currentRectangle.Width = view.drawAreaCanvas.Width / Math.Sqrt(e.NewValue);
                    currentRectangle.Height = view.drawAreaCanvas.Height / Math.Sqrt(e.NewValue);
                    double currentX = Canvas.GetLeft(currentRectangle);
                    double currentY = Canvas.GetTop(currentRectangle);

                    double left = currentRectangle.Width > view.drawAreaCanvas.Width - currentX ? view.drawAreaCanvas.Width - currentRectangle.Width - 2 :
                        currentX;

                    double top = currentRectangle.Height > view.drawAreaCanvas.Height - currentY ? view.drawAreaCanvas.Height - currentRectangle.Height - 2 :
                        currentY;
                    Canvas.SetLeft(currentRectangle, left);
                    Canvas.SetTop(currentRectangle, top);
                }
                CameraSetting.ZoomCoefficient = e.NewValue;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
            }
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
