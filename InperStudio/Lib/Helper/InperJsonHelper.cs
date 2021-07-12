using InperStudio.Lib.Helper.JsonBean;
using InperStudioControlLib.Lib.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private static T GetResByT<T>(string type)
        {
            JObject res = InperJsonConfig.Instance.Readjson();
            return JsonConvert.DeserializeObject<T>(res[type].ToString());
        }
        private static void SetResByT<T>(T t, string type)
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
    }
}
