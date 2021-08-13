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
    public class Bootstrapper : Bootstrapper<MainWindowViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "server=120.26.65.180;port=3306;Database=InperUpgrade;Uid=root;Pwd=Inper2021;SslMode=" + MySqlSslMode.None,//连接符字串
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
                        MessageBoxResult res = MessageBox.Show("发现新版本 " + list.Last().Version_Number + "，是否更新", "版本升级", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (res.Equals(MessageBoxResult.Yes))
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
    }
}
