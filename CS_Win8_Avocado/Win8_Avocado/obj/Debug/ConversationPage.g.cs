﻿

#pragma checksum "C:\Users\Reed\Documents\GitHub\Win8_Avocado\CS_Win8_Avocado\Win8_Avocado\ConversationPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5FAE24056381E32C79A138DF38ED9142"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Win8_Avocado
{
    partial class ConversationPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 78 "..\..\ConversationPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.SendButton_Click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 79 "..\..\ConversationPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.LogoutButton_Click;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


