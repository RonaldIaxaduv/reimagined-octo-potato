﻿#pragma checksum "..\..\ClientMenu.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "8C65BF3A88E885F026AC6D75649A6862F21F8589"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ProgrammierprojektWPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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


namespace ProgrammierprojektWPF {
    
    
    /// <summary>
    /// ClientMenu
    /// </summary>
    public partial class ClientMenu : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 79 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lbChatMessages;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lbUsers;
        
        #line default
        #line hidden
        
        
        #line 89 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdWhisper;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdGlobalMessage;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdLogout;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbMessage;
        
        #line default
        #line hidden
        
        
        #line 95 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tblBuffer;
        
        #line default
        #line hidden
        
        
        #line 96 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbBuffer;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\ClientMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tblVersion;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ProgrammierprojektWPF;component/clientmenu.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ClientMenu.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 8 "..\..\ClientMenu.xaml"
            ((ProgrammierprojektWPF.ClientMenu)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.lbChatMessages = ((System.Windows.Controls.ListBox)(target));
            return;
            case 3:
            this.lbUsers = ((System.Windows.Controls.ListBox)(target));
            return;
            case 4:
            this.cmdWhisper = ((System.Windows.Controls.Button)(target));
            
            #line 89 "..\..\ClientMenu.xaml"
            this.cmdWhisper.Click += new System.Windows.RoutedEventHandler(this.cmdWhisper_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.cmdGlobalMessage = ((System.Windows.Controls.Button)(target));
            
            #line 90 "..\..\ClientMenu.xaml"
            this.cmdGlobalMessage.Click += new System.Windows.RoutedEventHandler(this.cmdGlobalMessage_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.cmdLogout = ((System.Windows.Controls.Button)(target));
            
            #line 91 "..\..\ClientMenu.xaml"
            this.cmdLogout.Click += new System.Windows.RoutedEventHandler(this.cmdLogout_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.tbMessage = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.tblBuffer = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.tbBuffer = ((System.Windows.Controls.TextBox)(target));
            
            #line 96 "..\..\ClientMenu.xaml"
            this.tbBuffer.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.tbBuffer_TextChanged);
            
            #line default
            #line hidden
            return;
            case 10:
            this.tblVersion = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

