using InperStudio.Lib.Bean;
using InperStudio.ViewModels;
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
using System.Windows;

namespace InperStudio
{
    public class Bootstrapper : Bootstrapper<StartPageViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
        }

        protected override void Configure()
        {
            try
            {
                SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = InperConfig.Instance.ConStr + MySqlSslMode.None,//连接符字串
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true //不设成true要手动close
                });

                List<Tb_Version> list = db.Queryable<Tb_Version>().OrderBy(x => x.Id, OrderByType.Asc).ToList();

                if (list.Count() > 0)
                {
                    Tb_Version ver = list.FirstOrDefault(x => x.Version_Number == InperConfig.Instance.Version);
                    if (ver != null)
                    {
                        Tb_Version _new = list.FirstOrDefault(x => x.Id > ver.Id);

                        if (_new != null)
                        {
                            if (!InperConfig.Instance.IsSkip)
                            {
                                string content = InperConfig.Instance.Version + "," + Environment.CurrentDirectory + "/" + "," + Path.Combine(Environment.CurrentDirectory, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString() + ".exe");
                                _ = Process.Start(Environment.CurrentDirectory + @"\UpgradeClient.exe", content);
                                Environment.Exit(0);
                            }

                        }
                    }
                }
                db.Close();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            // Perform any other configuration before the application starts
        }
    }
}
