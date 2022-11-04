using InperStudioControlLib.Lib.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Config.Json
{
    public class JsonHelper
    {
 
        public static CameraSettingJsonBean GetCameraSetting(string type = "CameraSetting")
        {
            return GetResByT<CameraSettingJsonBean>(type) ?? default;
        }
        public static void SetCameraSetting(CameraSettingJsonBean cameraSignalSettings, string type = "CameraSetting")
        {
            SetResByT(cameraSignalSettings, type);
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
                LoggerHelper.Error("配置文件记录失败：" + t);
            }
        }
    }
}
