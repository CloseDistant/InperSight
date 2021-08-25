﻿using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Views;
using InperStudio.Views.Control;
using OpenCvSharp.Extensions;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InperStudio.ViewModels
{
    public class VideoWindowViewModel : Screen
    {
        #region properties
        private VideoWindowView view;
        private BehaviorRecorderKit kit;
        private VideoUserControl bottomControl;
        private bool isLoop = true;
        public BehaviorRecorderKit BehaviorRecorderKit { get => kit; set => SetAndNotify(ref kit, value); }
        #endregion
        public VideoWindowViewModel(BehaviorRecorderKit behaviorRecorderKit)
        {
            BehaviorRecorderKit = behaviorRecorderKit;
        }
        protected override void OnViewLoaded()
        {
            try
            {


                view = View as VideoWindowView;
                view.InperCustomDialogBottm = bottomControl = new VideoUserControl();

                view.Dispatcher.Invoke(() =>
                {
                    BehaviorRecorderKit.StartPreview();
                    //BehaviorRecorderKit.StartRecord(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss")));
                });


                bottomControl.FramerateCombox.SelectionChanged += (s, e) =>
                {
                    double frame = double.Parse((bottomControl.FramerateCombox.SelectedValue as ComboBoxItem).Content.ToString());
                    BehaviorRecorderKit.Device.Fps = frame;
                    _ = BehaviorRecorderKit.Device.Set(OpenCvSharp.VideoCaptureProperties.Fps, frame);
                };

                bottomControl.Screen.Click += (s, e) =>
                {
                    System.Drawing.Bitmap bit = BehaviorRecorderKit._PreviewMat.Clone().ToBitmap();
                    string path = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss") + ".bmp");
                    bit.Save(path);
                    Growl.Info(new GrowlInfo() { Message = "保存成功", Token = "SuccessMsg", WaitTime = 1 });
                };
                _ = Task.Factory.StartNew(() =>
                  {
                      while (isLoop)
                      {
                          if (InperGlobalClass.IsRecord)
                          {
                              view.Dispatcher.Invoke(() =>
                              {
                                  if ((bool)bottomControl.IsRecord.IsChecked)
                                  {
                                      BehaviorRecorderKit.StartRecord(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss") + "_" + BehaviorRecorderKit._CamIndex));
                                  }
                              });
                              isLoop = false;
                          }
                      }
                  });
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        protected override void OnClose()
        {
            BehaviorRecorderKit.StopPreview();
            BehaviorRecorderKit.StopRecord();
            BehaviorRecorderKit.Dispose();
            BehaviorRecorderKit.IsActive = false;
            GC.Collect(0);
        }
    }
}
