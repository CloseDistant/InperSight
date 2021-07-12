using InperStudio.Lib.Enum;
using InperStudio.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.ViewModels
{
    public class SignalPropertiesViewModel : Screen
    {
        #region filed
        private SignalPropertiesView view;
        private SignalPropertiesTypeEnum @enum;
        #endregion
        public SignalPropertiesViewModel(SignalPropertiesTypeEnum @enum)
        {
            this.@enum = @enum;
        }
        protected override void OnViewLoaded()
        {
            try
            {
                this.view = this.View as SignalPropertiesView;
                switch (@enum)
                {
                    case SignalPropertiesTypeEnum.Camera:
                        this.view.camera.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case SignalPropertiesTypeEnum.Analog:
                        this.view.analog.Visibility = System.Windows.Visibility.Visible;
                        this.view.Title = "Analog Signal Properties";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
    }
}
