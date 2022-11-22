using InperSight.Lib.Config.Json;
using InperSight.Lib.Helper;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Bean
{
    /// <summary>
    /// 状态机制
    /// </summary>
    public class InperGlobalClass
    {
        #region field
        private static bool isRecord = false;
        private static bool isPreview = false;
        private static bool isStop = true;
        private static string dataPath = string.Empty;
        private static string dataFolderName = string.Empty;
        private static bool isExistEvent = false;
        private static bool isAllowDragScroll = false;
        private static ObservableCollection<VideoDeviceHelper> activeVideos = new();
        private static CameraSettingJsonBean cameraSettingJsonBean = new();
        #endregion

        #region properties
        public static bool IsImportConfig { get; set; } = false;
        public static bool IsExistEvent
        {
            get => isExistEvent;
            set
            {
                isExistEvent = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsExistEvent)));
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
        public static CameraSettingJsonBean CameraSettingJsonBean
        {
            get => cameraSettingJsonBean;
            set => cameraSettingJsonBean = value;
        }
        public static ObservableCollection<VideoDeviceHelper> ActiveVideos
        {
            get => activeVideos;
            set
            {
                activeVideos = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ActiveVideos)));
            }
        }
        #endregion
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
    }
}
