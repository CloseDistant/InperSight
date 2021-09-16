using HandyControl.Controls;
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
                    System.Drawing.Bitmap bit = BehaviorRecorderKit._capturedFrame.Clone().ToBitmap();
                    string path = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName, DateTime.Now.ToString("HHmmss") + ".bmp");
                    bit.Save(path);
                    Growl.Info(new GrowlInfo() { Message = "保存成功", Token = "SuccessMsg", WaitTime = 1 });
                };
                bottomControl.record.Click += (s, e) =>
                {
                    BehaviorRecorderKit.AutoRecord = true;
                    bottomControl.no_record.Visibility = System.Windows.Visibility.Visible;
                    bottomControl.record.Visibility = System.Windows.Visibility.Collapsed;
                };
                bottomControl.no_record.Click += (s, e) =>
                {
                    BehaviorRecorderKit.AutoRecord = false;
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
            BehaviorRecorderKit.StopRecording();
            BehaviorRecorderKit.Dispose();
            BehaviorRecorderKit.IsActive = false;
            GC.Collect(0);
        }
    }
}
