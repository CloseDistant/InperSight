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

            SciChartSurface.SetRuntimeLicenseKey("bW86ckZuwaRUV5flBvrRQ/d/E+8F5cAU4WuffAeR1Hoc6bnQ1D0QfhTxcuhcL83LIrirIZDRkStMSa7a6Q72fj/8yMGyRuSb5lLyFKD0q0joheyuPIsLUDbekAzRIIz1HQZ1/zfBDVODHyLPJILv01T51bJIBV6Mebs03g093JDtFUbIZ+FEPp01D4Lb7/Prfx+TY6B+IKjTy/fl39oAo2MppBifiT+xZi+8KFBavLvh5JPLAd+zC/aUXLvTMC3eNWPIqpktyBX7IMcxGLRWqIVyqJyU8f+wGD6e9SkWMDUiF075N7UvzEIXb7WIkftN9aWLixXAvS4uJG6TsCN18eTFkpFGss+b5c54/FFgy3aH72CIFmp8KhD49CMO0bsnGWjEm5pevIHz2lC30JcuHCvwkMnURQn2yLbG81kxJB/yB3xFamqSWXo9WxYDT+Oidu9GkwxHOWnycl2UqDy4+P7CVn5NGdqT2gRK1z4Ar3QeA4TAVmjTBROpDPcRhu2ucoRn3oQUzhdrehcOBGTVCuh71Kpiq9QVMUyb4NVtfvJjg+grIsLMySyUbXgCGddCKklI+B9Du/AL1xZAX/JwKCODR+2tbTwACKeo");
            //SciChartSurface.SetRuntimeLicenseKey("RYLnOXgjEruY2Nh2EmZx86QLNsGKveD+J1b1iVWLO/mjGdIVZKUyaBJdgcEa9nqdJwNoEMA6Y9b3ltO3v3TqSVlvFRdm8W1FPibVBc7QmCjDO6jljcyZUV8STM8SFkdVuNKQNwKFlhDba8OY9fuPQBSMY/V0atNGBTc4cscDyOyofJxxPjZbUenA+PkmFAE+tHD4m0VU2dkUqFKYGkf6czcfLOCrIwcnxFJKjHr/+qLSTgDwF3Mh4k34YP7g0MxGBdlllTJTDHKBruQ1ZiBYPOZptgAnNM9VjLi1DWoOrjIzNaHfA+16tcLmdRWunHdPPQU/f+uCOaNjfcHPASoAuNWRoDd1f9xZw8ZdGWJXJ9XtSLjCzod+cTQP02fxJABATC/V9y0RqudDwdUCyGduKX0ESOz6JqHTCR7qysmdy+xWgg27t31PUeXT0WuGhwcEARl5o2UM4eVemwnCmErwX6mS6ac9Pv2gWKRxuiUkgE0UxOf9O/cVxfJ+IcwlQP+BxlPF6uMnUSfjSTu5GCohkSHANUzWcVJSPjNvwjDi2xhr/mDfSIV4yn33wjG7/aJBf2cXuKgIhyA6RzId2FNB2ZstwGD1zPqVT3A=");

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
