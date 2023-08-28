using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Chart;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudio.Views.Control;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InperStudio.ViewModels
{
    public class VideoWindowViewModel : Screen
    {
        #region properties
        private VideoWindowView view;
        private VideoRecordBean kit;
        private VideoUserControl bottomControl;

        public VideoRecordBean BehaviorRecorderKit { get => kit; set => SetAndNotify(ref kit, value); }
        #endregion
        public VideoWindowViewModel(VideoRecordBean behaviorRecorderKit)
        {
            BehaviorRecorderKit = behaviorRecorderKit;
            BehaviorRecorderKit.StartCapture();
            BehaviorRecorderKit.ContainsMouseZone += BehaviorRecorderKit_ContainsMouseZone1;
        }

        private void BehaviorRecorderKit_ContainsMouseZone1(object sender, Lib.Data.Model.Tracking e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var point = new System.Windows.Point(e.CenterX * (view.image.ActualWidth / BehaviorRecorderKit.WriteableBitmap.Width), e.CenterY * (view.image.ActualHeight / BehaviorRecorderKit.WriteableBitmap.Height));
                foreach (var item in view.drawCanvas.Children)
                {
                    var shape = (item as Shape);
                    var left = Canvas.GetLeft(shape);
                    var top = Canvas.GetTop(shape);
                    Rect rect = new Rect(left, top, shape.Width, shape.Height);
                    if (rect.Contains(point))
                    {
                        shape.Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2270D95F"));
                    }
                    else
                    {
                        shape.Fill = System.Windows.Media.Brushes.Transparent;
                    }
                }
            }));
        }
        protected override void OnViewLoaded()
        {
            try
            {
                view = View as VideoWindowView;
                view.InperCustomDialogBottm = bottomControl = new VideoUserControl();
                _startSize = view.image.DesiredSize;

                bottomControl.Screen.Click += (s, e) =>
                {
                    try
                    {
                        this.view.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DrawingVisual drawingVisual = new DrawingVisual();
                            using (DrawingContext context = drawingVisual.RenderOpen())
                            {
                                VisualBrush brush = new VisualBrush(view.image) { Stretch = Stretch.None };
                                context.DrawRectangle(brush, null, new Rect(0, 0, view.image.ActualWidth, view.image.ActualHeight));
                                context.Close();
                            }
                            RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)view.image.ActualWidth, (int)view.image.ActualHeight, 96d, 96d, PixelFormats.Default);
                            targetBitmap.Render(drawingVisual);
                            PngBitmapEncoder saveEncoder = new PngBitmapEncoder();
                            saveEncoder.Frames.Add(BitmapFrame.Create(targetBitmap));
                            string tempFile = System.IO.Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmssffff") + ".bmp");
                            System.IO.FileStream fs = System.IO.File.Open(tempFile, System.IO.FileMode.OpenOrCreate);
                            saveEncoder.Save(fs);
                            fs.Close();

                            Growl.Info(new GrowlInfo() { Message = "Snapshot saved successfully.", Token = "SuccessMsg", WaitTime = 1 });
                        }));
                    }
                    catch (Exception ex)
                    {
                        InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
                    }
                };

                if (InperClassHelper.GetWindowByNameChar("inper") is System.Windows.Window window)
                {
                    this.view.Owner = window;
                }

                if (InperGlobalClass.EventSettings.Channels.Count > 0)
                {
                    InperGlobalClass.EventSettings.Channels.ForEach(channel =>
                    {
                        if (channel.Type == ChannelTypeEnum.Zone.ToString() && channel.VideoZone.Name == BehaviorRecorderKit.Name)
                        {
                            channel.VideoZone.AllZoneConditions.ForEach(condition =>
                            {
                                var rect = new System.Windows.Shapes.Rectangle()
                                {
                                    Width = condition.ShapeWidth * _startSize.Width / _previewWidth,
                                    Height = condition.ShapeHeight * _startSize.Height / _previewHeight,
                                    //Name = condition.ZoneName,
                                    StrokeThickness = 1,
                                    Fill = System.Windows.Media.Brushes.Transparent,
                                    Stroke = System.Windows.Media.Brushes.Yellow
                                };
                                Canvas.SetLeft(rect, condition.ShapeLeft * _startSize.Width / _previewWidth);
                                Canvas.SetTop(rect, condition.ShapeTop * _startSize.Height / _previewHeight);
                                view.drawCanvas.Children.Add(rect);
                                var layer = AdornerLayer.GetAdornerLayer(rect);
                                var color = System.Windows.Media.Brushes.Yellow;
                                layer.Add(new InperAdorner(rect, condition.ZoneName, color, 0, -15, false));
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
            finally
            {
                BehaviorRecorderKit.ActHeight = view.image.ActualHeight;
                BehaviorRecorderKit.ActWidth = view.image.ActualWidth;

                view.image.SizeChanged += View_SizeChanged;
            }
        }

        private void View_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BehaviorRecorderKit.ActHeight = e.NewSize.Height;
            BehaviorRecorderKit.ActWidth = e.NewSize.Width;

        }

        System.Windows.Size _startSize = new System.Windows.Size(0, 0);
        double _previewWidth = 480, _previewHeight = 320;
        public void Image_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            try
            {

                if (view.drawCanvas.Children.Count > 0)
                {
                    foreach (var child in view.drawCanvas.Children)
                    {
                        var rect = child as Shape;

                        var left = Canvas.GetLeft(rect);
                        var top = Canvas.GetTop(rect);

                        var width = rect.Width;
                        var height = rect.Height;
                        var scaleW = e.NewSize.Width / e.PreviousSize.Width;
                        var scaleH = e.NewSize.Height / e.PreviousSize.Height;

                        rect.Width = width * scaleW;
                        rect.Height = height * scaleH;

                        Canvas.SetLeft(rect, left * scaleW);
                        Canvas.SetTop(rect, top * scaleH);
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }

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

        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string text = string.Empty;
            if (InperGlobalClass.IsRecord)
            {
                text = "This will stop video recording,are you sure to close it?";
                if (InperConfig.Instance.Language != "en_us")
                {
                    text = "这将会使视频停止录制，确定要关闭吗？";
                }
            }
            else
            {
                text = "The video will not be recorded while experiment running, are you sure to close it?";
                if (InperConfig.Instance.Language != "en_us")
                {
                    text = "后续采集时将不会同步录制视频，确定要关闭吗？";
                }
            }
            InperDialogWindow inperDialogWindow = new InperDialogWindow(text);
            inperDialogWindow.HideCancleButton();
            inperDialogWindow.ClickEvent += (s, statu) =>
            {
                if (statu == 1)
                {
                    e.Cancel = true;
                    return;
                }
            };
            inperDialogWindow.ShowDialog();
        }
        protected override void OnClose()
        {
            BehaviorRecorderKit.Stop();
            //BehaviorRecorderKit.StopRecording();
            BehaviorRecorderKit.IsActive = false;
            foreach (var item in InperGlobalClass.ActiveVideos)
            {
                if (item.Name.Equals(BehaviorRecorderKit.Name))
                {
                    item.IsActive = false;
                }
            }
        }
    }
}
