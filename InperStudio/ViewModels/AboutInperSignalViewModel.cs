using InperStudio.Lib.Bean;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InperStudio.ViewModels
{
    public class AboutInperSignalViewModel : Stylet.Screen
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

            if (InperGlobalClass.isNoNetwork)
            {
                view.ts.Text = "Unable to connect to the network";
            }
       
            if (!string.IsNullOrEmpty(InperGlobalClass.latestVersion))
            {
                if (InperConfig.Instance.Version.Equals(InperGlobalClass.latestVersion))
                {
                    view.ts.Text = "Latest Version";
                }
                else
                {
                    view.ts.Text = "The latest version is " + InperGlobalClass.latestVersion + ", Please restart the software to update";
                }
            }
        }
    }
}
