using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Bean
{
    public class InperGlobalClass
    {
        #region field
        private static bool isRecord = false;
        private static bool isPreview = false;
        private static bool isStop = false;
        private static string dataPath = string.Empty;
        private static string dataFolderName = string.Empty;

        private static EventPanelProperties eventPanelProperties = InperJsonHelper.GetEventPanelProperties();
        private static CameraSignalSettings cameraSignalSettings = InperJsonHelper.GetCameraSignalSettings();
        private static EventSettings eventSettings = InperJsonHelper.GetEventSettings();

        private static AdditionRecordConditionsTypeEnum additionRecordConditionsStart = AdditionRecordConditionsTypeEnum.Immediately;
        private static AdditionRecordConditionsTypeEnum additionRecordConditionsStop = AdditionRecordConditionsTypeEnum.Immediately;
        #endregion

        #region properties
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
        #endregion

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
    }
}
