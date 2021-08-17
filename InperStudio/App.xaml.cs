using InperStudio.Lib.Bean;
using InperStudio.Lib.Data;
using InperStudio.Lib.Helper;
using InperStudioControlLib.Lib.Config;
using InperStudioControlLib.Lib.Helper;
using log4net;
using SciChart.Charting.Visuals;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace InperStudio
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static SqlDataInit SqlDataInit;
        protected override void OnStartup(StartupEventArgs e)
        {
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("RYLnOXgjEruY2Nh2EmZx86QLNsGKveD+J1b1iVWLO/mjGdIVZKUyaBJdgcEa9nqdJwNoEMA6Y9b3ltO3v3TqSVlvFRdm8W1FPibVBc7QmCjDO6jljcyZUV8STM8SFkdVuNKQNwKFlhDba8OY9fuPQBSMY/V0atNGBTc4cscDyOyofJxxPjZbUenA+PkmFAE+tHD4m0VU2dkUqFKYGkf6czcfLOCrIwcnxFJKjHr/+qLSTgDwF3Mh4k34YP7g0MxGBdlllTJTDHKBruQ1ZiBYPOZptgAnNM9VjLi1DWoOrjIzNaHfA+16tcLmdRWunHdPPQU/f+uCOaNjfcHPASoAuNWRoDd1f9xZw8ZdGWJXJ9XtSLjCzod+cTQP02fxJABATC/V9y0RqudDwdUCyGduKX0ESOz6JqHTCR7qysmdy+xWgg27t31PUeXT0WuGhwcEARl5o2UM4eVemwnCmErwX6mS6ac9Pv2gWKRxuiUkgE0UxOf9O/cVxfJ+IcwlQP+BxlPF6uMnUSfjSTu5GCohkSHANUzWcVJSPjNvwjDi2xhr/mDfSIV4yn33wjG7/aJBf2cXuKgIhyA6RzId2FNB2ZstwGD1zPqVT3A=");
            #region 配置文件初始化

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string appConfigDir = Path.Combine(Environment.CurrentDirectory, "Config");
            if (!Directory.Exists(appConfigDir))
            {
                _ = Directory.CreateDirectory(appConfigDir);
            }
            string configPath = Path.Combine(appConfigDir, "ProductConfig.config");
            string configPathexe = Path.Combine(appConfigDir, assemblyName + ".exe.config");
            string jsonPath = Path.Combine(appConfigDir, "UserConfig.json");
            if (!File.Exists(configPathexe))
            {
                File.Copy(Environment.CurrentDirectory + @"\" + assemblyName + ".exe.config", configPathexe);
            }
            if (!File.Exists(configPath))
            {
                File.Copy(Environment.CurrentDirectory + @"\ProductConfig.config", configPath);
            }
            //if (!File.Exists(jsonPath))
            {
                File.Copy(Environment.CurrentDirectory + @"\UserConfig.json", jsonPath, true);
            }

            RegisterEvents();
            #endregion
            InperGlobalClass.DataPath = InperGlobalClass.DataPath == string.Empty ? Environment.CurrentDirectory + @"\Data\" : InperGlobalClass.DataPath;
            InperGlobalClass.DataFolderName = InperGlobalClass.DataFolderName == string.Empty ? DateTime.Now.ToString("yyyyMMddHHmmss") : InperGlobalClass.DataFolderName;

            SystemSleepHelper.PreventSleep(true);

            SqlDataInit= new SqlDataInit();

            base.OnStartup(e);
        }
        private void RegisterEvents()
        {
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Error(args.Exception.Message);
                args.SetObserved();
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Info(args.ExceptionObject.ToString());
        }
    }
}
