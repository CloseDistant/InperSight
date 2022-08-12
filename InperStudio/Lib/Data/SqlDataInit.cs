﻿using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using Newtonsoft.Json;
using SciChart.Charting.Model.ChartSeries;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InperStudio.Lib.Data
{
    public class SqlDataInit
    {
        public readonly SqlSugarScope sqlSugar;
        public Dictionary<string, string> RecordTablePairs = new Dictionary<string, string>();
        public SqlDataInit(string dataName = "data.db")
        {
            if (RecordTablePairs.Count > 0)
            {
                RecordTablePairs = new Dictionary<string, string>();
            }

            string dataPath = Path.Combine(InperGlobalClass.DataPath, InperGlobalClass.DataFolderName);

            string filePath = Path.Combine(dataPath, dataName);

            sqlSugar = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = @"DataSource=" + filePath,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true//自动释放
            },
            sqlSugar =>
            {
                sqlSugar.Aop.OnError = (exp) =>
                {
                    App.Log.Error(exp);
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
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.TablesDesc));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.ColumnDesc));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.Config));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.Marker));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.OutputMarker));
            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Model.ChannelConfig));
            sqlSugar.CodeFirst.InitTables(typeof(Model.IntervalScale));
            RecordInit();
        }
        private void RecordInit()
        {
            RecordTablePairs.Clear();
            bool _isFirst = true;

            foreach (var item in InperDeviceHelper.Instance.CameraChannels)
            {
                if (item.Type.Equals(ChannelTypeEnum.Camera.ToString()))
                {
                    item.LightModes.ForEach(x =>
                    {
                        ChannelConfig channelConfig = new ChannelConfig()
                        {
                            ChannelName = item.Name,
                            ChannelId = item.ChannelId,
                            LightId = x.LightType,
                            Type = ChannelTypeEnum.Camera.ToString(),
                            CreateTime = DateTime.Now,
                            Color = x.WaveColor.ToString(),
                            LightName = InperGlobalClass.CameraSignalSettings.LightMode.First(l => l.GroupId == x.LightType).WaveType
                        };
                        sqlSugar.Insertable(channelConfig).ExecuteCommand();
                    });

                    if (_isFirst)
                    {
                        item.RenderableSeries.ToList().ForEach(x =>
                        {
                            int id = int.Parse((x as LineRenderableSeriesViewModel).Tag.ToString());

                            sqlSugar.MappingTables.Add(nameof(AllChannelRecord), nameof(AllChannelRecord) + id);
                            sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(AllChannelRecord));

                            TablesDesc desc = new TablesDesc()
                            {
                                TableName = nameof(AllChannelRecord) + id,
                                TableType = item.Type == SignalSettingsTypeEnum.Camera.ToString() ? 0 : 1,
                                Desc = "0:Camera 通道数据表",//"0:Camera 通道数据表,1:Analog 通道数据表",
                                CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
                            };
                            _ = sqlSugar.Insertable(desc).ExecuteCommand();
                        });
                        _isFirst = false;
                    }
                }
                else if (item.Type.Equals(ChannelTypeEnum.Analog.ToString()))
                {
                    ChannelConfig channelConfig = new ChannelConfig()
                    {
                        ChannelName = item.Name,
                        ChannelId = item.ChannelId,
                        LightId = -1,
                        LightName = "",
                        Type = ChannelTypeEnum.Analog.ToString(),
                        CreateTime = DateTime.Now,
                        Color = InperGlobalClass.CameraSignalSettings.CameraChannels.First(ai => ai.Type == ChannelTypeEnum.Analog.ToString() && ai.ChannelId == item.ChannelId).Color
                    };
                    sqlSugar.Insertable(channelConfig).ExecuteCommand();

                    sqlSugar.MappingTables.Add(nameof(AnalogRecord), nameof(AnalogRecord) + item.ChannelId);
                    sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(AnalogRecord));
                    RecordTablePairs.Add(item.Type + item.ChannelId, nameof(AnalogRecord) + item.ChannelId);

                    TablesDesc desc = new TablesDesc()
                    {
                        TableName = nameof(AnalogRecord) + item.ChannelId,
                        TableType = item.Type == SignalSettingsTypeEnum.Camera.ToString() ? 0 : 1,
                        Desc = "1:Analog 通道数据表",//"0:Camera 通道数据表,1:Analog 通道数据表",
                        CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
                    };
                    _ = sqlSugar.Insertable(desc).ExecuteCommand();
                }
            }
            foreach (LineRenderableSeriesViewModel item in InperDeviceHelper.Instance.EventChannelChart.RenderableSeries)
            {
                sqlSugar.MappingTables.Add(nameof(Input), nameof(Input) + item.Tag);
                sqlSugar.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Input));
                TablesDesc desc = new TablesDesc()
                {
                    TableName = nameof(Input) + item.Tag,
                    TableType = 101,
                    Desc = "101:Input 输入电信号数据表",
                    CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
                };
                _ = sqlSugar.Insertable(desc).ExecuteCommand();
            }

            ColumnDesc desc1 = new ColumnDesc()
            {
                TableName = "ChannelRecord",
                ColumnName = "Type",
                ColumnType = 0,
                Desc = "0 410激发光,1 470激发光,2 561激发光,-1 代表Input 电信号",
                CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
            };
            _ = sqlSugar.Insertable(desc1).ExecuteCommand();

            List<Config> configs = new List<Config>();
            configs.Add(new Config()
            {
                JsonText = JsonConvert.SerializeObject(InperGlobalClass.AdditionRecordConditionsStart),
                PropertyName = nameof(InperGlobalClass.AdditionRecordConditionsStart),
                CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
            });
            configs.Add(new Config()
            {
                JsonText = JsonConvert.SerializeObject(InperGlobalClass.AdditionRecordConditionsStop),
                PropertyName = nameof(InperGlobalClass.AdditionRecordConditionsStop),
                CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
            });
            configs.Add(new Config()
            {
                JsonText = JsonConvert.SerializeObject(InperGlobalClass.CameraSignalSettings),
                PropertyName = nameof(InperGlobalClass.CameraSignalSettings),
                CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
            });
            configs.Add(new Config()
            {
                JsonText = JsonConvert.SerializeObject(InperGlobalClass.EventPanelProperties),
                PropertyName = nameof(InperGlobalClass.EventPanelProperties),
                CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
            });
            configs.Add(new Config()
            {
                JsonText = JsonConvert.SerializeObject(InperGlobalClass.EventSettings),
                PropertyName = nameof(InperGlobalClass.EventSettings),
                CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
            });

            _ = sqlSugar.Insertable(configs).ExecuteCommand();
        }
    }
}
