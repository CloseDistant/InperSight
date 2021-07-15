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
                this.view = this.View as EventPanelPropertiesView;

                this.view.heightAuto.Checked += (s, e) =>
                {
                    this.dataShowControlView.relativeBottom.Height = this.dataShowControlView.fixedBottom.Height = this.dataShowControlView.dataList.ActualHeight / this.dataShowControlView.dataList.Items.Count;
                };
                this.view.heightFixed.Checked += (s, e) =>
                {
                    this.dataShowControlView.relativeBottom.Height = this.dataShowControlView.fixedBottom.Height = InperGlobalClass.EventPanelProperties.HeightFixedValue;
                };
                this.view.fixedValue.TextChanged += (s, e) => this.dataShowControlView.relativeBottom.Height = this.dataShowControlView.fixedBottom.Height = double.Parse(this.view.fixedValue.Text);
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
