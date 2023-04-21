using InperStudio.Lib.Bean;
using InperStudio.Lib.Helper;
using InperStudio.ViewModels;
using InperStudioControlLib.Lib.Config;
using MySql.Data.MySqlClient;
using SciChart.Core.Extensions;
using SqlSugar;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InperStudio
{
    public class Bootstrapper : Bootstrapper<StartPageViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
        }
        //After-excitation
        protected async override void Configure()
        {
            try
            {
                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "UnderBinBackup")))
                {
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "UnderBinBackup"));
                }
                DirectoryInfo root = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "UnderBin"));
                FileInfo[] files = root.GetFiles();
                if (files.Length > 0)
                {
                    InperClassHelper.DelectDir(Path.Combine(Environment.CurrentDirectory, "UnderBinBackup"));
                    Directory.Delete(Path.Combine(Environment.CurrentDirectory, "UnderBinBackup"));
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "UnderBinBackup"));

                    File.Copy(files.Last().FullName, Path.Combine(Environment.CurrentDirectory, "UnderBinBackup", files.Last().Name), true);
                    files.ForEachDo(x =>
                    {
                        File.Delete(x.FullName);
                    });
                }
                await Task.Run(() =>
                {
                    SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = InperConfig.Instance.ConStr + MySqlSslMode.None,//连接符字串
                        DbType = DbType.MySql,
                        IsAutoCloseConnection = true //不设成true要手动close
                    });
                    List<Tb_Version> list = db.Queryable<Tb_Version>().OrderBy(x => x.Id, OrderByType.Asc).ToList();

                    if (list != null && list.Count() > 0)
                    {
                        InperGlobalClass.latestVersion = list.Last().Version_Number;
                        Tb_Version ver = list.FirstOrDefault(x => x.Version_Number == InperConfig.Instance.Version);
                        if (ver != null)
                        {
                            Tb_Version _new = list.FirstOrDefault(x => x.Id > ver.Id);

                            if (_new != null)
                            {
                                if (!InperConfig.Instance.IsSkip)
                                {
                                    string content = InperConfig.Instance.Version + "," + Environment.CurrentDirectory + "/" + "," + Path.Combine(Environment.CurrentDirectory, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString() + ".exe");
                                    if (content.Split(',').Length == 3)
                                    {
                                        _ = Process.Start(Environment.CurrentDirectory + @"\UpgradeClient.exe", content);
                                        Environment.Exit(0);
                                    }
                                }
                            }
                        }
                        InperConfig.Instance.ReleaseData = list.Last(x => x.Version_Number == InperConfig.Instance.Version).UploadTime.ToString("yyyy-MM-dd");
                        if (!string.IsNullOrEmpty(list.Last(x => x.Version_Number == InperConfig.Instance.Version).Desc))
                        {
                            InperConfig.Instance.VersionDesc = string.Empty;
                            if (list.Last(x => x.Version_Number == InperConfig.Instance.Version).Desc.Contains('@'))
                            {
                                list.Last(x => x.Version_Number == InperConfig.Instance.Version).Desc.Split('@').ToList().ForEach(desc =>
                                {
                                    InperConfig.Instance.VersionDesc += desc + Environment.NewLine;
                                });
                            }
                            else
                            {
                                InperConfig.Instance.VersionDesc = list.Last(x => x.Version_Number == InperConfig.Instance.Version).Desc;
                            }
                        }
                    }
                    else
                    {

                    }
                    InperConfig.Instance.IsSkip = false;
                    db.Close();
                    db.Dispose();
                });
            }
            catch (Exception ex)
            {
                InperGlobalClass.isNoNetwork = true;
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
            // Perform any other configuration before the application starts
        }
    }
}
