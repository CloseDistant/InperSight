using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Helper;
using SciChart.Charting.Model.ChartSeries;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data
{
    public class SqlDataInit
    {
        public readonly SqlSugarScope sqlSugar;
        public SqlDataInit(string dataName = "data.db")
        {
            string dataPath = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName);
            if (!Directory.Exists(dataPath))
            {
                _ = Directory.CreateDirectory(dataPath);
            }
            string filePath = Path.Combine(dataPath, dataName);

            sqlSugar = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = @"DataSource=" + filePath,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true//自动释放
            },
            sqlSugar =>
            {
                sqlSugar.Aop.OnLogExecuting = (s, p) =>
                {
                    App.Log.Error(s);
                };
            });

            bool res = sqlSugar.DbMaintenance.CreateDatabase(filePath);

            if (!res)
            {
                Growl.Error(new GrowlInfo() { Message = "数据文件初始化失败", Token = "SuccessMsg", WaitTime = 1 });
                return;
            }
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.Note));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.Environment));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.Tag));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.Output));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.TablesDesc));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.ColumnDesc));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.Config));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.AIROI));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.Manual));
            foreach (var item in InperDeviceHelper.Instance.CameraChannels)
            {
                sqlSugar.MappingTables.Add(nameof(ChannelRecord), nameof(ChannelRecord) + item.ChannelId);
                sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(ChannelRecord));
            }
            foreach(LineRenderableSeriesViewModel item in InperDeviceHelper.Instance.EventChannelChart.RenderableSeries)
            {
                sqlSugar.MappingTables.Add(nameof(Input), nameof(Input) + item.Tag);
                sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Input));
            }
        }
    }
}
