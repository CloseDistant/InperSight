using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InperStudio.ViewModels
{
    public class StartPageViewModel : Screen
    {
        public string Version { get; set; }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            Version = InperConfig.Instance.Version;
            _ = Task.Factory.StartNew(() =>
              {

              });
        }
        public void Close()
        {
            this.RequestClose();
        }
    }
}
