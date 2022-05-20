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
        public OnlineFilter _Lowpass { get; set; }
        public OnlineFilter _Hightpass { get; set; }
        public OnlineFilter _Bandpass { get; set; }
        public OnlineFilter _Bandstop { get; set; }
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
            _Bandpass = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
        }
        public void Bandstop(double sampleRate, double cutoffLowRate, double cutoffHeightRate)
        {
            _Bandstop = OnlineFilter.CreateBandstop(ImpulseResponse.Finite, sampleRate, cutoffLowRate, cutoffHeightRate);
        }
    }
}
