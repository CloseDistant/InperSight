using InperSight.Lib.Bean;
using InperSight.Lib.Bean.Data;
using InperStudioControlLib.Lib.Config;
using SciChart.Charting.Visuals;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace InperSight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SqlDataInit SqlDataInit;
        protected override void OnStartup(StartupEventArgs e)
        {

            SciChartSurface.SetRuntimeLicenseKey("xbmE9J8upY33gaal1KJRjc+Px0DrKE4Ydfu4SBMil7heQqRRHv/XIlLp8KGDoVkgwnnKLiIRw6/1wzujyVA+762R+1t/n95xBrf3D6BBbXq5oaOKTXnfQRppYvoGby8UBJYXpDyj0ziHGifPIjQaMKThITN/PXH3bAD1YdYi3KX62Vw+pkBH4T/vkJZ2YTqBPMkZ/xrzYOt2mEXndbhziQsJ2tgpveSKyTB+IpJgKBLWpaV2zNHfxutTYoQtEPatQ8PnAxiYgj5fRAUkhYDuI8vesbd/+uzBvxclWQcHl2+WOhFlTWNsRPGLQUSCG8beKQVmNB5q7G5JJ2/GBPS8SmOBq238Z6nGvpATqwXPGCnOsE1jPsmxwsfflY0/FRXKuFtCH5ecpm4Hx9c9o3XLqSsyFGkHSFRWKfiz01AXOo3OXYOI90TPeZs8yFRPT9WA/ta/S+zBhh3YGctZH4PLEBDlXT5g/bR0MMmSgJw4KCCl52Daeh+bQZIBzm6Sn23r+Q3P9HzBRj+3MebIspKxu66ipQfGa6vSv/I+niITz7qkrc1tj0V7+31egcr0u7pBlpGwKKRKpcfTFJmyGW5ijtVhOIqp218uSX2aA9GyatAig6lLmEfBEBUutvilNFBCDGu3H4J+");

            #region 文件和配置文件初始化
            InperGlobalClass.DataPath = InperGlobalClass.DataPath == string.Empty ? Environment.CurrentDirectory + @"\Data\" : InperGlobalClass.DataPath;
            InperGlobalClass.DataFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (!Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
            {
                _ = Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
            }
            //配置文件生成
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string appConfigDir = Path.Combine(Environment.CurrentDirectory, "Config");
            if (!Directory.Exists(appConfigDir))
            {
                _ = Directory.CreateDirectory(appConfigDir);
            }
            string jsonPath = Path.Combine(appConfigDir, "UserConfig.json");
            //if (!File.Exists(jsonPath))
            {
                File.Copy(Environment.CurrentDirectory + @"\UserConfig.json", jsonPath, true);
            }
            string configPathexe = Path.Combine(appConfigDir, assemblyName + ".exe.config");
            if (!File.Exists(configPathexe))
            {
                File.Copy(Environment.CurrentDirectory + @"\" + assemblyName + ".dll.config", configPathexe);
            }
            InperConfig.path = Environment.CurrentDirectory + @"/Config/" + assemblyName + ".exe.config";

            #endregion
            base.OnStartup(e);
        }
    }
}
