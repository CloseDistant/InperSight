using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
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
        private string version;
        private string mcu_version;
        private string conStr;
        private bool isSkip;
        private string upgradeKey;
        private string mcuKey;
        private string releaseData;
        private string versionDesc;
        private string themeColor;
        private string language;
        public string Language
        {
            get => language;
            set
            {
                language = value;
                SetConfigValue("language", language);
            }
        }
        public string ThemeColor
        {
            get => themeColor;
            set
            {
                themeColor = value;
                SetConfigValue("themeColor", themeColor);
            }
        }
        public string ReleaseData
        {
            get => releaseData;
            set
            {
                releaseData = value;
                SetConfigValue("releaseData", releaseData.ToString());
            }
        }
        public string VersionDesc
        {
            get => versionDesc;
            set
            {
                versionDesc = value;
                SetConfigValue("versionDesc", versionDesc.ToString());
            }
        }
        public bool IsSkip
        {
            get => isSkip;
            set
            {
                isSkip = value;
                SetConfigValue("isSkip", isSkip.ToString());
            }
        }
        public string ConStr
        {
            get => conStr;
            set
            {
                conStr = value;
                SetConfigValue("conStr", conStr);
            }
        }
        public string UpgradeKey
        {
            get => upgradeKey;
            set
            {
                upgradeKey = value;
                SetConfigValue("upgradeKey", upgradeKey);
            }
        }
        public string McuKey
        {
            get => mcuKey;
            set
            {
                mcuKey = value;
                SetConfigValue("mcuKey", mcuKey);
            }
        }
        public string Version
        {
            get => version;
            set
            {
                version = value;
                SetConfigValue("version", version);
            }
        }
        public string Mcu_Version
        {
            get => mcu_version;
            set
            {
                mcu_version = value;
                SetConfigValue("version", mcu_version);
            }
        }
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
            version = GetConfigValue("version");
            conStr = Helper.EnAndDecryption.Decrypt(GetConfigValue("conStr"));
            isSkip = bool.Parse(GetConfigValue("isSkip"));
            upgradeKey = GetConfigValue("upgradeKey");
            mcu_version = GetConfigValue("mcu_version");
            mcuKey = GetConfigValue("mcuKey");
            releaseData = GetConfigValue("releaseData");
            versionDesc = GetConfigValue("versionDesc");
            themeColor = GetConfigValue("themeColor");
            language = GetConfigValue("language");
        }
        #endregion

        #region xml
        public static string GetConfigValue(string sKey, string sDefault = null)
        {
            try
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
            catch (Exception ex)
            {

            }
            return default;
        }
        public static void SetConfigValue(string sKey, string sValue)
        {
            string sectionName = "appSettings";
            SetConfigValue(sKey, sValue, sectionName);

        }
        public static void SetConfigValue(string sKey, string sValue, string sectionName)
        {
            try
            {
                string xmlPath = string.IsNullOrEmpty(path) ? Environment.CurrentDirectory + @"/Config/InperStudio.exe.config" : path;
                XDocument xdoc = XDocument.Load(xmlPath);
                XElement xElement = xdoc.Element("configuration").Element(sectionName).Elements("add").FirstOrDefault(x => x.Attribute("key").Value == sKey);
                xElement.SetAttributeValue("value", sValue);
                xdoc.Save(xmlPath);
                ConfigurationManager.RefreshSection(sectionName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        #endregion
    }
}
