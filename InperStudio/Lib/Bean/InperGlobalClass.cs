using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using Stylet;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace InperStudio.Lib.Bean
{
    public class InperGlobalClass
    {
        #region field
        private static bool isRecord = false;
        private static bool isPreview = false;
        private static bool isStop = true;
        private static bool isExistEvent = false;
        private static bool isAllowDragScroll = false;
        private static string dataPath = string.Empty;
        private static string dataFolderName = string.Empty;
        private static DateTime runTime;

        private static EventPanelProperties eventPanelProperties = InperJsonHelper.GetEventPanelProperties();
        private static CameraSignalSettings cameraSignalSettings = InperJsonHelper.GetCameraSignalSettings();
        private static EventSettings eventSettings = InperJsonHelper.GetEventSettings();
        private static StimulusSettings stimulusSettings = InperJsonHelper.GetStimulusSettings() ?? new StimulusSettings();

        private static AdditionRecordConditionsTypeEnum additionRecordConditionsStart = AdditionRecordConditionsTypeEnum.Immediately;
        private static AdditionRecordConditionsTypeEnum additionRecordConditionsStop = AdditionRecordConditionsTypeEnum.Immediately;

        private static ObservableCollection<VideoRecordBean> activeVideos = new ObservableCollection<VideoRecordBean>();
        private static BindableCollection<EventChannelJson> manualEvents = new BindableCollection<EventChannelJson>();
        #endregion

        #region properties
        public static bool IsImportConfig { get; set; } = false;
        public static DateTime RunTime
        {
            get => runTime;
            set
            {
                runTime = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(RunTime)));
            }
        }
        public static bool IsAllowDragScroll
        {
            get => isAllowDragScroll;
            set
            {
                isAllowDragScroll = true;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsAllowDragScroll)));
            }
        }
        public static bool IsExistEvent
        {
            get => isExistEvent;
            set
            {
                isExistEvent = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsExistEvent)));
            }
        }
        public static bool IsRecord
        {
            get => isRecord;
            set
            {
                isRecord = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsRecord)));
            }
        }
        public static bool IsPreview
        {
            get => isPreview;
            set
            {
                isPreview = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsPreview)));
            }
        }
        public static bool IsStop
        {
            get => isStop;
            set
            {
                isStop = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsStop)));
            }
        }
        public static string DataPath
        {
            get => dataPath;
            set
            {
                dataPath = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DataPath)));
            }
        }
        public static string DataFolderName
        {
            get => dataFolderName;
            set
            {
                dataFolderName = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DataFolderName)));
            }
        }
        public static EventPanelProperties EventPanelProperties
        {
            get => eventPanelProperties;
            set
            {
                eventPanelProperties = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(EventPanelProperties)));
            }
        }
        public static CameraSignalSettings CameraSignalSettings
        {
            get => cameraSignalSettings;
            set
            {
                cameraSignalSettings = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CameraSignalSettings)));
            }
        }
        public static EventSettings EventSettings
        {
            get => eventSettings;
            set
            {
                eventSettings = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(EventSettings)));
            }
        }
        public static StimulusSettings StimulusSettings
        {
            get => stimulusSettings;
            set
            {
                stimulusSettings = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(StimulusSettings)));
            }
        }
        public static AdditionRecordConditionsTypeEnum AdditionRecordConditionsStart
        {
            get => additionRecordConditionsStart;
            set
            {
                additionRecordConditionsStart = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(AdditionRecordConditionsStart)));
            }
        }
        public static AdditionRecordConditionsTypeEnum AdditionRecordConditionsStop
        {
            get => additionRecordConditionsStop;
            set
            {
                additionRecordConditionsStop = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(AdditionRecordConditionsStop)));
            }
        }
        public static ObservableCollection<VideoRecordBean> ActiveVideos
        {
            get => activeVideos;
            set
            {
                activeVideos = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ActiveVideos)));
            }
        }
        public static BindableCollection<EventChannelJson> ManualEvents
        {
            get => manualEvents;
            set
            {
                manualEvents = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ManualEvents)));
            }
        }
        #endregion
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static void ShowReminderInfo(string message, int waitTime = 1)
        {
            Growl.Warning(new GrowlInfo() { Message = message, Token = "SuccessMsg", WaitTime = waitTime });
        }
        public static void SetSampling(double sampling)
        {
            InperDeviceHelper.Instance.device.SetFrameRate(sampling * (CameraSignalSettings.LightMode.Count < 1 ? 1 : CameraSignalSettings.LightMode.Count));
            CameraSignalSettings.Sampling = sampling;
        }
    }
}
