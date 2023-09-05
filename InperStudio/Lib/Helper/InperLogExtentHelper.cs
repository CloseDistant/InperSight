using InperStudio.Lib.Bean;
using InperStudio.Lib.Data.Monitor;
using InperStudio.Lib.Enum;
using InperStudio.ViewModels;
using InperStudioControlLib.Lib.Config;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Collections;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InperStudio.Lib.Helper
{
    public class InperLogExtentHelper
    {
        private static string token = string.Empty;
        private static Device_Statu @device_Statu = null;
        private static Device_Use_Monitor @device_Use_Monitor = null;
        private static Inpersignal_module_use @inpersignal_module_Use = null;
        /// <summary>
        /// 获取页面html
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        public static string HttpGetPageHtml(string url, string encoding)
        {
            string pageHtml = string.Empty;
            try
            {
                using (WebClient MyWebClient = new WebClient())
                {
                    Encoding encode = Encoding.GetEncoding(encoding);
                    MyWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.84 Safari/537.36");
                    MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                    Byte[] pageData = MyWebClient.DownloadData(url); //从指定网站下载数据
                    pageHtml = encode.GetString(pageData);
                }
            }
            catch (Exception e)
            {

            }
            return pageHtml;
        }
        /// <summary>
        /// 从html中通过正则找到ip信息(只支持ipv4地址)
        /// </summary>
        /// <param name="pageHtml"></param>
        /// <returns></returns>
        public static string GetIPFromHtml(String pageHtml)
        {
            //验证ipv4地址
            string reg = @"(?:(?:(25[0-5])|(2[0-4]\d)|((1\d{2})|([1-9]?\d)))\.){3}(?:(25[0-5])|(2[0-4]\d)|((1\d{2})|([1-9]?\d)))";
            string ip = "";
            Match m = Regex.Match(pageHtml, reg);
            if (m.Success)
            {
                ip = m.Value;
            }
            return ip;
        }
        public static async void InitLogDatabase()
        {
            try
            {
                await Task.Run(() =>
                  {
                      if (InperGlobalClass.isNoNetwork || string.IsNullOrEmpty(InperDeviceHelper.Instance.device.PhotometryInfo.SN))
                      {
                          return;
                      }
                      var apires = HttpClientHelper.Get("Token/login?number=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString() + "&type=0");
                      if (apires != null && apires.Success)
                      {
                          token = apires.Data.ToString();

                          var api_res = HttpClientHelper.Get("DeviceStatu/getOne?snumber=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString(), token);
                          if (api_res != null)
                          {
                              if (api_res.Data != null)
                              {
                                  @device_Statu = JsonConvert.DeserializeObject<Device_Statu>(api_res.Data.ToString());
                              }
                              else
                              {
                                  @device_Statu = new Device_Statu();
                              }

                              var t2_html = HttpGetPageHtml("http://www.net.cn/static/customercare/yourip.asp", "gbk");
                              var t2_ip = GetIPFromHtml(t2_html);
                              @device_Statu.Ip = t2_ip;
                              //Task.Factory.StartNew(() =>
                              //{
                              //    bool istrue = false;
                              //    int count = 0;
                              //    while (!istrue)
                              //    {
                              //        count++;
                              //        GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
                              //        watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
                              //        GeoCoordinate coord = watcher.Position.Location;
                              //        Console.WriteLine("Latitude: " + coord.Latitude.ToString());
                              //        Console.WriteLine("Longitude: " + coord.Longitude.ToString());
                              //        if (coord.Latitude.ToString() != "NaN")
                              //        {
                              //            @device_Statu.Latitude = coord.Latitude.ToString();
                              //            @device_Statu.Longitude = coord.Longitude.ToString();
                              //            istrue = true;
                              //        }
                              //        Task.Delay(100);
                              //        if (count > 100)
                              //        {
                              //            istrue = true;
                              //        }
                              //    }
                              //});
                          }

                          var api_res_use = HttpClientHelper.Get("DeviceUseMonitor/getOne?snumber=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString(), token);
                          if (api_res_use != null)
                          {
                              if (api_res_use.Data != null && api_res_use.Data.ToString() != "null")
                              {
                                  @device_Use_Monitor = JsonConvert.DeserializeObject<Device_Use_Monitor>(api_res_use.Data.ToString());
                              }
                              else
                              {
                                  @device_Use_Monitor = new Device_Use_Monitor();
                              }
                          }
                          var api_res_module = HttpClientHelper.Get("InpersignalModuleUse/getOne?snumber=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString(), token);
                          if (api_res_module != null)
                          {
                              if (api_res_module.Data != null && api_res_module.Data.ToString() != "null")
                              {
                                  @inpersignal_module_Use = JsonConvert.DeserializeObject<Inpersignal_module_use>(api_res_module.Data.ToString());
                              }
                              else
                              {
                                  @inpersignal_module_Use = new Inpersignal_module_use();
                              }
                          }
                          DeviceStatuSet(0);
                      }
                  });
            }
            catch (Exception ex)
            {
                App.Log.Info(ex);
            }
        }
        public async static void LogExtent(Exception ex, string errorPage, [CallerLineNumber] int lineNumber = 0)
        {
            App.Log.Info(ex.ToString());
            try
            {
                await Task.Run(() =>
                {
                    if (InperGlobalClass.isNoNetwork)
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(token))
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>
                    {
                        { "createtime", DateTime.Now },
                        { "error_level",0},
                        { "error_message",ex.Message + "@页面" + errorPage + "@行号" + lineNumber+"@版本号："+InperConfig.Instance.Version},
                        { "snumber",InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString()},
                        { "is_dispose",1 },
                        { "type",0}
                    };
                        var apires = HttpClientHelper.Post("DeviceLog/insert", dict, token);
                        if (apires != null && apires.Code == 401)
                        {
                            var res = HttpClientHelper.Get("Token/login?number=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString());
                            if (res != null && res.Success && res.Data != null)
                            {
                                token = res.Data.ToString();
                                LogExtent(ex, errorPage, lineNumber);
                            }
                        }
                    }
                });
            }
            catch (Exception ex2)
            {
                App.Log.Info(ex2.ToString());
            }

        }
        public async static void DeviceStatuSet(int statu)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (InperGlobalClass.isNoNetwork)
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(token))
                    {
                        @device_Statu.Status = statu;
                        bool isFirstUpload = false;
                        if (string.IsNullOrEmpty(@device_Statu.Snumber))
                        {
                            isFirstUpload = true;
                            @device_Statu.Snumber = InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString();
                            @device_Statu.Createtime = DateTime.Now;
                            @device_Statu.Type = 0;
                        }
                        @device_Statu.Updatetime = DateTime.Now;
                        @device_Statu.Updatetime = DateTime.Now;
                        Dictionary<string, object> dict = new Dictionary<string, object>
                    {
                        { "snumber", @device_Statu.Snumber},
                        { "latitude", @device_Statu.Latitude},
                        { "longitude", @device_Statu.Longitude},
                        { "ip", @device_Statu.Ip},
                        { "type", @device_Statu.Type},
                        { "updatetime", @device_Statu.Updatetime},
                        { "createtime", @device_Statu.Createtime},
                        { "status", @device_Statu.Status},
                    };
                        if (!isFirstUpload)
                        {
                            dict.Add("id", @device_Statu.Id);
                        }
                        var apires = HttpClientHelper.Post("DeviceStatu/insert", dict, token);
                        if (apires != null && apires.Code == 401)
                        {
                            var res = HttpClientHelper.Get("Token/login?number=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString());
                            if (res != null && res.Success && res.Data != null)
                            {
                                token = res.Data.ToString();
                                DeviceStatuSet(statu);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogExtent(ex, "InperLogExtentHelper");
            }
        }
        public async static void DeviceUseMonitorRecodSet(double recordTime)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (InperGlobalClass.isNoNetwork)
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(token))
                    {
                        bool isFirstUpload = false;
                        if (string.IsNullOrEmpty(@device_Use_Monitor.Snumber))
                        {
                            isFirstUpload = true;
                            @device_Use_Monitor.Snumber = InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString();
                            @device_Use_Monitor.Createtime = DateTime.Now;
                        }
                        @device_Use_Monitor.Updatetime = DateTime.Now;
                        @device_Use_Monitor.Last_Record_Time = DateTime.Now;
                        @device_Use_Monitor.Total_Record_Time += recordTime;
                        Dictionary<string, object> dict = new Dictionary<string, object>
                    {
                        { "snumber", @device_Use_Monitor.Snumber},
                        { "updatetime", @device_Use_Monitor.Updatetime},
                        { "createtime", @device_Use_Monitor.Createtime},
                        { "type", @device_Use_Monitor.Type},
                        { "last_open_time", @device_Use_Monitor.Last_Open_Time},
                        { "last_record_time", @device_Use_Monitor.Last_Record_Time},
                        { "total_open_time", @device_Use_Monitor.Total_Open_Time},
                        { "total_record_time", @device_Use_Monitor.Total_Record_Time},
                        { "record_count", ++@device_Use_Monitor.Record_Count},
                        { "open_count", @device_Use_Monitor.Open_Count},
                    };
                        if (!isFirstUpload)
                        {
                            dict.Add("id", @device_Use_Monitor.Id);
                        }
                        var apires = HttpClientHelper.Post("DeviceUseMonitor/insert", dict, token);
                        if (apires != null && apires.Code == 401)
                        {
                            var res = HttpClientHelper.Get("Token/login?number=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString());
                            if (res != null && res.Success && res.Data != null)
                            {
                                token = res.Data.ToString();
                                DeviceUseMonitorRecodSet(recordTime);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogExtent(ex, "InperLogExtentHelper");
            }
        }
        public async static void DeviceUseMonitorOpenCountSet()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (InperGlobalClass.isNoNetwork)
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(token))
                    {
                        bool isFirstUpload = false;
                        if (string.IsNullOrEmpty(@device_Use_Monitor.Snumber))
                        {
                            isFirstUpload = true;
                            @device_Use_Monitor.Snumber = InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString();
                            @device_Use_Monitor.Createtime = DateTime.Now;
                        }
                        @device_Use_Monitor.Updatetime = DateTime.Now;
                        @device_Use_Monitor.Type = 0;
                        @device_Use_Monitor.Last_Open_Time = DateTime.Now;
                        Dictionary<string, object> dict = new Dictionary<string, object>
                        {
                            { "snumber", @device_Use_Monitor.Snumber},
                            { "updatetime", @device_Use_Monitor.Updatetime},
                            { "createtime", @device_Use_Monitor.Createtime},
                            { "type", @device_Use_Monitor.Type},
                            { "last_open_time", @device_Use_Monitor.Last_Open_Time},
                            { "last_record_time", @device_Use_Monitor.Last_Record_Time},
                            { "total_open_time", @device_Use_Monitor.Total_Open_Time},
                            { "total_record_time", @device_Use_Monitor.Total_Record_Time},
                            { "record_count", @device_Use_Monitor.Record_Count},
                            { "open_count", ++@device_Use_Monitor.Open_Count},
                        };
                        if (!isFirstUpload)
                        {
                            dict.Add("id", @device_Use_Monitor.Id);
                        }
                        var apires = HttpClientHelper.Post("DeviceUseMonitor/insert", dict, token);
                        if (apires != null && apires.Code == 401)
                        {
                            var res = HttpClientHelper.Get("Token/login?number=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString());
                            if (res != null && res.Success && res.Data != null)
                            {
                                token = res.Data.ToString();
                                DeviceUseMonitorOpenCountSet();
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogExtent(ex, "InperLogExtentHelper");
            }
        }
        /// <summary>
        /// 模块使用
        /// </summary>
        public async static void DeviceModuleUseCountSet()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (InperGlobalClass.isNoNetwork)
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(token))
                    {
                        bool isFirstUpload = false;
                        if (string.IsNullOrEmpty(@inpersignal_module_Use.Snumber))
                        {
                            isFirstUpload = true;
                            @inpersignal_module_Use.Snumber = InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString();
                            @inpersignal_module_Use.Createtime = DateTime.Now;
                        }
                        @inpersignal_module_Use.Updatetime = DateTime.Now;
                        if (InperGlobalClass.AdditionRecordConditionsStart != AdditionRecordConditionsTypeEnum.Immediately || InperGlobalClass.AdditionRecordConditionsStop != AdditionRecordConditionsTypeEnum.Immediately)
                        {
                            inpersignal_module_Use.Trigger_count += 1;
                        }
                        if (InperGlobalClass.ActiveVideos.Count > 0)
                        {
                            inpersignal_module_Use.Video_count += 1;
                        }
                        if (InperGlobalClass.EventSettings.Channels.Count(x => x.Type == ChannelTypeEnum.Output.ToString()) > 0)
                        {
                            inpersignal_module_Use.Output_count += 1;
                        }
                        if (InperGlobalClass.EventSettings.Channels.Count(x => x.Type != ChannelTypeEnum.Output.ToString()) > 0)
                        {
                            inpersignal_module_Use.Marker_count += 1;
                        }
                        if (InperGlobalClass.StimulusSettings.IsConfigSweep)
                        {
                            inpersignal_module_Use.Stimulus_count += 1;
                        }
                        if (NoteSettingViewModel.NotesCache.Count > 0)
                        {
                            inpersignal_module_Use.Note_count += 1;
                        }
                        if (InperGlobalClass.CameraSignalSettings.CameraChannels.Count(x => x.Type == ChannelTypeEnum.Analog.ToString()) > 0)
                        {
                            inpersignal_module_Use.Analog_count += 1;
                        }
                        Dictionary<string, object> dict = new Dictionary<string, object>
                        {
                            { "snumber", @inpersignal_module_Use.Snumber},
                            { "updatetime", @inpersignal_module_Use.Updatetime},
                            { "createtime", @inpersignal_module_Use.Createtime},
                            { "analog_count", @inpersignal_module_Use.Analog_count},
                            { "marker_count", @inpersignal_module_Use.Marker_count},
                            { "output_count", @inpersignal_module_Use.Output_count},
                            { "stimulus_count", @inpersignal_module_Use.Stimulus_count},
                            { "trigger_count", @inpersignal_module_Use.Trigger_count},
                            { "note_count", @inpersignal_module_Use.Note_count},
                            { "video_count", @inpersignal_module_Use.Video_count}
                        };
                        if (!isFirstUpload)
                        {
                            dict.Add("id", @inpersignal_module_Use.Id);
                        }
                        var apires = HttpClientHelper.Post("InpersignalModuleUse/insert", dict, token);
                        if (apires != null && apires.Code == 401)
                        {
                            var res = HttpClientHelper.Get("Token/login?number=" + InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString());
                            if (res != null && res.Success && res.Data != null)
                            {
                                token = res.Data.ToString();
                                DeviceModuleUseCountSet();
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogExtent(ex, "InperLogExtentHelper");
            }
        }
    }
}
