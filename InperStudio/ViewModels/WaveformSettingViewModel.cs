using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
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
        private int[] freqs = new int[] { };
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
            var gys = GetGYS((int)InperGlobalClass.CameraSignalSettings.Sampling).OrderBy(x => x).ToList();
            Freqs = new int[gys.Count];
            for (int i = 0; i < gys.Count; i++)
            {
                Freqs[i] = gys[i];
            }

            if (isExist)
            {
                view.pluse.Text = waveForm.Pulse.ToString();
                int index = Freqs.ToList().IndexOf(waveForm.Frequence) < 0 ? 0 : Freqs.ToList().IndexOf(waveForm.Frequence);
                view.freq.SelectedValue = Freqs[index];
                view.duration.Text = waveForm.Duration.ToString();
            }
            else
            {
                view.freq.SelectedValue = Freqs[0];
            }

            this.view.ConfirmClickEvent += (s, e) =>
            {
                waveForm.Frequence = int.Parse(view.freq.SelectedValue.ToString());
                if (isExist)
                {
                    StimulusBeans.Instance.WaveForms.Remove(StimulusBeans.Instance.WaveForms.FirstOrDefault(x => x.Index == waveForm.Index));
                }
                else
                {
                    if (waveForm.Pulse < 0 || waveForm.Duration <= 0 || waveForm.Frequence <= 0)
                    {
                        this.view.remainder.Text = "The value cannot be empty";
                        if (InperConfig.Instance.Language == "zh_cn")
                        {
                            this.view.remainder.Text = "值不能为空";
                        }
                        this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                        return;
                    }
                    if (StimulusBeans.Instance.WaveForms.Count > 0)
                    {
                        waveForm.Index = StimulusBeans.Instance.WaveForms.OrderBy(x => x.Index).Last().Index + 1;
                    }
                    else
                    {
                        waveForm.Index = 1;
                    }
                }
                StimulusBeans.Instance.WaveForms.Insert(waveForm.Index - 1, waveForm);
                this.RequestClose();
            };
        }
        private List<int> GetGYS(int num)
        {
            if (num == 1)
            {
                return new List<int>() { 1 };
            }
            List<int> list = new List<int>() { 1, num };
            int temp = 2;
            while (temp < num)
            {
                if (num % temp == 0)
                {
                    list.Add(temp);
                }
                temp++;
            }
            return list;
        }
        public void Pulse_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var tb = sender as TextBox;
                if (!string.IsNullOrEmpty(tb.Text))
                {
                    if (double.TryParse(tb.Text, out double res))
                    {
                        double maxPulse = (1000 / InperGlobalClass.CameraSignalSettings.Sampling - InperGlobalClass.CameraSignalSettings.Exposure * InperGlobalClass.CameraSignalSettings.LightMode.Count) / InperGlobalClass.CameraSignalSettings.LightMode.Count;
                        if (res < maxPulse)
                        {
                            waveForm.Pulse = res;
                            this.view.remainder.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            this.view.remainder.Text = "The Value must be < " + maxPulse.ToString("0.00");
                            if (InperConfig.Instance.Language == "zh_cn")
                            {
                                this.view.remainder.Text = "最大值为："+ maxPulse.ToString("0.00");
                            }
                            this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                            return;
                        }
                        if (tb.Text.Contains('.'))
                        {
                            if (tb.Text.Split('.').Last().Length > 2)
                            {
                                tb.Text = tb.Text.Substring(0, tb.Text.Split('.')[0].Length + 3);
                                tb.SelectionStart = tb.Text.Length;
                            }
                        }
                    }
                    else
                    {
                        this.view.remainder.Text = "The value input is not valid";
                        if (InperConfig.Instance.Language == "zh_cn")
                        {
                            this.view.remainder.Text = "输入的值无效";
                        }
                        this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                            this.view.remainder.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            this.view.remainder.Text = "The Value must be > 0";
                            if (InperConfig.Instance.Language == "zh_cn")
                            {
                                this.view.remainder.Text = "值必须大于0";
                            }
                            this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                            return;
                        }
                    }
                    else
                    {
                        this.view.remainder.Text = "The value input is not valid";
                        if (InperConfig.Instance.Language == "zh_cn")
                        {
                            this.view.remainder.Text = "输入的值无效";
                        }
                        this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
    }
}
