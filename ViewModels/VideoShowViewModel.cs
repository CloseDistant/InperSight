using HandyControl.Controls;
using HandyControl.Data;
using InperSight.Lib.Bean;
using InperSight.Lib.Helper;
using InperSight.Views;
using InperSight.Views.Control;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using InperSight.Lib.Config;
using System.IO;

namespace InperSight.ViewModels
{
    public class VideoShowViewModel : Screen
    {
        #region properties
        private VideoShowView view;
        private VideoDeviceHelper kit;
        private VideoUserControl bottomControl;

        public VideoDeviceHelper BehaviorRecorderKit { get => kit; set => SetAndNotify(ref kit, value); }
        #endregion
        public VideoShowViewModel(VideoDeviceHelper behaviorRecorderKit)
        {
            BehaviorRecorderKit = behaviorRecorderKit;
            BehaviorRecorderKit.StartCapture();
        }
        protected override void OnViewLoaded()
        {
            try
            {
                view = View as VideoShowView;
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
                        LoggerHelper.Error(ex.ToString());
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
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #region 加锁解锁
        public void LockEvent()
        {
            this.view.Topmost = true;
            view.unLock.Visibility = Visibility.Visible;
            view._lock.Visibility = Visibility.Collapsed;
        }
        public void UnLockEvent()
        {
            this.view.Topmost = false;
            view.unLock.Visibility = Visibility.Collapsed;
            view._lock.Visibility = Visibility.Visible;
        }
        #endregion
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
