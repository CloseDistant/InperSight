using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.ViewModels
{
    public class AboutInperSignalViewModel : Screen
    {
        private AboutInperSignalView view;
        public AboutInperSignalViewModel()
        {
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            view = this.View as AboutInperSignalView;
            view.releaseData.Text = InperConfig.Instance.ReleaseData;
            view.vn.Text = view.version.Text = InperConfig.Instance.Version;
            view.content.Text = InperConfig.Instance.VersionDesc;
            InperDeviceHelper.Instance.device.PhotometryInfo.LightConfigInfos.ForEach(x =>
            {
                view.light.Text += x.WaveLength + ",";
            });
            if (view.light.Text.Last() == ',')
            {
                view.light.Text = view.light.Text.Remove(view.light.Text.Length - 1, 1);
            }
            view.model.Text = InperDeviceHelper.Instance.device.PhotometryInfo.Model.ToString();
            view.sn.Text = InperDeviceHelper.Instance.device.PhotometryInfo.SN.ToString();
        }
    }
}
