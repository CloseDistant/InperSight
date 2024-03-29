﻿#pragma checksum "..\..\..\..\Views\VideoSettingView.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "527EEA9593819CC8FFBA1D6A890C610A08149E96"
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
using InperSight.Lib.Bean;
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
    /// VideoSettingView
    /// </summary>
    public partial class VideoSettingView : InperStudioControlLib.Control.Window.InperDialogWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 18 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid video;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid Marker;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CameraCombox;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox format;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox framrate;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal HandyControl.Controls.TextBox CameraName;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button rightMove;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button leftMove;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\..\Views\VideoSettingView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox cameraActiveChannel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/InperSight;component/views/videosettingview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\VideoSettingView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.video = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.Marker = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.CameraCombox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.format = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.framrate = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 6:
            this.CameraName = ((HandyControl.Controls.TextBox)(target));
            return;
            case 7:
            this.img = ((System.Windows.Controls.Image)(target));
            return;
            case 8:
            this.rightMove = ((System.Windows.Controls.Button)(target));
            return;
            case 9:
            this.leftMove = ((System.Windows.Controls.Button)(target));
            return;
            case 10:
            this.cameraActiveChannel = ((System.Windows.Controls.ListBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

