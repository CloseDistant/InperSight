using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InperStudioControlLib.Lib.Config
{
    public class InperConfig
    {
        #region
        public static string path = string.Empty;
        private static InperConfig config;
        static readonly object _lock = new object();

        private string dataPath;
        private string dataCustomDirectoryPath;
        private string configPath;

        public string DataPath
        {
            get => dataPath;
            set
            {
                dataPath = value.ToString();
                SetConfigValue("dataPath", dataPath);
            }
        }
        public string DataCustomDirectoryPath
        {
            get => dataCustomDirectoryPath;
            set
            {
                dataCustomDirectoryPath = value.ToString();
                SetConfigValue("dataCustomDirectoryPath", dataCustomDirectoryPath);
            }
        }
        public string ConfigPath
        {
            get => configPath;
            set
            {
                configPath = value.ToString();
                SetConfigValue("configPath", dataPath);
            }
        }
        #endregion
        public static InperConfig Instance
        {
            get
            {
                if (config == null)
                {
                    lock (_lock)
                    {
                        if (config == null)
                        {
                            config = new InperConfig();
                        }
                    }
                }
                return config;
            }
        }
        private InperConfig()
        {
            Load();
        }

        #region 加载配置信息
        private void Load()
        {
            dataPath = GetConfigValue("dataPath");
            dataCustomDirectoryPath = GetConfigValue("dataCustomDirectoryPath");
            configPath = GetConfigValue("configPath");
        }
        #endregion

        #region xml
        public static string GetConfigValue(string sKey, string sDefault = null)
        {
            string xmlPath = string.IsNullOrEmpty(path) ? Environment.CurrentDirectory + @"/Config/InperStudio.exe.config" : path;
            XDocument xdoc = XDocument.Load(xmlPath);
            XElement xe = xdoc.Element("configuration").Element("appSettings").Elements("add").FirstOrDefault(x => x.Attribute("key").Value == sKey);
            if (xe != null)
            {
                return xe.Attribute("value").Value;
            }
            return sDefault;
        }
        public static void SetConfigValue(string sKey, string sValue)
        {
            string sectionName = "appSettings";
            SetConfigValue(sKey, sValue, sectionName);

        }
        public static void SetConfigValue(string sKey, string sValue, string sectionName)
        {
            string xmlPath = string.IsNullOrEmpty(path) ? Environment.CurrentDirectory + @"/Config/InperStudio.exe.config" : path;
            XDocument xdoc = XDocument.Load(xmlPath);
            XElement xElement = xdoc.Element("configuration").Element(sectionName).Elements("add").FirstOrDefault(x => x.Attribute("key").Value == sKey);
            xElement.SetAttributeValue("value", sValue);
            xdoc.Save(xmlPath);
            ConfigurationManager.RefreshSection(sectionName);
        }
        #endregion
    }
}
