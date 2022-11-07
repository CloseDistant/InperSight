using InperSight.Lib.Bean;
using SciChart.Charting.Visuals;
using System;
using System.IO;
using System.Windows;

namespace InperSight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //SciChartSurface.SetRuntimeLicenseKey("zhLOc5v67LQB5LLFP8qBcoUmu3x3s3LKyZPYniGuJ9RYbnIoa3kguM1Qze9Fg7jLoVHmXIgRtVt+3iJ7kRZFU1i4fyHp1OEb5HUk3qT6nWyFE2py20H622G4oVehIj2nrJ9hJ9E6AqOAFgOTDO9Ma/eqieJnZhEjfLJ9nXF0bBvAqqWfK9NBTfnCIZ1NwXLncVaVE0S/om9Nt3lDhucieK2Tl4rKiI7YLT+dIY6B3HvUBXenxdnSGgTtng8kGUuBkkTOU4CPasv/LOsu3zImAivThcmfIQ8jEunYQwtlY0bKP7WX2XR2RDW/Umj/AZLYVHUOX92e67wl4E+//VN++3ImCcYz8PnsCq61SioyXpB5ZelfU3xhHzhT9szXIt6HO8nMnwkmjTpSgok15lcvbOfuU0a9+iIjyl3fl2B1vX/l2u7tXzkkgzO/04r0bc11vxoARlXH3oDm8lAgae/2SNZHnRG4uJw7rKC+OFpchq89bsg31fbuS4x0NkRcsROARYteYLgYgnRofjBOS/L49SxBNm06anCc3r+4PpkTKM9A2xpcSMszAqHjeQox8NiPxftdicBrsONfGxprVRgB2yNJusgpuCjJyY+Pq60pKTnuyMbFv9MDax2yYQKQ");
            SciChartSurface.SetRuntimeLicenseKey("RYLnOXgjEruY2Nh2EmZx86QLNsGKveD+J1b1iVWLO/mjGdIVZKUyaBJdgcEa9nqdJwNoEMA6Y9b3ltO3v3TqSVlvFRdm8W1FPibVBc7QmCjDO6jljcyZUV8STM8SFkdVuNKQNwKFlhDba8OY9fuPQBSMY/V0atNGBTc4cscDyOyofJxxPjZbUenA+PkmFAE+tHD4m0VU2dkUqFKYGkf6czcfLOCrIwcnxFJKjHr/+qLSTgDwF3Mh4k34YP7g0MxGBdlllTJTDHKBruQ1ZiBYPOZptgAnNM9VjLi1DWoOrjIzNaHfA+16tcLmdRWunHdPPQU/f+uCOaNjfcHPASoAuNWRoDd1f9xZw8ZdGWJXJ9XtSLjCzod+cTQP02fxJABATC/V9y0RqudDwdUCyGduKX0ESOz6JqHTCR7qysmdy+xWgg27t31PUeXT0WuGhwcEARl5o2UM4eVemwnCmErwX6mS6ac9Pv2gWKRxuiUkgE0UxOf9O/cVxfJ+IcwlQP+BxlPF6uMnUSfjSTu5GCohkSHANUzWcVJSPjNvwjDi2xhr/mDfSIV4yn33wjG7/aJBf2cXuKgIhyA6RzId2FNB2ZstwGD1zPqVT3A=");
            base.OnStartup(e);

            #region 文件和配置文件初始化
            InperGlobalClass.DataPath = InperGlobalClass.DataPath == string.Empty ? Environment.CurrentDirectory + @"\Data\" : InperGlobalClass.DataPath;
            InperGlobalClass.DataFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (!Directory.Exists(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName)))
            {
                _ = Directory.CreateDirectory(Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName));
            }
            //配置文件生成
            //string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
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
            #endregion
        }
    }
}
