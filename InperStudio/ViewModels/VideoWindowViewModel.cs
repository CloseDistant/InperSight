﻿using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Views;
using InperStudio.Views.Control;
using Stylet;
using System;
using System.IO;
using System.Windows;
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
                        App.Log.Error(ex.ToString());
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
                App.Log.Error(ex.ToString());
            }
        }

        protected override void OnClose()
        {
            BehaviorRecorderKit.StopPreview();
            BehaviorRecorderKit.StopRecording(1);
            BehaviorRecorderKit.IsActive = false;
            foreach(var item in InperGlobalClass.ActiveVideos)
            {
                if (item.Name.Equals(BehaviorRecorderKit.Name))
                {
                    item.IsActive = false;
                }
            }
        }
    }
}
