﻿#pragma checksum "..\..\..\..\Views\CameraSettingView.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "C4844604F97600026DB0A2D87EA671EEA6094480"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Expression.Media;
using HandyControl.Expression.Shapes;
using HandyControl.Interactivity;
using HandyControl.Media.Animation;
using HandyControl.Media.Effects;
using HandyControl.Properties.Langs;
using HandyControl.Themes;
using HandyControl.Tools;
using HandyControl.Tools.Converter;
using HandyControl.Tools.Extension;
using InperSight.Lib.Helper;
using InperSight.ViewModels;
using InperSight.Views;
using InperStudioControlLib.Control.Window;
using Stylet.Xaml;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace InperSight.Views {
    
    
    /// <summary>
    /// CameraSettingView
    /// </summary>
    public partial class CameraSettingView : InperStudioControlLib.Control.Window.InperDialogWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 13 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal InperSight.Views.CameraSettingView camera;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button _lock;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button unLock;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas drawAreaCanvas;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button home;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border bd1;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock t1;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button frame;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border bd2;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock t2;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tb3;
        
        #line default
        #line hidden
        
        
        #line 136 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox neuronName;
        
        #line default
        #line hidden
        
        
        #line 138 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox roiType;
        
        #line default
        #line hidden
        
        
        #line 165 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox fps;
        
        #line default
        #line hidden
        
        
        #line 177 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox gain;
        
        #line default
        #line hidden
        
        
        #line 195 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider focusPlane;
        
        #line default
        #line hidden
        
        
        #line 212 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider upperLevel;
        
        #line default
        #line hidden
        
        
        #line 230 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider lowerLevel;
        
        #line default
        #line hidden
        
        
        #line 251 "..\..\..\..\Views\CameraSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider excitLowerLevel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.10.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/InperSight;component/views/camerasettingview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\CameraSettingView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.10.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.camera = ((InperSight.Views.CameraSettingView)(target));
            return;
            case 2:
            this._lock = ((System.Windows.Controls.Button)(target));
            return;
            case 3:
            this.unLock = ((System.Windows.Controls.Button)(target));
            return;
            case 4:
            this.drawAreaCanvas = ((System.Windows.Controls.Canvas)(target));
            return;
            case 5:
            this.home = ((System.Windows.Controls.Button)(target));
            return;
            case 6:
            this.bd1 = ((System.Windows.Controls.Border)(target));
            return;
            case 7:
            this.t1 = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.frame = ((System.Windows.Controls.Button)(target));
            return;
            case 9:
            this.bd2 = ((System.Windows.Controls.Border)(target));
            return;
            case 10:
            this.t2 = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 11:
            this.tb3 = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 12:
            this.neuronName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 13:
            this.roiType = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 14:
            this.fps = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 15:
            this.gain = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 16:
            this.focusPlane = ((System.Windows.Controls.Slider)(target));
            return;
            case 17:
            this.upperLevel = ((System.Windows.Controls.Slider)(target));
            return;
            case 18:
            this.lowerLevel = ((System.Windows.Controls.Slider)(target));
            return;
            case 19:
            this.excitLowerLevel = ((System.Windows.Controls.Slider)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

