using InperStudio.Lib.Bean.Channel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace InperStudio.Lib.Helper
{
    public enum DisplayEnum
    {
        Analog,
        Trigger,
        Note,
        Sprit
    }
    public class InperProductConfig
    {
        public static bool DisplayNodeRead(DisplayEnum displayEnum)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("Config/ProductConfig.config");
                XmlNode analogNode = xmlDoc.SelectSingleNode("/Root/Display/" + displayEnum.ToString());
                bool isAnalogEnabled = bool.Parse(analogNode.InnerText);
                return isAnalogEnabled;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, "InperProductConfig");
                return false;
            }
        }
        public static void DisplayNodeWrite(DisplayEnum displayEnum, bool display)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("Config/ProductConfig.config");
                XmlNode analogNode = xmlDoc.SelectSingleNode("/Root/Display/" + displayEnum.ToString());
                // 修改配置文件
                analogNode.InnerText = display.ToString();
                xmlDoc.Save("Config/ProductConfig.config");
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, "InperProductConfig");
            }
        }
    }
}
