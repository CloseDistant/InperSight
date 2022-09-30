using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InperStudio.ViewModels
{
    public class SweepSettingViewModel : Screen
    {
        private ObservableCollection<WaveForm> waveForms = StimulusBeans.Instance.WaveForms;
        public ObservableCollection<WaveForm> WaveForms
        {
            get => waveForms;
            set => SetAndNotify(ref waveForms, value);
        }
        SweepSettingView view;
        Sweep sweep = new Sweep();

        public SweepSettingViewModel()
        {
        }
        bool isExist = false;
        public SweepSettingViewModel(Sweep _sweep)
        {
            sweep = _sweep;
            isExist = true;
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            view = this.View as SweepSettingView;
            if (isExist)
            {
                view.selected.Text = sweep.WaveForm;
                view.duration.Text = sweep.Duration.ToString();
            }
            this.view.ConfirmClickEvent += (s, e) =>
            {
                if (isExist)
                {
                    StimulusBeans.Instance.Sweeps.Remove(StimulusBeans.Instance.Sweeps.FirstOrDefault(x => x.Index == sweep.Index));
                }
                else
                {
                    if (sweep.Duration <= 0 || string.IsNullOrEmpty(sweep.WaveForm))
                    {
                        this.view.remainder.Text = "值不能为空";
                        this.view.remainder.Visibility = System.Windows.Visibility.Visible;
                        return;
                    }
                    sweep.Index = StimulusBeans.Instance.Sweeps.Count + 1;
                }
                StimulusBeans.Instance.Sweeps.Add(sweep);
                this.RequestClose();
            };
        }
        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var wf = (sender as CheckBox).DataContext as WaveForm;
                this.view.selected.Text += ',' + wf.Index.ToString();
                if (view.selected.Text.StartsWith(","))
                {
                    view.selected.Text = view.selected.Text.Substring(1);
                }
                sweep.WaveForm = view.selected.Text;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }

        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var wf = (sender as CheckBox).DataContext as WaveForm;

                var res = this.view.selected.Text.Split(',').ToList();
                if (res.Contains(wf.Index.ToString()))
                {
                    res.Remove(wf.Index.ToString());
                    view.selected.Text = string.Empty;
                    res.ForEach(x =>
                    {
                        view.selected.Text += x + ',';
                    });
                    view.selected.Text = res.Count == 0 ? "" : view.selected.Text.Substring(0, view.selected.Text.Length - 1);
                    sweep.WaveForm = view.selected.Text;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Duration_Textchanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var tb = (sender as TextBox).Text;
                if (int.TryParse(tb, out int res))
                {
                    if (res > 0)
                    {
                        sweep.Duration = res;
                        this.view.remainder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        this.view.remainder.Text = "无效的值";
                        this.view.remainder.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    this.view.remainder.Text = "无效的值";
                    this.view.remainder.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
    }
}
