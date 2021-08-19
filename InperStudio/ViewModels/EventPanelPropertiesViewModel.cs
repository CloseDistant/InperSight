using InperStudio.Lib.Bean;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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
                    dataShowControlView.relativeBottom.Height = dataShowControlView.fixedBottom.Height = dataShowControlView.dataList.ActualHeight / dataShowControlView.dataList.Items.Count;
                };
                view.heightFixed.Checked += (s, e) =>
                {
                    dataShowControlView.relativeBottom.Height = dataShowControlView.fixedBottom.Height = InperGlobalClass.EventPanelProperties.HeightFixedValue;
                };
                view.fixedValue.TextChanged += (s, e) => dataShowControlView.relativeBottom.Height = dataShowControlView.fixedBottom.Height = double.Parse(view.fixedValue.Text);
                view.ConfirmClickEvent += (s, e) => RequestClose();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
                App.Log.Error(ex.ToString());
            }
        }
    }
}
