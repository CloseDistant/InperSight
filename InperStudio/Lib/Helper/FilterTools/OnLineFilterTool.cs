using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Filtering;
using MathNet.Filtering.FIR;

namespace InperStudio.Lib.Helper.FilterTools
{
    public class OnLineFilterTool
    {
        private OnlineFilter _Lowpass { get; set; }
        private OnlineFilter _Hightpass { get; set; }
        private OnlineFilter _Bandpass0 { get; set; }
        private OnlineFilter _Bandpass1 { get; set; }
        private OnlineFilter _Bandpass2 { get; set; }
        private OnlineFilter _Bandpass3 { get; set; }
        private OnlineFilter _Bandstop0 { get; set; }
        private OnlineFilter _Bandstop1 { get; set; }
        private OnlineFilter _Bandstop2 { get; set; }
        private OnlineFilter _Bandstop3 { get; set; }
        //public static OnLineFilterTool Instance
        //{
        //    get
        //    {
        //        if (filterTool == null)
        //        {
        //            var temp = new OnLineFilterTool();
        //            Interlocked.CompareExchange<OnLineFilterTool>(ref filterTool, temp, null);
        //        }
        //        return filterTool;
        //    }
        //}
        public void Lowpass(double sampleRate, double cutoffRate)
        {
            _Lowpass = OnlineFilter.CreateLowpass(ImpulseResponse.Finite, sampleRate, cutoffRate);
        }
        public void Highpass(double sampleRate, double cutoffRate)
        {
            _Hightpass = OnlineFilter.CreateHighpass(ImpulseResponse.Finite, sampleRate, cutoffRate);
        }
        public void Bandpass(double sampleRate, double cutoffLowRate, double cutoffHeightRate)
        {
            _Bandpass0 = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
            _Bandpass1 = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
            _Bandpass2 = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
            _Bandpass3 = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
        }
        public void Bandstop(double sampleRate, double cutoffLowRate, double cutoffHeightRate)
        {
            _Bandstop0 = OnlineFilter.CreateBandstop(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
            _Bandstop1 = OnlineFilter.CreateBandstop(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
            _Bandstop2 = OnlineFilter.CreateBandstop(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
            _Bandstop3 = OnlineFilter.CreateBandstop(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
        }
        public double GetBandpassValue(double r, int group)
        {
            double val = r;
            switch (group)
            {
                case 0:
                    val = _Bandpass0.ProcessSample(r);
                    break;
                case 1:
                    val = _Bandpass1.ProcessSample(r);
                    break;
                case 2:
                    val = _Bandpass2.ProcessSample(r);
                    break;
                case 3:
                    val = _Bandpass3.ProcessSample(r);
                    break;
            }
            return val;
        }
        public double GetBandstopValue(double r, int group)
        {
            double val = r;
            switch (group)
            {
                case 0:
                    val = _Bandstop0.ProcessSample(r);
                    break;
                case 1:
                    val = _Bandstop1.ProcessSample(r);
                    break;
                case 2:
                    val = _Bandstop2.ProcessSample(r);
                    break;
                case 3:
                    val = _Bandstop3.ProcessSample(r);
                    break;
            }
            return val;
        }
    }
}
