using InperStudioControlLib.Lib.Config;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.ViewModels
{
    public class StartPageViewModel : Screen
    {
        public string Version { get; set; }

        protected override void OnViewLoaded()
        {
            Version = InperConfig.Instance.Version;
        }
    }
}
