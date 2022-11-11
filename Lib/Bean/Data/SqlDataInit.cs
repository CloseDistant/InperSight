using InperSight.Lib.Bean.Data.Model;
using InperSight.Lib.Config;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Bean.Data
{
    public class SqlDataInit
    {
        private readonly SqlSugarScope Scope;
        public SqlDataInit(string dataName = "data.db")
        {
            string dataPath = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName);
            string filePath = Path.Combine(dataPath, dataName);
            Scope = new(new ConnectionConfig()
            {
                ConnectionString = @"DataSource=" + filePath,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
            },
            Scope =>
            {
                Scope.Aop.OnError = (exp) =>
                {
                    LoggerHelper.Error(exp.Message);
                };
            });

            bool res = Scope.DbMaintenance.CreateDatabase(filePath);
            if (!res)
            {
                InperGlobalFunc.ShowRemainder("数据文件初始化失败", 2);
                return;
            }
            Scope.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(ChannelRecord));
            Scope.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(ChannelConfig));

            List<ChannelConfig> configs = new();
            foreach (var item in InperGlobalClass.CameraSettingJsonBean.CameraChannelConfigs)
            {
                ChannelConfig config = new()
                {
                    ChannelId = item.ChannelId,
                    ChannelName = item.Name,
                    Color = item.Color,
                    CreateTime = DateTime.Now,
                    Type = item.Type,
                    Diameter = item.Diameter,
                    RectHeight = item.RectHeight,
                    RectWidth = item.RectWidth,
                };
                configs.Add(config);
            }
            Scope.Insertable(configs).ExecuteCommand();
        }

        public void ChannelRecordSave(List<ChannelRecord> records)
        {
            try
            {
                Scope.Insertable(records).ExecuteCommand();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
            }
        }
        public void NoteSave(List<Note> notes)
        {
            try
            {
                Scope.Insertable(notes).ExecuteCommand();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
            }
        }
    }
}
