using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Chart;
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
        }
        protected override void OnViewLoaded()
        {
            try
            {
                view = View as VideoWindowView;
                view.InperCustomDialogBottm = bottomControl = new VideoUserControl();

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
                            string tempFile = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmssffff") + ".bmp");
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
                bottomControl.record.Click += (s, e) =>
                {
                    BehaviorRecorderKit.AutoRecord = false;
                    bottomControl.no_record.Visibility = System.Windows.Visibility.Visible;
                    bottomControl.record.Visibility = System.Windows.Visibility.Collapsed;
                };
                bottomControl.no_record.Click += (s, e) =>
                {
                    BehaviorRecorderKit.AutoRecord = true;
                    bottomControl.no_record.Visibility = System.Windows.Visibility.Collapsed;
                    bottomControl.record.Visibility = System.Windows.Visibility.Visible;
                };
                this.view.Owner = Application.Current.MainWindow;

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
                                    Width = condition.ShapeWidth,
                                    Height = condition.ShapeHeight,
                                    //Name = condition.ZoneName,
                                    StrokeThickness = 1,
                                    Fill = System.Windows.Media.Brushes.Transparent,
                                    Stroke = System.Windows.Media.Brushes.Black
                                };
                                Canvas.SetLeft(rect, condition.ShapeLeft);
                                Canvas.SetTop(rect, condition.ShapeTop);
                                view.drawCanvas.Children.Add(rect);
                                var layer = AdornerLayer.GetAdornerLayer(rect);
                                var color = System.Windows.Media.Brushes.Black;
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
            string text = "This will stop video recording,are you sure to close?";
            if (InperConfig.Instance.Language != "en_us")
            {
                text = "这将会使视频停止录制，确定要关闭吗？";
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
            BehaviorRecorderKit.StopPreview();
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
