using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace InperStudioControlLib.Lib.Config
{
    public class InperJsonConfig
    {
        public static string filepath = Path.Combine(Environment.CurrentDirectory, "Config", "UserConfig.json");
        private static InperJsonConfig config;
        static readonly object _lock = new object();

        public static InperJsonConfig Instance
        {
            get
            {
                if (config == null)
                {
                    lock (_lock)
                    {
                        if (config == null)
                        {
                            config = new InperJsonConfig();
                        }
                    }
                }
                return config;
            }
        }
        /// <summary>
        /// 读取JSON文件
        /// </summary>
        /// <param name="key">JSON文件中的key值</param>
        /// <returns>JSON文件中的value值</returns>
        public JObject Readjson(string jsonfile = null)
        {
            if (jsonfile == null)
            {
                jsonfile = filepath;
            }
            using (System.IO.StreamReader file = System.IO.File.OpenText(jsonfile))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject jObject = (JObject)JToken.ReadFrom(reader);
                    return jObject;
                }
            }
        }
        public void Writejson(JObject jObject, string jsonfile = null)
        {
            if (jsonfile == null)
            {
                jsonfile = filepath;
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(jsonfile))
            {
                file.Write(jObject.ToString());
            }
        }
    }
}
