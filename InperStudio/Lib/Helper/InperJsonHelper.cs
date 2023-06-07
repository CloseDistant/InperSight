using HandyControl.Controls;
using InperStudio.Lib.Helper.JsonBean;
using InperStudioControlLib.Lib.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace InperStudio.Lib.Helper
{
    public class InperJsonHelper
    {
        public static AdditionRecordConditions GetAdditionRecordJson(string type = "start")
        {
            return GetResByT<AdditionRecordConditions>(type) ?? default;
        }
        public static void SetAdditionRecodConditions(AdditionRecordConditions recordConditions, string type = "start")
        {
            SetResByT(recordConditions, type);
        }
        public static EventPanelProperties GetEventPanelProperties(string type = "eventPanelProperties")
        {
            return GetResByT<EventPanelProperties>(type) ?? default;
        }
        public static void SetEventPanelProperties(EventPanelProperties eventPanelProperties, string type = "eventPanelProperties")
        {
            SetResByT(eventPanelProperties, type);
        }
        public static CameraSignalSettings GetCameraSignalSettings(string type = "cameraSignalSettings")
        {
            return GetResByT<CameraSignalSettings>(type) ?? default;
        }
        public static void SetCameraSignalSettings(CameraSignalSettings cameraSignalSettings, string type = "cameraSignalSettings")
        {
            SetResByT(cameraSignalSettings, type);
        }
        public static EventSettings GetEventSettings(string type = "eventSettings")
        {
            return GetResByT<EventSettings>(type) ?? default;
        }
        public static void SetEventSettings(EventSettings eventSettings, string type = "eventSettings")
        {
            SetResByT(eventSettings, type);
        }
        public static StimulusSettings GetStimulusSettings(string type = "stimulusSettings")
        {
            return GetResByT<StimulusSettings>(type) ?? default;
        }
        public static void SetStimulusSettings(StimulusSettings stimulusSettings, string type = "stimulusSettings")
        {
            SetResByT(stimulusSettings, type);
        }
        public static string GetDataPathSetting(string type = "dataPath")
        {
            return GetResByT<string>(type) ?? default;
        }
        public static void SetDataPathSetting(string dataPath, string type = "dataPath")
        {
            SetResByT<string>(dataPath, type);
        }
        public static string GetDisplaySetting(string type)
        {
            return GetResByT<string>(type) ?? "true";
        }
        public static void SetDisplaySetting(string value, string tyep)
        {
            SetResByT<string>(value, tyep);
        }
        private static T GetResByT<T>(string type)
        {
            try
            {
                JObject res = InperJsonConfig.Instance.Readjson();
                return JsonConvert.DeserializeObject<T>(res[type].ToString());
            }
            catch (Exception e)
            {
                InperLogExtentHelper.LogExtent(e, "InperJsonHelper");
            }
            return default;
        }
        private static void SetResByT<T>(T t, string type)
        {
            try
            {
                if (t != null)
                {
                    string res = JsonConvert.SerializeObject(t);
                    JObject jo = InperJsonConfig.Instance.Readjson();
                    jo[type] = res;

                    InperJsonConfig.Instance.Writejson(jo);
                }
                else
                {
                    App.Log.Error("配置文件记录失败：" + t);
                }
            }
            catch (Exception e)
            {
                InperLogExtentHelper.LogExtent(e, "InperJsonHelper");
            }
        }
    }
}
