using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
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
        private TimeSpanRange xRange = new TimeSpanRange(new TimeSpan(0), new TimeSpan(0, 0, 10));
        public TimeSpanRange XRange { get => xRange; set => SetAndNotify(ref xRange, value); }
        private XyDataSeries<TimeSpan, double> xyDataSeries = new XyDataSeries<TimeSpan, double>();
        public XyDataSeries<TimeSpan, double> XyDataSeries
        {
            get => xyDataSeries;
            set => SetAndNotify(ref xyDataSeries, value);
        }
        SweepSettingView view;
        Sweep sweep = new Sweep();
        private double oldDuration = -1;

        public SweepSettingViewModel()
        {
            //WaveForms.ToList().ForEach(w =>
            //{
            //    w.IsChecked = false;
            //});
        }
        bool isExist = false;
        public event EventHandler<double> SwepTimeChangeEvent;
        public SweepSettingViewModel(Sweep _sweep)
        {
            sweep = _sweep;
            isExist = true;
            oldDuration = sweep.Duration;
            //var select = sweep.WaveForm.Split(',').ToList();
            //WaveForms.ToList().ForEach(w =>
            //{
            //    w.IsChecked = select.Contains(w.Index.ToString()) ? true : false;
            //});
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
                        this.view.remainder.Text = "The value cannot be empty";
                        if (InperConfig.Instance.Language == "zh_cn")
                        {
                            this.view.remainder.Text = "值不能为空";
                        }
                        //view.remainder.Text = "The value cannot be empty";
                        view.remainder.Visibility = Visibility.Visible;
                        return;
                    }
                    if (StimulusBeans.Instance.Sweeps.Count > 0)
                    {
                        sweep.Index = StimulusBeans.Instance.Sweeps.OrderBy(x => x.Index).Last().Index + 1;
                    }
                    else
                    {
                        sweep.Index = 1;
                    }
                }
                if (!string.IsNullOrEmpty(sweep.WaveForm))
                {
                    StimulusBeans.Instance.Sweeps.Insert(sweep.Index - 1, sweep);
                }
                this.RequestClose();
                SwepTimeChangeEvent?.Invoke(this, double.Parse(string.IsNullOrEmpty(view.duration.Text) ? "0" : view.duration.Text));
            };
            view.selected.TextChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(view.selected.Text.Trim()))
                {
                    List<WaveForm> drawWaveForms = new List<WaveForm>();
                    double _duration = 0d;
                    foreach (var item in view.selected.Text.Split(','))
                    {
                        if (WaveForms.ToList().FirstOrDefault(x => x.Index == int.Parse(item)) is WaveForm waveForm)
                        {
                            drawWaveForms.Add(waveForm);
                            _duration += waveForm.Duration;
                        }
                    }
                    view.duration.Text = _duration.ToString();
                    if (double.TryParse(view.duration.Text, out double res))
                    {
                        if (res > 0)
                        {
                            XyDataSeries.Clear();
                            XyDataSeries = StimulusBeans.Instance.GetXyDataSeries(drawWaveForms, res);
                            view.scichart.ZoomExtents();
                        }
                    }
                }
                else
                {
                    view.duration.Text = String.Empty;
                    xyDataSeries.Clear();
                }
            };
            this.view.CancelClickEvent += View_CancelClickEvent;
        }

        private void View_CancelClickEvent(object obj)
        {
            if (isExist)
            {
                sweep.Duration = oldDuration;
            }
            this.RequestClose();
        }

        [Obsolete]
        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //var wf = (sender as CheckBox).DataContext as WaveForm;
                //wf.IsChecked = true;
                //string str = this.view.selected.Text + "," + wf.Index.ToString();
                //if (str.StartsWith(","))
                //{
                //    str = str.Substring(1);
                //}
                //sweep.WaveForm = str;
                //view.selected.Text = str;
                //if (string.IsNullOrEmpty(view.duration.Text))
                //{
                //    view.duration.Text = wf.Duration.ToString();
                //}
                //else
                //{
                //    view.duration.Text = (int.Parse(view.duration.Text) + wf.Duration).ToString();
                //}
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        [Obsolete]
        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                //var wf = (sender as CheckBox).DataContext as WaveForm;
                //wf.IsChecked = false;
                //var res = this.view.selected.Text.Split(',').ToList();
                //if (res.Contains(wf.Index.ToString()))
                //{
                //    res.Remove(wf.Index.ToString());
                //    view.selected.Text = string.Empty;
                //    string str = string.Empty;
                //    res.ForEach(x =>
                //    {
                //        str += x + ',';
                //    });
                //    str = res.Count == 0 ? "" : str.Substring(0, str.Length - 1);
                //    sweep.WaveForm = str;
                //    view.selected.Text = str;

                //    view.duration.Text = (int.Parse(view.duration.Text) - wf.Duration).ToString() == "0" ? "" : (int.Parse(view.duration.Text) - wf.Duration).ToString();
                //}
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void MarkerMove(string type)
        {
            try
            {
                if (type.Equals("rightMove"))
                {
                    var wf = this.view.sweepsList.SelectedItem as WaveForm;
                    if (wf != null)
                    {
                        string str = this.view.selected.Text + "," + wf.Index.ToString();
                        if (str.StartsWith(","))
                        {
                            str = str.Substring(1);
                        }
                        sweep.WaveForm = str;
                        view.selected.Text = str;
                        //if (string.IsNullOrEmpty(view.duration.Text))
                        //{
                        //    view.duration.Text = wf.Duration.ToString();
                        //}
                        //else
                        //{
                        //    view.duration.Text = (int.Parse(view.duration.Text) + wf.Duration).ToString();
                        //}
                    }
                }
                else
                {
                    var res = this.view.selected.Text.Split(',').ToList();
                    if (res.Count > 0)
                    {
                        res.RemoveAt(res.Count - 1);
                        string str = string.Empty;
                        res.ForEach(x =>
                        {
                            str += x + ',';
                        });
                        str = res.Count == 0 ? "" : str.Substring(0, str.Length - 1);
                        sweep.WaveForm = str;
                        view.selected.Text = str;
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                        List<WaveForm> wf = new List<WaveForm>();
                        var arr = view.selected.Text.Split(',');
                        if (arr.Length > 0)
                        {
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var item = WaveForms.ToList().FirstOrDefault(x => x.Index == int.Parse(arr[i]));
                                wf.Add(item);
                            }
                        }
                        sweep.Duration = res;
                        XyDataSeries.Clear();
                        XyDataSeries = StimulusBeans.Instance.GetXyDataSeries(wf, res);
                        view.scichart.ZoomExtents();
                        this.view.remainder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        this.view.remainder.Text = "The value input is not valid";
                        if (InperConfig.Instance.Language == "zh_cn")
                        {
                            this.view.remainder.Text = "输入的值无效";
                        }
                        this.view.remainder.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    this.view.remainder.Text = "The value input is not valid";
                    if (InperConfig.Instance.Language == "zh_cn")
                    {
                        this.view.remainder.Text = "输入的值无效";
                    }
                    this.view.remainder.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
    }
}
