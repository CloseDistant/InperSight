using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InperStudio.ViewModels
{
    public class WaveformSettingViewModel : Screen
    {
        private int[] freqs = new int[3];
        public int[] Freqs
        {
            get => freqs;
            set => SetAndNotify(ref freqs, value);
        }
        WaveformSettingView view;
        WaveForm waveForm = new WaveForm();

        public WaveformSettingViewModel()
        {
        }
        bool isExist = false;
        public WaveformSettingViewModel(WaveForm _waveForm)
        {
            waveForm = _waveForm;
            isExist = true;
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            view = this.View as WaveformSettingView;

            if (isExist)
            {
                view.pluse.Text = waveForm.Pulse.ToString();
                view.freq.SelectedValue = waveForm.Frequence;
                view.duration.Text = waveForm.Duration.ToString();
            }

            this.view.ConfirmClickEvent += (s, e) =>
            {
                if (isExist)
                {
                    StimulusBeans.Instance.WaveForms.Remove(StimulusBeans.Instance.WaveForms.FirstOrDefault(x => x.Index == waveForm.Index));
                }
                else
                {
                    if (waveForm.Pulse <= 0 || waveForm.Duration <= 0 || waveForm.Frequence <= 0)
                    {
                        this.view.remainder.Text = "值不能为空";
                        this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                        return;
                    }
                    waveForm.Index = StimulusBeans.Instance.WaveForms.Count + 1;
                }
                StimulusBeans.Instance.WaveForms.Add(waveForm);
                this.RequestClose();
            };
            Freqs[0] = (int)InperGlobalClass.CameraSignalSettings.Sampling;
            Freqs[1] = (int)InperGlobalClass.CameraSignalSettings.Sampling / 2;
            Freqs[2] = (int)InperGlobalClass.CameraSignalSettings.Sampling / 4;
            view.freq.SelectedValue = freqs[0];
        }
        public void Pulse_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var tb = sender as TextBox;
                if (!string.IsNullOrEmpty(tb.Text))
                {
                    if (int.TryParse(tb.Text, out int res))
                    {
                        if (res < InperGlobalClass.CameraSignalSettings.Exposure)
                        {
                            waveForm.Pulse = res;
                            this.view.remainder.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            this.view.remainder.Text = "The Value must be < " + InperGlobalClass.CameraSignalSettings.Exposure;
                            this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                            return;
                        }
                    }
                    else
                    {
                        this.view.remainder.Text = "无效的值";
                        this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Duration_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var tb = sender as TextBox;
                if (!string.IsNullOrEmpty(tb.Text))
                {
                    if (int.TryParse(tb.Text, out int res))
                    {
                        if (res > 0)
                        {
                            waveForm.Duration = res;
                        }
                        else
                        {
                            this.view.remainder.Text = "The Value must be > 0";
                            this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                            return;
                        }
                    }
                    else
                    {
                        this.view.remainder.Text = "无效的值";
                        this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Freq_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = int.Parse((sender as ComboBox).SelectedValue.ToString());
                waveForm.Frequence = item;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
    }
}
