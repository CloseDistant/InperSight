using InperStudio.Lib.Bean;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using Stylet;
using System;

namespace InperStudio.ViewModels
{
    public class EventPanelPropertiesViewModel : Screen
    {
        private DataShowControlView dataShowControlView;
        private EventPanelPropertiesView view;
        public EventPanelPropertiesViewModel(DataShowControlView dataShowControlView)
        {
            this.dataShowControlView = dataShowControlView;
        }
        protected override void OnViewLoaded()
        {
            try
            {
                view = View as EventPanelPropertiesView;

                view.heightAuto.Checked += (s, e) =>
                {
                    if (dataShowControlView.dataList.Items.Count != 0)
                    {
                        dataShowControlView.relativeBottom.Height = dataShowControlView.fixedBottom.Height = (dataShowControlView.fixedBottom.Height + dataShowControlView.dataList.ActualHeight) / (dataShowControlView.dataList.Items.Count == 1 ? 2 : dataShowControlView.dataList.Items.Count);
                        InperGlobalClass.EventPanelProperties.HeightAuto = true;
                        InperGlobalClass.EventPanelProperties.HeightFixed = false;
                    }
                    else
                    {
                        dataShowControlView.relativeBottom.Height = dataShowControlView.fixedBottom.Height = 150;
                    }
                };
                view.heightFixed.Checked += (s, e) =>
                {
                    dataShowControlView.relativeBottom.Height = dataShowControlView.fixedBottom.Height = InperGlobalClass.EventPanelProperties.HeightFixedValue;
                    InperGlobalClass.EventPanelProperties.HeightAuto = false;
                    InperGlobalClass.EventPanelProperties.HeightFixed = true;
                };
                view.fixedValue.TextChanged += (s, e) =>
                {
                    double value = 0;
                    if (double.TryParse(view.fixedValue.Text, out value))
                    {
                        //value = value < 2 ? 30 : (value >= 999 ? 999 : value);
                        view.fixedValue.Text = value.ToString();
                        dataShowControlView.relativeBottom.Height = dataShowControlView.fixedBottom.Height = value;
                    }
                };

                view.ConfirmClickEvent += (s, e) =>
                {
                    double value = 0;
                    if ((bool)view.heightFixed.IsChecked)
                    {
                        if (double.TryParse(view.fixedValue.Text, out value))
                        {
                            if (value < 30 || value > 999)
                            {
                                InperGlobalClass.ShowReminderInfo("值不符合要求，最小值为30");
                                return;
                            }
                        }
                    }
                    RequestClose();
                };

                if (InperGlobalClass.EventPanelProperties.HeightAuto)
                {
                    view.heightAuto.IsChecked = true;
                }
                else
                {
                    view.heightFixed.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        protected override void OnClose()
        {
            try
            {
                InperJsonHelper.SetEventPanelProperties(InperGlobalClass.EventPanelProperties);
                GC.Collect(0);
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
    }
}
