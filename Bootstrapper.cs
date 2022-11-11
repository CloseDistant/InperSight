using InperSight.Lib.Bean.Data.Model;
using InperSight.Lib.Config;
using InperSight.ViewModels;
using InperStudioControlLib.Lib.Config;
using MySql.Data.MySqlClient;
using SqlSugar;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace InperSight
{
    public class Bootstrapper : Bootstrapper<StartPageViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
            //ulong
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
            try
            {
                SqlSugarClient db = new(new ConnectionConfig()
                {
                    ConnectionString = InperConfig.Instance.ConStr + MySqlSslMode.Disabled,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                });
                List<Sight_Version> list = db.Queryable<Sight_Version>().OrderBy(x => x.Id, OrderByType.Asc).ToList();
                if (list != null && list.Count() > 0)
                {
                    Sight_Version ver = list.FirstOrDefault(x => x.Version_Number == InperConfig.Instance.Version);
                    if (ver != null)
                    {
                        Sight_Version _new = list.FirstOrDefault(x => x.Id > ver.Id);

                        if (_new != null)
                        {
                            if (!InperConfig.Instance.IsSkip)
                            {
                                string content = InperConfig.Instance.Version + "," + Environment.CurrentDirectory + "/" + "," + Path.Combine(Environment.CurrentDirectory, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString() + ".exe");
                                if (content.Split(',').Length == 3)
                                {
                                    _ = Process.Start(Environment.CurrentDirectory + @"\UpgradeClientCore.exe", content);
                                    Environment.Exit(0);
                                }
                            }
                        }
                    }
                    InperConfig.Instance.ReleaseData = list.Last().UploadTime.ToString("yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(list.Last().Desc))
                    {
                        InperConfig.Instance.VersionDesc = string.Empty;
                        if (list.Last().Desc.Contains('@'))
                        {
                            list.Last().Desc.Split('@').ToList().ForEach(desc =>
                            {
                                InperConfig.Instance.VersionDesc += desc + Environment.NewLine;
                            });
                        }
                        else
                        {
                            InperConfig.Instance.VersionDesc = list.Last().Desc;
                        }
                    }
                }
                InperConfig.Instance.IsSkip = false;
                db.Close();
                db.Dispose();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message.ToString());
            }
        }
    }
}
