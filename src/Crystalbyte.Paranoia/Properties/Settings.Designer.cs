﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Crystalbyte.Paranoia.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AcceptUntrustedCertificates {
            get {
                return ((bool)(this["AcceptUntrustedCertificates"]));
            }
            set {
                this["AcceptUntrustedCertificates"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Re:")]
        public string PrefixForAnswering {
            get {
                return ((string)(this["PrefixForAnswering"]));
            }
            set {
                this["PrefixForAnswering"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Fwd:")]
        public string PrefixForForwarding {
            get {
                return ((string)(this["PrefixForForwarding"]));
            }
            set {
                this["PrefixForForwarding"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsFirstStart {
            get {
                return ((bool)(this["IsFirstStart"]));
            }
            set {
                this["IsFirstStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Light")]
        public string Theme {
            get {
                return ((string)(this["Theme"]));
            }
            set {
                this["Theme"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Steelblue")]
        public string Accent {
            get {
                return ((string)(this["Accent"]));
            }
            set {
                this["Accent"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Trebuchet MS")]
        public string DefaultWebFont {
            get {
                return ((string)(this["DefaultWebFont"]));
            }
            set {
                this["DefaultWebFont"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("16")]
        public int DefaultWebFontSize {
            get {
                return ((int)(this["DefaultWebFontSize"]));
            }
            set {
                this["DefaultWebFontSize"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>gmail.com</string>\r\n  <string>googlemail.com</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection GmailDomains {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["GmailDomains"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>#FF0000</string>
  <string>#00FFFF</string>
  <string>#0000FF</string>
  <string>#0000A0</string>
  <string>#ADD8E6</string>
  <string>#800080</string>
  <string>#FFFF00</string>
  <string>#00FF00</string>
  <string>#FF00FF</string>
  <string>#C0C0C0</string>
  <string>#808080</string>
  <string>#000000</string>
  <string>#FFA500</string>
  <string>#A52A2A</string>
  <string>#800000</string>
  <string>#008000</string>
  <string>#808000</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection TextFontColors {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["TextFontColors"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>#FF0000</string>
  <string>#00FFFF</string>
  <string>#0000FF</string>
  <string>#0000A0</string>
  <string>#ADD8E6</string>
  <string>#800080</string>
  <string>#FFFF00</string>
  <string>#00FF00</string>
  <string>#FF00FF</string>
  <string>#C0C0C0</string>
  <string>#808080</string>
  <string>#FFFFFF</string>
  <string>#FFA500</string>
  <string>#A52A2A</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection BackgroundFontColors {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["BackgroundFontColors"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>Arial</string>
  <string>Georgia</string>
  <string>Times New Roman</string>
  <string>Trebuchet MS</string>
  <string>Verdana</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection WebFonts {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["WebFonts"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>DENIC eG|whois.denic.de|-T dn {0}|de</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection WhoisRegistrars {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["WhoisRegistrars"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20480")]
        public int StorageQuota {
            get {
                return ((int)(this["StorageQuota"]));
            }
            set {
                this["StorageQuota"] = value;
            }
        }
    }
}
